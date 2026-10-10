using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TypeTextRequest
    {
        // Characters to type into the focused text field, e.g. "Hello" or "こんにちは".
        public string text;
    }
}
