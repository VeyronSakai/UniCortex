using System;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal static class AssetUtility
    {
        // Creates every missing folder between the project root and the asset, one level at a time,
        // because AssetDatabase.CreateAsset and friends fail when the parent folder does not exist.
        public static void EnsureParentFolderExists(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException("Asset path must not be empty.");
            }

            var normalized = assetPath.Replace('\\', '/');
            var separatorIndex = normalized.LastIndexOf('/');
            if (separatorIndex <= 0)
            {
                throw new ArgumentException(
                    $"Asset path must be under 'Assets/' (got '{assetPath}').");
            }

            EnsureFolderExists(normalized[..separatorIndex], assetPath);
        }

        private static void EnsureFolderExists(string folderPath, string assetPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var separatorIndex = folderPath.LastIndexOf('/');
            if (separatorIndex <= 0)
            {
                throw new ArgumentException(
                    $"Asset path must be under 'Assets/' (got '{assetPath}').");
            }

            var parentPath = folderPath[..separatorIndex];
            var folderName = folderPath[(separatorIndex + 1)..];
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException($"Asset path contains an empty folder name (got '{assetPath}').");
            }

            EnsureFolderExists(parentPath, assetPath);

            var guid = AssetDatabase.CreateFolder(parentPath, folderName);
            if (string.IsNullOrEmpty(guid) || !AssetDatabase.IsValidFolder(folderPath))
            {
                throw new ArgumentException($"Failed to create folder '{folderPath}' for asset '{assetPath}'.");
            }
        }

        // Creates the parent folders, then the asset, and reports a failure as an invalid request
        // instead of letting UnityException surface as an internal server error.
        public static void CreateAsset(UnityEngine.Object asset, string assetPath)
        {
            EnsureParentFolderExists(assetPath);

            try
            {
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            catch (UnityEngine.UnityException ex)
            {
                throw new ArgumentException($"Failed to create asset at '{assetPath}': {ex.Message}", ex);
            }
        }
    }
}
