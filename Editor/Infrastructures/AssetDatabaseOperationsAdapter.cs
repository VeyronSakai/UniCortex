using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class AssetDatabaseOperationsAdapter : IAssetDatabaseOperations
    {
        public void Refresh()
        {
            AssetDatabase.Refresh();
        }
    }
}
