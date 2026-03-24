using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetTimeScaleUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimeOperations _timeOperations;

        public SetTimeScaleUseCase(IMainThreadDispatcher dispatcher, ITimeOperations timeOperations)
        {
            _dispatcher = dispatcher;
            _timeOperations = timeOperations;
        }

        public async Task<SetTimeScaleResponse> ExecuteAsync(float timeScale, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() => _timeOperations.TimeScale = timeScale, cancellationToken);
            return new SetTimeScaleResponse(true);
        }
    }
}
