using System.Text.RegularExpressions;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Handlers.Input
{
    // JsonUtility does not support Nullable<T>, so the pointer handlers deserialize into non-nullable fields and
    // detect the presence of a field by matching the body, because 0 is a valid coordinate.
    internal static class PointerRequestParser
    {
        public const string PositionRequiredMessage = "Specify either x and y, or instanceId.";

        // Matches the key followed by a colon so that a string value such as "x" is not taken as the key.
        public static bool HasField(string body, string name)
        {
            return Regex.IsMatch(body, $"\"{name}\"\\s*:");
        }

        // Returns the position given by exactly one of the coordinates (xName and yName) or the target (targetName).
        // Returns null and sets error when the fields are not given that way.
        public static PointerPosition ParsePosition(string body, float x, float y, int instanceId,
            string xName, string yName, string targetName, out string error)
        {
            var hasX = HasField(body, xName);
            var hasY = HasField(body, yName);
            var hasTarget = HasField(body, targetName);

            if (hasX != hasY)
            {
                error = $"{xName} and {yName} must be specified together.";
                return null;
            }

            if (hasX == hasTarget)
            {
                error = $"Specify either {xName} and {yName}, or {targetName}.";
                return null;
            }

            if (hasTarget && instanceId == 0)
            {
                error = $"{targetName} must not be 0.";
                return null;
            }

            error = null;
            return hasTarget ? new PointerPosition.Target(instanceId) : new PointerPosition.Coordinates(x, y);
        }
    }
}
