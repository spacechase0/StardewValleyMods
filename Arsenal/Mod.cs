using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;
using xTile.Tiles;

namespace Arsenal;

/* TODO
emerald: done
ruby: done
topaz: done
aquarmarine: done
amethyst: done
jade: done
diamond: done

slime: done
void essence: done
solar essence: done
bug meat: done
bat wing: done

alloying: all done

arcane primer: done

reforging: canceled

UI: done
tooltips: done (except outside of arsenal UI)
 */

public static class Extensions
{
    public static string GetExquisiteGemstone(this MeleeWeapon weapon)
    {
        if (weapon.modData.TryGetValue(Mod.DataKey_ExquisiteGemstone, out string id))
            return id;
        return null;
    }
    public static void SetExquisiteGemstone(this MeleeWeapon weapon, string exquisiteGemstoneId)
    {
        if (weapon.modData.ContainsKey(Mod.DataKey_ExquisiteGemstone))
            weapon.modData.Remove(Mod.DataKey_ExquisiteGemstone);
        weapon.modData.Add(Mod.DataKey_ExquisiteGemstone, exquisiteGemstoneId);
    }
    public static string GetBladeCoating(this MeleeWeapon weapon)
    {
        if (weapon.modData.TryGetValue(Mod.DataKey_BladeCoating, out string id))
            return id;
        return null;
    }
    public static void SetBladeCoating(this MeleeWeapon weapon, string coatingId)
    {
        if (weapon.modData.ContainsKey(Mod.DataKey_BladeCoating))
            weapon.modData.Remove(Mod.DataKey_BladeCoating);
        weapon.modData.Add(Mod.DataKey_BladeCoating, coatingId);
    }
    public static string GetBladeAlloying(this MeleeWeapon weapon)
    {
        if (weapon.modData.TryGetValue(Mod.DataKey_BladeAlloying, out string id))
            return id;
        return null;
    }
    public static void SetBladeAlloying(this MeleeWeapon weapon, string alloyId)
    {
        if (weapon.modData.ContainsKey(Mod.DataKey_BladeAlloying))
            weapon.modData.Remove(Mod.DataKey_BladeAlloying);
        weapon.modData.Add(Mod.DataKey_BladeAlloying, alloyId);
    }
}

public class SecondEnchantmentForgeRecipe : CustomForgeRecipe
{
    private class GenericIngredientMatcher : CustomForgeRecipe.IngredientMatcher
    {
        public string QualifiedId { get; }
        public int Quantity { get; }

        public GenericIngredientMatcher(string qualId, int qty)
        {
            QualifiedId = qualId;
            Quantity = qty;
        }

        public override bool HasEnoughFor(Item item)
        {
            return item.QualifiedItemId == QualifiedId && item.Stack >= Quantity;
        }

        public override void Consume(ref Item item)
        {
            if (Quantity >= item.Stack)
                item = null;
            else
                item.Stack -= Quantity;
        }
    }
    private class ToolIngredientMatcher  : CustomForgeRecipe.IngredientMatcher
    {
        public override bool HasEnoughFor(Item item)
        {
            if (item is Tool t)
            {
                var enchs = BaseEnchantment.GetAvailableEnchantmentsForItem(t);
                return enchs.Count > 0;
            }

            return false;
        }

        public override void Consume(ref Item item)
        {
            item = null;
        }
    }

    public override IngredientMatcher BaseItem { get; } = new ToolIngredientMatcher();
    public override IngredientMatcher IngredientItem { get; } = new GenericIngredientMatcher("(O)spacechase0.Arsenal_ArcanePrimer", 1 );
    public override int CinderShardCost { get; } = 30;
    public override Item CreateResult(Item baseItem, Item ingredItem)
    {
        var t = baseItem.getOne() as Tool;

        var newEnchs = BaseEnchantment.GetAvailableEnchantmentsForItem(t);
        if (newEnchs.Count > 0)
        {
            var oldEnchs = t.enchantments.ToList();
            var enchs = t.enchantments.ToList();
            enchs.RemoveAll(e => e.IsSecondaryEnchantment());

            if (enchs.Count != 1)
            {
                t.enchantments.Remove(enchs[1]);
            }

            t.enchantments.Add(Game1.random.ChooseFrom(newEnchs));

            t.previousEnchantments.Clear();
            t.previousEnchantments.AddRange(oldEnchs.Select(e => e.GetName()));
        }

        return t;
    }
}

