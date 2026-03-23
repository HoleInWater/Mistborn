"""
transcription/basic_pitch.py

Transcribes a single audio stem → MIDI using Spotify's Basic Pitch.

Basic Pitch handles internal audio loading and resampling, so we delegate
that entirely to the library rather than re-implementing it here.

Public API:
    transcribe_stem(stem_path, config, *, output_dir) -> Path
"""

from __future__ import annotations

import logging
from pathlib import Path
from typing import Any, Dict

from basic_pitch.inference import predict

logger = logging.getLogger(__name__)


def transcribe_stem(
    stem_path: Path,
    config: Dict[str, Any],
    *,
    output_dir: Path,
) -> Path:
    """
    Transcribe *stem_path* audio to a MIDI file.

    Args:
        stem_path:  Path to a single stem .wav file.
        config:     Full config dict; reads config["transcription"].
        output_dir: Directory where .mid file will be written.

    Returns:
        Path to the generated .mid file.

    Raises:
        FileNotFoundError: if *stem_path* does not exist.
        RuntimeError:      if Basic Pitch prediction fails.
    """
    stem_path = Path(stem_path)
    if not stem_path.exists():
        raise FileNotFoundError(f"Stem file not found: {stem_path}")

    output_dir.mkdir(parents=True, exist_ok=True)

    t_cfg = config.get("transcription", {})

    onset_threshold   = float(t_cfg.get("onset_threshold",   0.5))
    frame_threshold   = float(t_cfg.get("frame_threshold",   0.3))
    minimum_note_length = float(t_cfg.get("minimum_note_length", 0.1))

    logger.debug(
        "Transcribing '%s'  onset=%.2f  frame=%.2f  min_note=%.2fs",
        stem_path.name, onset_threshold, frame_threshold, minimum_note_length,
    )

    # Basic Pitch returns (model_output_dict, midi_data, note_events)
    # midi_data is a pretty_midi.PrettyMIDI object
    _, midi_data, _ = predict(
        audio_path=str(stem_path),
        onset_threshold=onset_threshold,
        frame_threshold=frame_threshold,
        minimum_note_length=minimum_note_length,
    )

    midi_path = output_dir / f"{stem_path.stem}.mid"
    midi_data.write(str(midi_path))

    logger.debug("MIDI written → %s", midi_path)
    return midi_path
