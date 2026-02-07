using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpaceShared.SourceGenerator
{
    [Generator]
    public class HasHarmonySourceGenerator : IIncrementalGenerator
    {
        private record struct GenerationData( string? Namespace, string Name );

        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var prov = ctx.SyntaxProvider.CreateSyntaxProvider(
                static (s, _) => (s is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Any( al => al.Attributes.Any( a => a.Name.ToString().StartsWith( "HasHarmony" ) || a.Name.ToString().StartsWith("SpaceShared.Attributes.HasHarmony") ) ) && classDecl.BaseList.Types.Any()),//static t => (t is SimpleBaseTypeSyntax simpleType && simpleType.Type is SimpleNameSyntax simpleName && ( simpleName.Identifier.ValueText?.StartsWith( "BaseMod<" ) ?? false )))),
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
                        if (typeNameStr.StartsWith( "BaseMod<" ) || typeNameStr.StartsWith( "SpaceShared.BaseMod<" ) )
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

                    return new GenerationData(ns, name);
                })
                .Where( static data => !string.IsNullOrEmpty( data.Name ) );

            ctx.RegisterSourceOutput(prov, DoGenerate);
        }

        private void DoGenerate( SourceProductionContext ctx, GenerationData data )
        {
            var code = $@"
using System;
using System.Reflection;
using StardewModdingAPI;
using HarmonyLib;
using SpaceShared;

namespace {data.Namespace};

public partial class {data.Name}
{{
    protected Harmony Harmony {{ get; private set; }}
    protected override void SetupHarmony()
    {{
        Harmony = new Harmony( ModManifest.UniqueID );
        Harmony.PatchAll( Assembly.GetExecutingAssembly() );
    }}
}}
";

            ctx.AddSource($"{data.Namespace}.{data.Name}.WithHarmony.g.cs", code);
        }
    }
}
