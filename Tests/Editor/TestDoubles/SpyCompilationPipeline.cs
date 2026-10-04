using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyCompilationPipeline : ICompilationPipeline
    {
        public int RequestScriptCompilationCallCount { get; private set; }

        // Result of the requested compilation: true when the domain is about to be reloaded,
        // false when the compilation failed.
        public bool CompilationSucceeds { get; set; } = true;

        public Task<bool> RequestScriptCompilation()
        {
            RequestScriptCompilationCallCount++;
            return Task.FromResult(CompilationSucceeds);
        }
    }
}
