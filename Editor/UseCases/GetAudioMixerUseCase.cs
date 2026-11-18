using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetAudioMixerUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAudioMixerOperations _operations;

        public GetAudioMixerUseCase(IMainThreadDispatcher dispatcher, IAudioMixerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetAudioMixerResponse> ExecuteAsync(string assetPath,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Get(assetPath), cancellationToken);
        }
    }
}
