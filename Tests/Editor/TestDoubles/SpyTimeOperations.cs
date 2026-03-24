using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyTimeOperations : ITimeOperations
    {
        public float TimeScale { get; set; } = 1f;
    }
}
