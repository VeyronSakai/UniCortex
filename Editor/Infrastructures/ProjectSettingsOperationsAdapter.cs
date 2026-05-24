using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ProjectSettingsOperationsAdapter : IProjectSettingsOperations
    {
        // Settings assets live under the project-root "ProjectSettings" folder.
        // The Unity Editor's working directory is the project root, so a relative path is sufficient.
        private const string ProjectSettingsDirectory = "ProjectSettings";
        private const string AssetExtension = ".asset";

        public GetProjectSettingsCategoriesResponse GetCategories()
        {
            var entries = EnumerateAssetPaths()
                .Select(path => new ProjectSettingsCategoryEntry(
                    Path.GetFileNameWithoutExtension(path),
                    NormalizePath(path)))
                .OrderBy(entry => entry.name, StringComparer.Ordinal)
                .ToList();

            return new GetProjectSettingsCategoriesResponse(entries);
        }

        public GetProjectSettingsResponse GetSettings(string category)
        {
            var serializedObject = LoadSerializedSettings(category);
            var properties = new List<SerializedPropertyEntry>();

            // Iterate over all visible serialized properties.
            // enterChildren=true on the first call to enter the root,
            // then false to stay at the top level (skip child properties of compound types).
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                properties.Add(new SerializedPropertyEntry(
                    iterator.propertyPath,
                    iterator.propertyType.ToString(),
                    SerializedPropertyValueConverter.ToValueString(iterator)));
            }

            return new GetProjectSettingsResponse(category, properties);
        }

        public void SetSetting(string category, string propertyPath, string value)
        {
            var serializedObject = LoadSerializedSettings(category);
            var property = serializedObject.FindProperty(propertyPath);

            if (property == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyPath}' not found in ProjectSettings category '{category}'.");
            }

            // Parse the string value and write it into the SerializedProperty,
            // then apply the modification (records Undo) and persist the settings asset to disk.
            SerializedPropertyValueParser.ApplyValue(property, value);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private static IEnumerable<string> EnumerateAssetPaths()
        {
            if (!Directory.Exists(ProjectSettingsDirectory))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(ProjectSettingsDirectory, "*" + AssetExtension,
                SearchOption.TopDirectoryOnly);
        }

        private static string NormalizePath(string path)
        {
            // Unify path separators so the value is stable across OSes and matches
            // what AssetDatabase APIs expect (forward slashes).
            return path.Replace('\\', '/');
        }

        private static SerializedObject LoadSerializedSettings(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentException(
                    "ProjectSettings category is required. Use get_project_settings_categories to see valid names.");
            }

            var assetPath = NormalizePath(Path.Combine(ProjectSettingsDirectory, category + AssetExtension));

            if (!File.Exists(assetPath))
            {
                throw new ArgumentException(
                    $"Unknown ProjectSettings category '{category}'. Use get_project_settings_categories to see valid names.");
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            UnityEngine.Object settings = null;
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    if (asset != null)
                    {
                        settings = asset;
                        break;
                    }
                }
            }

            if (settings == null)
            {
                throw new ArgumentException(
                    $"Settings asset for category '{category}' not found at '{assetPath}'.");
            }

            return new SerializedObject(settings);
        }
    }
}
