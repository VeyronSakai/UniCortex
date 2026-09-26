using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RemoveAnimationCurveUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAnimationClipOperations _operations;

        public RemoveAnimationCurveUseCase(IMainThreadDispatcher dispatcher, IAnimationClipOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string assetPath, string animatorRelativePath, string componentType,
            string assemblyName, string propertyName, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.RemoveCurve(assetPath, animatorRelativePath, componentType, assemblyName,
                    propertyName),
                cancellationToken);
        }
    }
}
