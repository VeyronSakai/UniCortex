using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyProjectViewOperations : IProjectViewOperations
    {
        public int SelectAssetCallCount { get; private set; }
        public string LastSelectedAssetPath { get; private set; }

        public void SelectAsset(string assetPath)
        {
            SelectAssetCallCount++;
            LastSelectedAssetPath = assetPath;
        }
    }
}
