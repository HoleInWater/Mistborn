"""
cli.py

Command-line interface for music-transcriber.

Usage examples:
    transcribe song.mp3
    transcribe song.mp3 --instruments vocals,bass
    transcribe song.mp3 --config my_config.yaml --output-dir ./out
    transcribe song.mp3 --bpm 120 --subdivision 16
"""

from __future__ import annotations

import logging
import sys
from pathlib import Path
from typing import Optional

import click
from rich.console import Console
from rich.logging import RichHandler
from rich.panel import Panel
from rich.text import Text

console = Console()


def _setup_logging(verbose: bool) -> None:
    level = logging.DEBUG if verbose else logging.INFO
    logging.basicConfig(
        level=level,
        format="%(message)s",
        handlers=[RichHandler(console=console, rich_tracebacks=True)],
    )


@click.command()
@click.argument("audio", type=click.Path(exists=True, path_type=Path))
@click.option(
    "--config", "-c",
    type=click.Path(path_type=Path),
    default=None,
    help="Path to a YAML config file (merged over defaults).",
)
@click.option(
    "--instruments", "-i",
    default=None,
    help="Comma-separated stems to process. e.g. vocals,bass (default: all from config).",
)
@click.option(
    "--output-dir", "-o",
    type=click.Path(path_type=Path),
    default=None,
    help="Root output directory (default: data/).",
)
@click.option(
    "--bpm",
    type=float,
    default=None,
    help="Override BPM detection with a known tempo.",
)
@click.option(
    "--subdivision",
    type=click.Choice(["4", "8", "16"]),
    default=None,
    help="Quantization grid: 4=quarter, 8=eighth, 16=sixteenth notes.",
)
@click.option(
    "--model",
    type=click.Choice(["htdemucs", "htdemucs_6s", "mdx_extra"]),
    default=None,
    help="Demucs model variant (default: htdemucs).",
)
@click.option("--verbose", "-v", is_flag=True, help="Enable debug logging.")
def main(
    audio: Path,
    config: Optional[Path],
    instruments: Optional[str],
    output_dir: Optional[Path],
    bpm: Optional[float],
    subdivision: Optional[str],
    model: Optional[str],
    verbose: bool,
) -> None:
    """
    Transcribe AUDIO (mp3/wav/flac) to sheet music PDF.

    Runs the full pipeline: separation → transcription → quantization → rendering.
    """
    _setup_logging(verbose)

    from transcriber.pipeline.runner import run_pipeline
    from transcriber.utils.config import load_config

    # ── Load and patch config ─────────────────────────────────────────────
    cfg = load_config(config)

    if bpm is not None:
        cfg.setdefault("quantization", {})["bpm"] = bpm
    if subdivision is not None:
        cfg.setdefault("quantization", {})["subdivision"] = int(subdivision)
    if model is not None:
        cfg.setdefault("separation", {})["model"] = model

    instrument_list = [s.strip() for s in instruments.split(",")] if instruments else None

    # ── Banner ────────────────────────────────────────────────────────────
    console.print(Panel.fit(
        f"[bold cyan]music-transcriber[/bold cyan]\n"
        f"[dim]Input :[/dim] {audio}\n"
        f"[dim]Stems :[/dim] {instrument_list or 'all'}\n"
        f"[dim]Model :[/dim] {cfg.get('separation', {}).get('model', 'htdemucs')}",
        border_style="cyan",
    ))

    # ── Run ───────────────────────────────────────────────────────────────
    result = run_pipeline(
        audio,
        cfg,
        output_dir=output_dir,
        instruments=instrument_list,
    )

    # ── Report ────────────────────────────────────────────────────────────
    console.print()
    if result.succeeded():
        console.print(Panel(
            Text.from_markup(
                f"[bold green]✓ Pipeline complete[/bold green]\n\n"
                f"[dim]PDF     :[/dim] {result.score_pdf}\n"
                f"[dim]MusicXML:[/dim] {result.score_musicxml}\n"
                f"[dim]BPM     :[/dim] {result.bpm:.1f}" if result.bpm else ""
            ),
            border_style="green",
        ))
    elif result.status == "partial":
        console.print(Panel(
            Text.from_markup(
                f"[bold yellow]⚠ Partial success[/bold yellow]\n\n"
                + "\n".join(f"[red]{k}[/red]: {v}" for k, v in result.errors.items())
            ),
            border_style="yellow",
        ))
        sys.exit(2)
    else:
        console.print(Panel(
            Text.from_markup(
                f"[bold red]✗ Pipeline failed[/bold red]\n\n"
                + "\n".join(f"[red]{k}[/red]: {v}" for k, v in result.errors.items())
            ),
            border_style="red",
        ))
        sys.exit(1)


if __name__ == "__main__":
    main()
