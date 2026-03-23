"""
transcription/mt3.py

Phase 2 transcription backend using Google's MT3 model.
Provides better polyphonic handling at the cost of slower inference.

This is a stub — swap in when MT3 integration is ready.
The function signature is intentionally identical to basic_pitch.transcribe_stem
so runner.py needs zero changes to switch backends.
"""

from __future__ import annotations

from pathlib import Path
from typing import Any, Dict


def transcribe_stem(
    stem_path: Path,
    config: Dict[str, Any],
    *,
    output_dir: Path,
) -> Path:
    raise NotImplementedError(
        "MT3 backend is not yet implemented. "
        "Set config['transcription']['backend'] = 'basic_pitch' to use the default backend."
    )
