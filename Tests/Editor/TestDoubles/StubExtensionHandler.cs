using UniCortex.Editor.Handlers.Extension;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class StubExtensionHandler : ExtensionHandler
    {
        public override string Name { get; }
        public override string Description { get; }
        public override bool ReadOnly { get; }
        public override ExtensionSchema InputSchema { get; }

        public string ExecuteResult { get; set; } = "ok";
        public string LastArgumentsJson { get; private set; }

        public StubExtensionHandler(string name = "stub_extension", string description = "A stub extension",
            bool readOnly = false, ExtensionSchema inputSchema = null)
        {
            Name = name;
            Description = description;
            ReadOnly = readOnly;
            InputSchema = inputSchema;
        }

        public override string Execute(string argumentsJson)
        {
            LastArgumentsJson = argumentsJson;
            return ExecuteResult;
        }
    }
}
