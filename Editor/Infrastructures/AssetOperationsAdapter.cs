using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class AssetOperationsAdapter : IAssetOperations
    {
        public void Refresh()
        {
            AssetDatabase.Refresh();
        }

        public void CreateAsset(string type, string assetPath)
        {
            Object asset;

            if (string.Equals(type, "Material", StringComparison.OrdinalIgnoreCase))
            {
                asset = new Material(Shader.Find("Standard"));
            }
            else
            {
                var scriptableType = ResolveScriptableObjectType(type);
                if (scriptableType == null)
                {
                    throw new ArgumentException($"Unsupported asset type: {type}");
                }

                asset = ScriptableObject.CreateInstance(scriptableType);
            }

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }

        public AssetInfoResponse GetInfo(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                throw new ArgumentException($"Asset not found at path: {assetPath}");
            }

            var serializedObject = new SerializedObject(asset);
            var properties = new List<SerializedPropertyEntry>();

            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                properties.Add(new SerializedPropertyEntry(
                    iterator.propertyPath,
                    iterator.propertyType.ToString(),
                    GetPropertyValueAsString(iterator)));
            }

            return new AssetInfoResponse(assetPath, asset.GetType().Name, properties);
        }

        public void SetProperty(string assetPath, string propertyPath, string value)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                throw new ArgumentException($"Asset not found at path: {assetPath}");
            }

            var serializedObject = new SerializedObject(asset);
            var property = serializedObject.FindProperty(propertyPath);

            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyPath}' not found on asset at '{assetPath}'.");
            }

            SetPropertyValue(property, value);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private static string GetPropertyValueAsString(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString().ToLower();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString();
                case SerializedPropertyType.String:
                    return property.stringValue ?? "";
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.enumValueIndex.ToString();
                case SerializedPropertyType.Color:
                    var c = property.colorValue;
                    return $"({c.r}, {c.g}, {c.b}, {c.a})";
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null
                        ? property.objectReferenceValue.name
                        : "null";
                default:
                    return property.propertyType.ToString();
            }
        }

        private static void SetPropertyValue(SerializedProperty property, string value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (int.TryParse(value, out var intVal))
                        property.intValue = intVal;
                    break;
                case SerializedPropertyType.Float:
                    if (float.TryParse(value, out var floatVal))
                        property.floatValue = floatVal;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = value == "true" || value == "True" || value == "1";
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = value;
                    break;
                case SerializedPropertyType.Enum:
                    if (int.TryParse(value, out var enumIdx))
                    {
                        property.enumValueIndex = enumIdx;
                    }
                    else
                    {
                        var names = property.enumDisplayNames;
                        for (var i = 0; i < names.Length; i++)
                        {
                            if (string.Equals(names[i], value, StringComparison.OrdinalIgnoreCase))
                            {
                                property.enumValueIndex = i;
                                break;
                            }
                        }
                    }

                    break;
                default:
                    if (float.TryParse(value, out var fallbackFloat))
                        property.floatValue = fallbackFloat;
                    break;
            }
        }

        private static Type ResolveScriptableObjectType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (t.Name == typeName && typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        return t;
                    }
                }
            }

            return null;
        }
    }
}
