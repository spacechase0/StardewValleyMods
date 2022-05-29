using System;
using System.Collections.Generic;
using System.Text;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace JsonAssets.Framework
{
    // This asset editor is for things that need to run later, ie. after
    // Content Patcher.
    // For gift tastes, this is so that if someone replaces the whole field
    // with vanilla + stuff, our stuff will still get added.
    internal static class ContentInjector2
    {
        /// <summary>
        /// Enum to match gift tastes to indexes.
        /// 2*n = the response. 2*n + 1 = the actual gift tastes.
        /// </summary>
        private enum GiftTasteIndex: int
        {
            Love = 0,
            Like = 1,
            Dislike = 2,
            Hate = 3,
            Neutral = 4,
        }

        // Using a Lazy to build the list of gift tastes only when first requested.
        internal static Lazy<Dictionary<string, HashSet<string>[]>> gifts = new(GenerateGiftTastes);

        internal static Dictionary<string, HashSet<string>[]> GenerateGiftTastes()
        {
            Log.Trace("Generating gift taste dictionary");
            Dictionary<string, HashSet<string>[]> friendship = new();

            foreach (Data.ObjectData obj in Mod.instance.Objects)
            {
                foreach (string key in obj.GiftTastes.Love)
                {
                    if (!friendship.TryGetValue(key, out HashSet<string>[] tastes))
                        tastes = new[] {new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
                    tastes[(int)GiftTasteIndex.Love].Add(obj.GetObjectId().ToString());
                }
                foreach (string key in obj.GiftTastes.Like)
                {
                    if (!friendship.TryGetValue(key, out HashSet<string>[] tastes))
                        tastes = new[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
                    tastes[(int)GiftTasteIndex.Like].Add(obj.GetObjectId().ToString());
                }
                foreach (string key in obj.GiftTastes.Dislike)
                {
                    if (!friendship.TryGetValue(key, out HashSet<string>[] tastes))
                        tastes = new[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
                    tastes[(int)GiftTasteIndex.Dislike].Add(obj.GetObjectId().ToString());
                }
                foreach (string key in obj.GiftTastes.Hate)
                {
                    if (!friendship.TryGetValue(key, out HashSet<string>[] tastes))
                        tastes = new[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
                    tastes[(int)GiftTasteIndex.Hate].Add(obj.GetObjectId().ToString());
                }
                foreach (string key in obj.GiftTastes.Neutral)
                {
                    if (!friendship.TryGetValue(key, out HashSet<string>[] tastes))
                        tastes = new[] { new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
                    tastes[(int)GiftTasteIndex.Neutral].Add(obj.GetObjectId().ToString());
                }
            }    
            return friendship;
        }

        private static readonly string GIFTTASTES = PathUtilities.NormalizeAssetName(@"Data\NPCGiftTastes");

        internal static void ResetGiftTastes()
        {
            if (gifts.IsValueCreated)
                gifts = new(GenerateGiftTastes);
        }

        internal static void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (Mod.instance.DidInit && e.NameWithoutLocale.IsEquivalentTo(GIFTTASTES))
            {
                e.Edit(
                    (asset) =>
                    {
                        var data = asset.AsDictionary<string, string>().Data;
                        foreach (var (key, tastes) in gifts.Value)
                        {
                            if (key == "Universal")
                            {
                                HashSet<string> loves = new(tastes[(int)GiftTasteIndex.Love]);
                                if (data.TryGetValue("Universal_Love", out string oldLoves))
                                    loves.UnionWith(oldLoves.Split(' '));
                                data["Universal_Love"] = string.Join(' ', loves);

                                HashSet<string> likes = new(tastes[(int)GiftTasteIndex.Like]);
                                if (data.TryGetValue("Universal_Like", out string oldLikes))
                                    likes.UnionWith(oldLikes.Split(' '));
                                data["Universal_Like"] = string.Join(' ', likes);

                                HashSet<string> neutrals = new(tastes[(int)GiftTasteIndex.Neutral]);
                                if (data.TryGetValue("Universal_Neutral", out string oldNeutrals))
                                    neutrals.UnionWith(oldNeutrals.Split(' '));
                                data["Universal_Neutral"] = string.Join(' ', neutrals);

                                HashSet<string> dislikes = new(tastes[(int)GiftTasteIndex.Dislike]);
                                if (data.TryGetValue("Universal_Dislike", out string oldDislikes))
                                    dislikes.UnionWith(oldDislikes.Split(' '));
                                data["Universal_Dislike"] = string.Join(' ', dislikes);

                                HashSet<string> hates = new(tastes[(int)GiftTasteIndex.Hate]);
                                if (data.TryGetValue("Universal_Hate", out string oldHates))
                                    hates.UnionWith(oldHates.Split(' '));
                                data["Universal_Hate"] = string.Join(' ', hates);
                            }
                            else
                            {
                                HashSet<string> loves = new(tastes[(int)GiftTasteIndex.Love]);
                                HashSet<string> likes = new(tastes[(int)GiftTasteIndex.Like]);
                                HashSet<string> neutrals = new(tastes[(int)GiftTasteIndex.Neutral]);
                                HashSet<string> dislikes = new(tastes[(int)GiftTasteIndex.Dislike]);
                                HashSet<string> hates = new(tastes[(int)GiftTasteIndex.Hate]);

                                List<string> tastearray;
                                if (data.TryGetValue(key, out string oldTastes))
                                    tastearray = new(oldTastes.Split('/'));
                                else
                                    tastearray = new(10);

                                int loveindex = ((int)GiftTasteIndex.Love) * 2 + 1;
                                int likeindex = ((int)GiftTasteIndex.Like) * 2 + 1;
                                int dislikeindex = ((int)GiftTasteIndex.Dislike) * 2 + 1;
                                int hateindex = ((int)GiftTasteIndex.Hate) * 2 + 1;
                                int neutralindex = ((int)GiftTasteIndex.Neutral) * 2 + 1;

                                if (tastearray.Count > loveindex)
                                    loves.UnionWith(tastearray[loveindex].Split(' '));
                                if (tastearray.Count > likeindex)
                                    likes.UnionWith(tastearray[likeindex].Split(' '));
                                if (tastearray.Count > dislikeindex)
                                    dislikes.UnionWith(tastearray[dislikeindex].Split(' '));
                                if (tastearray.Count > hateindex)
                                    hates.UnionWith(tastearray[hateindex].Split(' '));
                                if (tastearray.Count > neutralindex)
                                    neutrals.UnionWith(tastearray[neutralindex].Split(' '));

                                // string interpolation for some stupid reason sucks if you give it more than four
                                // inputs in NET 5.0
                                StringBuilder sb = new();
                                if (tastearray.Count > 0)
                                    sb.Append(tastearray[0]);
                                sb.Append('/');
                                sb.AppendJoin(' ', loves);
                                sb.Append('/');
                                if (tastearray.Count > 2)
                                    sb.Append(tastearray[2]);
                                sb.Append('/');
                                sb.AppendJoin(' ', likes);
                                sb.Append('/');
                                if (tastearray.Count > 4)
                                    sb.Append(tastearray[4]);
                                sb.Append('/');
                                sb.AppendJoin(' ', dislikes);
                                sb.Append('/');
                                if (tastearray.Count > 6)
                                    sb.Append(tastearray[6]);
                                sb.Append('/');
                                sb.AppendJoin(' ', hates);
                                sb.Append('/');
                                if (tastearray.Count > 8)
                                    sb.Append(tastearray[8]);
                                sb.Append('/');
                                sb.AppendJoin(' ', neutrals);

                                data[key] = sb.ToString();
                            }
                        }
                    },
                    (AssetEditPriority)1200); // using a custom asset edit priority here because CP packs will get the ability to
                                              // do late edits later.
            }
        }
    }
}