public class Mod : StardewModdingAPI.Mod
{
    public static Mod instance;

    public const string DataKey_ExquisiteGemstone = "spacechase0.Arsenal/ExquisiteGemstone";
    public const string DataKey_BladeCoating = "spacechase0.Arsenal/BladeCoating";
    public const string DataKey_BladeAlloying = "spacechase0.Arsenal/BladeAlloying";

    public static Texture2D SpriteSheet;

    public static Dictionary<string, string> ExquisiteGemMappings = new()
    {
        { StardewValley.Object.emeraldQID, "(O)spacechase0.Arsenal_ExquisiteEmerald" },
        { StardewValley.Object.rubyQID, "(O)spacechase0.Arsenal_ExquisiteRuby" },
        { StardewValley.Object.topazQID, "(O)spacechase0.Arsenal_ExquisiteTopaz" },
        { StardewValley.Object.aquamarineQID, "(O)spacechase0.Arsenal_ExquisiteAquamarine" },
        { StardewValley.Object.amethystClusterQID, "(O)spacechase0.Arsenal_ExquisiteAmethyst" },
        { StardewValley.Object.sapphireQID /* WHAT? */, "(O)spacechase0.Arsenal_ExquisiteJade" },
        { StardewValley.Object.diamondQID, "(O)spacechase0.Arsenal_ExquisiteDiamond" },
    };

    public static Dictionary<string, int> CoatingIconMapping = new()
    {
        { "(O)766", 13 },
        { "(O)769", 14 },
        { "(O)768", 15 },
        { "(O)684", 16 },
        { "(O)767", 17 },
    };
    public static Dictionary<string, int> AlloyIconMapping = new()
    {
        { "(O)334",  8 },
        { "(O)335",  9 },
        { "(O)336", 10 },
        { "(O)337", 11 },
        { "(O)910", 12 },
    };

    public static Dictionary<string, int> CoatingQuantities = new()
    {
        { "(O)766", 999 },
        { "(O)769", 300 },
        { "(O)768", 300 },
        { "(O)684", 500 },
        { "(O)767", 500 },
    };

