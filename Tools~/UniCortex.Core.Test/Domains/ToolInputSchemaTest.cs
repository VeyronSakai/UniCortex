using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Domains;

namespace UniCortex.Core.Test.Domains;

[TestFixture]
public class ToolInputSchemaTest
{
    [Test]
    public void Simplify_NullableTypeAndNullDefault_AreRemoved()
    {
        // Arrange
        var schema = Parse("""{"type":"object","properties":{"count":{"type":["integer","null"],"default":null}}}""");

        // Act
        var result = ToolInputSchema.Simplify(schema);

        // Assert
        Assert.That(result.GetRawText(),
            Is.EqualTo("""{"type":"object","properties":{"count":{"type":"integer"}}}"""));
    }

    [Test]
    public void Simplify_NonNullDefault_IsKept()
    {
        // Arrange
        var schema = Parse("""{"type":"object","properties":{"loop":{"type":"boolean","default":false}}}""");

        // Act
        var result = ToolInputSchema.Simplify(schema);

        // Assert
        Assert.That(result.GetRawText(),
            Is.EqualTo("""{"type":"object","properties":{"loop":{"type":"boolean","default":false}}}"""));
    }

    [Test]
    public void Simplify_NestedArrayItems_AreSimplified()
    {
        // Arrange
        var schema = Parse(
            """{"type":"object","properties":{"names":{"type":["array","null"],"items":{"type":["string","null"]},"default":null}}}""");

        // Act
        var result = ToolInputSchema.Simplify(schema);

        // Assert
        Assert.That(result.GetRawText(),
            Is.EqualTo("""{"type":"object","properties":{"names":{"type":"array","items":{"type":"string"}}}}"""));
    }

    [Test]
    public void Simplify_PropertiesNamedDefaultOrType_AreKept()
    {
        // Arrange
        var schema = Parse(
            """{"type":"object","properties":{"default":{"type":"string"},"type":{"type":"string"}},"required":["default","type"]}""");

        // Act
        var result = ToolInputSchema.Simplify(schema);

        // Assert
        Assert.That(result.GetRawText(), Is.EqualTo(schema.GetRawText()));
    }

    [Test]
    public void Simplify_TypeUnionWithoutNull_IsKept()
    {
        // Arrange
        var schema = Parse("""{"type":"object","properties":{"value":{"type":["string","integer"]}}}""");

        // Act
        var result = ToolInputSchema.Simplify(schema);

        // Assert
        Assert.That(result.GetRawText(), Is.EqualTo(schema.GetRawText()));
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
