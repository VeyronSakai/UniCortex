using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Handlers.Utility
{
    internal sealed class ScreenshotHandler
    {
        private readonly CaptureScreenshotUseCase _useCase;

        public ScreenshotHandler(CaptureScreenshotUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.Screenshot, HandleAsync);
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var pngData = await _useCase.ExecuteAsync(cancellationToken);
            await context.WriteBinaryResponseAsync(200, "image/png", pngData);
        }
    }
}
