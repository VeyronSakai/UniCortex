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
        public const string GameObjects = "/gameobjects";
        public const string GameObjectCreate = "/gameobject/create";
        public const string GameObjectDelete = "/gameobject/delete";
        public const string GameObjectModify = "/gameobject/modify";
        public const string TestsRun = "/tests/run";
        public const string TestsResult = "/tests/result";
        public const string ConsoleLogs = "/console/logs";
        public const string ConsoleClear = "/console/clear";
        public const string SceneOpen = "/scene/open";
        public const string SceneSave = "/scene/save";
        public const string SceneHierarchy = "/scene/hierarchy";
        public const string ComponentAdd = "/component/add";
        public const string ComponentRemove = "/component/remove";
        public const string ComponentProperties = "/component/properties";
        public const string ComponentSetProperty = "/component/set-property";
        public const string PrefabCreate = "/prefab/create";
        public const string PrefabInstantiate = "/prefab/instantiate";
    }
}
