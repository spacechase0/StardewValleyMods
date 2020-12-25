using Harmony;
using Newtonsoft.Json;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SpaceCore.Overrides
{
    public static class SaveGameHooks
    {
        public const string Filename = "spacecore-serialization.json";
        public const string FarmerFilename = "spacecore-serialization-farmer.json";

        private static bool initializedSerializers = false;

        internal static string loadFileContext = null;

        // Update these each game update
        private static Type[] vanillaMainTypes = new Type[25]
        {
            typeof(Tool),
            typeof(GameLocation),
            typeof(Duggy),
            typeof(Bug),
            typeof(BigSlime),
            typeof(Ghost),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Quest),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
            typeof(TerrainFeature)
        };
        private static Type[] vanillaFarmerTypes = new Type[1]
        {
            typeof(Tool)
        };
        private static Type[] vanillaGameLocationTypes = new Type[24]
        {
            typeof(Tool),
            typeof(Duggy),
            typeof(Ghost),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Bug),
            typeof(BigSlime),
            typeof(BreakableContainer),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
            typeof(TerrainFeature)
        };

        public static void InitializeSerializers()
        {
            if ( initializedSerializers )
                return;
            initializedSerializers = true;

            Log.trace( "Reinitializing serializers..." );

            SaveGame.serializer = InitializeSerializer( typeof( SaveGame ), vanillaMainTypes );
            SaveGame.farmerSerializer = InitializeSerializer( typeof( Farmer ), vanillaFarmerTypes );
            SaveGame.locationSerializer = InitializeSerializer( typeof( GameLocation ), vanillaGameLocationTypes );
        }

        public static XmlSerializer InitializeSerializer( Type baseType, Type[] extra = null )
        {
            List<Type> types = new List<Type>();
            if ( extra != null )
                types.AddRange( extra );
            types.AddRange( SpaceCore.modTypes );
            return new XmlSerializer( baseType, types.ToArray() );
        }
    }

    [HarmonyPatch( typeof( SaveGame ), nameof( SaveGame.GetSerializer ) )]
    public static class SaveGameGetSerializerPatch
    {
        public static bool Prefix( Type type, ref XmlSerializer __result )
        {
            __result = SaveGameHooks.InitializeSerializer( type );
            return false;
        }
    }

    [HarmonyPatch( typeof( LoadGameMenu ), "FindSaveGames" )]
    public static class LoadGameMenuFindSavesPatch
    {
        public static void Prefix()
        {
            SaveGameHooks.InitializeSerializers();
        }
    }

    [HarmonyPatch( typeof( SaveGame ), nameof( SaveGame.Load ) )]
    [HarmonyPriority(Priority.First)]
    public static class SaveGameLoadPatch
    {
        public static void Prefix( string filename )
        {
            SaveGameHooks.InitializeSerializers();
            SaveGameHooks.loadFileContext = filename;
        }
    }

    // Save is done as well for the case of creating a new save without loading one
    [HarmonyPatch( typeof( SaveGame ), nameof( SaveGame.Save ) )]
    public static class SaveGameSavePatch
    {
        public static void Prefix()
        {
            SaveGameHooks.InitializeSerializers();
        }
    }

    [HarmonyPatch]
    public static class SaveGameLoadEnumeratorPatch
    {
        private static Type FindModType( string xmlType )
        {
            return SpaceCore.modTypes.SingleOrDefault( t => t.GetCustomAttribute<XmlTypeAttribute>().TypeName == xmlType );
        }

        private static void RestoreModNodes( XmlDocument doc, XmlNode node, List< KeyValuePair< string, string > > modNodes, string currPath = "" )
        {
            var processed = new List<KeyValuePair<string, string>>();
            foreach ( var modNode in modNodes )
            {
                if ( !modNode.Key.StartsWith( $"{currPath}/" ) )
                    continue;

                string idxStr = modNode.Key.Substring( currPath.Length + 1 );
                if ( !idxStr.Contains( '/' ) )
                {
                    var newDoc = new XmlDocument();
                    newDoc.LoadXml( modNode.Value );

                    var attr = newDoc.DocumentElement.Attributes[ "xsi:type" ];
                    if ( attr == null || FindModType( attr.Value ) == null )
                        continue;

                    var newNode = doc.ImportNode( newDoc.DocumentElement, true );

                    int idx = int.Parse( idxStr );
                    if ( idx == 0 )
                        node.PrependChild( newNode );
                    else
                        node.InsertAfter( newNode, node.ChildNodes[ idx - 1 ] );

                    processed.Add( modNode );
                }
            }
            foreach ( var p in processed )
                modNodes.Remove( p );

            for ( int i = 0; i < node.ChildNodes.Count; ++i )
            {
                //Log.trace( "child " + i + "/" + node.ChildNodes.Count );
                RestoreModNodes( doc, node.ChildNodes[ i ] as XmlNode, modNodes, $"{currPath}/{i}" );
            }
        }

        public static object DeserializeProxy( XmlSerializer serializer, Stream stream, string farmerPath, bool fromSaveGame )
        {
            XmlDocument doc = new XmlDocument();
            doc.Load( stream );

            string filePath = null;
            if ( fromSaveGame )
            {
                farmerPath = Path.Combine( Constants.SavesPath, SaveGameHooks.loadFileContext );
                if ( serializer == SaveGame.farmerSerializer )
                    filePath = Path.Combine( farmerPath, SaveGameHooks.FarmerFilename );
                else
                    filePath = Path.Combine( farmerPath, SaveGameHooks.Filename );
            }
            else
                filePath = Path.Combine( Path.GetDirectoryName( farmerPath ), SaveGameHooks.FarmerFilename );

            if ( File.Exists( filePath ) )
            {
                List< KeyValuePair< string, string > > modNodes = null;
                modNodes = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>( File.ReadAllText( filePath ) );
                if ( modNodes != null )
                    RestoreModNodes( doc, doc, modNodes, "/1" ); // <?xml ... ?> is 1
            }

            return serializer.Deserialize( new XmlTextReader( new StringReader( doc.OuterXml ) ) );
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            List<MethodBase> ret = new List<MethodBase>();
            foreach ( var type in typeof( SaveGame ).GetNestedTypes( BindingFlags.NonPublic ) )
            {
                if ( type.Name.Contains( "getLoadEnumerator" ) )
                {
                    // Primary enumerator method
                    ret.Add( type.GetMethod( "MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) );
                }
                else if ( type.Name.Contains( "<>" ) && type.Name != "<>c" )
                {
                    foreach ( var meth in type.GetMethods( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) )
                    {
                        if ( meth.Name.Contains( "getLoadEnumerator" ) )
                        {
                            // A lambda inside the enumerator
                            ret.Add( meth );
                            break;
                        }
                    }
                }
            }
            if ( ret.Count != 2 )
            {
                Log.warn( $"Found {ret.Count} transpiler targets, expected 2" );
                foreach ( var meth in ret )
                {
                    Log.trace( "\t" + meth.Name + " " + meth );
                }
            }
            ret.Add( AccessTools.Method( typeof( LoadGameMenu ), "FindSaveGames" ) );
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            var newInsns = new List<CodeInstruction>();
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Callvirt && ( insn.operand as MethodInfo ).Name == "Deserialize" )
                {
                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof( SaveGameLoadEnumeratorPatch ).GetMethod( nameof( DeserializeProxy ) );

                    // We'll need the file path too since we can't use the current save constant.
                    if ( original.DeclaringType == typeof( LoadGameMenu ) )
                    {
                        newInsns.Add( new CodeInstruction( OpCodes.Ldloc_S, 5 ) );
                        newInsns.Add( new CodeInstruction( OpCodes.Ldc_I4_0 ) );
                    }
                    else
                    {
                        newInsns.Add( new CodeInstruction( OpCodes.Ldnull ) );
                        newInsns.Add( new CodeInstruction( OpCodes.Ldc_I4_1 ) );
                    }
                }
                newInsns.Add( insn );
            }

            return newInsns;
        }
    }
    [HarmonyPatch]
    public static class SaveGameSaveEnumeratorPatch
    {
        private static bool FindAndRemoveModNodes( XmlNode node, List<KeyValuePair<string, string>> modNodes, string currPath = "" )
        {
            if ( node.HasChildNodes )
            {
                for ( int i = node.ChildNodes.Count - 1; i >= 0; --i )
                {
                    var child = node.ChildNodes[ i ];
                    if ( FindAndRemoveModNodes( child, modNodes, $"{currPath}/{i}" ) )
                    {
                        modNodes.Insert( 0, new KeyValuePair<string, string>( $"{currPath}/{i}", child.OuterXml ) );
                        node.RemoveChild( child );
                    }
                }
            }

            if ( node.Attributes == null )
                return false;

            var attr = node.Attributes[ "xsi:type" ];
            if ( attr == null )
                return false;

            if ( attr.Value.StartsWith( "Mods_" ) )
                return true;
            return false;
        }

        public static void SerializeProxy( XmlSerializer serializer, XmlWriter origWriter, object obj )
        {
            using ( var ms = new MemoryStream() )
            {
                var writer = XmlWriter.Create( ms, new XmlWriterSettings() { CloseOutput = false } );

                serializer.Serialize( writer, obj );
                XmlDocument doc = new XmlDocument();
                ms.Position = 0;
                doc.Load( ms );

                var modNodes = new List< KeyValuePair< string, string > >();
                FindAndRemoveModNodes( doc, modNodes, "/1" ); // <?xml ... ?> is /0
                
                doc.WriteContentTo( origWriter );
                if ( serializer == SaveGame.farmerSerializer )
                    File.WriteAllText( Path.Combine( Constants.CurrentSavePath, SaveGameHooks.FarmerFilename ), JsonConvert.SerializeObject( modNodes ) );
                else
                    File.WriteAllText( Path.Combine( Constants.CurrentSavePath, SaveGameHooks.Filename ), JsonConvert.SerializeObject( modNodes ) );
            }
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            List<MethodBase> ret = new List<MethodBase>();
            foreach ( var type in typeof( SaveGame ).GetNestedTypes( BindingFlags.NonPublic ) )
            {
                if ( type.Name.Contains( "getSaveEnumerator" ) )
                {
                    // Primary enumerator method
                    ret.Add( type.GetMethod( "MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) );
                }
            }
            if ( ret.Count != 1 )
            {
                Log.warn( $"Found {ret.Count} transpiler targets, expected 1" );
                foreach ( var meth in ret )
                {
                    Log.trace( "\t" + meth.Name + " " + meth );
                }
            }
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            int skip = 0;

            var newInsns = new List<CodeInstruction>();
            foreach ( var insn in insns )
            {
                if ( skip > 0 )
                {
                    --skip;
                    continue;
                }

                if ( insn.opcode == OpCodes.Callvirt && ( insn.operand as MethodInfo ).Name == "Serialize" )
                {
                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof( SaveGameSaveEnumeratorPatch ).GetMethod( nameof( SerializeProxy ) );

                    // So, I couldn't figure out how to get SerializeProxy to not write the start/end tags of SaveGame
                    // So instead, remove when SaveGame does it.
                    // Search a few instructions before since one occurs at a different distance than the other.
                    for ( int i = 1; i < 6; ++i )
                    {
                        if ( newInsns[ newInsns.Count - i ].opcode == OpCodes.Callvirt && ( newInsns[ newInsns.Count - i ].operand as MethodInfo ).Name == "WriteStartDocument" )
                        {
                            newInsns.RemoveAt( newInsns.Count - i );
                            newInsns.RemoveAt( newInsns.Count - i );
                            skip = 2;
                            break;
                        }
                    }
                }
                newInsns.Add( insn );
            }

            return newInsns;
        }
    }

    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.loadDataToLocations))]
    public static class SaveGameCleanLocationsPatch
    {
        public static void Prefix( List<GameLocation> gamelocations )
        {
            foreach ( var loc in gamelocations )
            {
                var objs = loc.netObjects.Pairs.ToArray();
                foreach ( var pair in objs )
                {
                    if ( pair.Value == null )
                        loc.netObjects.Remove( pair.Key );
                }
                var tfs = loc.terrainFeatures.Pairs.ToArray();
                foreach ( var pair in tfs )
                {
                    if ( pair.Value == null )
                        loc.terrainFeatures.Remove( pair.Key );
                }
            }
        }
    }
}
