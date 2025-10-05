using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Stardew3D.Rendering;

public class GenericModelEffect : Effect, IEffectMatrices
{
    private bool matrixDirty = true;
    private EffectParameter worldViewProjectionParam;
    public Matrix _world, _view, _projection;
    public Matrix World { get => _world; set { if (value != _world) { _world = value; matrixDirty = true; } } }
    public Matrix View { get => _view; set { if (value != _view) { _view = value; matrixDirty = true; } } }
    public Matrix Projection { get => _projection; set { if (value != _projection) { _projection = value; matrixDirty = true; } } }

    private EffectParameter textureParam;
    public Texture2D Texture { get => textureParam.GetValueTexture2D(); set => textureParam.SetValue(value); }

    private EffectParameter colorParam;
    public Color Color { get => new(colorParam.GetValueVector4()); set => colorParam.SetValue(value.ToVector4()); }

    public GenericModelEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        worldViewProjectionParam = Parameters["WorldViewProj"];
        textureParam = Parameters["Texture"];
        colorParam = Parameters["Color"];

        Color = Color.White;
    }

    protected GenericModelEffect(GenericModelEffect other)
        : base(other)
    {
        worldViewProjectionParam = Parameters["WorldViewProj"];
        textureParam = Parameters["Texture"];
        colorParam = Parameters["Color"];

        _world = other._world;
        _view = other._view;
        _projection = other._projection;
        Texture = other.Texture;
        Color = other.Color;
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (matrixDirty)
        {
            worldViewProjectionParam.SetValue(_world * _view * _projection);
            matrixDirty = false;
        }
    }

    public override Effect Clone()
    {
        return new GenericModelEffect(this);
    }
}
