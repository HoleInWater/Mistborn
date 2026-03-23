"""
utils/midi.py

Shared MIDI helpers.
"""

from __future__ import annotations

from pathlib import Path
from typing import List

import pretty_midi


def load_midi(path: Path) -> pretty_midi.PrettyMIDI:
    path = Path(path)
    if not path.exists():
        raise FileNotFoundError(f"MIDI file not found: {path}")
    return pretty_midi.PrettyMIDI(str(path))


def note_count(midi: pretty_midi.PrettyMIDI) -> int:
    return sum(len(inst.notes) for inst in midi.instruments)


def all_notes(midi: pretty_midi.PrettyMIDI) -> List[pretty_midi.Note]:
    notes = []
    for inst in midi.instruments:
        notes.extend(inst.notes)
    return sorted(notes, key=lambda n: n.start)
