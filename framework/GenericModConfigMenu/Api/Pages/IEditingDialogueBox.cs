using GenericModConfigMenu.Api.Elements;

#nullable enable
namespace GenericModConfigMenu.Api.Pages;

public interface IEditingDialogueBox<T> : IDialogueBox
{
    public IConfigOption<T> Invoker { get; }
}
