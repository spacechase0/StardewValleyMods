using System.Collections.Generic;

namespace SpaceCore
{
    public class Menus
    {
        private static int currGameMenuTab = 8;
        internal static Dictionary<int, string> extraGameMenuTabs = new();
        public static int ReserveGameMenuTab(string name)
        {
            int tab = Menus.currGameMenuTab++;
            Menus.extraGameMenuTabs.Add(tab, name);
            return tab;
        }
    }
}
