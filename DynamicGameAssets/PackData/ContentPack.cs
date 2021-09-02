using DynamicGameAssets.PackData.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.PackData
{
    public class ContentPack : IAssetLoader
    {
        internal class ConfigModel
        {
            [JsonExtensionData]
            public Dictionary<string, JToken> Values = new();
        }

        internal IContentPack smapiPack;

        internal ISemanticVersion conditionVersion;

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        private Dictionary<string, int[]> animInfo = new Dictionary<string, int[]>(); // Index is full animation descriptor (items16.png:1@333/items16.png:2@333/items16.png:3@334), value is [frameDur1, frameDur2, frameDur3, ..., totalFrameDur]

        protected internal Dictionary<string, CommonPackData> items = new Dictionary<string, CommonPackData>();

        protected internal List<BasePackData> others = new List<BasePackData>();

        internal Dictionary< ContentIndexPackData?, List<BasePackData> > enableIndex = new();

        private List<ConfigPackData> configs = new();
        internal Dictionary<string, ConfigPackData> configIndex = new();
        internal ConfigModel currConfig = new();
        
        public ContentPack( IContentPack pack, int formatVer, ISemanticVersion condVer )
        {
            smapiPack = pack;

            if ( pack.Manifest.UniqueID != "null" )
            {
                conditionVersion = condVer;
                switch ( formatVer )
                {
                    case 1: new ContentPackLoaderV1( this ).Load(); break;
                    case 2: new ContentPackLoaderV2( this ).Load(); break;
                    default:
                        throw new Exception( "Invalid content pack format version: " + pack.Manifest.ExtraFields[ "DGA.FormatVersion" ].ToString() );
                }

                LoadConfig(); // TODO: Move this to pack loader as well, once it diverges
            }
        }
        public ContentPack( IContentPack pack )
        :   this( pack, pack.Manifest.UniqueID == "null" ? 1 : int.Parse( pack.Manifest.ExtraFields[ "DGA.FormatVersion" ].ToString() ), pack.Manifest.UniqueID == "null" ? null : new SemanticVersion( pack.Manifest.ExtraFields[ "DGA.ConditionsFormatVersion" ].ToString() ) )
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

        public bool CanLoad<T>( IAssetInfo asset )
        {
            string path = asset.AssetName.Replace( '\\', '/' );
            string start = "DGA/" + smapiPack.Manifest.UniqueID + "/";
            if ( !path.StartsWith( start ) || !path.EndsWith( ".png" ) )
                return false;
            return smapiPack.HasFile( path.Substring( start.Length ) );
        }

        public T Load<T>( IAssetInfo asset )
        {
            string path = asset.AssetName.Replace( '\\', '/' );
            string start = "DGA/" + smapiPack.Manifest.UniqueID + "/";
            if ( !path.StartsWith( start ) || !path.EndsWith( ".png" ) )
                return default( T );
            return ( T ) ( object ) smapiPack.LoadAsset<Texture2D>( path.Substring( start.Length ) );
        }

        private void LoadConfig()
        {
            if ( !smapiPack.HasFile( "config-schema.json" ) )
                return;

            var gmcm = Mod.instance.Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
            if ( gmcm == null )
                return;

            gmcm.UnregisterModConfig( smapiPack.Manifest );
            gmcm.RegisterModConfig( smapiPack.Manifest, this.ResetToDefaultConfig, () => smapiPack.WriteJsonFile( "config.json", currConfig ) );
            gmcm.SetDefaultIngameOptinValue( smapiPack.Manifest, true );
            gmcm.RegisterParagraph( smapiPack.Manifest, "Note: If in-game, config values may not take effect until the next in-game day." );

            var readConfig = smapiPack.ReadJsonFile< ConfigModel >( "config.json" );
            bool writeConfig = false;
            if ( readConfig == null )
            {
                readConfig = new ConfigModel();
                writeConfig = true;
            }

            var data = smapiPack.LoadAsset<List<ConfigPackData>>( "config-schema.json" ) ?? new List<ConfigPackData>();
            foreach ( var d in data )
            {
                Log.Trace( $"Loading config entry {d.Name}..." );
                configs.Add( d );

                gmcm.StartNewPage( smapiPack.Manifest, d.OnPage );
                switch ( d.ElementType )
                {
                    case ConfigPackData.ConfigElementType.Label:
                        if ( d.PageToGoTo != null )
                            gmcm.RegisterPageLabel( smapiPack.Manifest, d.Name, d.Description, d.PageToGoTo );
                        else
                            gmcm.RegisterLabel( smapiPack.Manifest, d.Name, d.Description );
                        break;

                    case ConfigPackData.ConfigElementType.Paragraph:
                        gmcm.RegisterParagraph( smapiPack.Manifest, d.Name );
                        break;

                    case ConfigPackData.ConfigElementType.Image:
                        gmcm.RegisterImage( smapiPack.Manifest, Path.Combine( "DGA", smapiPack.Manifest.UniqueID, d.ImagePath ), d.ImageRect, d.ImageScale );
                        break;

                    case ConfigPackData.ConfigElementType.ConfigOption:
                        string key = d.Name;
                        if ( !string.IsNullOrEmpty( d.OnPage ) )
                            key = d.OnPage + "/" + key;
                        if ( configIndex.ContainsKey( key ) )
                        {
                            Log.Error( "Duplicate config key: " + key );
                            continue;
                        }
                        configIndex.Add( key, d );
                        currConfig.Values.Add( key, readConfig.Values.ContainsKey( key ) ? readConfig.Values[ key ] : d.DefaultValue );

                        string[] valid = d.ValidValues?.Split( ',' )?.Select( s => s.Trim() )?.ToArray();
                        switch ( d.ValueType )
                        {
                            case ConfigPackData.ConfigValueType.Boolean:
                                gmcm.RegisterSimpleOption( smapiPack.Manifest, d.Name, d.Description, () => currConfig.Values[ key ].ToString() == "true" ? true : false, ( v ) => currConfig.Values[ key ] = v ? "true" : "false" );
                                break;

                            case ConfigPackData.ConfigValueType.Integer:
                                if ( valid?.Length == 2 )
                                    gmcm.RegisterClampedOption( smapiPack.Manifest, d.Name, d.Description, () => int.Parse( currConfig.Values[ key ].ToString() ), ( v ) => currConfig.Values[ key ] = v.ToString(), int.Parse( valid[ 0 ] ), int.Parse( valid[ 1 ] ) );
                                else if ( valid?.Length == 3 )
                                    gmcm.RegisterClampedOption( smapiPack.Manifest, d.Name, d.Description, () => int.Parse( currConfig.Values[ key ].ToString() ), ( v ) => currConfig.Values[ key ] = v.ToString(), int.Parse( valid[ 0 ] ), int.Parse( valid[ 1 ] ), int.Parse( valid[ 2 ] ) );
                                else
                                    gmcm.RegisterSimpleOption( smapiPack.Manifest, d.Name, d.Description, () => int.Parse( currConfig.Values[ key ].ToString() ), ( v ) => currConfig.Values[ key ] = v.ToString() );
                                break;

                            case ConfigPackData.ConfigValueType.Float:
                                if ( valid?.Length == 2 )
                                    gmcm.RegisterClampedOption( smapiPack.Manifest, d.Name, d.Description, () => float.Parse( currConfig.Values[ key ].ToString() ), ( v ) => currConfig.Values[ key ] = v.ToString(), float.Parse( valid[ 0 ] ), float.Parse( valid[ 1 ] ) );
                                else if ( valid?.Length == 3 )
                                    gmcm.RegisterClampedOption( smapiPack.Manifest, d.Name, d.Description, () => float.Parse( currConfig.Values[ key ].ToString() ), ( v ) => currConfig.Values[ key ] = v.ToString(), float.Parse( valid[ 0 ] ), float.Parse( valid[ 1 ] ), float.Parse( valid[ 2 ] ) );
                                else
                                    gmcm.RegisterSimpleOption( smapiPack.Manifest, d.Name, d.Description, () => float.Parse( currConfig.Values[ key ].ToString() ), ( v ) => currConfig.Values[ key ] = v.ToString() );
                                break;

                            case ConfigPackData.ConfigValueType.String:
                                if ( valid?.Length > 1 )
                                    gmcm.RegisterChoiceOption( smapiPack.Manifest, d.Name, d.Description, () => currConfig.Values[ key ].ToString(), ( v ) => currConfig.Values[ key ] = v, valid );
                                else
                                    gmcm.RegisterSimpleOption( smapiPack.Manifest, d.Name, d.Description, () => currConfig.Values[ key ].ToString(), ( v ) => currConfig.Values[ key ] = v );
                                break;
                        }
                        break;
                }
            }

            if ( writeConfig )
            {
                smapiPack.WriteJsonFile( "config.json", currConfig );
            }
        }

        private void ResetToDefaultConfig()
        {
            foreach ( var config in configs )
            {
                if ( !currConfig.Values.ContainsKey( config.Name ) )
                    currConfig.Values.Add( config.Name, config.DefaultValue );
                else
                    currConfig.Values[ config.Name ] = config.DefaultValue;
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
                    t.Name = Path.Combine( "DGA", smapiPack.Manifest.UniqueID, pathItself ).Replace( '\\', '/' );
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
