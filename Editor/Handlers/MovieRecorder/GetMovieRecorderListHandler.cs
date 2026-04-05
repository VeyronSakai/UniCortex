using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.MovieRecorder
{
    internal sealed class GetMovieRecorderListHandler
    {
        private readonly GetMovieRecorderListUseCase _useCase;

        public GetMovieRecorderListHandler(GetMovieRecorderListUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.MovieRecorderList, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            try
            {
                var entries = await _useCase.ExecuteAsync(cancellationToken);
                var json = JsonUtility.ToJson(new GetMovieRecorderListResponse(entries));
                await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
            }
            catch (NotSupportedException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
            }
        }
    }
}
