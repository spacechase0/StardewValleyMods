using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Graphics.PBRPunctualLight;

namespace Stardew3D.Rendering;

public class GenericModelEffect : Effect, IEffectMatrices
{
    private bool matrixDirty = true;
    private EffectParameter worldParam;
    private EffectParameter worldViewProjectionParam;
    public Matrix _world, _view, _projection;
    public Matrix World
    {
        get => _world;
        set
        {
            if (value != _world)
            {
                _world = value;
                matrixDirty = true;
                worldParam.SetValue(_world);
            }
        }
    }
    public Matrix View { get => _view; set { if (value != _view) { _view = value; matrixDirty = true; } } }
    public Matrix Projection { get => _projection; set { if (value != _projection) { _projection = value; matrixDirty = true; } } }

    private EffectParameter textureParam;
    public Texture2D Texture { get => textureParam.GetValueTexture2D(); set => textureParam.SetValue(value); }

    private EffectParameter colorParam;
    public Color Color { get => new(colorParam.GetValueVector4()); set => colorParam.SetValue(value.ToVector4()); }
    public float Exposure { get => 0; set { } } // ???

    private EffectParameter ambientLightParam;
    public Vector3 AmbientLightColor
    {
        get
        {
            var vec = ambientLightParam.GetValueVector4();
            return new Vector3(vec.X, vec.Y, vec.Z);
        }
        set => ambientLightParam.SetValue(new Vector4(value.X, value.Y, value.Z, 1));
    }

    private EffectParameter pointLightCountParam, pointLightPosParam, pointLightDirsParam, pointLightColorParam;
    public static int MaxLights => 16;

    public GenericModelEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        worldParam = Parameters["World"];
        worldViewProjectionParam = Parameters["WorldViewProj"];
        textureParam = Parameters["Texture"];
        colorParam = Parameters["Color"];
        ambientLightParam = Parameters["AmbientLightColor"];
        pointLightCountParam = Parameters["PointLightCount"];
        pointLightPosParam = Parameters["PointLightPositions"];
        pointLightDirsParam = Parameters["PointLightDirections"];
        pointLightColorParam = Parameters["PointLightColors"];

        Color = Color.White;
    }

    protected GenericModelEffect(GenericModelEffect other)
        : base(other)
    {
        worldParam = Parameters["World"];
        worldViewProjectionParam = Parameters["WorldViewProj"];
        textureParam = Parameters["Texture"];
        colorParam = Parameters["Color"];
        ambientLightParam = Parameters["AmbientLightColor"];
        pointLightCountParam = Parameters["PointLightCount"];
        pointLightPosParam = Parameters["PointLightPositions"];
        pointLightDirsParam = Parameters["PointLightDirections"];
        pointLightColorParam = Parameters["PointLightColors"];

        _world = other._world;
        _view = other._view;
        _projection = other._projection;
        Texture = other.Texture;
        Color = other.Color;
        AmbientLightColor = other.AmbientLightColor;
    }

    protected override void OnApply()
    {
        base.OnApply();

        if (matrixDirty)
        {
            worldViewProjectionParam.SetValue(_world * _view * _projection);
            matrixDirty = false;
        }

        // MonoGame seems to have a bug where sub-elements aren't checked for if updates are needed
        // https://github.com/MonoGame/MonoGame/issues/5845
        // This should force it when necessary
        foreach (var param in Parameters)
        {
            foreach (var elem in param.Elements)
                param.StateKey = Math.Max(param.StateKey, elem.StateKey);
        }
    }

    public override Effect Clone()
    {
        return new GenericModelEffect(this);
    }

    public void SetEnvironment(WorldEnvironment env)
    {
        AmbientLightColor = env.AmbientLight.ToVector3();

        int index = 0;
        foreach (var light in env.Lights)
        {
            if (light == null)
                continue;

            switch (light.Type)
            {
                case Light.LightType.Direction:
                    break;
                case Light.LightType.Point:
                    pointLightPosParam.Elements[index].SetValue(new Vector4(light.Position, light.Range));
                    pointLightDirsParam.Elements[index].SetValue(new Vector4(light.Direction, light.Falloff));
                    pointLightColorParam.Elements[index].SetValue(new Vector4(light.Color.ToVector3(), light.Intensity));
                    break;
                case Light.LightType.Spot:
                    break;
            }
            index += 1;
        }
        pointLightCountParam.SetValue(index);
    }
}
