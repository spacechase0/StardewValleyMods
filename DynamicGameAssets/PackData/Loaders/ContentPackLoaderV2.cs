using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;
using StardewModdingAPI;

namespace DynamicGameAssets.PackData.Loaders
{
    internal class BasePackDataListConverter : JsonConverter
    {
        public override bool CanConvert( Type objectType )
        {
            return objectType == typeof( List< BasePackData > );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            var jtoken = JToken.Load( reader );
            if ( jtoken is JArray array )
            {
                List<BasePackData> ret = new List<BasePackData>();

                int i = 0;
                foreach ( var entry in array )
                {
                    var obj = ( JObject ) entry;

                    var typeProp = obj.Properties().FirstOrDefault( prop => prop.Name == "$ItemType" );
                    if ( typeProp == null )
                    {
                        Log.Error( "No $ItemType prop @ " + reader.Path + "/" + i + "!" );
                        continue;
                    }

                    var actualType = Type.GetType( "DynamicGameAssets.PackData." + typeProp.Value + "PackData" );
                    if ( actualType == null )
                    {
                        Log.Error( "Invalid $ItemType prop @ " + reader.Path + "/" + i + "! (" + typeProp.Value + ")" );
                        continue;
                    }

                    ret.Add( ( BasePackData ) entry.ToObject( actualType, serializer ) );
                    ++i;
                }

                return ret;
            }
            else
            {
                Log.Error( "Must have array here! " + reader.Path );
                return null;
            }
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            JArray jarray = new JArray();
            foreach ( var val in ( List< BasePackData > ) value  )
            {
                var toAdd = ( JObject ) JToken.FromObject( val );
                List< string > toRemove = new();
                foreach ( var prop in toAdd )
                {
                    var cprop = val.GetType().GetProperty( prop.Key );
                    var defAttr = cprop.GetCustomAttribute( typeof( DefaultValueAttribute ) );
                    if ( defAttr != null )
                    {
                        if ( ( defAttr as DefaultValueAttribute ).Value == null )
                        {
                            if ( prop.Value.Type == JTokenType.Null )
                                toRemove.Add( prop.Key );
                            continue;
                        }

                        bool same = ( defAttr as DefaultValueAttribute ).Value.Equals( prop.Value.ToObject( ( defAttr as DefaultValueAttribute ).Value.GetType() ) );
                        if ( same )
                        {
                            toRemove.Add( prop.Key );
                            continue;
                        }
                    }

                    var ignAttr = cprop.GetCustomAttribute( typeof( JsonIgnoreAttribute ) );
                    if ( ignAttr != null )
                    {
                        toRemove.Add( prop.Key );
                        continue;
                    }

                    var cmeth = val.GetType().GetMethod( "ShouldSerialize" + prop.Key );
                    if ( cmeth != null && !( cmeth.Invoke( val, new object[ 0 ] ) as bool? ).Value )
                    {
                        toRemove.Add( prop.Key );
                        continue;
                    }
                }
                foreach ( string prop in toRemove )
                    toAdd.Remove( prop );

                toAdd.AddFirst( new JProperty( "$ItemType", val.GetType().Name.ToString().Substring( 0, val.GetType().Name.IndexOf( "PackData" ) ) ) );
                jarray.Add( toAdd );
            }
            serializer.Serialize( writer, jarray );
        }
    }

    public class ContentPackLoaderV2 : IContentPackLoader
    {
        private ContentPack pack;

        public ContentPackLoaderV2( ContentPack thePack )
        {
            this.pack = thePack;
        }

        public void Load()
        {
            /*
            List<BasePackData> data = new();
            data.Add( new ObjectPackData() );
            data.Add( new BigCraftablePackData() );
            var conv = new BasePackDataListConverter();
            //Log.Debug( JsonConvert.SerializeObject( data, Formatting.Indented, new BasePackDataListConverter() ) );
            */
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
                var colorConverter = (JsonConverter) AccessTools.Constructor( AccessTools.TypeByName( "StardewModdingAPI.Framework.Serialization.ColorConverter" ) ).Invoke( new object[ 0 ] );
                var vec2Converter = (JsonConverter) AccessTools.Constructor( AccessTools.TypeByName( "StardewModdingAPI.Framework.Serialization.Vector2Converter" ) ).Invoke( new object[ 0 ] );
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    Converters = new[] { new BasePackDataListConverter(), colorConverter, vec2Converter }
                };
                var data = JsonConvert.DeserializeObject<List<BasePackData>>( File.ReadAllText( Path.Combine( pack.smapiPack.DirectoryPath, json ) ), settings );
                foreach ( var d in data )
                {
                    if ( d is CommonPackData cd && pack.items.ContainsKey( cd.ID ) )
                    {
                        Log.Error( "Duplicate found! " + cd.ID );
                        continue;
                    }
                    Log.Debug( "Loading data< " + d.GetType() + " >..." );

                    if ( !pack.enableIndex.ContainsKey( parent ) )
                        pack.enableIndex.Add( parent, new() );
                    pack.enableIndex[ parent ].Add( d );
                    d.pack = pack;
                    d.parent = parent;
                    d.original = ( BasePackData ) d.Clone();
                    d.original.original = d.original;

                    if ( d is CommonPackData cdata )
                    {
                        pack.items.Add( cdata.ID, cdata );
                        Mod.itemLookup.Add( $"{pack.smapiPack.Manifest.UniqueID}/{cdata.ID}".GetDeterministicHashCode(), $"{pack.smapiPack.Manifest.UniqueID}/{cdata.ID}" );
                    }
                    else
                    {
                        pack.others.Add( d );
                    }
                    d.PostLoad();

                    if ( d is ContentIndexPackData cidata )
                    {
                        LoadIndex( cidata.FilePath, cidata );
                    }
                }
            }
            catch ( Exception e )
            {
                Log.Error( "Exception loading content index: \"" + json + "\": " + e );
            }
        }
    }
}
