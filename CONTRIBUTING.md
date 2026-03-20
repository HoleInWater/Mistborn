# Contributing to Mistborn Era One

Hey, thanks for jumping in. Here's how we keep things organized so we're not stepping on each other's toes.

## Branch Naming

When you're starting something new, create a branch with a clear name:

- `feature/steel-push` — adding new stuff
- `fix/camera-jitter` — fixing bugs
- `docs/update-readme` — documentation changes
- `wip/experimenting` — work in progress, don't merge yet

## Commit Messages

Keep it short and meaningful. Format: `[SYSTEM] What changed`

Examples:
- `[Allomancy] Add force calculation for anchored targets`
- `[Player] Fix camera clipping through walls`
- `[Docs] Update TODO with Sprint 2 tasks`

## Pull Requests

1. Branch off `main`
2. Make your changes
3. Submit a PR and request review from at least one teammate
4. Don't merge until it's approved

## Working in Scenes

**Important**: Don't work in the same scene at the same time as someone else. Unity scenes don't merge well. If you need to work on a scene, claim it in the team chat first.

## TODO Comments in Code

We use structured TODO comments so everyone knows what's done and what needs attention:

```csharp
// TODO (AI Agent):
//   - Specific implementation task

// TODO (Team):
//   - Decision needed from humans
```

Run `bash check.sh` to see all open TODOs in one place.

## Questions?

Ping the team. Don't guess — ask.
