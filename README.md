This repository contains my SMAPI mods for Stardew Valley. See the individual mods in the
subfolders for documentation and release notes.

## Translating the mods
The mods can be translated into any language supported by the game, and SMAPI will automatically
use the right translations.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

&nbsp;     | Bug Net                  | Capstone Professions                  | Displays  | Magic                   | More Rings | Preexisting Relationships | Surfing Festival
---------- | :----------------------- | :------------------------------------ | :-------- | :---------------------- | ---------- | :------------------------ | ----------------
Chinese    | ❑ missing                | ❑ missing                             | ❑ missing | [✓](Magic/i18n/zh.json) | ↻ partial  | ❑ missing                 | ❑ missing
French     | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing
German     | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing
Hungarian  | [✓](BugNet/i18n/hu.json) | [✓](CapstoneProfessions/i18n/hu.json) | ❑ missing | ❑ missing               | ✓          | ❑ missing                 | [✓](SurfingFestival/i18n/hu.json)
Italian    | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing
Japanese   | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing
Korean     | ❑ missing                | ❑ missing                             | ❑ missing | [✓](Magic/i18n/ko.json) | ❑ missing  | ❑ missing                 | ❑ missing
Portuguese | ❑ missing                | ❑ missing                             | ❑ missing | [✓](Magic/i18n/pt.json) | ❑ missing  | ❑ missing                 | ❑ missing
Russian    | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing
Spanish    | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing
Turkish    | ❑ missing                | ❑ missing                             | ❑ missing | ❑ missing               | ❑ missing  | ❑ missing                 | ❑ missing


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

### Release order
This order avoids releasing mod updates which need an unreleased update (e.g. updating Json Assets
before a SpaceCore update it needs).

```
Release phases:
      I: non-frameworks with no spacechase dependency
      II: framework mods
      III: mods which use phase II frameworks
      IV: mods which need custom packaging

phase |    mod
----- | ------------------------------
I     | 1.  Better Meteorites
      | 2.  Better Shop Menu
      | 3.  Carry Chest
      | 4.  Combat Level Damage Scaler
      | 5.  Console Code
      | 6.  Custom Critters
      | 7.  Custom NPC Fixes
      | 8.  Experience Bars
      | 9.  Extended Reach
      | 10. Flower Color Picker
      | 11. Flower Rain
      | 12. Jump Over
      | 13. Junimos Accept Cash
      | 14. More Giant Crops
      | 15. More Grass Starters
      | 16. MultiFertilizer
      | 17. Object Time Left
      | 18. Profit Calculator
      | 19. Rush Orders
      | 20. Spenny
      | 21. Three-Heart Dance Partner
      | 22. Throwable Axe
----- | ------------------------------
II    | 23. Content Patcher Animations
      | 24. Generic Mod Config Menu
      | 25. Hybrid Crop Engine
      | 26. SpaceCore
      | 27. Json Assets                 * needs SpaceCore
----- | ------------------------------
III   | 28. Animal Social Menu
      | 29. Another Hunger Mod
      | 30. Bigger Craftables
      | 31. Bug Net
      | 32. Capstone Professions
      | 33. Cooking Skill
      | 34. Customize Exterior
      | 35. Displays
      | 36. Luck Skill
      | 37. Mana Bar
      | 38. Magic                       * needs Mana Bar
      | 39. More Buildings
      | 40. More Rings
      | 41. Preexisting Relationship
      | 42. Pyromancer's Journey
      | 43. Sleepy Eye
      | 44. Statue of Generosity
      | 45. Theft of the Winter Star
----- | ------------------------------
IV    | 46. Surfing Festival
```
