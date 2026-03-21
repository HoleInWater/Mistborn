# GitHub Guide for Mistborn Project

## Basic Git Commands

### Clone the Repository
```bash
git clone https://github.com/HoleInWater/Mistborn.git
cd Mistborn
```

### Check Status
```bash
git status
```

### View Changes
```bash
git diff
```

### Add & Commit
```bash
git add .
git commit -m "Description of changes"
```

### Push Changes
```bash
git push origin main
```

### Pull Changes
```bash
git pull --rebase origin main
```

---

## Renaming Folders

**IMPORTANT: Always use `git mv` instead of manually renaming folders!**

### Wrong Way (causes merge issues)
```bash
mv Scripts Scripts_New        # DON'T do this
```

### Correct Way
```bash
git mv Assets/_Project/Scripts/Combat Assets/_Project/Scripts/Enemy
```

This renames the folder AND tells Git to track it as a rename (not delete + add).

### After Renaming
```bash
git add -A
git commit -m "Rename Combat folder to Enemy"
git push origin main
```

### Verify in GitHub Desktop
1. Open GitHub Desktop
2. The rename will appear as a rename (green folder icon)
3. Commit and push as normal

---

## Moving Files Between Folders

```bash
# Move a single file
git mv Assets/Scripts/OldFile.cs Assets/Scripts/NewFolder/NewFile.cs

# Move multiple files
git mv Assets/Scripts/File1.cs Assets/Scripts/File2.cs Assets/NewFolder/
```

---

## Creating & Managing Branches

### Create a New Branch
```bash
git checkout -b feature/new-ability
```

### Switch Between Branches
```bash
git checkout main
git checkout feature/new-ability
```

### List All Branches
```bash
git branch -a
```

### Delete a Branch
```bash
# Local
git branch -d feature/old-feature

# Remote
git push origin --delete feature/old-feature
```

### Merge Branch into Main
```bash
git checkout main
git merge feature/new-ability
git push origin main
```

---

## Handling Merge Conflicts

### When Pulling
```bash
git pull --rebase origin main
# If conflicts:
# 1. Open conflicting files
# 2. Fix the <<<<<<< ======= >>>>>>> sections
# 3. git add <fixed-file>
# 4. git rebase --continue
```

### When Merging
```bash
git checkout main
git merge feature/branch
# If conflicts: fix, add, commit
```

---

## Unity-Specific Gitignore

Make sure `.gitignore` includes:
```
# Unity
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Uu]ser[Ss]ettings/
*.unitypackage
*.asset
```

**Important: Don't commit the `Library` folder!**

---

## Working with GitHub Desktop

### 1. Clone Repository
- File → Clone Repository → URL → paste `https://github.com/HoleInWater/Mistborn.git`

### 2. Create Branch for Changes
- Branch → New Branch → name it (e.g., `feature/pewter-enhancement`)

### 3. Make Changes
- Edit files in your code editor
- GitHub Desktop shows all changes in the left panel

### 4. Commit Changes
- Select files to include
- Write a commit message
- Click "Commit to feature/name"

### 5. Push Branch
- Click "Publish branch" (first time) or "Push origin"

### 6. Create Pull Request (when ready)
- GitHub Desktop → Branch → Create Pull Request
- This opens GitHub in browser to submit PR

---

## Collaborator Workflow

### Owner (HoleInWater)
1. Reviews pull requests
2. Merges approved changes
3. Manages repository settings

### Collaborators
1. Clone the repo
2. Create feature branches
3. Commit changes to your branch
4. Push and create PR when ready
5. Owner reviews and merges

---

## Common Issues

### "Your branch is ahead of origin/main"
```bash
git push origin main
```

### "Please tell me who you are"
```bash
git config --global user.email "your@email.com"
git config --global user.name "Your Name"
```

### "Permission denied"
- Make sure you've been added as a collaborator in repo Settings → Collaborators
- Or regenerate SSH key: https://github.com/settings/keys

### "Merge conflict in binary file"
- Binary files (images, .unity) can't be merged manually
- Usually solved by: keeping local OR keeping remote, never both

---

## Useful Aliases

Add to `~/.gitconfig`:
```ini
[alias]
    co = checkout
    br = branch
    ci = commit
    st = status
    lg = log --oneline --graph --decorate
```
