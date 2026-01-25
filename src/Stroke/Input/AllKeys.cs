// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Stroke.Input;

/// <summary>
/// Provides a collection of all valid canonical key strings.
/// </summary>
/// <remarks>
/// <para>
/// This class provides access to all canonical key string values, matching Python
/// Prompt Toolkit's <c>ALL_KEYS</c> list. This is useful for validation, UI display,
/// and documentation generation.
/// </para>
/// <para>
/// The collection excludes aliases - only canonical key strings are included.
/// For alias resolution, use <see cref="KeyAliasMap"/>.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. The <see cref="Values"/> collection
/// is immutable and initialized via static constructor.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get all canonical key strings
/// IReadOnlyList&lt;string&gt; allKeys = AllKeys.Values;
///
/// // Check if a string is a valid key
/// bool isValid = allKeys.Contains("c-a");  // true
/// bool invalid = allKeys.Contains("foo");   // false
///
/// // Count available keys
/// int count = allKeys.Count;  // 143
/// </code>
/// </example>
public static class AllKeys
{
    /// <summary>
    /// Gets all canonical key string values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This collection contains the canonical string representation for each
    /// <see cref="Keys"/> enum value. The order matches the enum definition order.
    /// </para>
    /// <para>
    /// The collection contains exactly 143 entries (one for each Keys enum value).
    /// Aliases are not included in this collection.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<string> Values { get; }

    /// <summary>
    /// Static constructor to initialize the Values collection.
    /// </summary>
    static AllKeys()
    {
        Values = Enum.GetValues<Keys>()
            .Select(k => k.ToKeyString())
            .ToImmutableArray();
    }
}
