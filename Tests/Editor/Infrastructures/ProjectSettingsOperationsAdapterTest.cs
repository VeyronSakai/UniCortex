using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class ProjectSettingsOperationsAdapterTest
    {
        private ProjectSettingsOperationsAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _adapter = new ProjectSettingsOperationsAdapter();
        }

        [Test]
        public void GetCategories_EnumeratesEveryProjectSettingsAsset()
        {
            // The Unity Editor's working directory is the project root, so the adapter
            // scans the same "ProjectSettings" folder that this test can read directly.
            var expected = Directory.EnumerateFiles("ProjectSettings", "*.asset",
                SearchOption.TopDirectoryOnly).Count();

            var response = _adapter.GetCategories();

            Assert.That(response.categories, Has.Count.EqualTo(expected));
        }

        [Test]
        public void GetCategories_ExcludesNonAssetFiles()
        {
            var response = _adapter.GetCategories();

            // ProjectVersion.txt is a sibling file that must not surface as a category.
            Assert.That(response.categories.Select(c => c.name), Does.Not.Contain("ProjectVersion"));
            foreach (var entry in response.categories)
            {
                Assert.That(entry.assetPath, Does.EndWith(".asset"));
            }
        }

        [Test]
        public void GetCategories_IncludesAssetsNotInPreviousHardcodedDictionary()
        {
            // XRSettings is one of several assets that the old static dictionary did not list.
            // The fixture under Samples~ ships with it, so it must be discovered now.
            var response = _adapter.GetCategories();

            Assert.That(response.categories.Select(c => c.name), Does.Contain("XRSettings"));
        }

        [Test]
        public void GetCategories_UsesFilenameWithoutExtensionAsCategoryName()
        {
            var response = _adapter.GetCategories();

            var timeManager = response.categories.FirstOrDefault(c => c.name == "TimeManager");
            Assert.That(timeManager, Is.Not.Null);
            Assert.That(timeManager!.assetPath, Is.EqualTo("ProjectSettings/TimeManager.asset"));
        }

        [Test]
        public void GetSettings_UnknownCategory_ThrowsArgumentException()
        {
            Assert.That(() => _adapter.GetSettings("ThisCategoryDoesNotExist"),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetSettings_KnownCategory_ReturnsProperties()
        {
            var response = _adapter.GetSettings("TimeManager");

            Assert.That(response.category, Is.EqualTo("TimeManager"));
            Assert.That(response.properties, Is.Not.Empty);
        }
    }
}
