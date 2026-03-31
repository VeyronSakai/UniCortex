using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StartRecordingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IRecordingOperations _operations;

        public StartRecordingUseCase(IMainThreadDispatcher dispatcher, IRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int fps, string frameRatePlayback, string recordMode,
            float startTime, float endTime, int startFrame, int endFrame, int frameNumber,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.StartRecording(
                    fps, frameRatePlayback, recordMode,
                    startTime, endTime, startFrame, endFrame, frameNumber),
                cancellationToken);
        }
    }
}
