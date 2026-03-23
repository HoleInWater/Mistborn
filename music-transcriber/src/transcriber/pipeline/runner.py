"""
pipeline/runner.py

Orchestrates the full transcription pipeline:
    audio → stems → MIDI → quantized MIDI → sheet music PDF

Each stage is loosely coupled: reads from disk, writes to disk.
Failures are captured per-stage; partial results are always returned.
"""

from __future__ import annotations

import logging
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

logger = logging.getLogger(__name__)


# ─────────────────────────────────────────────
# Result contract
# ─────────────────────────────────────────────

@dataclass
class PipelineResult:
    # Original input
    input_audio: Path

    # Stage outputs (empty dicts / None until stage completes)
    stems:          Dict[str, Path] = field(default_factory=dict)
    midi_files:     Dict[str, Path] = field(default_factory=dict)
    quantized_midi: Dict[str, Path] = field(default_factory=dict)
    score_pdf:      Optional[Path]  = None
    score_musicxml: Optional[Path]  = None

    # Metadata
    bpm:         Optional[float]     = None
    config_used: Dict[str, Any]      = field(default_factory=dict)

    # Health
    status: str                  = "success"   # success | partial | failed
    errors: Dict[str, str]       = field(default_factory=dict)

    # ── helpers ──────────────────────────────

    def succeeded(self) -> bool:
        return self.status == "success"

    def failed_stages(self) -> List[str]:
        return list(self.errors.keys())

    def summary(self) -> str:
        lines = [
            f"Status  : {self.status}",
            f"Input   : {self.input_audio}",
            f"Stems   : {list(self.stems.keys()) or '—'}",
            f"MIDI    : {list(self.midi_files.keys()) or '—'}",
            f"BPM     : {self.bpm or 'unknown'}",
            f"PDF     : {self.score_pdf or '—'}",
            f"MusicXML: {self.score_musicxml or '—'}",
        ]
        if self.errors:
            lines.append("Errors  :")
            for stage, msg in self.errors.items():
                lines.append(f"  [{stage}] {msg}")
        return "\n".join(lines)


# ─────────────────────────────────────────────
# Orchestrator
# ─────────────────────────────────────────────

def run_pipeline(
    audio_path: Path,
    config: Dict[str, Any],
    *,
    output_dir: Optional[Path] = None,
    instruments: Optional[List[str]] = None,
) -> PipelineResult:
    """
    Run the full transcription pipeline.

    Args:
        audio_path:   Path to the source .mp3 / .wav file.
        config:       Parsed configuration dictionary (see config/default.yaml).
        output_dir:   Root output directory. Defaults to config["output"]["base_dir"].
        instruments:  Stems to process. None → use config["separation"]["stems"].

    Returns:
        PipelineResult — always returned, even on partial failure.
    """
    # Late imports keep startup fast and allow mocking in tests
    from transcriber.separation.demucs import separate_audio
    from transcriber.transcription.basic_pitch import transcribe_stem
    from transcriber.quantization.quantizer import quantize_midi
    from transcriber.rendering.sheet_music import render_score

    audio_path = Path(audio_path)
    base_dir   = Path(output_dir or config.get("output", {}).get("base_dir", "data"))

    stems_dir  = base_dir / "stems"
    midi_dir   = base_dir / "midi"
    quant_dir  = base_dir / "midi_quantized"
    score_dir  = base_dir / "scores"

    result = PipelineResult(input_audio=audio_path, config_used=config)

    # ── Stage 1: Source Separation ────────────────────────────────────────
    logger.info("[1/4] Separating audio → %s", stems_dir)
    try:
        result.stems = separate_audio(
            audio_path,
            config,
            output_dir=stems_dir,
            stems=instruments or config.get("separation", {}).get("stems"),
        )
        logger.info("      stems: %s", list(result.stems.keys()))
    except Exception as exc:
        logger.error("Separation failed: %s", exc)
        result.errors["separation"] = str(exc)
        result.status = "failed"
        return result

    # ── Stage 2: Transcription ────────────────────────────────────────────
    logger.info("[2/4] Transcribing stems → %s", midi_dir)
    for name, stem_path in result.stems.items():
        try:
            midi_path = transcribe_stem(stem_path, config, output_dir=midi_dir)
            result.midi_files[name] = midi_path
            logger.info("      %s → %s", name, midi_path.name)
        except Exception as exc:
            logger.warning("Transcription failed for %s: %s", name, exc)
            result.errors[f"transcription.{name}"] = str(exc)

    if not result.midi_files:
        result.status = "failed"
        result.errors["transcription"] = "All stems failed transcription."
        return result

    # ── Stage 3: Quantization ─────────────────────────────────────────────
    logger.info("[3/4] Quantizing MIDI → %s", quant_dir)
    try:
        result.quantized_midi, result.bpm = quantize_midi(
            result.midi_files, config, output_dir=quant_dir
        )
        logger.info("      detected BPM: %.1f", result.bpm or 0)
    except Exception as exc:
        logger.error("Quantization failed: %s", exc)
        result.errors["quantization"] = str(exc)
        result.status = "partial"
        return result

    # ── Stage 4: Rendering ────────────────────────────────────────────────
    logger.info("[4/4] Rendering score → %s", score_dir)
    try:
        result.score_pdf, result.score_musicxml = render_score(
            result.quantized_midi, config, output_dir=score_dir
        )
        logger.info("      PDF      : %s", result.score_pdf)
        logger.info("      MusicXML : %s", result.score_musicxml)
    except Exception as exc:
        logger.error("Rendering failed: %s", exc)
        result.errors["rendering"] = str(exc)
        result.status = "partial"
        return result

    # ── Final status ──────────────────────────────────────────────────────
    if result.errors:
        result.status = "partial"

    return result
