using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ListProjectSettingsCategoriesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProjectSettingsOperations _operations;

        public ListProjectSettingsCategoriesUseCase(IMainThreadDispatcher dispatcher,
            IProjectSettingsOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<ListProjectSettingsCategoriesResponse> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetCategories(), cancellationToken);
        }
    }
}
