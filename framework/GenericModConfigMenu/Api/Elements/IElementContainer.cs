using Microsoft.Xna.Framework;

#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface IElementContainer : IElement
{
    public IElementContainer AddChildren(params IElement[] children);

    public IElementContainer SetInnerSpacing(Vector2 spacing);
}
