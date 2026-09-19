using System;
using System.Collections.Generic;

namespace BuddyBee.Api.Services
{
    /// <summary>
    /// Scoped per-request container holding client-supplied provider credentials.
    /// Extensible to future providers (e.g. Grok, Anthropic) via key-value mapping.
    /// </summary>
    public class ProviderKeyContext
    {
        public Dictionary<string, string> UserKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// True if the current request supplied at least one user provider key.
        /// When true, developer/server keys must NEVER be substituted for missing provider keys.
        /// </summary>
        public bool HasUserKeys => UserKeys.Count > 0;

        /// <summary>
        /// Returns the user-supplied key for the specified provider, or null if not provided.
        /// </summary>
        public string? GetUserKey(string provider)
        {
            return UserKeys.TryGetValue(provider, out var key) && !string.IsNullOrWhiteSpace(key)
                ? key
                : null;
        }
    }
}
