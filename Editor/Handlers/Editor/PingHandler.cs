using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Editor
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
            var verbose = string.Equals(context.GetQueryParameter(QueryParameterNames.Verbose), "true",
                System.StringComparison.OrdinalIgnoreCase);
            var response = await _useCase.ExecuteAsync(verbose, cancellationToken);
            var json = JsonUtility.ToJson(response);
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
