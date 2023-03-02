using SObject = StardewValley.Object;

using StardewValley.TerrainFeatures;

#nullable enable

namespace MoreGrassStarters;
public interface IMoreGrassStartersAPI
{
    public Grass? GetGrass(int which, int numberOfWeeds = 4);

    public SObject? GetGrassStarter(int which);

    public Grass? GetMatchingGrass(SObject starter, int numberOfWeeds = 4);

    public SObject? GetMatchingGrassStarter(Grass grass);
}
