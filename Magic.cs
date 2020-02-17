using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using StardewModdingAPI.Events;
using Magic.Game.Interface;
using Magic.Schools;
using Magic.Spells;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static Magic.Mod;
using Magic.Other;
using StardewValley.Network;
using Newtonsoft.Json;
using System.IO;
using StardewModdingAPI;
using SpaceCore;
using StardewValley.Objects;
using SpaceShared;

namespace Magic
{
    // TODO: Refactor this mess
    static class Magic
    {
        public static Skill Skill;

        public static EventHandler<AnalyzeEventArgs> OnAnalyzeCast;

        private static IModEvents events;
        private static IInputHelper inputHelper;

        private static Texture2D spellBg;
        private static Texture2D manaBg;
        private static Texture2D manaFg;

        /// <summary>The active effects, spells, or projectiles which should be updated or drawn.</summary>
        private static readonly IList<IActiveEffect> activeEffects = new List<IActiveEffect>();

        public const string MSG_DATA = "spacechase0.Magic.Data";
        public const string MSG_MINIDATA = "spacechase0.Magic.MiniData";
        public const string MSG_CAST = "spacechase0.Magic.Cast";

        internal static void init(IModEvents events, IInputHelper inputHelper, Func<long> getNewId)
        {
            Magic.events = events;
            Magic.inputHelper = inputHelper;

            loadAssets();

            SpellBook.init(getNewId);

            events.GameLoop.SaveLoaded += onSaveLoaded;
            events.GameLoop.UpdateTicked += onUpdateTicked;

            events.Input.ButtonPressed += onButtonPressed;
            events.Input.ButtonReleased += onButtonReleased;
            
            events.GameLoop.DayStarted += onDayStarted;
            events.GameLoop.TimeChanged += onTimeChanged;
            events.Player.Warped += onWarped;
            
            SpaceEvents.OnBlankSave += onBlankSave;
            SpaceEvents.OnItemEaten += onItemEaten;
            SpaceEvents.ActionActivated += actionTriggered;
            SpaceCore.Networking.RegisterMessageHandler(MSG_DATA, onNetworkData);
            SpaceCore.Networking.RegisterMessageHandler(MSG_MINIDATA, onNetworkMiniData);
            SpaceCore.Networking.RegisterMessageHandler(MSG_CAST, onNetworkCast);
            SpaceEvents.ServerGotClient += onClientConnected;

            events.Display.RenderingHud += onRenderingHud;
            events.Display.RenderedHud += onRenderedHud;

            OnAnalyzeCast += onAnalyze;

            SpaceCore.Skills.RegisterSkill(Skill = new Skill());

            Command.register("player_addmana", addManaCommand);
            Command.register("player_setmaxmana", setMaxManaCommand);
            Command.register("player_learnspell", learnSpellCommand);
            Command.register("magicmenu", magicMenuCommand);

            PyTK.CustomTV.CustomTVMod.addChannel("magic", Mod.instance.Helper.Translation.Get("tv.analyzehints.name"), onTvChannelSelected);
        }

