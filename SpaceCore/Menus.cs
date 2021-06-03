using System.Collections.Generic;

namespace SpaceCore
{
    public class Menus
    {
        private static int currGameMenuTab = 8;
        internal static Dictionary<int, string> extraGameMenuTabs = new Dictionary<int, string>();
        public static int ReserveGameMenuTab(string name)
        {
            int tab = currGameMenuTab++;
            extraGameMenuTabs.Add(tab, name);
            return tab;
        }
    }
}
