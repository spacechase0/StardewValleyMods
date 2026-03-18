using System.Reflection;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics.Pipeline;
using StardewValley;

namespace Stardew3D.Models;
public class ContentPipelineTextureFactory : TextureFactory<byte[]>
{
    private ImageFileTextureFactory forEmbedded;
    private MethodInfo convertMethod;

    public ContentPipelineTextureFactory(GraphicsDevice device)
        : base(device)
    {
        forEmbedded = new(device);
        convertMethod = AccessTools.Method(forEmbedded.GetType(), nameof(ConvertTexture));
    }

    protected override Texture2D ConvertTexture(byte[] image)
    {
        if (image.Length <= "MGTEX:".Length)
            return ( Texture2D ) convertMethod.Invoke( forEmbedded, [ image ] );

        string asStr = Encoding.ASCII.GetString(image);
        if (!asStr.StartsWith("MGTEX:"))
            return (Texture2D) convertMethod.Invoke(forEmbedded, [image]);

        return Game1.content.Load<Texture2D>(asStr.Substring("MGTEX:".Length));
    }
}
