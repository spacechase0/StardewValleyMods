using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace NewGamePlus
{
    public class FloatModel { public float Value { get; set; } }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        internal static Configuration Config;

        internal ClickableTextureComponent plusButton;

        internal Texture2D legacyTokenTex;
        internal Texture2D stableTokenTex;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);
            Config = Helper.ReadConfig<Configuration>();

            plusButton = new ClickableTextureComponent("NewGamePlus", new Rectangle(0, 0, 36, 36), null, null, Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4);

            legacyTokenTex = Helper.ModContent.Load<Texture2D>("assets/LegacyToken.png");
            stableTokenTex = Helper.ModContent.Load<Texture2D>("assets/StableToken.png");

            Helper.ConsoleCommands.Add("legacytoken", "...", (cmd, args) => Game1.player.addItemByMenuIfNecessary(new StardewValley.Object( $"{ModManifest.UniqueID}_LegacyToken", 1 )));

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.GameLoop.SaveCreating += OnSaveCreating;
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.BaseName.Equals("Data/Objects"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, ObjectData>().Data;
                    dict.Add($"{ModManifest.UniqueID}_LegacyToken", new ObjectData()
                    {
                        Name = "Legacy Token",
                        DisplayName = I18n.Item_LegacyToken_Name(),
                        Description = I18n.Item_LegacyToken_Description(),
                        Texture = Helper.ModContent.GetInternalAssetName( "assets/LegacyToken.png" ).Name,
                        ExcludeFromShippingCollection = true,
                    }); dict.Add($"{ModManifest.UniqueID}_StableToken", new ObjectData()
                    {
                        Name = "Stable Token",
                        DisplayName = I18n.Item_StableToken_Name(),
                        Description = I18n.Item_StableToken_Description(),
                        Texture = Helper.ModContent.GetInternalAssetName("assets/StableToken.png").Name,
                        ExcludeFromShippingCollection = true,
                        ContextTags = [ "placeable" ],
                    });
                });
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.Register(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config), true);
                gmcm.AddBoolOption(ModManifest, () => Config.StockList, (x) => Config.StockList = x, () => I18n.Option_StockList_Name(), () => I18n.Option_StockList_Description() );
                gmcm.AddBoolOption(ModManifest, () => Config.GingerIsland, (x) => Config.GingerIsland = x, () => I18n.Option_GingerIsland_Name(), () => I18n.Option_GingerIsland_Description() );
                gmcm.AddNumberOption(ModManifest, () => Config.AdditionalProfitMultiplier, (x) => Config.AdditionalProfitMultiplier = x, () => I18n.Option_AdditionalProfit_Name(), () => I18n.Option_AdditionalProfit_Description(), 0.25f, 1f, 0.05f, (f) => (int)(f * 100) + "%");
                gmcm.AddNumberOption(ModManifest, () => Config.RelationshipPenaltyPercentage, (x) => Config.RelationshipPenaltyPercentage = x, () => I18n.Option_RelationshipPenalty_Name(), () => I18n.Option_RelationshipPenalty_Description(), 0f, 0.95f, 0.05f, (f) => (int)(f * 100) + "%");
                gmcm.AddNumberOption(ModManifest, () => Config.ExpCurveExponent, (x) => Config.ExpCurveExponent = x, () => I18n.Option_ExpCurve_Name(), () => I18n.Option_ExpCurve_Description(), 1f, 1.35f, 0.001f, (f) => ((int)Math.Pow( 15000, f)).ToString() );
                gmcm.AddNumberOption(ModManifest, () => Config.StartingPoints, (x) => Config.StartingPoints = x, () => I18n.Option_StartingPoints_Name(), () => I18n.Option_StartingPoints_Description());
                gmcm.AddNumberOption(ModManifest, () => Config.GoldPerLeftoverPoint, (x) => Config.GoldPerLeftoverPoint = x, () => I18n.Option_GoldPerLeftoverPoint_Name(), () => I18n.Option_GoldPerLeftoverPoint_Description());
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady &&
                 Game1.MasterPlayer.hasOrWillReceiveMail($"{this.ModManifest.UniqueID}/ReceivedLegacyToken") &&
                 !Game1.player.hasOrWillReceiveMail($"{this.ModManifest.UniqueID}/ReceivedLegacyToken") &&
                 Game1.player.isCustomized.Value)
            {
                Game1.addMail($"{this.ModManifest.UniqueID}/ReceivedLegacyToken", true);
                Game1.player.addItemByMenuIfNecessary(new StardewValley.Object($"{ModManifest.UniqueID}_LegacyToken", 1));
            }
        }

        private void OnSaveCreating(object sender, SaveCreatingEventArgs e)
        {
            if (Game1.IsMasterGame && plusButton.sourceRect.X == 236)
            {
                Game1.addMail($"{this.ModManifest.UniqueID}/ReceivedLegacyToken", true);
                Game1.player.addItemByMenuIfNecessary(new StardewValley.Object($"{ModManifest.UniqueID}_LegacyToken", 1));

                if (Config.StockList)
                    Game1.addMailForTomorrow("gotMissingStocklist", true, true);
                if (Config.GingerIsland)
                {
                    Game1.addMailForTomorrow("willyBackRoomInvitation", true, true);
                    Game1.addMailForTomorrow("willyBoatTicketMachine", true, true);
                    Game1.addMailForTomorrow("willyBoatHull", true, true);
                    Game1.addMailForTomorrow("willyBoatAnchor", true, true);
                }
                if (Config.AdditionalProfitMultiplier != 1)
                    Game1.MasterPlayer.difficultyModifier *= Config.AdditionalProfitMultiplier;
                if (Config.RelationshipPenaltyPercentage != 1)
                    Game1.player.modData.Add("NG+/relationshipPenalty", Config.RelationshipPenaltyPercentage.ToString());
                if ( Config.ExpCurveExponent != 1)
                    Game1.player.modData.Add("NG+/expExponent", Config.ExpCurveExponent.ToString());
                Game1.player.modData.Add("NG+/startingPoints", Config.StartingPoints.ToString() );
                Game1.player.modData.Add("NG+/goldPerPoint", Config.GoldPerLeftoverPoint.ToString() );
            }
        }
    }

    [HarmonyPatch(typeof(CharacterCustomization), "ResetComponents")]
    public static class CharacterCustomizationAddButtonPatch
    {
        public static void Postfix(CharacterCustomization __instance)
        {
            if (__instance.source != CharacterCustomization.Source.NewGame && __instance.source != CharacterCustomization.Source.HostNewFarm)
                return;
            Mod.instance.plusButton= new ClickableTextureComponent("NewGamePlus", new Rectangle(0, 0, 36, 36), null, null, Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4); ;
            Mod.instance.plusButton.bounds = new Rectangle(__instance.xPositionOnScreen - 80 + 16, __instance.yPositionOnScreen + __instance.height - 80 - 16 - 45, 36, 36);
        }
    }

    [HarmonyPatch(typeof(CharacterCustomization), "performHoverAction")]
    public static class CharacterCustomizationHoverButtonPatch
    {
        public static void Postfix(CharacterCustomization __instance, int x, int y, ref string ___hoverText, ref string ___hoverTitle )
        {
            if (__instance.source != CharacterCustomization.Source.NewGame && __instance.source != CharacterCustomization.Source.HostNewFarm)
                return;
            if (Mod.instance.plusButton.bounds.Contains(x, y))
            {
                ___hoverTitle = I18n.Create_Option_Name();
                ___hoverText = I18n.Create_Option_Description();
            }
        }
    }

    [HarmonyPatch(typeof(CharacterCustomization), "receiveLeftClick")]
    public static class CharacterCustomizationClickButtonPatch
    {
        public static void Postfix(CharacterCustomization __instance, int x, int y)
        {
            if (__instance.source != CharacterCustomization.Source.NewGame && __instance.source != CharacterCustomization.Source.HostNewFarm)
                return;
            if (Mod.instance.plusButton.bounds.Contains(x, y))
            {
                Game1.playSound("drumkit6");
                if (Mod.instance.plusButton.sourceRect.X == 227)
                    Mod.instance.plusButton.sourceRect.Offset(9, 0);
                else
                    Mod.instance.plusButton.sourceRect.Offset(-9, 0);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterCustomization), "draw")]
    public static class CharacterCustomizationDrawButtonPatch
    {
        public static void Prefix(CharacterCustomization __instance, SpriteBatch b)
        {
            if (__instance.source != CharacterCustomization.Source.NewGame && __instance.source != CharacterCustomization.Source.HostNewFarm)
                return;
            Mod.instance.plusButton.draw(b);
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.checkForLevelGain))]
    public static class FarmerExpCurvePatch
    {
        public static bool Prefix( int oldXP, int newXP, ref int __result )
        {
            float exp = 1;
            if (Game1.MasterPlayer.modData.ContainsKey("NG+/expExponent"))
                exp = float.Parse(Game1.MasterPlayer.modData["NG+/expExponent"]);

            int highestLevel = -1;
            if (oldXP < Math.Pow( 100, exp ) && newXP >= Math.Pow(100, exp))
            {
                highestLevel = 1;
            }
            if (oldXP < Math.Pow(380, exp) && newXP >= Math.Pow(380, exp))
            {
                highestLevel = 2;
            }
            if (oldXP < Math.Pow(770, exp) && newXP >= Math.Pow(770, exp))
            {
                highestLevel = 3;
            }
            if (oldXP < Math.Pow(1300, exp) && newXP >= Math.Pow(1300, exp))
            {
                highestLevel = 4;
            }
            if (oldXP < Math.Pow(2150, exp) && newXP >= Math.Pow(2150, exp))
            {
                highestLevel = 5;
            }
            if (oldXP < Math.Pow(3300, exp) && newXP >= Math.Pow(3300, exp))
            {
                highestLevel = 6;
            }
            if (oldXP < Math.Pow(4800, exp) && newXP >= Math.Pow(4800, exp))
            {
                highestLevel = 7;
            }
            if (oldXP < Math.Pow(6900, exp) && newXP >= Math.Pow(6900, exp))
            {
                highestLevel = 8;
            }
            if (oldXP < Math.Pow(10000, exp) && newXP >= Math.Pow(10000, exp))
            {
                highestLevel = 9;
            }
            if (oldXP < Math.Pow(15000, exp) && newXP >= Math.Pow(15000, exp))
            {
                highestLevel = 10;
            }
            __result = highestLevel;

            return false;
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.changeFriendship))]
    public static class FarmerFriendshipNerfPatch
    {
        public static void Prefix(ref int amount)
        {
            if (Game1.MasterPlayer.modData.ContainsKey("NG+/relationshipPenalty"))
            {
                amount = (int)(amount - Math.Abs(amount) * float.Parse(Game1.MasterPlayer.modData["NG+/relationshipPenalty"]));
            }
        }
    }

    [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
    public static class UtilityCanPlaceStablePatch
    {
        private static bool Impl(GameLocation location, Item item, int x, int y, Farmer f, bool show_error)
        {
            if (Utility.isPlacementForbiddenHere(location))
            {
                return false;
            }
            if (item is null or Tool || Game1.eventUp || f.bathingClothes.Value || f.onBridge.Value)
            {
                return false;
            }
            bool withinRadius = false;
            Vector2 tileLocation = new Vector2(x / 64, y / 64);
            Vector2 playerTile = f.Tile;
            for (int ix = (int)tileLocation.X; ix < (int)tileLocation.X + 4; ++ix)
            {
                for (int iy = (int)tileLocation.Y; iy < (int)tileLocation.Y + 2; ++iy)
                {
                    if (Math.Abs(ix - playerTile.X) <= 4 && Math.Abs(iy - playerTile.Y) <= 4)
                    {
                        withinRadius = true;
                    }
                }
            }

            if (withinRadius)
            {
                if (item.canBePlacedHere(location, tileLocation))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, bool show_error, ref bool __result)
        {
            if (item.ItemId == $"{Mod.instance.ModManifest.UniqueID}_StableToken")
            {
                __result = Impl(location, item, x, y, f, show_error);
                Console.WriteLine("meow:" + __result);
                return false;
            }
            return true;
        }
    }
}
