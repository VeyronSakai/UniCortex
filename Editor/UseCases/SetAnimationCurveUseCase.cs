using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetAnimationCurveUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAnimationClipOperations _operations;

        public SetAnimationCurveUseCase(IMainThreadDispatcher dispatcher, IAnimationClipOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string assetPath, string path, string componentType, string assemblyName,
            string propertyName, List<AnimationCurveKeyInput> keys, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetCurve(assetPath, path, componentType, assemblyName, propertyName, keys),
                cancellationToken);
        }
    }
}
