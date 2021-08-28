using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
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

        internal ISemanticVersion conditionVersion;

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        private Dictionary<string, int[]> animInfo = new Dictionary<string, int[]>(); // Index is full animation descriptor (items16.png:1@333/items16.png:2@333/items16.png:3@334), value is [frameDur1, frameDur2, frameDur3, ..., totalFrameDur]

        internal Dictionary<string, CommonPackData> items = new Dictionary<string, CommonPackData>();

        internal List<BasePackData> others = new List<BasePackData>();
        
        public ContentPack( IContentPack pack, ISemanticVersion condVer )
        {
            smapiPack = pack;
            if ( pack.Manifest.UniqueID != "null" )
            {
                conditionVersion = condVer;
                LoadAndValidateItems<ObjectPackData>( "objects.json" );
                LoadAndValidateItems<CraftingRecipePackData>( "crafting-recipes.json" );
                LoadAndValidateItems<FurniturePackData>( "furniture.json" );
                LoadAndValidateItems<CropPackData>( "crops.json" );
                LoadAndValidateItems<MeleeWeaponPackData>( "melee-weapons.json" );
                LoadAndValidateItems<BootsPackData>( "boots.json" );
                LoadAndValidateItems<HatPackData>( "hats.json" );
                LoadAndValidateItems<FencePackData>( "fences.json" );
                LoadAndValidateItems<BigCraftablePackData>( "big-craftables.json" );
                LoadAndValidateItems<FruitTreePackData>( "fruit-trees.json" );
                LoadAndValidateItems<ShirtPackData>( "shirts.json" );
                LoadAndValidateItems<PantsPackData>( "pants.json" );
                LoadOthers<ShopEntryPackData>( "shop-entries.json" );
                LoadOthers<ForgeRecipePackData>( "forge-recipes.json" );
                LoadOthers<MachineRecipePackData>( "machine-recipes.json" );
                LoadOthers<TailoringRecipePackData>( "tailoring-recipes.json" );
            }
        }
        public ContentPack( IContentPack pack )
        :   this( pack, pack.Manifest.UniqueID == "null" ? null : new SemanticVersion( pack.Manifest.ExtraFields[ "DGA.ConditionsFormatVersion" ].ToString() ) )
        {
        }

        public List<CommonPackData> GetItems()
        {
            return new List<CommonPackData>( items.Values );
        }

        public CommonPackData Find( string item )
        {
            if ( items.ContainsKey( item ) && items[ item ].Enabled )
                return items[ item ];
            return null;
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
                Log.Trace( "Loading data<" + typeof( T ) + ">: " + d.ID );
                items.Add( d.ID, d );
                Mod.itemLookup.Add( $"{smapiPack.Manifest.UniqueID}/{d.ID}".GetDeterministicHashCode(), $"{smapiPack.Manifest.UniqueID}/{d.ID}" );
                /*if ( d is ShirtPackData )
                    Mod.itemLookup.Add( $"{smapiPack.Manifest.UniqueID}/{d.ID}".GetDeterministicHashCode() + 1, $"{smapiPack.Manifest.UniqueID}/{d.ID}" );
                */d.parent = this;
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
                Log.Trace( "Loading data<" + typeof( T ) + ">..." );
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
            if ( paths == null )
                return new TexturedRect() { Texture = Game1.staminaRect, Rect = null };

            return GetTexture( paths[ decider % paths.Length ], xSize, ySize );
        }

        internal TexturedRect GetTexture( string path_, int xSize, int ySize )
        {
            if ( path_ == null )
                return new TexturedRect() { Texture = Game1.staminaRect, Rect = null };

            string path = path_;
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
                string pathItself = colon == -1 ? path : path.Substring( 0, colon );
                if ( textures.ContainsKey( pathItself ) )
                {
                    if ( colon == -1 )
                        return new TexturedRect() { Texture = textures[ pathItself ], Rect = null };
                    else
                    {
                        int sections = textures[ pathItself ].Width / xSize;
                        int ind = int.Parse( path.Substring( colon + 1 ) );

                        return new TexturedRect()
                        {
                            Texture = textures[ pathItself ],
                            Rect = new Rectangle( ind % sections * xSize, ind / sections * ySize, xSize, ySize )
                        };
                    }
                }

                if ( !smapiPack.HasFile( pathItself ) )
                    Log.Warn( "No such \"" + pathItself + "\" in " + smapiPack.Manifest.Name + " (" + smapiPack.Manifest.UniqueID + ")!" );

                Texture2D t;
                try
                {
                    t = smapiPack.LoadAsset<Texture2D>( pathItself );
                }
                catch ( Exception e )
                {
                    t = Game1.staminaRect;
                }
                textures.Add( pathItself, t );

                return GetTexture( path_, xSize, ySize );
            }
        }
    }
}
