using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetProjectSettingsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProjectSettingsOperations _operations;

        public GetProjectSettingsUseCase(IMainThreadDispatcher dispatcher, IProjectSettingsOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetProjectSettingsResponse> ExecuteAsync(string category,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetSettings(category), cancellationToken);
        }
    }
}
