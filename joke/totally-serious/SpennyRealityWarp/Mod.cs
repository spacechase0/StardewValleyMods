using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;

namespace SpennyRealityWarp;

internal class Mod : StardewModdingAPI.Mod
{
    public static Mod instance;

    public float? worldRotation = null;
    public Vector2 worldCenter;
    public bool inWorldRender = false;

    public override void Entry(IModHelper helper)
    {
        instance = this;
        Log.Monitor = this.Monitor;

        helper.Events.Display.MenuChanged += this.Display_MenuChanged;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Display.RenderingWorld += (_, _) => inWorldRender = true;
        helper.Events.Display.RenderedWorld += (_, _) => inWorldRender = false;

        var harmony = new Harmony(ModManifest.UniqueID);
        new SpriteBatchPatcher().Apply(harmony, Monitor);
        harmony.PatchAll();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 Transform(Vector2 pos, float rfactor = 1)
    {
        // https://community.monogame.net/t/off-axis-vector-2-rotation/11767/2
        float xDiff = pos.X - worldCenter.X;
        float yDiff = pos.Y - worldCenter.Y;
        float x = (float)((Math.Cos((worldRotation ?? 0) * rfactor) * xDiff) - (Math.Sin((worldRotation ?? 0) * rfactor) * yDiff) + worldCenter.X);
        float y = (float)((Math.Sin((worldRotation ?? 0) * rfactor) * xDiff) + (Math.Cos((worldRotation ?? 0) * rfactor) * yDiff) + worldCenter.Y);
        return new Vector2(x, y);
    }

    private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
    {
#if false
        if (e.OldMenu == null && e.NewMenu is DialogueBox db && db.characterDialogue?.speaker?.Name == "Penny")
        {
            worldRotation = 0;
        }
#endif
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
#if true
        bool hasPenny = Game1.currentLocation?.getCharacterFromName("Penny") != null;
        if (!worldRotation.HasValue && hasPenny)
        {
            worldRotation = 0;
        }
        else if (worldRotation.HasValue && !hasPenny)
        {
            worldRotation = null;
        }
#endif

        if (worldRotation.HasValue)
        {
            worldCenter = Game1.GlobalToLocal(Game1.getCharacterFromName("Penny").StandingPixel.ToVector2());
            worldRotation += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds * (MathF.PI * 2) / 12f;
            if (worldRotation.Value >= MathF.PI * 2)
                worldRotation = null;
        }
    }
}
