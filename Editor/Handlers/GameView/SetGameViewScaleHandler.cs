using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class SetGameViewScaleHandler
    {
        private readonly SetGameViewScaleUseCase _useCase;

        public SetGameViewScaleHandler(SetGameViewScaleUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameViewScale, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = JsonUtility.FromJson<SetGameViewScaleRequest>(body);
            var result = await _useCase.ExecuteAsync(request.scale, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
