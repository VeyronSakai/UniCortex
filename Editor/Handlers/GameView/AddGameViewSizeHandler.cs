using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.GameView
{
    internal sealed class AddGameViewSizeHandler
    {
        private readonly AddGameViewSizeUseCase _useCase;

        public AddGameViewSizeHandler(AddGameViewSizeUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.GameViewSizeAdd, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body)
                ? new AddGameViewSizeRequest()
                : JsonUtility.FromJson<AddGameViewSizeRequest>(body);
            var result = await _useCase.ExecuteAsync(request.name, request.width, request.height,
                cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
