using UniCortex.Editor.Handlers.Editor;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Settings;
using UniCortex.Editor.UseCases;
using UnityEditor;

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

            var pingUseCase = new PingUseCase(s_dispatcher);
            var pingHandler = new PingHandler(pingUseCase);

            var playUseCase = new PlayUseCase(s_dispatcher);
            var playHandler = new PlayHandler(playUseCase);

            var stopUseCase = new StopUseCase(s_dispatcher);
            var stopHandler = new StopHandler(stopUseCase);

            var pauseUseCase = new PauseUseCase(s_dispatcher);
            var pauseHandler = new PauseHandler(pauseUseCase);

            var unpauseUseCase = new UnpauseUseCase(s_dispatcher);
            var unpauseHandler = new UnpauseHandler(unpauseUseCase);

            var requestDomainReloadUseCase = new RequestDomainReloadUseCase(s_dispatcher);
            var requestDomainReloadHandler = new DomainReloadHandler(requestDomainReloadUseCase);

            var getEditorStatusUseCase = new GetEditorStatusUseCase(s_dispatcher);
            var editorStatusHandler = new EditorStatusHandler(getEditorStatusUseCase);

            var router = new RequestRouter();
            pingHandler.Register(router);
            playHandler.Register(router);
            stopHandler.Register(router);
            pauseHandler.Register(router);
            unpauseHandler.Register(router);
            requestDomainReloadHandler.Register(router);
            editorStatusHandler.Register(router);

            s_server = new HttpListenerServer(router, port);
            s_server.Start();

            ServerUrlFile.Write(port);
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
