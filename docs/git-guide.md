# Git & GitHub Guide

*Understanding version control for this project*

---

## What is Git?

Git is a **version control system**. It tracks changes to your files over time. Think of it like an unlimited undo button for your entire project.

### Key Concepts

| Term | What It Means |
|------|---------------|
| **Repository (Repo)** | The project folder with all its history |
| **Commit** | A snapshot of changes (like saving a game) |
| **Branch** | A separate copy to work on without breaking the main version |
| **Push** | Send your commits to GitHub (the cloud) |
| **Pull** | Download commits from GitHub to your computer |
| **Merge** | Combine branches together |

---

## What is the `.git` Folder?

When you clone a repo, you see folders and files. The `.git` folder is hidden — it contains:

```
.git/
├── config          # This repo's settings
├── objects/        # All your commits (compressed)
├── refs/           # Branch information
├── HEAD            # Which branch you're on
└── logs/           # History of all changes
```

**DO NOT:**
- Delete the `.git` folder
- Edit files inside `.git`
- Move the `.git` folder

**The `.git` folder IS your project history.** Without it, you lose everything.

---

## What is GitHub?

GitHub is a **website** that hosts Git repositories online. It lets you:
- Store your code in the cloud
- Share code with collaborators
- Track issues and bugs
- Review code changes

---

## Basic Git Workflow

### 1. Create a Branch (Before Any Work)

```bash
# Creates a new branch and switches to it
git checkout -b feature/my-cool-feature
```

**Why?** Your changes don't affect the main project until you merge them. This keeps the main branch stable.

### 2. Make Changes

Edit files, add files, delete files — whatever you need to do.

### 3. Check What Changed

```bash
git status
```

This shows you:
- Modified files
- New files (not tracked yet)
- Deleted files

### 4. Stage Changes

```bash
# Stage a specific file
git add filename.cs

# Stage all changes
git add .
```

**Staging** is like selecting files for your next commit.

### 5. Commit Changes

```bash
git commit -m "[SYSTEM] What you changed"
```

A commit is a snapshot. The message should briefly explain what you did.

**Commit Message Format:**
```
[Allomancy] Add Pewter enhancement ability
[Player] Fix camera clipping issue
[Docs] Update README with new instructions
[Fix] Resolve steel push not working in air
```

### 6. Push to GitHub

```bash
# Push your branch to the remote repository
git push origin feature/my-cool-feature
```

**Why push?** So others can see your work and you have a backup.

### 7. Open a Pull Request (PR)

On GitHub:
1. Go to the repository
2. Click "Pull Requests" → "New Pull Request"
3. Select your branch
4. Write a description of your changes
5. Request review from teammates

---

## Branch Types

| Branch | Purpose | Example |
|--------|---------|---------|
| `main` | Stable, shippable code | Don't touch directly |
| `develop` | Integration branch | Merge features here first |
| `feature/*` | New features | `feature/steelpush-jump` |
| `fix/*` | Bug fixes | `fix/camera-jitter` |
| `docs/*` | Documentation | `docs/update-readme` |
| `wip/*` | Work in progress | `wip/experimenting` |

---

## Common Git Commands

```bash
# Check which branch you're on
git branch

# Switch to a branch
git checkout main

# Pull latest changes
git pull origin main

# See recent commits
git log --oneline -10

# Undo unstaged changes to a file
git checkout -- filename.cs

# Undo a commit (careful!)
git revert HEAD

# See what's different from last commit
git diff
```

---

## Resolving Conflicts

Sometimes Git can't automatically merge changes. This happens when two people edited the same line.

**When you see a conflict:**
```
<<<<<<< HEAD
Your changes
=======
Someone else's changes
>>>>>>> branch-name
```

**To fix:**
1. Edit the file, keeping the correct version (or both if needed)
2. Delete the `<<<<`, `====`, `>>>>` markers
3. Save the file
4. `git add filename.cs`
5. `git commit`

---

## .gitignore — What NOT to Track

Some files should never be committed:
- **Build outputs** (`Build/`, `*.exe`)
- **Unity-generated** (`Library/`, `Temp/`)
- **Personal settings** (`UserSettings/`)
- **Secrets** (`*.env`, API keys)

Our `.gitignore` handles this, but don't add sensitive files.

---

## Why Do We Use Git?

### For Collaboration
Multiple people can work on the same project without overwriting each other's work.

### For Safety
Every change is saved. If you break something, you can go back.

### For History
See exactly who changed what and when.

### For Organization
Branches keep features separate until they're ready.

---

## Quick Reference Card

```bash
# Start working on something new
git checkout -b feature/feature-name

# Save your work
git add .
git commit -m "[SYSTEM] Description"
git push origin feature/feature-name

# Get latest changes
git pull origin main

# Switch branches
git checkout main
git checkout feature/other-feature

# Check what changed
git status
git diff
```

---

## Questions?

Ask the team. Git can be confusing at first — everyone struggled with it once.
