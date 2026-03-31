using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ConfigureRecorderUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IRecordingOperations _operations;

        public ConfigureRecorderUseCase(IMainThreadDispatcher dispatcher, IRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string outputPath, string source, string cameraSource,
            string cameraTag, bool captureUI, string outputFormat,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.ConfigureRecorder(
                    outputPath, source, cameraSource, cameraTag,
                    captureUI, outputFormat),
                cancellationToken);
        }
    }
}
