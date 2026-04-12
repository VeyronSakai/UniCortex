namespace UniCortex.Editor.Handlers.Extension
{
    /// <summary>
    /// Base class for user-defined extensions.
    /// Inherit from this class and override the abstract members to define an extension.
    /// Extensions are automatically discovered and exposed as MCP tools and CLI commands.
    /// The <see cref="Execute"/> method runs on the Unity main thread, so Unity API calls are safe.
    /// </summary>
    public abstract class ExtensionHandler
    {
        /// <summary>
        /// The extension name (snake_case recommended). Must be unique across all extensions.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// A description of what the extension does, shown to the AI agent.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Whether the extension is read-only (does not modify state). Defaults to false.
        /// </summary>
        public virtual bool ReadOnly => false;

        /// <summary>
        /// The input parameter schema for this extension. Return null if it takes no parameters.
        /// </summary>
        public virtual ExtensionSchema InputSchema => null;

        /// <summary>
        /// Execute the extension. This method runs on the Unity main thread.
        /// </summary>
        /// <param name="argumentsJson">JSON string of the arguments, or empty string if no arguments.</param>
        /// <returns>Result string that is returned to the AI agent.</returns>
        public abstract string Execute(string argumentsJson);
    }
}
