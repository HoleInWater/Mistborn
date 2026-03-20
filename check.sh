#!/bin/bash
# check.sh — Mistborn Project Health Check
# Run: bash check.sh

echo ""
echo "MISTBORN ERA ONE — PROJECT CHECK"
echo "======================================"
echo "Date: $(date)"
echo ""

echo "--- TODO COMMENTS IN CODE ---"
grep -rn "TODO" Assets/_Project/Scripts/ --include="*.cs" | sed 's|Assets/_Project/Scripts/||' | awk -F: '{
    file=$1;
    if(file != lastfile) {
      print "\nFile: " file;
      lastfile=file;
    }
    print "   Line " $2 ": " $3
  }'

echo ""
echo "--- TODO COUNT ---"
TOTAL=$(grep -rn "TODO" Assets/_Project/Scripts/ --include="*.cs" | wc -l)
AI_TODOS=$(grep -rn "TODO (AI Agent):" Assets/_Project/Scripts/ --include="*.cs" | wc -l)
TEAM_TODOS=$(grep -rn "TODO (Team):" Assets/_Project/Scripts/ --include="*.cs" | wc -l)
echo "  Total TODOs   : $TOTAL"
echo "  AI TODOs: $AI_TODOS"
echo "  Team TODOs: $TEAM_TODOS"

echo ""
echo "--- GIT STATUS ---"
git status --short

echo ""
echo "--- RECENT COMMITS ---"
git log --oneline -5

echo ""
echo "--- MASTER TODO STATUS ---"
DONE=$(grep -c "\[x\]" TODO.md 2>/dev/null || echo 0)
OPEN=$(grep -c "\[ \]" TODO.md 2>/dev/null || echo 0)
echo "  Completed : $DONE"
echo "  Open      : $OPEN"

echo ""
echo "Check complete. Good luck out there."
echo ""
