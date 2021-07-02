// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.BellsAndWhistles
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Paritee.StardewValley.Core.Utilities
{
  public class BellsAndWhistles
  {
    public static void PlaySound(string soundName, int timer = 0, GameLocation location = null)
    {
      if (timer == 0)
        Game1.playSound(soundName);
      else
        DelayedAction.playSoundAfterDelay(soundName, timer, location);
    }

    public static bool HasSoundBank()
    {
      return Game1.soundBank != null;
    }

    public static void CueSound(string sound, string name, float value)
    {
      if (!HasSoundBank())
        return;
      var cue = Game1.soundBank.GetCue(sound);
      cue.SetVariable(name, value);
      cue.Play();
    }

    public static void FadeToBlack(bool global = true, Action afterFade = null, float fadeSpeed = 0.02f)
    {
      if (global)
        Game1.globalFadeToBlack(afterFade.Invoke, fadeSpeed);
      else
        Game1.fadeScreenToBlack();
    }

    public static bool IsFaded()
    {
      return Game1.globalFade;
    }

    public static void AddHudMessage(string message)
    {
      Game1.addHUDMessage(new HUDMessage(message));
    }

    public static void AddHudMessage(string message, bool achievement)
    {
      Game1.addHUDMessage(new HUDMessage(message, achievement));
    }

    public static void AddHudMessage(string message, int whatType)
    {
      Game1.addHUDMessage(new HUDMessage(message, whatType));
    }

    public static void AddHudMessage(string message, string leaveMeNull)
    {
      Game1.addHUDMessage(new HUDMessage(message, leaveMeNull));
    }

    public static void AddHudMessage(string message, Color color, float timeLeft, bool fadeIn = false)
    {
      Game1.addHUDMessage(new HUDMessage(message, color, timeLeft, fadeIn));
    }

    public static void AddHudMessage(
      string type,
      int number,
      bool add,
      Color color,
      Item messageSubject = null)
    {
      Game1.addHUDMessage(new HUDMessage(type, number, add, color, messageSubject));
    }

    public static void DrawScroll(
      SpriteBatch spriteBatch,
      string str,
      int x,
      int y,
      string placeHolderWidthText = "",
      float alpha = 1f,
      int color = -1)
    {
      SpriteText.drawStringWithScrollBackground(spriteBatch, str, x, y, placeHolderWidthText, alpha, color);
    }
  }
}