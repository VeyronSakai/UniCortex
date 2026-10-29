using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateAudioMixerUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAudioMixerOperations _operations;

        public CreateAudioMixerUseCase(IMainThreadDispatcher dispatcher, IAudioMixerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<CreateAudioMixerResponse> ExecuteAsync(string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Create(assetPath), cancellationToken);
        }
    }
}
