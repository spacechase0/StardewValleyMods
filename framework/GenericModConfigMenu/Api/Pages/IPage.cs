using System;

#nullable enable
namespace GenericModConfigMenu.Api.Pages;

public interface IPage : IPageContents
{
    public IPage CreateSubPage(Func<string> pageDisplayName, out IPage newPage);

    public bool TryOpen();
    public void ForceOpen();
}
