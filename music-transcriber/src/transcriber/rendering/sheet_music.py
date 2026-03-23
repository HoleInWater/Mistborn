"""
rendering/sheet_music.py

Converts quantized MIDI files into sheet music:
    MIDI → music21 Score → MusicXML (source of truth) → PDF (via MuseScore CLI)

MusicXML is always generated. PDF generation gracefully degrades to None
if MuseScore is not installed, so the pipeline never hard-fails here.

Public API:
    render_score(quantized_midi, config, *, output_dir) -> Tuple[Optional[Path], Optional[Path]]
"""

from __future__ import annotations

import logging
import subprocess
from pathlib import Path
from typing import Any, Dict, Optional, Tuple

from music21 import converter, metadata, stream, tempo as m21_tempo

logger = logging.getLogger(__name__)

# MuseScore CLI flags that produce clean output
_MUSESCORE_FLAGS = ["-o"]


def _label_to_part_name(stem_name: str) -> str:
    """Map a Demucs stem name to a human-readable part label."""
    mapping = {
        "vocals": "Vocals",
        "bass":   "Bass",
        "drums":  "Drums",
        "other":  "Other",
        "guitar": "Guitar",
        "piano":  "Piano",
    }
    return mapping.get(stem_name.lower(), stem_name.title())


def render_score(
    quantized_midi: Dict[str, Path],
    config: Dict[str, Any],
    *,
    output_dir: Path,
) -> Tuple[Optional[Path], Optional[Path]]:
    """
    Combine quantized MIDI stems into a single notated score.

    Args:
        quantized_midi: Mapping of stem name → quantized .mid path.
        config:         Full config dict; reads config["rendering"].
        output_dir:     Directory for score.musicxml and score.pdf.

    Returns:
        (pdf_path, musicxml_path)
        pdf_path is None if MuseScore is unavailable or fails.
        musicxml_path is None only on catastrophic failure.
    """
    if not quantized_midi:
        raise ValueError("render_score received an empty midi dict — nothing to render.")

    output_dir.mkdir(parents=True, exist_ok=True)

    r_cfg          = config.get("rendering", {})
    musescore_path = r_cfg.get("musescore_path", "musescore")
    title          = r_cfg.get("title", "Transcription")
    composer       = r_cfg.get("composer", "")
    bpm            = config.get("_detected_bpm")  # optionally injected by runner

    # ── Build a combined music21 Score ────────────────────────────────────
    score = stream.Score()

    # Metadata
    md = metadata.Metadata()
    md.title    = title
    md.composer = composer
    score.insert(0, md)

    # Insert tempo marking if BPM is known
    if bpm:
        mm = m21_tempo.MetronomeMark(number=bpm)
        score.insert(0, mm)

    for stem_name, midi_path in quantized_midi.items():
        logger.debug("Parsing MIDI part '%s': %s", stem_name, midi_path)
        try:
            part = converter.parse(str(midi_path))
        except Exception as exc:
            logger.warning("Skipping part '%s' — could not parse: %s", stem_name, exc)
            continue

        # Label the part
        part_name = _label_to_part_name(stem_name)
        for p in part.parts:
            p.partName = part_name

        score.append(part)

    if not score.parts:
        raise RuntimeError("No parts could be parsed — score is empty.")

    # ── Export MusicXML (always) ──────────────────────────────────────────
    musicxml_path: Optional[Path] = output_dir / "score.musicxml"
    try:
        score.write("musicxml", fp=str(musicxml_path))
        logger.debug("MusicXML written → %s", musicxml_path)
    except Exception as exc:
        logger.error("MusicXML export failed: %s", exc)
        musicxml_path = None

    # ── Export PDF via MuseScore CLI (graceful degradation) ───────────────
    pdf_path: Optional[Path] = None

    if musicxml_path and musicxml_path.exists():
        pdf_out = output_dir / "score.pdf"
        cmd = [musescore_path, str(musicxml_path), "-o", str(pdf_out)]
        logger.debug("Running MuseScore: %s", " ".join(cmd))
        try:
            subprocess.run(
                cmd,
                check=True,
                capture_output=True,
                timeout=120,   # large scores can take a while
            )
            pdf_path = pdf_out
            logger.debug("PDF written → %s", pdf_path)
        except FileNotFoundError:
            logger.warning(
                "MuseScore not found at '%s'. "
                "Install MuseScore and set config['rendering']['musescore_path']. "
                "MusicXML is still available at: %s",
                musescore_path,
                musicxml_path,
            )
        except subprocess.CalledProcessError as exc:
            logger.error(
                "MuseScore exited with code %d:\n%s",
                exc.returncode,
                exc.stderr.decode(errors="replace"),
            )
        except subprocess.TimeoutExpired:
            logger.error("MuseScore timed out after 120 s.")

    return pdf_path, musicxml_path
