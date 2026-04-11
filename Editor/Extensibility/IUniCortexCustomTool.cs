using System;

namespace UniCortex.Editor.Extensibility
{
    public interface IUniCortexCustomTool
    {
        UniCortexCustomToolDefinition Definition { get; }
        Type ArgumentsType { get; }
        string Invoke(string argumentsJson);
    }
}
