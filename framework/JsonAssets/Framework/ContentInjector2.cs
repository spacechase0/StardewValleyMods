using System.Collections.Generic;
using SpaceShared;
using StardewModdingAPI;

namespace JsonAssets.Framework
{
    // This asset editor is for things that need to run later, ie. after
    // Content Patcher.
    // For gift tastes, this is so that if someone replaces the whole field
    // with vanilla + stuff, our stuff will still get added.
    internal class ContentInjector2
    {
        private readonly List<string> Files;
        public ContentInjector2()
        {
            Mod.instance.Helper.Events.Content.AssetRequested += this.Content_AssetRequested;

            this.Files = new List<string>(new[]
            {
                "Data\\NPCGiftTastes"
            });
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.Name.BaseName == "Data\\NPCGiftTastes")
            {
                e.Edit((asset) =>
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
                        {
                            foreach (var obj in Mod.instance.Objects)
                            {
                                if (npc.Key == "Universal_Love" && obj.GiftTastes.Love.Contains("Universal"))
                                    newData[npc.Key] = npc.Value + " " + obj.Name;
                                if (npc.Key == "Universal_Like" && obj.GiftTastes.Like.Contains("Universal"))
                                    newData[npc.Key] = npc.Value + " " + obj.Name;
                                if (npc.Key == "Universal_Neutral" && obj.GiftTastes.Neutral.Contains("Universal"))
                                    newData[npc.Key] = npc.Value + " " + obj.Name;
                                if (npc.Key == "Universal_Dislike" && obj.GiftTastes.Dislike.Contains("Universal"))
                                    newData[npc.Key] = npc.Value + " " + obj.Name;
                                if (npc.Key == "Universal_Hate" && obj.GiftTastes.Hate.Contains("Universal"))
                                    newData[npc.Key] = npc.Value + " " + obj.Name;
                            }
                            continue;
                        }

                        string[] sections = npc.Value.Split('/');
                        if (sections.Length != 11)
                        {
                            Log.Warn($"Bad gift taste data for {npc.Key}!");
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

                        foreach (var obj in Mod.instance.Objects)
                        {
                            if (obj.GiftTastes.Love.Contains(npc.Key))
                                loveIds.Add(obj.Name.ToString().FixIdJA("O"));
                            if (obj.GiftTastes.Like.Contains(npc.Key))
                                likeIds.Add(obj.Name.ToString().FixIdJA("O"));
                            if (obj.GiftTastes.Neutral.Contains(npc.Key))
                                neutralIds.Add(obj.Name.ToString().FixIdJA("O"));
                            if (obj.GiftTastes.Dislike.Contains(npc.Key))
                                dislikeIds.Add(obj.Name.ToString().FixIdJA("O"));
                            if (obj.GiftTastes.Hate.Contains(npc.Key))
                                hateIds.Add(obj.Name.ToString().FixIdJA("O"));
                        }

                        string loveIdStr = string.Join(" ", loveIds);
                        string likeIdStr = string.Join(" ", likeIds);
                        string dislikeIdStr = string.Join(" ", dislikeIds);
                        string hateIdStr = string.Join(" ", hateIds);
                        string neutralIdStr = string.Join(" ", neutralIds);
                        newData[npc.Key] = $"{loveStr}/{loveIdStr}/{likeStr}/{likeIdStr}/{dislikeStr}/{dislikeIdStr}/{hateStr}/{hateIdStr}/{neutralStr}/{neutralIdStr}/ ";

                        Log.Verbose($"Adding gift tastes for {npc.Key}: {newData[npc.Key]}");
                    }
                    asset.ReplaceWith(newData);
                }, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
        }
    }
}
