using System;

namespace UniCortex.Editor.Extensibility
{
    [Serializable]
    public class UniCortexQueryParameter
    {
        public string name;
        public string value;

        public UniCortexQueryParameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
