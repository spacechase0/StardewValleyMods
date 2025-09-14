using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MageDelve.Alchemy;
using MageDelve.Mana;
using MageDelve.Mercenaries;
using MageDelve.Skill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace MageDelve
{
    // TODO: Move this stuff to content mod.
    public abstract class ActiveEffect
    {
        public abstract bool Update(GameTime time); // return false on done
        public abstract void Draw(SpriteBatch b);
    }

    internal class ShockwaveActiveEffect : ActiveEffect
    {
        private Farmer player;
        private int damage;

        private float centerX;
        private float centerY;
        private float timer;
        private int currRad;


        public ShockwaveActiveEffect(Farmer player, int damage)
        {
            this.player = player;
            this.damage = damage;
            centerX = player.Position.X;
            centerY = player.Position.Y;
        }

        /// <summary>Update the effect state if needed.</summary>
        /// <param name="e">The update tick event args.</param>
        /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
        public override bool Update(GameTime time)
        {
            if (--this.timer > 0)
            {
                return true;
            }
            this.timer = 10;

            int spotsForCurrRadius = 1 + this.currRad * 7;
            for (int i = 0; i < spotsForCurrRadius; ++i)
            {
                Vector2 pixelPos = new(
                    x: this.centerX + (float)Math.Cos(Math.PI * 2 / spotsForCurrRadius * i) * this.currRad * Game1.tileSize,
                    y: this.centerY + (float)Math.Sin(Math.PI * 2 / spotsForCurrRadius * i) * this.currRad * Game1.tileSize
                );

                this.player.currentLocation.localSound("hoeHit", pixelPos);
                this.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, pixelPos, Color.White, 8, Game1.random.NextDouble() < 0.5, 30));
                this.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(12, pixelPos, Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
            }

            foreach (var character in this.player.currentLocation.characters.ToList())
            {
                if (character is Monster mob)
                {
                    if (Vector2.Distance(new Vector2(this.centerX, this.centerY), mob.position.Value) < this.currRad * Game1.tileSize)
                    {
                        player.currentLocation.damageMonster(new Rectangle(0, 0, 99999, 99999), damage, damage, false, player);
                    }
                }
            }

            ++this.currRad;

            if (this.currRad >= 5)
                return false;

            return true;
        }

        /// <summary>Draw the effect to the screen if needed.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public override void Draw(SpriteBatch spriteBatch) { }
    }

    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.FireProjectile))]
    public class MeleeWeaponTriggerAdornmentEffectPatch
    {
        public static void Postfix(MeleeWeapon __instance, Farmer who)
        {
            // TODO: Content mod unique ID
            if (!__instance.modData.TryGetValue("spacechase0.MageDelve/WeaponAdornment", out string adorn))
                return;
            if (false&&
                Game1.random.NextDouble() > 0.1)
                return;

            switch (adorn)
            {
                case "earth":
                    Mod.effects.Add(new ShockwaveActiveEffect(who, (__instance.minDamage.Value + __instance.maxDamage.Value) / 2 / 2));
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.getExtraSpaceNeededForTooltipSpecialIcons))]
    public class MeleeWeaponAdornmentTooltipSpacePatch
    {
        // TODO: Content mod unique ID
        public static void Postfix(MeleeWeapon __instance, SpriteFont font, ref Point __result)
        {
            if (!__instance.modData.TryGetValue("spacechase0.MageDelve/WeaponAdornment", out string adorn))
                return;

            __result = new(__result.X, __result.Y + (int)font.MeasureString("Meow").Y + 8);
        }
    }

    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.drawTooltip))]
    public class MeleeWeaponAdornmentTooltipPatch
    {
        public static void Postfix(MeleeWeapon __instance, SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font)
        {
            // TODO: Content mod unique ID
            if (!__instance.modData.TryGetValue("spacechase0.MageDelve/WeaponAdornment", out string adorn))
                return;

            //spriteBatch.DrawString(font, I18n.Adornment_Tooltip(I18n.GetByKey("adornment." + adorn + ".name")), new( x, y ), Color.DarkGreen );
            Utility.drawTextWithShadow(spriteBatch, I18n.Adornment_Tooltip(I18n.GetByKey("adornment." + adorn + ".name")), font, new(x + 20, y + 20), Color.DarkGreen);
            y += (int)font.MeasureString("Meow").Y + 8;
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static Configuration Config { get; set; }

        public static ISpaceCoreApi SpaceCore { get; set; }

        public static EventHandler ApisReady;

        public ArcanaSkill skill;

        public static List<ActiveEffect> effects = new();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Config = Helper.ReadConfig<Configuration>();

            skill = new();
            new ManaEngine();
            new AlchemyEngine();
            AlchemyRecipes.Init();
            new MercenaryEngine();

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }


        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            for (int i = effects.Count - 1; i >= 0; --i)
            {
                if (!effects[i].Update(Game1.currentGameTime))
                    effects.RemoveAt(i);
            }
        }
        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            for (int i = effects.Count - 1; i >= 0; --i)
            {
                effects[i].Draw(e.SpriteBatch);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            SpaceCore = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            Skills.RegisterSkill(skill);

            ApisReady.Invoke(null, new());
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    dict.Add("spacechase0.MageDelve_EarthEssence", $"Earth Essence/25/-300/Basic -28/{I18n.Item_EarthEssence_Name()}/{I18n.Item_EarthEssence_Description()}////0/spacechase0.MageDelve\\assets\\essences.png");
                    dict.Add("spacechase0.MageDelve_AirEssence", $"Air Essence/25/-300/Basic -28/{I18n.Item_AirEssence_Name()}/{I18n.Item_AirEssence_Description()}////1/spacechase0.MageDelve\\assets\\essences.png");
                    dict.Add("spacechase0.MageDelve_WaterEssence", $"Water Essence/25/-300/Basic -28/{I18n.Item_WaterEssence_Name()}/{I18n.Item_WaterEssence_Description()}////2/spacechase0.MageDelve\\assets\\essences.png");
                    dict.Add("spacechase0.MageDelve_FireEssence", $"Fire Essence/25/-300/Basic -28/{I18n.Item_FireEssence_Name()}/{I18n.Item_FireEssence_Description()}////3/spacechase0.MageDelve\\assets\\essences.png");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MageDelve/assets/essences.png"))
                e.LoadFromModFile<Texture2D>("assets/essences.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
        }
    }
}
