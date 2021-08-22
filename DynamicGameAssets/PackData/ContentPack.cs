using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.PackData
{
    public class ContentPack
    {
        internal IContentPack smapiPack;

        internal SemanticVersion conditionVersion;

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        private Dictionary<string, int[]> animInfo = new Dictionary<string, int[]>(); // Index is full animation descriptor (items16.png:1@333/items16.png:2@333/items16.png:3@334), value is [frameDur1, frameDur2, frameDur3, ..., totalFrameDur]

        internal Dictionary<string, CommonPackData> items = new Dictionary<string, CommonPackData>();

        internal List<BasePackData> others = new List<BasePackData>();

        public ContentPack( IContentPack pack )
        {
            smapiPack = pack;
            conditionVersion = new SemanticVersion( pack.Manifest.ExtraFields[ "DGA.ConditionsFormatVersion" ].ToString() );
            LoadAndValidateItems<ObjectPackData>( "objects.json" );
            LoadAndValidateItems<CraftingRecipePackData>("crafting-recipes.json");
            LoadAndValidateItems<FurniturePackData>("furniture.json");
            LoadAndValidateItems<CropPackData>( "crops.json" );
            LoadAndValidateItems<MeleeWeaponPackData>( "melee-weapons.json" );
            LoadAndValidateItems<BootsPackData>( "boots.json" );
            LoadAndValidateItems<HatPackData>( "hats.json" );
            LoadAndValidateItems<FencePackData>( "fences.json" );
            LoadAndValidateItems<BigCraftablePackData>( "big-craftables.json" );
            LoadOthers<ShopEntryPackData>( "shop-entries.json" );
            LoadOthers<ForgeRecipePackData>( "forge-recipes.json" );
            LoadOthers<MachineRecipePackData>( "machine-recipes.json" );
        }

        public CommonPackData Find( string item )
        {
            return items.ContainsKey( item ) ? items[ item ] : null;
        }

        private void LoadAndValidateItems< T >( string json ) where T : CommonPackData
        {
            if (!smapiPack.HasFile(json))
                return;

            var data = smapiPack.LoadAsset<List<T>>( json ) ?? new List<T>();
            foreach ( var d in data )
            {
                if ( items.ContainsKey( d.ID ) )
                    throw new ArgumentException( "Duplicate found! " + d.ID );
                items.Add( d.ID, d );
                Mod.itemLookup.Add( $"{smapiPack.Manifest.UniqueID}/{d.ID}".GetHashCode(), $"{smapiPack.Manifest.UniqueID}/{d.ID}" );
                d.parent = this;
                d.original = ( T ) d.Clone();
                d.original.original = d.original;
                d.PostLoad();
            }
        }

        private void LoadOthers<T>( string json ) where T : BasePackData
        {
            if (!smapiPack.HasFile(json))
                return;

            var data = smapiPack.LoadAsset<List<T>>( json ) ?? new List<T>();
            foreach ( var d in data )
            {
                others.Add( d );
                d.parent = this;
                d.original = ( T ) d.Clone();
                d.original.original = d.original;
                d.PostLoad();
            }
        }

        internal string GetTextureFrame( string path )
        {
            string[] frames = path.Split( ',' );
            int[] frameDurs = null;
            if ( animInfo.ContainsKey( path ) )
                frameDurs = animInfo[ path ];
            else
            {
                int total = 0;
                var frameData = new List<int>();
                for ( int i = 0; i < frames.Length; ++i )
                {
                    int dur = 1;
                    int at = frames[ i ].IndexOf( '@' );
                    if ( at != -1 )
                        dur = int.Parse( frames[ i ].Substring( at + 1 ).Trim() );

                    frameData.Add( dur );
                    total += dur;
                }
                frameData.Add( total );
                animInfo.Add( path, frameDurs = frameData.ToArray() );
            }

            int spot = Mod.State.AnimationFrames % frameDurs[ frames.Length ];
            for ( int i = 0; i < frames.Length; ++i )
            {
                spot -= frameDurs[ i ];
                if ( spot < 0 )
                    return frames[ i ].Trim();
            }

            throw new Exception( "This should never happen (" + path + ")" );
        }

        internal TexturedRect GetMultiTexture( string[] paths, int decider, int xSize, int ySize )
        {
            return GetTexture( paths[ decider % paths.Length ], xSize, ySize );
        }

        internal TexturedRect GetTexture( string path, int xSize, int ySize )
        {
            if ( path.Contains( ',' ) )
            {
                return GetTexture( GetTextureFrame( path ), xSize, ySize );
            }
            else
            {
                int at = path.IndexOf( '@' );
                if ( at != -1 )
                    path = path.Substring( 0, at );

                int colon = path.IndexOf( ':' );
                if ( colon == -1 && !smapiPack.HasFile( path ) || colon != -1 && !smapiPack.HasFile( path.Substring( 0, colon ) ) )
                    throw new ArgumentException( "No such file \"" + path + "\"!" );
                if ( colon == -1 )
                    return new TexturedRect() { Texture = textures.ContainsKey( path ) ? textures[ path ] : smapiPack.LoadAsset<Texture2D>( path ), Rect = null };
                string texPath = path.Substring( 0, colon );
                var tex = textures.ContainsKey( texPath ) ? textures[ texPath] : smapiPack.LoadAsset< Texture2D >( texPath );
                if ( !textures.ContainsKey( texPath ) )
                    textures.Add( texPath, tex );
                int sections = tex.Width / xSize;
                int ind = int.Parse( path.Substring( colon + 1 ) );
                return new TexturedRect()
                {
                    Texture = tex,
                    Rect = new Microsoft.Xna.Framework.Rectangle( ind % sections * xSize, ind / sections * ySize, xSize, ySize )
                };
            }
        }
    }
}
