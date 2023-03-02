using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Framework.Schools;
using Magic.Framework.Spells;
using SpaceShared;
using StardewValley;

namespace Magic.Framework
{
    /// <summary>Manages the available spells.</summary>
    internal static class SpellManager
    {
        /*********
        ** Fields
        *********/
        private static readonly Dictionary<string, Spell> Spells = new();


        /*********
        ** Public methods
        *********/
        public static Spell Get(string id)
        {
            return !string.IsNullOrEmpty(id) && SpellManager.Spells.TryGetValue(id, out Spell spell)
                ? spell
                : null;
        }

        public static List<string> GetAll()
        {
            return SpellManager.Spells.Keys.ToList<string>();
        }

        internal static void Init(Func<long> getNewId)
        {

            // Register Most Spells here during the Int Phase
            // Spells that can be turned on/off need to be loaded during the save, to prevent errors.
            // As if a save is loaded, back out of, the spell is still loaded in memory and will crash the Spell Menu at Yaba's alter

            SpellManager.Register(new AnalyzeSpell());
            SpellManager.Register(new ProjectileSpell(SchoolId.Arcane, "magicmissle", 5, 7, 15, "flameSpell", "flameSpellHit", seeking: true));
            
            SpellManager.Register(new RewindSpell());

            SpellManager.Register(new ClearDebrisSpell());
            SpellManager.Register(new TillSpell());
            SpellManager.Register(new WaterSpell());

            SpellManager.Register(new LanternSpell(getNewId));
            SpellManager.Register(new TendrilsSpell());
            SpellManager.Register(new ShockwaveSpell());
            SpellManager.Register(new PhotosynthesisSpell());

            SpellManager.Register(new HealSpell());
            SpellManager.Register(new HasteSpell());
            SpellManager.Register(new BuffSpell());
            SpellManager.Register(new EvacSpell());

            SpellManager.Register(new ProjectileSpell(SchoolId.Elemental, "frostbolt", 7, 10, 20, "flameSpell", "flameSpellHit"));
            SpellManager.Register(new ProjectileSpell(SchoolId.Elemental, "fireball", 7, 10, 20, "flameSpell", "flameSpellHit"));
            SpellManager.Register(new DescendSpell());
            SpellManager.Register(new TeleportSpell());

            SpellManager.Register(new MeteorSpell());
            SpellManager.Register(new BloodManaSpell());
            SpellManager.Register(new LuckStealSpell());
            SpellManager.Register(new SpiritSpell());
        }


        /*********
        ** Private methods
        *********/
        public static void Register(Spell spell)
        {
            SpellManager.Spells.Add(spell.ParentSchool.Id + ":" + spell.Id, spell);
            spell.LoadIcon();
        }

        public static void Remove(Spell spell)
        {
            try { 
            SpellManager.Spells.Remove(spell.ParentSchool.Id + ":" + spell.Id);
            } catch
            {
                Log.Error("Tried to remove spell that wasn't registered");
            }
        }

        public static void EnchantmentSpellManager(bool modconfig)
        {
            if (modconfig)
            {
                Log.DebugOnlyLog("Testing, Changed Enchantment Config Fired");
                SpellManager.Remove(new EnchantSpell(false));
                SpellManager.Remove(new EnchantSpell(true));
                SpellManager.Remove(new EnchantCostlySpell(false));
                SpellManager.Remove(new EnchantCostlySpell(true));
                SpellManager.Register(new EnchantCostlySpell(false));
                SpellManager.Register(new EnchantCostlySpell(true));

                Magic.SpellModConfigFix(Game1.player.GetSpellBook(), "arcane:enchant", "arcane:enchant_costly");
                Magic.SpellModConfigFix(Game1.player.GetSpellBook(), "arcane:disenchant", "arcane:disenchant_costly");
            }
            else
            {
                Log.DebugOnlyLog("Testing, Regular Enchantment Config Fired");
                SpellManager.Remove(new EnchantSpell(false));
                SpellManager.Remove(new EnchantSpell(true));
                SpellManager.Remove(new EnchantCostlySpell(false));
                SpellManager.Remove(new EnchantCostlySpell(true));
                SpellManager.Register(new EnchantSpell(false));
                SpellManager.Register(new EnchantSpell(true));
                Magic.SpellModConfigFix(Game1.player.GetSpellBook(), "arcane:enchant_costly", "arcane:enchant");
                Magic.SpellModConfigFix(Game1.player.GetSpellBook(), "arcane:disenchant_costly", "arcane:disenchant");
            }
        }

        public static void BlinkSpellManager(bool modconfig)
        {
            if (modconfig)
            {
                Log.DebugOnlyLog("Testing, Blink Spell Config Fired");
                SpellManager.Remove(new HarvestSpell());
                SpellManager.Remove(new BlinkSpell());
                SpellManager.Register(new BlinkSpell());
                Magic.SpellModConfigFix(Game1.player.GetSpellBook(), "toil:harvest", "toil:blink");
            }
            else
            {
                Log.DebugOnlyLog("Testing, Harvest Spell Config Fired");
                SpellManager.Remove(new HarvestSpell());
                SpellManager.Remove(new BlinkSpell());
                SpellManager.Register(new HarvestSpell());
                Magic.SpellModConfigFix(Game1.player.GetSpellBook(), "toil:blink", "toil:harvest");
            }
        }
    }
}
