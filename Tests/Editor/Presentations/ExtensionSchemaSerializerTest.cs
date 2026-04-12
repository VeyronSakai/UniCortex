using UniCortex.Editor.Handlers.Extension;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class ExtensionSchemaSerializerTest
    {
        [Test]
        public void ToJsonSchema_ReturnsNull_WhenSchemaIsNull()
        {
            var result = ExtensionSchemaSerializer.ToJsonSchema(null);
            Assert.IsNull(result);
        }

        [Test]
        public void ToJsonSchema_ReturnsNull_WhenNoProperties()
        {
            var schema = new ExtensionSchema();
            var result = ExtensionSchemaSerializer.ToJsonSchema(schema);
            Assert.IsNull(result);
        }

        [Test]
        public void ToJsonSchema_ReturnsValidJsonSchema_WithSingleProperty()
        {
            var schema = new ExtensionSchema(
                new ExtensionProperty("name", ExtensionPropertyType.String, "The name", required: true)
            );

            var result = ExtensionSchemaSerializer.ToJsonSchema(schema);

            StringAssert.Contains("\"type\":\"object\"", result);
            StringAssert.Contains("\"name\":{\"type\":\"string\",\"description\":\"The name\"}", result);
            StringAssert.Contains("\"required\":[\"name\"]", result);
        }

        [Test]
        public void ToJsonSchema_ReturnsValidJsonSchema_WithMultipleProperties()
        {
            var schema = new ExtensionSchema(
                new ExtensionProperty("x", ExtensionPropertyType.Number, "X value", required: true),
                new ExtensionProperty("y", ExtensionPropertyType.Integer, "Y value"),
                new ExtensionProperty("flag", ExtensionPropertyType.Boolean, "A flag")
            );

            var result = ExtensionSchemaSerializer.ToJsonSchema(schema);

            StringAssert.Contains("\"x\":{\"type\":\"number\",\"description\":\"X value\"}", result);
            StringAssert.Contains("\"y\":{\"type\":\"integer\",\"description\":\"Y value\"}", result);
            StringAssert.Contains("\"flag\":{\"type\":\"boolean\",\"description\":\"A flag\"}", result);
            StringAssert.Contains("\"required\":[\"x\"]", result);
        }

        [Test]
        public void ToJsonSchema_OmitsRequired_WhenNoRequiredProperties()
        {
            var schema = new ExtensionSchema(
                new ExtensionProperty("name", ExtensionPropertyType.String, "The name")
            );

            var result = ExtensionSchemaSerializer.ToJsonSchema(schema);

            Assert.IsFalse(result.Contains("required"));
        }

        [Test]
        public void ToJsonSchema_EscapesSpecialCharacters_InDescription()
        {
            var schema = new ExtensionSchema(
                new ExtensionProperty("text", ExtensionPropertyType.String, "Has \"quotes\" and \\backslash")
            );

            var result = ExtensionSchemaSerializer.ToJsonSchema(schema);

            StringAssert.Contains("Has \\\"quotes\\\" and \\\\backslash", result);
        }
    }
}
