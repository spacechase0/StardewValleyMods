using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HarmonyLib;
using MageDelve.Mercenaries.Actions;
using MageDelve.Mercenaries.Effects;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Tools;

namespace MageDelve.Mercenaries
{
    [XmlType("Mods_spacechase0_MageDelve_Mercenary")]
    public class Mercenary : Character
    {
        private readonly NetString correspondingNpc = new();
        public string CorrespondingNpc => correspondingNpc.Value;

        private readonly NetFloat untilNextAction = new();
        private readonly NetFloat untilEndAction = new();

        private Dictionary<string, float> currentCooldowns = new();

        private Stack<AnimatedSprite> spriteStack = new();

        private GameLocation lastPathfindLoc;
        private bool[] pathfindSolids;

        internal float untilNextPathfind;
        private Queue<Vector2> lastPath = new();
        private Dictionary<Vector2, float> encountered = new(); // here for debug display

        internal Farmer dummy = new();

        internal Monster targeting = new();
        internal MeleeWeapon attackWeapon;
        internal string attackId;
        internal bool showWeapon = false;
        internal float swingTime = 0;
        internal bool hasSwung = false;

        internal Vector2 targetPos;

        private static float pathfindStep = 16;
        private static float pathfindStepDiag = 16 * 0.707f;

        public Mercenary()
        {
        }

        public Mercenary(string npc, Vector2 pos)
        : base(new AnimatedSprite($"Characters\\{npc}", 0, 16, 32), pos, 2, npc)
        {
            correspondingNpc.Value = npc;
            Speed = 5;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(correspondingNpc, "correspondingNpc");
            NetFields.AddField(untilNextAction, "untilNextAction");
            NetFields.AddField(untilEndAction, "untilEndAction");
        }

        public void OnLeave()
        {
            NPC npc = Game1.getCharacterFromName(correspondingNpc, true);
            npc.IsInvisible = false;
            
            npc.controller = npc.temporaryController = null;
            npc.DirectionsToNewLocation = null;
            //npc.previousEndPoint = new Point(npc.DefaultPosition.X / 64, npc.DefaultPosition.Y / 64);
            //npc.isWalkingInSquare = false;
            //npc.returningToEndPoint = false;
            npc.lastCrossroad = Microsoft.Xna.Framework.Rectangle.Empty;
            //npc._startedEndOfRouteBehavior = null;
            //npc._finishingEndOfRouteBehavior = null;
            npc.lastAttemptedSchedule = 0;
            npc.currentLocation.characters.Remove(npc);
            var l = Game1.getLocationFromName(npc.DefaultMap);
            l.characters.Add(npc);
            npc.currentLocation = l;
            npc.Position = npc.DefaultPosition;
            npc.faceDirection(npc.DefaultFacingDirection);

            if (!npc.TryLoadSchedule())
            {
                Log.Warn($"Failed to load NPC schedule for {name} after mercenary dismissal");
            }
            npc.lastAttemptedSchedule = 0;
            for (int time = 600; time < Game1.timeOfDay; time += 10)
            {
                npc.checkSchedule(time);
                npc.warpToPathControllerDestination();
            }
            npc.checkSchedule(Game1.timeOfDay);
        }

