using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class UnpauseHandler
    {
        private readonly UnpauseUseCase _useCase;

        public UnpauseHandler(UnpauseUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.Unpause, HandleUnpauseAsync);
        }

        private Task HandleUnpauseAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            _useCase.Execute();
            var json = JsonUtility.ToJson(new UnpauseResponse(true));
            return context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
