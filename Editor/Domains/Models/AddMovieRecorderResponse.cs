using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddMovieRecorderResponse
    {
        public string name;

        public AddMovieRecorderResponse(string name)
        {
            this.name = name;
        }
    }
}
