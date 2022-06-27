using System;
using System.Collections.Generic;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

using NVector3 = System.Numerics.Vector3;

namespace MiniGolfMod
{
    public struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation)
        {
        }

        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 1;
            pairMaterial.MaximumRecoveryVelocity = 2;
            pairMaterial.SpringSettings = new(30, 1);

            return true;
        }

        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }

    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        // Sure, why not?
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.ConserveMomentumWithGyroscopicTorque;

        private NVector3 grav;

        public void Initialize(Simulation simulation)
        {
        }

        public void PrepareForIntegration(float dt)
        {
            grav = new NVector3(0, -10, 0) * dt;
        }

        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            if (localInertia.InverseMass > 0)
                velocity.Linear += grav;
        }
    }

    public class GolfGameLocation : GameLocation
    {

        internal Dictionary<Farmer, BodyHandle> golfBalls = new();

        public GolfGameLocation(string mapPath, string name) : base(mapPath, name)
        {
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            foreach (var gb in golfBalls)
            {
                var body = Mod.instance.sim.Bodies.GetBodyReference(gb.Value);
                var bcenter = body.BoundingBox.Min + (body.BoundingBox.Max - body.BoundingBox.Min) / 2;
                var bcenter2d_ = new Vector2(bcenter.X, bcenter.Z + bcenter.Y) * Game1.tileSize;
                float bdepth = (bcenter.Z * Game1.tileSize + bcenter.Y) / 10000f;
                var bcenter2d = Game1.GlobalToLocal(bcenter2d_);
                b.Draw(Mod.instance.ballTex, bcenter2d, null, Color.White, 0, new Vector2( 2, 2 ), Vector2.One * Game1.pixelZoom, SpriteEffects.None, bdepth);

                if (gb.Key == Game1.player)
                {
                    b.Draw(Game1.staminaRect, bcenter2d, null, Color.Red, Mod.instance.angle * 3.14f / 180, new Vector2( 0, 0.5f ), new Vector2((Mod.instance.str ?? 0.5f) * 96, 8), SpriteEffects.None, 1);
                    Game1.player.Position = bcenter2d_;
                }
            }
        }

        protected override void drawFarmers(SpriteBatch b)
        {
            //base.drawFarmers(b);
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        internal Texture2D ballTex;

        internal Simulation sim;
        private BufferPool pool;

        internal float? str = null;
        internal float angle = 0;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            ballTex = Helper.ModContent.Load<Texture2D>("assets/ball.png");

            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            Helper.Events.Input.CursorMoved += this.Input_CursorMoved;
            Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += this.Input_ButtonReleased;
            
            Helper.ConsoleCommands.Add("golf", "...", DoGolfCommand);
        }

        private void Input_CursorMoved(object sender, StardewModdingAPI.Events.CursorMovedEventArgs e)
        {
            if (str != null)
                return;

            float hdiff = e.NewPosition.ScreenPixels.X - e.OldPosition.ScreenPixels.X;
            angle += hdiff;
        }

        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (Game1.currentLocation is GolfGameLocation ggl)
            {
                var gb = ggl.golfBalls[Game1.player];
                var body = sim.Bodies.GetBodyReference(gb);
                float rad = angle * 3.14f / 180;
                body.ApplyImpulse(new NVector3(MathF.Cos(rad) * str.Value, 0, MathF.Sin(rad) * str.Value), NVector3.Zero);
                body.Awake = true;
            }
            str = null;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            str = 0;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (sim != null)
            {
                sim.Timestep((float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds);
            }

            if (str != null)
            {
                str = str + (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                if (str > 5)
                    str = 5;
            }
        }

        private void DoGolfCommand(string cmd, string[] args)
        {
            if (sim != null)
            {
                sim.Dispose();
                sim = null;
            }

            var loc = new GolfGameLocation(Helper.ModContent.GetInternalAssetName("assets/Blank.tmx").Name, "Golf");
            var mapSize = loc.Map.Layers[0].LayerSize;

            Game1.currentLocation = loc;
            Game1.player.currentLocation = loc;
            Game1.player.Position = new Vector2( loc.Map.DisplaySize.Width, loc.Map.DisplaySize.Height ) / 2;

            pool = new BufferPool();
            sim = Simulation.Create(pool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new PositionLastTimestepper());
            sim.Statics.Add(new StaticDescription(new NVector3(-1, 0, -1), new CollidableDescription(sim.Shapes.Add(new Box(mapSize.Width + 2, 1, mapSize.Height + 2)), 1f)));
            // ...

            var sphere = new Sphere(0.25f / 2);
            sphere.ComputeInertia(0.046f /* kg? */, out BodyInertia sbi);
            loc.golfBalls.Add(
                Game1.player,
                sim.Bodies.Add(
                    BodyDescription.CreateDynamic(
                        new NVector3(Game1.player.Position.X / Game1.tileSize, 1, Game1.player.Position.Y / Game1.tileSize),
                        sbi,
                        new CollidableDescription(sim.Shapes.Add(sphere), 0.1f),
                        new BodyActivityDescription(0.01f)
                        )
                    )
                );
        }
    }
}
