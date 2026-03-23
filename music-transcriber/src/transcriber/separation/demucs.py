"""
separation/demucs.py

Source separation using Meta's Demucs v4 (htdemucs / htdemucs_6s / mdx_extra).

Responsibilities:
- Load model (from ~/.cache/music-transcriber/ via demucs internals)
- Resample input to model's native sample rate
- Run separation in chunks (split=True) for memory safety
- Normalise outputs into a flat output_dir structure
- Return only requested stems

Public API:
    separate_audio(audio_path, config, *, output_dir, stems) -> Dict[str, Path]
"""

from __future__ import annotations

import logging
from pathlib import Path
from typing import Any, Dict, List, Optional

import torch
import torchaudio
from demucs.apply import apply_model
from demucs.pretrained import get_model

logger = logging.getLogger(__name__)


def separate_audio(
    audio_path: Path,
    config: Dict[str, Any],
    *,
    output_dir: Path,
    stems: Optional[List[str]] = None,
) -> Dict[str, Path]:
    """
    Run Demucs source separation on *audio_path*.

    Args:
        audio_path: Path to source audio (.mp3 / .wav / .flac …).
        config:     Full config dict; reads config["separation"].
        output_dir: Directory where stem .wav files will be written.
        stems:      Whitelist of stem names to keep (None = keep all).

    Returns:
        Dict mapping stem name → Path  e.g. {"vocals": Path("…/song_vocals.wav")}

    Raises:
        FileNotFoundError: if *audio_path* does not exist.
        RuntimeError:      if model loading or separation fails.
    """
    audio_path = Path(audio_path)
    if not audio_path.exists():
        raise FileNotFoundError(f"Audio file not found: {audio_path}")

    output_dir.mkdir(parents=True, exist_ok=True)

    sep_cfg    = config.get("separation", {})
    model_name = sep_cfg.get("model", "htdemucs")
    device_str = sep_cfg.get("device", "cpu")
    device     = torch.device(device_str)

    # ── Load model ────────────────────────────────────────────────────────
    logger.debug("Loading Demucs model '%s' on %s", model_name, device)
    model = get_model(model_name)
    model.to(device)
    model.eval()

    # ── Load audio ────────────────────────────────────────────────────────
    logger.debug("Loading audio: %s", audio_path)
    waveform, sr = torchaudio.load(str(audio_path))

    # Demucs requires stereo (2 channels)
    if waveform.shape[0] == 1:
        waveform = waveform.repeat(2, 1)
    elif waveform.shape[0] > 2:
        waveform = waveform[:2]          # take first two channels

    # Resample to model's native rate if needed
    target_sr: int = model.samplerate   # type: ignore[attr-defined]
    if sr != target_sr:
        logger.debug("Resampling %d Hz → %d Hz", sr, target_sr)
        waveform = torchaudio.functional.resample(waveform, sr, target_sr)
        sr = target_sr

    waveform = waveform.to(device)

    # ── Run separation ────────────────────────────────────────────────────
    logger.debug("Running apply_model (split=True) …")
    with torch.no_grad():
        # shape after [0]: (num_sources, channels, samples)
        sources = apply_model(
            model,
            waveform[None],    # add batch dim
            device=device,
            split=True,        # chunked processing — prevents OOM on long tracks
            progress=False,
        )[0]

    source_names: List[str] = model.sources  # type: ignore[attr-defined]

    # ── Normalise + save outputs ──────────────────────────────────────────
    stem_paths: Dict[str, Path] = {}
    for i, name in enumerate(source_names):
        if stems is not None and name not in stems:
            logger.debug("Skipping stem '%s' (not in requested list)", name)
            continue

        source_audio = sources[i].cpu()
        out_path     = output_dir / f"{audio_path.stem}_{name}.wav"

        torchaudio.save(str(out_path), source_audio, sr)
        stem_paths[name] = out_path
        logger.debug("Saved stem '%s' → %s", name, out_path)

    return stem_paths
