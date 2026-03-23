"""
tests/test_runner.py

Integration tests for pipeline/runner.py

All external stage functions are mocked so we can test orchestration logic
(error handling, partial results, PipelineResult assembly) without real models.
"""

from __future__ import annotations

from pathlib import Path
from unittest.mock import MagicMock, patch

import pytest
import torch


@pytest.fixture()
def dummy_audio(tmp_path):
    import torchaudio
    wav = tmp_path / "song.wav"
    torchaudio.save(str(wav), torch.zeros(2, 44100), 44100)
    return wav


@pytest.fixture()
def dummy_midi(tmp_path):
    import pretty_midi
    midi = pretty_midi.PrettyMIDI(initial_tempo=120.0)
    inst = pretty_midi.Instrument(program=0)
    inst.notes.append(pretty_midi.Note(velocity=80, pitch=60, start=0.0, end=0.5))
    midi.instruments.append(inst)
    path = tmp_path / "vocals.mid"
    midi.write(str(path))
    return path


@pytest.fixture()
def base_config():
    return {
        "separation":    {"model": "htdemucs", "device": "cpu", "stems": ["vocals"]},
        "transcription": {"onset_threshold": 0.5, "frame_threshold": 0.3, "minimum_note_length": 0.1},
        "quantization":  {"bpm": 120.0, "subdivision": 8},
        "rendering":     {"musescore_path": "musescore", "title": "Test"},
        "output":        {"base_dir": "data"},
    }


# ─────────────────────────────────────────────
# Happy path
# ─────────────────────────────────────────────

def test_successful_pipeline(dummy_audio, dummy_midi, base_config, tmp_path):
    from transcriber.pipeline.runner import run_pipeline

    stems     = {"vocals": dummy_midi.parent / "vocals.wav"}
    midi_map  = {"vocals": dummy_midi}
    quant_map = {"vocals": dummy_midi}   # reuse for simplicity
    pdf       = tmp_path / "score.pdf"
    xml       = tmp_path / "score.musicxml"

    with patch("transcriber.pipeline.runner.separate_audio",   return_value=stems), \
         patch("transcriber.pipeline.runner.transcribe_stem",  return_value=dummy_midi), \
         patch("transcriber.pipeline.runner.quantize_midi",    return_value=(quant_map, 120.0)), \
         patch("transcriber.pipeline.runner.render_score",     return_value=(pdf, xml)):

        result = run_pipeline(dummy_audio, base_config, output_dir=tmp_path)

    assert result.status        == "success"
    assert result.stems         == stems
    assert result.bpm           == pytest.approx(120.0)
    assert result.score_pdf     == pdf
    assert result.score_musicxml == xml
    assert not result.errors


# ─────────────────────────────────────────────
# Failure handling
# ─────────────────────────────────────────────

def test_separation_failure_returns_failed_result(dummy_audio, base_config, tmp_path):
    from transcriber.pipeline.runner import run_pipeline

    with patch("transcriber.pipeline.runner.separate_audio", side_effect=RuntimeError("GPU OOM")):
        result = run_pipeline(dummy_audio, base_config, output_dir=tmp_path)

    assert result.status == "failed"
    assert "separation" in result.errors
    assert "GPU OOM" in result.errors["separation"]


def test_transcription_failure_marks_partial(dummy_audio, dummy_midi, base_config, tmp_path):
    from transcriber.pipeline.runner import run_pipeline

    stems = {"vocals": dummy_midi.parent / "vocals.wav"}

    with patch("transcriber.pipeline.runner.separate_audio",  return_value=stems), \
         patch("transcriber.pipeline.runner.transcribe_stem", side_effect=RuntimeError("model load failed")), \
         patch("transcriber.pipeline.runner.quantize_midi"), \
         patch("transcriber.pipeline.runner.render_score"):

        result = run_pipeline(dummy_audio, base_config, output_dir=tmp_path)

    assert result.status == "failed"
    assert any("transcription" in k for k in result.errors)


def test_quantization_failure_returns_partial(dummy_audio, dummy_midi, base_config, tmp_path):
    from transcriber.pipeline.runner import run_pipeline

    stems    = {"vocals": dummy_midi.parent / "vocals.wav"}
    midi_map = {"vocals": dummy_midi}

    with patch("transcriber.pipeline.runner.separate_audio",  return_value=stems), \
         patch("transcriber.pipeline.runner.transcribe_stem", return_value=dummy_midi), \
         patch("transcriber.pipeline.runner.quantize_midi",   side_effect=RuntimeError("bad midi")):

        result = run_pipeline(dummy_audio, base_config, output_dir=tmp_path)

    assert result.status == "partial"
    assert "quantization" in result.errors
    assert result.stems    == stems       # stage 1 preserved
    assert result.midi_files              # stage 2 preserved


def test_rendering_failure_returns_partial(dummy_audio, dummy_midi, base_config, tmp_path):
    from transcriber.pipeline.runner import run_pipeline

    stems     = {"vocals": dummy_midi.parent / "vocals.wav"}
    quant_map = {"vocals": dummy_midi}

    with patch("transcriber.pipeline.runner.separate_audio",  return_value=stems), \
         patch("transcriber.pipeline.runner.transcribe_stem", return_value=dummy_midi), \
         patch("transcriber.pipeline.runner.quantize_midi",   return_value=(quant_map, 120.0)), \
         patch("transcriber.pipeline.runner.render_score",    side_effect=RuntimeError("musescore missing")):

        result = run_pipeline(dummy_audio, base_config, output_dir=tmp_path)

    assert result.status == "partial"
    assert "rendering" in result.errors


# ─────────────────────────────────────────────
# PipelineResult helpers
# ─────────────────────────────────────────────

def test_pipeline_result_summary_no_error(dummy_audio):
    from transcriber.pipeline.runner import PipelineResult

    r = PipelineResult(input_audio=dummy_audio, status="success")
    summary = r.summary()
    assert "success" in summary


def test_pipeline_result_failed_stages(dummy_audio):
    from transcriber.pipeline.runner import PipelineResult

    r = PipelineResult(
        input_audio=dummy_audio,
        status="partial",
        errors={"transcription.vocals": "timeout"},
    )
    assert "transcription.vocals" in r.failed_stages()
    assert not r.succeeded()
