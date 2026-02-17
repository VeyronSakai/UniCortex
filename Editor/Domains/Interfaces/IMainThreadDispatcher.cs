using System;
using System.Threading.Tasks;

namespace EditorBridge.Editor.Domains.Interfaces
{
    internal interface IMainThreadDispatcher
    {
        Task<T> RunOnMainThread<T>(Func<T> func);
        Task RunOnMainThread(Action action);
    }
}
