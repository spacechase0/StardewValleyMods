using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpGLTF.Runtime;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Events;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Layers;
using xTile.Tiles;

namespace Stardew3D
{
    public class State
    {
        public Camera Camera { get; set; } = new();
        public Dictionary<Character, MonoGameModelInstance> CharacterModels { get; set; } = new();
        public ModelData ActiveLocation { get; set; }
    }

    public class MyModHooks : DelegatingModHooks
    {
        public MyModHooks(ModHooks theParent)
            : base( theParent )
        {
        }

        public override bool OnRendering(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D target_screen)
        {
            var models = Game1.content.Load<Dictionary<string, ModelData>>("spacechase0.Stardew3D/Models");
            if ( !Context.IsWorldReady || Game1.game1.takingMapScreenshot || !models.ContainsKey( Game1.currentLocation.Name ) )
                return Parent.OnRendering(step, sb, time, target_screen);

            if (true&&step == RenderSteps.World)
            {
                Mod.instance.DoRender();
                return false;
            }
            return Parent.OnRendering(step, sb, time, target_screen);
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static bool ShouldRun { get; private set; }
        public static Configuration Config { get; private set; }

        private static PerScreen<State> _state = new(() => new State());
        public static State State { get { return _state.Value; } }

        public Matrix projectionMatrix;
        public Matrix worldMatrixOrigin;
        public AlphaTestEffect basicEffect;
        public DepthStencilState depthState;
        public RasterizerState rasterState;
        public RasterizerState rasterStateCull;
        public SamplerState samplerState;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Config = Helper.ReadConfig<Configuration>();

            int expectedMajor = 4, expectedMinor = 0, expectedPatch = 0;
            if (Constants.ApiVersion.MajorVersion != expectedMajor && Constants.ApiVersion.MinorVersion != expectedMinor &&
                 Constants.ApiVersion.PatchVersion != expectedPatch)
            {
                Log.Error($"SMAPI version {expectedMajor}.{expectedMinor}.{expectedPatch} required! This mod will not run.");
                ShouldRun = false;
                return;
            }

            SkinnedEffectBony.Bytecode = File.ReadAllBytes(Path.Combine(Helper.DirectoryPath, "assets", "SkinnedEffectBony.mgfx"));

            Helper.Events.Player.Warped += OnWarp;
            Helper.Events.GameLoop.UpdateTicking += (s, e) => DoInput();
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;

            var hooks = AccessTools.Field(typeof(Game1), "hooks");
            hooks.SetValue(null, new MyModHooks(( ModHooks ) hooks.GetValue(null)));

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 200);
            worldMatrixOrigin = Matrix.CreateWorld(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
            basicEffect = new(Game1.graphics.GraphicsDevice)
            {
                Alpha = 1,
                VertexColorEnabled = true,
                Projection = projectionMatrix,
                //LightingEnabled = false, // TODO
                FogEnabled = false,
                //TextureEnabled = true,
            };
            rasterState = new()
            {
                CullMode = CullMode.None,
            };
            rasterStateCull = new()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
            };
            samplerState = new()
            {
                Filter = TextureFilter.Point,
            };

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            harmony.Patch(AccessTools.Method("SharpGLTF.Runtime.BasicEffectsLoaderContext:CreateSkinnedEffect"),
                          new HarmonyMethod(AccessTools.Method(typeof(BasicEffectsLoaderContextCreateSkinOverride), nameof(BasicEffectsLoaderContextCreateSkinOverride.Prefix))));
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.Stardew3D/Models"))
                e.LoadFrom( () => new Dictionary<string, ModelData>(), AssetLoadPriority.Low );
        }

