"""
utils/audio.py

Shared audio helpers used across pipeline stages.
"""

from __future__ import annotations

from pathlib import Path
from typing import Tuple

import torchaudio

SUPPORTED_EXTENSIONS = {".mp3", ".wav", ".flac", ".ogg", ".m4a", ".aac"}


def validate_audio_path(path: Path) -> Path:
    path = Path(path)
    if not path.exists():
        raise FileNotFoundError(f"Audio file not found: {path}")
    if path.suffix.lower() not in SUPPORTED_EXTENSIONS:
        raise ValueError(
            f"Unsupported audio format '{path.suffix}'. "
            f"Supported: {SUPPORTED_EXTENSIONS}"
        )
    return path


def get_audio_info(path: Path) -> Tuple[int, int, float]:
    """
    Returns (sample_rate, num_channels, duration_seconds).
    Does not load the full waveform.
    """
    info = torchaudio.info(str(path))
    duration = info.num_frames / info.sample_rate
    return info.sample_rate, info.num_channels, duration
