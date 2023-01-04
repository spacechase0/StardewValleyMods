using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MajesticArcana.Elements
{
    internal abstract class Element
    {
        public string Id { get; }
        public virtual string Name => Mod.instance.Helper.Translation.Get($"element.{Id.ToLower()}.name");
        public virtual string Description => Mod.instance.Helper.Translation.Get($"element.{Id.ToLower()}.description");
        public virtual string Manifestation => Mod.instance.Helper.Translation.Get($"element.{Id.ToLower()}.manifestation");
        public virtual string Attribute => Mod.instance.Helper.Translation.Get($"element.{Id.ToLower()}.attribute");

        public virtual Texture2D Tilesheet => Mod.instance.Helper.ModContent.Load<Texture2D>("assets/elements.png");
        public abstract Rectangle TextureRect { get; }
        public abstract Color Color { get; }


        public string[] ParentIds { get; }

        public Element(string id, string[] parentIds = null)
        {
            Id = id;
            ParentIds = parentIds;
        }

        // TODO: Should return a projectile or something?
        public abstract void Manifest(Character caster, Vector2 castDir, float strength/*, CraftedSpell restOfSpell*/);

        // might need more of these...
        public abstract void ApplyAttribute(Character caster, Character target);

        public static Dictionary<string, Element> Elements { get; } = GetDefaultElements();

        private static Dictionary<string, Element> GetDefaultElements()
        {
            Dictionary<string, Element> ret = new();

            ret.Add("earth", new NullElement("earth", new Rectangle(7 * 0, 7 * 0, 7, 7), new Color(255, 224, 181), null));
            ret.Add("fire", new NullElement("fire", new Rectangle(7 * 1, 7 * 0, 7, 7), new Color(255, 102, 0), null));
            ret.Add("dark", new NullElement("dark", new Rectangle(7 * 2, 7 * 0, 7, 7), new Color(139, 33, 170), null));
            ret.Add("light", new NullElement("light", new Rectangle(7 * 3, 7 * 0, 7, 7), new Color(255, 254, 211), null));
            ret.Add("air", new NullElement("air", new Rectangle(7 * 4, 7 * 0, 7, 7), new Color(221, 255, 161), null));
            ret.Add("water", new NullElement("water", new Rectangle(7 * 5, 7 * 0, 7, 7), new Color(161, 252, 255), null));

            ret.Add("void", new NullElement("void", new Rectangle(7 * 0, 7 * 1, 7, 7), new Color(105, 0, 106), new[] { "air", "dark" }));
            ret.Add("solar", new NullElement("solar", new Rectangle(7 * 1, 7 * 1, 7, 7), new Color(255, 186, 0), new[] { "air", "light" }));
            ret.Add("lightning", new NullElement("lightning", new Rectangle(7 * 2, 7 * 1, 7, 7), new Color(252, 255, 0), new[] { "air", "fire" }));
            ret.Add("metal", new NullElement("metal", new Rectangle(7 * 3, 7 * 1, 7, 7), new Color(226, 220, 208), new[] { "earth", "fire" }));
            ret.Add("corrupt", new NullElement("corrupt", new Rectangle(7 * 4, 7 * 1, 7, 7), new Color(41, 11, 58), new[] { "earth", "dark" }));
            ret.Add("life", new NullElement("life", new Rectangle(7 * 5, 7 * 1, 7, 7), new Color(12, 255, 0), new[] { "earth", "water" }));
            ret.Add("healing", new NullElement("healing", new Rectangle(7 * 0, 7 * 2, 7, 7), new Color(255, 0, 0), new[] { "water", "light" }));
            ret.Add("space", new NullElement("space", new Rectangle(7 * 1, 7 * 2, 7, 7), new Color(43, 57, 243), new[] { "water", "dark" }));
            ret.Add("time", new NullElement("time", new Rectangle(7 * 2, 7 * 2, 7, 7), new Color(162, 255, 0), new[] { "light", "fire" }));

            ret.Add("soul", new NullElement("soul", new Rectangle(7 * 3, 7 * 2, 7, 7), new Color(0, 255, 228), new[] { "life", "lightning" }));
            ret.Add("dispell", new NullElement("dispell", new Rectangle(7 * 4, 7 * 2, 7, 7), new Color(13, 127, 160), new[] { "void", "corrupt" }));

            return ret;
        }
    }
}
