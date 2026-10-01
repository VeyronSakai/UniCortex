using System;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class AssetFolderUtilityTest
    {
        private const string RootFolder = "Assets/AssetFolderUtilityTest";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(RootFolder);
        }

        [Test]
        public void EnsureParentFolderExists_CreatesNestedFolders_WhenMissing()
        {
            // Arrange
            const string assetPath = RootFolder + "/Nested/Deep/Foo.anim";

            // Act
            AssetFolderUtility.EnsureParentFolderExists(assetPath);

            // Assert
            Assert.IsTrue(AssetDatabase.IsValidFolder(RootFolder + "/Nested/Deep"));
        }

        [Test]
        public void EnsureParentFolderExists_DoesNothing_WhenParentIsAssets()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => AssetFolderUtility.EnsureParentFolderExists("Assets/Foo.anim"));
        }

        [Test]
        public void EnsureParentFolderExists_Throws_WhenPathIsNotUnderAssets()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => AssetFolderUtility.EnsureParentFolderExists("Foo.anim"));
            Assert.Throws<ArgumentException>(() =>
                AssetFolderUtility.EnsureParentFolderExists("NotAssets/Sub/Foo.anim"));
        }

        [Test]
        public void CreateAsset_CreatesAssetInMissingFolder()
        {
            // Arrange
            const string assetPath = RootFolder + "/Clips/Foo.anim";
            var asset = new AnimationClip();

            // Act
            AssetFolderUtility.CreateAsset(asset, assetPath);

            // Assert
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath));
        }
    }
}
