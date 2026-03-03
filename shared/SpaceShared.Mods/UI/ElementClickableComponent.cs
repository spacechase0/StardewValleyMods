#if !DEPENDENCY_HAS_SPACESHARED
using Microsoft.Xna.Framework;
using StardewValley.Menus;

#if IS_SPACECORE
namespace SpaceCore.UI;

public
#else
namespace SpaceShared.UI;

internal
#endif
class ElementClickableComponent : ClickableComponent, IScreenReadable
{
    public Element Parent { get; }

    public ElementClickableComponent(Element parent, Rectangle bounds, string name = null)
    :   base( bounds, name )
    {
        Parent = parent;
    }

    public virtual new string ScreenReaderText
    {
        get => base.ScreenReaderText ?? Parent.ScreenReaderText;
        set => base.ScreenReaderText = value;
    }
    public virtual new string ScreenReaderDescription
    {
        get => base.ScreenReaderDescription ?? Parent.ScreenReaderDescription;
        set => base.ScreenReaderDescription = value;
    }

    private bool? screenReaderIgnoreVal;
    public virtual new bool ScreenReaderIgnore
    {
        get => screenReaderIgnoreVal ?? Parent.ScreenReaderIgnore;
        set => screenReaderIgnoreVal = value;
    }
}
#endif
