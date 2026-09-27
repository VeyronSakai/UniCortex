using NUnit.Framework;
using UniCortex.Editor.Handlers.GameObject;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class JsonFieldPresenceTest
    {
        [TestCase("{\"siblingIndex\":0}", "siblingIndex", ExpectedResult = true)]
        [TestCase("{\"siblingIndex\" : 2}", "siblingIndex", ExpectedResult = true)]
        [TestCase("{\"name\":\"null\"}", "name", ExpectedResult = true)]
        [TestCase("{\"siblingIndex\":null}", "siblingIndex", ExpectedResult = false)]
        [TestCase("{\"siblingIndex\" : null }", "siblingIndex", ExpectedResult = false)]
        [TestCase("{\"name\":\"Obj\"}", "siblingIndex", ExpectedResult = false)]
        [TestCase("{\"name\":\"siblingIndex\"}", "siblingIndex", ExpectedResult = false)]
        public bool HasValue_ReturnsTrueOnlyForNonNullField(string json, string fieldName)
        {
            // Act & Assert
            return JsonFieldPresence.HasValue(json, fieldName);
        }
    }
}
