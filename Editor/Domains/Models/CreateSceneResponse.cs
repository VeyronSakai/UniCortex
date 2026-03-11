namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateSceneResponse
    {
        public bool success;

        public CreateSceneResponse(bool success)
        {
            this.success = success;
        }
    }
}
