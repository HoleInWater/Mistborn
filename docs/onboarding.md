# Getting Started

## I just cloned this. What now?

1. **Install Unity 2022 LTS** — Get it from unity.com/download
2. **Open Unity Hub** → Projects → Open → select this folder (the root, not a scene)
3. **Wait** — First import takes a few minutes while Unity processes everything
4. **Open the test scene** — `Assets/_Project/Scenes/TestArena.unity`
5. **Hit Play** — Use WASD to move, mouse to look

## Before You Start Coding

1. Read `docs/architecture.md` — understand how the scripts connect
2. Read `docs/allomancy-design.md` — understand the game systems
3. Check `TODO.md` — see what's in progress and what's coming up
4. Run `bash check.sh` — see all open TODOs in the code

## Branching and Committing

See `CONTRIBUTING.md` for the full guide. Quick version:

- Create a branch: `git checkout -b feature/your-feature-name`
- Make changes
- Commit: `git commit -m "[SYSTEM] What you did"`
- Push: `git push origin your-branch-name`

## Common Issues

**Unity says scripts are missing?**
- Make sure you have all packages installed (check Packages/manifest.json)
- Try reimporting: Assets → Reimport All

**Scene looks broken?**
- The scene might be in an intermediate state. Check the TODO to see if it's being worked on.

**Camera is weird?**
- Mouse might be stuck. Press Escape to unlock cursor.

## Questions?

Ask the team. Check the issues on GitHub. Don't sit on confusion — reach out.
