# MISTBORN ERA ONE — TEAM PLAYBOOK

*How we work together on this project*

---

## Getting Started

1. Clone the repo
2. Install Unity 2022 LTS
3. Open project in Unity (open root folder, not scene)
4. Read `docs/onboarding.md`
5. Run `bash check.sh` to see what's needed

---

## Communication

- Keep discussions on GitHub Issues
- Use PRs for all changes (even small ones)
- Tag teammates for review

---

## Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Stable, shippable code |
| `develop` | Integration branch |
| `feature/*` | New features |
| `fix/*` | Bug fixes |
| `docs/*` | Documentation |
| `wip/*` | Work in progress, don't merge yet |

---

## Commit Format

```
[SYSTEM] Short description

Longer explanation if needed.
```

Examples:
- `[Allomancy] Add weight-proportional push force`
- `[UI] Hook up metal reserve bars`
- `[Docs] Update lore notes with Coppermind references`
- `[Fix] Resolve camera clipping through walls`

---

## Pull Request Rules

1. Create PR from feature branch → develop
2. Fill out PR template
3. Request review from at least one teammate
4. Address feedback
5. Squash and merge

---

## Working in Unity

### Scene Rules
- **Never edit the same scene simultaneously**
- Claim scenes in the team chat before working
- Commit scene changes frequently

### Prefab Rules
- All metal objects must follow naming: `Metal_[Type]_[Number]`
- Every AllomanticTarget must have correct mass and isAnchored set

### Script Rules
- Every script needs the header template (see CONTRIBUTING.md)
- TODO comments go in the header
- No magic numbers — use constants

---

## Code Review Checklist

Before approving a PR:

- [ ] Code follows existing patterns
- [ ] No TODO comments left (unless marked Team decision)
- [ ] Lore accuracy verified (check Coppermind)
- [ ] Scene changes are minimal and tested
- [ ] Builds without errors

---

## Reference Resources

- **Lore**: https://coppermind.net/wiki/Allomancy
- **Physics Reference**: https://github.com/austin-j-taylor/Invested
- **Brandon's FAQ**: https://faq.brandonsanderson.com

---

## Questions?

Ping the team. No question is too small.
