using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class AddRecorderUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IRecordingOperations _operations;

        public AddRecorderUseCase(IMainThreadDispatcher dispatcher, IRecordingOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<string> ExecuteAsync(string name, string outputPath,
            string encoder = RecorderDefaults.EncoderUnityMedia,
            string encodingQuality = RecorderDefaults.QualityLow,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.AddRecorder(name, outputPath, encoder, encodingQuality),
                cancellationToken);
        }
    }
}
