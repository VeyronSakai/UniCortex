using NUnit.Framework;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SceneSearchQueryParserTest
    {
        [Test]
        public void Parse_EmptyString_ReturnsDefaultQuery()
        {
            var query = SceneSearchQueryParser.Parse("");

            Assert.IsNull(query.namePattern);
            Assert.IsNull(query.componentTypePattern);
            Assert.IsNull(query.tagPartial);
            Assert.IsNull(query.tagExact);
            Assert.IsNull(query.instanceId);
            Assert.IsNull(query.layer);
            Assert.IsNull(query.pathPattern);
            Assert.AreEqual(0, query.stateFilters.Count);
        }

        [Test]
        public void Parse_Null_ReturnsDefaultQuery()
        {
            var query = SceneSearchQueryParser.Parse(null);

            Assert.IsNull(query.namePattern);
        }

        [Test]
        public void Parse_PlainText_SetsNamePattern()
        {
            var query = SceneSearchQueryParser.Parse("Main Camera");

            Assert.AreEqual("Main Camera", query.namePattern);
        }

        [Test]
        public void Parse_ComponentType_SetsComponentTypePattern()
        {
            var query = SceneSearchQueryParser.Parse("t:Camera");

            Assert.AreEqual("Camera", query.componentTypePattern);
            Assert.IsNull(query.namePattern);
        }

        [Test]
        public void Parse_TagPartial_SetsTagPartial()
        {
            var query = SceneSearchQueryParser.Parse("tag:resp");

            Assert.AreEqual("resp", query.tagPartial);
        }

        [Test]
        public void Parse_TagExact_SetsTagExact()
        {
            var query = SceneSearchQueryParser.Parse("tag=Player");

            Assert.AreEqual("Player", query.tagExact);
        }

        [Test]
        public void Parse_InstanceIdEquals_SetsInstanceId()
        {
            var query = SceneSearchQueryParser.Parse("id=12345");

            Assert.AreEqual(12345, query.instanceId);
        }

        [Test]
        public void Parse_InstanceIdColon_SetsInstanceId()
        {
            var query = SceneSearchQueryParser.Parse("id:12345");

            Assert.AreEqual(12345, query.instanceId);
        }

        [Test]
        public void Parse_Layer_SetsLayer()
        {
            var query = SceneSearchQueryParser.Parse("layer:5");

            Assert.AreEqual(5, query.layer);
        }

        [Test]
        public void Parse_Path_SetsPathPattern()
        {
            var query = SceneSearchQueryParser.Parse("path:Canvas/Button");

            Assert.AreEqual("Canvas/Button", query.pathPattern);
        }

        [Test]
        public void Parse_IsRoot_AddsStateFilter()
        {
            var query = SceneSearchQueryParser.Parse("is:root");

            Assert.AreEqual(1, query.stateFilters.Count);
            Assert.AreEqual("root", query.stateFilters[0]);
        }

        [Test]
        public void Parse_MultipleStateFilters_AddsAll()
        {
            var query = SceneSearchQueryParser.Parse("is:root is:static");

            Assert.AreEqual(2, query.stateFilters.Count);
            Assert.Contains("root", query.stateFilters);
            Assert.Contains("static", query.stateFilters);
        }

        [Test]
        public void Parse_CombinedQuery_ParsesAllTokens()
        {
            var query = SceneSearchQueryParser.Parse("Camera t:Camera tag=MainCamera layer:0");

            Assert.AreEqual("Camera", query.namePattern);
            Assert.AreEqual("Camera", query.componentTypePattern);
            Assert.AreEqual("MainCamera", query.tagExact);
            Assert.AreEqual(0, query.layer);
        }

        [Test]
        public void Parse_InvalidInstanceId_IgnoresValue()
        {
            var query = SceneSearchQueryParser.Parse("id=abc");

            Assert.IsNull(query.instanceId);
        }

        [Test]
        public void Parse_InvalidLayer_IgnoresValue()
        {
            var query = SceneSearchQueryParser.Parse("layer:abc");

            Assert.IsNull(query.layer);
        }
    }
}
