using System;
using Newtonsoft.Json;
using Magic.Schools;
using Magic.Spells;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using SpaceShared;

namespace Magic
{
    public class SpellBook
    {
        //private static Spell UNKNOWN_SPELL = new DummySpell("unknown");

        private static readonly Dictionary<string, Spell> spells = new Dictionary<string, Spell>();

        public static void register( Spell spell )
        {
            spells.Add(spell.ParentSchool.Id + ":" + spell.Id, spell);
            spell.loadIcon();
        }

        public static Spell get( string id )
        {
            if (string.IsNullOrEmpty(id))
                return null;
            if (!spells.ContainsKey(id))
                return null;// UNKNOWN_SPELL;

            return spells[id];
        }

        public static List< string > getAll()
        {
            return spells.Keys.ToList<string>();
        }

        internal static void init(Func<long> getNewId)
        {
            register(new AnalyzeSpell());
            register(new ProjectileSpell(SchoolId.Arcane, "magicmissle", 5, 7, 15, "flameSpell", "flameSpellHit", seeking: true));
            register(new EnchantSpell(false));
            register(new EnchantSpell(true));
            register(new RewindSpell());

            register(new ClearDebrisSpell());
            register(new TillSpell());
            register(new WaterSpell());
            register(new BlinkSpell());

            register(new LanternSpell(getNewId));
            register(new TendrilsSpell());
            register(new ShockwaveSpell());
            register(new PhotosynthesisSpell());

            register(new HealSpell());
            register(new HasteSpell());
            register(new BuffSpell());
            register(new EvacSpell());

            register(new ProjectileSpell(SchoolId.Elemental, "frostbolt", 7, 10, 20, "flameSpell", "flameSpellHit"));
            register(new ProjectileSpell(SchoolId.Elemental, "fireball", 7, 10, 20, "flameSpell", "flameSpellHit"));
            register(new DescendSpell());
            register(new TeleportSpell());

            register(new MeteorSpell());
            register(new BloodManaSpell());
            register(new LuckStealSpell());
            register(new SpiritSpell());
        }


        [JsonIgnore]
        public Farmer Owner { get; internal set; }

        public Dictionary<string, int> knownSpells = new Dictionary<string, int>();
        public PreparedSpell[][] prepared =
        new PreparedSpell[2][]
        {
            new PreparedSpell[5] { null, null, null, null, null },
            new PreparedSpell[5] { null, null, null, null, null },
        };
        public int selectedPrepared = 0;

        public SpellBook()
        {
        }

        public PreparedSpell[] getPreparedSpells()
        {
            if (selectedPrepared >= prepared.Length)
                return new PreparedSpell[5];
            return prepared[selectedPrepared];
        }

        public void swapPreparedSet()
        {
            selectedPrepared = (selectedPrepared + 1) % prepared.Length;
            Log.trace("Swapped prepared spell set to set " + (selectedPrepared + 1) + "/" + prepared.Length + ".");
        }
    }
}