    public override void Entry(IModHelper helper)
    {
        Log.Monitor = Monitor;
        instance = this;

        I18n.Init(Helper.Translation);

        SpriteSheet = Helper.ModContent.Load<Texture2D>("assets/objects.png");

        Helper.Events.Content.AssetRequested += ContentOnAssetRequested;
        Helper.Events.Player.Warped += PlayerOnWarped;

        Helper.ConsoleCommands.Add("player_encrustweapon", "...", (cmd, args) => (Game1.player.CurrentTool as MeleeWeapon)?.SetExquisiteGemstone( args[ 0 ] ) );
        Helper.ConsoleCommands.Add("player_coatweapon", "...", (cmd, args) => (Game1.player.CurrentTool as MeleeWeapon)?.SetBladeCoating( args[ 0 ] ) );
        Helper.ConsoleCommands.Add("player_alloyweapon", "...", (cmd, args) => (Game1.player.CurrentTool as MeleeWeapon)?.SetBladeAlloying( args[ 0 ] ) );
        Helper.ConsoleCommands.Add("arsenal_ui", "...", (cmd, args) => Game1.activeClickableMenu = new ArsenalMenu());

        SpaceCore.CustomForgeRecipe.Recipes.Add(new SecondEnchantmentForgeRecipe());

        GameLocation.RegisterTileAction( "OpenArsenalUI", (loc, args, who, Tile) =>
        {
            Game1.activeClickableMenu = new ArsenalMenu();
            return true;
        });

        Harmony harmony = new(ModManifest.UniqueID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void PlayerOnWarped(object sender, WarpedEventArgs e)
    {
        if (e.NewLocation.Name == "Caldera" && !e.Player.craftingRecipes.ContainsKey("spacechase0.Arsenal_ArcanePrimer"))
        {
            e.Player.craftingRecipes.Add("spacechase0.Arsenal_ArcanePrimer", 0);
        }
        else if (e.NewLocation.Name == "Blacksmith")
        {
            e.NewLocation.setTileProperty(12, 13, "Buildings", "Action", "OpenArsenalUI");
            e.NewLocation.setTileProperty(13, 13, "Buildings", "Action", "OpenArsenalUI");
        }
    }

    private void ContentOnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit((asset) =>
            {
                var dict = asset.AsDictionary<string, ObjectData>().Data;

                var test = Helper.ModContent.GetInternalAssetName("assets/objects.png");

                dict.Add( "spacechase0.Arsenal_ExquisiteEmerald", new()
                {
                    Name = "Exquisite Emerald",
                    DisplayName = I18n.Object_ExquisiteEmerald_Name(),
                    Description = I18n.Object_ExquisiteEmerald_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 2500,
                     Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 0,
                });
                dict.Add( "spacechase0.Arsenal_ExquisiteRuby", new()
                {
                    Name = "Exquisite Ruby",
                    DisplayName = I18n.Object_ExquisiteRuby_Name(),
                    Description = I18n.Object_ExquisiteRuby_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 2500,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 2,
                });
                dict.Add( "spacechase0.Arsenal_ExquisiteTopaz", new()
                {
                    Name = "Exquisite Topaz",
                    DisplayName = I18n.Object_ExquisiteTopaz_Name(),
                    Description = I18n.Object_ExquisiteTopaz_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 800,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 1,
                });
                dict.Add( "spacechase0.Arsenal_ExquisiteJade", new()
                {
                    Name = "Exquisite Jade",
                    DisplayName = I18n.Object_ExquisiteJade_Name(),
                    Description = I18n.Object_ExquisiteJade_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 2000,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 3,
                });
                dict.Add( "spacechase0.Arsenal_ExquisiteAquamarine", new()
                {
                    Name = "Exquisite Aquamarine",
                    DisplayName = I18n.Object_ExquisiteAquamarine_Name(),
                    Description = I18n.Object_ExquisiteAquamarine_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 1800,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 6,
                });
                dict.Add( "spacechase0.Arsenal_ExquisiteAmethyst", new()
                {
                    Name = "Exquisite Amethyst",
                    DisplayName = I18n.Object_ExquisiteAmethyst_Name(),
                    Description = I18n.Object_ExquisiteAmethyst_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 1000,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 4,
                });
                dict.Add( "spacechase0.Arsenal_ExquisiteDiamond", new()
                {
                    Name = "Exquisite Diamond",
                    DisplayName = I18n.Object_ExquisiteDiamond_Name(),
                    Description = I18n.Object_ExquisiteDiamond_Description(),
                    Type = "Minerals",
                    Category = StardewValley.Object.GemCategory,
                    ContextTags = new() { "exquisite_gem" },
                    Price = 7500,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 5,
                });
                dict.Add( "spacechase0.Arsenal_ArcanePrimer", new()
                {
                    Name = "Arcane Primer",
                    DisplayName = I18n.Object_ArcanePrimer_Name(),
                    Description = I18n.Object_ArcanePrimer_Description(),
                    Type = "Crafting",
                    Category = StardewValley.Object.CraftingCategory,
                    Price = 10000,
                    Texture = Helper.ModContent.GetInternalAssetName( "assets/objects.png" ).Name,
                    SpriteIndex = 7,
                    ExcludeFromShippingCollection = true,
                });
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
        {
            e.Edit((asset) =>
            {
                var dict = asset.AsDictionary<string, string>().Data;
                dict.Add( "spacechase0.Arsenal_ArcanePrimer", $"797 1 896 1/meow/spacechase0.Arsenal_ArcanePrimer 1/false/null/{I18n.Object_ArcanePrimer_Name()}/{I18n.Object_ArcanePrimer_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteEmerald", $"{StardewValley.Object.emeraldID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteEmerald 1/false/Mining 10/{I18n.Object_ExquisiteEmerald_Name()}/{I18n.Object_ExquisiteEmerald_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteRuby", $"{StardewValley.Object.rubyID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteRuby 1/false/Mining 10/{I18n.Object_ExquisiteRuby_Name()}/{I18n.Object_ExquisiteRuby_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteTopaz", $"{StardewValley.Object.topazID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteTopaz 1/false/Mining 10/{I18n.Object_ExquisiteTopaz_Name()}/{I18n.Object_ExquisiteTopaz_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteAquamarine", $"{StardewValley.Object.aquamarineID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteAquamarine 1/false/Mining 10/{I18n.Object_ExquisiteAquamarine_Name()}/{I18n.Object_ExquisiteAquamarine_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteAmethyst", $"{StardewValley.Object.amethystClusterID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteAmethyst 1/false/Mining 10/{I18n.Object_ExquisiteAmethyst_Name()}/{I18n.Object_ExquisiteAmethyst_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteJade", $"{StardewValley.Object.sapphireID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteJade 1/false/Mining 10/{I18n.Object_ExquisiteJade_Name()}/{I18n.Object_ExquisiteJade_Description()}");
                dict.Add( "spacechase0.Arsenal_ExquisiteDiamond", $"{StardewValley.Object.diamondID} 50 74 1/meow/spacechase0.Arsenal_ExquisiteDiamond 1/false/Mining 10/{I18n.Object_ExquisiteDiamond_Name()}/{I18n.Object_ExquisiteDiamond_Description()}");
            });
        }
    }
}

[HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.getExtraSpaceNeededForTooltipSpecialIcons))]
public static class MeleeWeaponTooltipPatch1
{
    public static void Postfix(MeleeWeapon __instance, SpriteFont font, ref Point __result)
    {
        if (__instance.GetExquisiteGemstone() != null)
            __result.Y += Math.Max((int)font.MeasureString("TT").Y, 48);;
        if (__instance.GetBladeCoating() != null)
            __result.Y += Math.Max((int)font.MeasureString("TT").Y, 48);;
        if (__instance.GetBladeAlloying() != null)
            __result.Y += Math.Max((int)font.MeasureString("TT").Y, 48);
    }
}

[HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.drawTooltip))]
public static class MeleeWeaponTooltipPatch2
{
    public static void Postfix(MeleeWeapon __instance, SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font)
    {
        string gem = __instance.GetExquisiteGemstone();
        if (gem != null)
        {
            var data = ItemRegistry.GetData(gem);
            Utility.drawWithShadow(spriteBatch, data.GetTexture(), new Vector2(x + 16, y + 16), data.GetSourceRect(), Color.White, 0, Vector2.Zero, 3f, layerDepth: 1);
            Utility.drawTextWithShadow(spriteBatch, I18n.GetByKey( $"tooltip.gem.{gem}" ), font, new Vector2( x + 16 + 44, y + 16 + 12), Game1.textColor );
            y += Math.Max((int)font.MeasureString("TT").Y, 48);
        }

        string coating = __instance.GetBladeCoating();
        if (coating != null)
        {
            Utility.drawWithShadow(spriteBatch, Mod.SpriteSheet, new Vector2(x + 4, y + 8), Game1.getSourceRectForStandardTileSheet(Mod.SpriteSheet, Mod.CoatingIconMapping[coating], 16, 16 ), Color.White, 0, Vector2.Zero, 4f, layerDepth: 1);
            Utility.drawTextWithShadow(spriteBatch, I18n.GetByKey( $"tooltip.coating.{coating}" ), font, new Vector2( x + 16 + 44, y + 16 + 12), Game1.textColor );
            y += Math.Max((int)font.MeasureString("TT").Y, 48);
        }

        string alloy = __instance.GetBladeAlloying();
        if (alloy != null)
        {
            Utility.drawWithShadow(spriteBatch, Mod.SpriteSheet, new Vector2(x + 4, y + 8), Game1.getSourceRectForStandardTileSheet(Mod.SpriteSheet, Mod.AlloyIconMapping[alloy], 16, 16 ), Color.White, 0, Vector2.Zero, 4f, layerDepth: 1);
            Utility.drawTextWithShadow(spriteBatch, I18n.GetByKey( $"tooltip.alloying.{alloy}" ), font, new Vector2( x + 16 + 44, y + 16 + 12), Game1.textColor );
            y += Math.Max((int)font.MeasureString("TT").Y, 48);
        }
    }
}

