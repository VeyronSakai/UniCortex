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
        public CreateScriptableObjectResponse Create(string typeName, string assetPath)
        {
            var type = UnityTypeResolver.Resolve<ScriptableObject>(typeName);
            if (type == null)
            {
                throw new ArgumentException($"ScriptableObject type '{typeName}' not found.");
            }

            var asset = ScriptableObject.CreateInstance(type);
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create instance of ScriptableObject type '{typeName}'.");
            }

            AssetDatabase.CreateAsset(asset, assetPath);
            Undo.RegisterCreatedObjectUndo(asset, $"Create {type.Name}");
            AssetDatabase.SaveAssets();

            return new CreateScriptableObjectResponse(true, asset.GetInstanceID());
        }

        public GetScriptableObjectPropertiesResponse GetProperties(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null)
            {
                throw new ArgumentException(
                    $"ScriptableObject asset not found at path '{assetPath}'.");
            }

            var serializedObject = new SerializedObject(asset);
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

            return new GetScriptableObjectPropertiesResponse(asset.GetType().FullName, properties);
        }

        public void SetProperty(string assetPath, string propertyPath, string value)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null)
            {
                throw new ArgumentException(
                    $"ScriptableObject asset not found at path '{assetPath}'.");
            }

            // Use SerializedObject/SerializedProperty so that the change is recorded by Undo.
            var serializedObject = new SerializedObject(asset);
            var property = serializedObject.FindProperty(propertyPath);

            if (property == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyPath}' not found on ScriptableObject at '{assetPath}'.");
            }

            SerializedPropertyValueParser.ApplyValue(property, value);
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

    }
}
