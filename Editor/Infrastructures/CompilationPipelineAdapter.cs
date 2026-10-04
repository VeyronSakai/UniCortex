using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor.Compilation;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class CompilationPipelineAdapter : ICompilationPipeline
    {
        // All members run on the main thread: RequestScriptCompilation is called through the dispatcher,
        // and the compilation events and NotifyBeforeAssemblyReload are raised there.
        private TaskCompletionSource<bool> _pendingCompilation;
        private bool _hasCompileErrors;

        public CompilationPipelineAdapter()
        {
            // Event subscriptions are discarded by a domain reload, so they live as long as this domain.
            // Compile errors are only reported per assembly (assemblyCompilationFinished), which is raised several
            // times in one compilation, so errors are recorded there and the result is decided once in
            // compilationFinished. EditorUtility.scriptCompilationFailed cannot be used instead: it still holds
            // the result of the previous compilation when compilationFinished is raised.
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        public Task<bool> RequestScriptCompilation()
        {
            // RunContinuationsAsynchronously: the continuation writes the HTTP response, which must not run
            // synchronously on the main thread inside the compilation events or beforeAssemblyReload.
            _pendingCompilation ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            CompilationPipeline.RequestScriptCompilation();
            return _pendingCompilation.Task;
        }

        // Called before the server stops for a domain reload, so that a pending request learns that the
        // compilation succeeded and can respond before the server closes.
        public void NotifyBeforeAssemblyReload()
        {
            CompletePendingCompilation(true);
        }

        private void OnCompilationStarted(object context)
        {
            _hasCompileErrors = false;
        }

        private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (_hasCompileErrors)
            {
                return;
            }

            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    _hasCompileErrors = true;
                    return;
                }
            }
        }

        private void OnCompilationFinished(object context)
        {
            // A successful compilation is followed by a domain reload, reported by NotifyBeforeAssemblyReload.
            if (_hasCompileErrors)
            {
                CompletePendingCompilation(false);
            }
        }

        private void CompletePendingCompilation(bool succeeded)
        {
            _pendingCompilation?.TrySetResult(succeeded);
            _pendingCompilation = null;
        }
    }
}
