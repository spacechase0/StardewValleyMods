using System;
using GenericModConfigMenu.Api.Pages;

#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface IElement
{
    public IModConfig RootConfig { get; }
    public IPageContents Owner { get; }
    public IElementContainer? Parent { get; }

    public string Label { get; }
    public bool IsVisible { get; }

    public IElement SetLabel(Func<IElement, string> label);
    public IElement SetTooltip(Func<IElement, string> tooltip);

    public IElement SetContextVisibility(VisibilityContext context, bool visible);
    public IElement SetDefaultVisibility(Func<IElement, bool> visible);
    public IElement SetCurrentVisibility(bool visible);

    public IElement SetMinimumSize(int? width, int? height);
    public IElement SetHorizontalPositionAnchor(PositionElementAnchor anchor = PositionElementAnchor.After, IElement? relativeTo = null);
    public IElement SetVerticalPositionAnchor(PositionElementAnchor anchor = PositionElementAnchor.After, IElement? relativeTo = null);
    public IElement SetOuterSpacing(int left = 0, int top = 0, int right = 0, int bottom = 0);
}
