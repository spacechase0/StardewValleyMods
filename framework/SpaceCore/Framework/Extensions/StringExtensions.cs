using System;
using System.Diagnostics.Contracts;

namespace SpaceCore.Framework.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminator">deliminator to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    [Pure]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char deliminator, int index = 0)
        => str.GetNthChunk(new[] { deliminator }, index);

    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminators">deliminators to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    /// <remarks>Inspired by the lovely Wren.</remarks>
    [Pure]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char[] deliminators, int index = 0)
    {
        if (index < 0 || index > str.Length + 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        int start = 0;
        int ind = 0;
        while (index-- >= 0)
        {
            ind = str.IndexOfAny(deliminators, start);
            if (ind == -1)
            {
                // since we've previously decremented index, check against -1;
                // this means we're done.
                if (index == -1)
                {
                    return str.AsSpan()[start..];
                }

                // else, we've run out of entries
                // and return an empty span to mark as failure.
                return ReadOnlySpan<char>.Empty;
            }

            if (index > -1)
            {
                start = ind + 1;
            }
        }
        return str.AsSpan()[start..ind];
    }
}
