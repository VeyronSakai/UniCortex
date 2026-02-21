using System.Threading;
using System.Threading.Tasks;
using EditorBridge.Editor.Domains.Interfaces;
using EditorBridge.Editor.Domains.Models;
using EditorBridge.Editor.UseCases;
using UnityEngine;

namespace EditorBridge.Editor.Presentations
{
    internal sealed class PingHandler
    {
        private readonly PingUseCase _useCase;

        public PingHandler(PingUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.Ping, HandlePingAsync);
        }

        private async Task HandlePingAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var message = await _useCase.ExecuteAsync();
            var json = JsonUtility.ToJson(new PingResponse(status: "ok", message: message));
            await context.WriteResponseAsync(200, json);
        }
    }
}
