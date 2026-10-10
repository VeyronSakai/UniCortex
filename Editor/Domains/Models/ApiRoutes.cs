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
        public const string EditorSave = "/editor/save";        
        public const string Undo = "/editor/undo";
        public const string Redo = "/editor/redo";
        public const string Platform = "/editor/platform";
        public const string PlatformSwitch = "/editor/platform/switch";
        public const string GameObjects = "/gameobjects";
        public const string GameObjectCreate = "/gameobject/create";
        public const string GameObjectDelete = "/gameobject/delete";
        public const string GameObjectModify = "/gameobject/modify";
        public const string GameObjectDuplicate = "/gameobject/duplicate";
        public const string TestsRun = "/tests/run";
        public const string TestsResult = "/tests/result";
        public const string ConsoleLogs = "/console/logs";
        public const string ConsoleClear = "/console/clear";
        public const string SceneCreate = "/scene/create";
        public const string SceneOpen = "/scene/open";
        public const string Hierarchy = "/hierarchy";
        public const string ComponentAdd = "/component/add";
        public const string ComponentRemove = "/component/remove";
        public const string ComponentProperties = "/component/properties";
        public const string ComponentSetProperty = "/component/set-property";
        public const string PrefabCreate = "/prefab/create";
        public const string PrefabInstantiate = "/prefab/instantiate";
        public const string PrefabOpen = "/prefab/open";
        public const string PrefabClose = "/prefab/close";
        public const string ScriptableObjectCreate = "/scriptable-object/create";
        public const string ScriptableObjectProperties = "/scriptable-object/properties";
        public const string ScriptableObjectProperty = "/scriptable-object/property";
        public const string AnimationClipCreate = "/animation-clip/create";
        public const string AnimationClipCurves = "/animation-clip/curves";
        public const string AnimationClipSetCurve = "/animation-clip/curve/set";
        public const string AnimationClipRemoveCurve = "/animation-clip/curve/remove";
        public const string AssetDatabaseRefresh = "/asset-database/refresh";
        public const string ProjectWindowSelect = "/project-window/select";
        public const string MenuItemExecute = "/menu-item/execute";

        public const string InputKeyPress = "/input/key/press";
        public const string InputTextType = "/input/text/type";
        public const string InputMouseClick = "/input/mouse/click";
        public const string InputMouseMove = "/input/mouse/move";
        public const string InputMouseDrag = "/input/mouse/drag";
        public const string InputUIPointerTargets = "/input/ui-pointer-targets";
        public const string TimelineCreate = "/timeline/create";
        public const string TimelineTracks = "/timeline/tracks";
        public const string TimelineAddTrack = "/timeline/track/add";
        public const string TimelineRemoveTrack = "/timeline/track/remove";
        public const string TimelineBindTrack = "/timeline/track/bind";
        public const string TimelineTrackProperties = "/timeline/track/properties";
        public const string TimelineSetTrackProperty = "/timeline/track/property/set";
        public const string TimelineAddClip = "/timeline/clip/add";
        public const string TimelineRemoveClip = "/timeline/clip/remove";
        public const string TimelineModifyClip = "/timeline/clip/modify";
        public const string TimelineClipProperties = "/timeline/clip/properties";
        public const string TimelineSetClipProperty = "/timeline/clip/property/set";
        public const string TimelinePlay = "/timeline/play";
        public const string TimelineStop = "/timeline/stop";
        public const string TimelineEvaluate = "/timeline/evaluate";
        public const string FocusSceneView = "/scene-view/focus";
        public const string SceneViewCapture = "/scene-view/capture";
        public const string FocusGameView = "/game-view/focus";
        public const string GameViewCapture = "/game-view/capture";
        public const string GameViewSize = "/game-view/size";
        public const string GameViewSizeList = "/game-view/size/list";
        public const string GameViewScale = "/game-view/scale";
        public const string RecorderAllList = "/recorder/all/list";
        public const string RecorderMovieAdd = "/recorder/movie/add";
        public const string RecorderMovieRemove = "/recorder/movie/remove";
        public const string RecorderMovieStart = "/recorder/movie/start";
        public const string RecorderMovieStop = "/recorder/movie/stop";

        public const string ExtensionList = "/extensions/list";
        public const string ExtensionExecute = "/extensions/execute";
    }
}
