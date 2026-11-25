using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Extensions;

public static class SerializedPropertyEntryExtensions
{
    /// <summary>
    /// Clears the empty <c>reference</c> of the entries that are not object references. Unity's JsonUtility writes
    /// an empty string for it, and a null one is left out of the output.
    /// </summary>
    public static void OmitEmptyReferences(this IEnumerable<SerializedPropertyEntry>? entries)
    {
        if (entries is null)
        {
            return;
        }

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.reference))
            {
                entry.reference = null;
            }
        }
    }
}
