This repository contains a unified solution with my SMAPI mods for Stardew Valley for convenience.
The actual mod projects are in separate Git repos; this readme explains how to fetch them into the
solution folder.

## Using this repo
### First-time setup
1. Clone this repo.
2. Run this from the solution folder to clone the mod repos:

   ```powershell
   # PowerShell (Windows)
   cat mod-list.txt | where { $_ -match "^[^#]" } | foreach { git clone https://github.com/spacechase0/$_.git; }
   ```

   ```bash
   # Bash (Linux/Mac)
   cat mod-list.txt | grep -e '^[^#]' | while read -r repo; do git clone https://github.com/spacechase0/$repo.git; done
   ```

3. Clone Tiled.Net
4. Open the solution in Visual Studio.
5. Unload the projects in the 'archived' solution folder.

### Update all mod repos
To update all repos to match the server (assuming you have no local changes):

```powershell
# PowerShell (Windows)
cat mod-list.txt | where { $_ -match "^[^#]" } | foreach { pushd $_; git pull; popd; }
```
```bash
# Bash (Linux/Mac)
cat mod-list.txt | grep -e '^[^#]' | while read -r repo; do ( cd $repo; git pull; ) done
```

### Commit changes
Although you can make changes across all repos, each mod still has its own separate repo. To commit
changes, you'll need to open each individual repo folder and commit the changes there.

### Add a mod
The mod list is in `mod-list.txt`; just add new repo names to that file, and it'll be handled by
the commands in this file.
