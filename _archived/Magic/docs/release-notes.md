﻿[← back to readme](README.md)

# Release notes
## 0.8.1
Released 12 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Fixed version check for Generic Mod Config Menu not working with some older versions.
* Improved translations. Thanks to Scartiana (added German)!

## 0.8.0
Released 09 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* You can now analyze inventory items when hovering the cursor over them, not only after selecting them in the toolbar.
* Replaced magic TV channel with a radio in the Wizard tower. This fixes the vanilla [fishing channel](https://stardewvalleywiki.com/Television#F.I.B.S.) being unavailable if any custom channels were added through PyTK.
* Added section titles to [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) UI.
* Added min-version check for integration with Generic Mod Config Menu.
* Removed PyTK dependency.
* Fixed default altar position.
* The _Till_ spell no longer removes placed objects.
* Fixed typo in event dialogue.
* Improved translations. Thanks to Evexyron (updated Spanish) and wally232 (updated Korean)!

## 0.7.1
Released 24 December 2021 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.5.
* Improved translations. Thanks to Evexyron (updated Spanish) and Ombrophore (added Russian)!

## 0.7.0
Released 27 November 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added icons for magic school tabs.
* Added tooltips.
* Revamped spell icons (thanks to Ash!).
* You can no longer select a magic school you don't know any spells for.
* The menu now preselects the first known spell so it's more intuitive.
* Various UI tweaks.

## 0.6.2
Released 29 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Fixed error adding TV channel through PyTK.
* Improved translations. Thanks to wally232 (updated Korean)!

## 0.6.1
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Fixed 'no longer compatible' error when playing in Stardew Valley 1.5.4.

## 0.6.0
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* The config UI is now translatable.
* Moved item translations into the standard `i18n` folder.
* Updated for [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) 1.5.0.
* Fixed mana regen not starting until level 2, and slightly increase regen at higher levels.
* Fixed controller not usable in Magic menus.

## 0.5.7
Released 11 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Internal refactoring.

## 0.5.6
Released 04 September 2021 for SMAPI 3.12.6 or later. Updated by Pathoschild.

* Fixed able to level up spells with zero free points.
* Fixed initial spells no longer learned immediately.
* Fixed PyTK still marked as a requirement in the manifest.
* Fixed unable to learn spell from meteors outside the farm.
* Improved translations. Thanks to Evelyon (added Spanish)!

## 0.5.5
Released 01 August 2021 for SMAPI 3.12.0 or later. Updated by Pathoschild.

* Switching spell bar now requires holding the cast key, and no longer rotates your toolbar too (thanks to AWolters-ru!).
* Fixed initial spells not learned if you bypass the learn-magic event.

## 0.5.4
Released 24 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed altar broken when other mods edit Pierre's shop map.
* Fixed players having one spellbar instead of two.

## 0.5.3
Released 18 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed some changes through the altar not persisted when you save and reload.
* Fixed teleport spell opening the menu for all players.
* Fixed error using buff spell in multiplayer (possibly).
* Fixed typos.

## 0.5.2
Released 17 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* PyTK is now optional. If it's not installed, the Magic TV channel won't be added in-game.

## 0.5.1
Released 12 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Added professions to the `magic_summary` console command output.
* Fixed spell selection UI not showing hotbar in some cases.
* Fixed `magic_summary` showing max spell levels one higher than they are.

## 0.5.0
Released 11 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Added `magic_summary` console command to show troubleshooting info.
* Added `help` documentation for console commands.
* Fixed upgrading spells in Magic 0.4.0.
* Fixed error when pressing the swap key when no spell bars are prepared.
* Fixed error with multiplayer projectiles in some cases.
* Fixed typo in Magic Missile spell name.

## 0.4.0
Released 10 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Added API methods to support Skill Prestige.
* Magic sounds are now localized, so only nearby players in multiplayer will hear them.
* Balance changes:
  * You now get the initial 100 mana points immediately after the Wizard event, so you can start using magic on the same day.
  * The _clear debris_ spell no longer clears non-debris objects.
* Reworked how player data is stored to simplify multiplayer sync and reduce edge cases.
* Fixed Wizard event not being skippable.
* Fixed Wizard event broken if another mod changes the location in an incompatible way.
* Fixed Wizard event needing 3.004 hearts instead of 3 (which mainly affected players using CJB Cheats Menu to set the relationship).
* Fixed magic UI rendered before learning magic if another mod added mana points.
* Fixed players sometimes having no mana points despite learning magic.
* Fixed multiplayer issues with _clear debris_ spell.

## 0.3.3
Released 19 June 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).
* Improved documentation.
* Internal refactoring.

## 0.3.2
Released 26 January 2021 for SMAPI 3.9.5 or later.

* Updated for Stardew Valley 1.5.
* The mod now requires [Mana Bar](https://www.nexusmods.com/stardewvalley/mods/7831).
* Fixed heal spell letting you 'overheal' past your max HP (thanks to Elec0!).
* Fixed clear debris level 3 not working outside the farm (thanks to Elec0!).
* Updated translations. Thanks to asqwedcxz741 (added Chinese), Caco-o-sapo (added Portuguese), and yura496 (updated Korean)!

## 0.3.1
Released 26 November 2019 for Stardew Valley 1.4.

* Updated for Stardew Valley 1.4.
* Added support for [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
* Updated translations. Thanks to S2SKY (added Korean)!

## 0.3.0
Released 18 May 2019 for Stardew Valley 1.3.36.

* Added new profession.
* Fixed analyzing being global.
* Fixed upgrade point professions not working.
* Fixed wizard cutscene stopping halfway.
* Fixed internal code.
* Fixed typos.

## 0.2.1
Released 12 May 2019 for Stardew Valley 1.3.36.

* Fixed altar not working.

## 0.2.0
Released 11 May 2019 for Stardew Valley 1.3.36.

* Added a new system for learning spells.
* Added a few new spells.
* Changed leveling to use SpaceCore's skill API.
* The new max level is 10, with professions at levels 5 and 10.

## 0.1.6
Released 29 January 2019 for Stardew Valley 1.3.33.

* Added more localization.
* Made altar locations configurable.
* Fixed frostbolt not having an image.

## 0.1.5
Released 17 January 2019 for Stardew Valley 1.3.33.

* Added 'active effects' system to support non-instant spells like meteor, shockwave, and tendrils (thanks to Pathoschild!).
* Updated for Stardew Valley 1.3.33 and SMAPI 3.0 (thanks to Pathoschild!).

## 0.1.4
Released 15 January 2019 for Stardew Valley 1.3.32.

* Fixed tendrils.

## 0.1.3
Released 18 August 2018 for Stardew Valley 1.3.28.

* Updated for Stardew Valley 1.3.28 and multiplayer.

## 0.1.2
Released 15 April 2018 for Stardew Valley 1.2.

* Migrated to SMAPI's input system.
* Added more config options for controls.
* Switched to SpaceCore experience bars API.
* Removed stardrop knowledge limitation.
* Points are now given every level instead of every other level.
* Fixed prepared spells not loading back into the game.

## 0.1.1
Released 29 March 2018 for Stardew Valley 1.2.

* Fixed weird vanilla thing.
* Fixed conflict with Advanced Location Loader.

## 0.1.0
Released 25 March 2018 for Stardew Valley 1.2.

* Initial release.
