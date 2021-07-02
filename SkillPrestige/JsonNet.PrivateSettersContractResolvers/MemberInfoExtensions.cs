using System.Reflection;

// ReSharper disable once CheckNamespace
namespace JsonNet.PrivateSettersContractResolvers
{
    /// <summary>
    /// Extension methods for member info for Json.Net.
    /// </summary>
    internal static class MemberInfoExtensions
    {
        /// <summary>
        /// Checks to see if a property has a setter.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns></returns>
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }
}