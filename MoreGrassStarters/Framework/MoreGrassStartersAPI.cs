using System;

using StardewValley.TerrainFeatures;

using SObject = StardewValley.Object;


namespace MoreGrassStarters.Framework;

#nullable enable
public class MoreGrassStartersAPI : IMoreGrassStartersAPI
{
    public Grass? GetGrass(int which, int numberOfWeeds = 4)
    {
        if (which == Grass.springGrass)
        {
            return new Grass(Grass.springGrass, numberOfWeeds);
        }
        else if (which >= Grass.caveGrass && which < 5 + GrassStarterItem.ExtraGrassTypes)
        {
            return new CustomGrass(which, numberOfWeeds);
        }
        return null;
    }

    public SObject? GetGrassStarter(int which)
    {
        if (which == Grass.springGrass)
        {
            return new SObject(GrassStarterItem.grassID, 1);
        }
        else if (which >= Grass.caveGrass && which < 5 + GrassStarterItem.ExtraGrassTypes)
        {
            return new GrassStarterItem(which);
        }
        return null;
    }

    public Grass? GetMatchingGrass(SObject starter, int numberOfWeeds = 4)
    {
        if (starter is GrassStarterItem grassStarter)
        {
            return new CustomGrass(grassStarter.WhichGrass, numberOfWeeds);
        }
        else if (starter.ParentSheetIndex == GrassStarterItem.grassID)
        {
            return new Grass(Grass.springGrass, numberOfWeeds);
        }
        return null;
    }

    public SObject? GetMatchingGrassStarter(Grass grass)
    {
        if (grass.grassType.Value == Grass.springGrass)
        {
            return new SObject(GrassStarterItem.grassID, 1);
        }
        else if (grass is CustomGrass custom)
        {
            return new GrassStarterItem(custom.grassType.Value);
        }
        return null;
    }
}
