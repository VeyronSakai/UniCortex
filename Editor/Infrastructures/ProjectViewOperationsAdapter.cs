using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ProjectViewOperationsAdapter : IProjectViewOperations
    {
        private static readonly Type s_projectBrowserType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");

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

            if (s_projectBrowserType == null)
            {
                throw new InvalidOperationException(
                    "ProjectBrowser type not found. This Unity version may not be supported.");
            }

            var projectBrowser = EditorWindow.GetWindow(s_projectBrowserType);
            projectBrowser.Focus();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
