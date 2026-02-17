using System;
using System.Threading.Tasks;
using EditorBridge.Editor.Domains.Interfaces;

namespace EditorBridge.Editor.Tests.TestDoubles
{
    internal sealed class FakeMainThreadDispatcher : IMainThreadDispatcher
    {
        public int CallCount { get; private set; }

        public Task<T> RunOnMainThread<T>(Func<T> func)
        {
            CallCount++;
            return Task.FromResult(func());
        }

        public Task RunOnMainThread(Action action)
        {
            CallCount++;
            action();
            return Task.CompletedTask;
        }
    }
}
