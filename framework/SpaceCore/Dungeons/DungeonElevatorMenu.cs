using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley;
using Microsoft.Xna.Framework;
using SpaceShared;

namespace SpaceCore.Dungeons
{
    public class DungeonElevatorMenu : IClickableMenu
    {
        private readonly string dungeonId;

        public List<ClickableComponent> elevators = new List<ClickableComponent>();

        public DungeonElevatorMenu(string dungeonId)
            : base(0, 0, 0, 0, showUpperRightCloseButton: true)
        {
            this.dungeonId = dungeonId;
            var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
            dungeonsData.TryGetValue(dungeonId, out var dungeonData);

            int deepest = 0;
            DungeonImpl.deepestLevels.GetOrCreateValue(Game1.player.team).TryGetValue(dungeonId, out deepest);
            var elevators = dungeonData.FloorsWithElevator.Where(i => i <= deepest).ToList();

            int numElevators = elevators.Count;
            base.width = ((numElevators > 50) ? (484 + IClickableMenu.borderWidth * 2) : Math.Min(220 + IClickableMenu.borderWidth * 2, (numElevators + 1) * 44 + IClickableMenu.borderWidth * 2));
            base.height = Math.Max(64 + IClickableMenu.borderWidth * 3, numElevators * 44 / (base.width - IClickableMenu.borderWidth) * 44 + 64 + IClickableMenu.borderWidth * 3);
            base.xPositionOnScreen = Game1.uiViewport.Width / 2 - base.width / 2;
            base.yPositionOnScreen = Game1.uiViewport.Height / 2 - base.height / 2;
            Game1.playSound("crystal", 0);
            int buttonsPerRow = base.width / 44 - 1;
            int x = base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
            int y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
            this.elevators.Add(new ClickableComponent(new Rectangle(x, y, 44, 44), 0.ToString() ?? "")
            {
                myID = 0,
                rightNeighborID = 1,
                downNeighborID = buttonsPerRow
            });
            x = x + 64 - 20;
            if (x > base.xPositionOnScreen + base.width - IClickableMenu.borderWidth)
            {
                x = base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                y += 44;
            }
            for (int i = 1; i <= numElevators; i++)
            {
                this.elevators.Add(new ClickableComponent(new Rectangle(x, y, 44, 44), elevators[i - 1].ToString() ?? "")
                {
                    myID = i,
                    rightNeighborID = ((i % buttonsPerRow == buttonsPerRow - 1) ? (-1) : (i + 1)),
                    leftNeighborID = ((i % buttonsPerRow == 0) ? (-1) : (i - 1)),
                    downNeighborID = i + buttonsPerRow,
                    upNeighborID = i - buttonsPerRow
                });
                x = x + 64 - 20;
                if (x > base.xPositionOnScreen + base.width - IClickableMenu.borderWidth)
                {
                    x = base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                    y += 44;
                }
            }
            base.initializeUpperRightCloseButton();
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                this.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.isWithinBounds(x, y))
            {
                foreach (ClickableComponent c in this.elevators)
                {
                    if (!c.containsPoint(x, y))
                    {
                        continue;
                    }
                    Game1.playSound("smallSelect");
                    if (Convert.ToInt32(c.name) == 0)
                    {
                        var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
                        dungeonsData.TryGetValue(dungeonId, out var dungeonData);
                        Game1.warpFarmer(dungeonData.ElevatorExitLocation, dungeonData.ElevatorExitTile.X, dungeonData.ElevatorExitTile.Y, flip: true);
                        Game1.exitActiveMenu();
                        continue;
                    }
                    if (Convert.ToInt32(c.name) == Game1.CurrentMineLevel)
                    {
                        return;
                    }
                    Game1.player.ridingMineElevator = true;
                    Game1.warpFarmer(DungeonImpl.GetLevelNamePrefix(dungeonId) + c.name, 0, 0, 2);
                    Game1.player.temporarilyInvincible = true;
                    Game1.player.temporaryInvincibilityTimer = 0;
                    Game1.player.flashDuringThisTemporaryInvincibility = false;
                    Game1.player.currentTemporaryInvincibilityDuration = 1000;
                    Game1.exitActiveMenu();
                }
                base.receiveLeftClick(x, y);
            }
            else
            {
                Game1.exitActiveMenu();
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            foreach (ClickableComponent c in this.elevators)
            {
                if (c.containsPoint(x, y))
                {
                    c.scale = 2f;
                }
                else
                {
                    c.scale = 1f;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            int currLevel = Game1.currentLocation.GetDungeonExtData().spaceCoreDungeonLevel.Value;

            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }
            Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen - 64 + 8, base.width + 21, base.height + 64, speaker: false, drawOnlyBox: true);
            foreach (ClickableComponent c in this.elevators)
            {
                b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X - 4, c.bounds.Y + 4), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
                b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
                NumberSprite.draw(position: new Vector2(c.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(c.name)) * 6, c.bounds.Y + 24 - NumberSprite.getHeight() / 4), number: Convert.ToInt32(c.name), b: b, c: (currLevel == Convert.ToInt32(c.name)) ? (Color.Gray * 0.75f) : Color.Gold, scale: 0.5f, layerDepth: 0.86f, alpha: 1f, secondDigitOffset: 0);
            }
            base.draw(b);
            base.drawMouse(b);
        }
    }
}
