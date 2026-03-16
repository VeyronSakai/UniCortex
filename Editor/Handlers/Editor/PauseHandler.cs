using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class PauseHandler
    {
        private readonly PauseUseCase _useCase;

        public PauseHandler(PauseUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.Pause, HandlePauseAsync);
        }

        private Task HandlePauseAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            _useCase.Execute();
            var json = JsonUtility.ToJson(new PauseResponse(true));
            return context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
