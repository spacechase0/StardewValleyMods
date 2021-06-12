using System.Collections.Generic;

namespace SpaceCore
{
    public class Menus
    {
        private static int CurrGameMenuTab = 8;
        internal static Dictionary<int, string> ExtraGameMenuTabs = new();

        public static int ReserveGameMenuTab(string name)
        {
            int tab = Menus.CurrGameMenuTab++;
            Menus.ExtraGameMenuTabs.Add(tab, name);
            return tab;
        }
    }
}
