using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UniCortex.Editor.Infrastructures
{
    // A find_game_objects query split into the part handed to Unity Search and the scene:<name> filters.
    internal sealed class GameObjectQuery
    {
        private static readonly Regex s_sceneTokenPattern = new Regex(
            @"(?<!\S)scene:(?:""(?<name>[^""]*)""|(?<name>\S+))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex s_whitespacePattern = new Regex(@"\s+");

        public string SearchQuery { get; }
        public IReadOnlyList<string> SceneNames { get; }

        private GameObjectQuery(string searchQuery, IReadOnlyList<string> sceneNames)
        {
            SearchQuery = searchQuery;
            SceneNames = sceneNames;
        }

        public static GameObjectQuery Parse(string query)
        {
            var sceneNames = new List<string>();
            var searchQuery = s_sceneTokenPattern.Replace(query ?? string.Empty, match =>
            {
                sceneNames.Add(match.Groups["name"].Value);
                return " ";
            });

            searchQuery = s_whitespacePattern.Replace(searchQuery, " ").Trim();
            return new GameObjectQuery(searchQuery, sceneNames);
        }

        public bool MatchesScene(string sceneName)
        {
            if (SceneNames.Count == 0)
            {
                return true;
            }

            foreach (var name in SceneNames)
            {
                if (string.Equals(name, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
