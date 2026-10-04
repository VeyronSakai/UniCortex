using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RequestDomainReloadUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ICompilationPipeline _compilationPipeline;
        private readonly IEditorApplication _editorApplication;

        public RequestDomainReloadUseCase(IMainThreadDispatcher dispatcher, ICompilationPipeline compilationPipeline,
            IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _compilationPipeline = compilationPipeline;
            _editorApplication = editorApplication;
        }

        // Returns true when the compilation succeeded and the domain is about to be reloaded,
        // or false when the compilation failed with errors.
        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var compilation = await _dispatcher.RunOnMainThreadAsync(() =>
            {
                // During play mode, compilation may be put off until play mode ends
                // ("Script Changes While Playing" preference). The request would then be held until then,
                // and the server handles one request at a time, so no other request could stop play mode.
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot reload the domain during play mode. Exit play mode first.");
                }

                Debug.Log("[UniCortex] Domain Reload");
                return _compilationPipeline.RequestScriptCompilation();
            }, cancellationToken);

            // The cancellation token is not used here. The server is only stopped for a domain reload, and
            // the result is set before that (see EntryPoint.Shutdown). Cancelling would answer 503, and the
            // client would resend the request and reload the domain once more.
            return await compilation;
        }
    }
}
