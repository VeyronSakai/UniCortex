using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyMenuItemOperations : IMenuItemOperations
    {
        public int ExecuteMenuItemCallCount { get; private set; }
        public string LastMenuPath { get; private set; }
        public bool ExecuteMenuItemResult { get; set; } = true;

        public bool ExecuteMenuItem(string menuPath)
        {
            ExecuteMenuItemCallCount++;
            LastMenuPath = menuPath;
            return ExecuteMenuItemResult;
        }
    }
}
