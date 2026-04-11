#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class InvokeCustomToolResponse
    {
        public string content;

        public InvokeCustomToolResponse(string content)
        {
            this.content = content;
        }
    }
}
