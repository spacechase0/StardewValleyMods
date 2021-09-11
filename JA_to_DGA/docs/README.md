**JA to DGA** is a [Stardew Valley](http://stardewvalley.net/) mod which adds a `dga_convert`
console command to convert a specific Json Assets content pack into a Dynamic Game Assets pack.

## Install
1. Install the latest version of...
   * [SMAPI](https://smapi.io);
   * [Dynamic Game Assets](http://www.nexusmods.com/stardewvalley/mods/9365);
   * and [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720).
3. Install [this mod from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/9365).
4. Run the game using SMAPI.

## Use
**Note:** this is meant to help mod authors migrate their content packs. You should review the
migrated content pack to make sure everything looks correct. Due to differences between Json Assets
and Dynamic Game Assets, the content pack may behave a little differently in some cases.

To convert a content pack:

1. Launch the game.
2. Enter this command in the SMAPI console window, where `JA_PACK_ID` is the unique ID from the
   `manifest.json` of the Json Assets content pack you want to migrate, and `NEW_ID` is the new
   unique ID to set after the migration (which can be the same ID as before):
   ```
   dga_convert JA_PACK_ID NEW_ID
   ```
3. The migrated content pack will be created in a `Mods/[DGA] NEW_ID` folder (where `NEW_ID` is the
   one you set in step 2).

At this point you can exit the game, delete the Json Assets version, and launch again to use the
DGA version of the content pack. Don't forget to review the new content pack to make sure it was
migrated correctly.

## Compatibility
Compatible with Stardew Valley 1.5+ on Linux/macOS/Windows.

## See also
* [Release notes](release-notes.md)
