namespace UniCortex.Editor.Handlers.CustomTool
{
    /// <summary>
    /// Base class for user-defined custom MCP tools.
    /// Inherit from this class and override the abstract members to define a custom tool.
    /// The <see cref="Execute"/> method runs on the Unity main thread, so Unity API calls are safe.
    /// </summary>
    public abstract class CustomToolHandler
    {
        /// <summary>
        /// The MCP tool name (snake_case recommended). Must be unique across all custom tools.
        /// </summary>
        public abstract string ToolName { get; }

        /// <summary>
        /// A description of what the tool does, shown to the AI agent.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Whether the tool is read-only (does not modify state). Defaults to false.
        /// </summary>
        public virtual bool ReadOnly => false;

        /// <summary>
        /// The input parameter schema for this tool. Return null if the tool takes no parameters.
        /// </summary>
        public virtual CustomToolSchema InputSchema => null;

        /// <summary>
        /// Execute the custom tool. This method runs on the Unity main thread.
        /// </summary>
        /// <param name="argumentsJson">JSON string of the tool arguments, or empty string if no arguments.</param>
        /// <returns>Result string that is returned to the AI agent.</returns>
        public abstract string Execute(string argumentsJson);
    }
}
