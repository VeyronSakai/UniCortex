using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
{
    internal sealed class SetTimeScaleHandler
    {
        private readonly SetTimeScaleUseCase _useCase;

        public SetTimeScaleHandler(SetTimeScaleUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.TimeScale, HandleSetTimeScaleAsync);
        }

        private async Task HandleSetTimeScaleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = JsonUtility.FromJson<SetTimeScaleRequest>(body);
            var result = await _useCase.ExecuteAsync(request.timeScale, cancellationToken);
            var json = JsonUtility.ToJson(result);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
