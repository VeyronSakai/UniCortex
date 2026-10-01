using System;
using System.Threading;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor.Compilation;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class EditorDomainState : IEditorDomainState
    {
        private int _failedCompilationCount;

        // Only touched by the compilation events, which run on the main thread.
        private bool _hasCompileErrors;

        public string DomainId { get; } = Guid.NewGuid().ToString("N");

        // Read from the HTTP server threads.
        public int FailedCompilationCount => Volatile.Read(ref _failedCompilationCount);

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
                Interlocked.Increment(ref _failedCompilationCount);
            }
        }
    }
}
