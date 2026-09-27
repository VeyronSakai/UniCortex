namespace UniCortex.Core.Domains.CodeGraph;

public enum CodeEdgeKind
{
    /// <summary>Type-to-type: the source class derives from the target class.</summary>
    Inherits,

    /// <summary>Type-to-type: the source type implements the target interface.</summary>
    Implements,

    /// <summary>Member-to-member: the source member implements the target interface member.</summary>
    ImplementsMember,

    /// <summary>Member-to-member: the source method overrides the target method.</summary>
    Overrides,

    /// <summary>Semantically resolved call from the source member to the target method.</summary>
    Calls,

    /// <summary>
    /// A base-qualified call (base.Foo()). Distinguished from Calls because it is
    /// non-virtual and can never dispatch to an override of the caller.
    /// </summary>
    CallsBase,

    /// <summary>Name-matched call whose target could not be resolved semantically.</summary>
    CallsUnresolved,

    /// <summary>The source symbol references the target type or member (non-call usage).</summary>
    Uses
}

public sealed record CodeEdge(string FromId, string ToId, CodeEdgeKind Kind, string File, int Line);
