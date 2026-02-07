using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpaceShared.SourceGenerator
{
    [Generator]
    public class CustomDictionaryAssetSourceGenerator : IIncrementalGenerator
    {
        private record struct GenerationData( string? TypeNamespace, string TypeName, string AssetName );

        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var prov = ctx.SyntaxProvider.CreateSyntaxProvider(
                static (s, _) => (s is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString().StartsWith("CustomDictionaryAsset") || a.Name.ToString().StartsWith("SpaceShared.Attributes.CustomDictionaryAsset")))),//static t => (t is SimpleBaseTypeSyntax simpleType && simpleType.Type is SimpleNameSyntax simpleName && ( simpleName.Identifier.ValueText?.StartsWith( "BaseMod<" ) ?? false )))),
                static (ctx, _) =>
                {
                    var classDecl = ctx.Node as ClassDeclarationSyntax;
                    var classSym = ctx.SemanticModel.GetDeclaredSymbol( classDecl ) as ITypeSymbol;

                    string? ns = classSym.ContainingNamespace?.Name;
                    var checkNs = classSym.ContainingNamespace;
                    while (checkNs != null && checkNs.ContainingNamespace != null && !checkNs.ContainingNamespace.IsGlobalNamespace)
                    {
                        if ( !string.IsNullOrEmpty(checkNs.ContainingNamespace.Name ) )
                            ns = $"{checkNs.ContainingNamespace.Name}.{ns}";
                        checkNs = checkNs.ContainingNamespace;
                    }
                    string name = classSym.Name;
                    var attr = classSym.GetAttributes().First(a => a.AttributeClass.Name == "CustomDictionaryAssetAttribute");
                    if ( attr.ConstructorArguments.Length == 0 )
                        return new GenerationData(ns, name, "asdf_" + attr.ConstructorArguments[0].Value?.ToString());

                    return new GenerationData(ns, name, attr.ConstructorArguments[0].Value?.ToString());
                })
                .Where( static data => !string.IsNullOrEmpty( data.AssetName ) );

            ctx.RegisterSourceOutput(prov, DoGenerate);
        }

        private void DoGenerate( SourceProductionContext ctx, GenerationData data )
        {
            char[] assetNameChars = data.AssetName.ToCharArray();
            for (int i = 0; i < assetNameChars.Length; i++)
            {
                char c = assetNameChars[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    assetNameChars[i] = '_';
            }
            string assetNameSafe = new string(assetNameChars);

            var code = $@"
using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using SpaceShared;
using SpaceShared.Content;

namespace {data.TypeNamespace}
{{
    public partial class {data.TypeName}
    {{
        public static PerScreen<Dictionary<string, {data.TypeName}>> _assetInstance = new( () => null );

        internal static void RefreshData(bool initial = false)
        {{
             _assetInstance.Value = ContentRegistry.Mod.Helper.GameContent.Load<Dictionary<string, {data.TypeName}>>($""{{ContentRegistry.Mod.ModManifest.UniqueID}}/{data.AssetName}"");
            if ( !initial )
                AfterRefreshData();
        }}

        static partial void AfterRefreshData();

        public static {data.TypeName} Get(string id)
        {{
            if ( _assetInstance.Value == null )
                RefreshData(initial: true);

            if ( !_assetInstance.Value.TryGetValue( id, out var data ) )
                return default;
            return data;
        }}
    }}
}}

namespace SpaceShared.Content
{{
    internal static partial class ContentRegistry
    {{
        private static ContentRegistry.CustomDictionaryAssetRegisterer _CustomDictionaryAsset_{assetNameSafe} = new( ""{data.AssetName}"", typeof( {data.TypeNamespace}.{data.TypeName} ) );
    }}
}}
";

            ctx.AddSource($"{data.TypeNamespace}.{data.TypeName}.ContentRegistryPopulation.g.cs", code);
        }
    }
}
