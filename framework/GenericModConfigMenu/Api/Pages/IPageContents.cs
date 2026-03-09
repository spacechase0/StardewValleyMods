using GenericModConfigMenu.Api.Elements;

#nullable enable
namespace GenericModConfigMenu.Api.Pages;

public interface IPageContents
{
    public IModConfig RootConfig { get; }
    public IPage Parent { get; }

    public IPage AddElements(params IElement[] elements);

    public bool TryOpenDialogue(IDialogueBox dialogue);
}
