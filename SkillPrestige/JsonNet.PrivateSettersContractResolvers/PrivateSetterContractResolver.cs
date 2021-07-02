using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace JsonNet.PrivateSettersContractResolvers
{
    /// <summary>
    /// Json Contract resolver to set values of private fields when deserializing Json data.
    /// </summary>
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.Writable) return property;

            property.Writable = member.IsPropertyWithSetter();

            return property;
        }
    }
}