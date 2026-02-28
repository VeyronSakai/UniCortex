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

            // Use SerializedObject/SerializedProperty to modify the component
            // so that the change is recorded by the Undo system.
            var serializedObject = new SerializedObject(component);
            var property = serializedObject.FindProperty(propertyPath);

            if (property == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyPath}' not found on component '{componentType}'.");
            }

            // Parse the string value and write it into the SerializedProperty,
            // then apply the modification to persist the change.
            SerializedPropertyValueParser.ApplyValue(property, value);
            serializedObject.ApplyModifiedProperties();
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
