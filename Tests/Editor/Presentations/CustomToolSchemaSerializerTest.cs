using UniCortex.Editor.Handlers.CustomTool;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class CustomToolSchemaSerializerTest
    {
        [Test]
        public void ToJsonSchema_ReturnsNull_WhenSchemaIsNull()
        {
            var result = CustomToolSchemaSerializer.ToJsonSchema(null);
            Assert.IsNull(result);
        }

        [Test]
        public void ToJsonSchema_ReturnsNull_WhenNoProperties()
        {
            var schema = new CustomToolSchema();
            var result = CustomToolSchemaSerializer.ToJsonSchema(schema);
            Assert.IsNull(result);
        }

        [Test]
        public void ToJsonSchema_ReturnsValidJsonSchema_WithSingleProperty()
        {
            var schema = new CustomToolSchema(
                new CustomToolProperty("name", CustomToolPropertyType.String, "The name", required: true)
            );

            var result = CustomToolSchemaSerializer.ToJsonSchema(schema);

            StringAssert.Contains("\"type\":\"object\"", result);
            StringAssert.Contains("\"name\":{\"type\":\"string\",\"description\":\"The name\"}", result);
            StringAssert.Contains("\"required\":[\"name\"]", result);
        }

        [Test]
        public void ToJsonSchema_ReturnsValidJsonSchema_WithMultipleProperties()
        {
            var schema = new CustomToolSchema(
                new CustomToolProperty("x", CustomToolPropertyType.Number, "X value", required: true),
                new CustomToolProperty("y", CustomToolPropertyType.Integer, "Y value"),
                new CustomToolProperty("flag", CustomToolPropertyType.Boolean, "A flag")
            );

            var result = CustomToolSchemaSerializer.ToJsonSchema(schema);

            StringAssert.Contains("\"x\":{\"type\":\"number\",\"description\":\"X value\"}", result);
            StringAssert.Contains("\"y\":{\"type\":\"integer\",\"description\":\"Y value\"}", result);
            StringAssert.Contains("\"flag\":{\"type\":\"boolean\",\"description\":\"A flag\"}", result);
            StringAssert.Contains("\"required\":[\"x\"]", result);
        }

        [Test]
        public void ToJsonSchema_OmitsRequired_WhenNoRequiredProperties()
        {
            var schema = new CustomToolSchema(
                new CustomToolProperty("name", CustomToolPropertyType.String, "The name")
            );

            var result = CustomToolSchemaSerializer.ToJsonSchema(schema);

            Assert.IsFalse(result.Contains("required"));
        }

        [Test]
        public void ToJsonSchema_EscapesSpecialCharacters_InDescription()
        {
            var schema = new CustomToolSchema(
                new CustomToolProperty("text", CustomToolPropertyType.String, "Has \"quotes\" and \\backslash")
            );

            var result = CustomToolSchemaSerializer.ToJsonSchema(schema);

            StringAssert.Contains("Has \\\"quotes\\\" and \\\\backslash", result);
        }
    }
}
