using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class MenuItemOperationsAdapter : IMenuItemOperations
    {
        public bool ExecuteMenuItem(string menuPath)
        {
            return EditorApplication.ExecuteMenuItem(menuPath);
        }
    }
}
