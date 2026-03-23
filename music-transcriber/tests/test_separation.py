"""
tests/test_separation.py

Unit tests for separation/demucs.py

We mock the heavy ML dependencies so tests run without GPU or large model weights.
"""

from __future__ import annotations

from pathlib import Path
from unittest.mock import MagicMock, patch

import pytest
import torch


@pytest.fixture()
def dummy_audio(tmp_path):
    """Create a minimal valid WAV file."""
    import torchaudio
    wav = tmp_path / "test.wav"
    waveform = torch.zeros(2, 44100)   # 1 second stereo silence
    torchaudio.save(str(wav), waveform, 44100)
    return wav


@pytest.fixture()
def base_config():
    return {
        "separation": {
            "model": "htdemucs",
            "device": "cpu",
            "stems": ["vocals", "bass"],
        }
    }


def _make_mock_model(source_names=("vocals", "drums", "bass", "other"), samplerate=44100):
    model = MagicMock()
    model.sources     = list(source_names)
    model.samplerate  = samplerate
    model.to          = MagicMock(return_value=model)
    model.eval        = MagicMock(return_value=model)
    return model


def _make_mock_sources(n_sources, channels=2, samples=44100):
    """Return a fake apply_model result: [batch, sources, channels, samples]"""
    return [torch.zeros(n_sources, channels, samples)]


@patch("transcriber.separation.demucs.apply_model")
@patch("transcriber.separation.demucs.get_model")
def test_separate_audio_returns_correct_stems(mock_get_model, mock_apply_model, dummy_audio, base_config, tmp_path):
    from transcriber.separation.demucs import separate_audio

    mock_get_model.return_value = _make_mock_model()
    mock_apply_model.return_value = _make_mock_sources(4)

    result = separate_audio(dummy_audio, base_config, output_dir=tmp_path / "stems", stems=["vocals", "bass"])

    assert set(result.keys()) == {"vocals", "bass"}
    for path in result.values():
        assert path.exists()


@patch("transcriber.separation.demucs.apply_model")
@patch("transcriber.separation.demucs.get_model")
def test_separate_audio_all_stems_when_none(mock_get_model, mock_apply_model, dummy_audio, base_config, tmp_path):
    from transcriber.separation.demucs import separate_audio

    mock_get_model.return_value = _make_mock_model()
    mock_apply_model.return_value = _make_mock_sources(4)

    result = separate_audio(dummy_audio, base_config, output_dir=tmp_path / "stems", stems=None)

    assert set(result.keys()) == {"vocals", "drums", "bass", "other"}


def test_separate_audio_missing_file(base_config, tmp_path):
    from transcriber.separation.demucs import separate_audio

    with pytest.raises(FileNotFoundError):
        separate_audio(Path("nonexistent.wav"), base_config, output_dir=tmp_path)


@patch("transcriber.separation.demucs.apply_model")
@patch("transcriber.separation.demucs.get_model")
def test_separate_audio_resamples_when_needed(mock_get_model, mock_apply_model, tmp_path):
    """Input at 22050 Hz should be resampled to model's 44100 Hz."""
    import torchaudio
    from transcriber.separation.demucs import separate_audio

    wav = tmp_path / "low_sr.wav"
    torchaudio.save(str(wav), torch.zeros(2, 22050), 22050)

    mock_get_model.return_value = _make_mock_model(samplerate=44100)
    mock_apply_model.return_value = _make_mock_sources(4, samples=44100)

    config = {"separation": {"model": "htdemucs", "device": "cpu"}}
    result = separate_audio(wav, config, output_dir=tmp_path / "stems")
    assert result  # just verify it ran without error