        private static void onAnalyze(object sender, AnalyzeEventArgs e)
        {
            var farmer = sender as Farmer;
            if (farmer != Game1.player)
                return;

            List<string> spellsLearnt = new List<string>();
            if ( farmer.CurrentItem != null )
            {
                if ( farmer.CurrentTool != null )
                {
                    if (farmer.CurrentTool is StardewValley.Tools.Axe || farmer.CurrentTool is StardewValley.Tools.Pickaxe)
                        spellsLearnt.Add("toil:cleardebris");
                    else if (farmer.CurrentTool is StardewValley.Tools.Hoe)
                        spellsLearnt.Add("toil:till");
                    else if (farmer.CurrentTool is StardewValley.Tools.WateringCan)
                        spellsLearnt.Add("toil:water");
                }
                else if ( farmer.CurrentItem is StardewValley.Objects.Boots )
                {
                    spellsLearnt.Add("life:evac");
                }
                else if ( farmer.ActiveObject != null )
                {
                    if ( !farmer.ActiveObject.bigCraftable.Value )
                    {
                        int index = farmer.ActiveObject.ParentSheetIndex;
                        if (index == 395) // Coffee
                            spellsLearnt.Add("life:haste");
                        else if (index == 773) // Life elixir
                            spellsLearnt.Add("life:heal");
                        else if (index == 86) // Earth crystal
                            spellsLearnt.Add("nature:shockwave");
                        else if (index == 82) // Fire quartz
                            spellsLearnt.Add("elemental:fireball");
                        else if (index == 161) // Ice Pip
                            spellsLearnt.Add("elemental:frostbolt");
                    }
                }
            }
            foreach ( var lightSource in farmer.currentLocation.sharedLights.Values )
            {
                if ( Utility.distance(e.TargetX, lightSource.position.X, e.TargetY, lightSource.position.Y) < lightSource.radius.Value * Game1.tileSize )
                {
                    spellsLearnt.Add("nature:lantern");
                    break;
                }
            }
            var tilePos = new Vector2(e.TargetX / Game1.tileSize, e.TargetY / Game1.tileSize);
            if ( farmer.currentLocation.terrainFeatures.ContainsKey(tilePos) && farmer.currentLocation.terrainFeatures[ tilePos ] is StardewValley.TerrainFeatures.HoeDirt hd )
            {
                if (hd.crop != null)
                    spellsLearnt.Add("nature:tendrils");
            }
            // TODO: Add proper tilesheet check
            var tile = farmer.currentLocation.map.GetLayer("Buildings").Tiles[(int)tilePos.X, (int)tilePos.Y];
            if (tile != null && tile.TileIndex == 173)
                spellsLearnt.Add("elemental:descend");
            if ( farmer.currentLocation is Farm farm )
            {
                foreach ( var clump in farm.resourceClumps )
                {
                    if (clump.parentSheetIndex.Value == 622 && new Rectangle((int)clump.tile.Value.X, (int)clump.tile.Value.Y, clump.width.Value, clump.height.Value).Contains((int)tilePos.X, (int)tilePos.Y))
                        spellsLearnt.Add("eldritch:meteor");
                }
            }
            if (farmer.currentLocation.doesTileHaveProperty((int)tilePos.X, (int)tilePos.Y, "Action", "Buildings") == "EvilShrineLeft")
                spellsLearnt.Add("eldritch:lucksteal");
            if (farmer.currentLocation is StardewValley.Locations.MineShaft ms && ms.mineLevel == 100 && ms.waterTiles[(int)tilePos.X, (int)tilePos.Y])
                spellsLearnt.Add("eldritch:bloodmana");

            for (int i = spellsLearnt.Count - 1; i >= 0; --i)
                if (farmer.knowsSpell(spellsLearnt[i], 0))
                    spellsLearnt.RemoveAt(i);
            if (spellsLearnt.Count > 0)
            {
                Game1.playSound("secret1");
                foreach (var spell in spellsLearnt)
                {
                    Log.debug("Player learnt spell: " + spell);
                    farmer.learnSpell(spell, 0, true);
                    //Game1.drawObjectDialogue(Mod.instance.Helper.Translation.Get("spell.learn", new { spellName = Mod.instance.Helper.Translation.Get("spell." + spell + ".name") }));
                    Game1.addHUDMessage(new HUDMessage(Mod.instance.Helper.Translation.Get("spell.learn", new { spellName = SpellBook.get(spell).getTranslatedName() })));
                }
            }

            // Temporary - 0.3.0 will add dungeons to get these
            bool knowsAll = true;
            foreach ( var schoolId in School.getSchoolList() )
            {
                var school = School.getSchool(schoolId);

                bool knowsAllSchool = true;
                foreach ( var spell in school.GetSpellsTier1() )
                {
                    if (!farmer.knowsSpell(spell, 0))
                    {
                        knowsAll = knowsAllSchool = false;
                        break;
                    }
                }
                foreach (var spell in school.GetSpellsTier2())
                {
                    if (!farmer.knowsSpell(spell, 0))
                    {
                        knowsAll = knowsAllSchool = false;
                        break;
                    }
                }

                // Have to know all other spells for the arcane one
                if (schoolId == SchoolId.Arcane)
                    continue;

                var ancientSpell = school.GetSpellsTier3()[0];
                if ( knowsAllSchool && !farmer.knowsSpell(ancientSpell, 0 ) )
                {
                    Log.debug("Player learnt ancient spell: " + ancientSpell);
                    farmer.learnSpell(ancientSpell, 0, true);
                    Game1.addHUDMessage(new HUDMessage(Mod.instance.Helper.Translation.Get("spell.learn.ancient", new { spellName = ancientSpell.getTranslatedName() })));
                }
            }

            var rewindSpell = School.getSchool( SchoolId.Arcane ).GetSpellsTier3()[0];
            if (knowsAll && !farmer.knowsSpell(rewindSpell, 0))
            {
                Log.debug("Player learnt ancient spell: " + rewindSpell);
                farmer.learnSpell(rewindSpell, 0, true);
                Game1.addHUDMessage(new HUDMessage(Mod.instance.Helper.Translation.Get("spell.learn.ancient", new { spellName = rewindSpell.getTranslatedName() })));
            }
        }

