using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.AnimationClip;
using UniCortex.Editor.Handlers.Asset;
using UniCortex.Editor.Handlers.Component;
using UniCortex.Editor.Handlers.Console;
using UniCortex.Editor.Handlers.Extension;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Handlers.GameObject;
using UniCortex.Editor.Handlers.Prefab;
using UniCortex.Editor.Handlers.Scene;
using UniCortex.Editor.Handlers.ScriptableObject;
using UniCortex.Editor.Handlers.Tests;
using UniCortex.Editor.Handlers.MenuItem;
using UniCortex.Editor.Handlers.Input;
using UniCortex.Editor.Handlers.SceneView;
using UniCortex.Editor.Handlers.GameView;
using UniCortex.Editor.Handlers.ProjectWindow;
using UniCortex.Editor.Handlers.MovieRecorder;
using UniCortex.Editor.Handlers.Timeline;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Settings;
using UniCortex.Editor.UseCases;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniCortex.Editor
{
    [InitializeOnLoad]
    internal static class EntryPoint
    {
        private const string PortKey = "UniCortex.Port";

        private const string TestResultJsonKey = "UniCortex.TestResultJson";
        private const string TestPendingResultsJsonKey = "UniCortex.TestPendingResultsJson";

        static EntryPoint()
        {
            // AssetImportWorkerProcess runs in a separate process with its own SessionState,
            // so it would pick a new port via FindFreePort() and overwrite Library/UniCortex/config.json,
            // causing the MCP server to connect to a port that doesn't belong to the main Editor.
            if (AssetDatabase.IsAssetImportWorkerProcess())
            {
                return;
            }

            var dispatcher = new MainThreadDispatcher();
            var playerLoopDispatcher = new PlayerLoopDispatcher(new EditorApplicationAdapter(),
                new PlayerLoopAdapter());
            var compilationPipeline = new CompilationPipelineAdapter();

            var server = StartServer(dispatcher, playerLoopDispatcher, compilationPipeline);

            EditorApplication.update += dispatcher.OnUpdate;
            EditorApplication.playModeStateChanged += playerLoopDispatcher.OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += () => Shutdown(compilationPipeline, server);
            EditorApplication.quitting += OnQuit;
        }

        private static HttpListenerServer StartServer(IMainThreadDispatcher dispatcher,
            IPlayerLoopDispatcher playerLoopDispatcher, ICompilationPipeline compilationPipeline)
        {
            var port = SessionState.GetInt(PortKey, 0);
            if (port == 0)
            {
                port = FindFreePort();
                SessionState.SetInt(PortKey, port);
            }

            var router = new RequestRouter();

            RegisterHandlers(router, dispatcher, playerLoopDispatcher, compilationPipeline);

            var server = new HttpListenerServer(router, port);
            try
            {
                server.Start();
                ServerUrlFile.Write(port);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UniCortex] Failed to start server on port {port}: {ex.Message}");
            }

            return server;
        }

        private static void RegisterHandlers(RequestRouter router, IMainThreadDispatcher dispatcher,
            IPlayerLoopDispatcher playerLoopDispatcher, ICompilationPipeline compilationPipeline)
        {
            var testResultStore = new TestResultStore(TestResultJsonKey, TestPendingResultsJsonKey);
            RegisterTestCallbacks(testResultStore);

            var editorApplication = new EditorApplicationAdapter();

            var pingUseCase = new PingUseCase(dispatcher);
            var pingHandler = new PingHandler(pingUseCase);

            var sceneManagerAdapter = new EditorSceneManagerAdapter();

            var playUseCase = new PlayUseCase(dispatcher, editorApplication);
            var playHandler = new PlayHandler(playUseCase);

            var stopUseCase = new StopUseCase(dispatcher, editorApplication);
            var stopHandler = new StopHandler(stopUseCase);

            var requestDomainReloadUseCase = new RequestDomainReloadUseCase(dispatcher, compilationPipeline,
                editorApplication);
            var requestDomainReloadHandler = new DomainReloadHandler(requestDomainReloadUseCase);

            var getEditorStatusUseCase = new GetEditorStatusUseCase(dispatcher, editorApplication);
            var editorStatusHandler = new EditorStatusHandler(getEditorStatusUseCase);

            var pauseUseCase = new PauseUseCase(dispatcher, editorApplication);
            var pauseHandler = new PauseHandler(pauseUseCase);

            var unpauseUseCase = new UnpauseUseCase(dispatcher, editorApplication);
            var unpauseHandler = new UnpauseHandler(unpauseUseCase);

            var stepUseCase = new StepUseCase(dispatcher, editorApplication);
            var stepHandler = new StepHandler(stepUseCase);

            var undoAdapter = new UndoAdapter();

            var undoUseCase = new UndoUseCase(dispatcher, undoAdapter);
            var undoHandler = new UndoHandler(undoUseCase);

            var redoUseCase = new RedoUseCase(dispatcher, undoAdapter);
            var redoHandler = new RedoHandler(redoUseCase);

            var testRunnerAdapter = new TestRunnerAdapter(dispatcher, testResultStore);
            var runTestsUseCase = new RunTestsUseCase(testRunnerAdapter, dispatcher, editorApplication);
            var runTestsHandler = new RunTestsHandler(runTestsUseCase);
            var testResultHandler = new TestResultHandler(dispatcher, testResultStore);

            var consoleLogCollector = new ConsoleLogCollector();

            var getConsoleLogsUseCase = new GetConsoleLogsUseCase(dispatcher, consoleLogCollector);
            var consoleLogsHandler = new ConsoleLogsHandler(getConsoleLogsUseCase);

            var clearConsoleLogsUseCase = new ClearConsoleLogsUseCase(dispatcher, consoleLogCollector);
            var consoleClearHandler = new ConsoleClearHandler(clearConsoleLogsUseCase);

            var createSceneUseCase = new CreateSceneUseCase(dispatcher, sceneManagerAdapter, editorApplication);
            var createSceneHandler = new CreateSceneHandler(createSceneUseCase);

            var openSceneUseCase = new OpenSceneUseCase(dispatcher, sceneManagerAdapter, editorApplication);
            var openSceneHandler = new OpenSceneHandler(openSceneUseCase);

            var saveUseCase = new SaveUseCase(dispatcher, editorApplication);
            var saveHandler = new Handlers.Editor.SaveHandler(saveUseCase);

            var getHierarchyUseCase = new GetHierarchyUseCase(dispatcher, sceneManagerAdapter);
            var hierarchyHandler = new HierarchyHandler(getHierarchyUseCase);

            var gameObjectOps = new GameObjectOperationsAdapter();

            var getGameObjectsUseCase = new GetGameObjectsUseCase(dispatcher, gameObjectOps);
            var getGameObjectsHandler = new GetGameObjectsHandler(getGameObjectsUseCase);

            var createGameObjectUseCase = new CreateGameObjectUseCase(dispatcher, gameObjectOps);
            var createGameObjectHandler = new CreateGameObjectHandler(createGameObjectUseCase);

            var deleteGameObjectUseCase = new DeleteGameObjectUseCase(dispatcher, gameObjectOps);
            var deleteGameObjectHandler = new DeleteGameObjectHandler(deleteGameObjectUseCase);

            var modifyGameObjectUseCase = new ModifyGameObjectUseCase(dispatcher, gameObjectOps);
            var modifyGameObjectHandler = new ModifyGameObjectHandler(modifyGameObjectUseCase);

            var duplicateGameObjectUseCase = new DuplicateGameObjectUseCase(dispatcher, gameObjectOps);
            var duplicateGameObjectHandler = new DuplicateGameObjectHandler(duplicateGameObjectUseCase);

            var componentOps = new ComponentOperationsAdapter();

            var addComponentUseCase = new AddComponentUseCase(dispatcher, componentOps);
            var addComponentHandler = new AddComponentHandler(addComponentUseCase);

            var removeComponentUseCase = new RemoveComponentUseCase(dispatcher, componentOps);
            var removeComponentHandler = new RemoveComponentHandler(removeComponentUseCase);

            var getComponentPropertiesUseCase = new GetComponentPropertiesUseCase(dispatcher, componentOps);
            var componentPropertiesHandler = new ComponentPropertiesHandler(getComponentPropertiesUseCase);

            var setComponentPropertyUseCase = new SetComponentPropertyUseCase(dispatcher, componentOps);
            var setComponentPropertyHandler = new SetComponentPropertyHandler(setComponentPropertyUseCase);

            var prefabOps = new PrefabOperationsAdapter();

            var createPrefabUseCase = new CreatePrefabUseCase(dispatcher, prefabOps);
            var createPrefabHandler = new CreatePrefabHandler(createPrefabUseCase);

            var instantiatePrefabUseCase = new InstantiatePrefabUseCase(dispatcher, prefabOps);
            var instantiatePrefabHandler = new InstantiatePrefabHandler(instantiatePrefabUseCase);

            var openPrefabUseCase = new OpenPrefabUseCase(dispatcher, prefabOps);
            var openPrefabHandler = new OpenPrefabHandler(openPrefabUseCase);

            var closePrefabUseCase = new ClosePrefabUseCase(dispatcher, prefabOps);
            var closePrefabHandler = new ClosePrefabHandler(closePrefabUseCase);

            var scriptableObjectOps = new ScriptableObjectOperationsAdapter();

            var createScriptableObjectUseCase = new CreateScriptableObjectUseCase(dispatcher, scriptableObjectOps);
            var createScriptableObjectHandler = new CreateScriptableObjectHandler(createScriptableObjectUseCase);

            var getScriptableObjectPropertiesUseCase =
                new GetScriptableObjectPropertiesUseCase(dispatcher, scriptableObjectOps);
            var scriptableObjectPropertiesHandler =
                new ScriptableObjectPropertiesHandler(getScriptableObjectPropertiesUseCase);

            var setScriptableObjectPropertyUseCase =
                new SetScriptableObjectPropertyUseCase(dispatcher, scriptableObjectOps);
            var setScriptableObjectPropertyHandler =
                new SetScriptableObjectPropertyHandler(setScriptableObjectPropertyUseCase);

            var animationClipOps = new AnimationClipOperationsAdapter();

            var createAnimationClipUseCase = new CreateAnimationClipUseCase(dispatcher, animationClipOps);
            var createAnimationClipHandler = new CreateAnimationClipHandler(createAnimationClipUseCase);

            var getAnimationCurvesUseCase = new GetAnimationCurvesUseCase(dispatcher, animationClipOps);
            var animationCurvesHandler = new AnimationCurvesHandler(getAnimationCurvesUseCase);

            var setAnimationCurveUseCase = new SetAnimationCurveUseCase(dispatcher, animationClipOps);
            var setAnimationCurveHandler = new SetAnimationCurveHandler(setAnimationCurveUseCase);

            var removeAnimationCurveUseCase = new RemoveAnimationCurveUseCase(dispatcher, animationClipOps);
            var removeAnimationCurveHandler = new RemoveAnimationCurveHandler(removeAnimationCurveUseCase);


            var assetDbOps = new AssetDatabaseOperationsAdapter();
            var projectWindowOps = new ProjectWindowOperationsAdapter();

            var refreshAssetDatabaseUseCase = new RefreshAssetDatabaseUseCase(dispatcher, assetDbOps);
            var assetRefreshHandler = new AssetDatabaseRefreshHandler(refreshAssetDatabaseUseCase);

            var selectProjectWindowAssetUseCase = new SelectProjectWindowAssetUseCase(dispatcher, projectWindowOps);
            var selectProjectWindowAssetHandler =
                new SelectProjectWindowAssetHandler(selectProjectWindowAssetUseCase);


            var menuItemOps = new MenuItemOperationsAdapter();
            var captureOps = new CaptureOperationsAdapter();

            var executeMenuItemUseCase = new ExecuteMenuItemUseCase(dispatcher, menuItemOps);
            var executeMenuItemHandler = new ExecuteMenuItemHandler(executeMenuItemUseCase);

            var editorWindowOps = new EditorWindowOperationsAdapter();

            var captureGameViewUseCase =
                new CaptureGameViewUseCase(dispatcher, editorApplication, editorWindowOps, captureOps);
            var captureGameViewHandler = new CaptureGameViewHandler(captureGameViewUseCase);

            var captureSceneViewUseCase = new CaptureSceneViewUseCase(dispatcher, editorWindowOps, captureOps);
            var captureSceneViewHandler = new CaptureSceneViewHandler(captureSceneViewUseCase);

            var focusSceneViewUseCase = new FocusSceneViewUseCase(dispatcher, editorWindowOps);
            var focusSceneViewHandler = new FocusSceneViewHandler(focusSceneViewUseCase);

            var focusGameViewUseCase = new FocusGameViewUseCase(dispatcher, editorWindowOps);
            var focusGameViewHandler = new FocusGameViewHandler(focusGameViewUseCase);

            var getGameViewSizeUseCase = new GetGameViewSizeUseCase(dispatcher, editorWindowOps);
            var getGameViewSizeHandler = new GetGameViewSizeHandler(getGameViewSizeUseCase);

            var getGameViewSizeListUseCase = new GetGameViewSizeListUseCase(dispatcher, editorWindowOps);
            var getGameViewSizeListHandler = new GetGameViewSizeListHandler(getGameViewSizeListUseCase);

            var setGameViewSizeUseCase = new SetGameViewSizeUseCase(dispatcher, editorWindowOps);
            var setGameViewSizeHandler = new SetGameViewSizeHandler(setGameViewSizeUseCase);

            var getGameViewScaleUseCase = new GetGameViewScaleUseCase(dispatcher, editorWindowOps);
            var getGameViewScaleHandler = new GetGameViewScaleHandler(getGameViewScaleUseCase);

            var setGameViewScaleUseCase = new SetGameViewScaleUseCase(dispatcher, editorWindowOps);
            var setGameViewScaleHandler = new SetGameViewScaleHandler(setGameViewScaleUseCase);

#if UNICORTEX_RECORDER
            var allRecorderOps = new AllRecorderOperationsAdapter();
            var movieRecordingOps = new MovieRecordingOperationsAdapter();
#else
            var allRecorderOps = new AllRecorderNotSupportedAdapter();
            var movieRecordingOps = new MovieRecordingNotSupportedAdapter();
#endif

            var addMovieRecorderUseCase = new AddMovieRecorderUseCase(dispatcher, movieRecordingOps);
            var addMovieRecorderHandler = new AddMovieRecorderHandler(addMovieRecorderUseCase);

            var getRecorderListUseCase = new GetRecorderListUseCase(dispatcher, allRecorderOps);
            var getRecorderListHandler = new GetRecorderListHandler(getRecorderListUseCase);

            var removeMovieRecorderUseCase = new RemoveMovieRecorderUseCase(dispatcher, movieRecordingOps);
            var removeMovieRecorderHandler = new RemoveMovieRecorderHandler(removeMovieRecorderUseCase);

            var startMovieRecordingUseCase = new StartMovieRecordingUseCase(dispatcher, movieRecordingOps);
            var startMovieRecorderHandler = new StartMovieRecorderHandler(startMovieRecordingUseCase);

            var stopMovieRecordingUseCase = new StopMovieRecordingUseCase(dispatcher, movieRecordingOps);
            var stopMovieRecorderHandler = new StopMovieRecorderHandler(stopMovieRecordingUseCase);

#if UNICORTEX_INPUT_SYSTEM
            var inputSimOps = new InputOperationsAdapter();
            EditorApplication.playModeStateChanged += inputSimOps.OnPlayModeStateChanged;
#else
            var inputSimOps = new InputNotSupportedAdapter();
#endif

#if UNICORTEX_UGUI
            var uiPointerTargetOps = new UIPointerTargetOperationsAdapter(playerLoopDispatcher);
#else
            var uiPointerTargetOps = new UIPointerTargetNotSupportedAdapter();
#endif

            var pointerPositionResolver = new PointerPositionResolver(dispatcher, uiPointerTargetOps);

            var time = new TimeAdapter();
            var playerLoopRunner = new PlayerLoopRunner(dispatcher, playerLoopDispatcher);

            var pressKeyUseCase = new PressKeyUseCase(playerLoopRunner, inputSimOps, time);
            var pressKeyHandler = new PressKeyHandler(pressKeyUseCase);

            var clickMouseUseCase = new ClickMouseUseCase(playerLoopRunner, pointerPositionResolver, inputSimOps,
                time);
            var clickMouseHandler = new ClickMouseHandler(clickMouseUseCase);

            var moveMouseUseCase = new MoveMouseUseCase(dispatcher, pointerPositionResolver, inputSimOps);
            var moveMouseHandler = new MoveMouseHandler(moveMouseUseCase);

            var dragMouseUseCase = new DragMouseUseCase(playerLoopRunner, pointerPositionResolver, inputSimOps,
                time);
            var dragMouseHandler = new DragMouseHandler(dragMouseUseCase);

            var getUIPointerTargetsUseCase = new GetUIPointerTargetsUseCase(dispatcher, uiPointerTargetOps);
            var getUIPointerTargetsHandler = new GetUIPointerTargetsHandler(getUIPointerTargetsUseCase);

            var timelineOps = new TimelineOperationsAdapter();

            var createTimelineUseCase = new CreateTimelineUseCase(dispatcher, timelineOps);
            var createTimelineHandler = new CreateTimelineHandler(createTimelineUseCase);

            var addTimelineTrackUseCase = new AddTimelineTrackUseCase(dispatcher, timelineOps);
            var addTimelineTrackHandler = new AddTimelineTrackHandler(addTimelineTrackUseCase);

            var removeTimelineTrackUseCase = new RemoveTimelineTrackUseCase(dispatcher, timelineOps);
            var removeTimelineTrackHandler = new RemoveTimelineTrackHandler(removeTimelineTrackUseCase);

            var bindTimelineTrackUseCase = new BindTimelineTrackUseCase(dispatcher, timelineOps);
            var bindTimelineTrackHandler = new BindTimelineTrackHandler(bindTimelineTrackUseCase);

            var addTimelineClipUseCase = new AddTimelineClipUseCase(dispatcher, timelineOps);
            var addTimelineClipHandler = new AddTimelineClipHandler(addTimelineClipUseCase);

            var removeTimelineClipUseCase = new RemoveTimelineClipUseCase(dispatcher, timelineOps);
            var removeTimelineClipHandler = new RemoveTimelineClipHandler(removeTimelineClipUseCase);

            var getTimelineTracksUseCase = new GetTimelineTracksUseCase(dispatcher, timelineOps);
            var getTimelineTracksHandler = new GetTimelineTracksHandler(getTimelineTracksUseCase);

            var getTimelineTrackPropertiesUseCase = new GetTimelineTrackPropertiesUseCase(dispatcher, timelineOps);
            var getTimelineTrackPropertiesHandler = new GetTimelineTrackPropertiesHandler(getTimelineTrackPropertiesUseCase);

            var getTimelineClipPropertiesUseCase = new GetTimelineClipPropertiesUseCase(dispatcher, timelineOps);
            var getTimelineClipPropertiesHandler = new GetTimelineClipPropertiesHandler(getTimelineClipPropertiesUseCase);

            var modifyTimelineClipUseCase = new ModifyTimelineClipUseCase(dispatcher, timelineOps);
            var modifyTimelineClipHandler = new ModifyTimelineClipHandler(modifyTimelineClipUseCase);

            var setTimelineClipPropertyUseCase =
                new SetTimelineClipPropertyUseCase(dispatcher, timelineOps);
            var setTimelineClipPropertyHandler =
                new SetTimelineClipPropertyHandler(setTimelineClipPropertyUseCase);

            var setTimelineTrackPropertyUseCase = new SetTimelineTrackPropertyUseCase(dispatcher, timelineOps);
            var setTimelineTrackPropertyHandler = new SetTimelineTrackPropertyHandler(setTimelineTrackPropertyUseCase);

            var playTimelineUseCase = new PlayTimelineUseCase(dispatcher, timelineOps);
            var playTimelineHandler = new PlayTimelineHandler(playTimelineUseCase);

            var stopTimelineUseCase = new StopTimelineUseCase(dispatcher, timelineOps);
            var stopTimelineHandler = new StopTimelineHandler(stopTimelineUseCase);

            var evaluateTimelineUseCase = new EvaluateTimelineUseCase(dispatcher, timelineOps);
            var evaluateTimelineHandler = new EvaluateTimelineHandler(evaluateTimelineUseCase);

            pingHandler.Register(router);
            playHandler.Register(router);
            stopHandler.Register(router);
            requestDomainReloadHandler.Register(router);
            editorStatusHandler.Register(router);
            pauseHandler.Register(router);
            unpauseHandler.Register(router);
            stepHandler.Register(router);
            undoHandler.Register(router);
            redoHandler.Register(router);
            runTestsHandler.Register(router);
            testResultHandler.Register(router);
            consoleLogsHandler.Register(router);
            consoleClearHandler.Register(router);
            createSceneHandler.Register(router);
            openSceneHandler.Register(router);
            saveHandler.Register(router);
            hierarchyHandler.Register(router);
            getGameObjectsHandler.Register(router);
            createGameObjectHandler.Register(router);
            deleteGameObjectHandler.Register(router);
            modifyGameObjectHandler.Register(router);
            duplicateGameObjectHandler.Register(router);
            addComponentHandler.Register(router);
            removeComponentHandler.Register(router);
            componentPropertiesHandler.Register(router);
            setComponentPropertyHandler.Register(router);
            createPrefabHandler.Register(router);
            instantiatePrefabHandler.Register(router);
            openPrefabHandler.Register(router);
            closePrefabHandler.Register(router);
            createScriptableObjectHandler.Register(router);
            scriptableObjectPropertiesHandler.Register(router);
            setScriptableObjectPropertyHandler.Register(router);
            createAnimationClipHandler.Register(router);
            animationCurvesHandler.Register(router);
            setAnimationCurveHandler.Register(router);
            removeAnimationCurveHandler.Register(router);
            assetRefreshHandler.Register(router);
            selectProjectWindowAssetHandler.Register(router);
            executeMenuItemHandler.Register(router);
            captureGameViewHandler.Register(router);
            captureSceneViewHandler.Register(router);
            focusSceneViewHandler.Register(router);
            focusGameViewHandler.Register(router);
            getGameViewSizeHandler.Register(router);
            getGameViewSizeListHandler.Register(router);
            setGameViewSizeHandler.Register(router);
            getGameViewScaleHandler.Register(router);
            setGameViewScaleHandler.Register(router);
            addMovieRecorderHandler.Register(router);
            getRecorderListHandler.Register(router);
            removeMovieRecorderHandler.Register(router);
            startMovieRecorderHandler.Register(router);
            stopMovieRecorderHandler.Register(router);

            pressKeyHandler.Register(router);
            clickMouseHandler.Register(router);
            moveMouseHandler.Register(router);
            dragMouseHandler.Register(router);
            getUIPointerTargetsHandler.Register(router);
            createTimelineHandler.Register(router);
            addTimelineTrackHandler.Register(router);
            removeTimelineTrackHandler.Register(router);
            bindTimelineTrackHandler.Register(router);
            addTimelineClipHandler.Register(router);
            removeTimelineClipHandler.Register(router);
            getTimelineTracksHandler.Register(router);
            getTimelineTrackPropertiesHandler.Register(router);
            getTimelineClipPropertiesHandler.Register(router);
            modifyTimelineClipHandler.Register(router);
            setTimelineClipPropertyHandler.Register(router);
            setTimelineTrackPropertyHandler.Register(router);
            playTimelineHandler.Register(router);
            stopTimelineHandler.Register(router);
            evaluateTimelineHandler.Register(router);

            var extensionRegistry = new ExtensionRegistry();
            var extensionListHandler = new ExtensionListHandler(extensionRegistry);
            var extensionExecuteHandler = new ExtensionExecuteHandler(extensionRegistry, dispatcher);
            extensionListHandler.Register(router);
            extensionExecuteHandler.Register(router);
        }

        private static int FindFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(
                System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static void OnQuit()
        {
            ServerUrlFile.Delete();
        }

        // Registers the callbacks that record the results of runs started through POST /tests/run.
        // Registered callbacks are discarded by a domain reload, so this runs in every domain, including the one
        // that continues a run after a reload. Must be called only once per domain (it is reached only from the static
        // constructor via StartServer); a second call would register another set of callbacks and record every result twice.
        private static void RegisterTestCallbacks(ITestResultStore testResultStore)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new SessionStoreTestCallbacks(testResultStore));
        }

        private static void Shutdown(CompilationPipelineAdapter compilationPipeline, HttpListenerServer server)
        {
            // Must run before the server stops: it lets a pending POST /editor/domain-reload request respond.
            // Stopping first would cancel the request with a 503, and the client would resend it and reload
            // the domain once more. The server waits for that response to be written before it closes.
            compilationPipeline.NotifyBeforeAssemblyReload();

            server.Stop();
        }
    }
}
