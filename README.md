This repository contains my SMAPI mods for Stardew Valley. See the individual mods in the
subfolders for documentation and release notes.

## Translating the mods
The mods can be translated into any language supported by the game, and SMAPI will automatically
use the right translations.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

&nbsp;     | Bug Net                   | Capstone Professions | Displays   | Magic                    | Preexisting Relationships
---------- | :------------------------ | :------------------- | :--------- | :----------------------- | :-------------------------
Chinese    | ❑ missing                | ❑ missing           | ❑ missing | [✓](Magic/i18n/zh.json) | ❑ missing
French     | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
German     | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
Hungarian  | [✓](BugNet/i18n/hu.json) | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
Italian    | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
Japanese   | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
Korean     | ❑ missing                | ❑ missing           | ❑ missing | [✓](Magic/i18n/ko.json) | ❑ missing
Portuguese | ❑ missing                | ❑ missing           | ❑ missing | [✓](Magic/i18n/pt.json) | ❑ missing
Russian    | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
Spanish    | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing
Turkish    | ❑ missing                | ❑ missing           | ❑ missing | ❑ missing               | ❑ missing


Contributions are welcome! See [Modding:Translations](https://stardewvalleywiki.com/Modding:Translations)
on the wiki for help contributing translations.

## Compiling the mods
Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, read on.

These mods use the [crossplatform build config](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
so they can be built on Linux, macOS, and Windows without changes. See [the build config documentation](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
for troubleshooting.

### Compiling a mod for testing
To compile a mod and add it to your game's `Mods` directory:

1. Clone Tiled.Net:

   ```bash
   git clone https://github.com/napen123/Tiled.Net.git
   ```

2. Rebuild the solution in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).  
   <small>This will compile the code and package it into the `Mods` directory.</small>
3. Launch any project with debugging.  
   <small>This will start the game through SMAPI and attach the Visual Studio debugger.</small>

### Compiling a mod for release
To package a mod for release:

1. Switch to `Release` build configuration.
2. Recompile the mods per the previous section.
3. Upload the generated `bin/Release/<mod name>-<version>.zip` file from the project folder.
