using UniCortex.Editor.Handlers.Console;
using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Handlers.Tests;
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
            s_server.Start();

            ServerUrlFile.Write(port);
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
