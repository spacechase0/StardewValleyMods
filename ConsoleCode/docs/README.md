**Console Code** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you write and
execute C# code from the SMAPI console.

## Install
1. Install the latest version of [SMAPI](https://smapi.io).
2. Install [this mod from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/3101).
3. Run the game using SMAPI.

## Use
### Short snippets
You can run code directly in the SMAPI console using the `cs` command. You can separate multiple
expressions with `;`. If the last expression returns a value, it'll be shown in the SMAPI console.

For example:

```
cs Game1.player.Money = 5000; Game1.player.currentLocation.Name
> Output: "FarmHouse"
```

You can't use double quotes (`"`) directly in the console (due to the SMAPI command parser), but
you can replace them with backticks (<code>&#96;</code>).

### Script files
You can load a block from a file in Console Code's folder. The script must not include `using`
statements, class declarations, etc.

For example, let's say you have this `sell-items.cs` file in the mod folder:
```c#
foreach (var item in Game1.player.items)
{
    if (item == null)
        continue;

    Game1.player.money += item.salePrice();
    Game1.player.removeItemFromInventory(item);
}
```

You can execute it like this:
```
cs --script sell-items.cs
```

## Compatibility
Compatible with Stardew Valley 1.5.5+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
