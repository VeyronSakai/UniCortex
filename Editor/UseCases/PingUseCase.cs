using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.UseCases
{
    internal sealed class PingUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorDomainState _domainState;

        public PingUseCase(IMainThreadDispatcher dispatcher, IEditorDomainState domainState)
        {
            _dispatcher = dispatcher;
            _domainState = domainState;
        }

        public async Task<PingResponse> ExecuteAsync(bool verbose, CancellationToken cancellationToken)
        {
            if (verbose)
            {
                await _dispatcher.RunOnMainThreadAsync(() => Debug.Log("pong"), cancellationToken);
            }

            return new PingResponse(status: "ok", message: "pong", domainId: _domainState.DomainId,
                failedCompilationCount: _domainState.FailedCompilationCount);
        }
    }
}
