using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ComponentOperationsAdapter : IComponentOperations
    {
        public void AddComponent(int instanceId, string componentType)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            var type = ResolveComponentType(componentType);
            if (type == null)
            {
                throw new ArgumentException($"Component type '{componentType}' not found.");
            }

            Undo.AddComponent(go, type);
        }

        public void RemoveComponent(int instanceId, string componentType, int componentIndex)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            var components = go.GetComponents<Component>()
                .Where(c => c != null && c.GetType().FullName == componentType)
                .ToArray();

            if (components.Length == 0)
            {
                throw new ArgumentException(
                    $"Component '{componentType}' not found on GameObject {instanceId}.");
            }

            if (componentIndex < 0 || componentIndex >= components.Length)
            {
                throw new ArgumentException(
                    $"componentIndex {componentIndex} out of range. Found {components.Length} component(s) of type '{componentType}'.");
            }

            Undo.DestroyObjectImmediate(components[componentIndex]);
        }

        public ComponentPropertiesResponse GetProperties(int instanceId, string componentType, int componentIndex)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            var components = go.GetComponents<Component>()
                .Where(c => c != null && c.GetType().FullName == componentType)
                .ToArray();

            if (components.Length == 0)
            {
                throw new ArgumentException(
                    $"Component '{componentType}' not found on GameObject {instanceId}.");
            }

            if (componentIndex < 0 || componentIndex >= components.Length)
            {
                throw new ArgumentException(
                    $"componentIndex {componentIndex} out of range. Found {components.Length} component(s) of type '{componentType}'.");
            }

            var component = components[componentIndex];
            var serializedObject = new SerializedObject(component);
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

            return new ComponentPropertiesResponse(componentType, properties);
        }

        public void SetProperty(int instanceId, string componentType, string propertyPath, string value)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            var component = go.GetComponents<Component>()
                .FirstOrDefault(c => c != null && c.GetType().FullName == componentType);

            if (component == null)
            {
                throw new ArgumentException(
                    $"Component '{componentType}' not found on GameObject {instanceId}.");
            }

            var serializedObject = new SerializedObject(component);
            var property = serializedObject.FindProperty(propertyPath);

            if (property == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyPath}' not found on component '{componentType}'.");
            }

            SetPropertyValue(property, value);
            serializedObject.ApplyModifiedProperties();
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
                case SerializedPropertyType.Vector2:
                    var v2 = property.vector2Value;
                    return $"({v2.x}, {v2.y})";
                case SerializedPropertyType.Vector3:
                    var v3 = property.vector3Value;
                    return $"({v3.x}, {v3.y}, {v3.z})";
                case SerializedPropertyType.Vector4:
                    var v4 = property.vector4Value;
                    return $"({v4.x}, {v4.y}, {v4.z}, {v4.w})";
                case SerializedPropertyType.Color:
                    var c = property.colorValue;
                    return $"({c.r}, {c.g}, {c.b}, {c.a})";
                case SerializedPropertyType.Rect:
                    var r = property.rectValue;
                    return $"(x:{r.x}, y:{r.y}, w:{r.width}, h:{r.height})";
                case SerializedPropertyType.Bounds:
                    var b = property.boundsValue;
                    return $"(center:{b.center}, size:{b.size})";
                case SerializedPropertyType.Quaternion:
                    var q = property.quaternionValue;
                    return $"({q.x}, {q.y}, {q.z}, {q.w})";
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
                        // Try to find enum value by name
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
                    // For other types, try to parse as float
                    if (float.TryParse(value, out var fallbackFloat))
                        property.floatValue = fallbackFloat;
                    break;
            }
        }

        private static Type ResolveComponentType(string fullTypeName)
        {
            // Search all loaded assemblies by full namespace-qualified name
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullTypeName);
                if (type != null && typeof(Component).IsAssignableFrom(type))
                {
                    return type;
                }
            }

            return null;
        }
    }
}
