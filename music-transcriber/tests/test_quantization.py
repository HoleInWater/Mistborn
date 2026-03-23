"""
tests/test_quantization.py

Unit tests for quantization/quantizer.py

These tests are pure Python — no ML models, no audio I/O.
They verify the core grid-snapping math and cleanup logic directly.
"""

from __future__ import annotations

from pathlib import Path

import pretty_midi
import pytest


# ─────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────

def _make_note(start: float, end: float, pitch: int = 60, velocity: int = 80) -> pretty_midi.Note:
    return pretty_midi.Note(velocity=velocity, pitch=pitch, start=start, end=end)


def _make_midi_file(notes: list, tmp_path: Path, name: str = "test") -> Path:
    midi = pretty_midi.PrettyMIDI(initial_tempo=120.0)
    inst = pretty_midi.Instrument(program=0, name="test")
    inst.notes = notes
    midi.instruments.append(inst)
    path = tmp_path / f"{name}.mid"
    midi.write(str(path))
    return path


# ─────────────────────────────────────────────
# snap_to_grid
# ─────────────────────────────────────────────

class TestSnapToGrid:
    def test_already_on_grid(self):
        from transcriber.quantization.quantizer import snap_to_grid

        # At 120 BPM, eighth note grid = 0.25 s
        notes = [_make_note(0.0, 0.25), _make_note(0.25, 0.5)]
        result = snap_to_grid(notes, bpm=120.0, subdivision=8)

        assert result[0].start == pytest.approx(0.0)
        assert result[0].end   == pytest.approx(0.25)
        assert result[1].start == pytest.approx(0.25)

    def test_snaps_off_grid_note(self):
        from transcriber.quantization.quantizer import snap_to_grid

        # Note starts 30ms late — should snap back to 0.0
        notes = [_make_note(0.03, 0.28)]
        result = snap_to_grid(notes, bpm=120.0, subdivision=8)

        assert result[0].start == pytest.approx(0.0)

    def test_zero_length_note_gets_minimum_duration(self):
        from transcriber.quantization.quantizer import snap_to_grid

        # start and end both snap to 0.0 → end should be bumped to grid_size
        notes = [_make_note(0.01, 0.02)]
        result = snap_to_grid(notes, bpm=120.0, subdivision=8)

        assert result[0].end > result[0].start

    def test_preserves_pitch_and_velocity(self):
        from transcriber.quantization.quantizer import snap_to_grid

        notes = [_make_note(0.0, 0.25, pitch=72, velocity=100)]
        result = snap_to_grid(notes, bpm=120.0, subdivision=8)

        assert result[0].pitch    == 72
        assert result[0].velocity == 100

    def test_quarter_note_subdivision(self):
        from transcriber.quantization.quantizer import snap_to_grid

        # At 120 BPM quarter note = 0.5 s
        notes = [_make_note(0.1, 0.6)]
        result = snap_to_grid(notes, bpm=120.0, subdivision=4)

        assert result[0].start == pytest.approx(0.0)
        assert result[0].end   == pytest.approx(0.5)

    def test_sixteenth_note_subdivision(self):
        from transcriber.quantization.quantizer import snap_to_grid

        # At 120 BPM sixteenth note = 0.125 s
        notes = [_make_note(0.13, 0.26)]
        result = snap_to_grid(notes, bpm=120.0, subdivision=16)

        assert result[0].start == pytest.approx(0.125)
        assert result[0].end   == pytest.approx(0.25)

    def test_empty_notes_returns_empty(self):
        from transcriber.quantization.quantizer import snap_to_grid

        assert snap_to_grid([], bpm=120.0, subdivision=8) == []


# ─────────────────────────────────────────────
# cleanup_notes
# ─────────────────────────────────────────────

class TestCleanupNotes:
    def test_removes_short_notes(self):
        from transcriber.quantization.quantizer import cleanup_notes

        notes = [
            _make_note(0.0, 0.03),   # 30 ms — below 50 ms threshold
            _make_note(0.5, 0.6),    # 100 ms — keep
        ]
        result = cleanup_notes(notes, min_duration=0.05)

        assert len(result) == 1
        assert result[0].start == pytest.approx(0.5)

    def test_keeps_notes_at_threshold(self):
        from transcriber.quantization.quantizer import cleanup_notes

        notes = [_make_note(0.0, 0.05)]
        result = cleanup_notes(notes, min_duration=0.05)

        assert len(result) == 1

    def test_empty_input(self):
        from transcriber.quantization.quantizer import cleanup_notes

        assert cleanup_notes([], min_duration=0.05) == []


# ─────────────────────────────────────────────
# quantize_midi (integration)
# ─────────────────────────────────────────────

class TestQuantizeMidi:
    def test_produces_output_files(self, tmp_path):
        from transcriber.quantization.quantizer import quantize_midi

        midi_path = _make_midi_file(
            [_make_note(0.03, 0.28), _make_note(0.53, 0.78)],
            tmp_path,
        )
        config = {"quantization": {"bpm": 120.0, "subdivision": 8, "min_note_duration": 0.05}}

        out_dir = tmp_path / "quantized"
        result, bpm = quantize_midi({"test": midi_path}, config, output_dir=out_dir)

        assert "test" in result
        assert result["test"].exists()

    def test_uses_bpm_hint(self, tmp_path):
        from transcriber.quantization.quantizer import quantize_midi

        midi_path = _make_midi_file([_make_note(0.0, 0.5)], tmp_path)
        config = {"quantization": {"bpm": 95.0, "subdivision": 8}}

        _, bpm = quantize_midi({"test": midi_path}, config, output_dir=tmp_path / "q")

        assert bpm == pytest.approx(95.0)

    def test_output_suffix(self, tmp_path):
        from transcriber.quantization.quantizer import quantize_midi

        midi_path = _make_midi_file([_make_note(0.0, 0.5)], tmp_path, name="bass")
        config = {"quantization": {"bpm": 120.0, "subdivision": 8}}

        result, _ = quantize_midi({"bass": midi_path}, config, output_dir=tmp_path / "q")

        assert "_quantized" in result["bass"].name

    def test_handles_multiple_stems(self, tmp_path):
        from transcriber.quantization.quantizer import quantize_midi

        stems = {
            "vocals": _make_midi_file([_make_note(0.0, 0.5)], tmp_path, name="vocals"),
            "bass":   _make_midi_file([_make_note(0.0, 0.5)], tmp_path, name="bass"),
        }
        config = {"quantization": {"bpm": 120.0, "subdivision": 8}}

        result, _ = quantize_midi(stems, config, output_dir=tmp_path / "q")

        assert set(result.keys()) == {"vocals", "bass"}
