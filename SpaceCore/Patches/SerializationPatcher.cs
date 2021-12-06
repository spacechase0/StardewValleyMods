using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Utility"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class SerializationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            Type type = AccessTools.TypeByName( "System.Xml.Serialization.StructModel" );

            harmony.Patch(
                original: AccessTools.Method(type, "GetMemberInfos"),
                postfix: this.GetHarmonyMethod(nameof(After_GetMemberInfos))
            );

            harmony.Patch(
                original: AccessTools.Method(type, "GetPropertyModel"),
                postfix: this.GetHarmonyMethod( nameof( After_GetPropertyModel ) )
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="StructModel.GetMemberInfos"/>.</summary>
        private static void After_GetMemberInfos( object __instance, ref MemberInfo[] __result)
        {
            var type = ( Type ) AccessTools.Field( __instance.GetType(), "_type" ).GetValue( __instance );
            if ( !SpaceCore.CustomProperties.ContainsKey( type ) )
                return;

            var ret = new List<MemberInfo>( __result );
            foreach ( var prop in SpaceCore.CustomProperties[ type ] )
            {
                ret.Add( prop.Value.GetFakePropertyInfo() );
            }
            __result = ret.ToArray();
        }

        /// <summary>The method to call after <see cref="StructModel.GetPropertyModel"/>.</summary>
        private static void After_GetPropertyModel( object __instance, PropertyInfo propertyInfo, ref object __result )
        {
            // This patch is necessary because it doesn't like static methods for the property getter
            // TODO: Test transpiling the above check (in CheckPropertyRead) out?

            if ( __result == null && SpaceCore.CustomProperties.ContainsKey( propertyInfo.DeclaringType ) && SpaceCore.CustomProperties[ propertyInfo.DeclaringType ].ContainsKey( propertyInfo.Name ) )
            {
                var myProp = SpaceCore.CustomProperties[ propertyInfo.DeclaringType ][ propertyInfo.Name ];

                var type = AccessTools.TypeByName( "System.Xml.Serialization.FieldModel" );
                var typeCParam = AccessTools.TypeByName( "System.Xml.Serialization.TypeDesc" );
                object modelScope = AccessTools.Property( __instance.GetType(), "ModelScope" ).GetValue( __instance );
                object typeScope = AccessTools.Property( modelScope.GetType(), "TypeScope" ).GetValue( modelScope );
                object typeDesc = AccessTools.Method( typeScope.GetType(), "GetTypeDesc",
                                                      new Type[] { typeof( Type ), typeof( MemberInfo ), typeof( bool ), typeof( bool ) } )
                                             .Invoke( typeScope, new object[] { propertyInfo.PropertyType, propertyInfo, true, false } );
                __result = AccessTools.Constructor( type, new Type[] { typeof( MemberInfo ), typeof( Type ), typeCParam } ).Invoke( new object[] { propertyInfo, myProp.PropertyType, typeDesc } );
            }
        }
    }
}
