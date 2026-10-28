using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class AddAudioMixerGroupUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAudioMixerOperations _operations;

        public AddAudioMixerGroupUseCase(IMainThreadDispatcher dispatcher, IAudioMixerOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<AddAudioMixerGroupResponse> ExecuteAsync(string assetPath, string groupName, string parentGroup,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.AddGroup(assetPath, groupName, parentGroup), cancellationToken);
        }
    }
}
