using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace NewGamePlus
{
    [XmlType("Mods_spacechase0_NewGamePlus_LegacyToken" )]
    public class LegacyToken : StardewValley.Object
    {
        public LegacyToken()
        : base(74, 1)
        {
        }

        public override string DisplayName { get => I18n.Item_LegacyToken_Name(); set { } }

        public override string getDescription()
        {
            return I18n.Item_LegacyToken_Description();
        }

        public override bool performUseAction(GameLocation location)
        {
            Game1.activeClickableMenu = new LegacyMenu();
            return false;
        }

        public override bool canStackWith(ISalable other)
        {
            return false;
        }

        public override Item getOne()
        {
            var ret = new LegacyToken();
            ret._GetOneFrom(this);
            return ret;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(Mod.instance.legacyTokenTex, location + new Vector2(32), null, Color.White * transparency, 0, new Vector2(8), 4 * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw( Mod.instance.legacyTokenTex, objectPosition, null, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (f.getStandingY() + 3) / 10000f);
        }
    }
}