        private static void onNetworkData(IncomingMessage msg)
        {
            int count = msg.Reader.ReadInt32();
            for ( int i = 0; i < count; ++i )
            {
                Mod.Data.players[msg.Reader.ReadInt64()] = JsonConvert.DeserializeObject<MultiplayerSaveData.PlayerData>(msg.Reader.ReadString());
            }
        }

        private static void onNetworkMiniData(IncomingMessage msg)
        {
            Mod.Data.players[msg.FarmerID].mana = msg.Reader.ReadInt32();
            Mod.Data.players[msg.FarmerID].manaCap = msg.Reader.ReadInt32();
        }

        private static void onNetworkCast( IncomingMessage msg )
        {
            IActiveEffect effect = Game1.getFarmer(msg.FarmerID).castSpell(msg.Reader.ReadString(), msg.Reader.ReadInt32(), msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
            if (effect != null)
                activeEffects.Add(effect);
        }

        private static void onClientConnected(object sender, EventArgsServerGotClient args)
        {
            if ( !Data.players.ContainsKey( args.FarmerID ) )
                Data.players[args.FarmerID] = new MultiplayerSaveData.PlayerData();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((int)Data.players.Count);
                foreach (var entry in Data.players)
                {
                    writer.Write(entry.Key);
                    writer.Write(JsonConvert.SerializeObject(entry.Value, MultiplayerSaveData.networkSerializerSettings));
                }
                SpaceCore.Networking.BroadcastMessage(Magic.MSG_DATA, stream.ToArray());
            }
        }

