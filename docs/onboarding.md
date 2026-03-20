# Getting Started — Unity & Git Setup

## Quick Summary

1. Install Unity 2022 LTS
2. Clone the repo to your computer
3. Open the Mistborn folder in Unity Hub
4. Wait for import
5. Open the test scene
6. Hit Play

---

## Step 1: Install Unity

### What You Need

1. Go to https://unity.com/download
2. Download **Unity Hub** (it's free)
3. Install Unity Hub
4. Open Unity Hub

### Add Unity Version

1. In Unity Hub, click **Installs** (left sidebar)
2. Click **Install Editor** (blue button)
3. Find **Unity 2022 LTS** (long term support = stable)
4. Click **Install** on that version
5. **CHECK THE BOXES** for:
   - Microsoft Visual Studio (you need this to write code)
   - Your platform (Windows Build Support if on Windows, Mac if on Mac)
6. Click **Install**
7. Wait for it to download and install (can take 20+ minutes)

### What Version to Pick

Look for these:
- **2022.3.x** (where x is a number) = Good choice
- **2022.2.x** = Also good
- **2022.1.x** = Fine

Don't pick 2023 or 2024 yet — we want 2022 LTS for stability.

---

## Step 2: Clone the Repo (Download the Project)

### What is "Clone"?

Cloning means downloading the project from GitHub to your computer.

### Option A: Using GitHub Desktop (Easiest)

1. Download GitHub Desktop: https://desktop.github.com/
2. Install and sign in
3. Click **Clone a repository from the Internet**
4. Search for **Mistborn** or **HoleInWater**
5. Find **HoleInWater/Mistborn**
6. Choose where to save it on your computer (pick somewhere you'll remember)
7. Click **Clone**

### Option B: Using Command Line

1. Open a terminal (Git Bash on Windows, Terminal on Mac)
2. Decide where to save the project:
   ```bash
   # Example: Documents folder
   cd ~/Documents
   ```
3. Clone the repo:
   ```bash
   git clone https://github.com/HoleInWater/Mistborn.git
   ```
4. You now have a **Mistborn** folder

### Where Did It Download?

Whatever folder you chose. For example:
- `~/Documents/Mistborn`
- `~/Desktop/Mistborn`
- `C:\Users\YourName\Projects\Mistborn`

**Remember this location!** You'll need it for the next step.

---

## Step 3: Open in Unity Hub

### The Problem

People often open the wrong folder. Here's how to do it right.

### What NOT to Do

❌ Don't double-click a `.unity` scene file  
❌ Don't open a folder inside Mistborn  
❌ Don't open the folder without the `.git` inside

### What TO Do

1. **Open Unity Hub**
2. Click **Projects** (left sidebar)
3. Click **Open** (the big button)
4. **Navigate to where you saved the project**
5. Click on the **Mistborn folder** (the one with the airplane icon or folder icon)
6. Click **Select Folder** (Windows) or **Open** (Mac)

### What You Should See

When you select the right folder, you should see folders like these inside:

```
Mistborn/
├── .git/              ← This folder (hidden but must exist)
├── Assets/             ← Game assets
├── docs/              ← Documentation
├── Packages/          ← Unity packages
└── ProjectSettings/   ← Unity settings
```

**If you see Assets, docs, Packages, and ProjectSettings — you're in the right place.**

### Still Confused?

Look for the `.git` folder. It proves you're in the root folder.

On Windows:
- Open the Mistborn folder
- Click View → Hidden items
- You should see a `.git` folder

On Mac:
- Open Finder in the Mistborn folder
- Press Cmd+Shift+. (dot)
- You should see a `.git` folder

**If you don't see .git — you're in the wrong folder. Go up a level.**

---

## Step 4: Wait for Unity to Import

### What is Importing?

When you first open a Unity project, Unity reads all the files and creates a "Library" of processed data. This makes everything load faster later.

### How Long Does It Take?

| Computer | Time |
|----------|------|
| Fast (gaming PC) | 2-5 minutes |
| Average laptop | 5-15 minutes |
| Slow computer | 15-30 minutes |

### What to Look For

- Progress bar at the bottom of Unity
- "Importing assets" in the status bar
- The project window filling with files

### DO THIS

✅ **Wait** — Don't close Unity  
✅ **Be patient** — It's working  
❌ **Don't close Unity** — You'll have to start over

### If It Seems Stuck

It's probably not stuck. Check the progress bar at the bottom. If it's moving, wait.

---

## Step 5: Open the Test Scene

### What is a Scene?

A scene is like a level. It contains all the objects, settings, and game content.

### How to Find It

1. In Unity, look at the **Project window** (usually bottom-left)
2. You should see folders like Assets, docs, Packages
3. Click on **Assets**
4. Click on **_** (underscore means it's a project folder)
5. Click on **Scenes**
6. Double-click **TestArena**

### What It Should Look Like

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   └── TestArena.unity  ← DOUBLE-CLICK THIS
│   ├── Scripts/
│   └── ...
├── docs/
└── ...
```

---

## Step 6: Hit Play

### Where is the Play Button?

At the top center of Unity, you should see:

```
[▶️ Play]  [⏸ Pause]  [⏭ Step]
```

Click the **Play button** (▶️).

### What Happens

- The scene loads
- You see the test arena
- You can move with WASD
- You can look around with the mouse
- You can push/pull metal objects

### If You Don't See Anything

The scene might need to be set up first. This is normal — the code is ready but someone needs to put the objects in the scene.

Try running the setup:
1. Click **Mistborn** in the top menu
2. Click **Setup Test Arena**
3. Click Play again

---

## Troubleshooting — It Doesn't Work

### Problem: "No Unity version found"

**Fix:** Install Unity through Unity Hub (see Step 1)

---

### Problem: "Can't find the Mistborn folder"

**Fix:** 
1. Open File Explorer (Windows) or Finder (Mac)
2. Navigate to where you saved the project
3. Make sure there's a folder called **Mistborn** with Assets inside
4. If not, clone again

---

### Problem: "This is not a Unity project"

**Cause:** You're trying to open a folder inside Mistborn, not the Mistborn folder itself.

**Fix:**
1. Close Unity
2. Open Unity Hub
3. Click Open
4. Go UP one folder level
5. Select the Mistborn folder
6. Make sure you can see Assets, docs, Packages in the list

---

### Problem: "Scripts are missing" / Red errors

**Cause:** Unity is still importing, or you opened the wrong folder.

**Fix:**
1. Wait for import to finish (check progress bar)
2. Close Unity
3. Open Unity Hub
4. Open the Mistborn ROOT folder again
5. Wait for import to finish
6. If still broken: Assets → Reimport All

---

### Problem: Empty scene / nothing there

**Cause:** The TestArena scene hasn't been set up yet.

**Fix:**
1. In Unity, click **Mistborn** menu at top
2. Click **Setup Test Arena**
3. Click **Generate Basic Arena**
4. Click **Add Metal Coins**
5. Click **Add Anchored Metal**

---

### Problem: Camera is weird / mouse stuck

**Fix:**
1. Press **Escape** to unlock the mouse
2. Click in the Game window to re-lock the mouse

---

### Problem: Git says "not a repository"

**Cause:** You opened a folder inside Mistborn instead of the Mistborn folder itself.

**Fix:**
1. Open terminal/command prompt
2. Navigate to the Mistborn folder:
   ```bash
   cd ~/Documents/Mistborn
   ```
3. Type:
   ```bash
   ls -la
   ```
4. You should see `.git` in the list
5. If not, go up a level:
   ```bash
   cd ..
   ```
6. Check again

---

## Finding the Mistborn Folder — Picture Guide

### Windows

1. Open **File Explorer**
2. Go to where you saved it (Documents, Desktop, etc.)
3. You should see a folder called **Mistborn**
4. **Double-click it**
5. Inside, you should see: Assets, docs, Packages, ProjectSettings
6. If you also see .git (or it's hidden), that's perfect
7. Select this folder in Unity Hub

### Mac

1. Open **Finder**
2. Go to where you saved it (Documents, Desktop, etc.)
3. You should see a folder called **Mistborn**
4. **Double-click it**
5. Inside, you should see: Assets, docs, Packages, ProjectSettings
6. Press **Cmd+Shift+.** to show hidden files
7. You should see **.git** now
8. Select this folder in Unity Hub

---

## Still Stuck?

Ask the team! It's okay to need help.

Try:
- Pinging the team chat
- Screenshots help a lot
- Tell us what step you're on

---

## What Comes Next?

Once you're in Unity and can hit Play:

1. Read `docs/architecture.md` to understand how the code fits together
2. Read `docs/allomancy-design.md` to understand the game systems
3. Check `TODO.md` to see what needs doing
4. Create a branch and start working!

---

## Glossary of Terms

| Term | What It Means |
|------|---------------|
| **Root folder** | The main project folder with `.git` inside. "Mistborn" is the root. |
| **Subfolder** | A folder inside another folder. Like Assets/_Project/ |
| **Repository (Repo)** | The whole project tracked by Git. Contains .git and all files. |
| **Clone** | Download the repo from GitHub to your computer. |
| **Branch** | A separate copy to work on without breaking the main version. |
| **Commit** | A saved snapshot of changes. Like saving a game. |
| **Push** | Upload commits to GitHub. |
| **Pull** | Download commits from GitHub. |
| **Scene (.unity)** | A Unity level file. Contains game objects and settings. |
| **Asset** | Anything in Assets/ — scripts, models, scenes, textures. |
| **GameObject** | An object in your Unity scene. |
| **Component** | A piece attached to a GameObject. Scripts become components. |