        // TODO: Clean this function up
        public void UpdateForFarmer(Farmer farmer, int numInFormation, GameTime gameTime)
        {
            // TODO: Don't update if time isn't passing!

            if (targeting != null && targeting.Health <= 0)
                targeting = null;

            var data = this.GetMercenaryData();

            currentLocation = farmer.currentLocation;
            this.UpdateEffects((float)gameTime.ElapsedGameTime.TotalSeconds);
            UpdateCooldowns(gameTime);

            if (Sprite.CurrentAnimation != null)
            {
                int oldAnim = Sprite.currentAnimationIndex;
                Sprite.animateOnce(gameTime);
                if (Sprite.currentAnimationIndex < oldAnim)
                {
                    if (spriteStack.Count > 0)
                    {
                        Sprite = spriteStack.Pop();
                    }
                }
            }

            untilNextPathfind -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Game1.player == farmer)
            {
                if (targeting != null && attackWeapon != null)
                {
                    //Console.WriteLine(Vector2.Distance(targeting.Position, Position) / Game1.tileSize);
                    if (Vector2.Distance(targeting.Position, Position) < Game1.tileSize * 1.25f)
                    {
                        if (!hasSwung)
                        {
                            hasSwung = true;
                            dummy.Position = Position;
                            dummy.FacingDirection = FacingDirection;
                            foreach (var ench in dummy.enchantments)
                            {
                                (ench as BaseWeaponEnchantment).OnSwing(attackWeapon, dummy);
                            }
                            untilEndAction.Value = swingTime * 2 + 0.05f;
                        }

                        // TODO: Refactor when weapon stuff is implemented.
                        faceGeneralDirection(targeting.Position);
                        Vector2 t1 = Vector2.Zero, t2 = Vector2.Zero;
                        int pseudoframeCount = attackWeapon.type.Value == 1 ? 4 : 8;
                        int pseudoframe = (int)(untilEndAction.Value / (swingTime / pseudoframeCount));
                        Rectangle aoe = attackWeapon.getAreaOfEffect((int)Position.X, (int)Position.Y, FacingDirection, ref t1, ref t2, GetBoundingBox(), pseudoframe);
                        farmer.currentLocation.damageMonster(aoe, attackWeapon.minDamage.Value, attackWeapon.maxDamage.Value, false, attackWeapon.knockback.TargetValue, attackWeapon.addedPrecision.Value, attackWeapon.critChance.Value, attackWeapon.critMultiplier.Value, true, dummy);
                    }
                }
            }

            if (untilEndAction.Value > 0)
            {
                if (Game1.player == farmer)
                {
                    // Only want to update this for the player who owns the merc
                    untilEndAction.Value -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (untilEndAction.Value > 0)
                    goto afterUpdates;
                else
                {
                    hasSwung = false;
                }
            }

            var trail = MercenaryEngine.trails.GetOrCreateValue(farmer);
            int ind = Math.Min(trail.Count - 1, ( numInFormation + 1 ) * MercenaryEngine.TrailingDistance);
            Vector2 origTarget = targetPos = ind < 0 ? farmer.Position : trail[ind];
            //Console.WriteLine(targetPos);

            if (targeting != null && attackWeapon != null)
            {
                if (targeting.currentLocation == farmer.currentLocation)
                {
                    Vector2 away = Position - targeting.Position;
                    away.Normalize();

                    targetPos = targeting.Position + away * Game1.tileSize * 0.8f;
                    untilNextPathfind = 0;
                    if (!DoPathfind(farmer.currentLocation))
                    {
                        targetPos = origTarget;
                        untilNextPathfind = 0;
                    }
                }
                else
                    targeting = null;
            }

            float tposStepX = MathF.Round(targetPos.X / pathfindStep) * pathfindStep;
            float tposStepY = MathF.Round(targetPos.Y / pathfindStep) * pathfindStep;
            targetPos = new Vector2(tposStepX, tposStepY);

            Vector2 posDiff = targetPos - Position;
            int targetDir = -3;
            if (posDiff.Y < 0)
            {
                if (MathF.Abs(posDiff.X) < -posDiff.Y)
                    targetDir = 0;
                else if (posDiff.X < 0)
                    targetDir = 3;
                else if (posDiff.X > 0)
                    targetDir = 1;
            }
            else if (posDiff.Y > 0)
            {
                if (MathF.Abs(posDiff.X) < posDiff.Y)
                    targetDir = 2;
                else if (posDiff.X < 0)
                    targetDir = 3;
                else if (posDiff.X > 0)
                    targetDir = 1;
            }
            else
            {
                if (posDiff.X < 0)
                    targetDir = 3;
                else if (posDiff.X > 0)
                    targetDir = 1;
            }


            if (Game1.player == farmer)
            {
                faceDirection(targetDir);
                DoPathfind( farmer.currentLocation );

                if (Position != targetPos)
                {
                    if (lastPath?.Count > 0)
                    {
                        var lastPos = Position;
                        float speedLeft = Speed;
                        do
                        {
                            float dist = Vector2.Distance(Position, lastPath.Peek());
                            if (dist <= speedLeft)
                            {
                                speedLeft -= dist;
                                Position = lastPath.Dequeue();
                                /*
                                if (lastPath.Count <= 0)
                                    Position = targetPos;
                                */
                            }
                            else
                            {
                                var v = (lastPath.Peek() - Position);
                                v.Normalize();
                                //Console.WriteLine($"{v} {Position} " + string.Join( '=', lastPath.Select( v => v.ToString() )));
                                Position += v * speedLeft;
                                speedLeft = 0;
                            }

                            int facing = 0;
                            var diff = Position - lastPos;
                            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                            {
                                if (diff.X < 0)
                                    facing = 3;
                                else
                                    facing = 1;
                            }
                            else
                            {
                                if (diff.Y < 0)
                                    facing = 0;
                                else
                                    facing = 2;
                            }

                            FacingDirection = facing;
                        }
                        while (speedLeft > 0 && lastPath.Count > 0);

                        animateInFacingDirection(gameTime);
                    }
                    else
                    {
                        //Log.Trace("no path; teleporting");
                        //Position = targetPos;
                    }
                }

                untilNextAction.Value -= (float) gameTime.ElapsedGameTime.TotalSeconds;
                if (untilNextAction.Value <= 0)
                {
                    untilNextAction.Value = 0.1f;

                    foreach (var actions_ in data.ActionsByPriority)
                    {
                        var actions = actions_.ToList();
                        int total = actions.Sum(action => action.Weight);
                        while (actions.Count > 0)
                        {
                            if (Game1.random.Next(total) < actions[0].Weight)
                            {
                                if (MercenaryActionData.ActionTypes.TryGetValue(actions[0].ActionType, out var func) && CheckCooldowns( actions[ 0 ] ) && func( this, actions[0] ))
                                {
                                    if (!actions[0].ActionType.StartsWith("MeleeAttack"))
                                    {
                                        untilEndAction.Value = actions[0].ActionLength;
                                        foreach (var cooldown in actions[0].Cooldowns)
                                            currentCooldowns.Add(cooldown.Key, cooldown.Value);
                                    }
                                    goto executedAction;
                                }
                            }
                            total -= actions[0].Weight;
                            actions.Remove(actions[0]);
                        }
                    }
                executedAction:
                    ;
                }
            }
            afterUpdates:

            updateGlow();
            updateEmote(gameTime);
            
        }

