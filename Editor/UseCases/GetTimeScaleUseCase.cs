using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetTimeScaleUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimeOperations _timeOperations;

        public GetTimeScaleUseCase(IMainThreadDispatcher dispatcher, ITimeOperations timeOperations)
        {
            _dispatcher = dispatcher;
            _timeOperations = timeOperations;
        }

        public async Task<GetTimeScaleResponse> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var timeScale = await _dispatcher.RunOnMainThreadAsync(
                () => _timeOperations.TimeScale, cancellationToken);
            return new GetTimeScaleResponse(timeScale);
        }
    }
}
