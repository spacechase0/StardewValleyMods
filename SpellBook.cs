using Newtonsoft.Json;
using Magic.Schools;
using Magic.Spells;
using System.Collections.Generic;
using System.Linq;
using SFarmer = StardewValley.Farmer;

namespace Magic
{
    public class SpellBook
    {
        //private static Spell UNKNOWN_SPELL = new DummySpell("unknown");

        private static Dictionary<string, Spell> spells = new Dictionary<string, Spell>();

        public static void register( Spell spell )
        {
            spells.Add(spell.ParentSchool.Id + ":" + spell.Id, spell);
            spell.loadIcon();
        }

        public static Spell get( string id )
        {
            if (id == null || id == "")
                return null;
            if (!spells.ContainsKey(id))
                return null;// UNKNOWN_SPELL;

            return spells[id];
        }

        public static List< string > getAll()
        {
            return spells.Keys.ToList<string>();
        }

        internal static void init()
        {
            register(new ClearDebrisSpell());
            register(new TillSpell());
            register(new WaterSpell());
            register(new BlinkSpell());

            register(new LanternSpell());
            register(new TendrilsSpell());
            register(new ShockwaveSpell());
            register(new PhotosynthesisSpell());

            register(new HealSpell());
            register(new HasteSpell());
            register(new BuffSpell());
            register(new EvacSpell());

            register(new ProjectileSpell(SchoolId.Elemental, "frostbolt", 1, 4, 10, 20, "flameSpell", "flameSpellHit"));
            register(new ProjectileSpell(SchoolId.Elemental, "fireball", 1, 4, 10, 20, "flameSpell", "flameSpellHit"));
            register(new DescendSpell());
            register(new TeleportSpell());

            register(new MeteorSpell());
            register(new BloodManaSpell());
            register(new LuckStealSpell());
        }


        [JsonIgnore]
        public SFarmer Owner { get; internal set; }

        public Dictionary<string, int> knownSpells = new Dictionary<string, int>();
        public HashSet<string> knownSchools = new HashSet<string>();
        public PreparedSpell[][] prepared =
        new PreparedSpell[2][]
        {
            new PreparedSpell[4] { null, null, null, null },
            new PreparedSpell[4] { null, null, null, null },
        };
        public int selectedPrepared = 0;

        public SpellBook()
        {
        }

        public PreparedSpell[] getPreparedSpells()
        {
            if (selectedPrepared >= prepared.Length)
                return new PreparedSpell[4];
            return prepared[selectedPrepared];
        }

        public void swapPreparedSet()
        {
            selectedPrepared = (selectedPrepared + 1) % prepared.Length;
            Log.trace("Swapped prepared spell set to set " + (selectedPrepared + 1) + "/" + prepared.Length + ".");
        }
    }
}
