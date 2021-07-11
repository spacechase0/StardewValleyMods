using System.Diagnostics.CodeAnalysis;

namespace SpaceShared
{
    /// <summary>Contains common diagnostic messages for use with <see cref="SuppressMessageAttribute"/>.</summary>
    internal static class DiagnosticMessages
    {
        /// <summary>The code was copied from the decompiled game, so we should avoid refactoring to simplify future updates.</summary>
        public const string CopiedFromGameCode = "The code was copied from the decompiled game, so we should avoid refactoring to simplify future updates.";

        /// <summary>The class or member is used via reflection.</summary>
        public const string IsUsedViaReflection = "The class or member is used via reflection.";

        /// <summary>The class is part of a mod's API, so we can't make breaking changes.</summary>
        public const string IsPublicApi = "The class is part of a mod's API, so we can't make breaking changes.";

        /// <summary>The parameter names must match the convention defined by Harmony so it can find them.</summary>
        public const string NamedForHarmony = "The parameter names must match the convention defined by Harmony so it can find them.";

        /// <summary>The disposable object can't be disposed since it survives past the end of this scope.</summary>
        public const string DisposableOutlivesScope = "The disposable object can't be disposed since it survives past the end of this scope.";
    }
}