[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] {typeof( Microsoft.Xna.Framework.Rectangle), typeof( int ), typeof( int ), typeof( bool ), typeof( float ), typeof( int ), typeof( float ), typeof( float ), typeof( bool ), typeof( Farmer ), typeof( bool ) })]
public static class GameLocationDamageMonsterFlagsPatch
{
    internal static int IsSwinging = 0;
    internal static bool hasHealedYet = true;

    public static void Prefix()
    {
        if (IsSwinging == 0)
            hasHealedYet = false;
        ++IsSwinging;
    }

    public static void Postfix()
    {
        --IsSwinging;
        if (IsSwinging == 0)
            hasHealedYet = true;
    }
}

[HarmonyPatch(typeof(GameLocation),"isMonsterDamageApplicable")]
public static class GameLocationBatWingDamagePatch
{
    public static void Postfix(Farmer who, Monster monster, ref bool __result)
    {
        if (who.CurrentTool is MeleeWeapon mw && mw.GetBladeCoating() == "(O)767")
            __result = true;
    }
}

[HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.DoDamage))]
public static class MeleeWeaponVoidEssenceCoatingPatch
{
    public static void Postfix(MeleeWeapon __instance, GameLocation location, int x, int y, int facingDirection, int power,
        Farmer who)
    {
        if (__instance.GetBladeCoating() != "(O)769")
            return;

        List<int> facings = new() { 0, 1, 2, 3 };
        facings.Remove(facingDirection);
        foreach (int newFacing in facings)
        {
            Vector2 z1 = Vector2.Zero, z2 = Vector2.Zero;
            Rectangle area = __instance.getAreaOfEffect(x, y, newFacing, ref z1, ref z2, who.GetBoundingBox(),
                who.FarmerSprite.currentAnimationIndex);
            location.damageMonster(area, (int) ((double) (int) __instance.minDamage * (1.0 + (double) who.buffs.AttackMultiplier)), (int) ((double) (int) __instance.maxDamage * (1.0 + (double) who.buffs.AttackMultiplier)), false, __instance.knockback.Value * (1f + who.buffs.KnockbackMultiplier), (int) ((double) (int) __instance.addedPrecision * (1.0 + (double) who.buffs.WeaponPrecisionMultiplier)), __instance.critChance.Value * (1f + who.buffs.CriticalChanceMultiplier), __instance.critMultiplier.Value * (1f + who.buffs.CriticalPowerMultiplier), (int) __instance.type != 1 || !__instance.isOnSpecial, who);
            /*location.projectiles.RemoveWhere((Func<Projectile, bool>) (projectile =>
               {
               if (areaOfEffect.Intersects(projectile.getBoundingBox()) && !projectile.ignoreMeleeAttacks.Value)
               projectile.behaviorOnCollisionWithOther(location);
               return projectile.destroyMe;
               }));
             */
        }
    }
}

