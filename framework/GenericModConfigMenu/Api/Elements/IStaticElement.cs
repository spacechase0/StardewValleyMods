using Microsoft.Xna.Framework;

#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface IStaticElement : IElement
{
    public delegate void ClickHandler(IStaticElement option, Vector2 pixelOnElement);
    public IStaticElement AddClickHandler(ClickHandler handler);
}
