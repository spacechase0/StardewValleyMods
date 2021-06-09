using System.Collections.Generic;
using System.Linq;
using BiggerCraftables.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace BiggerCraftables
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static List<ContentList.Entry> entries = new List<ContentList.Entry>();

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            foreach (var cp in helper.ContentPacks.GetOwned())
            {
                var list = cp.ReadJsonFile<ContentList>("content.json");
                foreach (var entry in list.BiggerCraftables)
                {
                    entry.Texture = cp.LoadAsset<Texture2D>(entry.Image);
                    Log.debug($"Bigger craftable - {entry.Name} from {cp.Manifest.Name} - {entry.Width}x{entry.Length}");
                    Mod.entries.Add(entry);
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

        private bool doingStuff = false;
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (this.doingStuff)
                return;
            this.doingStuff = true;

            var loc = e.Location;

            foreach (var pair in e.Removed)
            {
                var pos = pair.Key;
                var obj = pair.Value;

                if (!obj.bigCraftable.Value)
                    continue;
                var entry = Mod.entries.SingleOrDefault(cle => cle.Name == obj.Name);
                if (entry == null)
                    continue;

                int ind = obj.GetBiggerIndex();

                int relPosX = ind % entry.Width, relPosY = entry.Length - 1 - ind / entry.Width;
                Vector2 basePos = new Vector2(pos.X - relPosX, pos.Y - relPosY);
                for (int ix = 0; ix < entry.Width; ++ix)
                {
                    for (int iy = 0; iy < entry.Length; ++iy)
                    {
                        Vector2 localPos = basePos + new Vector2(ix, iy);
                        if (localPos == pos || !loc.Objects.ContainsKey(localPos))
                            continue;
                        loc.Objects.Remove(localPos);
                    }
                }
            }

            this.doingStuff = false;
        }
    }
}
