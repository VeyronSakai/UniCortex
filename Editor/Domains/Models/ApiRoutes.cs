namespace UniCortex.Editor.Domains.Models
{
    public static class ApiRoutes
    {
        public const string Ping = "/editor/ping";
        public const string Play = "/editor/play";
        public const string Stop = "/editor/stop";
        public const string Status = "/editor/status";
        public const string DomainReload = "/editor/domain-reload";
        public const string Undo = "/editor/undo";
        public const string Redo = "/editor/redo";
        public const string GameObjectCreate = "/gameobject/create";
        public const string TestsRun = "/tests/run";
        public const string TestsResult = "/tests/result";
        public const string ConsoleLogs = "/console/logs";
        public const string ConsoleClear = "/console/clear";
    }
}
