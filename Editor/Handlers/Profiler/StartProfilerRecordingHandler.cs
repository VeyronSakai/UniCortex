using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Profiler
{
    internal sealed class StartProfilerRecordingHandler
    {
        private readonly StartProfilerRecordingUseCase _useCase;

        public StartProfilerRecordingHandler(StartProfilerRecordingUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.ProfilerStartRecording, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();
            var request = string.IsNullOrEmpty(body)
                ? new StartProfilerRecordingRequest()
                : JsonUtility.FromJson<StartProfilerRecordingRequest>(body) ?? new StartProfilerRecordingRequest();

            await _useCase.ExecuteAsync(request.profileEditor, cancellationToken);
            var json = JsonUtility.ToJson(new StartProfilerRecordingResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }
    }
}
