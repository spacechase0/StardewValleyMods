using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
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
        private static Type typeMember;

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                Type type = AccessTools.TypeByName("System.Xml.Serialization.StructModel");

                harmony.Patch(
                    original: AccessTools.Method(type, "GetMemberInfos"),
                    postfix: this.GetHarmonyMethod(nameof(After_GetMemberInfos))
                );

                harmony.Patch(
                    original: AccessTools.Method(type, "GetPropertyModel"),
                    postfix: this.GetHarmonyMethod(nameof(After_GetPropertyModel))
                );
            }
            else
            {
                Type importer = AccessTools.TypeByName("System.Xml.Serialization.XmlReflectionImporter");
                Type typedata = AccessTools.TypeByName("System.Xml.Serialization.TypeData");

                typeMember = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");

                harmony.Patch(
                    original: AccessTools.Method(importer, "ImportClassMapping", new Type[] { typedata, typeof(XmlRootAttribute), typeof(string), typeof(bool) } ),
                    postfix: this.GetHarmonyMethod(nameof(Android_After_ImportClassMapping))
                );

                harmony.Patch(
                    original: AccessTools.Method(typeMember, "GetValue", new Type[] { typeof(object) }),
                    prefix: this.GetHarmonyMethod(nameof(Android_Before_GetValue))
                );

                harmony.Patch(
                    original: AccessTools.Method(typeMember, "SetValue", new Type[] { typeof(object), typeof(object) }),
                    prefix: this.GetHarmonyMethod(nameof(Android_Before_SetValue))
                );
            }
        }

        /*********
         * Android methods
         */
        public static bool Android_Before_SetValue(object __instance, object ob, object value, string ____name )
        {
            if (!SpaceCore.CustomProperties.TryGetValue(ob.GetType(), out var props))
                return true;

            if (props.TryGetValue(____name, out var prop))
            {
                props[____name].Setter.Invoke(ob, new object[] { value } );
                return false;
            }

            return true;
        }
        public static bool Android_Before_GetValue(object __instance, object ob, string ____name, ref object __result)
        {
            if (!SpaceCore.CustomProperties.TryGetValue(ob.GetType(), out var props))
                return true;

            if (props.TryGetValue(____name, out var prop))
            {
                __result = props[____name].Getter.Invoke(ob, new object[0]);
                return false;
            }

            return true;
        }

        public static void Android_After_ImportClassMapping(XmlTypeMapping __instance, object typeData)
        {
            var type = (Type) AccessTools.Field(typeData.GetType(), "type").GetValue( typeData );
            if (!SpaceCore.CustomProperties.ContainsKey(type))
                return;

            var memberType = AccessTools.TypeByName("System.Xml.Serialization.XmlTypeMapMember");
            var memberTypeConstructor = memberType.GetConstructor(new Type[0]);

            object map = AccessTools.Field(typeof(XmlTypeMapping), "map").GetValue( __instance );
            var mapAddMethod = AccessTools.Method(map.GetType(), "AddMember");
            foreach (var prop in SpaceCore.CustomProperties[type])
            {
                object member = memberTypeConstructor.Invoke(new object[0]);
                AccessTools.Property(memberType, "Name").SetValue(member, prop.Key);
                AccessTools.Property(memberType, "TypeData").SetValue(member, AccessTools.Method("System.Xml.Serialization.TypeTranslator:GetTypeData", new Type[] { typeof(Type) }).Invoke(null, new object[] { prop.Value.PropertyType }));
                mapAddMethod.Invoke(map, new object[] { member });
            }
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
