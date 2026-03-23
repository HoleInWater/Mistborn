"""
tests/test_transcription.py

Unit tests for transcription/basic_pitch.py

Heavy ML inference is mocked so tests run without GPU or model weights.
"""

from __future__ import annotations

from pathlib import Path
from unittest.mock import MagicMock, patch

import pretty_midi
import pytest
import torch


@pytest.fixture()
def dummy_stem(tmp_path):
    """Create a minimal valid WAV stem file."""
    import torchaudio
    wav = tmp_path / "test_vocals.wav"
    torchaudio.save(str(wav), torch.zeros(2, 44100), 44100)
    return wav


@pytest.fixture()
def base_config():
    return {
        "transcription": {
            "backend": "basic_pitch",
            "onset_threshold": 0.5,
            "frame_threshold": 0.3,
            "minimum_note_length": 0.1,
        }
    }


def _make_mock_midi() -> pretty_midi.PrettyMIDI:
    """Return a minimal PrettyMIDI object with one note."""
    midi = pretty_midi.PrettyMIDI(initial_tempo=120.0)
    inst = pretty_midi.Instrument(program=0, name="test")
    inst.notes.append(pretty_midi.Note(velocity=80, pitch=60, start=0.0, end=0.5))
    midi.instruments.append(inst)
    return midi


@patch("transcriber.transcription.basic_pitch.predict")
def test_transcribe_stem_creates_midi_file(mock_predict, dummy_stem, base_config, tmp_path):
    from transcriber.transcription.basic_pitch import transcribe_stem

    mock_predict.return_value = ({}, _make_mock_midi(), [])

    result = transcribe_stem(dummy_stem, base_config, output_dir=tmp_path / "midi")

    assert result.exists()
    assert result.suffix == ".mid"


@patch("transcriber.transcription.basic_pitch.predict")
def test_transcribe_stem_output_filename(mock_predict, dummy_stem, base_config, tmp_path):
    from transcriber.transcription.basic_pitch import transcribe_stem

    mock_predict.return_value = ({}, _make_mock_midi(), [])
    out_dir = tmp_path / "midi"

    result = transcribe_stem(dummy_stem, base_config, output_dir=out_dir)

    assert result.name == f"{dummy_stem.stem}.mid"


@patch("transcriber.transcription.basic_pitch.predict")
def test_transcribe_stem_passes_config_thresholds(mock_predict, dummy_stem, base_config, tmp_path):
    from transcriber.transcription.basic_pitch import transcribe_stem

    mock_predict.return_value = ({}, _make_mock_midi(), [])

    transcribe_stem(dummy_stem, base_config, output_dir=tmp_path)

    _, kwargs = mock_predict.call_args
    assert kwargs["onset_threshold"]     == pytest.approx(0.5)
    assert kwargs["frame_threshold"]     == pytest.approx(0.3)
    assert kwargs["minimum_note_length"] == pytest.approx(0.1)


def test_transcribe_stem_missing_file(base_config, tmp_path):
    from transcriber.transcription.basic_pitch import transcribe_stem

    with pytest.raises(FileNotFoundError):
        transcribe_stem(Path("nonexistent.wav"), base_config, output_dir=tmp_path)


@patch("transcriber.transcription.basic_pitch.predict")
def test_transcribe_stem_creates_output_dir(mock_predict, dummy_stem, base_config, tmp_path):
    from transcriber.transcription.basic_pitch import transcribe_stem

    mock_predict.return_value = ({}, _make_mock_midi(), [])
    deep_dir = tmp_path / "a" / "b" / "c"

    assert not deep_dir.exists()
    transcribe_stem(dummy_stem, base_config, output_dir=deep_dir)
    assert deep_dir.exists()
