"""
quantization/quantizer.py

Converts raw MIDI timing (floating-point seconds) into clean rhythmic notation.

Pipeline:
    1. Detect or accept BPM
    2. For each MIDI file: snap note start/end times to rhythmic grid
    3. Cleanup: remove notes below minimum duration
    4. Rebuild PrettyMIDI and write to disk

Public API:
    quantize_midi(midi_files, config, *, output_dir) -> Tuple[Dict[str, Path], Optional[float]]
"""

from __future__ import annotations

import logging
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

import pretty_midi

logger = logging.getLogger(__name__)


# ─────────────────────────────────────────────
# Core algorithms
# ─────────────────────────────────────────────

def _estimate_bpm_from_midi(midi: pretty_midi.PrettyMIDI) -> Optional[float]:
    """
    Attempt to extract tempo from MIDI tempo change events.
    Returns None if no tempo events exist.
    """
    tempos = midi.get_tempo_change_times()
    # get_tempo_change_times() returns (tempo_change_times, tempos)
    _, tempo_values = midi.get_tempo_change_times(), midi.estimate_tempo()
    if tempo_values:
        return float(tempo_values)
    return None


def _estimate_bpm_from_audio(audio_path: Path) -> float:
    """
    Fallback: use librosa beat tracking on the original audio stem.
    More expensive but more reliable than raw MIDI timing.
    """
    import librosa  # lazy — only imported when needed as BPM fallback
    y, sr = librosa.load(str(audio_path), sr=None, mono=True)
    tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
    return float(tempo)


def snap_to_grid(
    notes: List[pretty_midi.Note],
    bpm: float,
    subdivision: int,
) -> List[pretty_midi.Note]:
    """
    Snap note start/end times to the nearest rhythmic grid point.

    Args:
        notes:       List of pretty_midi.Note objects.
        bpm:         Tempo in beats per minute.
        subdivision: Grid resolution. 4=quarter, 8=eighth, 16=sixteenth.

    Returns:
        New list of quantized pretty_midi.Note objects.
    """
    seconds_per_beat = 60.0 / bpm
    # subdivision=8 → eighth notes → grid_size = beat / 2
    grid_size = seconds_per_beat / (subdivision / 4)

    quantized: List[pretty_midi.Note] = []
    for note in notes:
        start = round(note.start / grid_size) * grid_size
        end   = round(note.end   / grid_size) * grid_size

        # Guarantee minimum length of one grid cell
        if end <= start:
            end = start + grid_size

        quantized.append(
            pretty_midi.Note(
                velocity=note.velocity,
                pitch=note.pitch,
                start=round(start, 6),
                end=round(end,   6),
            )
        )
    return quantized


def cleanup_notes(
    notes: List[pretty_midi.Note],
    min_duration: float = 0.05,
) -> List[pretty_midi.Note]:
    """
    Remove notes shorter than *min_duration* seconds.
    These are almost always transcription noise, not real notes.
    """
    return [n for n in notes if (n.end - n.start) >= min_duration]


def _rebuild_midi(
    source_midi: pretty_midi.PrettyMIDI,
    notes_by_instrument: Dict[int, List[pretty_midi.Note]],
    bpm: float,
) -> pretty_midi.PrettyMIDI:
    """
    Reconstruct a PrettyMIDI object with quantized notes,
    preserving instrument program numbers and names.
    """
    new_midi = pretty_midi.PrettyMIDI(initial_tempo=bpm)

    for idx, instrument in enumerate(source_midi.instruments):
        new_inst = pretty_midi.Instrument(
            program=instrument.program,
            is_drum=instrument.is_drum,
            name=instrument.name,
        )
        new_inst.notes = notes_by_instrument.get(idx, [])
        new_midi.instruments.append(new_inst)

    return new_midi


# ─────────────────────────────────────────────
# Public entry point
# ─────────────────────────────────────────────

def quantize_midi(
    midi_files: Dict[str, Path],
    config: Dict[str, Any],
    *,
    output_dir: Path,
) -> Tuple[Dict[str, Path], Optional[float]]:
    """
    Quantize a collection of MIDI files to a common rhythmic grid.

    A single BPM is detected once (from the first file or config hint) and
    applied to all stems so they stay rhythmically aligned.

    Args:
        midi_files: Mapping of stem name → raw MIDI path.
        config:     Full config dict; reads config["quantization"].
        output_dir: Directory for quantized .mid files.

    Returns:
        (quantized_paths, detected_bpm)
    """
    output_dir.mkdir(parents=True, exist_ok=True)

    q_cfg        = config.get("quantization", {})
    subdivision  = int(q_cfg.get("subdivision", 8))
    bpm_hint     = q_cfg.get("bpm", None)
    min_duration = float(q_cfg.get("min_note_duration", 0.05))

    # ── Stage 1: Determine BPM ────────────────────────────────────────────
    detected_bpm: Optional[float] = None

    if bpm_hint:
        detected_bpm = float(bpm_hint)
        logger.debug("Using user-supplied BPM: %.1f", detected_bpm)
    else:
        # Try MIDI tempo events from the first available file
        first_path = next(iter(midi_files.values()), None)
        if first_path:
            try:
                midi_probe = pretty_midi.PrettyMIDI(str(first_path))
                detected_bpm = midi_probe.estimate_tempo()
                if detected_bpm:
                    logger.debug("BPM from MIDI events: %.1f", detected_bpm)
            except Exception as exc:
                logger.warning("MIDI tempo probe failed: %s", exc)

        # Fallback: librosa beat tracking on the audio stem
        # (stem path mirrors midi path with .wav extension in stems/)
        if not detected_bpm and first_path:
            try:
                # Infer the corresponding wav path heuristically
                stem_wav = first_path.parent.parent / "stems" / (
                    first_path.stem.replace("_quantized", "") + ".wav"
                )
                if stem_wav.exists():
                    detected_bpm = _estimate_bpm_from_audio(stem_wav)
                    logger.debug("BPM from librosa: %.1f", detected_bpm)
            except Exception as exc:
                logger.warning("librosa BPM estimation failed: %s", exc)

        if not detected_bpm:
            detected_bpm = 120.0
            logger.warning("Could not detect BPM — defaulting to 120")

    # ── Stage 2: Quantize each MIDI file ──────────────────────────────────
    quantized_paths: Dict[str, Path] = {}

    for name, midi_path in midi_files.items():
        logger.debug("Quantizing '%s' at %.1f BPM / subdivision %d", name, detected_bpm, subdivision)
        try:
            midi = pretty_midi.PrettyMIDI(str(midi_path))

            notes_by_instrument: Dict[int, List[pretty_midi.Note]] = {}
            for idx, instrument in enumerate(midi.instruments):
                snapped  = snap_to_grid(instrument.notes, detected_bpm, subdivision)
                cleaned  = cleanup_notes(snapped, min_duration)
                notes_by_instrument[idx] = cleaned

            new_midi = _rebuild_midi(midi, notes_by_instrument, detected_bpm)

            out_path = output_dir / f"{midi_path.stem}_quantized.mid"
            new_midi.write(str(out_path))
            quantized_paths[name] = out_path
            logger.debug("Quantized MIDI → %s", out_path)

        except Exception as exc:
            logger.error("Failed to quantize '%s': %s", name, exc)
            raise

    return quantized_paths, detected_bpm
