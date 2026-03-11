#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class UnityServerConfig
    {
        public string server_url;

        public UnityServerConfig(string server_url)
        {
            this.server_url = server_url;
        }
    }
}
