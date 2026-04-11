#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetCustomToolsManifestResponse
    {
        public CustomToolManifestEntry[] tools;

        public GetCustomToolsManifestResponse(CustomToolManifestEntry[] tools)
        {
            this.tools = tools ?? Array.Empty<CustomToolManifestEntry>();
        }
    }
}
