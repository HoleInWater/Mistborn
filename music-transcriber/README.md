# music-transcriber

AI-powered pipeline that converts an MP3 into sheet music PDF.

```
audio.mp3  →  stems  →  MIDI  →  quantized MIDI  →  score.pdf
             Demucs     Basic Pitch    quantizer       MuseScore
```

---

## Features

- **Source separation** via Meta's Demucs v4 (`htdemucs`, `htdemucs_6s`, `mdx_extra`)
- **MIDI transcription** via Spotify's Basic Pitch (MT3 backend in Phase 2)
- **Rhythmic quantization** with automatic BPM detection (librosa fallback)
- **Sheet music rendering** to MusicXML + PDF via MuseScore
- **Artifact-based pipeline** — each stage saves to disk; re-run any stage independently
- **Partial failure recovery** — always returns a `PipelineResult`, even on stage errors
- **Rich CLI** with per-stage progress and coloured status output

---

## Requirements

| Dependency | Version | Notes |
|---|---|---|
| Python | ≥ 3.10 | |
| PyTorch | ≥ 2.0 | CPU or CUDA |
| MuseScore | 4.x | For PDF export — [download](https://musescore.org) |

---

## Installation

```bash
# 1. Clone
git clone https://github.com/YOUR_USERNAME/music-transcriber.git
cd music-transcriber

# 2. Create virtual environment
python -m venv .venv
source .venv/bin/activate      # Windows: .venv\Scripts\activate

# 3. Install
pip install -e ".[dev]"

# 4. Verify
transcribe --help
```

---

## Quick Start

```bash
# Transcribe all stems
transcribe data/input/song.mp3

# Transcribe vocals and bass only
transcribe song.mp3 --instruments vocals,bass

# Override BPM and use sixteenth-note grid
transcribe song.mp3 --bpm 140 --subdivision 16

# Use the 6-stem model (adds guitar + piano)
transcribe song.mp3 --model htdemucs_6s --instruments guitar,piano

# Custom output directory + verbose logging
transcribe song.mp3 --output-dir ./my_output --verbose
```

### Output structure

```
data/
├── stems/
│   ├── song_vocals.wav
│   └── song_bass.wav
├── midi/
│   ├── song_vocals.mid
│   └── song_bass.mid
├── midi_quantized/
│   ├── song_vocals_quantized.mid
│   └── song_bass_quantized.mid
└── scores/
    ├── score.musicxml      ← always generated
    └── score.pdf           ← requires MuseScore
```

---

## Configuration

Copy and edit `config/default.yaml`:

```yaml
separation:
  model: htdemucs        # htdemucs | htdemucs_6s | mdx_extra
  device: cpu            # cpu | cuda | mps
  stems: [vocals, bass, other, drums]

transcription:
  onset_threshold: 0.5   # higher = fewer notes detected
  frame_threshold: 0.3
  minimum_note_length: 0.1

quantization:
  subdivision: 8         # 4=quarter, 8=eighth, 16=sixteenth
  bpm: null              # null = auto-detect

rendering:
  musescore_path: musescore
  title: "My Transcription"
```

Pass your config with `--config my_config.yaml`. CLI flags override config values.

---

## Architecture

```
src/transcriber/
├── pipeline/
│   └── runner.py          # orchestrator — PipelineResult, run_pipeline()
├── separation/
│   └── demucs.py          # separate_audio()
├── transcription/
│   ├── basic_pitch.py     # transcribe_stem()  ← active backend
│   └── mt3.py             # transcribe_stem()  ← Phase 2 stub
├── quantization/
│   └── quantizer.py       # quantize_midi(), snap_to_grid(), cleanup_notes()
├── rendering/
│   └── sheet_music.py     # render_score()
├── utils/
│   ├── audio.py
│   ├── midi.py
│   └── config.py
└── cli.py                 # Click entry point
```

### Design principles

- **Loosely coupled stages** — each stage reads from and writes to disk; swap any backend without touching others
- **Single BPM** across all stems — prevents rhythmic drift between parts
- **MusicXML as source of truth** — PDF is a render; MusicXML is always preserved
- **Graceful degradation** — MuseScore missing → warn + return MusicXML only; stage failure → partial result with all prior artifacts intact

---

## Running tests

```bash
pytest tests/ -v
pytest tests/ -v --cov=src/transcriber --cov-report=term-missing
```

Tests mock all ML inference — they run instantly, no GPU required.

---

## Roadmap

- [ ] **Phase 2**: MT3 transcription backend for better polyphonic handling
- [ ] **Instrument hinting**: frequency-band filtering to guide extraction
- [ ] **Drum notation**: separate percussion clef rendering
- [ ] **MIDI export**: `--format midi` flag alongside PDF
- [ ] **Web UI**: upload + configure in browser, download PDF

---

## Known limitations

- Demucs v4 separates into 4–6 fixed stems. Instruments that fall into "other" (violin, brass, etc.) will have lower transcription accuracy.
- Polyphonic transcription accuracy degrades with dense mixes. Start with clean pop/rock recordings.
- PDF export requires MuseScore 4 installed locally. MusicXML is always available as a fallback.

---

## License

MIT
