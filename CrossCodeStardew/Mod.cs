using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewValley;

namespace CrossCodeStardew
{
    public enum Element
    {
        Heat,
        Cold,
        Shock,
        Wave,
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.Specialized.LoadStageChanged += this.Specialized_LoadStageChanged;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(EmpowerLocation));
        }

        private void Specialized_LoadStageChanged(object sender, StardewModdingAPI.Events.LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations)
            {
                Game1.locations.Add(new EmpowerLocation(Helper.Content, Element.Heat));
            }
        }
    }

    [XmlType("Mods_spacechase0_CCStardew_EmpowerLocation")]
    public class EmpowerLocation : GameLocation
    {
        public enum AnimStage
        {
            PillarRise,
            PillarFall,
            RingMove,
            Circles,
            PlayerFall,
            Done
        };

        public float[] animLengths = new float[ (int) AnimStage.Done]
        {
            4,
            4,
            2,
            3,
            4,
        };

        public readonly NetPoint CurrentEmpoweringPoint = new();
        public readonly NetEnum<Element> CurrentEmpoweringElement = new();
        public readonly NetLong CurrentEmpoweringPlayer = new();
        public readonly NetEnum<AnimStage> CurrentEmpoweringAnim = new(AnimStage.Done);
        private float stageTimer = 0;

        private Texture2D empowerTex;

        public EmpowerLocation() { }

        public EmpowerLocation(IContentHelper helper, Element element)
        :   base( helper.GetActualAssetKey( $"assets/Empower{element}.tmx" ), $"Custom_CCStardew_Empower{element}" )
        {
            empowerTex = helper.Load<Texture2D>($"assets/Empower{element}.png");
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(CurrentEmpoweringPoint,
                                CurrentEmpoweringElement,
                                CurrentEmpoweringPlayer,
                                CurrentEmpoweringAnim);
            CurrentEmpoweringAnim.fieldChangeEvent += AnimStageChanged;
        }

        private void AnimStageChanged(NetEnum<AnimStage> stage, AnimStage oldStage, AnimStage newStage)
        {
            if (CurrentEmpoweringPlayer.Value == 0) return;
            Farmer master = Game1.getFarmer(CurrentEmpoweringPlayer.Value);

            stageTimer = 0;
            switch (newStage)
            {
                case AnimStage.PillarRise:
                    master.doEmote(Character.exclamationEmote);
                    master.CanMove = false;
                    master.facingDirection.Value = 2;

                    playSound("thunder_small");
                    break;
                case AnimStage.PillarFall:
                    playSound("thunder_small");
                    break;
                case AnimStage.RingMove:
                    break;
                case AnimStage.Circles:
                    break;
                case AnimStage.PlayerFall:
                    Game1.playSound("stardrop");
                    break;
                case AnimStage.Done:
                    if (master == Game1.player)
                    {
                        Game1.drawObjectDialogue(I18n.GainedPower_Heat());
                        Game1.addMail($"CCStardewPower{CurrentEmpoweringElement.Value}", true, false);
                        master.FarmerSprite.loopThisAnimation = false;
                        master.yOffset = 0;
                    }
                    break;
            }
        }

        public override void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
        {
            base.performTouchAction(fullActionString, playerStandingPosition);

            if (fullActionString.StartsWith("CCEmpower "))
            {
                // TODO: Require holding item?

                if (CurrentEmpoweringAnim.Value != AnimStage.Done)
                    return;

                string elem = fullActionString.Substring("CCEmpower ".Length);
                CurrentEmpoweringPoint.Value = new((int)playerStandingPosition.X, (int)playerStandingPosition.Y);
                CurrentEmpoweringElement.Value = (Element) Enum.Parse(typeof(Element), elem);
                CurrentEmpoweringPlayer.Value = Game1.player.UniqueMultiplayerID;
                CurrentEmpoweringAnim.Value = AnimStage.PillarRise;
            }
        }

        public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
        {
            if (CurrentEmpoweringAnim.Value != AnimStage.Done)
            {
                Rectangle[] rs = new[]
                {
                    new Rectangle( CurrentEmpoweringPoint.X * 64 - 128, CurrentEmpoweringPoint.Y * 64 - 128, 64, 64 ),
                    new Rectangle( CurrentEmpoweringPoint.X * 64 + 128, CurrentEmpoweringPoint.Y * 64 - 128, 64, 64 ),
                    new Rectangle( CurrentEmpoweringPoint.X * 64 + 128, CurrentEmpoweringPoint.Y * 64 + 128, 64, 64 ),
                    new Rectangle( CurrentEmpoweringPoint.X * 64 - 128, CurrentEmpoweringPoint.Y * 64 + 128, 64, 64 ),
                };
                foreach (var r in rs)
                {
                    if (position.Intersects(r))
                        return true;
                }
            }

            return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
        }

        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);

            if (CurrentEmpoweringAnim.Value == AnimStage.Done)
                return;

            stageTimer += (float) time.ElapsedGameTime.TotalSeconds;

            if (stageTimer >= animLengths[(int)CurrentEmpoweringAnim.Value])
            {
                CurrentEmpoweringAnim.Value = (AnimStage)((int)CurrentEmpoweringAnim.Value + 1);
                stageTimer = 0;
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            if (CurrentEmpoweringAnim.Value == AnimStage.Done)
                return;
            Farmer master = Game1.getFarmer(CurrentEmpoweringPlayer.Value);

            float farmerDepth = FarmerRenderer.GetLayerDepth(0f, FarmerRenderer.FarmerSpriteLayers.MAX);

            Vector2[] pillars = new Vector2[]
            {
                new Vector2( CurrentEmpoweringPoint.X - 2, CurrentEmpoweringPoint.Y - 2.25f ),
                new Vector2( CurrentEmpoweringPoint.X + 2, CurrentEmpoweringPoint.Y - 2.25f ),
                new Vector2( CurrentEmpoweringPoint.X + 2, CurrentEmpoweringPoint.Y + 1.75f ),
                new Vector2( CurrentEmpoweringPoint.X - 2, CurrentEmpoweringPoint.Y + 1.75f ),
            };

            Vector2 vp = new Vector2(Game1.viewport.X, Game1.viewport.Y);

            float scaleMod = 1f + (float) Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 2) * 0.4f;

            int fireFrameNum = (int)((Game1.currentGameTime.TotalGameTime.TotalSeconds % 1) / (1f / 7));

            master.FarmerSprite.setCurrentAnimation(new FarmerSprite.AnimationFrame[] { new(94, 100) });
            master.FarmerSprite.loop = true;
            master.FarmerSprite.loopThisAnimation = true;
            master.FarmerSprite.PauseForSingleAnimation = true;

            float floatMod = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 2)) * 16;

            switch (CurrentEmpoweringAnim.Value)
            {
                case AnimStage.PillarRise:
                    {
                        float progress = Math.Min(1, stageTimer / 3f);
                        int height = (int)(64 * progress);
                        Rectangle pillarRect = new Rectangle(0, 0, 16, height);
                        Rectangle orbRect = new Rectangle(15, 0, 16, 12);
                        Rectangle ring1Rect = new Rectangle(15, 13, 16, 9);
                        Rectangle ring2Rect = new Rectangle(15, 23, 16, 9);
                        foreach (var pillar in pillars)
                        {
                            float depth = pillar.Y / 10000f;
                            Vector2 pos = pillar * Game1.tileSize - vp;
                            b.Draw(empowerTex, pos + new Vector2(0, -height * Game1.pixelZoom + Game1.tileSize), pillarRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth);
                            b.Draw(empowerTex, pos + new Vector2(0, -height * Game1.pixelZoom + Game1.tileSize), orbRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.0003f);

                            if (height > 13)
                            {
                                b.Draw(empowerTex, pos + new Vector2(0, (-height + 13) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring1Rect.X, ring1Rect.Y, ring1Rect.Width, ring1Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth - 0.0001f);
                                b.Draw(empowerTex, pos + new Vector2(0, (-height + 13 + ring1Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring1Rect.X, ring1Rect.Y + ring1Rect.Height / 2, ring1Rect.Width, ring1Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.0002f);
                            }
                            if (height > 23)
                            {
                                b.Draw(empowerTex, pos + new Vector2(0, (-height + 23) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring2Rect.X, ring2Rect.Y, ring2Rect.Width, ring2Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth - 0.0001f);
                                b.Draw(empowerTex, pos + new Vector2(0, (-height + 23 + ring2Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring2Rect.X, ring2Rect.Y + ring2Rect.Height / 2, ring2Rect.Width, ring2Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.0002f);
                            }
                        }
                        master.yOffset = height * Game1.pixelZoom / 2 + floatMod;
                    }
                    break;

                case AnimStage.PillarFall:
                    {
                        float progress = Math.Min(1, stageTimer / 3f);
                        int height = (int)(64 * (1 - progress));
                        Rectangle pillarRect = new Rectangle(0, 0, 16, height);
                        Rectangle orbRect = new Rectangle(15, 0, 16, 12);
                        Rectangle ring1Rect = new Rectangle(15, 13, 16, 9);
                        Rectangle ring2Rect = new Rectangle(15, 23, 16, 9);
                        foreach (var pillar in pillars)
                        {
                            float depth = pillar.Y / 10000f;
                            Vector2 pos = pillar * Game1.tileSize - vp;
                            b.Draw(empowerTex, pos + new Vector2(0, -height * Game1.pixelZoom + Game1.tileSize), pillarRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth);

                            b.Draw(empowerTex, pos + new Vector2(0, -64 * Game1.pixelZoom + Game1.tileSize), orbRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.0003f);

                            b.Draw(empowerTex, pos + new Vector2(0, (-64 + 13) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring1Rect.X, ring1Rect.Y, ring1Rect.Width, ring1Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth - 0.0001f);
                            b.Draw(empowerTex, pos + new Vector2(0, (-64 + 13 + ring1Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring1Rect.X, ring1Rect.Y + ring1Rect.Height / 2, ring1Rect.Width, ring1Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.0002f);

                            b.Draw(empowerTex, pos + new Vector2(0, (-64 + 23) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring2Rect.X, ring2Rect.Y, ring2Rect.Width, ring2Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth - 0.0001f);
                            b.Draw(empowerTex, pos + new Vector2(0, (-64 + 23 + ring2Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring2Rect.X, ring2Rect.Y + ring2Rect.Height / 2, ring2Rect.Width, ring2Rect.Height / 2), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, depth + 0.0002f);
                        }

                        master.yOffset = 64 * Game1.pixelZoom / 2 + floatMod;
                    }
                    break;

                case AnimStage.RingMove:
                    {
                        float progress = Math.Min(1, stageTimer / 1f);
                        int offset = (int)(-22 * progress);

                        Rectangle orbRect = new Rectangle(15, 0, 16, 12);
                        Rectangle ring1Rect = new Rectangle(15, 13, 16, 9);
                        Rectangle ring2Rect = new Rectangle(15, 23, 16, 9);
                        int fireCount = 0;
                        foreach (var pillar in pillars)
                        {
                            float depth = pillar.Y / 10000f;
                            Vector2 pos = pillar * Game1.tileSize - vp;

                            if (progress < 1f)
                            {
                                b.Draw(empowerTex, pos + new Vector2(0, -64 * Game1.pixelZoom + Game1.tileSize) + orbRect.Size.ToVector2() / 2 * Game1.pixelZoom, orbRect, Color.White, 0, orbRect.Size.ToVector2() / 2, Game1.pixelZoom * scaleMod, SpriteEffects.None, depth + 0.0003f);
                            }
                            else
                            {
                                Rectangle fireRect = new Rectangle(18 + (fireFrameNum + fireCount) % 7 * 16, 48, 16, 16);
                                float scaleMod2 = scaleMod * (1 + 0.2f * (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4 + fireCount * 0.25f)));
                                b.Draw(empowerTex, pos + new Vector2(0, -16-64 * Game1.pixelZoom + Game1.tileSize) + fireRect.Size.ToVector2() / 2 * Game1.pixelZoom, fireRect, Color.White, 0, fireRect.Size.ToVector2() / 2, Game1.pixelZoom * scaleMod2, SpriteEffects.None, depth + 0.0003f);
                                ++fireCount;
                            }

                            if (progress < 0.5f)
                            {
                                b.Draw(empowerTex, pos + new Vector2(ring1Rect.Width / 2 * Game1.pixelZoom, (-64 + 13 + offset + ring2Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring1Rect.X, ring1Rect.Y, ring1Rect.Width, ring1Rect.Height / 2), Color.White, 0, new Vector2( ring1Rect.Width / 2, ring1Rect.Height / 2), Game1.pixelZoom * (0.5f + ( 1 - progress ) * 0.5f) * scaleMod, SpriteEffects.None, depth - 0.0001f);
                                b.Draw(empowerTex, pos + new Vector2(ring1Rect.Width / 2 * Game1.pixelZoom, (-64 + 13 + offset + ring1Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring1Rect.X, ring1Rect.Y + ring1Rect.Height / 2, ring1Rect.Width, ring1Rect.Height / 2), Color.White, 0, new Vector2(ring1Rect.Width / 2, 0), Game1.pixelZoom * (0.5f + (1 - progress) * 0.5f) * scaleMod, SpriteEffects.None, depth + 0.0002f);
                            }

                            if (progress < 1f)
                            {
                                b.Draw(empowerTex, pos + new Vector2(ring2Rect.Width / 2 * Game1.pixelZoom, (-64 + 23 + offset + ring2Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring2Rect.X, ring2Rect.Y, ring2Rect.Width, ring2Rect.Height / 2), Color.White, 0, new Vector2(ring2Rect.Width / 2, ring2Rect.Height / 2), Game1.pixelZoom * (0.5f + (1 - progress) * 0.5f) * scaleMod, SpriteEffects.None, depth - 0.0001f);
                                b.Draw(empowerTex, pos + new Vector2(ring2Rect.Width / 2 * Game1.pixelZoom, (-64 + 23 + offset + ring2Rect.Height / 2) * Game1.pixelZoom + Game1.tileSize), new Rectangle(ring2Rect.X, ring2Rect.Y + ring2Rect.Height / 2, ring2Rect.Width, ring2Rect.Height / 2), Color.White, 0, new Vector2(ring2Rect.Width / 2, 0), Game1.pixelZoom * (0.5f + (1 - progress) * 0.5f) * scaleMod, SpriteEffects.None, depth + 0.0002f);
                            }
                        }

                        master.yOffset = 64 * Game1.pixelZoom / 2 + floatMod;

                        if (stageTimer >= 1 && stageTimer - Game1.currentGameTime.ElapsedGameTime.TotalSeconds < 1)
                            Game1.playSound("furnace");
                    }
                    break;


                case AnimStage.Circles:
                    {
                        float progress = Math.Min(1, stageTimer / 2.75f);
                        int offset = (int)(-22 * progress);

                        int fireCount = 0;
                        foreach (var pillar in pillars)
                        {
                            Vector2 pos = pillar * Game1.tileSize - vp;

                            Vector2 center = new Vector2(CurrentEmpoweringPoint.Value.X + 0.5f, CurrentEmpoweringPoint.Value.Y + 0.5f) * Game1.tileSize - vp;
                            center += new Vector2(-20, 16); // tweak, is camera really centered??? 

                            pos = Vector2.Transform(pos - center, Matrix.CreateRotationZ(progress * progress * 3.14f * 6)) + center;

                            Vector2 towardsCenter = (center - pos);
                            towardsCenter.Normalize();
                            pos += towardsCenter * (center - pos).Length() * progress * progress;

                            float depth = farmerDepth + ( pos.Y - center.Y ) / 0.0001f;

                            {
                                Rectangle fireRect = new Rectangle(18 + (fireFrameNum + fireCount) % 7 * 16, 48, 16, 16);
                                float scaleMod2 = scaleMod * (1 + 0.2f * (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4 + fireCount * 0.25f)));
                                b.Draw(empowerTex, pos + new Vector2(0, -16-64 * Game1.pixelZoom + Game1.tileSize) + fireRect.Size.ToVector2() / 2 * Game1.pixelZoom, fireRect, Color.White, 0, fireRect.Size.ToVector2() / 2, Game1.pixelZoom * scaleMod2, SpriteEffects.None, depth + 0.0003f);
                                ++fireCount;
                            }
                        }

                        master.yOffset = 64 * Game1.pixelZoom / 2 + floatMod;

                        if (stageTimer >= 2.75f && stageTimer - Game1.currentGameTime.ElapsedGameTime.TotalSeconds < 2.75f)
                            Game1.flashAlpha = 2;
                    }
                    break;

                case AnimStage.PlayerFall:
                    {
                        double progress = Math.Max(0, 1 - stageTimer / 3f);
                        int height = (int)(64 * progress);

                        master.yOffset = Math.Max(0, height * Game1.pixelZoom / 2 + floatMod);
                    }
                    break;
            }
        }
    }
}