[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof(Rectangle), typeof(int ), typeof(int ), typeof(bool ), typeof(float ), typeof(int ), typeof(float ), typeof(float ), typeof(bool), typeof(Farmer), typeof(bool)})]
public static class GameLocationDamageMonsterWorkaroundPatch
{
    public static int takeDamageInlineWorkaround(Monster monster, int damage, int xTrajectory, int yTrajectory,
        bool isBomb, double addedPrecision, Farmer who)
    {
        MonsterTakeDamagePatch.Prefix(monster, ref damage, who);
        int ret = monster.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        MonsterTakeDamagePatch.Postfix(monster, damage, who, ret);
        return ret;
    }

    public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
    {
        // Writing a transpiler without a CIL viewer! Fun!
        // Pretty sure there's a utility method for this but I can't find it

        List<CodeInstruction> ret = new();

        foreach (var insn in insns)
        {
            if ( insn.Calls( typeof(Monster).GetMethod(nameof(Monster.takeDamage), new Type[] {typeof(int), typeof(int),typeof(int),typeof(bool), typeof(double),typeof(Farmer) } ) ) )
            {
                insn.opcode = OpCodes.Call;
                insn.operand = AccessTools.Method( typeof( GameLocationDamageMonsterWorkaroundPatch ), nameof( takeDamageInlineWorkaround ) );
            }

            ret.Add(insn);
        }

        return ret;
    }
}

