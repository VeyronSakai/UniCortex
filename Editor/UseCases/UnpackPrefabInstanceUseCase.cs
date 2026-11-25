using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class UnpackPrefabInstanceUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPrefabOperations _operations;

        public UnpackPrefabInstanceUseCase(IMainThreadDispatcher dispatcher, IPrefabOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(int instanceId, string mode, CancellationToken cancellationToken = default)
        {
            var unpackMode = string.IsNullOrEmpty(mode) ? PrefabUnpackModes.OutermostRoot : mode;
            if (unpackMode != PrefabUnpackModes.OutermostRoot && unpackMode != PrefabUnpackModes.Completely)
            {
                throw new ArgumentException(
                    $"Unknown unpack mode: '{mode}'. " +
                    $"Valid values: {PrefabUnpackModes.OutermostRoot}, {PrefabUnpackModes.Completely}.");
            }

            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.UnpackPrefabInstance(instanceId, unpackMode), cancellationToken);
        }
    }
}
