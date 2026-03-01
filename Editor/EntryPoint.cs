using UniCortex.Editor.Handlers.Asset;
using UniCortex.Editor.Handlers.Component;
using UniCortex.Editor.Handlers.Console;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Handlers.GameObject;
using UniCortex.Editor.Handlers.Prefab;
using UniCortex.Editor.Handlers.Scene;
using UniCortex.Editor.Handlers.Tests;
using UniCortex.Editor.Handlers.Utility;
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
        private static MainThreadDispatcher s_dispatcher;
        private static HttpListenerServer s_server;

        static EntryPoint()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += OnQuit;

            s_dispatcher = new MainThreadDispatcher();
            EditorApplication.update += s_dispatcher.OnUpdate;

            ReregisterTestCallbacksIfNeeded();

            if (UniCortexSettings.instance.AutoStart)
            {
                StartServer();
            }
        }

        private static void StartServer()
        {
            var port = SessionState.GetInt("UniCortex.Port", 0);
            if (port == 0)
            {
                port = FindFreePort();
                SessionState.SetInt("UniCortex.Port", port);
            }

            var router = new RequestRouter();

            RegisterHandlers(router);

            s_server = new HttpListenerServer(router, port);
            try
            {
                s_server.Start();
                ServerUrlFile.Write(port);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UniCortex] Failed to start server on port {port}: {ex.Message}");
            }
        }

        private static void RegisterHandlers(RequestRouter router)
        {
            var editorApplication = new EditorApplicationAdapter();
            var compilationPipeline = new CompilationPipelineAdapter();

            var pingUseCase = new PingUseCase(s_dispatcher);
            var pingHandler = new PingHandler(pingUseCase);

            var playUseCase = new PlayUseCase(s_dispatcher, editorApplication);
            var playHandler = new PlayHandler(playUseCase);

            var stopUseCase = new StopUseCase(s_dispatcher, editorApplication);
            var stopHandler = new StopHandler(stopUseCase);

            var requestDomainReloadUseCase = new RequestDomainReloadUseCase(s_dispatcher, compilationPipeline);
            var requestDomainReloadHandler = new DomainReloadHandler(requestDomainReloadUseCase);

            var getEditorStatusUseCase = new GetEditorStatusUseCase(s_dispatcher, editorApplication);
            var editorStatusHandler = new EditorStatusHandler(getEditorStatusUseCase);

            var undoAdapter = new UndoAdapter();

            var undoUseCase = new UndoUseCase(s_dispatcher, undoAdapter);
            var undoHandler = new UndoHandler(undoUseCase);

            var redoUseCase = new RedoUseCase(s_dispatcher, undoAdapter);
            var redoHandler = new RedoHandler(redoUseCase);

            var testRunnerAdapter = new TestRunnerAdapter(s_dispatcher);
            var runTestsUseCase = new RunTestsUseCase(testRunnerAdapter);
            var runTestsHandler = new RunTestsHandler(runTestsUseCase);
            var testResultHandler = new TestResultHandler();

            var consoleLogCollector = new ConsoleLogCollector();

            var getConsoleLogsUseCase = new GetConsoleLogsUseCase(s_dispatcher, consoleLogCollector);
            var consoleLogsHandler = new ConsoleLogsHandler(getConsoleLogsUseCase);

            var clearConsoleLogsUseCase = new ClearConsoleLogsUseCase(s_dispatcher, consoleLogCollector);
            var consoleClearHandler = new ConsoleClearHandler(clearConsoleLogsUseCase);

            var sceneManagerAdapter = new EditorSceneManagerAdapter();

            var openSceneUseCase = new OpenSceneUseCase(s_dispatcher, sceneManagerAdapter);
            var openSceneHandler = new OpenSceneHandler(openSceneUseCase);

            var saveSceneUseCase = new SaveSceneUseCase(s_dispatcher, sceneManagerAdapter);
            var saveSceneHandler = new SaveSceneHandler(saveSceneUseCase);

            var getSceneHierarchyUseCase = new GetSceneHierarchyUseCase(s_dispatcher, sceneManagerAdapter);
            var sceneHierarchyHandler = new SceneHierarchyHandler(getSceneHierarchyUseCase);

            var gameObjectOps = new GameObjectOperationsAdapter();

            var getGameObjectsUseCase = new GetGameObjectsUseCase(s_dispatcher, gameObjectOps);
            var getGameObjectsHandler = new GetGameObjectsHandler(getGameObjectsUseCase);

            var createGameObjectUseCase = new CreateGameObjectUseCase(s_dispatcher, gameObjectOps);
            var createGameObjectHandler = new CreateGameObjectHandler(createGameObjectUseCase);

            var deleteGameObjectUseCase = new DeleteGameObjectUseCase(s_dispatcher, gameObjectOps);
            var deleteGameObjectHandler = new DeleteGameObjectHandler(deleteGameObjectUseCase);

            var modifyGameObjectUseCase = new ModifyGameObjectUseCase(s_dispatcher, gameObjectOps);
            var modifyGameObjectHandler = new ModifyGameObjectHandler(modifyGameObjectUseCase);

            var componentOps = new ComponentOperationsAdapter();

            var addComponentUseCase = new AddComponentUseCase(s_dispatcher, componentOps);
            var addComponentHandler = new AddComponentHandler(addComponentUseCase);

            var removeComponentUseCase = new RemoveComponentUseCase(s_dispatcher, componentOps);
            var removeComponentHandler = new RemoveComponentHandler(removeComponentUseCase);

            var getComponentPropertiesUseCase = new GetComponentPropertiesUseCase(s_dispatcher, componentOps);
            var componentPropertiesHandler = new ComponentPropertiesHandler(getComponentPropertiesUseCase);

            var setComponentPropertyUseCase = new SetComponentPropertyUseCase(s_dispatcher, componentOps);
            var setComponentPropertyHandler = new SetComponentPropertyHandler(setComponentPropertyUseCase);

            var prefabOps = new PrefabOperationsAdapter();

            var createPrefabUseCase = new CreatePrefabUseCase(s_dispatcher, prefabOps);
            var createPrefabHandler = new CreatePrefabHandler(createPrefabUseCase);

            var instantiatePrefabUseCase = new InstantiatePrefabUseCase(s_dispatcher, prefabOps);
            var instantiatePrefabHandler = new InstantiatePrefabHandler(instantiatePrefabUseCase);

            var assetDbOps = new AssetDatabaseOperationsAdapter();

            var refreshAssetDatabaseUseCase = new RefreshAssetDatabaseUseCase(s_dispatcher, assetDbOps);
            var assetRefreshHandler = new AssetRefreshHandler(refreshAssetDatabaseUseCase);


            var utilityOps = new UtilityOperationsAdapter();

            var executeMenuItemUseCase = new ExecuteMenuItemUseCase(s_dispatcher, utilityOps);
            var executeMenuItemHandler = new ExecuteMenuItemHandler(executeMenuItemUseCase);

            var captureScreenshotUseCase = new CaptureScreenshotUseCase(s_dispatcher, utilityOps);
            var screenshotHandler = new ScreenshotHandler(captureScreenshotUseCase);

            pingHandler.Register(router);
            playHandler.Register(router);
            stopHandler.Register(router);
            requestDomainReloadHandler.Register(router);
            editorStatusHandler.Register(router);
            undoHandler.Register(router);
            redoHandler.Register(router);
            runTestsHandler.Register(router);
            testResultHandler.Register(router);
            consoleLogsHandler.Register(router);
            consoleClearHandler.Register(router);
            openSceneHandler.Register(router);
            saveSceneHandler.Register(router);
            sceneHierarchyHandler.Register(router);
            getGameObjectsHandler.Register(router);
            createGameObjectHandler.Register(router);
            deleteGameObjectHandler.Register(router);
            modifyGameObjectHandler.Register(router);
            addComponentHandler.Register(router);
            removeComponentHandler.Register(router);
            componentPropertiesHandler.Register(router);
            setComponentPropertyHandler.Register(router);
            createPrefabHandler.Register(router);
            instantiatePrefabHandler.Register(router);
            assetRefreshHandler.Register(router);
            executeMenuItemHandler.Register(router);
            screenshotHandler.Register(router);
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

        private static void ReregisterTestCallbacksIfNeeded()
        {
            if (!TestResultStore.IsPending)
            {
                return;
            }

            // After a domain reload the TaskCompletionSource used by TestRunnerAdapter
            // no longer exists, so there is no way to complete the HTTP response.
            // Register SessionStoreTestCallbacks directly (without the TestCallbacks wrapper)
            // so that test results are still persisted to SessionState via TestResultStore.
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new SessionStoreTestCallbacks());
            Debug.Log("[UniCortex] Re-registered test callbacks after domain reload");
        }

        private static void Shutdown()
        {
            s_server?.Stop();
            s_server = null;

            if (s_dispatcher != null)
            {
                EditorApplication.update -= s_dispatcher.OnUpdate;
                s_dispatcher = null;
            }
        }
    }
}
