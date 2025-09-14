using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace SpaceShared.APIs
{
#nullable enable
    /// <summary>A set of parsed conditions linked to the Content Patcher context. These conditions are <strong>per-screen</strong>, so the result depends on the screen that's active when calling the members.</summary>
    public interface IManagedConditions
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the conditions were parsed successfully (regardless of whether they're in scope currently).</summary>
        [MemberNotNullWhen(false, nameof(IManagedConditions.ValidationError))]
        bool IsValid { get; }

        /// <summary>If <see cref="IsValid"/> is false, an error phrase indicating why the conditions failed to parse, formatted like this: <c>'seasonz' isn't a valid token name; must be one of &lt;token list&gt;</c>. If the conditions are valid, this is <c>null</c>.</summary>
        string? ValidationError { get; }

        /// <summary>Whether the conditions' tokens are all valid in the current context. For example, this would be false if the conditions use <c>Season</c> and a save isn't loaded yet.</summary>
        bool IsReady { get; }

        /// <summary>Whether <see cref="IsReady"/> is true, and the conditions all match in the current context.</summary>
        bool IsMatch { get; }

        /// <summary>Whether <see cref="IsMatch"/> may change depending on the context. For example, <c>Season</c> is mutable since it depends on the in-game season. <c>HasMod</c> is not mutable, since it can't change after the game is launched.</summary>
        bool IsMutable { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Update the conditions based on Content Patcher's current context for every active screen. It's safe to call this as often as you want, but it has no effect if the Content Patcher context hasn't changed since you last called it.</summary>
        /// <returns>Returns the screens for which <see cref="IsMatch"/> changed value. To check if the current screen changed, you can check <c>UpdateContext()</c></returns>
        IEnumerable<int> UpdateContext();

        /// <summary>If <see cref="IsMatch"/> is false, analyze the conditions/context and get a human-readable reason phrase explaining why the conditions don't match the context. For example: <c>conditions don't match: season</c>. If the conditions do match, this returns <c>null</c>.</summary>
        string? GetReasonNotMatched();
    }

    /// <summary>A set of parsed values linked to the Content Patcher context. These values are <strong>per-screen</strong>, so the result depends on the screen that's active when calling the members.</summary>
    public interface IManagedValue
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the conditions were parsed successfully (regardless of whether they're in scope currently).</summary>
        [MemberNotNullWhen(false, nameof(IManagedValue.ValidationError))]
        bool IsValid { get; }

        /// <summary>If <see cref="IsValid"/> is false, an error phrase indicating why the conditions failed to parse, formatted like this: <c>'seasonz' isn't a valid token name; must be one of &lt;token list&gt;</c>. If the conditions are valid, this is <c>null</c>.</summary>
        string? ValidationError { get; }

        /// <summary>Whether the conditions' tokens are all valid in the current context. For example, this would be false if the conditions use <c>Season</c> and a save isn't loaded yet.</summary>
        bool IsReady { get; }

        /// <summary>The value(s) provided at the current context.</summary>
        string? Value { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Update the conditions based on Content Patcher's current context for every active screen. It's safe to call this as often as you want, but it has no effect if the Content Patcher context hasn't changed since you last called it.</summary>
        /// <returns>Returns the screens for which <see cref="Values"/> changed value. To check if the current screen changed, you can check <c>UpdateContext()</c></returns>
        IEnumerable<int> UpdateContext();
    }


    /// <summary>A parsed string which may contain Content Patcher tokens matched against Content Patcher's internal context for an API consumer. This value is <strong>per-screen</strong>, so the result depends on the screen that's active when calling the members.</summary>
    public interface IManagedTokenString
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the token string was parsed successfully (regardless of whether its tokens are in scope currently).</summary>
        [MemberNotNullWhen(false, nameof(IManagedTokenString.ValidationError))]
        bool IsValid { get; }

        /// <summary>If <see cref="IsValid"/> is false, an error phrase indicating why the token string failed to parse, formatted like this: <c>'seasonz' isn't a valid token name; must be one of &lt;token list&gt;</c>. If the token string is valid, this is <c>null</c>.</summary>
        string? ValidationError { get; }

        /// <summary>Whether the token string's tokens are all valid in the current context. For example, this would be false if the token string use <c>{{Season}}</c> and a save isn't loaded yet.</summary>
        bool IsReady { get; }

        /// <summary>The parsed value for the current context.</summary>
        string? Value { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Update the token string based on Content Patcher's current context for every active screen. It's safe to call this as often as you want, but it has no effect if the Content Patcher context hasn't changed since you last called it.</summary>
        /// <returns>Returns the screens for which <see cref="Value"/> changed.</returns>
        IEnumerable<int> UpdateContext();
    }

    /// <summary>The Content Patcher API which other mods can access.</summary>
    public interface IContentPatcherApi
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the conditions API is initialized and ready for use.</summary>
        /// <remarks>Due to the Content Patcher lifecycle, the conditions API becomes available roughly two ticks after the <see cref="IGameLoopEvents.GameLaunched"/> event.</remarks>
        bool IsConditionsApiReady { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get a set of managed conditions which are matched against Content Patcher's internal context.</summary>
        /// <param name="manifest">The manifest of the mod parsing the conditions (see <see cref="Mod.ModManifest"/> in your entry class).</param>
        /// <param name="rawConditions">The conditions to parse, in the same format as <c>When</c> blocks in Content Patcher content packs.</param>
        /// <param name="formatVersion">The format version for which to parse conditions, used to ensure forward compatibility with future Content Patcher versions. See <c>Format</c> in the Content Patcher token documentation.</param>
        /// <param name="assumeModIds">
        /// <para>The unique IDs of mods whose custom tokens to allow in the <paramref name="rawConditions"/>. You don't need to list the mod identified by <paramref name="manifest"/>, mods listed as a required dependency in the <paramref name="manifest"/>, or mods identified by a <c>HasMod</c> condition in the <paramref name="rawConditions"/>.</para>
        /// <para>NOTE: this is meant to prevent mods from breaking if a player doesn't have a required mod installed. You shouldn't simply list all installed mods, and parsing conditions will still fail if a mod isn't installed regardless of the listed mod IDs.</para>
        /// </param>
        IManagedConditions ParseConditions(IManifest manifest, IDictionary<string, string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null);

        /// <summary>Get a managed string which may contain Content Patcher tokens matched against Content Patcher's internal context.</summary>
        /// <param name="manifest">The manifest of the mod parsing the token string (see <see cref="Mod.ModManifest"/> in your entry class).</param>
        /// <param name="rawValue">The token string to parse, in the same format as strings in Content Patcher content packs.</param>
        /// <param name="formatVersion">The format version for which to parse the token string, used to ensure forward compatibility with future Content Patcher versions. See <c>Format</c> in the Content Patcher token documentation.</param>
        /// <param name="assumeModIds">
        /// <para>The unique IDs of mods whose custom tokens to allow in the <paramref name="rawValue"/>. You don't need to list the mod identified by <paramref name="manifest"/> or mods listed as a required dependency in the <paramref name="manifest"/>.</para>
        /// <para>NOTE: this is meant to prevent mods from breaking if a player doesn't have a required mod installed. You shouldn't simply list all installed mods, and parsing conditions will still fail if a mod isn't installed regardless of the listed mod IDs.</para>
        /// </param>
        IManagedTokenString ParseTokenString(IManifest manifest, string rawValue, ISemanticVersion formatVersion, string[]? assumeModIds = null);

        /// <summary>Register a simple token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> in your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>YourName.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="getValue">A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.</param>
        void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue);

        /// <summary>Register a complex token. This is an advanced API; only use this method if you've read the documentation and are aware of the consequences.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> in your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>YourName.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="token">An arbitrary class with one or more methods from <see cref="ConventionDelegates"/>.</param>
        void RegisterToken(IManifest mod, string name, object token);
    }

#nullable disable
}
