"""
scripts/run_pipeline.py

Developer convenience script — hardcoded inputs for fast local iteration.
This is NOT a duplicate of cli.py; it is a scratchpad.

Usage:
    python scripts/run_pipeline.py
"""

import logging
from pathlib import Path

from transcriber.pipeline.runner import run_pipeline
from transcriber.utils.config import load_config

logging.basicConfig(level=logging.DEBUG)

config = load_config()

# ── Tweak these for your test session ────────────────────────────────────
AUDIO   = Path("data/input/song.mp3")
STEMS   = ["vocals", "bass"]
OUT_DIR = Path("data")
# ─────────────────────────────────────────────────────────────────────────

result = run_pipeline(AUDIO, config, output_dir=OUT_DIR, instruments=STEMS)
print(result.summary())
