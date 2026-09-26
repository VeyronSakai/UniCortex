using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Infrastructures;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.Infrastructures;

[TestFixture]
public class UnityJsonTest
{
    [Test]
    public void QuoteNonFiniteLiterals_QuotesBareTokens()
    {
        var json = UnityJson.QuoteNonFiniteLiterals("{\"a\":Infinity,\"b\":-Infinity,\"c\":[NaN,1.0]}");

        Assert.That(json, Is.EqualTo("{\"a\":\"Infinity\",\"b\":\"-Infinity\",\"c\":[\"NaN\",1.0]}"));
    }

    [Test]
    public void QuoteNonFiniteLiterals_LeavesStringValuesUntouched()
    {
        const string json = "{\"name\":\"Infinity and NaN\",\"escaped\":\"a\\\"Infinity\",\"v\":Infinity}";

        var result = UnityJson.QuoteNonFiniteLiterals(json);

        Assert.That(result,
            Is.EqualTo("{\"name\":\"Infinity and NaN\",\"escaped\":\"a\\\"Infinity\",\"v\":\"Infinity\"}"));
    }

    [Test]
    public void QuoteNonFiniteLiterals_ReturnsSameString_WhenNothingToQuote()
    {
        const string json = "{\"a\":1.5,\"b\":\"text\"}";

        Assert.That(UnityJson.QuoteNonFiniteLiterals(json), Is.SameAs(json));
    }

    [Test]
    public void Deserialize_ReadsBareInfinityFromJsonUtility()
    {
        var entry = UnityJson.Deserialize<AnimationCurveKeyEntry>(
            "{\"time\":0.0,\"value\":1.0,\"inTangent\":-Infinity,\"outTangent\":Infinity," +
            "\"leftTangentMode\":\"Constant\",\"rightTangentMode\":\"Constant\"}")!;

        Assert.That(float.IsNegativeInfinity(entry.inTangent), Is.True);
        Assert.That(float.IsPositiveInfinity(entry.outTangent), Is.True);
    }

    [Test]
    public void RequestOptions_WritesNonFiniteAsBareTokens()
    {
        var key = new AnimationCurveKeyInput
        {
            time = 0f, value = 1f, inTangent = float.NegativeInfinity, outTangent = float.PositiveInfinity
        };

        var json = JsonSerializer.Serialize(key, UnityJson.RequestOptions);

        Assert.That(json, Does.Contain("\"inTangent\":-Infinity"));
        Assert.That(json, Does.Contain("\"outTangent\":Infinity"));
        Assert.That(json, Does.Contain("\"value\":1"));
    }
}
