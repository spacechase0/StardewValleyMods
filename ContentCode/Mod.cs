using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ContentCode
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private Dictionary<string, Dictionary<string, object>> packStates = new();
        private Dictionary<string, Dictionary<string, BaseRunner>> runnables = new();

        internal Queue<string> pendingUpdateFiles = new();

        private List< MetadataReference > compileRefs;
        private CSharpCompilationOptions compileOpts = new(
            OutputKind.DynamicallyLinkedLibrary,
            generalDiagnosticOption: ReportDiagnostic.Error,
            metadataImportOptions: MetadataImportOptions.All
        );

        private string[] SupportedFiles = new[]
        {
            "SaveLoaded",
            "Action",
            "TouchAction",
        };

        private IAssetName[] SupportedAssetNames = null!;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            compileRefs = CompileReferences();

            SupportedAssetNames = SupportedFiles.Select((file) => helper.GameContent.ParseAssetName($"spacechase0.ContentCode/{file}")).ToArray();

            string path = Path.Combine( Helper.DirectoryPath, "_generated" );
            if ( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += this.OnAssetInvalidated;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            Helper.Events.GameLoop.SaveLoaded += ( sender, args ) => Run( "SaveLoaded", new[] { args } );
            SpaceEvents.ActionActivated += ( sender, args ) => Run( "Action", new[] { args }, args.Action );
            SpaceEvents.TouchActionActivated += ( sender, args ) => Run( "TouchAction", new[] { args }, args.Action );

            // These should be empty at this point, but when they get reloaded our compilation stuff will run
            foreach ( string file in SupportedFiles )
            {
                Game1.content.Load<Dictionary<string, string>>( $"spacechase0.ContentCode/{file}" );
                runnables.Add( file, new() );
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsDirectlyUnderPath("spacechase0.ContentCode"))
            {
                string file = Path.GetFileName(e.NameWithoutLocale.BaseName);
                if (SupportedFiles.Contains(file))
                    e.LoadFrom(static () => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
            }
        }

        private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            foreach (IAssetName file in SupportedAssetNames)
            {
                if (e.NamesWithoutLocale.Contains(file))
                {
                    this.pendingUpdateFiles.Enqueue(file.BaseName);
                }
            }
        }

        private void OnUpdateTicked( object sender, UpdateTickedEventArgs e )
        {
            while ( pendingUpdateFiles.Count > 0 )
            {
                string entry = pendingUpdateFiles.Dequeue();
                Compile( entry.Substring( "spacechase0.ContentCode/".Length ) );
            }
        }

        private void Run( string evt, object[] args, string specific = null )
        {
            if ( specific != null )
            {
                try
                {
                    if ( runnables[ evt ].ContainsKey( specific ) )
                        runnables[ evt ][ specific ].Run( args );
                }
                catch ( Exception e )
                {
                    Log.Error( $"Exception while running {evt} for {specific}: " + e );
                }
            }
            else
            {
                foreach ( var entry in runnables[ evt ] )
                {
                    try
                    {
                        entry.Value.Run( args );
                    }
                    catch ( Exception e )
                    {
                        Log.Error( $"Exception while running {evt} for {entry.Key}: " + e );
                    }
                }
            }
        }

        private void Compile( string evt )
        {
            Log.Trace( $"Compiling for {evt}" );
            runnables[ evt ].Clear();

            bool needSubentry = ( evt == "Action" || evt == "TouchAction" );

            var codes = Game1.content.Load< Dictionary< string, string > >( "spacechase0.ContentCode/" + evt );
            foreach ( var code in codes )
            {
                int slash = code.Key.IndexOf( '/' );
                if ( needSubentry && slash == -1 )
                {
                    Log.Warn( $"Found entry in {evt} file without content pack ID! {code.Key}" );
                    continue;
                }

                string pack = needSubentry ? code.Key.Substring( 0, slash ) : code.Key;
                var packInfo = Helper.ModRegistry.Get( pack );
                if ( packInfo == null || packInfo.GetType().GetProperty( "ContentPack" ).GetValue( packInfo ) == null )
                {
                    Log.Warn( $"Found an entry in {evt} without a valid content pack! {code.Key}" );
                    continue;
                }
                string key = needSubentry ? code.Key.Substring( slash + 1 ) : "";

                try
                {
                    var runner = Compile( pack, evt, code.Value );
                    runnables[ evt ].Add( code.Key, runner );
                }
                catch ( Exception e )
                {
                    Log.Error( $"Exception while compiling {evt}:{code.Key}: {e}" );
                }
            }
        }

        private BaseRunner Compile( string pack, string key, string codeStr )
        {
            Log.Trace( $"Compiling {pack}/{key}..." );
            string filename = $"v{ModManifest.Version}_{pack}_{key}_{codeStr.Length}_{codeStr.GetDeterministicHashCode()}";
            string path = Path.Combine( Helper.DirectoryPath, "_generated", filename + ".dll" );
            if ( File.Exists( path ) )
            {
                Log.Trace( "\tFound in cache!" );
                var asm = Assembly.LoadFrom( path );
                return GetRunnerFrom( asm, pack );
            }

            string code = File.ReadAllText( Path.Combine( Helper.DirectoryPath, "assets", key + "Template.cs" ) );
            code = code.Replace( "#REPLACE_packname", pack );
            code = code.Replace( "#REPLACE_code", codeStr.Replace( '`', '"' ) );

            var comp = CSharpCompilation.Create( filename, new[] { CSharpSyntaxTree.ParseText( code ) }, compileRefs, compileOpts );

            using MemoryStream ms = new();
            var result = comp.Emit( ms );

            if ( !result.Success )
            {
                var fails = result.Diagnostics.Where( d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error );

                StringBuilder sb = new();
                sb.AppendLine( "Errors: " );
                foreach ( var fail in fails )
                {
                    sb.AppendLine( $"{fail.Id}: {fail.GetMessage()}" );
                }
                Log.Error( sb.ToString() );

                return null;
            }
            else
            {
                ms.Seek( 0, SeekOrigin.Begin );
                File.WriteAllBytes( path, ms.ToArray() );
                ms.Seek( 0, SeekOrigin.Begin );

                var asm = Assembly.LoadFrom( path );
                return GetRunnerFrom( asm, pack );
            }
        }

        private BaseRunner GetRunnerFrom( Assembly asm, string pack )
        {
            var type = asm.GetType( $"{pack}.Runner" );
            var ret = ( BaseRunner ) type.GetConstructor( new Type[ 0 ] ).Invoke( new object[ 0 ] );

            if ( !packStates.ContainsKey( pack ) )
                packStates.Add( pack, new() );

            var modInfo = Helper.ModRegistry.Get( pack );
            ret.ContentPack = ( IContentPack ) modInfo.GetType().GetProperty( "ContentPack" ).GetValue( modInfo );
            ret.Reflection = Helper.Reflection;
            ret.State = packStates[ pack ];

            return ret;
        }

        private static List<MetadataReference> CompileReferences()
        {
            List<MetadataReference> ret = new();

            foreach ( var asm in AppDomain.CurrentDomain.GetAssemblies() )
            {
                try
                {
                    // This is a no-no for these types of mods
                    if ( asm.FullName.Contains( "HarmonyLib" ) )
                        continue;

                    ret.Add( MetadataReference.CreateFromFile( asm.Location ) );
                }
                catch ( Exception e )
                {
                    //Log.Trace("Couldn't add assembly " + asm + ": " + e);
                }
            }

            return ret;
        }
    }
}
