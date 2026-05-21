using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetProjectSettingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IProjectSettingsOperations _operations;

        public SetProjectSettingUseCase(IMainThreadDispatcher dispatcher, IProjectSettingsOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(string category, string propertyPath, string value,
            CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetSetting(category, propertyPath, value), cancellationToken);
        }
    }
}
