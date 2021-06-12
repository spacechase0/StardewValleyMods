using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using SpaceShared;
using StardewValley;
using StardewValley.Monsters;
using SObject = StardewValley.Object;

namespace MoreBuildings.Buildings.SpookyShed
{
    public class SpookyShedLocation : GameLocation, ISaveElement//, ICustomItem
    {
        public readonly Netcode.NetInt CurrSpawnerItem = new(0);

        public const int BatWing = 767;
        public const int SolarEssence = 768;
        public const int VoidEssence = 769;
        public const int BugMeat = 684;

        public SpookyShedLocation()
            : base("Maps\\SpookyShed", "SpookyShed") { }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddField(this.CurrSpawnerItem);
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            base.drawAboveFrontLayer(b);

            Color col = this.CurrSpawnerItem.Value switch
            {
                SpookyShedLocation.BatWing => Color.Gray,
                SpookyShedLocation.SolarEssence => Color.Yellow,
                SpookyShedLocation.VoidEssence => Color.Purple,
                SpookyShedLocation.BugMeat => Color.Pink,
                _ => Color.White
            };

            var tileLocation = new Vector2(10, 9);
            b.Draw(Mod.Instance.SpookyGemTex, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * Game1.tileSize, tileLocation.Y * Game1.tileSize)), null, col, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
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

        public override bool checkAction(xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (tileLocation.X == 10 && tileLocation.Y == 10)
            {
                if (who.CurrentItem != null && who.CurrentItem is SObject obj)
                {
                    if (!obj.bigCraftable.Value)
                    {
                        Log.Trace("Changing active spawning item to " + obj.ParentSheetIndex);
                        this.CurrSpawnerItem.Value = obj.ParentSheetIndex;
                    }
                }
                return true;
            }
            return base.checkAction(tileLocation, viewport, who);
        }

        protected override void resetSharedState()
        {
            base.resetSharedState();
            this.characters.Clear();

            Log.Trace("Player entered spooky shed, current spawner item: " + this.CurrSpawnerItem.Value);
            int total = 15 + Game1.random.Next(10);

            switch (this.CurrSpawnerItem.Value)
            {
                case SpookyShedLocation.BatWing:
                    for (int i = 0; i < total; ++i)
                    {
                        var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                        pos = pos * Game1.tileSize;
                        this.characters.Add(new Bat(pos, 100));
                    }
                    break;

                case SpookyShedLocation.SolarEssence:
                    for (int i = 0; i < total; ++i)
                    {
                        var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                        pos = pos * Game1.tileSize;
                        this.characters.Add(new SquidKid(pos) { currentLocation = this });
                    }
                    break;

                case SpookyShedLocation.VoidEssence:
                    for (int i = 0; i < total; ++i)
                    {
                        var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                        pos = pos * Game1.tileSize;
                        this.characters.Add(new ShadowBrute(pos));
                    }
                    break;

                case SpookyShedLocation.BugMeat:
                    for (int i = 0; i < total; ++i)
                    {
                        var pos = new Vector2(4 + Game1.random.Next(12), 6 + Game1.random.Next(10));
                        pos = pos * Game1.tileSize;
                        this.characters.Add(new Fly(pos, Game1.random.Next(3) == 0));
                    }
                    break;
            }
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var data = new Dictionary<string, string>();
            if (this.uniqueName.Value != null)
                data.Add("u", this.uniqueName.Value);

            return data;
        }

        public object getReplacement()
        {
            Shed shed = new Shed("Maps\\SpookyShed", "SpookyShed");
            foreach (Vector2 key in this.objects.Keys)
                shed.objects.Add(key, this.objects[key]);
            foreach (Vector2 key in this.terrainFeatures.Keys)
                shed.terrainFeatures.Add(key, this.terrainFeatures[key]);

            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Shed shed = (Shed)replacement;

            if (additionalSaveData.TryGetValue("u", out string savedName))
                this.uniqueName.Value = savedName;

            foreach (Vector2 key in shed.objects.Keys)
                this.objects.Add(key, shed.objects[key]);
            foreach (Vector2 key in this.terrainFeatures.Keys)
                this.terrainFeatures.Add(key, shed.terrainFeatures[key]);
        }
    }
}
