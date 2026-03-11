#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TestResultEntry
    {
        public string name;
        public string status;
        public float duration;
        public string message;

        public TestResultEntry(string name, string status, float duration, string message = "")
        {
            this.name = name;
            this.status = status;
            this.duration = duration;
            this.message = message;
        }
    }
}
