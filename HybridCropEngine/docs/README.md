# Hybrid Crop Engine
Fully grown adjacent crops can have a chance of having a hybrid crop appear in between them.

Edit the file "Data/HybridCrops" with Content Patcher or IAssetEditor. The key is a crop index in
the spritesheet TileSheets\crops (which can use a JA crop CP token), and the value is an object
containing "BaseCropA" (index of the first crop), "BaseCropB" (index of the second crop) and
"Chance" (a percent chance, 0-1 with 5% being 0.05, of the hybrid appearing each day).

This is a framework, and contains no content on its own. For an example pack, see
[Garsnips](https://spacechase0.com/files/sdvmod/HCE_Garsnips.zip).

![](screenshot.png)

## See also
* [Release notes](release-notes.md)
