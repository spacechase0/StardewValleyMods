using System.Collections.Generic;

using BiggerCraftables.Framework;
using BiggerCraftables.Patches;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace BiggerCraftables
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Dictionary<string, Entry> Entries = new();

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            foreach (var cp in helper.ContentPacks.GetOwned())
            {
                var list = cp.ReadJsonFile<ContentList>("content.json");
                foreach (var entry in list.BiggerCraftables)
                {
                    entry.Texture = cp.ModContent.Load<Texture2D>(entry.Image);
                    Log.Debug($"Bigger craftable - {entry.Name} from {cp.Manifest.Name} - {entry.Width}x{entry.Length}");
                    if (!Mod.Entries.TryAdd(entry.Name, entry))
                        Log.Error($"{entry.Name} appears to be a duplicate and won't be added.");
                }
            }

            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

            HarmonyPatcher.Apply(this,
                new ObjectPatcher(),
                new UtilityPatcher()
            );
        }

        public override object GetApi()
        {
            return new Api();
        }

        private bool DoingStuff;
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (this.DoingStuff)
                return;
            this.DoingStuff = true;

            var loc = e.Location;

            foreach (var (pos, obj) in e.Removed)
            {
                if (!obj.bigCraftable.Value)
                    continue;

                if (!Mod.Entries.TryGetValue(obj.Name, out var entry))
                    continue;

                int ind = obj.GetBiggerIndex();

                int relPosX = ind % entry.Width, relPosY = entry.Length - 1 - ind / entry.Width;
                Vector2 basePos = new(pos.X - relPosX, pos.Y - relPosY);
                for (int ix = 0; ix < entry.Width; ++ix)
                {
                    for (int iy = 0; iy < entry.Length; ++iy)
                    {
                        Vector2 localPos = basePos + new Vector2(ix, iy);
                        if (localPos != pos)
                            loc.Objects.Remove(localPos);
                    }
                }
            }

            this.DoingStuff = false;
        }
    }
}