        private static void onBlankSave( object sender, EventArgs args )
        {
            placeAltar(Mod.Config.AltarLocation, Mod.Config.AltarX, Mod.Config.AltarY, 54 * 4);
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public static void onSaveLoaded( object sender, SaveLoadedEventArgs e )
        {
            Data.players[ Game1.player.UniqueMultiplayerID ].spellBook.Owner = Game1.player;
            foreach ( var farmer in Game1.otherFarmers )
            {
                if (!Data.players.ContainsKey(farmer.Key))
                    continue;
                Data.players[farmer.Key].spellBook.Owner = farmer.Value;
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // update active effects
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                IActiveEffect effect = activeEffects[i];
                if (!effect.Update(e))
                    activeEffects.RemoveAt(i);
            }
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open). Content drawn to the sprite batch at this point will appear under the HUD.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onRenderingHud( object sender, RenderingHudEventArgs e )
        {
            // draw active effects
            foreach (IActiveEffect effect in activeEffects)
                effect.Draw(e.SpriteBatch);
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public static void onRenderedHud( object sender, RenderedHudEventArgs e )
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp || Game1.player.getMaxMana() == 0)
                return;

            SpriteBatch b = e.SpriteBatch;
            
            Vector2 manaPos = new Vector2(20, Game1.viewport.Height - manaBg.Height * 4 - 20);
            b.Draw(manaBg, manaPos, new Rectangle(0, 0, manaBg.Width, manaBg.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
            if (Game1.player.getCurrentMana() > 0)
            {
                Rectangle targetArea = new Rectangle(3, 13, 6, 41);
                float perc = Game1.player.getCurrentMana() / (float)Game1.player.getMaxMana();
                int h = (int)(targetArea.Height * perc);
                targetArea.Y += targetArea.Height - h;
                targetArea.Height = h;

                targetArea.X *= 4;
                targetArea.Y *= 4;
                targetArea.Width *= 4;
                targetArea.Height *= 4;
                targetArea.X += (int)manaPos.X;
                targetArea.Y += (int)manaPos.Y;
                b.Draw(manaFg, targetArea, new Rectangle(0, 0, 1, 1), Color.White);

                if ((double)Game1.getOldMouseX() >= (double)targetArea.X && (double)Game1.getOldMouseY() >= (double)targetArea.Y && (double)Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, (int)Game1.player.getCurrentMana()).ToString() + "/" + Game1.player.getMaxMana(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32));
            }

            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.ProfessionFifthSpellSlot);

            int spotYAffector = -1;
            if (hasFifthSpellSlot)
                spotYAffector = 0;
            Point[] spots =
            new Point[5]/*
            {
                new Point((int)manaPos.X + manaBg.Width * 4 + 20 + 40, Game1.viewport.Height - 20 - 50 - 30 - 50 - 25),
                new Point((int)manaPos.X + manaBg.Width * 4 + 20 + 50 + 30, Game1.viewport.Height - 20 - 50 - 40 - 25),
                new Point((int)manaPos.X + manaBg.Width * 4 + 20 + 40, Game1.viewport.Height - 20 - 50 - 25 ),
                new Point((int)manaPos.X + manaBg.Width * 4 + 20, Game1.viewport.Height - 20 - 50 - 40 - 25 ),
            };*/
            {
                new Point( (int)manaPos.X + manaBg.Width * 4 + 20 + 60 * 0, Game1.viewport.Height - 20 - 50 - 60 * ( 4 + spotYAffector ) ),
                new Point( (int)manaPos.X + manaBg.Width * 4 + 20 + 60 * 0, Game1.viewport.Height - 20 - 50 - 60 * ( 3 + spotYAffector ) ),
                new Point( (int)manaPos.X + manaBg.Width * 4 + 20 + 60 * 0, Game1.viewport.Height - 20 - 50 - 60 * ( 2 + spotYAffector ) ),
                new Point( (int)manaPos.X + manaBg.Width * 4 + 20 + 60 * 0, Game1.viewport.Height - 20 - 50 - 60 * ( 1 + spotYAffector ) ),
                new Point( (int)manaPos.X + manaBg.Width * 4 + 20 + 60 * 0, Game1.viewport.Height - 20 - 50 - 60 * ( 0 + spotYAffector ) ),
            };

            SpellBook book = Game1.player.getSpellBook();
            if (book == null || book.selectedPrepared >= book.prepared.Length)
                return;
            PreparedSpell[] prepared = book.getPreparedSpells();

            for (int i = 0; i < (hasFifthSpellSlot ? 5 : 4); ++i)
            {
                b.Draw(spellBg, new Rectangle(spots[i].X, spots[i].Y, 50, 50), Color.White);
            }

            string prepStr = (book.selectedPrepared + 1) + "/" + book.prepared.Length;
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 + 2, spots[Game1.up].Y - 35 + 0), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 - 2, spots[Game1.up].Y - 35 + 0), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 + 0, spots[Game1.up].Y - 35 + 2), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 + 0, spots[Game1.up].Y - 35 - 2), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25, spots[Game1.up].Y - 35), Color.White, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);

            for (int i = 0; i < (hasFifthSpellSlot ? 5 : 4); ++i)
            {
                if (i >= prepared.Length)
                    break;

                PreparedSpell prep = prepared[i];
                if (prep == null)
                    continue;

                Spell spell = SpellBook.get(prep.SpellId);
                if (spell == null || spell.Icons.Length <= prep.Level || spell.Icons[prep.Level] == null)
                    continue;
                
                b.Draw(spell.Icons[prep.Level], new Rectangle(spots[i].X, spots[i].Y, 50, 50), Game1.player.canCastSpell(spell, prep.Level) ? Color.White : new Color(128, 128, 128));
            }
        }

        private static void loadAssets()
        {
            spellBg = Content.loadTexture("interface/spellbg.png");
            manaBg = Content.loadTexture("interface/manabg.png");

            Color manaCol = new Color(0, 48, 255);
            manaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            manaFg.SetData(new Color[] { manaCol });
        }

        private static bool castPressed = false;

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.ProfessionFifthSpellSlot);

            if (e.Button == Config.Key_Cast)
            {
                castPressed = true;
            }

            if (Data == null || Game1.activeClickableMenu != null) return;
            if (e.Button == Config.Key_SwapSpells)
            {
                Game1.player.getSpellBook().swapPreparedSet();
            }
            else if (castPressed &&
                     (e.Button == Config.Key_Spell1 || e.Button == Config.Key_Spell2 ||
                      e.Button == Config.Key_Spell3 || e.Button == Config.Key_Spell4 ||
                      (e.Button == Config.Key_Spell5 && hasFifthSpellSlot)))
            {
                int slot = 0;
                if (e.Button == Config.Key_Spell1) slot = 0;
                else if (e.Button == Config.Key_Spell2) slot = 1;
                else if (e.Button == Config.Key_Spell3) slot = 2;
                else if (e.Button == Config.Key_Spell4) slot = 3;
                else if (e.Button == Config.Key_Spell5) slot = 4;

                Magic.inputHelper.Suppress(e.Button);

                SpellBook book = Game1.player.getSpellBook();
                PreparedSpell[] prepared = book.getPreparedSpells();
                if (prepared[slot] == null)
                    return;
                PreparedSpell prep = prepared[slot];

                Spell toCast = SpellBook.get(prep.SpellId);
                if (toCast == null)
                    return;

                //Log.trace("MEOW " + prep.SpellId + " " + prep.Level + " " + Game1.player.canCastSpell(toCast, prep.Level));
                if (Game1.player.canCastSpell(toCast, prep.Level))
                {
                    Log.trace("Casting " + prep.SpellId);

                    IActiveEffect effect = Game1.player.castSpell(toCast, prep.Level);
                    if (effect != null)
                        activeEffects.Add(effect);
                    Game1.player.addMana(-toCast.getManaCost(Game1.player, prep.Level));
                }
            }
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == Config.Key_Cast)
            {
                castPressed = false;
            }
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.addMana(Game1.player.getMaxMana());
        }

        private static float carryoverManaRegen = 0;
        private static void onTimeChanged(object sender, TimeChangedEventArgs e)
        {
            float manaRegen = Game1.player.GetCustomSkillLevel(Skill) / 2 + carryoverManaRegen;
            if (Game1.player.HasCustomProfession(Skill.ProfessionManaRegen2))
                manaRegen *= 3;
            else if (Game1.player.HasCustomProfession(Skill.ProfessionManaRegen1))
                manaRegen *= 2;

            Game1.player.addMana((int)manaRegen);
            carryoverManaRegen = manaRegen - (int)manaRegen;
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onWarped( object sender, WarpedEventArgs e )
        {
            if (!e.IsLocalPlayer)
                return;

            // update spells
            EvacSpell.onLocationChanged();

            // check events
            if ( e.NewLocation.Name == "WizardHouse" && !Game1.player.eventsSeen.Contains( 90001 ) &&
                 Game1.player.friendshipData.ContainsKey( "Wizard" ) && Game1.player.friendshipData[ "Wizard" ].Points > 750 )
            {
                string eventStr = "WizardSong/0 5/Wizard 8 5 0 farmer 8 15 0/move farmer 0 -8 0/speak Wizard \"{0}#$b#{1}#$b#{2}#$b#{3}#$b#{4}#$b#{5}#$b#{6}#$b#{7}#$b#{8}\"/textAboveHead Wizard \"{9}\"/pause 750/fade 750/end";
                eventStr = string.Format(eventStr, Mod.instance.Helper.Translation.Get("event.wizard.1"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.2"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.3"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.4"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.5"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.6"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.7"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.8"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.9"),
                                                   Mod.instance.Helper.Translation.Get("event.wizard.abovehead"));
                e.NewLocation.currentEvent = new Event(eventStr, 90001);
                Game1.eventUp = true;
                Game1.displayHUD = false;
                Game1.player.CanMove = false;
                Game1.player.showNotCarrying();

                Game1.player.AddCustomSkillExperience(Skill, Skill.ExperienceCurve[0]);
                Game1.player.addMana(Game1.player.getMaxMana());
                Game1.player.learnSpell("arcane:analyze", 0, true);
                Game1.player.learnSpell("arcane:magicmissle", 0, true);
                Game1.player.learnSpell("arcane:enchant", 0, true);
                Game1.player.learnSpell("arcane:disenchant", 0, true);
                Game1.player.eventsSeen.Add(90001);
            }
        }

        private static void actionTriggered(object sender, EventArgsAction args)
        {
            string[] actionArgs = args.ActionString.Split(' ');
            if (args.Action == "MagicAltar")
            {
                if ( !Game1.player.eventsSeen.Contains(90001) )
                {
                    Game1.drawObjectDialogue(Mod.instance.Helper.Translation.Get("altar.glow"));
                }
                else
                {
                    Game1.playSound("secret1");
                    Game1.activeClickableMenu = new MagicMenu();// School.getSchool(actionArgs[1]));
                }
            }
        }

        private static void onItemEaten(object sender, EventArgs args)
        {
            if (Game1.player.itemToEat == null)
            {
                Log.warn("No item eaten for the item eat event?!?");
                return;
            }
            if (Game1.player.itemToEat.ParentSheetIndex == ja.GetObjectId("Magic Elixir"))
                Game1.player.addMana(Game1.player.getMaxMana());
        }
        
        private static void onTvChannelSelected(TV tv, TemporaryAnimatedSprite sprite, Farmer farmer, string answer)
        {
            TemporaryAnimatedSprite tas = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 305, 42, 28), 150f, 2, 999999, tv.getScreenPosition(), false, false, (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);

            string transKey = "tv.analyzehints.notmagical";
            Random r = new Random((int)Game1.stats.DaysPlayed + (int)(Game1.uniqueIDForThisGame / 2));
            if (Game1.player.getMaxMana() > 0)
                transKey = "tv.analyzehints." + (r.Next(12) + 1);

            PyTK.CustomTV.CustomTVMod.showProgram(tas, Mod.instance.Helper.Translation.Get(transKey));
        }

        public static void placeAltar(string locName, int x, int y, int baseAltarIndex)
        {
            Log.debug($"Placing altar @ {locName}({x}, {y})");

            // AddTileSheet sorts the tilesheets by ID after adding them.
            // The game sometimes refers to tilesheets by their index (such as in Beach.fixBridge)
            // Prepending this to the ID should ensure that this tilesheet is added to the end,
            // which preserves the normal indices of the tilesheets.
            char comeLast = '\u03a9'; // Omega

            GameLocation loc = Game1.getLocationFromName(locName);

            Dictionary<int, SpaceCore.Content.TileAnimation> anims;
            var tileSheet = Content.loadTilesheet("altarsobjects", loc.Map, out anims);
            tileSheet.Id = comeLast + tileSheet.Id;
            loc.map.AddTileSheet(tileSheet);
            if (Game1.currentLocation == loc)
                loc.map.LoadTileSheets(Game1.mapDisplayDevice);

            var front = loc.Map.GetLayer("Front");
            var buildings = loc.Map.GetLayer("Buildings");

            front.Tiles[x + 0, y - 1] = anims[baseAltarIndex + 0 + 0 * 18].makeTile(tileSheet, front);
            front.Tiles[x + 1, y - 1] = anims[baseAltarIndex + 1 + 0 * 18].makeTile(tileSheet, front);
            front.Tiles[x + 2, y - 1] = anims[baseAltarIndex + 2 + 0 * 18].makeTile(tileSheet, front);
            buildings.Tiles[x + 0, y + 0] = anims[baseAltarIndex + 0 + 1 * 18].makeTile(tileSheet, buildings);
            buildings.Tiles[x + 1, y + 0] = anims[baseAltarIndex + 1 + 1 * 18].makeTile(tileSheet, buildings);
            buildings.Tiles[x + 2, y + 0] = anims[baseAltarIndex + 2 + 1 * 18].makeTile(tileSheet, buildings);
            buildings.Tiles[x + 0, y + 1] = anims[baseAltarIndex + 0 + 2 * 18].makeTile(tileSheet, buildings);
            buildings.Tiles[x + 1, y + 1] = anims[baseAltarIndex + 1 + 2 * 18].makeTile(tileSheet, buildings);
            buildings.Tiles[x + 2, y + 1] = anims[baseAltarIndex + 2 + 2 * 18].makeTile(tileSheet, buildings);
            loc.setTileProperty(x + 0, y + 0, "Buildings", "Action", "MagicAltar");
            loc.setTileProperty(x + 1, y + 0, "Buildings", "Action", "MagicAltar");
            loc.setTileProperty(x + 2, y + 0, "Buildings", "Action", "MagicAltar");
            loc.setTileProperty(x + 0, y + 1, "Buildings", "Action", "MagicAltar");
            loc.setTileProperty(x + 1, y + 1, "Buildings", "Action", "MagicAltar");
            loc.setTileProperty(x + 2, y + 1, "Buildings", "Action", "MagicAltar");
        }

        private static void addManaCommand(string[] args)
        {
            Game1.player.addMana(int.Parse(args[0]));
        }
        private static void setMaxManaCommand(string[] args)
        {
            Game1.player.setMaxMana(int.Parse(args[0]));
        }
        private static void learnSpellCommand(string[] args)
        {
            if (args.Length == 1 && args[0] == "all")
            {
                foreach (var spellName in SpellBook.getAll())
                    Game1.player.learnSpell(SpellBook.get(spellName), SpellBook.get(spellName).getMaxCastingLevel(), true);
                return;
            }

            if (args.Length != 2 || (args.Length > 1 && (args[0] == "" || args[1] == "")))
            {
                Log.info("Usage: player_learnspell <spell> <level>");
                return;
            }

            Spell spell = SpellBook.get(args[0]);
            if (spell == null)
            {
                Log.error($"Spell '{args[0]}' does not exist.");
                return;
            }

            int level;
            if (!Int32.TryParse(args[1], out level))
            {
                Log.error($"That spell only casts up to level {spell.getMaxCastingLevel()}.");
                return;
            }

            Game1.player.learnSpell(spell, level, true);
        }
        private static void magicMenuCommand(string[] args)
        {
            Game1.activeClickableMenu = new MagicMenu();
        }
    }
}
