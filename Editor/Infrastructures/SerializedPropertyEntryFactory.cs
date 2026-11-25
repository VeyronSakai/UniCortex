using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal static class SerializedPropertyEntryFactory
    {
        public static SerializedPropertyEntry Create(SerializedProperty property)
        {
            return new SerializedPropertyEntry(
                property.propertyPath,
                property.propertyType.ToString(),
                SerializedPropertyValueConverter.ToValueString(property),
                DescribeReference(property));
        }

        // Describes the target of an object reference so that it can be identified, since the value of a scene
        // object is only its instanceId. Returns null for other properties and for references that are not set.
        private static string DescribeReference(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    var target = property.objectReferenceValue;
                    if (target != null)
                    {
                        return DescribeObject(target, property.serializedObject.targetObject);
                    }

                    // A reference whose target was destroyed or cannot be loaded keeps its instanceId.
                    return property.objectReferenceInstanceIDValue != 0 ? "Missing" : null;
                case SerializedPropertyType.ExposedReference:
                    var exposedTarget = property.exposedReferenceValue;
                    return exposedTarget != null
                        ? DescribeObject(exposedTarget, property.serializedObject.targetObject)
                        : null;
                default:
                    return null;
            }
        }

        // "Assets/Foo.mat (Type)" for a main asset, "Assets/Foo.prefab: Root/Child (Type)" for an object inside an
        // asset, and "Root/Child (Type)" for a scene object, prefixed with its scene when that is not the scene of
        // the object that holds the reference.
        private static string DescribeObject(Object target, Object owner)
        {
            var typeName = target.GetType().FullName;
            var targetGameObject = GetGameObject(target);
            var assetPath = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (AssetDatabase.IsMainAsset(target))
                {
                    return $"{assetPath} ({typeName})";
                }

                var pathInAsset = targetGameObject != null ? GetHierarchyPath(targetGameObject) : target.name;
                return $"{assetPath}: {pathInAsset} ({typeName})";
            }

            if (targetGameObject == null)
            {
                return $"{target.name} ({typeName})";
            }

            var description = $"{GetHierarchyPath(targetGameObject)} ({typeName})";
            var ownerGameObject = GetGameObject(owner);
            if (ownerGameObject == null || ownerGameObject.scene != targetGameObject.scene)
            {
                var scene = targetGameObject.scene;
                var sceneName = string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;
                return $"{sceneName}: {description}";
            }

            return description;
        }

        private static GameObject GetGameObject(Object obj)
        {
            switch (obj)
            {
                case GameObject gameObject:
                    return gameObject;
                case Component component:
                    return component.gameObject;
                default:
                    return null;
            }
        }

        private static string GetHierarchyPath(GameObject gameObject)
        {
            var names = new List<string>();
            for (var transform = gameObject.transform; transform != null; transform = transform.parent)
            {
                names.Add(transform.name);
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}
