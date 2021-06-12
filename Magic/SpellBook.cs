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

        private static readonly Dictionary<string, Spell> Spells = new();

        public static void Register(Spell spell)
        {
            SpellBook.Spells.Add(spell.ParentSchool.Id + ":" + spell.Id, spell);
            spell.LoadIcon();
        }

        public static Spell Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            if (!SpellBook.Spells.ContainsKey(id))
                return null;// UNKNOWN_SPELL;

            return SpellBook.Spells[id];
        }

        public static List<string> GetAll()
        {
            return SpellBook.Spells.Keys.ToList<string>();
        }

        internal static void Init(Func<long> getNewId)
        {
            SpellBook.Register(new AnalyzeSpell());
            SpellBook.Register(new ProjectileSpell(SchoolId.Arcane, "magicmissle", 5, 7, 15, "flameSpell", "flameSpellHit", seeking: true));
            SpellBook.Register(new EnchantSpell(false));
            SpellBook.Register(new EnchantSpell(true));
            SpellBook.Register(new RewindSpell());

            SpellBook.Register(new ClearDebrisSpell());
            SpellBook.Register(new TillSpell());
            SpellBook.Register(new WaterSpell());
            SpellBook.Register(new BlinkSpell());

            SpellBook.Register(new LanternSpell(getNewId));
            SpellBook.Register(new TendrilsSpell());
            SpellBook.Register(new ShockwaveSpell());
            SpellBook.Register(new PhotosynthesisSpell());

            SpellBook.Register(new HealSpell());
            SpellBook.Register(new HasteSpell());
            SpellBook.Register(new BuffSpell());
            SpellBook.Register(new EvacSpell());

            SpellBook.Register(new ProjectileSpell(SchoolId.Elemental, "frostbolt", 7, 10, 20, "flameSpell", "flameSpellHit"));
            SpellBook.Register(new ProjectileSpell(SchoolId.Elemental, "fireball", 7, 10, 20, "flameSpell", "flameSpellHit"));
            SpellBook.Register(new DescendSpell());
            SpellBook.Register(new TeleportSpell());

            SpellBook.Register(new MeteorSpell());
            SpellBook.Register(new BloodManaSpell());
            SpellBook.Register(new LuckStealSpell());
            SpellBook.Register(new SpiritSpell());
        }


        [JsonIgnore]
        public Farmer Owner { get; internal set; }

        public Dictionary<string, int> KnownSpells = new();
        public PreparedSpell[][] Prepared =
        new PreparedSpell[2][]
        {
            new PreparedSpell[5] { null, null, null, null, null },
            new PreparedSpell[5] { null, null, null, null, null },
        };
        public int SelectedPrepared;

        public SpellBook()
        {
        }

        public PreparedSpell[] GetPreparedSpells()
        {
            if (this.SelectedPrepared >= this.Prepared.Length)
                return new PreparedSpell[5];
            return this.Prepared[this.SelectedPrepared];
        }

        public void SwapPreparedSet()
        {
            this.SelectedPrepared = (this.SelectedPrepared + 1) % this.Prepared.Length;
            Log.Trace("Swapped prepared spell set to set " + (this.SelectedPrepared + 1) + "/" + this.Prepared.Length + ".");
        }
    }
}
