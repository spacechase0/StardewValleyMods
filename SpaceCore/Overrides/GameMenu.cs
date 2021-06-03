namespace SpaceCore.Overrides
{
    public class GameMenuTabNameHook
    {
        public static void Postfix(GameMenuTabNameHook __instance, string name, ref int __result)
        {
            foreach ( var tab in Menus.extraGameMenuTabs )
            {
                if (name == tab.Value)
                    __result = tab.Key;
            }
        }
    }
}
