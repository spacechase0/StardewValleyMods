using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Schools;
using Magic.Spells;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace Magic
{
    public class SpellBook
    {
        //private static Spell UNKNOWN_SPELL = new DummySpell("unknown");

        private static readonly Dictionary<string, Spell> spells = new();

        public static void register( Spell spell )
        {
            SpellBook.spells.Add(spell.ParentSchool.Id + ":" + spell.Id, spell);
            spell.loadIcon();
        }

        public static Spell get( string id )
        {
            if (string.IsNullOrEmpty(id))
                return null;
            if (!SpellBook.spells.ContainsKey(id))
                return null;// UNKNOWN_SPELL;

            return SpellBook.spells[id];
        }

        public static List< string > getAll()
        {
            return SpellBook.spells.Keys.ToList<string>();
        }

        internal static void init(Func<long> getNewId)
        {
            SpellBook.register(new AnalyzeSpell());
            SpellBook.register(new ProjectileSpell(SchoolId.Arcane, "magicmissle", 5, 7, 15, "flameSpell", "flameSpellHit", seeking: true));
            SpellBook.register(new EnchantSpell(false));
            SpellBook.register(new EnchantSpell(true));
            SpellBook.register(new RewindSpell());

            SpellBook.register(new ClearDebrisSpell());
            SpellBook.register(new TillSpell());
            SpellBook.register(new WaterSpell());
            SpellBook.register(new BlinkSpell());

            SpellBook.register(new LanternSpell(getNewId));
            SpellBook.register(new TendrilsSpell());
            SpellBook.register(new ShockwaveSpell());
            SpellBook.register(new PhotosynthesisSpell());

            SpellBook.register(new HealSpell());
            SpellBook.register(new HasteSpell());
            SpellBook.register(new BuffSpell());
            SpellBook.register(new EvacSpell());

            SpellBook.register(new ProjectileSpell(SchoolId.Elemental, "frostbolt", 7, 10, 20, "flameSpell", "flameSpellHit"));
            SpellBook.register(new ProjectileSpell(SchoolId.Elemental, "fireball", 7, 10, 20, "flameSpell", "flameSpellHit"));
            SpellBook.register(new DescendSpell());
            SpellBook.register(new TeleportSpell());

            SpellBook.register(new MeteorSpell());
            SpellBook.register(new BloodManaSpell());
            SpellBook.register(new LuckStealSpell());
            SpellBook.register(new SpiritSpell());
        }


        [JsonIgnore]
        public Farmer Owner { get; internal set; }

        public Dictionary<string, int> knownSpells = new();
        public PreparedSpell[][] prepared =
        new PreparedSpell[2][]
        {
            new PreparedSpell[5] { null, null, null, null, null },
            new PreparedSpell[5] { null, null, null, null, null },
        };
        public int selectedPrepared;

        public SpellBook()
        {
        }

        public PreparedSpell[] getPreparedSpells()
        {
            if (this.selectedPrepared >= this.prepared.Length)
                return new PreparedSpell[5];
            return this.prepared[this.selectedPrepared];
        }

        public void swapPreparedSet()
        {
            this.selectedPrepared = (this.selectedPrepared + 1) % this.prepared.Length;
            Log.Trace("Swapped prepared spell set to set " + (this.selectedPrepared + 1) + "/" + this.prepared.Length + ".");
        }
    }
}