        public override void draw(SpriteBatch b, float alpha = 1)
        {
            b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.GetSpriteWidthForPositioning() * 4 / 2, this.GetBoundingBox().Height / 2 - 16), this.Sprite.SourceRect, Color.White * alpha, 0, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.Scale) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
            if (this.IsEmoting)
            {
                Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 96f;
                b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.StandingPixel.Y / 10000f);
            }

            if (targeting != null && attackWeapon != null && untilEndAction.Value > 0)
            {
                int pseudoframeCount = attackWeapon.type.Value == 1 ? 4 : 8;
                int pseudoframe = (int)(untilEndAction.Value / (swingTime / pseudoframeCount));
                //Console.WriteLine("MEOW swing draw " + pseudoframe + " " + pseudoframeCount+" "+untilEndAction.Value + " " + swingTime);
                //Console.WriteLine("mine " + ((Game1.player.CurrentTool as MeleeWeapon)?.speed?.Value ?? -1));
                //Console.WriteLine("pos " + Position + " " + dummy.Position + " " + targeting?.Position+" "+targeting.Name);
                dummy.Position = Position;
                MeleeWeapon.drawDuringUse(pseudoframe, FacingDirection, b, Game1.GlobalToLocal(Position), dummy, attackWeapon.QualifiedItemId, attackWeapon.type, false);
            }

            /*

            var bb = GetBoundingBox();
            foreach (var e in encountered)
            {
                var ep = e.Key.ToPoint();
                b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport,new Rectangle(ep.X, ep.Y, 16, 16)), Color.LightGreen);
            }
            foreach (var e in encountered)
            {
                var ep = e.Key.ToPoint();
                b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Rectangle(ep.X, ep.Y, bb.Width, bb.Height)), Color.Green);
            }

            b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, GetBoundingBox()), Color.Red);
            dummy.Position = Position;
            var r = Game1.GlobalToLocal(Game1.viewport, dummy.GetBoundingBox());
            //r.Offset(0, 16);
            b.Draw(Game1.staminaRect, r, Color.Blue);
            //*/
        }

        internal bool CheckCooldowns(MercenaryActionData action)
        {
            foreach (var cooldown in action.Cooldowns)
            {
                if (currentCooldowns.ContainsKey(cooldown.Key))
                    return false;
            }

            return true;
        }

        internal void UpdateCooldowns(GameTime time)
        {
            foreach (var cooldown in currentCooldowns.ToList())
            {
                if (cooldown.Value < time.ElapsedGameTime.TotalSeconds)
                    currentCooldowns.Remove(cooldown.Key);
                else
                    currentCooldowns[cooldown.Key] -= (float) time.ElapsedGameTime.TotalSeconds;
            }
        }

        internal void ApplyAnimation(string tileSheet, int startFrame, float frameLength, int frameCount, int loops)
        {
            spriteStack.Push(Sprite);
            Sprite = new AnimatedSprite(tileSheet, startFrame, 16, 32);
            for (int i = 0; i < loops; ++i)
            {
                for ( int fi = 0; fi < frameLength; ++fi )
                    Sprite.AddFrame(new(startFrame + fi, (int)(frameLength * 1000)));
            }
        }

        private void CachePathfindSolidsIfNeeded( GameLocation loc )
        {
            // TODO: Redo this in a way that supports dynamic environment
            /*
            if (loc == lastPathfindLoc)
                return;
            */
            lastPathfindLoc = loc;

            int mapW = loc.Map.Layers[0].LayerSize.Width;
            int mapH = loc.Map.Layers[0].LayerSize.Height;
            var buildings = loc.Map.GetLayer("Buildings");
            pathfindSolids = new bool[mapW * mapH];
            for (int i = 0; i < pathfindSolids.Length; ++i)
            {
                int ix = i % mapW;
                int iy = i / mapW;
                Rectangle r = new Rectangle(ix * Game1.tileSize, iy * Game1.tileSize, Game1.tileSize, Game1.tileSize);

                bool val = false;

                if (!loc.isTilePassable(new(ix, iy)))
                    val = true;
                else if (loc.Objects.TryGetValue(new(ix, iy), out var obj) && !obj.isPassable())
                    val = true;
                else if (loc.terrainFeatures.TryGetValue(new(ix, iy), out var tf) && !tf.isPassable())
                    val = true;
                else
                {
                    foreach (var furn in loc.furniture)
                    {
                        if (!furn.isPassable() && furn.GetBoundingBox().Intersects(r))
                        {
                            val = true;
                            break;
                        }
                    }

                    if (!val)
                    {
                        foreach (var building in loc.buildings)
                        {
                            if (building.intersects(r))
                            {
                                val = true;
                                break;
                            }
                        }
                        if (!val)
                        {
                            foreach (var clump in loc.resourceClumps)
                            {
                                if (!clump.isPassable() && clump.getBoundingBox().Intersects(r))
                                {
                                    val = true;
                                    break;
                                }
                            }
                            if (!val)
                            {
                                var ltf = loc.getLargeTerrainFeatureAt(ix, iy);
                                if (ltf != null && !ltf.isPassable() && ltf.getBoundingBox().Intersects(r))
                                    val = true;
                            }
                        }
                    }
                }

                pathfindSolids[i] = val;
            }
        }

        internal bool DoPathfind(GameLocation loc)
        {
            // TODO: Optimize me PLEASE

            var oldpos = Position;
            if (untilNextPathfind > 0)
                return false;
            untilNextPathfind = 0.5f;

            lastPath?.Clear();

            CachePathfindSolidsIfNeeded(loc);
            int mapW = loc.Map.Layers[0].LayerSize.Width;
            int mapH = loc.Map.Layers[0].LayerSize.Height;
            // TODO: THE BRIDGE

            /*
            string str = "";
            for (int i = 0; i < solids.Length; ++i)
            {
                int ix = i % mapW;
                int iy = i / mapW;
                //if (ix == 0 && i != 0) { Console.WriteLine(str+"|"); str = ""; }
                str += solids[i] ? "1" : " ";

            }
            Console.WriteLine(str + "|");
            */

            encountered.Clear();
            PriorityQueue<Tuple<Vector2, Queue<Vector2>, float>, float> toCheck = new();
            Queue<Vector2> starterQueue = new();
            float posStepX = MathF.Round(Position.X / pathfindStep) * pathfindStep;
            float posStepY = MathF.Round(Position.Y / pathfindStep) * pathfindStep;
            //starterQueue.Enqueue(new( posStepX, posStepY ));
            toCheck.Enqueue(new(new Vector2(posStepX, posStepY), starterQueue, 0), 0);


            Rectangle rr = GetBoundingBox();
            rr.Offset(-Position + new Vector2(posStepX, posStepY));
            //rr.Offset(0, 16);
            rr.Inflate(-2, -2);
            //Console.WriteLine(rr);

            float foundDist = float.MaxValue;
            Queue<Vector2> found = null;
            float pathfindStepSq = pathfindStep * pathfindStep;
            float maxDistSq = Vector2.Distance(Position, targetPos) * 3;
            maxDistSq *= maxDistSq;
            float maxLen = Math.Max(1000, Vector2.Distance(Position, targetPos) * 5);
            while (toCheck.Count > 0)
            {
                var entry = toCheck.Dequeue();

                if (Vector2.DistanceSquared(targetPos, entry.Item1) < pathfindStepSq)
                {
                    if (entry.Item3 < foundDist)
                    {
                        foundDist = entry.Item3;
                        found = entry.Item2;
                    }
                }

                if (entry.Item3 >= maxLen /*||
                            Vector2.DistanceSquared(targetPos, entry.Item1) >= maxDistSq*/)
                    continue;

                void TryPosition(Vector2 orig, Vector2 nextOffset)
                {
                    Rectangle rect = GetBoundingBox();
                    rect.Offset(-Position + orig);
                    rect.Offset(0, -16);
                    rect.Offset(nextOffset);
                    //rect.Inflate(-2, -2);
                    rect = new(rect.X, rect.Y, rect.Width - 16, rect.Height);
                    Vector2 next = orig + nextOffset;

                    if (next.X < 0 || next.Y < 0 ||
                        next.X > loc.Map.DisplaySize.Width ||
                        next.Y > loc.Map.DisplaySize.Height)
                    {
                        return;
                    }

                    for (int ix = (rect.Left + 0) / 64; ix <= (rect.Right - 1) / 64; ++ix)
                    {
                        for (int iy = (rect.Top + 1) / 64; iy <= (rect.Bottom - 1) / 64; ++iy)
                        {
                            if (ix < 0 || iy < 0 || ix >= mapW || iy >= mapH ||
                                pathfindSolids[ix + iy * mapW])
                                return;
                        }
                    }

                    if (encountered.TryGetValue(next, out float existing2))
                    {
                        if (existing2 >= entry.Item3)
                        {
                            encountered[next] = entry.Item3;
                        }
                        return;
                    }

                    Queue<Vector2> steps = new(entry.Item2);
                    steps.Enqueue(next);
                    float newPriority = entry.Item3 + nextOffset.Length();
                    encountered.Add(next, newPriority);
                    toCheck.Enqueue(new(next, steps, newPriority), newPriority);
                }

                TryPosition(entry.Item1, new Vector2(-pathfindStep, 0));
                TryPosition(entry.Item1, new Vector2(+pathfindStep, 0));
                TryPosition(entry.Item1, new Vector2(0, -pathfindStep));
                TryPosition(entry.Item1, new Vector2(0, +pathfindStep));
                /*
                TryPosition(entry.Item1, new Vector2(-pathfindStepDiag, -pathfindStepDiag));
                TryPosition(entry.Item1, new Vector2(-pathfindStepDiag, +pathfindStepDiag));
                TryPosition(entry.Item1, new Vector2(+pathfindStepDiag, -pathfindStepDiag));
                TryPosition(entry.Item1, new Vector2(+pathfindStepDiag, +pathfindStepDiag));
                //*/
            }

            lastPath = found ?? null;

            Position = oldpos;

            return (lastPath?.Count ?? 0) > 0;
        }
    }
}
