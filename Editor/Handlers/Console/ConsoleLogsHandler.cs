using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Console
{
    internal sealed class ConsoleLogsHandler
    {
        private readonly GetConsoleLogsUseCase _useCase;

        public ConsoleLogsHandler(GetConsoleLogsUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Get, ApiRoutes.ConsoleLogs, HandleConsoleLogsAsync);
        }

        private async Task HandleConsoleLogsAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var count = 100;
            var countParam = context.GetQueryParameter("count");
            if (!string.IsNullOrEmpty(countParam) && int.TryParse(countParam, out var parsed))
            {
                count = parsed;
            }

            var includeStackTrace = ParseBool(context.GetQueryParameter("stackTrace"), false);
            var showLog = ParseBool(context.GetQueryParameter("log"), true);
            var showWarning = ParseBool(context.GetQueryParameter("warning"), true);
            var showError = ParseBool(context.GetQueryParameter("error"), true);

            var logs = await _useCase.ExecuteAsync(count, includeStackTrace, showLog, showWarning, showError,
                cancellationToken);
            var json = JsonUtility.ToJson(new ConsoleLogsResponse(logs));
            await context.WriteResponseAsync(200, json);
        }

        private static bool ParseBool(string value, bool defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return value == "true" || value == "1";
        }
    }
}
