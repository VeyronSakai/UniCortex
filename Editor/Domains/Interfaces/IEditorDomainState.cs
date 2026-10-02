namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IEditorDomainState
    {
        // Identifies the current domain. It changes on every domain reload.
        string DomainId { get; }

        // Number of script compilations in the current domain that ended with errors.
        // Such compilations do not reload the domain, so DomainId stays the same.
        int FailedCompilationCount { get; }
    }
}
