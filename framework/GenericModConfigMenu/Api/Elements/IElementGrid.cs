using Microsoft.Xna.Framework;

#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface IElementGrid : IElementContainer
{
    public int Columns { get; }

    public IElementContainer SetCellMinimumSize(Vector2 size);
    public IElementContainer SetChild(int column, int row, IElement child);
}
