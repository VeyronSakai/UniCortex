using System.Text.RegularExpressions;

namespace UniCortex.Editor.Handlers.Input
{
    // Validation shared by the pointer and GameObject handlers.
    // JsonUtility gives 0 for a missing number, and 0 is a valid coordinate, so the presence of a coordinate is
    // checked by matching the body.
    internal static class PointerRequestParser
    {
        // Matches the key followed by a colon so that a string value such as "x" is not taken as the key.
        public static bool HasField(string body, string name)
        {
            return Regex.IsMatch(body, $"\"{name}\"\\s*:");
        }

        // Returns an error message for the first of the given fields missing from the body, or null.
        public static string FindMissingField(string body, params string[] names)
        {
            foreach (var name in names)
            {
                if (!HasField(body, name))
                {
                    return $"{name} is required.";
                }
            }

            return null;
        }

        // Returns an error message when the instanceId is 0 (missing from the body, or invalid), or null.
        public static string ValidateInstanceId(int instanceId, string name)
        {
            return instanceId == 0 ? $"{name} is required and must not be 0." : null;
        }
    }
}
