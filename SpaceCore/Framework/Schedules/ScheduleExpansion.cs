using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace SpaceCore.Framework.Schedules
{
    internal class ScheduleExpansion
    {
        internal static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        // TODO: Return type and stuff
        internal static void GetOverriddenScheduleQueue(string npc, string sched)
        {
            string filePath = $"{SpaceCore.Instance.ModManifest.UniqueID}/ScheduleOverrides/{npc}";
            Dictionary<string, List<ScheduleDataPoint>> data = null;
            try
            {
                data = Game1.content.Load<Dictionary<string, List<ScheduleDataPoint>>>(filePath);
            }
            catch (Exception)
            {
            }


        }

        private static void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            // I don't know how to "properly" check for in a folder or something
            if (e.NameWithoutLocale.Name.StartsWith($"{SpaceCore.Instance.ModManifest.UniqueID}/ScheduleOverrides/") ||
                e.NameWithoutLocale.Name.StartsWith($"{SpaceCore.Instance.ModManifest.UniqueID}\\ScheduleOverrides\\"))
            {
                e.LoadFrom(() => new Dictionary<string, List<ScheduleDataPoint>>(), StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }
    }
}
