using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpaceShared.SourceGenerator
{
    [Generator]
    public class FileAssetSourceGenerator : IIncrementalGenerator
    {
        private record struct GenerationData(string? Namespace, string Name, string? TypeNamespace, string TypeName, string PropName, string AssetName, string? PipelineName );

        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var prov = ctx.SyntaxProvider.CreateSyntaxProvider(
                static (s, _) => (s is PropertyDeclarationSyntax propDecl && propDecl.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString().StartsWith("FileAsset") || a.Name.ToString().StartsWith("SpaceShared.Attributes.FileAsset")))),//static t => (t is SimpleBaseTypeSyntax simpleType && simpleType.Type is SimpleNameSyntax simpleName && ( simpleName.Identifier.ValueText?.StartsWith( "BaseMod<" ) ?? false )))),
                static (ctx, _) =>
                {
                    var propDecl = (ctx.Node as PropertyDeclarationSyntax)!;
                    var propSym = ctx.SemanticModel.GetDeclaredSymbol(propDecl) as IPropertySymbol;
                    var classSym = propSym?.ContainingType;

                    string classTypeNameStr = classSym?.BaseType?.Name;
                    if (classTypeNameStr == null
                        || (!classTypeNameStr.StartsWith("BaseAssetInstances") && !classTypeNameStr.StartsWith("SpaceShared.Content.BaseAssetInstances")))
                        return new GenerationData();

                    string? ns, typeNs;
                    string name, typeName;
                    {
                        ns = classSym.ContainingNamespace?.Name;
                        var checkNs = classSym.ContainingNamespace;
                        while (checkNs != null && checkNs.ContainingNamespace != null && !checkNs.ContainingNamespace.IsGlobalNamespace)
                        {
                            if (!string.IsNullOrEmpty(checkNs.ContainingNamespace.Name))
                                ns = $"{checkNs.ContainingNamespace.Name}.{ns}";
                            checkNs = checkNs.ContainingNamespace;
                        }
                        name = classSym.Name;
                    }
                    {
                        var fieldTypeSym = propSym.Type;

                        typeNs = fieldTypeSym.ContainingNamespace?.Name;
                        var checkNs = fieldTypeSym.ContainingNamespace;
                        while (checkNs != null && checkNs.ContainingNamespace != null && !checkNs.ContainingNamespace.IsGlobalNamespace)
                        {
                            if (!string.IsNullOrEmpty(checkNs.ContainingNamespace.Name))
                                typeNs = $"{checkNs.ContainingNamespace.Name}.{typeNs}";
                            checkNs = checkNs.ContainingNamespace;
                        }
                        typeName = fieldTypeSym.Name;
                    }

                    var attr = propSym?.GetAttributes().First(a => a.AttributeClass.Name == "FileAssetAttribute");
                    string assetName = attr.ConstructorArguments[0].Value?.ToString();
                    string? pipelineName = null;
                    if (attr?.ConstructorArguments.Length >= 2)
                        pipelineName = attr.ConstructorArguments[1].Value?.ToString();

                    return new GenerationData(ns, name, typeNs, typeName, propSym.Name, assetName, pipelineName);
                })
                .Where( static data => !string.IsNullOrEmpty( data.AssetName ) );

            ctx.RegisterSourceOutput(prov, DoGenerate);
        }

        private void DoGenerate( SourceProductionContext ctx, GenerationData data )
        {
            char[] fullTypeNameChars = $"{data.Namespace}.{data.Name}".ToCharArray();
            for (int i = 0; i < fullTypeNameChars.Length; i++)
            {
                char c = fullTypeNameChars[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    fullTypeNameChars[i] = '_';
            }
            string fullTypeNameSafe = new string(fullTypeNameChars);

            char[] fullFieldTypeNameChars = $"{data.TypeNamespace}.{data.TypeName}".ToCharArray();
            for (int i = 0; i < fullFieldTypeNameChars.Length; i++)
            {
                char c = fullFieldTypeNameChars[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    fullFieldTypeNameChars[i] = '_';
            }
            string fullFieldTypeNameSafe = new string(fullFieldTypeNameChars);

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
using StardewModdingAPI;
using SpaceShared;
using SpaceShared.ContentAssets;

namespace {data.Namespace}
{{
    public partial class {data.Name}
    {{
        private BaseAssetInstances.FileAssetRegisterer _FileAsset_{data.PropName} = new(""{data.PropName}"", ""{data.AssetName}"", typeof( {data.TypeNamespace}.{data.TypeName} ), ""{(string.IsNullOrEmpty(data.PipelineName) ? null : data.PipelineName)}"" );
    }}
}}
";

            ctx.AddSource($"{data.Namespace}.{data.Name}.Asset.{data.PropName}.g.cs", code);
        }
    }
}
