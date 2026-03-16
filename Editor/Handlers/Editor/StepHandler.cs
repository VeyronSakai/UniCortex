using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class StepHandler
    {
        private readonly StepUseCase _useCase;

        public StepHandler(StepUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.Step, HandleStepAsync);
        }

        private async Task HandleStepAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            await _useCase.ExecuteAsync(cancellationToken);
            var json = JsonUtility.ToJson(new StepResponse(success: true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
