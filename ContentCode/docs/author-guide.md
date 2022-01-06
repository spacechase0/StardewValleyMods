﻿[← back to readme](README.md)

# Content Code
This documentation is for making mods; for using Content Code as a user, please check the [Nexus]() page.

(There is also an example pack on the Nexus page.)

Content packs can run code by editing data files with C# code. In the data file, you use your content pack ID as the key (and with some events, a slash and sub key as well; see the table below and example pack)

This document assumes some knowledge of C# and how the game works.

There are some predefined variables you can use in any event:

| Variable | Type | Notes |
| --- | --- | --- |
| ContentPack | [`IContentPack`](https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/IContentPack.cs) | |
| Reflection | [`IReflectionHelper`](https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/IReflectionHelper.cs) (see [here](https://stardewcommunitywiki.com/Modding:Modder_Guide/APIs/Reflection) for more information) | |
| State | `Dictionary<string, object>` | This exists for you to store data in and share between all of your code snippets. Best to initialize in the `SaveLoaded` event. |

Content Code supports a few different points in the game to run code at:

| Data file | Needs subkey | Time it runs | Additional variables |
| --- | --- | --- | --- |
| spacechase0.ContentCode/SaveLoaded | false | When the save is loaded, even in multiplayer. See [here](https://stardewcommunitywiki.com/Modding:Modder_Guide/APIs/Events#GameLoop.SaveLoaded). | none |
| spacechase0.ContentCode/Action | true | When a tile with the map property 'Action' is activated with your action. | `xTile.Dimensions.Location tilePosition`, `string actionString` |
| spacechase0.ContentCode/TouchAction | true | When a tile with the map property 'TouchAction' is walked on with your action. | `string actionString` |

When something needs a sub key, you do it in this format: `pack.id/SubKey`

For actions and tile actions, the map properties needs the full key (your pack id and sub key) to find your code, not just the sub key.

## Example content pack
This doesn't include the map file used in this page. See the Nexus page for the example pack download for that.

```json
{
  "Format": "1.24.0",
  "Changes": [
    {
      "Action": "EditMap",
      "FromFile": "assets/SeedShop_ContentCodeOverlay.tmx",
      "ToArea": { "X": 4, "Y": 22, "Width": 1, "Height": 2 },
      "Target": "Maps/SeedShop"
    },
    {
      "Action": "EditData",
      "Target": "spacechase0.ContentCode/SaveLoaded",
      "Entries": {
        "spacechase0.ContentCode.Example": "
        // Store the time they started playing in the state dictionary.
        State.Add( `timeStartedPlaying`, DateTime.Now.ToString() );
        "
      }
    },
    {
      "Action": "EditData",
      "Target": "spacechase0.ContentCode/Action",
      "Entries": {
        "spacechase0.ContentCode.Example/TestAction": ""
        // Use the state from before and a translation to show to the player.
        Game1.drawObjectDialogue( `{{i18n: time-started}}` + State[ `timeStartedPlaying` ] );
        "
      }
    },
    {
      "Action": "EditData",
      "Target": "spacechase0.ContentCode/TouchAction",
      "Entries": {
        "spacechase0.ContentCode.Example/TestTouchAction": "
        // Jump at a strength given by the action string
        string[] split = actionString.Split( ' ' );
          Game1.player.synchronizedJump( Convert.ToInt32( split[ 1 ] ) );
        "
      }
    },
  ]
}
```
