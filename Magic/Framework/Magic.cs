using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Framework.Game.Interface;
using Magic.Framework.Integrations;
using Magic.Framework.Schools;
using Magic.Framework.Skills;
using Magic.Framework.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Magic.Framework
{
    // TODO: Refactor this mess
    internal static class Magic
    {
        /*********
        ** Fields
        *********/
        private static Texture2D SpellBg;
        private static Texture2D ManaBg;
        private static Texture2D ManaFg;
        private static IInputHelper InputHelper;
        private static bool CastPressed;
        private static float CarryoverManaRegen;

        /// <summary>The active effects, spells, or projectiles which should be updated or drawn.</summary>
        private static readonly IList<IActiveEffect> ActiveEffects = new List<IActiveEffect>();

        /// <summary>The self-updating views of magic metadata for each player.</summary>
        /// <remarks>This should only be accessed through <see cref="GetSpellBook"/> or <see cref="Extensions.GetSpellBook"/> to make sure an updated instance is retrieved.</remarks>
        private static readonly IDictionary<long, SpellBook> SpellBookCache = new Dictionary<long, SpellBook>();


        /*********
        ** Accessors
        *********/
        public static Skill Skill;
        public static EventHandler<AnalyzeEventArgs> OnAnalyzeCast;
        public const string MsgCast = "spacechase0.Magic.Cast";


        /*********
        ** Public methods
        *********/
        public static void Init(IModEvents events, IInputHelper inputHelper, IModRegistry modRegistry, Func<long> getNewId)
        {
            Magic.InputHelper = inputHelper;

            Magic.LoadAssets();

            SpellManager.Init(getNewId);

            events.GameLoop.UpdateTicked += Magic.OnUpdateTicked;

            events.Input.ButtonPressed += Magic.OnButtonPressed;
            events.Input.ButtonReleased += Magic.OnButtonReleased;

            events.GameLoop.TimeChanged += Magic.OnTimeChanged;
            events.Player.Warped += Magic.OnWarped;

            SpaceEvents.OnItemEaten += Magic.OnItemEaten;
            SpaceEvents.ActionActivated += Magic.ActionTriggered;
            Networking.RegisterMessageHandler(Magic.MsgCast, Magic.OnNetworkCast);

            events.Display.RenderingHud += Magic.OnRenderingHud;
            events.Display.RenderedHud += Magic.OnRenderedHud;

            Magic.OnAnalyzeCast += Magic.OnAnalyze;
            Magic.OnAnalyzeCast += (sender, e) =>
            {
                Mod.Instance.Api.InvokeOnAnalyzeCast(sender as Farmer);
            };

            SpaceCore.Skills.RegisterSkill(Magic.Skill = new Skill());

            // add TV channel
            new PyTkChannelManager(modRegistry).AddTvChannel();
        }

        /// <summary>Get a self-updating view of a player's magic metadata.</summary>
        /// <param name="player">The player whose spell book to get.</param>
        public static SpellBook GetSpellBook(Farmer player)
        {
            if (!Magic.SpellBookCache.TryGetValue(player.UniqueMultiplayerID, out SpellBook book) || !object.ReferenceEquals(player, book.Player))
                Magic.SpellBookCache[player.UniqueMultiplayerID] = book = new SpellBook(player);

            return book;
        }

        /// <summary>Fix the player's magic spells and mana pool to match their skill level if needed.</summary>
        /// <param name="player">The player to fix.</param>
        /// <param name="overrideMagicLevel">The magic skill level, or <c>null</c> to get it from the player.</param>
        public static void FixMagicIfNeeded(Farmer player, int? overrideMagicLevel = null)
        {
            // skip if player hasn't learned magic
            if (!Game1.player.eventsSeen.Contains(MagicConstants.LearnedMagicEventId) && overrideMagicLevel is not > 0)
                return;

            // get magic info
            int magicLevel = overrideMagicLevel ?? player.GetCustomSkillLevel(Skill.MagicSkillId);
            SpellBook spellBook = Game1.player.GetSpellBook();

            // fix mana pool
            {
                int expectedPoints = magicLevel * MagicConstants.ManaPointsPerLevel;
                if (player.GetMaxMana() < expectedPoints)
                {
                    player.SetMaxMana(expectedPoints);
                    player.AddMana(expectedPoints);
                }
            }

            // fix spell bars
            if (spellBook.Prepared.Count < MagicConstants.SpellBarCount)
            {
                spellBook.Mutate(data =>
                {
                    while (spellBook.Prepared.Count < MagicConstants.SpellBarCount)
                        data.Prepared.Add(new PreparedSpellBar());
                });
            }

            // fix learned spells
            foreach (string spellId in new[] { "arcane:analyze", "arcane:magicmissle", "arcane:enchant", "arcane:disenchant" })
                spellBook.LearnSpell(spellId, 0, true);
        }


        /*********
        ** Private methods
        *********/
        private static void OnAnalyze(object sender, AnalyzeEventArgs e)
        {
            var farmer = (Farmer)sender;
            if (farmer != Game1.player)
                return;

            SpellBook spellBook = farmer.GetSpellBook();
            List<string> spellsLearnt = new();
            // ReSharper disable twice PossibleLossOfFraction
            Vector2 tilePos = new(e.TargetX / Game1.tileSize, e.TargetY / Game1.tileSize);

            // items
            foreach (var activeItem in new[] { Magic.GetItemFromMenu(Game1.activeClickableMenu) ?? Magic.GetItemFromToolbar(), farmer.CurrentItem })
            {
                if (activeItem is not null)
                {
                    // by item type
                    switch (activeItem)
                    {
                        case Axe or Pickaxe:
                            spellsLearnt.Add("toil:cleardebris");
                            break;

                        case Hoe:
                            spellsLearnt.Add("toil:till");
                            break;

                        case WateringCan:
                            spellsLearnt.Add("toil:water");
                            break;

                        case Boots:
                            spellsLearnt.Add("life:evac");
                            break;
                    }

                    // by item ID
                    if (activeItem is SObject activeObj && activeItem.GetType() == typeof(SObject) && !activeObj.bigCraftable.Value)
                    {
                        switch (activeItem.ParentSheetIndex)
                        {
                            case 395: // coffee
                                spellsLearnt.Add("life:haste");
                                break;

                            case 773: // life elixir
                                spellsLearnt.Add("life:heal");
                                break;

                            case 86: // earth crystal
                                spellsLearnt.Add("nature:shockwave");
                                break;

                            case 82: // fire quartz
                                spellsLearnt.Add("elemental:fireball");
                                break;

                            case 161: // ice pip
                                spellsLearnt.Add("elemental:frostbolt");
                                break;
                        }
                    }
                }
            }

            // light sources
            foreach (var lightSource in farmer.currentLocation.sharedLights.Values)
            {
                if (Utility.distance(e.TargetX, lightSource.position.X, e.TargetY, lightSource.position.Y) < lightSource.radius.Value * Game1.tileSize)
                {
                    spellsLearnt.Add("nature:lantern");
                    break;
                }
            }

            // terrain features
            {
                if (farmer.currentLocation.terrainFeatures.TryGetValue(tilePos, out TerrainFeature feature) && (feature as HoeDirt)?.crop != null)
                    spellsLearnt.Add("nature:tendrils");

                foreach (ResourceClump clump in farmer.currentLocation.resourceClumps)
                {
                    if (clump.parentSheetIndex.Value == ResourceClump.meteoriteIndex && new Rectangle((int)clump.tile.Value.X, (int)clump.tile.Value.Y, clump.width.Value, clump.height.Value).Contains((int)tilePos.X, (int)tilePos.Y))
                    {
                        spellsLearnt.Add("eldritch:meteor");
                        break;
                    }
                }
            }

            // map tile
            {
                // TODO: Add proper tilesheet check
                var tile = farmer.currentLocation.map.GetLayer("Buildings").Tiles[(int)tilePos.X, (int)tilePos.Y];
                if (tile?.TileIndex == 173)
                    spellsLearnt.Add("elemental:descend");

                if (farmer.currentLocation.doesTileHaveProperty((int)tilePos.X, (int)tilePos.Y, "Action", "Buildings") == "EvilShrineLeft")
                    spellsLearnt.Add("eldritch:lucksteal");
                if (farmer.currentLocation is StardewValley.Locations.MineShaft { mineLevel: 100 } ms && ms.waterTiles[(int)tilePos.X, (int)tilePos.Y])
                    spellsLearnt.Add("eldritch:bloodmana");
            }

            // learn spells
            bool learnedAny = false;
            foreach (string spell in spellsLearnt)
            {
                if (spellBook.KnowsSpell(spell, 0))
                    continue;

                if (!learnedAny)
                {
                    Game1.playSound("secret1");
                    learnedAny = true;
                }

                Log.Debug($"Player learnt spell: {spell}");
                spellBook.LearnSpell(spell, 0, true);
                Game1.addHUDMessage(new HUDMessage(I18n.Spell_Learn(spellName: SpellManager.Get(spell).GetTranslatedName())));
            }

            // learn hidden spell if players knows all of the other spells for a school
            // TODO: add dungeons to get these
            {
                bool knowsAll = true;
                foreach (string schoolId in School.GetSchoolList())
                {
                    var school = School.GetSchool(schoolId);

                    bool knowsAllSchool = true;
                    foreach (var spell in school.GetSpellsTier1())
                    {
                        if (!spellBook.KnowsSpell(spell, 0))
                        {
                            knowsAll = knowsAllSchool = false;
                            break;
                        }
                    }
                    foreach (var spell in school.GetSpellsTier2())
                    {
                        if (!spellBook.KnowsSpell(spell, 0))
                        {
                            knowsAll = knowsAllSchool = false;
                            break;
                        }
                    }

                    // Have to know all other spells for the arcane one
                    if (schoolId == SchoolId.Arcane)
                        continue;

                    var ancientSpell = school.GetSpellsTier3()[0];
                    if (knowsAllSchool && !spellBook.KnowsSpell(ancientSpell, 0))
                    {
                        Log.Debug("Player learnt ancient spell: " + ancientSpell);
                        spellBook.LearnSpell(ancientSpell, 0, true);
                        Game1.addHUDMessage(new HUDMessage(I18n.Spell_Learn_Ancient(spellName: ancientSpell.GetTranslatedName())));
                    }
                }

                var rewindSpell = School.GetSchool(SchoolId.Arcane).GetSpellsTier3()[0];
                if (knowsAll && !spellBook.KnowsSpell(rewindSpell, 0))
                {
                    Log.Debug("Player learnt ancient spell: " + rewindSpell);
                    spellBook.LearnSpell(rewindSpell, 0, true);
                    Game1.addHUDMessage(new HUDMessage(I18n.Spell_Learn_Ancient(spellName: rewindSpell.GetTranslatedName())));
                }
            }
        }

        /// <summary>Get the hovered item from an arbitrary menu.</summary>
        /// <param name="menu">The menu whose hovered item to find.</param>
        private static Item GetItemFromMenu(IClickableMenu menu)
        {
            //
            // Copied from CJB Show Item Sell Price by CJBok and Pathoschild: https://github.com/CJBok/SDV-Mods
            // Released under the MIT License.
            //

            var reflection = Mod.Instance.Helper.Reflection;

            // game menu
            if (menu is GameMenu gameMenu)
            {
                IClickableMenu page = reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[gameMenu.currentTab];
                if (page is InventoryPage)
                    return reflection.GetField<Item>(page, "hoveredItem").GetValue();
                if (page is CraftingPage)
                    return reflection.GetField<Item>(page, "hoverItem").GetValue();
            }

            // from inventory UI
            else if (menu is MenuWithInventory inventoryMenu)
                return inventoryMenu.hoveredItem;

            return null;
        }

        /// <summary>Get the hovered item from the on-screen toolbar.</summary>
        private static Item GetItemFromToolbar()
        {
            //
            // Derived from CJB Show Item Sell Price by CJBok and Pathoschild: https://github.com/CJBok/SDV-Mods
            // Released under the MIT License.
            //

            var reflection = Mod.Instance.Helper.Reflection;

            // get toolbar
            if (Game1.activeClickableMenu is not null)
                return null;
            Toolbar toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            var toolbarSlots = toolbar != null ? reflection.GetField<List<ClickableComponent>>(toolbar, "buttons").GetValue() : null;
            if (toolbarSlots is null)
                return null;

            // find hovered slot
            int x = Game1.getMouseX();
            int y = Game1.getMouseY();
            ClickableComponent hoveredSlot = toolbarSlots.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (hoveredSlot == null)
                return null;

            // get inventory index
            int index = toolbarSlots.IndexOf(hoveredSlot);
            if (index < 0 || index > Game1.player.Items.Count - 1)
                return null;

            // get hovered item
            return Game1.player.Items[index];
        }

        private static void OnNetworkCast(IncomingMessage msg)
        {
            Farmer player = Game1.getFarmer(msg.FarmerID);
            if (player == null)
                return;

            IActiveEffect effect = player.GetSpellBook().CastSpell(msg.Reader.ReadString(), msg.Reader.ReadInt32(), msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
            if (effect != null)
                Magic.ActiveEffects.Add(effect);
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // update active effects
            for (int i = Magic.ActiveEffects.Count - 1; i >= 0; i--)
            {
                IActiveEffect effect = Magic.ActiveEffects[i];
                if (!effect.Update(e))
                    Magic.ActiveEffects.RemoveAt(i);
            }
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open). Content drawn to the sprite batch at this point will appear under the HUD.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            // draw active effects
            foreach (IActiveEffect effect in Magic.ActiveEffects)
                effect.Draw(e.SpriteBatch);
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp || !Game1.player.eventsSeen.Contains(MagicConstants.LearnedMagicEventId))
                return;

            SpriteBatch b = e.SpriteBatch;

            Vector2 manaPos = new Vector2(20, Game1.uiViewport.Height - 56 * 4 - 20);

            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.MemoryProfession);

            int spotYAffector = -1;
            if (hasFifthSpellSlot)
                spotYAffector = 0;
            Point[] spots =
            {
                new((int)manaPos.X + Magic.ManaBg.Width * 4 + 20 + 60 * 0, Game1.uiViewport.Height - 20 - 50 - 60 * ( 4 + spotYAffector )),
                new((int)manaPos.X + Magic.ManaBg.Width * 4 + 20 + 60 * 0, Game1.uiViewport.Height - 20 - 50 - 60 * ( 3 + spotYAffector )),
                new((int)manaPos.X + Magic.ManaBg.Width * 4 + 20 + 60 * 0, Game1.uiViewport.Height - 20 - 50 - 60 * ( 2 + spotYAffector )),
                new((int)manaPos.X + Magic.ManaBg.Width * 4 + 20 + 60 * 0, Game1.uiViewport.Height - 20 - 50 - 60 * ( 1 + spotYAffector )),
                new((int)manaPos.X + Magic.ManaBg.Width * 4 + 20 + 60 * 0, Game1.uiViewport.Height - 20 - 50 - 60 * ( 0 + spotYAffector ))
            };

            // read spell info
            SpellBook spellBook = Game1.player.GetSpellBook();
            PreparedSpellBar prepared = spellBook.GetPreparedSpells();
            if (prepared == null)
                return;

            // render empty spell slots
            for (int i = 0; i < (hasFifthSpellSlot ? 5 : 4); ++i)
                b.Draw(Magic.SpellBg, new Rectangle(spots[i].X, spots[i].Y, 50, 50), Color.White);

            // render spell bar count
            string prepStr = (spellBook.SelectedPrepared + 1) + "/" + spellBook.Prepared.Count;
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 + 2, spots[Game1.up].Y - 35 + 0), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 - 2, spots[Game1.up].Y - 35 + 0), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 + 0, spots[Game1.up].Y - 35 + 2), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25 + 0, spots[Game1.up].Y - 35 - 2), Color.Black, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, prepStr, new Vector2(spots[Game1.down].X + 25, spots[Game1.up].Y - 35), Color.White, 0, new Vector2(Game1.dialogueFont.MeasureString(prepStr).X / 2, 0), 0.6f, SpriteEffects.None, 0);

            // render spell bar
            string hoveredText = null;
            for (int i = 0; i < (hasFifthSpellSlot ? 5 : 4) && i < prepared.Spells.Count; ++i)
            {
                PreparedSpell prep = prepared.GetSlot(i);
                if (prep == null)
                    continue;

                Spell spell = SpellManager.Get(prep.SpellId);
                if (spell == null || spell.Icons.Length <= prep.Level || spell.Icons[prep.Level] == null)
                    continue;

                Rectangle bounds = new Rectangle(spots[i].X, spots[i].Y, 50, 50);

                b.Draw(spell.Icons[prep.Level], bounds, spellBook.CanCastSpell(spell, prep.Level) ? Color.White : new Color(128, 128, 128));
                if (bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    hoveredText = spell.GetTooltip(level: prep.Level);
            }

            // render hover text
            if (hoveredText != null)
                StardewValley.Menus.IClickableMenu.drawHoverText(b, hoveredText, Game1.smallFont);
        }

        private static void LoadAssets()
        {
            Magic.SpellBg = Content.LoadTexture("interface/spellbg.png");
            Magic.ManaBg = Content.LoadTexture("interface/manabg.png");

            Color manaCol = new Color(0, 48, 255);
            Magic.ManaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Magic.ManaFg.SetData(new[] { manaCol });
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            bool hasFifthSpellSlot = Game1.player.HasCustomProfession(Skill.MemoryProfession);
            bool hasMenuOpen = Game1.activeClickableMenu is not null;

            if (e.Button == Mod.Config.Key_Cast)
                Magic.CastPressed = true;

            if (Magic.CastPressed && e.Button == Mod.Config.Key_SwapSpells && !hasMenuOpen)
            {
                Game1.player.GetSpellBook().SwapPreparedSet();
                Magic.InputHelper.Suppress(e.Button);
            }
            else if (Magic.CastPressed &&
                     (e.Button == Mod.Config.Key_Spell1 || e.Button == Mod.Config.Key_Spell2 ||
                      e.Button == Mod.Config.Key_Spell3 || e.Button == Mod.Config.Key_Spell4 ||
                      (e.Button == Mod.Config.Key_Spell5 && hasFifthSpellSlot)))
            {
                int slotIndex = 0;
                if (e.Button == Mod.Config.Key_Spell1) slotIndex = 0;
                else if (e.Button == Mod.Config.Key_Spell2) slotIndex = 1;
                else if (e.Button == Mod.Config.Key_Spell3) slotIndex = 2;
                else if (e.Button == Mod.Config.Key_Spell4) slotIndex = 3;
                else if (e.Button == Mod.Config.Key_Spell5) slotIndex = 4;

                Magic.InputHelper.Suppress(e.Button);

                SpellBook spellBook = Game1.player.GetSpellBook();

                PreparedSpellBar prepared = spellBook.GetPreparedSpells();
                PreparedSpell slot = prepared?.GetSlot(slotIndex);
                if (slot == null)
                    return;

                Spell spell = SpellManager.Get(slot.SpellId);
                if (spell == null)
                    return;

                bool canCast =
                    spellBook.CanCastSpell(spell, slot.Level)
                    && (!hasMenuOpen || spell.CanCastInMenus);

                if (canCast)
                {
                    Log.Trace("Casting " + slot.SpellId);

                    IActiveEffect effect = spellBook.CastSpell(spell, slot.Level);
                    if (effect != null)
                        Magic.ActiveEffects.Add(effect);
                    Game1.player.AddMana(-spell.GetManaCost(Game1.player, slot.Level));
                }
            }
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == Mod.Config.Key_Cast)
            {
                Magic.CastPressed = false;
            }
        }

        private static void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            float manaRegen = (Game1.player.GetCustomSkillLevel(Magic.Skill) + 1) / 2 + Magic.CarryoverManaRegen; // start at +1 mana at level 1
            if (Game1.player.HasCustomProfession(Skill.ManaRegen2Profession))
                manaRegen *= 3;
            else if (Game1.player.HasCustomProfession(Skill.ManaRegen1Profession))
                manaRegen *= 2;

            Game1.player.AddMana((int)manaRegen);
            Magic.CarryoverManaRegen = manaRegen - (int)manaRegen;
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            // update spells
            EvacSpell.OnLocationChanged();

            // check events
            if (e.NewLocation.Name == "WizardHouse" && !Game1.player.eventsSeen.Contains(MagicConstants.LearnedMagicEventId) && Game1.player.friendshipData.TryGetValue("Wizard", out Friendship wizardFriendship) && wizardFriendship.Points >= 750)
            {
                string eventStr = "WizardSong/0 5/Wizard 8 5 0 farmer 8 15 0/skippable/ignoreCollisions farmer/move farmer 0 -8 0/speak Wizard \"{0}#$b#{1}#$b#{2}#$b#{3}#$b#{4}#$b#{5}#$b#{6}#$b#{7}#$b#{8}\"/textAboveHead Wizard \"{9}\"/pause 750/fade 750/end";
                eventStr = string.Format(
                    eventStr,
                    I18n.Event_Wizard_1(),
                    I18n.Event_Wizard_2(),
                    I18n.Event_Wizard_3(),
                    I18n.Event_Wizard_4(),
                    I18n.Event_Wizard_5(),
                    I18n.Event_Wizard_6(),
                    I18n.Event_Wizard_7(),
                    I18n.Event_Wizard_8(),
                    I18n.Event_Wizard_9(),
                    I18n.Event_Wizard_Abovehead()
                );
                e.NewLocation.currentEvent = new Event(eventStr, MagicConstants.LearnedMagicEventId);
                Game1.eventUp = true;
                Game1.displayHUD = false;
                Game1.player.CanMove = false;
                Game1.player.showNotCarrying();

                Game1.player.AddCustomSkillExperience(Magic.Skill, Magic.Skill.ExperienceCurve[0]);
                Magic.FixMagicIfNeeded(Game1.player, overrideMagicLevel: 1); // let player start using magic immediately
                Game1.player.eventsSeen.Add(MagicConstants.LearnedMagicEventId);
            }
        }

        private static void ActionTriggered(object sender, EventArgsAction args)
        {
            if (args.Action == "MagicAltar")
            {
                if (!Game1.player.eventsSeen.Contains(MagicConstants.LearnedMagicEventId))
                {
                    Game1.drawObjectDialogue(I18n.Altar_ClickMessage());
                }
                else
                {
                    Game1.playSound("secret1");
                    Game1.activeClickableMenu = new MagicMenu();
                }
            }
        }

        private static void OnItemEaten(object sender, EventArgs args)
        {
            if (Game1.player.itemToEat == null)
            {
                Log.Warn("No item eaten for the item eat event?!?");
                return;
            }
            if (Game1.player.itemToEat.ParentSheetIndex == Mod.Ja.GetObjectId("Magic Elixir"))
                Game1.player.AddMana(Game1.player.GetMaxMana());
        }
    }
}
