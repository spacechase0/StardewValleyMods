using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.CustomElementHandler;
using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugNet
{
    public class CritterItem : StardewValley.Object, ISaveElement
    {
        public readonly NetString critterIndex = new NetString();
        public string CritterIndex
        {
            get { return critterIndex.Value; }
            set { critterIndex.Value = value; }
        }

        public CritterItem()
        {
            Name = "Critter Cage";
            Category = monsterLootCategory;
            CanBeSetDown = true;
            CanBeGrabbed = true; // ?
            Edibility = -300;
            IsSpawnedObject = false;
        }

        public CritterItem(string critter)
        :   this()
        {
            CritterIndex = critter;
            DisplayName = Mod.instance.Helper.Translation.Get("critter.cage", new { critterName = Mod.GetCritterName(CritterIndex) });
        }

        public override string getDescription()
        {
            return Mod.GetCritterName(CritterIndex);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddField(critterIndex);
        }

        public object getReplacement()
        {
            return new StardewValley.Object();
        }

        public void rebuild(Dictionary<string, string> saveData, object replacement)
        {
            CritterIndex = saveData["Critter"];
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var data = new Dictionary<string, string>();
            data["Critter"] = CritterIndex;
            return data;
        }

        public override bool canStackWith(ISalable other)
        {
            return (other is CritterItem critterItem && critterItem.CritterIndex == this.CritterIndex);
        }

        public override int salePrice()
        {
            return 100; // TODO
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool flag = (drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            if ((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex) != 590 & drawShadow)
                spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(Mod.GetCritterTexture(CritterIndex), location + new Vector2((float)(int)(32.0 * (double)scaleSize), (int)(32.0 * (double)scaleSize)), Mod.GetCritterRect(CritterIndex), color * transparency, 0, new Vector2(8, 8) * scaleSize, 4 * scaleSize, SpriteEffects.None, layerDepth);
            if (flag)
                Utility.drawTinyDigits((int)((NetFieldBase<int, NetInt>)this.stack), spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString((int)((NetFieldBase<int, NetInt>)this.stack), 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, color);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(Mod.GetCritterTexture(CritterIndex), objectPosition, Mod.GetCritterRect(CritterIndex), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            // Why did I leave this empty
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            x /= Game1.tileSize;
            y /= Game1.tileSize;
            x += 1;
            y += 1;

            Log.trace("Releasing critter: " + CritterIndex);
            var critter = Mod.GetCritterMaker(CritterIndex)(x, y);
            location.addCritter(critter);

            return true;
        }
    }
}
