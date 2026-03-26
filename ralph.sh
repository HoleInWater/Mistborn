#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════════════
# ralph.sh — Ralph Wiggum Technique for Mistborn Development
#
# Usage:
#   cd ~/src/github.com/holeinwater/Mistborn
#   nohup ./ralph.sh 15 > ralph_output.log 2>&1 &
#
# Monitor:
#   tail -f ralph_output.log
#   tail -f progress.txt
#   git log --oneline -20
# ═══════════════════════════════════════════════════════════════════════════

set -uo pipefail

export PATH="$HOME/.local/bin:$HOME/.nvm/versions/node/$(ls $HOME/.nvm/versions/node/ 2>/dev/null | tail -1)/bin:/usr/local/bin:/usr/bin:/bin:$PATH"

MAX_ITERATIONS=${1:-15}
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
PLANS="$PROJECT_DIR/plans.json"
PROGRESS="$PROJECT_DIR/progress.txt"

# Find claude
CLAUDE_BIN="$(which claude 2>/dev/null || echo "$HOME/.local/bin/claude")"
if [ ! -x "$CLAUDE_BIN" ]; then
    echo "ERROR: claude not found"
    exit 1
fi

echo "═══════════════════════════════════════════════════════════"
echo "  Ralph Wiggum Loop — Mistborn Development"
echo "  Started: $(date)"
echo "  Claude:  $CLAUDE_BIN"
echo "  Max:     $MAX_ITERATIONS iterations"
echo "═══════════════════════════════════════════════════════════"

cd "$PROJECT_DIR"

for i in $(seq 1 "$MAX_ITERATIONS"); do
    echo ""
    echo "─── Iteration $i / $MAX_ITERATIONS — $(date '+%H:%M:%S') ───"

    # Check if all tasks done
    REMAINING=$(python3 -c "
import json
with open('plans.json') as f:
    data = json.load(f)
print(len([t for t in data['tasks'] if not t['passes']]))
" 2>/dev/null || echo "?")

    if [ "$REMAINING" = "0" ]; then
        echo "✓ All tasks complete! Exiting."
        break
    fi
    echo "  $REMAINING tasks remaining"

    # Run Claude — --print for non-interactive, --dangerously-skip-permissions so no approval needed
    "$CLAUDE_BIN" \
      --print \
      --dangerously-skip-permissions \
      -p "You are working through a development backlog for the Mistborn Unity game.
The project is at: $PROJECT_DIR

INSTRUCTIONS:
1. Read plans.json to find the highest-priority task where \"passes\" is false.
2. Read progress.txt for context on what has been done in previous iterations.
3. Implement that ONE task fully. Write all the code needed.
4. After implementing, update plans.json to set that task's \"passes\" to true.
5. Append a summary of what you did to progress.txt with the current date/time and iteration number ($i).
6. Stage and commit your changes with message format: [AGENT] <description>
7. Do NOT push to remote. Keep changes local.

RULES:
- Only work on ONE task per iteration.
- Follow the project naming conventions (PascalCase scripts, camelCase vars).
- All code is C# for Unity with HDRP.
- Respect lore accuracy — reference docs/PHYSICS-MATH-BOOK.md for physics formulas.
- Read existing code before modifying to understand patterns.
- Do not create documentation files unless the task requires it.

Current iteration: $i of $MAX_ITERATIONS"

    RESULT=$?

    if [ $RESULT -ne 0 ]; then
        echo "⚠ Claude exited with code $RESULT on iteration $i"
        echo "### Iteration $i — $(date) — ERROR (exit code $RESULT)" >> "$PROGRESS"
    else
        echo "  ✓ Iteration $i complete."
    fi

    sleep 3
done

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  Done at $(date). Review: git log --oneline -$MAX_ITERATIONS"
echo "═══════════════════════════════════════════════════════════"
