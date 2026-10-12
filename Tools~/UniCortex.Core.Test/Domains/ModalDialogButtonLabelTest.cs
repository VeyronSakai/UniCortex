using NUnit.Framework;
using UniCortex.Core.Domains;

namespace UniCortex.Core.Test.Domains;

[TestFixture]
public class ModalDialogButtonLabelTest
{
    [TestCase("OK", "OK", ExpectedResult = true)]
    [TestCase("Don't Save", "don't save", ExpectedResult = true)]
    [TestCase(" OK ", "OK", ExpectedResult = true)]
    [TestCase("OK", "Cancel", ExpectedResult = false)]
    [TestCase("Save", "Save As", ExpectedResult = false)]
    public bool Matches_IgnoresCaseAndSurroundingSpaces(string label, string requested)
    {
        // Act
        return ModalDialogButtonLabel.Matches(label, requested);
    }
}