//[HarmonyPatch(typeof(Monster), nameof(Monster.takeDamage),new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) })]
public static class MonsterTakeDamagePatch
{
    public static void Prefix(Monster __instance, ref int damage, Farmer who)
    {
        if (!(who.CurrentTool is MeleeWeapon mw))
            return;

        float mult = 1;
        switch (mw.GetBladeAlloying())
        {
            case "(O)334": mult = 1.05f; break;
            case "(O)335": mult = 1.10f; break;
            case "(O)336": mult = 1.15f; break;
            case "(O)337": mult = 1.20f; break;
            case "(O)910": mult = 1.25f; break;
        }

        switch (mw.GetExquisiteGemstone())
        {
            case "(O)ExquisiteAquamarine":
                if (Game1.random.NextDouble() < 0.15)
                    mult *= mw.critMultiplier.Value * (1 + who.buffs.CriticalPowerMultiplier);
                break;
        }

        damage = (int)(damage * mult);
    }
    public static void Postfix(Monster __instance, int damage, Farmer who, int __result)
    {
        Console.WriteLine("meow! " + __result + " " + __instance.Health);
        if (__result <= 0 || !(who.CurrentTool is MeleeWeapon mw))
            return;

        switch (mw.GetExquisiteGemstone())
        {
            case "(O)spacechase0.Arsenal_ExquisiteEmerald":
                who.buffs.Apply(new Buff("exquisiteemerald", duration: 5000,
                    effects: new BuffEffects() { Speed = { 1.5f } }));
                break;

            case "(O)spacechase0.Arsenal_ExquisiteRuby":
                DelayedAction.functionAfterDelay(() => { if ( __instance.health.Value > 0 ) __instance.takeDamage((int)(damage * 0.1f), 0, 0, false, 0, "hitEnemy" ); }, 1000);
                DelayedAction.functionAfterDelay(() => { if ( __instance.health.Value > 0 ) __instance.takeDamage((int)(damage * 0.1f), 0, 0, false, 0, "hitEnemy" ); }, 2000);
                DelayedAction.functionAfterDelay(() => { if ( __instance.health.Value > 0 ) __instance.takeDamage((int)(damage * 0.1f), 0, 0, false, 0, "hitEnemy" ); }, 3000);
                break;

            case "(O)spacechase0.Arsenal_ExquisiteJade":
                if (!GameLocationDamageMonsterFlagsPatch.hasHealedYet)
                {
                    GameLocationDamageMonsterFlagsPatch.hasHealedYet = true;
                    who.Stamina += 2;
                }
                break;

            case "(O)spacechase0.Arsenal_ExquisiteAmethyst":
                if ( Game1.random.NextDouble() < 0.15 )
                    __instance.stunTime.Value = 3000;
                break;

            case "(O)spacechase0.Arsenal_ExquisiteDiamond":
                if (!GameLocationDamageMonsterFlagsPatch.hasHealedYet)
                {
                    GameLocationDamageMonsterFlagsPatch.hasHealedYet = true;
                    who.health = Math.Min(who.health + 1, who.maxHealth);
                }
                break;
        }

        switch (mw.GetBladeCoating())
        {
            case "(O)766": // Slime
                if (!__instance.modData.ContainsKey("spacechase0.Arsenal_Slimed"))
                {
                    __instance.modData.Add("spacechase0.Arsenal_Slimed", "meow");
                    __instance.addedSpeed = -1.5f;
                }
                break;
            case "(O)768": // solar essence
                if (__instance.health.Value <= 0)
                {
                    __instance.currentLocation.explode(__instance.Tile, 2, who, false, 25);
                }
                break;
        }
    }
}

