using NUnit.Framework;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class GameObjectQueryTest
    {
        [Test]
        public void Parse_WithoutSceneToken_KeepsQueryAndMatchesAnyScene()
        {
            // Act
            var query = GameObjectQuery.Parse("Camera t:Camera");

            // Assert
            Assert.AreEqual("Camera t:Camera", query.SearchQuery);
            Assert.AreEqual(0, query.SceneNames.Count);
            Assert.IsTrue(query.MatchesScene("AnyScene"));
        }

        [Test]
        public void Parse_WithSceneToken_RemovesTokenFromSearchQuery()
        {
            // Act
            var query = GameObjectQuery.Parse("t:Button scene:Menu layer:5");

            // Assert
            Assert.AreEqual("t:Button layer:5", query.SearchQuery);
            CollectionAssert.AreEqual(new[] { "Menu" }, query.SceneNames);
        }

        [Test]
        public void Parse_WithQuotedSceneName_SupportsSpaces()
        {
            // Act
            var query = GameObjectQuery.Parse("scene:\"Title Screen\" Logo");

            // Assert
            Assert.AreEqual("Logo", query.SearchQuery);
            CollectionAssert.AreEqual(new[] { "Title Screen" }, query.SceneNames);
        }

        [Test]
        public void Parse_WithOnlySceneToken_ReturnsEmptySearchQuery()
        {
            // Act
            var query = GameObjectQuery.Parse("scene:Boot");

            // Assert
            Assert.AreEqual(string.Empty, query.SearchQuery);
            CollectionAssert.AreEqual(new[] { "Boot" }, query.SceneNames);
        }

        [Test]
        public void Parse_WithSceneInsideAnotherToken_DoesNotTreatItAsSceneToken()
        {
            // Act
            var query = GameObjectQuery.Parse("path:Root/scene:Foo");

            // Assert
            Assert.AreEqual("path:Root/scene:Foo", query.SearchQuery);
            Assert.AreEqual(0, query.SceneNames.Count);
        }

        [Test]
        public void MatchesScene_WithMultipleSceneTokens_MatchesAnyCaseInsensitively()
        {
            // Arrange
            var query = GameObjectQuery.Parse("SCENE:menu scene:Hud");

            // Act & Assert
            Assert.IsTrue(query.MatchesScene("Menu"));
            Assert.IsTrue(query.MatchesScene("HUD"));
            Assert.IsFalse(query.MatchesScene("Boot"));
        }
    }
}
