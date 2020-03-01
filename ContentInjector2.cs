using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crafting;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace JsonAssets
{
    // This asset editor is for things that need to run later, ie. after
    // Content Patcher.
    // For gift tastes, this is so that if someone replaces the whole field
    // with vanilla + stuff, our stuff will still get added.
    public class ContentInjector2 : IAssetEditor
    {
        private List<string> files;
        public ContentInjector2()
        {
            files = new List<string>(new string[]
            {
                "Data\\NPCGiftTastes",
            });
        }

        public void InvalidateUsed()
        {
            Mod.instance.Helper.Content.InvalidateCache((a) =>
            {
                foreach (var file in files)
                {
                    if (a.AssetNameEquals(file))
                        return true;
                }
                return false;
            });
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            foreach (var file in files )
            {
                if (asset.AssetNameEquals(file))
                    return true;
            }
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!Mod.instance.didInit)
                return;
            
            if (asset.AssetNameEquals("Data\\NPCGiftTastes"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                // TODO: This could be optimized from mn to... m + n?
                // Basically, iterate through objects and create Dictionary<NPC name, GiftData[]>
                // Iterate through objects, each section and add to dict[npc][approp. section]
                // Point is, I'm doing this the lazy way right now
                var newData = new Dictionary<string, string>(data);
                foreach (var npc in data)
                {
                    if (npc.Key.StartsWith("Universal_"))
                        continue;

                    string[] sections = npc.Value.Split('/');
                    if ( sections.Length != 11 )
                    {
                        Log.warn($"Bad gift taste data for {npc.Key}!");
                        continue;
                    }

                    string loveStr = sections[0];
                    List<string> loveIds = new List<string>(sections[1].Split(' '));
                    string likeStr = sections[2];
                    List<string> likeIds = new List<string>(sections[3].Split(' '));
                    string dislikeStr = sections[4];
                    List<string> dislikeIds = new List<string>(sections[5].Split(' '));
                    string hateStr = sections[6];
                    List<string> hateIds = new List<string>(sections[7].Split(' '));
                    string neutralStr = sections[8];
                    List<string> neutralIds = new List<string>(sections[9].Split(' '));

                    foreach ( var obj in Mod.instance.objects )
                    {
                        if (obj.GiftTastes == null)
                            continue;
                        if (obj.GiftTastes.Love != null && obj.GiftTastes.Love.Contains(npc.Key))
                            loveIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Like != null && obj.GiftTastes.Like.Contains(npc.Key))
                            likeIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Neutral != null && obj.GiftTastes.Neutral.Contains(npc.Key))
                            neutralIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Dislike != null && obj.GiftTastes.Dislike.Contains(npc.Key))
                            dislikeIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Hate != null && obj.GiftTastes.Hate.Contains(npc.Key))
                            hateIds.Add(obj.GetObjectId().ToString());
                    }

                    string loveIdStr = string.Join(" ", loveIds);
                    string likeIdStr = string.Join(" ", likeIds);
                    string dislikeIdStr = string.Join(" ", dislikeIds);
                    string hateIdStr = string.Join(" ", hateIds);
                    string neutralIdStr = string.Join(" ", neutralIds);
                    newData[npc.Key] = $"{loveStr}/{loveIdStr}/{likeStr}/{likeIdStr}/{dislikeStr}/{dislikeIdStr}/{hateStr}/{hateIdStr}/{neutralStr}/{neutralIdStr}/ ";

                    Log.verbose($"Adding gift tastes for {npc.Key}: {newData[npc.Key]}");
                }
                asset.ReplaceWith(newData);
            }
        }
    }
}
