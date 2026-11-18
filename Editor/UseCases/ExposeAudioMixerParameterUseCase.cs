using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ExposeAudioMixerParameterUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAudioMixerOperations _operations;

        public ExposeAudioMixerParameterUseCase(IMainThreadDispatcher dispatcher, IAudioMixerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string assetPath, string group, string parameter, string exposedName,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.ExposeParameter(assetPath, group, parameter, exposedName), cancellationToken);
        }
    }
}
