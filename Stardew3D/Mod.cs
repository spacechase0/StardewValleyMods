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
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Layers;
using xTile.Tiles;

namespace Stardew3D
{
    public class State
    {
        public Camera Camera { get; set; } = new();
        public Dictionary<Character, MonoGameModelInstance> CharacterModels { get; set; } = new();
    }

    public class Mod : StardewModdingAPI.Mod, IAssetLoader
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

            Helper.Events.GameLoop.UpdateTicking += (s, e) => DoInput();

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
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.SGame:DrawImpl"),
                          new HarmonyMethod(AccessTools.Method(typeof(SGameDrawOverride), nameof(SGameDrawOverride.Prefix))));
            harmony.Patch(AccessTools.Method("SharpGLTF.Runtime.BasicEffectsLoaderContext:CreateSkinnedEffect"),
                          new HarmonyMethod(AccessTools.Method(typeof(BasicEffectsLoaderContextCreateSkinOverride), nameof(BasicEffectsLoaderContextCreateSkinOverride.Prefix))));
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.Name.IsEquivalentTo("spacechase0.Stardew3D/Models"))
                return true;

            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.Name.IsEquivalentTo("spacechase0.Stardew3D/Models"))
                return (T)(object)new Dictionary<string, ModelData>();

            return default(T);
        }

        public void DoInput()
        {
            var models = Game1.content.Load<Dictionary<string, ModelData>>("spacechase0.Stardew3D/Models");

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
                if (models.ContainsKey(objPair.Value.QualifiedItemID))
                {
                    model = models[objPair.Value.QualifiedItemID];
                }

                BoundingBox check = new BoundingBox(Vector3.Zero, objPair.Value.bigCraftable.Value ? new Vector3(1, 2, 1) : Vector3.One);
                if (model != null && model.InteractBox.HasValue)
                    check = model.InteractBox.Value;
                check.Min += objPair.Key.To3D();
                check.Max += objPair.Key.To3D();

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
            moveVec.Normalize();
            var moveVec3d = Vector3.Transform(moveVec.To3D(), Quaternion.CreateFromAxisAngle(Vector3.Down, State.Camera.RotationY - MathF.PI / 2));

            float threshold = 0.5f;
            Game1.player.SetMovingLeft(moveVec3d.X < -threshold);
            Game1.player.SetMovingRight(moveVec3d.X > threshold);
            Game1.player.SetMovingUp(moveVec3d.Z < -threshold);
            Game1.player.SetMovingDown(moveVec3d.Z > threshold);
            Game1.player.MovePosition(Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
        }

        private void DoObjectInteraction(StardewValley.Object obj)
        {
            Farmer who = Game1.player;

            if (obj.Type != null)
            {
                if (who.isRidingHorse() && !(obj is Fence))
                {
                    return;
                }
                /*
                if (vect.Equals(who.getTileLocation()) && !this.objects[vect].isPassable())
                {
                    Tool t = new Pickaxe();
                    t.DoFunction(Game1.currentLocation, -1, -1, 0, who);
                    if (this.objects[vect].performToolAction(t, this))
                    {
                        this.objects[vect].performRemoveAction(this.objects[vect].tileLocation, Game1.currentLocation);
                        this.objects[vect].dropItem(this, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
                        Game1.currentLocation.Objects.Remove(vect);
                        return true;
                    }
                    t = new Axe();
                    t.DoFunction(Game1.currentLocation, -1, -1, 0, who);
                    if (this.objects.ContainsKey(vect) && this.objects[vect].performToolAction(t, this))
                    {
                        this.objects[vect].performRemoveAction(this.objects[vect].tileLocation, Game1.currentLocation);
                        this.objects[vect].dropItem(this, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
                        Game1.currentLocation.Objects.Remove(vect);
                        return true;
                    }
                    if (!this.objects.ContainsKey(vect))
                    {
                        return true;
                    }
                }
                */
                if ((obj.Type.Equals("Crafting") || obj.Type.Equals("interactive")))
                {
                    if (who.ActiveObject == null && obj.checkForAction(who))
                    {
                        return;
                    }
                    //if (this.objects.ContainsKey(vect))
                    {
                        if (who.CurrentItem != null)
                        {
                            StardewValley.Object old_held_object = obj.heldObject.Value;
                            obj.heldObject.Value = null;
                            bool probe_returned_true = obj.performObjectDropInAction(who.CurrentItem, probe: true, who);
                            obj.heldObject.Value = old_held_object;
                            bool perform_returned_true = obj.performObjectDropInAction(who.CurrentItem, probe: false, who);
                            if ((probe_returned_true || perform_returned_true) && who.isMoving())
                            {
                                Game1.haltAfterCheck = false;
                            }
                            if (perform_returned_true)
                            {
                                who.reduceActiveItemByOne();
                                return;
                            }
                            obj.checkForAction(who);
                            return;
                        }
                        obj.checkForAction(who);
                        return;
                    }
                }
                else if (obj.isSpawnedObject)
                {
                    int oldQuality = obj.quality;
                    Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)obj.TileLocation.X + (int)obj.TileLocation.Y * 777);
                    if (who.professions.Contains(16) && obj.isForage(Game1.currentLocation))
                    {
                        obj.Quality = 4;
                    }
                    else if (obj.isForage(Game1.currentLocation))
                    {
                        if (r.NextDouble() < (double)((float)who.ForagingLevel / 30f))
                        {
                            obj.Quality = 2;
                        }
                        else if (r.NextDouble() < (double)((float)who.ForagingLevel / 15f))
                        {
                            obj.Quality = 1;
                        }
                    }
                    if ((bool)obj.questItem && obj.questId.Value != 0 && !who.hasQuest(obj.questId))
                    {
                        return;
                    }
                    if (who.couldInventoryAcceptThisItem(obj))
                    {
                        if (who.IsLocalPlayer)
                        {
                            Game1.currentLocation.localSound("pickUpItem");
                            DelayedAction.playSoundAfterDelay("coin", 300);
                        }
                        who.animateOnce(279 + who.FacingDirection);
                        if (!Game1.currentLocation.isFarmBuildingInterior())
                        {
                            if (obj.isForage(Game1.currentLocation))
                            {
                                who.gainExperience(2, 7);
                            }
                        }
                        else
                        {
                            who.gainExperience(0, 5);
                        }
                        who.addItemToInventoryBool(obj.getOne());
                        Game1.stats.ItemsForaged++;
                        if (who.professions.Contains(13) && r.NextDouble() < 0.2 && !obj.questItem && who.couldInventoryAcceptThisItem(obj) && !Game1.currentLocation.isFarmBuildingInterior())
                        {
                            who.addItemToInventoryBool(obj.getOne());
                            who.gainExperience(2, 7);
                        }
                        Game1.currentLocation.objects.Remove(obj.TileLocation);
                        return;
                    }
                    obj.Quality = oldQuality;
                }
            }
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
                if (models.ContainsKey(obj.Value.QualifiedItemID))
                {
                    var model = models[obj.Value.QualifiedItemID];

                    var inst = model.Model.Instance.CreateInstance();
                    Matrix m1 = Matrix.CreateScale(model.Scale) *
                                (Matrix.CreateRotationX(model.Rotation.X) * Matrix.CreateRotationY(model.Rotation.Y) * Matrix.CreateRotationZ(model.Rotation.Z)) *
                                Matrix.CreateTranslation(model.Translation);
                    Matrix m = m1 * Matrix.CreateWorld(obj.Key.To3D() + new Vector3(0.5f, 0, 0.5f), Vector3.Forward, Vector3.Up);
                    inst.Draw(projectionMatrix, basicEffect.View, m);

                    if (obj.Value.readyForHarvest)
                    {
                        if (model.HeldObjectOffset.HasValue)
                        {
                            if (models.ContainsKey(obj.Value.heldObject.Value.QualifiedItemID))
                            {
                                var mo = models[obj.Value.heldObject.Value.QualifiedItemID];
                                inst = mo.Model.Instance.CreateInstance();
                                m1 = Matrix.CreateScale(mo.Scale) *
                                     (Matrix.CreateRotationX(mo.Rotation.X) * Matrix.CreateRotationY(mo.Rotation.Y) * Matrix.CreateRotationZ(mo.Rotation.Z)) *
                                     Matrix.CreateTranslation(mo.Translation);
                                m = m1 * Matrix.CreateWorld(obj.Key.To3D() + new Vector3(0.5f, 0, 0.5f) + model.HeldObjectOffset.Value, Vector3.Forward, Vector3.Up);
                                inst.Draw(projectionMatrix, basicEffect.View, Matrix.CreateScale(model.HeldObjectScale) * m );
                            }
                            else
                            {
                                ParsedItemData draw = Utility.GetItemDataForItemID(obj.Value.heldObject.Value.QualifiedItemID);
                                DoDrawBillboard(draw.texture, obj.Key.To3D() + new Vector3(0.5f, 0, 0.5f) + model.HeldObjectOffset.Value, new Vector2(1, 1) * model.HeldObjectScale, draw.GetSourceRect(0));
                            }
                        }
                        else
                        {
                            Vector3 pos = obj.Key.To3D() + new Vector3(0.5f, 2, 0.5f);
                            DoDrawBillboard(Game1.mouseCursors, pos, new Vector2( 1, 1 ), new Rectangle(141, 465, 20, 24) );
                            ParsedItemData draw = Utility.GetItemDataForItemID(obj.Value.heldObject.Value.QualifiedItemID);
                            DoDrawBillboard(draw.texture, pos, new Vector2(0.9f, 0.9f), draw.GetSourceRect(0));
                        }
                    }
                }
                else
                {
                    ParsedItemData draw = Utility.GetItemDataForItemID(obj.Value.QualifiedItemID);
                    DoDrawBillboard(draw.texture, obj.Key.To3D() + new Vector3(0.5f, 0, 0.5f), new Vector2(1, 2), draw.GetSourceRect(obj.Value.showNextIndex.Value ? 1 : 0));

                    if (obj.Value.readyForHarvest)
                    {
                        Vector3 pos = obj.Key.To3D() + new Vector3(0.5f, 2, 0.5f);
                        DoDrawBillboard(Game1.mouseCursors, pos, new Vector2(1, 1), new Rectangle(141, 465, 20, 24));
                        draw = Utility.GetItemDataForItemID(obj.Value.heldObject.Value.QualifiedItemID);
                        DoDrawBillboard(draw.texture, pos + new Vector3( 0, 0.1f, 0 ), new Vector2(0.9f, 0.9f), draw.GetSourceRect(0));
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
                    Log.Debug("set " + Game1.currentGameTime.TotalGameTime.TotalSeconds);
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

    public static class SGameDrawOverride
    {
        public static bool Prefix(GameTime gameTime, RenderTarget2D target_screen,
                                  Task ____newDayTask, IMonitor ___Monitor, Multiplayer ___multiplayer)
        {
            var models = Game1.content.Load<Dictionary<string, ModelData>>("spacechase0.Stardew3D/Models");
            if (!Context.IsWorldReady || Game1.game1.takingMapScreenshot || !models.ContainsKey( Game1.currentLocation.Name ) )
                return true;

            Impl(gameTime, target_screen,
                 ____newDayTask, ___Monitor, ___multiplayer);
            return false;
        }

        private static void Impl(GameTime gameTime, RenderTarget2D target_screen,
                                 Task ____newDayTask, IMonitor ___Monitor, Multiplayer ___multiplayer)
        {
            var graphicsDevice = Game1.graphics.GraphicsDevice;

            Game1.showingHealthBar = false;
            if (____newDayTask != null && Game1.game1.isLocalMultiplayerNewDayActive)
            {
                graphicsDevice.Clear(Game1.bgColor);
                return;
            }
            if (target_screen != null)
            {
                //Game1.SetRenderTarget(target_screen);
                Game1.graphics.GraphicsDevice.SetRenderTarget(target_screen);
            }

            if (Game1.game1.IsSaving)
            {
                graphicsDevice.Clear(Game1.bgColor);
                Game1.PushUIMode();
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    //events.Rendering.RaiseEmpty();
                    try
                    {
                        //events.RenderingActiveMenu.RaiseEmpty();
                        menu.draw(Game1.spriteBatch);
                        //events.RenderedActiveMenu.RaiseEmpty();
                    }
                    catch (Exception ex)
                    {
                        ___Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself during save. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        menu.exitThisMenu();
                    }
                    //events.Rendered.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                if (Game1.overlayMenu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.overlayMenu.draw(Game1.spriteBatch);
                    Game1.spriteBatch.End();
                }
                Game1.PopUIMode();
                return;
            }
            graphicsDevice.Clear(Game1.bgColor);
            if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !Game1.game1.takingMapScreenshot)
            {
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                //events.Rendering.RaiseEmpty();
                IClickableMenu curMenu = null;
                try
                {
                    Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
                    //events.RenderingActiveMenu.RaiseEmpty();
                    for (curMenu = Game1.activeClickableMenu; curMenu != null; curMenu = curMenu.GetChildMenu())
                    {
                        curMenu.draw(Game1.spriteBatch);
                    }
                    //events.RenderedActiveMenu.RaiseEmpty();
                }
                catch (Exception ex)
                {
                    ___Monitor.Log($"The {curMenu.GetMenuChainLabel()} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                    Game1.activeClickableMenu.exitThisMenu();
                }
                //events.Rendered.RaiseEmpty();
                if (Game1.specialCurrencyDisplay != null)
                {
                    Game1.specialCurrencyDisplay.Draw(Game1.spriteBatch);
                }
                Game1.spriteBatch.End();
                AccessTools.Method( Game1.game1.GetType(), "drawOverlays" ).Invoke(Game1.game1, new object[] { Game1.spriteBatch } );
                Game1.PopUIMode();
                return;
            }
            if (Game1.gameMode == 11)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
                //events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();
                return;
            }
            if (Game1.currentMinigame != null)
            {
                /*
                if (events.Rendering.HasListeners())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    //events.Rendering.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                */

                Game1.currentMinigame.draw(Game1.spriteBatch);
                if (Game1.globalFade && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                {
                    Game1.PushUIMode();
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                    Game1.spriteBatch.End();
                    Game1.PopUIMode();
                }
                Game1.PushUIMode();

                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                /*
                if (events.Rendered.HasListeners())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    //events.Rendered.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                */
                //Game1.SetRenderTarget(target_screen);
                Game1.graphics.GraphicsDevice.SetRenderTarget(target_screen);
                return;
            }
            if (Game1.showingEndOfNightStuff)
            {
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
                if (Game1.activeClickableMenu != null)
                {
                    IClickableMenu curMenu = null;
                    try
                    {
                        //events.RenderingActiveMenu.RaiseEmpty();
                        for (curMenu = Game1.activeClickableMenu; curMenu != null; curMenu = curMenu.GetChildMenu())
                        {
                            curMenu.draw(Game1.spriteBatch);
                        }
                        //events.RenderedActiveMenu.RaiseEmpty();
                    }
                    catch (Exception ex)
                    {
                        ___Monitor.Log($"The {curMenu.GetMenuChainLabel()} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                }
                Game1.spriteBatch.End();

                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                return;
            }
            if (Game1.gameMode == 6 || (Game1.gameMode == 3 && Game1.currentLocation == null))
            {
                Game1.PushUIMode();
                graphicsDevice.Clear(Game1.bgColor);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
                string addOn = "".PadRight((int)Math.Ceiling(gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0), '.');
                string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
                string msg = text + addOn;
                string largestMessage = text + "... ";
                int msgw = SpriteText.getWidthOfString(largestMessage);
                int msgh = 64;
                int msgx = 64;
                int msgy = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - msgh;
                SpriteText.drawString(Game1.spriteBatch, msg, msgx, msgy, 999999, msgw, msgh, 1f, 0.88f, junimoText: false, 0, largestMessage);
                //events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();

                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                return;
            }
            if (Game1.gameMode == 0)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
            }
            else
            {
                if (Game1.gameMode == 3 && Game1.dayOfMonth == 0 && Game1.newDay)
                {
                    // This was commented out in the SMAPI code, not by me
                    //base.Draw(gameTime);
                    return;
                }
                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                bool renderingRaised = false;
                Mod.instance.DoRender();
                /*
                if (Game1.drawLighting)
                {
                    // I'll need to redo this anyways
                    //Game1.game1.DrawLighting(gameTime, target_screen, out renderingRaised);
                }
                graphicsDevice.Clear(Game1.bgColor);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!renderingRaised)
                    ;// events.Rendering.RaiseEmpty();
                //events.RenderingWorld.RaiseEmpty();
                if (Game1.background != null)
                {
                    Game1.background.draw(Game1.spriteBatch);
                }
                Game1.currentLocation.drawBackground(Game1.spriteBatch);
                Game1.spriteBatch.End();
                for (int i = 0; i < Game1.currentLocation.backgroundLayers.Count; i++)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.currentLocation.backgroundLayers[i].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, -1f);
                    Game1.spriteBatch.End();
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Game1.currentLocation.drawWater(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                Game1.currentLocation.drawFloorDecorations(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                var _farmerShadows = Mod.instance.Helper.Reflection.GetField<List<Farmer>>(Game1.game1, "_farmerShadows").GetValue();
                _farmerShadows.Clear();
                if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
                {
                    foreach (Farmer f in Game1.currentLocation.currentEvent.farmerActors)
                    {
                        if ((f.IsLocalPlayer && Game1.displayFarmer) || !f.hidden)
                        {
                            _farmerShadows.Add(f);
                        }
                    }
                }
                else
                {
                    foreach (Farmer f2 in Game1.currentLocation.farmers)
                    {
                        if ((f2.IsLocalPlayer && Game1.displayFarmer) || !f2.hidden)
                        {
                            _farmerShadows.Add(f2);
                        }
                    }
                }
                if (!Game1.currentLocation.shouldHideCharacters())
                {
                    if (Game1.CurrentEvent == null)
                    {
                        foreach (NPC n in Game1.currentLocation.characters)
                        {
                            if (!n.swimming && !n.HideShadow && !n.IsInvisible && !Game1.game1.checkCharacterTilesForShadowDrawFlag(n))
                            {
                                n.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    else
                    {
                        foreach (NPC n2 in Game1.CurrentEvent.actors)
                        {
                            if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n2)) && !n2.swimming && !n2.HideShadow && !Game1.game1.checkCharacterTilesForShadowDrawFlag(n2))
                            {
                                n2.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    foreach (Farmer f3 in _farmerShadows)
                    {
                        if (!___multiplayer.isDisconnecting(f3.UniqueMultiplayerID) && !f3.swimming && !f3.isRidingHorse() && !f3.IsSitting() && (Game1.currentLocation == null || !Game1.game1.checkCharacterTilesForShadowDrawFlag(f3)))
                        {
                            f3.DrawShadow(Game1.spriteBatch);
                        }
                    }
                }
                float layer_sub_sort = 0.1f;
                for (int j = 0; j < Game1.currentLocation.buildingLayers.Count; j++)
                {
                    float layer = 0f;
                    if (Game1.currentLocation.buildingLayers.Count > 1)
                    {
                        layer = (float)j / (float)(Game1.currentLocation.buildingLayers.Count - 1);
                    }
                    Game1.currentLocation.buildingLayers[j].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, layer_sub_sort * layer);
                }
                Layer building_layer = Game1.currentLocation.Map.GetLayer("Buildings");
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!Game1.currentLocation.shouldHideCharacters())
                {
                    if (Game1.CurrentEvent == null)
                    {
                        foreach (NPC n3 in Game1.currentLocation.characters)
                        {
                            if (!n3.swimming && !n3.HideShadow && !n3.isInvisible && Game1.game1.checkCharacterTilesForShadowDrawFlag(n3))
                            {
                                n3.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    else
                    {
                        foreach (NPC n4 in Game1.CurrentEvent.actors)
                        {
                            if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n4)) && !n4.swimming && !n4.HideShadow && Game1.game1.checkCharacterTilesForShadowDrawFlag(n4))
                            {
                                n4.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    foreach (Farmer f4 in _farmerShadows)
                    {
                        Math.Max(0.0001f, f4.getDrawLayer() + 0.00011f);
                        if (!f4.swimming && !f4.isRidingHorse() && !f4.IsSitting() && Game1.currentLocation != null && Game1.game1.checkCharacterTilesForShadowDrawFlag(f4))
                        {
                            f4.DrawShadow(Game1.spriteBatch);
                        }
                    }
                }
                if ((Game1.eventUp || Game1.killScreen) && !Game1.killScreen && Game1.currentLocation.currentEvent != null)
                {
                    Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
                }
                Game1.currentLocation.draw(Game1.spriteBatch);
                foreach (Vector2 tile_position in Game1.crabPotOverlayTiles.Keys)
                {
                    Tile tile = building_layer.Tiles[(int)tile_position.X, (int)tile_position.Y];
                    if (tile != null)
                    {
                        Vector2 vector_draw_position = Game1.GlobalToLocal(Game1.viewport, tile_position * 64f);
                        xTile.Dimensions.Location draw_location = new xTile.Dimensions.Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
                        Game1.mapDisplayDevice.DrawTile(tile, draw_location, (tile_position.Y * 64f - 1f) / 10000f);
                    }
                }
                if (Game1.player.ActiveObject == null && (Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool))
                {
                    Game1.drawTool(Game1.player);
                }
                if (Game1.tvStation >= 0)
                {
                    Game1.spriteBatch.Draw(Game1.tvStationTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(400f, 160f)), new Microsoft.Xna.Framework.Rectangle(Game1.tvStation * 24, 0, 24, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                }
                if (Game1.panMode)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
                    foreach (Warp w in Game1.currentLocation.warps)
                    {
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(w.X * 64 - Game1.viewport.X, w.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
                    }
                }
                for (int l = 0; l < Game1.currentLocation.frontLayers.Count; l++)
                {
                    float layer2 = 0f;
                    if (Game1.currentLocation.frontLayers.Count > 1)
                    {
                        layer2 = (float)l / (float)(Game1.currentLocation.frontLayers.Count - 1);
                    }
                    Game1.currentLocation.frontLayers[l].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, 64f + layer_sub_sort * layer2);
                }
                Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                Game1.spriteBatch.End();
                for (int m = 0; m < Game1.currentLocation.alwaysFrontLayers.Count; m++)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.currentLocation.alwaysFrontLayers[m].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, -1f);
                    Game1.spriteBatch.End();
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!Game1.IsFakedBlackScreen())
                {
                    Game1.game1.drawWeather(gameTime, target_screen);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (Game1.currentLocation.LightLevel > 0f && Game1.timeOfDay < 2000)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
                }
                if (Game1.screenGlow)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (Game1.toolHold > 400f && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                {
                    Color barColor = Color.White;
                    switch ((int)(Game1.toolHold / 600f) + 2)
                    {
                        case 1:
                            barColor = Tool.copperColor;
                            break;
                        case 2:
                            barColor = Tool.steelColor;
                            break;
                        case 3:
                            barColor = Tool.goldColor;
                            break;
                        case 4:
                            barColor = Tool.iridiumColor;
                            break;
                    }
                    Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - ((!Game1.player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0) - 2, (int)(Game1.toolHold % 600f * 0.08f) + 4, 12), Color.Black);
                    Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - ((!Game1.player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0), (int)(Game1.toolHold % 600f * 0.08f), 8), barColor);
                }
                Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
                if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && ((Game1.player.CurrentTool as FishingRod).isTimingCast || (Game1.player.CurrentTool as FishingRod).castingChosenCountdown > 0f || (Game1.player.CurrentTool as FishingRod).fishCaught || (Game1.player.CurrentTool as FishingRod).showingTreasure))
                {
                    Game1.player.CurrentTool.draw(Game1.spriteBatch);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                {
                    foreach (NPC n5 in Game1.currentLocation.currentEvent.actors)
                    {
                        if (n5.isEmoting)
                        {
                            Vector2 emotePosition = n5.getLocalPosition(Game1.viewport);
                            if (n5.NeedsBirdieEmoteHack())
                            {
                                emotePosition.X += 64f;
                            }
                            emotePosition.Y -= 140f;
                            if (n5.Age == 2)
                            {
                                emotePosition.Y += 32f;
                            }
                            else if (n5.Gender == 1)
                            {
                                emotePosition.Y += 10f;
                            }
                            Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(n5.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, n5.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)n5.getStandingY() / 10000f);
                        }
                    }
                }
                Game1.spriteBatch.End();
                Game1.mapDisplayDevice.EndScene();
                if (Game1.drawLighting && !Game1.IsFakedBlackScreen())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, Mod.instance.Helper.Reflection.GetField<BlendState>(Game1.game1, "lightingBlend").GetValue(), SamplerState.LinearClamp);
                    Viewport vp = graphicsDevice.Viewport;
                    vp.Bounds = target_screen?.Bounds ?? graphicsDevice.PresentationParameters.Bounds;
                    graphicsDevice.Viewport = vp;
                    float render_zoom = Game1.options.lightingQuality / 2;
                    if (Game1.game1.useUnscaledLighting)
                    {
                        render_zoom /= Game1.options.zoomLevel;
                    }
                    Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, Game1.lightmap.Bounds, Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
                    if (Game1.IsRainingHere() && (bool)Game1.currentLocation.isOutdoors)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, vp.Bounds, Color.OrangeRed * 0.45f);
                    }
                    Game1.spriteBatch.End();
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.RenderedWorld.RaiseEmpty();
                if (Game1.drawGrid)
                {
                    int startingX = -Game1.viewport.X % 64;
                    float startingY = -Game1.viewport.Y % 64;
                    for (int x = startingX; x < Game1.graphics.GraphicsDevice.Viewport.Width; x += 64)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x, (int)startingY, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
                    }
                    for (float y = startingY; y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 64f)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(startingX, (int)y, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
                    }
                }
                if (Game1.ShouldShowOnscreenUsernames() && Game1.currentLocation != null)
                {
                    Game1.currentLocation.DrawFarmerUsernames(Game1.spriteBatch);
                }
                if (Game1.currentBillboard != 0 && !Game1.game1.takingMapScreenshot)
                {
                    Game1.game1.drawBillboard();
                }
                if (!Game1.eventUp && Game1.farmEvent == null && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.game1.takingMapScreenshot && Game1.isOutdoorMapSmallerThanViewport())
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -Game1.viewport.X, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64, 0, Game1.graphics.GraphicsDevice.Viewport.Width - (-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64), Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, -Game1.viewport.Y), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, -Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - (-Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64)), Color.Black);
                }
                Game1.spriteBatch.End();
                */
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if ((Game1.displayHUD || Game1.eventUp) && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused && !Game1.game1.takingMapScreenshot)
                {
                    //events.RenderingHud.RaiseEmpty();
                    Mod.instance.Helper.Reflection.GetMethod(Game1.game1, "drawHUD").Invoke();
                    //events.RenderedHud.RaiseEmpty();
                }
                else if (Game1.activeClickableMenu == null)
                {
                    _ = Game1.farmEvent;
                }
                if (Game1.hudMessages.Count > 0 && !Game1.game1.takingMapScreenshot)
                {
                    for (int k = Game1.hudMessages.Count - 1; k >= 0; k--)
                    {
                        Game1.hudMessages[k].draw(Game1.spriteBatch, k);
                    }
                }
                Game1.spriteBatch.End();
                Game1.PopUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            if (Game1.farmEvent != null)
            {
                Game1.farmEvent.draw(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            Game1.PushUIMode();
            if (Game1.dialogueUp && !Game1.nameSelectUp && !Game1.messagePause && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)) && !Game1.game1.takingMapScreenshot)
            {
                Mod.instance.Helper.Reflection.GetMethod(Game1.game1, "drawDialogueBox").Invoke();
            }
            if (Game1.progressBar && !Game1.game1.takingMapScreenshot)
            {
                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, Game1.dialogueWidth, 32), Color.LightGray);
                Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, (int)(Game1.pauseAccumulator / Game1.pauseTime * (float)Game1.dialogueWidth), 32), Color.DimGray);
            }
            Game1.spriteBatch.End();
            Game1.PopUIMode();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (Game1.eventUp && Game1.currentLocation != null && Game1.currentLocation.currentEvent != null)
            {
                Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
            }
            if (!Game1.IsFakedBlackScreen() && Game1.IsRainingHere() && Game1.currentLocation != null && (bool)Game1.currentLocation.isOutdoors)
            {
                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
            }
            if ((Game1.fadeToBlack || Game1.globalFade) && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause) && !Game1.game1.takingMapScreenshot)
            {
                Game1.spriteBatch.End();
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                Game1.spriteBatch.End();
                Game1.PopUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            else if (Game1.flashAlpha > 0f && !Game1.game1.takingMapScreenshot)
            {
                if (Game1.options.screenFlash)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
                }
                Game1.flashAlpha -= 0.1f;
            }
            if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp && !Game1.game1.takingMapScreenshot)
            {
                Mod.instance.Helper.Reflection.GetMethod(Game1.game1, "drawDialogueBox").Invoke();
            }
            if (!Game1.game1.takingMapScreenshot)
            {
                foreach (TemporaryAnimatedSprite screenOverlayTempSprite in Game1.screenOverlayTempSprites)
                {
                    screenOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
                }
                Game1.spriteBatch.End();
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                foreach (TemporaryAnimatedSprite uiOverlayTempSprite in Game1.uiOverlayTempSprites)
                {
                    uiOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
                }
                Game1.spriteBatch.End();
                Game1.PopUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            /*
            if (Game1.debugMode)
            {
                StringBuilder sb = Game1._debugStringBuilder;
                sb.Clear();
                if (Game1.panMode)
                {
                    sb.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
                    sb.Append(",");
                    sb.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
                }
                else
                {
                    sb.Append("player: ");
                    sb.Append(Game1.player.getStandingX() / 64);
                    sb.Append(", ");
                    sb.Append(Game1.player.getStandingY() / 64);
                }
                sb.Append(" mouseTransparency: ");
                sb.Append(Game1.mouseCursorTransparency);
                sb.Append(" mousePosition: ");
                sb.Append(Game1.getMouseX());
                sb.Append(",");
                sb.Append(Game1.getMouseY());
                sb.Append(Environment.NewLine);
                sb.Append(" mouseWorldPosition: ");
                sb.Append(Game1.getMouseX() + Game1.viewport.X);
                sb.Append(",");
                sb.Append(Game1.getMouseY() + Game1.viewport.Y);
                sb.Append("  debugOutput: ");
                sb.Append(Game1.debugOutput);
                Game1.spriteBatch.DrawString(Game1.smallFont, sb, new Vector2(graphicsDevice.Viewport.GetTitleSafeArea().X, base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            */
            Game1.spriteBatch.End();
            Game1.PushUIMode();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (Game1.showKeyHelp && !Game1.game1.takingMapScreenshot)
            {
                Game1.spriteBatch.DrawString(Game1.smallFont, Game1.keyHelpString, new Vector2(64f, (float)(Game1.viewport.Height - 64 - (Game1.dialogueUp ? (192 + (Game1.isQuestion ? (Game1.questionChoices.Count * 64) : 0)) : 0)) - Game1.smallFont.MeasureString(Game1.keyHelpString).Y), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            if (Game1.activeClickableMenu != null && !Game1.game1.takingMapScreenshot)
            {
                IClickableMenu curMenu = null;
                try
                {
                    //events.RenderingActiveMenu.RaiseEmpty();
                    for (curMenu = Game1.activeClickableMenu; curMenu != null; curMenu = curMenu.GetChildMenu())
                    {
                        curMenu.draw(Game1.spriteBatch);
                    }
                    //events.RenderedActiveMenu.RaiseEmpty();
                }
                catch (Exception ex)
                {
                    ___Monitor.Log($"The {curMenu.GetMenuChainLabel()} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                    Game1.activeClickableMenu.exitThisMenu();
                }
            }
            else if (Game1.farmEvent != null)
            {
                Game1.farmEvent.drawAboveEverything(Game1.spriteBatch);
            }
            if (Game1.specialCurrencyDisplay != null)
            {
                Game1.specialCurrencyDisplay.Draw(Game1.spriteBatch);
            }
            if (Game1.emoteMenu != null && !Game1.game1.takingMapScreenshot)
            {
                Game1.emoteMenu.draw(Game1.spriteBatch);
            }
            if (Game1.HostPaused && !Game1.game1.takingMapScreenshot)
            {
                string msg2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
                SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, msg2, 96, 32);
            }
            //events.Rendered.RaiseEmpty();
            Game1.spriteBatch.End();
            AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
            Game1.PopUIMode();
        }
    }
}
