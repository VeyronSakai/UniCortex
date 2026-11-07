using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetAudioSourceOutputUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAudioMixerOperations _operations;

        public SetAudioSourceOutputUseCase(IMainThreadDispatcher dispatcher, IAudioMixerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, int componentIndex, string assetPath, string group,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetAudioSourceOutput(instanceId, componentIndex, assetPath, group), cancellationToken);
        }
    }
}
