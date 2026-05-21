using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ProjectSettingsOperationsAdapter : IProjectSettingsOperations
    {
        // Friendly category name (case-insensitive) -> ProjectSettings asset path.
        // Each asset is a native settings object that can be wrapped in a SerializedObject,
        // so the same SerializedProperty read/write flow used for components applies here.
        private static readonly Dictionary<string, string> s_categoryToAssetPath =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Player"] = "ProjectSettings/ProjectSettings.asset",
                ["Editor"] = "ProjectSettings/EditorSettings.asset",
                ["Graphics"] = "ProjectSettings/GraphicsSettings.asset",
                ["Quality"] = "ProjectSettings/QualitySettings.asset",
                ["Physics"] = "ProjectSettings/DynamicsManager.asset",
                ["Physics2D"] = "ProjectSettings/Physics2DSettings.asset",
                ["Tags"] = "ProjectSettings/TagManager.asset",
                ["Time"] = "ProjectSettings/TimeManager.asset",
                ["Audio"] = "ProjectSettings/AudioManager.asset",
                ["Input"] = "ProjectSettings/InputManager.asset",
                ["Preset"] = "ProjectSettings/PresetManager.asset",
                ["VFX"] = "ProjectSettings/VFXManager.asset",
                ["Navigation"] = "ProjectSettings/NavMeshAreas.asset",
                ["Memory"] = "ProjectSettings/MemorySettings.asset",
                ["EditorBuildSettings"] = "ProjectSettings/EditorBuildSettings.asset",
                ["ScriptExecutionOrder"] = "ProjectSettings/MonoManager.asset",
            };

        public ListProjectSettingsCategoriesResponse GetCategories()
        {
            var categories = new List<ProjectSettingsCategoryEntry>();
            foreach (var pair in s_categoryToAssetPath)
            {
                categories.Add(new ProjectSettingsCategoryEntry(pair.Key, pair.Value));
            }

            return new ListProjectSettingsCategoriesResponse(categories);
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

        private static SerializedObject LoadSerializedSettings(string category)
        {
            if (string.IsNullOrEmpty(category) || !s_categoryToAssetPath.TryGetValue(category, out var assetPath))
            {
                throw new ArgumentException(
                    $"Unknown ProjectSettings category '{category}'. Use list_project_settings_categories to see valid names.");
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
