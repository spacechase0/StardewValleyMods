using GenericModConfigMenu.Api.Pages;
using StardewModdingAPI;

#nullable enable
namespace GenericModConfigMenu.Api;

public interface IModConfig
{
    public IManifest Owner { get; }
    public IPage RootPage { get; }

    public IModConfig SetCanOpen(VisibilityContext context, bool canOpen);
}
