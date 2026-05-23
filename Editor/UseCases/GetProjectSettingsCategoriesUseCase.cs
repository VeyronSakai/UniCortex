using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetProjectSettingsCategoriesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProjectSettingsOperations _operations;

        public GetProjectSettingsCategoriesUseCase(IMainThreadDispatcher dispatcher,
            IProjectSettingsOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetProjectSettingsCategoriesResponse> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetCategories(), cancellationToken);
        }
    }
}
