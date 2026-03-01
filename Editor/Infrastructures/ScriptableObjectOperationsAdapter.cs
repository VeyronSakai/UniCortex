using System;
using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ScriptableObjectOperationsAdapter : IScriptableObjectOperations
    {
        public void Create(string type, string assetPath)
        {
            var scriptableType = ResolveScriptableObjectType(type);
            if (scriptableType == null)
            {
                throw new ArgumentException($"Unsupported asset type: {type}");
            }

            var asset = ScriptableObject.CreateInstance(scriptableType);

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }

        public ScriptableObjectInfoResponse GetInfo(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null)
            {
                throw new ArgumentException($"ScriptableObject not found at path: {assetPath}");
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
                    SerializedPropertyValueConverter.ToValueString(iterator)));
            }

            return new ScriptableObjectInfoResponse(assetPath, asset.GetType().Name, properties);
        }

        public void SetProperty(string assetPath, string propertyPath, string value)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null)
            {
                throw new ArgumentException($"ScriptableObject not found at path: {assetPath}");
            }

            var serializedObject = new SerializedObject(asset);
            var property = serializedObject.FindProperty(propertyPath);

            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyPath}' not found on asset at '{assetPath}'.");
            }

            SerializedPropertyValueParser.ApplyValue(property, value);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
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