[HarmonyPatch(typeof(GameLocation), "onMonsterKilled")]
public static class GameLocationBugMeatCoatingMoreLootPatch
{
    public static void Prefix(GameLocation __instance, Farmer who, Monster monster)
    {
        if (who.CurrentTool is MeleeWeapon mw &&
            mw.GetBladeCoating() == "(O)684")
        {
            __instance.monsterDrop(monster, monster.GetBoundingBox().Center.X, monster.GetBoundingBox().Center.Y, who);
        }
    }
}

[HarmonyPatch(typeof(Farmer), nameof(Farmer.takeDamage))]
public static class FarmerLessDamageForExquisiteTopazPatch
{
    public static void Prefix(Farmer __instance, ref int damage)
    {
        if (__instance.CurrentTool is MeleeWeapon mw &&
            mw.GetExquisiteGemstone() == "(O)spacechase0.Arsenal_ExquisiteTopaz")
        {
            damage = Math.Max(1, (int)(damage * 0.7));
        }
    }
}

[HarmonyPatch(typeof(GameLocation), "breakStone")]
public static class GameLocationBreakingStoneFlagPatch
{
    internal static int IsBreakingStone = 0;

    public static void Prefix()
    {
        IsBreakingStone++;
    }

    public static void Postfix()
    {
        IsBreakingStone--;
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.createObjectDebris), new Type[] {typeof(string), typeof(int),typeof(int),typeof(long),typeof(GameLocation)})]
public static class Game1ChangeGemToExquisitePatch
{
    public static void Prefix(ref string id, int xTile, int yTile, long whichPlayer, GameLocation location)
    {
        if (GameLocationBreakingStoneFlagPatch.IsBreakingStone > 0 &&
            Mod.ExquisiteGemMappings.TryGetValue(id, out string newId) &&
            Game1.random.NextDouble() < 0.02)
            id = newId;
    }
}

[HarmonyPatch(typeof(MeleeWeapon), "GetOneCopyFrom")]
public static class MeleeWeaponCopyDataInGetOneFromPatch
{

    public static void Postfix(MeleeWeapon __instance, Item source)
    {
        var mw = source as MeleeWeapon;
        if (mw == null) return;

        if (mw.GetExquisiteGemstone() != null) __instance.SetExquisiteGemstone(mw.GetExquisiteGemstone());
        if (mw.GetBladeCoating() != null) __instance.SetBladeCoating(mw.GetBladeCoating());
        if (mw.GetBladeAlloying() != null) __instance.SetBladeAlloying(mw.GetBladeAlloying());
    }
}

// Allow innate enchantments on everything
[HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.CanForge))]
public static class MeleeWeaponAllowForgingDragontoothOnAllWeaponsPatch
{
    public static void Postfix(MeleeWeapon __instance, Item item, ref bool __result)
    {
        if (item.QualifiedItemId == "(O)852")
            __result = true;
    }
}
