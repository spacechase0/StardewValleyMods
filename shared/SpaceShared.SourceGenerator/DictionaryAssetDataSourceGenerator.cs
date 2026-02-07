using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpaceShared.SourceGenerator
{
    [Generator]
    public class DictionaryAssetDataSourceGenerator : IIncrementalGenerator
    {
        private record struct GenerationData( string? TypeNamespace, string TypeName, string AssetName );

        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var prov = ctx.SyntaxProvider.CreateSyntaxProvider(
                static (s, _) => (s is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Any( al => al.Attributes.Any( a => a.Name.ToString().StartsWith("DictionaryAssetData") || a.Name.ToString().StartsWith("SpaceShared.Attributes.DictionaryAssetData") ) ) && classDecl.BaseList.Types.Any()),//static t => (t is SimpleBaseTypeSyntax simpleType && simpleType.Type is SimpleNameSyntax simpleName && ( simpleName.Identifier.ValueText?.StartsWith( "BaseMod<" ) ?? false )))),
                static (ctx, _) =>
                {
                    var classDecl = ctx.Node as ClassDeclarationSyntax;
                    var classSym = ctx.SemanticModel.GetDeclaredSymbol( classDecl ) as ITypeSymbol;

                    bool valid = false;
                    foreach (var baseType in classDecl.BaseList.Types)
                    {
                        if (baseType.Type is not NameSyntax typeName)
                            continue;

                        string typeNameStr = typeName.ToString();
                        if (typeNameStr.StartsWith("BaseDictionaryAssetData") || typeNameStr.StartsWith("SpaceShared.Content.BaseDictionaryAssetData") )
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (!valid)
                    {
                        return new GenerationData();
                    }

                    string? ns = classSym.ContainingNamespace?.Name;
                    var checkNs = classSym.ContainingNamespace;
                    while (checkNs != null && checkNs.ContainingNamespace != null && !checkNs.ContainingNamespace.IsGlobalNamespace)
                    {
                        if (!string.IsNullOrEmpty(checkNs.ContainingNamespace.Name))
                            ns = $"{checkNs.ContainingNamespace.Name}.{ns}";
                        checkNs = checkNs.ContainingNamespace;
                    }
                    string name = classSym.Name;
                    var attr = classSym.GetAttributes().First(a => a.AttributeClass.Name == "DictionaryAssetDataAttribute");
                    if ( attr.ConstructorArguments.Length == 0 )
                        return new GenerationData();

                    return new GenerationData(ns, name, attr.ConstructorArguments[0].Value?.ToString() );
                })
                .Where( static data => !string.IsNullOrEmpty( data.AssetName ) );

            ctx.RegisterSourceOutput(prov, DoGenerate);
        }

        private void DoGenerate( SourceProductionContext ctx, GenerationData data )
        {
            char[] fullTypeNameChars = $"{data.TypeNamespace}.{data.TypeName}".ToCharArray();
            for (int i = 0; i < fullTypeNameChars.Length; i++)
            {
                char c = fullTypeNameChars[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    fullTypeNameChars[i] = '_';
            }
            string fullTypeNameSafe = new string(fullTypeNameChars);

            var code = $@"
using System;
using StardewModdingAPI;
using SpaceShared;
using SpaceShared.Content;

namespace SpaceShared.Content
{{
    internal static partial class ContentRegistry
    {{
        private static ContentRegistry.DictionaryDataRegisterer _DictionaryAssetData_{fullTypeNameSafe} = new( ""{data.AssetName}"", typeof( {data.TypeNamespace}.{data.TypeName} ) );
    }}
}}
";

            ctx.AddSource($"{data.TypeNamespace}.{data.TypeName}.ContentRegistryPopulation.g.cs", code);
        }
    }
}
