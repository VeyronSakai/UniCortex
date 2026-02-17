using System.Threading.Tasks;
using EditorBridge.Editor.Domains.Interfaces;
using UnityEngine;

namespace EditorBridge.Editor.UseCases
{
    internal sealed class PingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;

        public PingUseCase(IMainThreadDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<string> ExecuteAsync()
        {
            await _dispatcher.RunOnMainThread(() => Debug.Log("pong"));
            return "pong";
        }
    }
}
