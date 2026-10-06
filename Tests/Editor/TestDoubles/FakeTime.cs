using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class FakeTime : ITime
    {
        public double UnscaledTime { get; set; }
    }
}
