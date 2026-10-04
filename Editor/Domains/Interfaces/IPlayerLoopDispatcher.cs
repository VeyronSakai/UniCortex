using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniCortex.Editor.Domains.Interfaces
{
    // Runs functions inside the player loop in Play Mode.
    // Some APIs (e.g. Screen.width/height, which GraphicRaycaster uses) return Game View values only while
    // the player loop runs; from EditorApplication.update they return the size of another view.
    internal interface IPlayerLoopDispatcher
    {
        // Must be called on the main thread.
        Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken = default);
    }
}
