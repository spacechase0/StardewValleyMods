This repository contains a unified solution with my SMAPI mods for Stardew Valley for convenience.
The actual mod projects are in separate Git repos; this readme explains how to fetch them into the
solution folder.

## Using this repo
The commands assume Bash, which you can run in a terminal in Linux/Mac or using
[Git Bash](https://gitforwindows.org/) on Windows.

### First-time setup
1. Clone this repo.
2. Run this from the solution folder to clone the mod repos:

   ```bash
   cat mod-list.txt | grep -e '^\w' | sed -e 's/^[[:space:]]*(.*)[[:space:]]*$/$1/' | while read -r repo; do
      git clone https://github.com/spacechase0/$repo.git
   done
   ```

3. Clone Tiled.Net:

   ```bash
   git clone https://github.com/napen123/Tiled.Net.git
   ```

4. Open the solution in Visual Studio.
5. Unload the projects in the 'archived' solution folder.

### Update all mod repos
To update all repos to match the server (assuming you have no local changes):

```bash
cat mod-list.txt | grep -e '^\w' | sed -e 's/^[[:space:]]*(.*)[[:space:]]*$/$1/' | while read -r repo; do
   (
      cd $repo;
      git pull;
   )
done
```

### Commit changes
Although you can make changes across all repos, each mod still has its own separate repo. To commit
changes, you'll need to open each individual repo folder and commit the changes there.

### Add a mod
The mod list is in `mod-list.txt`; just add new repo names to that file, and it'll be handled by
the commands in this file.
