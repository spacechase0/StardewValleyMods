using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace SpaceShared
{

#nullable enable

    internal static partial class Util
    {
        public static Texture2D FetchTexture( IModRegistry modRegistry, string modIdAndPath )
        {
            if ( modIdAndPath == null || modIdAndPath.IndexOf( '/' ) == -1 )
                return Game1.staminaRect;

            string packId = modIdAndPath.Substring( 0, modIdAndPath.IndexOf( '/' ) );
            string path = modIdAndPath.Substring( modIdAndPath.IndexOf( '/' ) + 1 );

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get( packId );

            if (modInfo is null)
                return Game1.staminaRect;

            if ( modInfo.GetType().GetProperty( "Mod" )?.GetValue( modInfo ) is IMod mod )
                return mod.Helper.ModContent.Load<Texture2D>( path );
            else if ( modInfo.GetType().GetProperty( "ContentPack" )?.GetValue( modInfo ) is IContentPack pack )
                return pack.ModContent.Load<Texture2D>( path );

            return Game1.staminaRect;
        }

        public static IAssetName? FetchTextureLocation(IModRegistry modRegistry, string modIdAndPath)
        {
            if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
                return null;

            string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
            string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get(packId);
            if (modInfo is null)
                return null;

            if (modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo) is IMod mod)
                return mod.Helper.ModContent.GetInternalAssetName(path);
            else if (modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) is IContentPack pack)
                return pack.ModContent.GetInternalAssetName(path);

            return null;
        }

        public static string? FetchTexturePath( IModRegistry modRegistry, string modIdAndPath )
            => FetchTextureLocation(modRegistry, modIdAndPath)?.BaseName;

        public static string FetchFullPath(IModRegistry modRegistry, string modIdAndPath, char partSep = '/')
        {
            if (modIdAndPath == null || modIdAndPath.IndexOf(partSep) == -1)
                return null;

            string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf(partSep));
            string path = modIdAndPath.Substring(modIdAndPath.IndexOf(partSep) + 1);

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get(packId);
            if (modInfo is null)
                return null;

            if (modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo) is IMod mod)
                return Path.Combine(mod.Helper.DirectoryPath, path);
            else if (modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) is IContentPack pack)
                return Path.Combine(pack.DirectoryPath, path);

            return null;
        }

#nullable restore
    }
}
