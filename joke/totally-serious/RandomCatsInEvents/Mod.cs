using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;

namespace RandomCatsInEvents
{
    public class Configuration
    {
        public int CatMultiplier { get; set; } = 1;
        public int MeowChance { get; set; } = 750;
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config { get; private set; }

        public static bool animateCats = false;
        private static Dictionary<Pet, int> pets = new();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            Config = Helper.ReadConfig<Configuration>();
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config));
                gmcm.AddNumberOption(ModManifest, () => Config.CatMultiplier, (i) => Config.CatMultiplier = i, I18n.CatMultiplier_Name, I18n.CatMultiplier_Description);
                gmcm.AddNumberOption(ModManifest, () => Config.MeowChance, (i) => Config.MeowChance = i, I18n.MeowChance_Name, I18n.MeowChance_Description);
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (animateCats && Game1.CurrentEvent != null)
            {
                if (pets.Count == 0)
                {
                    foreach (var pet in Game1.CurrentEvent.actors.Where(a => a is Pet))
                    {
                        pets.Add(pet as Pet, 0);
                    }
                }

                foreach (var pet in pets.ToList())
                {
                    if (Game1.random.Next(Config.MeowChance) == 0)
                        pet.Key.playContentSound();

                    pets[pet.Key] = pets[pet.Key] + (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                    
                    if (pet.Key.isMoving())
                    {
                        pet.Key.movementPause = 0;
                        pet.Key.MovePosition(Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
                        if (pet.Value >= 1000 && Game1.random.Next(180) == 0)
                        {
                            pet.Key.Halt();
                            pets[pet.Key] = 0;
                        }
                    }
                    else if (!pet.Key.isMoving() && pet.Value >= 1000 && Game1.random.Next(250) == 0)
                    {
                        switch (Game1.random.Next(4))
                        {
                            case 0: pet.Key.SetMovingUp(true); break;
                            case 1: pet.Key.SetMovingRight(true); break;
                            case 2: pet.Key.SetMovingDown(true); break;
                            case 3: pet.Key.SetMovingLeft(true); break;
                        }
                        pet.Key.Speed = 1 + Game1.random.Next(3);
                        pets[pet.Key] = 0;
                    }
                }
            }
            else pets.Clear();
        }
    }

    [HarmonyPatch(typeof(Event), "setUpCharacters")]
    public static class EventSetupCharactersAddCatsPatch
    {
        public static void Postfix(Event __instance, GameLocation location)
        {
            if (Mod.Config.CatMultiplier < 1)
                return;

            // Exclude adoption events and festivals
            if (__instance.isFestival)
            {
                Mod.animateCats = false;
                return;
            }
            foreach (var actor in __instance.actors)
            {
                if (actor is Pet)
                {
                    Mod.animateCats = false;
                    return;
                }
            }
            Mod.animateCats = true;

            var catData = DataLoader.Pets(Game1.content)["Cat"];
            var catBreeds = catData.Breeds.Select(b => b.Id).ToList();

            int catCounter = 0;
            void SpawnCat( Point tilePos )
            {
                string breed = catBreeds[Game1.random.Next(catBreeds.Count)];

                for (int i = 0; i < 50; ++i)
                {
                    Point actualTile = tilePos + new Point(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3));
                    if (!location.isTilePassable(new xTile.Dimensions.Location(actualTile.X, actualTile.Y), Game1.viewport))
                        continue;
                    if (__instance.actors.Any(a => a.TilePoint == actualTile))
                        continue;

                    Pet cat = new Pet(actualTile.X, actualTile.Y, breed, "Cat");
                    cat.Name = $"RandomCat{catCounter++}";
                    cat.FacingDirection = Game1.random.Next(4);
                    cat.position.X -= 32f;
                    __instance.actors.Add(cat);
                    break;
                }
            }

            List<Point> spots = new();
            spots.Add(__instance.farmer.TilePoint);

            for (int i = 0; i < __instance.actors.Count; ++i)
                spots.Add(__instance.actors[i].TilePoint);
            for (int i = 0; i < __instance.farmerActors.Count; ++i)
                spots.Add( __instance.farmerActors[ i ].TilePoint );

            for (int i = 1; i < Mod.Config.CatMultiplier; ++i)
            {
                spots.AddRange(spots.ToArray());
            }

            foreach (var spot in spots)
                SpawnCat(spot);
        }
    }
}
