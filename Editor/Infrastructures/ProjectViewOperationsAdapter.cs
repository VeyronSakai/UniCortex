using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ProjectViewOperationsAdapter : IProjectViewOperations
    {
        public void SelectAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException("assetPath is required.");
            }

            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset == null)
            {
                throw new ArgumentException($"Asset not found at path: {assetPath}");
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
