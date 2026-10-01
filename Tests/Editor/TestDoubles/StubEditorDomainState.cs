using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class StubEditorDomainState : IEditorDomainState
    {
        public string DomainId { get; set; } = "domain-id";
        public int FailedCompilationCount { get; set; }
    }
}
