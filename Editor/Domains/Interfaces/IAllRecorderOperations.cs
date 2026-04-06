using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IAllRecorderOperations
    {
        RecorderEntry[] GetRecorderList();
    }
}
