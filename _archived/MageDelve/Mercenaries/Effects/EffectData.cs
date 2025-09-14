using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using StardewValley;

namespace MageDelve.Mercenaries.Effects
{
    // TODO: Move elsewhere to somewhere more generic?
    public class EffectData : ICloneable
    {
        public float InitialDelay { get; set; } = 0;

        public int Occurrences { get; set; } = 1;
        public float RecurringDelay { get; set; } = 0;

        public string EffectType { get; set; }
        public JObject Parameters { get; set; } = new();

        public static Dictionary<string, Action<Character, EffectData>> EffectTypes { get; internal set; } = new();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class TemporarySpriteEffectParameters
    {
        public string TileSheet { get; set; }
        public Rectangle SourceRect { get; set; }

        public float FrameLength { get; set; } = 1;
        public int FrameCount { get; set; } = 1;
        public int Loops { get; set; } = 1;

        public bool AttachToCharacter { get; set; } = false;

        public Vector2 Offset { get; set; } = Vector2.Zero;
        public float LayerModifier { get; set; } = 0;

        public JObject TAS { get; set; } = new();
    }

    public class LocalSoundEffectParameters
    {
        public string SoundId { get; set; }
        public int? PitchOverride { get; set; } = null;
    }

    public class DamageEffectParameters
    {
        public string Type { get; set; }
        public int Amount { get; set; }
    }

    // Only for use with mercenaries
    public class MercenaryAnimationOverrideEffectParameters
    {
        public string TileSheet { get; set; }
        public int StartFrame { get; set; }

        public float FrameLength { get; set; } = 0.05f;
        public int FrameCount { get; set; } = 1;
        public int Loops { get; set; } = 1;
    }
}
