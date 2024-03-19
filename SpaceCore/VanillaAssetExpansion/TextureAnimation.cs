using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpaceCore.VanillaAssetExpansion
{
    /// <summary>A texture animation.</summary>
    internal class TextureAnimation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The individual frames in the animation.</summary>
        public TextureAnimationFrame[] Frames { get; }

        /// <summary>The number of game ticks needed to fully play the animation before it repeats.</summary>
        public int Duration { get; }

        /// <summary>A standardized descriptor for the animation.</summary>
        public string Descriptor { get; }

        /// <summary>A singleton instance for an empty animation.</summary>
        public static TextureAnimation Empty { get; } = new(Enumerable.Empty<TextureAnimationFrame>());


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="frames">The individual frames in the animation.</param>
        public TextureAnimation(IEnumerable<TextureAnimationFrame> frames)
        {
            this.Frames = frames.ToArray();
            this.Duration = this.Frames.Sum(p => p.Duration);
            this.Descriptor = string.Join(",", this.Frames.Select(p => p.Descriptor));
        }

        /// <summary>Get the current animation frame to display based on the game tick value.</summary>
        /// <param name="tick">The current game tick.</param>
        public TextureAnimationFrame GetCurrentFrame(int tick)
        {
            tick %= this.Duration;

            foreach (var frame in this.Frames)
            {
                tick -= frame.Duration;
                if (tick <= 0)
                    return frame;
            }

            return this.Frames.Last();
        }

        /// <summary>Parse an animation descriptor.</summary>
        /// <param name="descriptor">The animation descriptor. See 'texture animations' in the DGA author guide for a description of the format.</param>
        public static TextureAnimation ParseFrom(string descriptor)
        {
            if (string.IsNullOrWhiteSpace(descriptor))
                return TextureAnimation.Empty;

            var frames = new List<TextureAnimationFrame>();

            Regex regex = new Regex(@"((?<assetName>[^,:@]+)(:(?<startFrame>\d+))?(\.\.(?<endFrame>\d+))?(@(?<duration>\d+))?)(,\1)*");
            foreach (Match match in regex.Matches(descriptor))
            {
                // parse animation descriptor
                string assetName = match.Groups["assetName"].Value;
                if (!int.TryParse(match.Groups["startFrame"].Value, out int startFrame))
                    startFrame = 0;
                if (!int.TryParse(match.Groups["endFrame"].Value, out int endFrame))
                    endFrame = startFrame;
                if (!int.TryParse(match.Groups["duration"].Value, out int duration))
                    duration = 1;

                // save frames
                for (int frame = startFrame; frame <= endFrame; frame++)
                    frames.Add(new TextureAnimationFrame(assetName, frame, duration));
            }

            return new TextureAnimation(frames);
        }
    }
}
