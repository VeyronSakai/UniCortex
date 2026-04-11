using System;
using UniCortex.Editor.Handlers.CustomTool;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class StubCustomToolHandler : CustomToolHandler
    {
        public override string ToolName { get; }
        public override string Description { get; }
        public override bool ReadOnly { get; }
        public override CustomToolSchema InputSchema { get; }

        public string ExecuteResult { get; set; } = "ok";
        public Exception ExecuteException { get; set; }
        public string LastArgumentsJson { get; private set; }

        public StubCustomToolHandler(string toolName = "stub_tool", string description = "A stub tool",
            bool readOnly = false, CustomToolSchema inputSchema = null)
        {
            ToolName = toolName;
            Description = description;
            ReadOnly = readOnly;
            InputSchema = inputSchema;
        }

        public override string Execute(string argumentsJson)
        {
            LastArgumentsJson = argumentsJson;
            if (ExecuteException != null) throw ExecuteException;
            return ExecuteResult;
        }
    }
}
