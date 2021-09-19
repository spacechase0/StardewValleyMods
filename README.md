This repository contains my SMAPI mods for Stardew Valley. See the individual mods in the
subfolders for documentation and release notes.

## Translating the mods
The mods can be translated into any language supported by the game, and SMAPI will automatically
use the right translations.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

&nbsp;     | Bug Net                          | Capstone Professions                  | Displays                   | Magic                   | More Rings | Preexisting Relationships                  | Surfing Festival
---------- | :------------------------------- | :------------------------------------ | :------------------------- | :---------------------- | ---------- | :----------------------------------------- | ----------------
Chinese    | [✓](BugNet/i18n/zh.json)         | [✓](CapstoneProfessions/i18n/zh.json) | [✓](Displays/i18n/zh.json) | [✓](Magic/i18n/zh.json) | ✓          | [✓](PreexistingRelationships/i18n/zh.json) | ✓
French     | ❑ missing                        | ❑ missing                             | ❑ missing                  | ❑ missing               | ❑ missing  | ❑ missing                                  | ❑ missing
German     | ❑ missing                        | ❑ missing                             | [✓](Displays/i18n/de.json) | ❑ missing               | ❑ missing  | ❑ missing                                  | ❑ missing
Hungarian  | [↻ partial](BugNet/i18n/hu.json) | [✓](CapstoneProfessions/i18n/hu.json) | ❑ missing                  | ❑ missing               | ✓          | ❑ missing                                  | ✓
Italian    | ❑ missing                        | ❑ missing                             | ❑ missing                  | ❑ missing               | ❑ missing  | ❑ missing                                  | ❑ missing
Japanese   | ❑ missing                        | ❑ missing                             | ❑ missing                  | ❑ missing               | ❑ missing  | ❑ missing                                  | ❑ missing
Korean     | ❑ missing                        | ❑ missing                             | ❑ missing                  | [✓](Magic/i18n/ko.json) | ❑ missing  | ❑ missing                                  | ✓
Portuguese | ❑ missing                        | ❑ missing                             | ❑ missing                  | [✓](Magic/i18n/pt.json) | ❑ missing  | ❑ missing                                  | ❑ missing
Russian    | ❑ missing                        | ❑ missing                             | ❑ missing                  | ❑ missing               | ❑ missing  | ❑ missing                                  | ✓
Spanish    | [✓](BugNet/i18n/es.json)         | [✓](CapstoneProfessions/i18n/es.json) | [✓](Displays/i18n/es.json) | [✓](Magic/i18n/es.json) | ✓          | [✓](PreexistingRelationships/i18n/es.json) | ✓
Turkish    | ❑ missing                        | ❑ missing                             | ❑ missing                  | ❑ missing               | ❑ missing  | ❑ missing                                  | ❑ missing

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
      | 19. Realtime Minimap
      | 20. Rush Orders
      | 21. Spenny
      | 22. Super Hopper
      | 23. Three-Heart Dance Partner
      | 24. Throwable Axe
----- | ------------------------------
II    | 25. Content Patcher Animations
      | 26. Generic Mod Config Menu
      | 27. Hybrid Crop Engine
      | 28. SpaceCore
      | 29. Dynamic Game Assets         * needs SpaceCore
      | 30. Json Assets                 * needs SpaceCore
----- | ------------------------------
III   | 31. Animal Social Menu
      | 32. Another Hunger Mod
      | 33. Bigger Craftables
      | 34. Bug Net
      | 35. Capstone Professions
      | 36. Cooking Skill
      | 37. Customize Exterior
      | 38. DGAAutomate
      | 39. Displays
      | 40. Luck Skill
      | 41. Mana Bar
      | 42. Magic                       * needs Mana Bar
      | 43. More Buildings
      | 44. More Rings
      | 45. Preexisting Relationship
      | 46. Pyromancer's Journey
      | 47. Sleepy Eye
      | 48. Statue of Generosity
      | 49. Theft of the Winter Star
----- | ------------------------------
IV    | 50. Surfing Festival
```
