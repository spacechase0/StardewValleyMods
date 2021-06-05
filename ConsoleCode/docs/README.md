# Console Code
This mod lets you write and execute C# code from the console.

You can write a short snippet using `cs <code>`, such as cs `Game1.activeClickableMenu = null;` or
`cs return Game1.player.money;`.

* Note: Snippets with "quotes" don't work in the console due to the SMAPI command argument parser.

Alternatively, you can load a block from a file, like `cs --script file` and it will load a script
from the mod folder. The script must not include imports, `public class Xyz`, etc. Example script:

```c#
foreach ( var item in Game1.player.items )
{
	if ( item == null )
		continue;

	Game1.player.money += item.salePrice();
	Game1.player.removeItemFromInventory( item );
}
```

## See also
* [Release notes](release-notes.md)
