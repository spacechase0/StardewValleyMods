using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Framework.Schools;
using Magic.Framework.Spells;

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
            SpellManager.Register(new AnalyzeSpell());
            SpellManager.Register(new ProjectileSpell(SchoolId.Arcane, "magicmissle", 5, 7, 15, "flameSpell", "flameSpellHit", seeking: true));
            SpellManager.Register(new EnchantSpell(false));
            SpellManager.Register(new EnchantSpell(true));
            SpellManager.Register(new RewindSpell());

            SpellManager.Register(new ClearDebrisSpell());
            SpellManager.Register(new TillSpell());
            SpellManager.Register(new WaterSpell());
            SpellManager.Register(new BlinkSpell());

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
    }
}
