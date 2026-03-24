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
            var result = await _dispatcher.RunOnMainThreadAsync(() =>
            {
                _timeOperations.TimeScale = timeScale;
                return _timeOperations.TimeScale;
            }, cancellationToken);
            return new SetTimeScaleResponse(true, result);
        }
    }
}
