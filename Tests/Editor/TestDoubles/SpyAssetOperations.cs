using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyAssetDatabaseOperations : IAssetDatabaseOperations
    {
        public int RefreshCallCount { get; private set; }

        public void Refresh()
        {
            RefreshCallCount++;
        }
    }
}
