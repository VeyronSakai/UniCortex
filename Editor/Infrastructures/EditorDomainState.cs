using System;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor.Compilation;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorDomainState : IEditorDomainState
    {
        // Only touched by the compilation events, which run on the main thread.
        private bool _hasCompileErrors;

        public string DomainId { get; } = Guid.NewGuid().ToString("N");

        // Written only on the main thread. The HTTP server threads read it on every ping, so a stale value is
        // at worst seen until the next poll.
        public int FailedCompilationCount { get; private set; }

        public EditorDomainState()
        {
            // Event subscriptions are discarded by a domain reload, so they live as long as this domain.
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
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
            if (_hasCompileErrors)
            {
                FailedCompilationCount++;
            }
        }
    }
}
