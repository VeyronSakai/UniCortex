using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    internal static class SceneSearchQueryParser
    {
        private static readonly string[] s_prefixes =
        {
            "t:", "tag=", "tag:", "id=", "id:", "layer:", "path:", "is:"
        };

        public static SceneSearchQuery Parse(string query)
        {
            var result = new SceneSearchQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            var tokens = Tokenize(query);

            foreach (var token in tokens)
            {
                if (token.StartsWith("t:", StringComparison.OrdinalIgnoreCase))
                {
                    result.componentTypePattern = token.Substring(2);
                }
                else if (token.StartsWith("tag=", StringComparison.OrdinalIgnoreCase))
                {
                    result.tagExact = token.Substring(4);
                }
                else if (token.StartsWith("tag:", StringComparison.OrdinalIgnoreCase))
                {
                    result.tagPartial = token.Substring(4);
                }
                else if (token.StartsWith("id=", StringComparison.OrdinalIgnoreCase) ||
                         token.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
                {
                    var value = token.Substring(3);
                    if (int.TryParse(value, out var id))
                    {
                        result.instanceId = id;
                    }
                }
                else if (token.StartsWith("layer:", StringComparison.OrdinalIgnoreCase))
                {
                    var value = token.Substring(6);
                    if (int.TryParse(value, out var layerValue))
                    {
                        result.layer = layerValue;
                    }
                }
                else if (token.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                {
                    result.pathPattern = token.Substring(5);
                }
                else if (token.StartsWith("is:", StringComparison.OrdinalIgnoreCase))
                {
                    result.stateFilters.Add(token.Substring(3).ToLowerInvariant());
                }
                else
                {
                    // Plain text = name pattern
                    if (result.namePattern == null)
                    {
                        result.namePattern = token;
                    }
                    else
                    {
                        result.namePattern += " " + token;
                    }
                }
            }

            return result;
        }

        private static List<string> Tokenize(string query)
        {
            var tokens = new List<string>();
            var i = 0;
            while (i < query.Length)
            {
                // Skip whitespace
                while (i < query.Length && char.IsWhiteSpace(query[i]))
                {
                    i++;
                }

                if (i >= query.Length) break;

                // Check if current position starts a known prefix
                var prefixFound = false;
                foreach (var prefix in s_prefixes)
                {
                    if (i + prefix.Length <= query.Length &&
                        query.Substring(i, prefix.Length).Equals(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        // Read until next whitespace or next known prefix
                        var start = i;
                        i += prefix.Length;
                        while (i < query.Length && !char.IsWhiteSpace(query[i]))
                        {
                            i++;
                        }

                        tokens.Add(query.Substring(start, i - start));
                        prefixFound = true;
                        break;
                    }
                }

                if (!prefixFound)
                {
                    // Plain text token: read until whitespace or known prefix
                    var start = i;
                    i++;
                    while (i < query.Length && !char.IsWhiteSpace(query[i]) && !StartsWithPrefix(query, i))
                    {
                        i++;
                    }

                    tokens.Add(query.Substring(start, i - start));
                }
            }

            return tokens;
        }

        private static bool StartsWithPrefix(string query, int index)
        {
            foreach (var prefix in s_prefixes)
            {
                if (index + prefix.Length <= query.Length &&
                    query.Substring(index, prefix.Length).Equals(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
