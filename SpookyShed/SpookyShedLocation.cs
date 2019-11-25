using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using SpaceShared;

namespace MoreBuildings.SpookyShed
{
    public class SpookyShedLocation : GameLocation, ISaveElement//, ICustomItem
    {
        public readonly Netcode.NetInt currSpawnerItem = new Netcode.NetInt(0);

        public const int BAT_WING = 767;
        public const int SOLAR_ESSENCE = 768;
        public const int VOID_ESSENCE = 769;
        public const int BUG_MEAT = 684;

        public SpookyShedLocation()
        :   base( "Maps\\SpookyShed", "SpookyShed" )
        {
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(currSpawnerItem);
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            base.drawAboveFrontLayer(b);
            
            Color col = Color.White;
            if (currSpawnerItem == BAT_WING)
                col = Color.Gray;
            else if (currSpawnerItem == SOLAR_ESSENCE)
                col = Color.Yellow;
            else if (currSpawnerItem == VOID_ESSENCE)
                col = Color.Purple;
            else if (currSpawnerItem == BUG_MEAT)
                col = Color.Pink;

            
            var tileLocation = new Vector2(10, 9);
            b.Draw(Mod.instance.spookyGemTex, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), null, col, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);

            foreach (NPC character in this.characters)
            {
                if (character is Monster monster)
                    monster.drawAboveAllLayers(b);
            }
        }

        public override bool checkAction(xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, StardewValley.Farmer who)
        {
            if ( tileLocation.X == 10 && tileLocation.Y == 10 )
            {
                if ( who.CurrentItem != null && who.CurrentItem is StardewValley.Object obj )
                {
                    if (!obj.bigCraftable.Value)
                    {
                        Log.trace("Changing active spawning item to " + obj.ParentSheetIndex);
                        currSpawnerItem.Value = obj.ParentSheetIndex;
                    }
                }
                return true;
            }
            return base.checkAction(tileLocation, viewport, who);
        }

        protected override void resetSharedState()
        {
            base.resetSharedState();
            characters.Clear();

            Log.trace("Player entered spooky shed, current spawner item: " + currSpawnerItem);
            if (currSpawnerItem == BAT_WING)
            {
                var total = 15 + Game1.random.Next(10);
                for (int i = 0; i < total; ++i)
                {
                    var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                    pos = pos * Game1.tileSize;
                    this.characters.Add(new Bat(pos, 100));
                }
            }
            else if ( currSpawnerItem == SOLAR_ESSENCE )
            {
                var total = 15 + Game1.random.Next(10);
                for (int i = 0; i < total; ++i)
                {
                    var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                    pos = pos * Game1.tileSize;
                    this.characters.Add(new SquidKid(pos) { currentLocation = this } );
                }
            }
            else if (currSpawnerItem == VOID_ESSENCE)
            {
                var total = 15 + Game1.random.Next(10);
                for (int i = 0; i < total; ++i)
                {
                    var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                    pos = pos * Game1.tileSize;
                    this.characters.Add(new ShadowBrute(pos));
                }
            }
            else if (currSpawnerItem == BUG_MEAT)
            {
                var total = 15 + Game1.random.Next(10);
                for (int i = 0; i < total; ++i)
                {
                    var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                    pos = pos * Game1.tileSize;
                    this.characters.Add(new Fly(pos, Game1.random.Next(3) == 0));
                }
            }
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var data = new Dictionary<string, string>();
            if (uniqueName.Value != null)
                data.Add("u", uniqueName.Value);

            return data;
        }

        public object getReplacement()
        {
            Shed shed = new Shed("Maps\\SpookyShed", "SpookyShed");
            foreach (Vector2 key in objects.Keys)
                shed.objects.Add(key, objects[key]);
            foreach (Vector2 key in terrainFeatures.Keys)
                shed.terrainFeatures.Add(key, terrainFeatures[key]);

            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Shed shed = (Shed)replacement;

            if (additionalSaveData.ContainsKey("u"))
                uniqueName.Value = additionalSaveData["u"];

            foreach (Vector2 key in shed.objects.Keys)
                objects.Add(key, shed.objects[key]);
            foreach (Vector2 key in terrainFeatures.Keys)
                terrainFeatures.Add(key, shed.terrainFeatures[key]);
        }
    }
}
