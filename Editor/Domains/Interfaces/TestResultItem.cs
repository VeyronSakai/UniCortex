namespace UniCortex.Editor.Domains.Interfaces
{
    internal sealed class TestResultItem
    {
        public string Name { get; }
        public string Status { get; }
        public float Duration { get; }
        public string Message { get; }

        public TestResultItem(string name, string status, float duration, string message = "")
        {
            Name = name;
            Status = status;
            Duration = duration;
            Message = message;
        }
    }
}
