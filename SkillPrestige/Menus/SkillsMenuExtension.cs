using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.InputHandling;
using SkillPrestige.Logging;
using SkillPrestige.Menus.Elements.Buttons;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus
{
    /// <summary>
    /// Extends the skills menu in Stardew Valley to add prestige buttons next to the skills.
    /// </summary>
    internal static class SkillsMenuExtension
    {
        private static bool _skillsMenuInitialized;
        private static readonly IList<TextureButton> PrestigeButtons = new List<TextureButton>();

        /// <summary>Get whether a skills page is supported for extension.</summary>
        /// <param name="skillsPage">The skills page to check.</param>
        private static bool IsSupportedSkillsPage(IClickableMenu skillsPage)
        {
            return
                skillsPage is SkillsPage // vanilla menu
                || skillsPage?.GetType().FullName == "SpaceCore.Interface.NewSkillsPage"; // SpaceCore (e.g. Luck Skill)
        }

        // ReSharper disable once SuggestBaseTypeForParameter - we specifically want the skills page here, even if our usage could work against IClickableMenu.
        private static void IntializeSkillsMenu(IClickableMenu skillsPage)
        {
            Logger.LogVerbose("Initializing Skills Menu...");
            _skillsMenuInitialized = true;
            PrestigeButtons.Clear();
            foreach (var skill in Skill.AllSkills)
            {
                var width = 8 * Game1.pixelZoom;
                var height = 8 * Game1.pixelZoom;
                var yOffset = (Game1.tileSize/2.5).Floor();
                var yPadding = (Game1.tileSize * 1.05).Floor();
                var xOffset = skillsPage.width + Game1.tileSize;
                var bounds = new Rectangle(skillsPage.xPositionOnScreen + xOffset, skillsPage.yPositionOnScreen + yPadding + yOffset * skill.SkillScreenPosition + skill.SkillScreenPosition * height, width, height);
                var prestigeButton = new TextureButton(bounds, SkillPrestigeMod.PrestigeIconTexture, new Rectangle(0, 0, 32, 32), () => OpenPrestigeMenu(skill), "Click to open the Prestige menu.");
                PrestigeButtons.Add(prestigeButton);
                Logger.LogVerbose($"{skill.Type.Name} skill prestige button initiated at {bounds.X}, {bounds.Y}. Width: {bounds.Width}, Height: {bounds.Height}");
            }
            Logger.LogVerbose("Skills Menu initialized.");
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        internal static void OnCursorMoved(CursorMovedEventArgs e)
        {
            foreach (var button in PrestigeButtons)
                button.OnCursorMoved(e);

        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        internal static void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            foreach (var button in PrestigeButtons)
                button.OnButtonPressed(e, isClick);
        }

        private static void UnloadSkillsPageAdditions()
        {
            Logger.LogVerbose("Unloading Skills Menu.");
            _skillsMenuInitialized = false;
            PrestigeButtons.Clear();
            Logger.LogVerbose("Skills Menu unloaded.");
        }

        public static void AddPrestigeButtonsToMenu()
        {
            var activeClickableMenu = Game1.activeClickableMenu as GameMenu;
            if (activeClickableMenu == null || activeClickableMenu.currentTab != 1)
            {
                if (_skillsMenuInitialized) UnloadSkillsPageAdditions();
            }
            else
            {
                IClickableMenu skillsPage = ((List<IClickableMenu>)activeClickableMenu.GetInstanceField("pages"))[1];
                if (IsSupportedSkillsPage(skillsPage))
                {
                    if (!_skillsMenuInitialized)
                        IntializeSkillsMenu(skillsPage);
                    var spriteBatch = Game1.spriteBatch;
                    DrawPrestigeButtons(spriteBatch);
                    DrawProfessionHoverText(spriteBatch, skillsPage);
                    DrawPrestigeButtonsHoverText(spriteBatch);
                    Mouse.DrawCursor(spriteBatch);
                }
            }
        }

        private static void DrawPrestigeButtons(SpriteBatch spriteBatch)
        {
            foreach (var prestigeButton in PrestigeButtons)
            {
                prestigeButton.Draw(spriteBatch);
            }
        }

        private static void DrawPrestigeButtonsHoverText(SpriteBatch spriteBatch)
        {
            foreach (var prestigeButton in PrestigeButtons)
            {
                prestigeButton.DrawHoverText(spriteBatch);
            }
        }

        private static void DrawProfessionHoverText(SpriteBatch spriteBatch, IClickableMenu skillsPage)
        {
            var hoverText = (string) skillsPage.GetInstanceField("hoverText");
            if (hoverText.Length <= 0) return;
            var hoverTitle = (string) skillsPage.GetInstanceField("hoverTitle");
            IClickableMenu.drawHoverText(spriteBatch, hoverText, Game1.smallFont, 0, 0, -1, hoverTitle.Length > 0 ? hoverTitle : null);
        }

        private static void OpenPrestigeMenu(Skill skill)
        {
            Logger.LogVerbose("Skills Menu - Setting up Prestige Menu...");
            var menuWidth = Game1.tileSize * 18;
            var menuHeight = Game1.tileSize * 10;

            var menuXCenter = (menuWidth + IClickableMenu.borderWidth * 2) / 2;
            var menuYCenter = (menuHeight + IClickableMenu.borderWidth * 2) / 2;
            var viewport = Game1.graphics.GraphicsDevice.Viewport;
            var screenXCenter = (int)(viewport.Width * (1.0 / Game1.options.zoomLevel)) / 2;
            var screenYCenter = (int)(viewport.Height * (1.0 / Game1.options.zoomLevel)) / 2;
            var bounds = new Rectangle(screenXCenter - menuXCenter, screenYCenter - menuYCenter, menuWidth + IClickableMenu.borderWidth*2, menuHeight + IClickableMenu.borderWidth*2);
            Game1.playSound("bigSelect");
            Logger.LogVerbose("Getting currently loaded prestige data...");
            var prestige = PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.Single(x => x.SkillType == skill.Type);
            Game1.activeClickableMenu = new PrestigeMenu(bounds, skill, prestige);
            Logger.LogVerbose("Skills Menu - Loaded Prestige Menu.");
        }
    }
}
