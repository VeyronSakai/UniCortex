namespace UniCortex.Editor.Domains.Models
{
    public static class ApiRoutes
    {
        public const string Ping = "/editor/ping";
        public const string Play = "/editor/play";
        public const string Stop = "/editor/stop";
        public const string Status = "/editor/status";
        public const string Pause = "/editor/pause";
        public const string Unpause = "/editor/unpause";
        public const string Step = "/editor/step";
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
        public const string SceneCreate = "/scene/create";
        public const string SceneOpen = "/scene/open";
        public const string SceneSave = "/scene/save";
        public const string SceneHierarchy = "/scene/hierarchy";
        public const string ComponentAdd = "/component/add";
        public const string ComponentRemove = "/component/remove";
        public const string ComponentProperties = "/component/properties";
        public const string ComponentSetProperty = "/component/set-property";
        public const string PrefabCreate = "/prefab/create";
        public const string PrefabInstantiate = "/prefab/instantiate";
        public const string AssetDatabaseRefresh = "/asset-database/refresh";
        public const string MenuItemExecute = "/menu-item/execute";
        public const string GameViewCapture = "/game-view/capture";
        public const string SceneViewCapture = "/scene-view/capture";
        public const string InputKey = "/input/key";
        public const string InputMouse = "/input/mouse";
        public const string TimelineCreate = "/timeline/create";
        public const string TimelineAddTrack = "/timeline/track/add";
        public const string TimelineRemoveTrack = "/timeline/track/remove";
        public const string TimelineBindTrack = "/timeline/track/bind";
        public const string TimelineAddClip = "/timeline/clip/add";
        public const string TimelineRemoveClip = "/timeline/clip/remove";
    }
}
