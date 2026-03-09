#nullable enable
namespace GenericModConfigMenu.Api;

public enum VisibilityContext
{
    TitleMenu = 1 << 0,
    InGame = 1 << 1,

    Everywhere = TitleMenu | InGame,
}
