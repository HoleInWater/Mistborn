# Getting Started — Unity & Git Setup

## I just cloned the repo. What now?

### Step 1: Install Unity 2022 LTS

1. Go to https://unity.com/download
2. Download the Unity Hub
3. Open Unity Hub
4. Click **Installs** → **Add** → Select **Unity 2022 LTS** (or 2022.3.x)
5. Make sure you include:
   - **Microsoft Visual Studio** (for C# scripting)
   - **Build Support** for your target platform (Windows, Mac, etc.)

### Step 2: Add the Project to Unity Hub

1. Open Unity Hub
2. Click **Projects**
3. Click **Open**
4. Navigate to where you cloned the repo
5. Select the **Mistborn folder** (the root folder, NOT a scene file)
6. Click **Open**

**Important:** You must open the folder that contains `.git`, not just a scene inside it.

Your folder structure should look like this when Unity opens it:
```
Mistborn/          ← Open THIS folder
├── .git/          ← Hidden folder (Unity needs this)
├── Assets/
├── docs/
├── Packages/
└── ProjectSettings/
```

### Step 3: Wait for Unity to Import

The first time you open a Unity project, it imports all the assets. This can take 5-15 minutes depending on your computer.

You'll see a progress bar at the bottom of Unity. **Don't close Unity while it's importing.**

### Step 4: Open the Test Scene

1. In Unity, find the **Project window** (usually bottom-left)
2. Navigate to `Assets/_Project/Scenes/`
3. Double-click `TestArena.unity`
4. The scene opens in the Editor

### Step 5: Run the Game

1. Hit the **Play** button at the top center of Unity (or press Ctrl+P)
2. Use WASD to move, mouse to look
3. Try pushing/pulling metal objects

---

## Unity & Git — How They Work Together

### What Unity Creates

When you open a Unity project, it creates some folders automatically:

| Folder | What It Is | In Git? |
|--------|-----------|---------|
| `Library/` | Cached data, imported assets | ❌ NO — add to .gitignore |
| `Temp/` | Temporary files | ❌ NO — add to .gitignore |
| `obj/` | Build objects | ❌ NO — add to .gitignore |
| `Logs/` | Unity logs | ❌ NO — add to .gitignore |
| `UserSettings/` | Your personal settings | ❌ NO — add to .gitignore |

### What You Should Commit

| Folder/File | What It Is | In Git? |
|------------|-----------|---------|
| `Assets/` | Your scripts, scenes, models, etc. | ✅ YES |
| `Packages/` | Unity package dependencies | ✅ YES |
| `ProjectSettings/` | Project configuration | ✅ YES |
| `docs/` | Documentation | ✅ YES |
| `.gitignore` | Tells Git what to ignore | ✅ YES |

### Our .gitignore Already Handles This

We've already set up a `.gitignore` file that tells Git to ignore the Unity-generated folders. You don't need to change it.

---

## Making Changes — The Workflow

### 1. Always Pull First

Before you start working, make sure you have the latest changes:

```bash
cd ~/path/to/Mistborn
git pull origin master
```

### 2. Create a Branch

Never work directly on `main`:

```bash
git checkout -b feature/my-feature-name
```

### 3. Make Changes in Unity

- Edit scripts in Unity or Visual Studio
- Create new scenes, prefabs, materials
- Everything you create goes in `Assets/`

### 4. Check What Changed

```bash
git status
```

Unity will have created/modified some files. The `.gitignore` should ignore the big ones.

### 5. Stage and Commit

```bash
# Stage all changes
git add .

# Commit
git commit -m "[SYSTEM] Description of what you did"

# Push
git push origin feature/my-feature-name
```

### 6. Open a Pull Request on GitHub

1. Go to https://github.com/HoleInWater/Mistborn
2. Click "Pull Requests" → "New Pull Request"
3. Select your branch
4. Write what you changed
5. Submit

---

## Unity Generated Files — What You'll See

When you make changes in Unity, these files will change:

### You CAN Commit These
- `Assets/*.cs` — Your scripts
- `Assets/*.unity` — Scene files
- `Assets/*.prefab` — Prefab files
- `Assets/*.mat` — Materials
- `Packages/manifest.json` — Dependencies
- `ProjectSettings/*` — Project settings

### You Should NOT Commit These
These are already in `.gitignore`:
- `Library/` — Everything in here is regenerated
- `Temp/` — Temporary files
- `UserSettings/` — Your personal editor settings
- `obj/` — Build objects

---

## If You Accidentally Committed Library/

Don't panic. Here's how to fix it:

```bash
# Remove Library from git tracking
git rm -r --cached Library/

# Commit the change
git commit -m "Remove Library from tracking"

# Push
git push origin master
```

The Library folder will come back when anyone opens the project — it's just regenerated.

---

## Installing the Git Package in Unity

If you want to use Git directly inside Unity (optional):

1. In Unity, go to **Window** → **Package Manager**
2. Click the **+** button → **Add package from git URL**
3. Paste: `com.unity.collab-proxy`
4. Click **Add**

This adds the **Unity Collaborate** package (if available) or shows Git integration options.

**Note:** This is optional. Most people use Git Bash or GitHub Desktop instead.

---

## Common Issues

### "Scripts are missing" errors
- Make sure you opened the ROOT folder, not a subfolder
- Try: **Assets** → **Reimport All**
- Close Unity and reopen

### "Scene looks empty"
- The TestArena scene needs to be set up
- Run: **Mistborn** → **Setup Test Arena** in the Unity menu

### "My changes don't appear"
- Did you save in Unity? (Ctrl+S)
- Did you commit and push?
- Did you switch to the right branch?

### "Git says I need to pull but I have changes"
```bash
# Stash your changes
git stash

# Pull
git pull origin master

# Apply your changes back
git stash pop
```

### "Merge conflict in Unity scene"
Unity scene files (.unity) are text-based but complex. If you get conflicts:
1. Discuss with your teammate
2. Decide whose version to keep
3. Keep that version, delete the conflict markers

---

## Before You Start Coding

1. Read `docs/architecture.md` — understand how the scripts connect
2. Read `docs/allomancy-design.md` — understand the game systems
3. Check `TODO.md` — see what's in progress
4. Run `bash check.sh` — see all open TODOs

---

## Glossary of Terms

| Term | What It Means |
|------|---------------|
| **Root folder** | The main project folder. The one with `.git` inside. For this project, it's the `Mistborn` folder you cloned. |
| **Subfolder** | A folder inside another folder. `Assets/_Project/Scripts/` is a subfolder of `Assets/`. |
| **Repository (Repo)** | The entire project folder tracked by Git. Contains `.git` and all your files. |
| **Branch** | A separate copy of the project to work on without affecting the main version. Like a save file for your work-in-progress. |
| **Commit** | A snapshot of changes. Like saving your game — Git saves exactly what changed. |
| **Push** | Upload your commits to GitHub (the cloud). |
| **Pull** | Download commits from GitHub to your computer. |
| **Merge** | Combine one branch into another. |
| **Clone** | Download a repository to your computer for the first time. |
| **Fork** | Make your own copy of someone else's repo (not what we use here). |
| **Pull Request (PR)** | Ask to merge your branch into the main project. Lets others review your changes. |
| **Origin** | The default name for the remote repository (GitHub). `origin/master` = the master branch on GitHub. |
| **HEAD** | Your current position. HEAD points to the latest commit on your current branch. |
| **.gitignore** | A file that tells Git which folders/files to ignore (like Library/, Temp/). |
| **Conflict** | When two people edited the same thing. Git can't auto-merge — you have to pick which version to keep. |
| **Stash** | Temporarily save your changes to switch branches. Like pausing a game. |
| **Scene (.unity)** | A Unity file that contains your game level, objects, settings. |
| **Prefab** | A reusable game object template. Make one, use it many times. |
| **Asset** | Anything in the Assets folder — scripts, models, textures, scenes, prefabs. |
| **Script** | C# code file that makes things happen in the game. |
| **Component** | A piece attached to a GameObject in Unity. Scripts become components. |
| **GameObject** | An object in your Unity scene. Everything is a GameObject. |
| **Rigidbody** | A Unity component that makes objects move with physics. |
| **Collider** | A Unity component that defines an object's shape for collision detection. |

## Questions?

Ask the team. Don't sit on confusion — reach out.