        private void OnWarp(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation != null)
            {
                var models = Game1.content.Load<Dictionary<string, ModelData>>("spacechase0.Stardew3D/Models");
                if (models.ContainsKey(e.NewLocation.Name))
                    State.ActiveLocation = models[e.NewLocation.Name];
                else
                    State.ActiveLocation = null;
            }
            else
                State.ActiveLocation = null;
        }

        public void DoInput()
        {
            var models = Game1.content.Load<Dictionary<string, ModelData>>("spacechase0.Stardew3D/Models");
            //if (true) return;
            if (!Context.IsWorldReady || !Context.IsPlayerFree || !models.ContainsKey(Game1.currentLocation.Name))
                return;

            Game1.options.useToolButton.Do(b => Helper.Input.Suppress(b.ToSButton()));
            Game1.options.actionButton.Do(b => Helper.Input.Suppress(b.ToSButton()));
            Game1.options.moveLeftButton.Do(b => Helper.Input.Suppress(b.ToSButton()));
            Game1.options.moveRightButton.Do(b => Helper.Input.Suppress(b.ToSButton()));
            Game1.options.moveUpButton.Do(b => Helper.Input.Suppress(b.ToSButton()));
            Game1.options.moveDownButton.Do(b => Helper.Input.Suppress(b.ToSButton()));

            // TODO: Clean this section up with a local function and less copy paste
            var ks = Keyboard.GetState();
            var ms = Mouse.GetState();
            bool useToolPressed = false, actionPressed = false;
            bool leftPressed = false, rightPressed = false, upPressed = false, downPressed = false;
            foreach (var button in Game1.options.useToolButton)
            {
                if (button.mouseLeft && ms.LeftButton == ButtonState.Pressed ||
                    button.mouseRight && ms.RightButton == ButtonState.Pressed ||
                    button.key != Keys.None && ks.IsKeyDown(button.key))
                    useToolPressed = true;
            }
            foreach (var button in Game1.options.actionButton)
            {
                if (button.mouseLeft && ms.LeftButton == ButtonState.Pressed ||
                    button.mouseRight && ms.RightButton == ButtonState.Pressed ||
                    button.key != Keys.None && ks.IsKeyDown(button.key))
                    actionPressed = true;
            }
            foreach (var button in Game1.options.moveLeftButton)
            {
                if (button.mouseLeft && ms.LeftButton == ButtonState.Pressed ||
                    button.mouseRight && ms.RightButton == ButtonState.Pressed ||
                    button.key != Keys.None && ks.IsKeyDown(button.key))
                    leftPressed = true;
            }
            foreach (var button in Game1.options.moveRightButton)
            {
                if (button.mouseLeft && ms.LeftButton == ButtonState.Pressed ||
                    button.mouseRight && ms.RightButton == ButtonState.Pressed ||
                    button.key != Keys.None && ks.IsKeyDown(button.key))
                    rightPressed = true;
            }
            foreach (var button in Game1.options.moveUpButton)
            {
                if (button.mouseLeft && ms.LeftButton == ButtonState.Pressed ||
                    button.mouseRight && ms.RightButton == ButtonState.Pressed ||
                    button.key != Keys.None && ks.IsKeyDown(button.key))
                    upPressed = true;
            }
            foreach (var button in Game1.options.moveDownButton)
            {
                if (button.mouseLeft && ms.LeftButton == ButtonState.Pressed ||
                    button.mouseRight && ms.RightButton == ButtonState.Pressed ||
                    button.key != Keys.None && ks.IsKeyDown(button.key))
                    downPressed = true;
            }

            if (models.ContainsKey(Game1.currentLocation.Name))
            {
                // todo
            }

            
            float interactRayIntersectDist = float.MaxValue;
            object toInteract = null;
            // https://gamedev.stackexchange.com/q/110464
            var v = new Viewport(0, 0, Game1.viewport.Width, Game1.viewport.Height);
            var near = v.Unproject(new Vector3(Game1.getMouseX(), Game1.getMouseY(), 0), projectionMatrix, State.Camera.CreateViewMatrix(), Matrix.Identity);
            var far = v.Unproject(new Vector3(Game1.getMouseX(), Game1.getMouseY(), 1), projectionMatrix, State.Camera.CreateViewMatrix(), Matrix.Identity);
            var dir = far - near;
            dir.Normalize();
            Ray interactRay = new Ray(near, dir);

            foreach (var objPair in Game1.currentLocation.Objects.Pairs)
            {
                ModelData model = null;
                if (models.ContainsKey(objPair.Value.QualifiedItemId))
                {
                    model = models[objPair.Value.QualifiedItemId];
                }

                BoundingBox check = new BoundingBox(Vector3.Zero, objPair.Value.bigCraftable.Value ? new Vector3(1, 2, 1) : Vector3.One);
                if (model != null && model.InteractBox.HasValue)
                    check = model.InteractBox.Value;
                check.Min += (objPair.Key + new Vector2( 0.5f, 0.5f )).To3D() - new Vector3( 0.5f, 0, 0.5f );
                check.Max += (objPair.Key + new Vector2( 0.5f, 0.5f )).To3D() - new Vector3(0.5f, 0, 0.5f);

                float? interactDist = check.Intersects(interactRay);
                if (interactDist.HasValue && interactDist.Value < interactRayIntersectDist)
                {
                    interactRayIntersectDist = interactDist.Value;
                    toInteract = objPair.Value;

                    if (objPair.Value.TileLocation != objPair.Key)
                        objPair.Value.TileLocation = objPair.Key;
                }
            }

            foreach (var character in Game1.currentLocation.characters)
            {
                var spr = character.Sprite;

                BoundingBox check = new BoundingBox(Vector3.Zero, new Vector3(1, spr.SourceRect.Height / 16, 1));
                check.Min += character.GetPosition3D();
                check.Max += character.GetPosition3D();

                float? interactDist = check.Intersects(interactRay);
                if (interactDist.HasValue && interactDist.Value < interactRayIntersectDist)
                {
                    interactRayIntersectDist = interactDist.Value;
                    toInteract = character;
                }
            }

            // furniture
            // buildings
            // sitting?
            // other farmers
            // events
            // resource clumps

            Game1.isInspectionAtCurrentCursorTile = false;
            Game1.isActionAtCurrentCursorTile = false;
            Game1.isSpeechAtCurrentCursorTile = false;
            if (toInteract is StardewValley.Object obj)
            {
                Game1.isActionAtCurrentCursorTile = true;
                Game1.lastCursorTile = Game1.currentCursorTile = obj.TileLocation;
                if (actionPressed &&
                    Utility.tileWithinRadiusOfPlayer((int)obj.TileLocation.X, (int)obj.tileLocation.Y, 1, Game1.player))
                {
                    // cursor
                    Game1.player.lastGrabTile = obj.TileLocation;
                    DoObjectInteraction(obj);
                }
            }
            else if (toInteract is StardewValley.Character character)
            {
            }

            // placing objects, other left click things

            // TODO: Smoother movement code instead of hacking into existing
            Vector2 moveVec = Vector2.Zero;
            if (leftPressed) moveVec.X -= 1;
            if (rightPressed) moveVec.X += 1;
            if (upPressed) moveVec.Y -= 1;
            if (downPressed) moveVec.Y += 1;
            if ( moveVec.Length() != 0 ) moveVec.Normalize();
            var moveVec3d = Vector3.Transform(moveVec.To3D( false ), Quaternion.CreateFromAxisAngle(Vector3.Down, State.Camera.RotationY - MathF.PI / 2));

            float threshold = 0.5f;
            Game1.player.SetMovingLeft(moveVec3d.X < -threshold);
            Game1.player.SetMovingRight(moveVec3d.X > threshold);
            Game1.player.SetMovingUp(moveVec3d.Z < -threshold);
            Game1.player.SetMovingDown(moveVec3d.Z > threshold);
            DoFarmerMovePosition(Game1.player, moveVec3d, Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
        }

        private void DoFarmerMovePosition(Farmer player, Vector3 movement, GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (player.IsSitting())
                return;

            if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
            {
                return;
            }

            BoundingBoxGroup temporaryPassableTiles = Helper.Reflection.GetField<BoundingBoxGroup>(player, "temporaryPassableTiles").GetValue();
            if (player.CanMove || Game1.eventUp || player.controller != null)
            {
                // Need to rewrite to check both at once?

                float movementSpeed = player.getMovementSpeed();

                Vector3 actualMovement = movement * movementSpeed;

                Rectangle next = player.GetBoundingBox();
                Rectangle nextH = next, nextH2 = next;
                Rectangle nextV = next, nextV2 = next;
                nextH.Location += new Point((int)Math.Ceiling(actualMovement.X), 0);
                nextH2.Location += new Point((int)Math.Ceiling( actualMovement.X / 2), 0);
                nextV.Location += new Point(0, (int)Math.Ceiling(actualMovement.Z));
                nextV2.Location += new Point(0, (int)Math.Ceiling(actualMovement.Z / 2));

                temporaryPassableTiles.ClearNonIntersecting(player.GetBoundingBox());
                player.temporarySpeedBuff = 0f;
                if (actualMovement.Z != 0)
                {
                    Warp warp = Game1.currentLocation.isCollidingWithWarp(nextV, player);
                    if (warp != null && player.IsLocalPlayer)
                    {
                        player.warpFarmer(warp, 0);
                        return;
                    }
                    if (!currentLocation.isCollidingPosition(nextV, viewport, isFarmer: true, 0, glider: false, player) || player.ignoreCollisions)
                    {
                        player.position.Y += actualMovement.Z;
                        //player.behaviorOnMovement(0);
                    }
                    else if (!currentLocation.isCollidingPosition(nextV2, viewport, isFarmer: true, 0, glider: false, player))
                    {
                        player.position.Y += actualMovement.Z / 2;
                        //player.behaviorOnMovement(0);
                    }
                    //else Log.Debug("vcoll");
                }
                if (actualMovement.X != 0)
                {
                    Warp warp3 = Game1.currentLocation.isCollidingWithWarp(nextH, player);
                    if (warp3 != null && player.IsLocalPlayer)
                    {
                        player.warpFarmer(warp3, 1);
                        return;
                    }
                    if (!currentLocation.isCollidingPosition(nextH, viewport, isFarmer: true, 0, glider: false, player) || player.ignoreCollisions)
                    {
                        player.position.X += actualMovement.X;
                        //player.behaviorOnMovement(1);
                    }
                    else if (!currentLocation.isCollidingPosition(nextH2, viewport, isFarmer: true, 0, glider: false, player))
                    {
                        player.position.X += actualMovement.X / 2f;
                        //player.behaviorOnMovement(1);
                    }
                    //else Log.Debug("hcoll");
                }
            }
            /*
            if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
            {
                temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)player.getTileLocation().X * 64, (int)player.getTileLocation().Y * 64, 64, 64));
            }
            */
        }

        private void DoObjectInteraction(StardewValley.Object obj)
        {
            // todo
        }

        private void DoDrawBillboard(Texture2D tex, Vector3 pos, Vector2 displaySize, Rectangle texCoords)
        {
            float tx = texCoords.X / (float)tex.Width;
            float ty = texCoords.Y / (float)tex.Height;
            float txi = texCoords.Width / (float)tex.Width;
            float tyi = texCoords.Height / (float)tex.Height;

            MyVertex v1 = new(new(-displaySize.X / 2, displaySize.Y, 0), new Vector2(tx, ty));
            MyVertex v2 = new(new(displaySize.X / 2, displaySize.Y, 0), new Vector2(tx + txi, ty));
            MyVertex v3 = new(new(displaySize.X / 2, 0, 0), new Vector2(tx + txi, ty + tyi));
            MyVertex v4 = new(new(-displaySize.X / 2, 0, 0), new Vector2(tx, ty + tyi));

            using var vbo = new VertexBuffer(Game1.graphics.GraphicsDevice, typeof(MyVertex), 6, BufferUsage.WriteOnly);
            vbo.SetData(new[] { v1, v2, v3, v1, v3, v4 });
            Game1.graphics.GraphicsDevice.SetVertexBuffer(vbo);

            basicEffect.Texture = tex;
            //basicEffect.World = Matrix.CreateBillboard(pos, State.Camera.GetPosition(), Vector3.Up, State.Camera.Target - State.Camera.GetPosition());
            basicEffect.World = Matrix.CreateConstrainedBillboard( pos, State.Camera.GetPosition(), Vector3.Up, State.Camera.Target - State.Camera.GetPosition(), Vector3.Forward);

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 6);
            }
        }

        private void DoCamera()
        {
            State.Camera.RotationX = MathHelper.ToRadians(15);
            if (Config.RotateLeft.IsDown())
                State.Camera.RotationY += MathHelper.ToRadians(2);
            if (Config.RotateRight.IsDown())
                State.Camera.RotationY -= MathHelper.ToRadians(2);

            State.Camera.Target = Game1.player.GetPosition3D();

            basicEffect.View = State.Camera.CreateViewMatrix();
        }

        public void DoRender()
        {
            var models = Game1.content.Load<Dictionary<string, ModelData>>("spacechase0.Stardew3D/Models");

            DoCamera();

            Game1.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (models.ContainsKey(Game1.currentLocation.Name))
            {
                Game1.graphics.GraphicsDevice.RasterizerState = rasterStateCull;
                Game1.graphics.GraphicsDevice.SamplerStates[0] = samplerState;

                var m = models[Game1.currentLocation.Name];
                var inst = m.Model.Instance.CreateInstance();
                basicEffect.World = Matrix.CreateScale(m.Scale) *
                                    (Matrix.CreateRotationX(m.Rotation.X) * Matrix.CreateRotationY(m.Rotation.Y) * Matrix.CreateRotationZ(m.Rotation.Z)) *
                                    Matrix.CreateTranslation(m.Translation);
                // Can't find SetAnimationFrame method in Controller... hmm...
                inst.Draw(projectionMatrix, basicEffect.View, worldMatrixOrigin);
            }
            else
            {
                // This really should never happen...
                // Well, now it will. Probably should put some code here
            }

            Game1.graphics.GraphicsDevice.RasterizerState = rasterState;

            foreach (var obj in Game1.currentLocation.Objects.Pairs)
            {
                if (models.ContainsKey(obj.Value.QualifiedItemId))
                {
                    var model = models[obj.Value.QualifiedItemId];

                    var inst = model.Model.Instance.CreateInstance();
                    Matrix m1 = Matrix.CreateScale(model.Scale) *
                                (Matrix.CreateRotationX(model.Rotation.X) * Matrix.CreateRotationY(model.Rotation.Y) * Matrix.CreateRotationZ(model.Rotation.Z)) *
                                Matrix.CreateTranslation(model.Translation);
                    Matrix m = m1 * Matrix.CreateWorld((obj.Key + new Vector2( 0.5f, 0.5f )).To3D(), Vector3.Forward, Vector3.Up);
                    inst.Draw(projectionMatrix, basicEffect.View, m);

                    if (obj.Value.readyForHarvest)
                    {
                        if (model.HeldObjectOffset.HasValue)
                        {
                            if (models.ContainsKey(obj.Value.heldObject.Value.QualifiedItemId))
                            {
                                var mo = models[obj.Value.heldObject.Value.QualifiedItemId];
                                inst = mo.Model.Instance.CreateInstance();
                                m1 = Matrix.CreateScale(mo.Scale) *
                                     (Matrix.CreateRotationX(mo.Rotation.X) * Matrix.CreateRotationY(mo.Rotation.Y) * Matrix.CreateRotationZ(mo.Rotation.Z)) *
                                     Matrix.CreateTranslation(mo.Translation);
                                m = m1 * Matrix.CreateWorld((obj.Key + new Vector2( 0.5f, 0.5f )).To3D() + model.HeldObjectOffset.Value, Vector3.Forward, Vector3.Up);
                                inst.Draw(projectionMatrix, basicEffect.View, Matrix.CreateScale(model.HeldObjectScale) * m );
                            }
                            else
                            {
                                ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(obj.Value.heldObject.Value.QualifiedItemId);
                                DoDrawBillboard(draw.GetTexture(), (obj.Key + new Vector2(0.5f, 0.5f)).To3D() + model.HeldObjectOffset.Value, new Vector2(1, 1) * model.HeldObjectScale, draw.GetSourceRect(0));
                            }
                        }
                        else
                        {
                            Vector3 pos = (obj.Key + new Vector2(0.5f, 0.5f)).To3D() + new Vector3(0, 2, 0);
                            DoDrawBillboard(Game1.mouseCursors, pos, new Vector2( 1, 1 ), new Rectangle(141, 465, 20, 24) );
                            ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(obj.Value.heldObject.Value.QualifiedItemId);
                            DoDrawBillboard(draw.GetTexture(), pos, new Vector2(0.9f, 0.9f), draw.GetSourceRect(0));
                        }
                    }
                }
                else
                {
                    ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(obj.Value.QualifiedItemId);
                    DoDrawBillboard(draw.GetTexture(), (obj.Key+ new Vector2(0.5f, 0.5f)).To3D(), new Vector2(1, obj.Value.bigCraftable.Value ? 2 : 1), draw.GetSourceRect(obj.Value.showNextIndex.Value ? 1 : 0));

                    if (obj.Value.readyForHarvest)
                    {
                        Vector3 pos = (obj.Key + new Vector2( 0.5f, 0.5f )).To3D() + new Vector3(0, 2, 0);
                        DoDrawBillboard(Game1.mouseCursors, pos, new Vector2(1, 1), new Rectangle(141, 465, 20, 24));
                        draw = ItemRegistry.GetDataOrErrorItem(obj.Value.heldObject.Value.QualifiedItemId);
                        DoDrawBillboard(draw.GetTexture(), pos + new Vector3( 0, 0.1f, 0 ), new Vector2(0.9f, 0.9f), draw.GetSourceRect(0));
                    }
                }
            }

            foreach (var farmer in Game1.currentLocation.farmers)
            {
                var model = models["farmer"];
                if (!State.CharacterModels.ContainsKey(farmer))
                {
                    var newModel = model.Model.Instance.CreateInstance();
                    State.CharacterModels.Add(farmer, newModel);
                }
                var modelInst = State.CharacterModels[farmer];

                int ind = 0;
                for (; ind < modelInst.Controller.Armature.AnimationTracks.Count; ++ind)
                {
                    if (modelInst.Controller.Armature.AnimationTracks[ind].Name == "GeneWalking.002")
                    {
                        break;
                    }
                }
                if (ind < modelInst.Controller.Armature.AnimationTracks.Count)
                {
                    modelInst.Controller.Armature.SetAnimationFrame(ind, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds, true);
                }

                Matrix m1 = Matrix.CreateScale(model.Scale) *
                            (Matrix.CreateRotationX(model.Rotation.X) * Matrix.CreateRotationY(model.Rotation.Y) * Matrix.CreateRotationZ(model.Rotation.Z)) *
                            Matrix.CreateTranslation(model.Translation);
                Matrix m = m1 * Matrix.CreateRotationY(farmer.GetFacing3D()) * Matrix.CreateWorld(farmer.GetPosition3D(), Vector3.Forward, Vector3.Up);

                modelInst.Draw(projectionMatrix, basicEffect.View, m);
            }

            foreach (var character in Game1.currentLocation.characters)
            {
                var spr = character.Sprite;

                // todo - models

                DoDrawBillboard(spr.Texture, character.GetPosition3D(), new Vector2(spr.SourceRect.Width / 16, spr.SourceRect.Height / 16), spr.SourceRect);
            }
        }
    }

    public static class BasicEffectsLoaderContextCreateSkinOverride
    {
        public static bool Prefix(object __instance, SharpGLTF.Schema2.Material srcMaterial, ref Effect __result)
        {
            var dstMaterial = new SkinnedEffectBony(Game1.graphics.GraphicsDevice);

            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.Alpha = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetAlphaLevel" ).Invoke< float >( srcMaterial );// GetAlphaLevel(srcMaterial);
            dstMaterial.DiffuseColor = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetDiffuseColor").Invoke<Vector3>(srcMaterial);// GetDiffuseColor(srcMaterial);
            dstMaterial.SpecularColor = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetSpecularColor").Invoke<Vector3>(srcMaterial);// GetSpecularColor(srcMaterial);
            dstMaterial.SpecularPower = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GetSpecularPower").Invoke<float>(srcMaterial);// GetSpecularPower(srcMaterial);
            dstMaterial.EmissiveColor = Mod.instance.Helper.Reflection.GetMethod(__instance.GetType(), "GeEmissiveColor").Invoke<Vector3>(srcMaterial);// GeEmissiveColor(srcMaterial);
            dstMaterial.Texture = Mod.instance.Helper.Reflection.GetMethod(__instance, "UseDiffuseTexture").Invoke<Texture2D>(srcMaterial);// UseDiffuseTexture(srcMaterial);

            dstMaterial.WeightsPerVertex = 4;
            dstMaterial.PreferPerPixelLighting = true;

            // apparently, SkinnedEffect does not support disabling textures, so we set a white texture here.
            if (dstMaterial.Texture == null) dstMaterial.Texture = Mod.instance.Helper.Reflection.GetMethod(__instance, "UseTexture").Invoke<Texture2D>((SharpGLTF.Schema2.MaterialChannel?) null, (string) null);// UseTexture(null, null); // creates a dummy white texture.

            // todo - why no texture?

            __result = dstMaterial;
            return false;
        }
    }

    [HarmonyPatch(typeof(MonoGameModelInstance), "UpdateTransforms")]
    public static class MonoGameModelInstanceUpdateTransformsPatch
    {
        public static void Postfix(MonoGameModelInstance __instance, Effect effect, Matrix[] skinTransforms)
        {
            if (effect is SkinnedEffectBony seb)
            {
                seb.SetBoneTransforms(skinTransforms);
            }
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer))]
    public static class Game1ForceRenderOnBufferPatch
    {
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1.SetWindowSize))]
    public static class Game1AddDepthAndStencilToScreenPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldstr && insn.operand is string str && str == "Screen")
                {
                    ret[ret.Count - 7].opcode = OpCodes.Ldc_I4_3;
                }

                ret.Add(insn);
            }

            return ret;
        }
    }
}
