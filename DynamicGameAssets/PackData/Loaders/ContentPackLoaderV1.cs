using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpaceShared;
using StardewModdingAPI;

namespace DynamicGameAssets.PackData.Loaders
{
    public class ContentPackLoaderV1 : IContentPackLoader
    {
        private ContentPack pack;

        public ContentPackLoaderV1( ContentPack thePack )
        {
            this.pack = thePack;
        }

        public void Load()
        {
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
            LoadOthers<TextureOverridePackData>( "texture-overrides.json" );

            LoadIndex( "content.json" );
        }

        private void LoadIndex( string json, ContentIndexPackData parent = null )
        {
            if ( !pack.smapiPack.HasFile( json ) )
            {
                if ( parent != null )
                    Log.Warn( "Missing json file: " + json );
                return;
            }
            if ( parent == null )
            {
                parent = new ContentIndexPackData()
                {
                    pack = pack,
                    parent = null,
                    ContentType = "ContentIndex",
                    FilePath = json,
                };
                parent.original = ( ContentIndexPackData ) parent.Clone();
                parent.original.original = parent.original;
            }

            try
            {
                var data = pack.smapiPack.LoadAsset<List<ContentIndexPackData>>( json ) ?? new List<ContentIndexPackData>();
                foreach ( var d in data )
                {
                    Log.Trace( "Loading data<" + typeof( ContentIndexPackData ) + "> " + d.ContentType + " " + d.FilePath + "..." );
                    pack.others.Add( d );
                    if ( !pack.enableIndex.ContainsKey( parent ) )
                        pack.enableIndex.Add( parent, new() );
                    pack.enableIndex[ parent ].Add( d );
                    d.pack = pack;
                    d.parent = parent;
                    d.original = ( ContentIndexPackData ) d.Clone();
                    d.original.original = d.original;
                    d.PostLoad();

                    var packDataType = Type.GetType( "DynamicGameAssets.PackData." + d.ContentType + "PackData" );
                    if ( packDataType == null )
                    {
                        Log.Error( "Invalid ContentType: " + d.ContentType );
                        continue;
                    }

                    MethodInfo baseMethod = null;
                    if ( packDataType == typeof( ContentIndexPackData ) )
                        baseMethod = typeof( ContentPack ).GetMethod( nameof( LoadIndex ), BindingFlags.NonPublic | BindingFlags.Instance );
                    else if ( packDataType.BaseType == typeof( CommonPackData ) )
                        baseMethod = typeof( ContentPack ).GetMethod( nameof( LoadAndValidateItems ), BindingFlags.NonPublic | BindingFlags.Instance );
                    else if ( packDataType.BaseType == typeof( BasePackData ) )
                        baseMethod = typeof( ContentPack ).GetMethod( nameof( LoadOthers ), BindingFlags.NonPublic | BindingFlags.Instance );
                    else
                        throw new Exception( "this should never happen" );

                    MethodInfo genMethod = baseMethod.IsGenericMethod ? baseMethod.MakeGenericMethod( packDataType ) : baseMethod;
                    genMethod.Invoke( this, new object[] { d.FilePath, d } );
                }
            }
            catch ( Exception e )
            {
                Log.Error( "Exception loading content index: \"" + json + "\": " + e );
            }
        }

        private void LoadAndValidateItems<T>( string json, ContentIndexPackData parent = null ) where T : CommonPackData
        {
            if ( !pack.smapiPack.HasFile( json ) )
            {
                if ( parent != null )
                    Log.Warn( "Missing json file: " + json );
                return;
            }
            if ( parent == null )
            {
                parent = new ContentIndexPackData()
                {
                    pack = pack,
                    parent = null,
                    ContentType = typeof( T ).Name.Substring( 0, typeof( T ).Name.Length - "PackData".Length ),
                    FilePath = json,
                };
                parent.original = ( ContentIndexPackData ) parent.Clone();
                parent.original.original = parent.original;
            }

            try
            {
                var data = pack.smapiPack.LoadAsset<List<T>>( json ) ?? new List<T>();
                foreach ( var d in data )
                {
                    if ( pack.items.ContainsKey( d.ID ) )
                    {
                        Log.Error( "Duplicate found! " + d.ID );
                        continue;
                    }
                    try
                    {
                        Log.Trace( "Loading data<" + typeof( T ) + ">: " + d.ID );
                        pack.items.Add( d.ID, d );
                        if ( !pack.enableIndex.ContainsKey( parent ) )
                            pack.enableIndex.Add( parent, new() );
                        pack.enableIndex[ parent ].Add( d );
                        Mod.itemLookup.Add( $"{pack.smapiPack.Manifest.UniqueID}/{d.ID}".GetDeterministicHashCode(), $"{pack.smapiPack.Manifest.UniqueID}/{d.ID}" );
                        /*if ( d is ShirtPackData )
                            Mod.itemLookup.Add( $"{smapiPack.Manifest.UniqueID}/{d.ID}".GetDeterministicHashCode() + 1, $"{smapiPack.Manifest.UniqueID}/{d.ID}" );
                        */
                        d.pack = pack;
                        d.parent = parent;
                        d.original = ( T ) d.Clone();
                        d.original.original = d.original;
                        d.PostLoad();
                    }
                    catch ( Exception e )
                    {
                        Log.Error( "Exception loading item \"" + d.ID + "\": " + e );
                    }
                }
            }
            catch ( Exception e )
            {
                Log.Error( "Exception loading data of type " + typeof( T ) + ": " + e );
            }
        }

        private void LoadOthers<T>( string json, ContentIndexPackData parent = null ) where T : BasePackData
        {
            if ( !pack.smapiPack.HasFile( json ) )
            {
                if ( parent != null )
                    Log.Warn( "Missing json file: " + json );
                return;
            }
            if ( parent == null )
            {
                parent = new ContentIndexPackData()
                {
                    pack = pack,
                    parent = null,
                    ContentType = typeof( T ).Name.Substring( 0, typeof( T ).Name.Length - "PackData".Length ),
                    FilePath = json,
                };
                parent.original = ( ContentIndexPackData ) parent.Clone();
                parent.original.original = parent.original;
            }

            try
            {
                var data = pack.smapiPack.LoadAsset<List<T>>( json ) ?? new List<T>();
                int i = 0;
                foreach ( var d in data )
                {
                    Log.Trace( "Loading data<" + typeof( T ) + ">..." );
                    try
                    {
                        pack.others.Add( d );
                        if ( !pack.enableIndex.ContainsKey( parent ) )
                            pack.enableIndex.Add( parent, new() );
                        pack.enableIndex[ parent ].Add( d );
                        d.pack = pack;
                        d.parent = parent;
                        d.original = ( T ) d.Clone();
                        d.original.original = d.original;
                        d.PostLoad();
                    }
                    catch ( Exception e )
                    {
                        Log.Debug( $"Exception loading item entry {i} from {json}: " + e );
                    }
                    ++i;
                }
            }
            catch ( Exception e )
            {
                Log.Error( "Exception loading data of type " + typeof( T ) + ": " + e );
            }
        }
    }
}
