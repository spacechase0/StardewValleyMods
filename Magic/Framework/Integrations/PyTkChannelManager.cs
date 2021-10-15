using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace Magic.Framework.Integrations
{
    /// <summary>Handles integrating with PyTK to add the custom Magic TV channel.</summary>
    internal class PyTkChannelManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether the PyTK mod is installed.</summary>
        private readonly bool HasPyTk;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        public PyTkChannelManager(IModRegistry modRegistry)
        {
            this.HasPyTk = modRegistry.IsLoaded("Platonymous.Toolkit");
        }

        /// <summary>Add the Magic TV channel through PyTK if it's installed, and log a warning if registering the channel fails.</summary>
        public void AddTvChannel()
        {
            if (!this.HasPyTk)
            {
                Log.Trace("PyTK is not installed, so the Magic TV channel won't be available in-game.");
                return;
            }

            // get PyTK's static custom TV class
            Type customTv = Type.GetType("PyTK.CustomTV.CustomTVMod, PyTK", throwOnError: false);
            if (customTv == null)
            {
                Log.Warn("PyTK is installed, but couldn't access its Custom TV API. The Magic TV channel won't be available in-game.");
                return;
            }

            // get addChannel method
            MethodInfo addChannel = customTv.GetMethod("addChannel", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(string), typeof(string), typeof(Action<TV, TemporaryAnimatedSprite, Farmer, string>) }, new ParameterModifier[0]);
            if (addChannel == null)
            {
                Log.Warn("PyTK is installed, but couldn't access its Custom TV 'addChannel' method. The Magic TV channel won't be available in-game.");
                return;
            }

            // add channel
            try
            {
                addChannel.Invoke(null, new object[] { "magic", I18n.Tv_Analyzehints_Name(), (Action<TV, TemporaryAnimatedSprite, Farmer, string>)OnTvChannelSelected });
            }
            catch (Exception ex)
            {
                Log.Warn($"PyTK is installed, but couldn't register the Magic TV channel. Technical details:\n{ex}");
                return;
            }

            // show program
            void OnTvChannelSelected(TV tv, TemporaryAnimatedSprite sprite, Farmer farmer, string answer)
            {
                // get screen sprite
                TemporaryAnimatedSprite screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 305, 42, 28), 150f, 2, 999999, tv.getScreenPosition(), false, false, (float)((tv.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f);

                // get channel text
                string transKey = "tv.analyzehints.notmagical";
                Random r = new Random((int)Game1.stats.DaysPlayed + (int)(Game1.uniqueIDForThisGame / 2));
                if (Game1.player.GetMaxMana() > 0)
                    transKey = "tv.analyzehints." + (r.Next(12) + 1);

                // get showProgram method
                var showProgram = customTv.GetMethod("showProgram", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(TemporaryAnimatedSprite), typeof(string), typeof(Action), typeof(TemporaryAnimatedSprite) }, new ParameterModifier[0]);
                if (showProgram == null)
                {
                    Log.Warn("PyTK is installed, but couldn't access its Custom TV 'showProgram' method.");
                    return;
                }

                // register with PyTK
                try
                {
                    showProgram.Invoke(null, new object[] { screen, I18n.GetByKey(transKey), null, null });
                }
                catch (Exception ex)
                {
                    Log.Warn($"PyTK is installed, but couldn't show the Magic TV channel. Technical details:\n{ex}");
                    return;
                }
            }
        }
    }
}
