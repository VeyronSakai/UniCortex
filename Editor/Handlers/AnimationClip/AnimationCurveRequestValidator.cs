using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Handlers.AnimationClip
{
    internal static class AnimationCurveRequestValidator
    {
        // Returns the name of the first missing required field, or null when all are present.
        // "path" is intentionally not validated: an empty path targets the Animator's own GameObject.
        public static string FindMissingField(string assetPath, string componentType, string assemblyName,
            string propertyName)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return nameof(RemoveAnimationCurveRequest.assetPath);
            }

            if (string.IsNullOrEmpty(componentType))
            {
                return nameof(RemoveAnimationCurveRequest.componentType);
            }

            if (string.IsNullOrEmpty(assemblyName))
            {
                return nameof(RemoveAnimationCurveRequest.assemblyName);
            }

            return string.IsNullOrEmpty(propertyName) ? nameof(RemoveAnimationCurveRequest.propertyName) : null;
        }
    }
}
