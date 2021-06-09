using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Minigames;

namespace PyromancersJourney
{
    public class PyromancerMinigame : IMinigame
    {
        private World world = new World();

        public void changeScreenSize()
        {
        }

        public bool doMainGameUpdates()
        {
            return false;
        }

        public void draw(SpriteBatch b)
        {
            world.Render();
        }

        public bool forceQuit()
        {
            return true;
        }

        public void leftClickHeld(int x, int y)
        {
        }

        public string minigameId()
        {
            return "PyromancerJourney";
        }

        public bool overrideFreeMouseMovement()
        {
            return false;
        }

        public void receiveEventPoke(int data)
        {
        }

        public void receiveKeyPress(Keys k)
        {
        }

        public void receiveKeyRelease(Keys k)
        {
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public void releaseLeftClick(int x, int y)
        {
        }

        public void releaseRightClick(int x, int y)
        {
        }

        public bool tick(GameTime time)
        {
            world.Update();
            return world.HasQuit;
        }

        public void unload()
        {
        }
    }
}
