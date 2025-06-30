using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.AccessControl;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.Internal;

namespace RiverlandsFeatureAddition
{
    public class FishGeodeLocation
    {
        public List<Rectangle> SpawnAreas { get; set; }
    }

    public struct CloneableRect : ICloneable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public CloneableRect() { }
        public CloneableRect(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public object Clone()
        {
            return new CloneableRect(X, Y, Width, Height);
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            // When initially loaded locations aren't loaded, so I *think* the fish geode has empty drops
            Helper.GameContent.InvalidateCache("Data/Objects");
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            var data = Game1.content.Load<Dictionary<string, FishGeodeLocation>>($"{ModManifest.UniqueID}/FishGeodeLocations");
            FishGeodeLocation chosen = null;

            var farm = Game1.getFarm();
            if (Game1.GetFarmTypeID() == Farm.riverlands_layout.ToString())
            {
                chosen = new()
                {
                    SpawnAreas = [
                        new Rectangle( 17, 28,30 - 17 + 1, 36 - 28 + 1),
                        new Rectangle( 61, 36,73 - 61 + 1, 48 - 36 + 1),
                        new Rectangle( 3, 53,27 - 3 + 1, 60 - 53 + 1),
                    ],
                };
            }
            else data.TryGetValue(Game1.GetFarmTypeID(), out chosen);

            if (chosen == null)
                return;

            List<Weighted<CloneableRect>> stuff = new();
            foreach (var area in chosen.SpawnAreas)
                stuff.Add(new(area.Width * area.Height, new(area.X, area.Y, area.Width, area.Height)));

            double chance = 1.0;
            int maxSpawn = 3;
            if (Game1.season == Season.Spring && Game1.dayOfMonth == 1)
            {
                chance = chance / MathF.Pow(0.75f, 25); // Try at least 25 times since the farm will be cluttered
                maxSpawn = 7; // But if we spawn this many stop
            }
            int spawned = 0;
            while ( Game1.random.NextDouble() < chance )
            {
                chance *= 0.75f;
                if (spawned >= maxSpawn)
                    break;

                var r = stuff.Choose(Game1.random);
                Vector2 v = Utility.getRandomPositionInThisRectangle(new Rectangle(r.X, r.Y, r.Width, r.Height), Game1.random);
                if (farm.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Dirt") && farm.CanItemBePlacedHere(v, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
                {
                    string stoneId = $"{ModManifest.UniqueID}_FishGeodeStone";
                    int health = 2;
                    farm.objects.Add(v, new StardewValley.Object(stoneId, 10) // Don't ask why we're using 10 - just mimicking vanilla
                    {
                        MinutesUntilReady = health,
                    });
                    ++spawned;
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if ( e.NameWithoutLocale.IsEquivalentTo( "Data/Objects" ) )
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, ObjectData>().Data;

                    List<ObjectGeodeDropData> drops = new();
                    try
                    {
                        foreach ( var data in Game1.content.Load<Dictionary<string, LocationData>>( "Data/Locations")[ "Forest" ].Fish )
                        {
                            if (data.Precedence < 0)
                                continue;

                            var fish = ItemQueryResolver.TryResolveRandomItem(data, new(Game1.getFarm(), null, Game1.random));
                            if (fish == null)
                                continue;

                            drops.Add(new()
                            {
                                Chance = data.Chance,
                                Condition = data.Condition,
                                ItemId = fish.QualifiedItemId,
                            });
                        }

                        foreach (var data in Game1.content.Load<Dictionary<string, LocationData>>("Data/Locations")["Town"].Fish)
                        {
                            if (data.Precedence < 0)
                                continue;

                            var fish = ItemQueryResolver.TryResolveRandomItem(data, new(Game1.getFarm(), null, Game1.random));
                            if (fish == null)
                                continue;

                            drops.Add(new()
                            {
                                Chance = data.Chance,
                                Condition = data.Condition,
                                ItemId = fish.QualifiedItemId,
                            });
                        }
                    }
                    catch (Exception e)
                    {
                    }

                    foreach ( var d in drops )
                    {
                        d.Chance /= drops.Count;
                    }
                    drops.Add(new ObjectGeodeDropData()
                    {
                        Chance = 1.0,
                        ItemId = "(O)153",
                    });

                    dict.Add($"{ModManifest.UniqueID}_FishGeode", new()
                    {
                        Name = "Fish Geode",
                        DisplayName = I18n.FishGeode_Name(),
                        Description = I18n.FishGeode_Description(),
                        Type = "Basic",
                        Category = 0,
                        Price = 25,
                        Texture = Helper.ModContent.GetInternalAssetName("assets/geode.png").Name,
                        SpriteIndex = 0,
                        GeodeDropsDefaultItems = false,
                        GeodeDrops = drops,
                        ContextTags = [ "color_blue", "geode" ],
                    });
                    dict.Add($"{ModManifest.UniqueID}_FishGeodeStone", new()
                    {
                        Name = "Stone",
                        DisplayName = "Stone",
                        Description = "...",
                        Type = "Litter",
                        Category = -999,
                        Texture = Helper.ModContent.GetInternalAssetName("assets/rock.png").Name,
                        SpriteIndex = 0,
                    });
                });
            }
            else if ( e.NameWithoutLocale.IsEquivalentTo( $"{ModManifest.UniqueID}/FishGeodeLocations" ) )
            {
                e.LoadFrom(() => new Dictionary<string, FishGeodeLocation>(), StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocation),"breakStone")]
    public static class FeodeDropPatch
    {
        public static void Postfix(GameLocation __instance, string stoneId, int x, int y, Farmer who, Random r, ref bool __result)
        {
            if ( stoneId == $"{Mod.instance.ModManifest.UniqueID}_FishGeodeStone" )
            {
                Game1.createObjectDebris($"(O){Mod.instance.ModManifest.UniqueID}_FishGeode", x, y, who?.UniqueMultiplayerID ?? 0, __instance);
                if ( who != null )
                {
                    who.gainExperience(Farmer.fishingSkill, 4);
                    who.gainExperience(Farmer.miningSkill, 4);
                }
                __result = true;
            }
        }
    }
}
