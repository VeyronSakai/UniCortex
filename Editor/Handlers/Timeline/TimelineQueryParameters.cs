using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Handlers.Timeline
{
    /// <summary>
    /// Parses the query parameters shared by the Timeline GET endpoints.
    /// Each method returns an error message, or null when the parameters are valid.
    /// </summary>
    internal static class TimelineQueryParameters
    {
        private const string InstanceIdName = "instanceId";
        private const string AssetPathName = "assetPath";

        /// <summary>
        /// Reads the target timeline: either instanceId (a PlayableDirector) or assetPath.
        /// instanceId is 0 when it is omitted.
        /// </summary>
        public static string ParseTarget(IRequestContext context, out int instanceId, out string assetPath)
        {
            assetPath = context.GetQueryParameter(AssetPathName);
            instanceId = 0;

            var instanceIdParam = context.GetQueryParameter(InstanceIdName);
            if (!string.IsNullOrEmpty(instanceIdParam) && !int.TryParse(instanceIdParam, out instanceId))
            {
                return $"{InstanceIdName} must be an integer.";
            }

            if (instanceId == 0 && string.IsNullOrEmpty(assetPath))
            {
                return $"Either {InstanceIdName} or {AssetPathName} query parameter is required.";
            }

            return null;
        }

        /// <summary>
        /// Reads a required index parameter such as trackIndex or clipIndex.
        /// </summary>
        public static string ParseIndex(IRequestContext context, string name, out int index)
        {
            index = 0;
            var param = context.GetQueryParameter(name);
            if (string.IsNullOrEmpty(param))
            {
                return $"{name} query parameter is required.";
            }

            return int.TryParse(param, out index) ? null : $"{name} must be an integer.";
        }
    }
}
