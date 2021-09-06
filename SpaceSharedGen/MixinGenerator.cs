using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SpaceSharedGen
{
    [Generator]
    public class MixinGenerator : ISourceGenerator
    {
        private string log = "";

        private void Log( string str )
        {
            log += str + "\n";
        }

        public void Initialize( GeneratorInitializationContext context )
        {
        }

        // https://stackoverflow.com/a/65126680
        public void Execute( GeneratorExecutionContext context )
        {
            var attrSymbol = context.Compilation.GetTypeByMetadataName( "SpaceShared.MixinAttribute" );
            var withAttrs = context.Compilation.SyntaxTrees.Where( st => st.GetRoot().DescendantNodes().OfType< ClassDeclarationSyntax >()
                                                                           .Any( p => p.DescendantNodes().OfType< AttributeSyntax >().Any() ) );

            log = "";

            try
            {
                foreach ( var tree in withAttrs )
                {
                    var semModel = context.Compilation.GetSemanticModel( tree );

                    Log( "@ " + tree.FilePath );

                    foreach ( var declClass in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Where( cd => cd.DescendantNodes().OfType<AttributeSyntax>().Any() ) )
                    {
                        var nodes = declClass.DescendantNodes().OfType< AttributeSyntax >()
                                             .FirstOrDefault( a => a.DescendantTokens().Any( dt => dt.IsKind( SyntaxKind.IdentifierToken ) &&
                                                                                                   semModel.GetTypeInfo( dt.Parent ).Type?.Name == attrSymbol.Name ) )
                                             ?.DescendantTokens()?.Where( dt => dt.IsKind( SyntaxKind.IdentifierToken ) )?.ToList();
                        if ( nodes == null )
                            continue;

                        Log( $"\t{( declClass.Parent as NamespaceDeclarationSyntax ).Name}.{declClass.Identifier}" );
                        var mixerClass = context.Compilation.GetTypeByMetadataName( (declClass.Parent as NamespaceDeclarationSyntax).Name + "." + declClass.Identifier.Text );
                        var mixinClass = semModel.GetTypeInfo( nodes[ 1 ].Parent );
                        string mixedClass = GenerateMix( context, ( declClass.Parent as NamespaceDeclarationSyntax ).Name.ToString(), mixerClass.Name, mixinClass, nodes );

                        context.AddSource( $"{declClass.Identifier}_MixedWith_{mixinClass.Type.Name}", SourceText.From( mixedClass, Encoding.UTF8 ) );
                    }
                }
            }
            catch ( Exception e )
            {
                Log( $"Exception! {e}" );
                //Debugger.Launch();
            }

            context.AddSource( "mixinlog", "/*" + log + "*/" );
        }

        private string GenerateMix( GeneratorExecutionContext context, string mixerNamespace, string mixerName, TypeInfo mixinClass, List<SyntaxToken> mixinNodes )
        {
            string fullMixinBase = mixinNodes[ 1 ].Text;
            for ( int i = 2; i < mixinNodes.Count; ++i )
            {
                if ( i == 2 )
                    fullMixinBase += "<";

                fullMixinBase += mixinNodes[ i ];

                if ( i == mixinNodes.Count - 1 )
                    fullMixinBase += ">";
                else
                    fullMixinBase += ", ";
            }

            string mixinFull = $"{mixinClass.Type.ContainingNamespace}.{mixinClass.Type.Name}";
            Log( $"\tMaking mix for {mixerNamespace}.{mixerName} - {mixinFull} - {fullMixinBase}" );

            var ret = new StringBuilder();

            var mixinTree = context.Compilation.SyntaxTrees.First( st => st.GetRoot().DescendantNodes().OfType< ClassDeclarationSyntax >().Any( cd => $"{(cd.Parent as NamespaceDeclarationSyntax)?.Name}.{cd.Identifier}" == mixinFull ) );
            var mixin = mixinTree.GetRoot().DescendantNodes().OfType< ClassDeclarationSyntax >().First( cd => $"{(cd.Parent as NamespaceDeclarationSyntax).Name}.{cd.Identifier}" == mixinFull );

            var root = mixinTree.GetRoot() as CompilationUnitSyntax;
            foreach ( var use in root.Usings )
                ret.AppendLine( use.ToString() );

            ret.Append( $@"
namespace {mixerNamespace}
{{
    public partial class {mixerName}" );

            bool didColon = false;
            if ( mixinClass.Type.BaseType != null && mixinClass.Type.BaseType.SpecialType != SpecialType.System_Object )
            {
                ret.Append( $" : {mixinClass.Type.BaseType.Name}" );
                didColon = true;
            }
            if ( mixinClass.Type.AllInterfaces.Length > 0 )
            {
                if ( !didColon )
                    ret.Append( " : " );

                for ( int i = 0; i < mixinClass.Type.AllInterfaces.Length; ++i )
                {
                    if ( didColon && i == 0 || i > 0 )
                        ret.Append( ", " );
                    ret.Append( mixinClass.Type.AllInterfaces[ i ].Name );
                }

                didColon = true;
            }

            ret.AppendLine( $@"
    {{" );

            var mixType = mixinClass.Type as INamedTypeSymbol;

            Dictionary<string, string> tParams = new();
            for ( int i = 0; i < mixType.TypeParameters.Length; ++i )
            {
                tParams.Add( mixType.TypeParameters[ i ].OriginalDefinition.Name, mixinNodes[ i + 2 ].Text );
            }

            foreach ( var member in mixin.Members )
            {
                Log( "\t\tDoing member " + member.GetType() );
                string memberStr = "\t\t" + member.ToString();
                memberStr = memberStr.Replace( mixType.Name, mixerName ); // TODO: Do this smarter (not blind replace)
                foreach ( var entry in tParams )
                {
                    memberStr = memberStr.Replace( entry.Key, entry.Value ); // TODO: Do this smarter (not blind replace)
                }
                ret.AppendLine( memberStr );
            }

            ret.AppendLine( $@"
    }}
}}" );

            return ret.ToString();
        }
    }
}
