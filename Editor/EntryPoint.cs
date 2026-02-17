using EditorBridge.Editor.Infrastructures;
using EditorBridge.Editor.Presentations;
using EditorBridge.Editor.Settings;
using EditorBridge.Editor.UseCases;
using UnityEditor;
using UnityEngine;

namespace EditorBridge.Editor
{
    [InitializeOnLoad]
    internal static class EntryPoint
    {
        private static MainThreadDispatcher s_dispatcher;
        private static HttpListenerServer s_server;

        static EntryPoint()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;

            s_dispatcher = new MainThreadDispatcher();
            EditorApplication.update += s_dispatcher.OnUpdate;

            if (EditorBridgeSettings.instance.AutoStart)
            {
                StartServer();
            }
        }

        private static void StartServer()
        {
            var port = EditorBridgeSettings.instance.Port;
            if (port is < 1 or > 65535)
            {
                Debug.LogError($"[EditorBridge] Invalid port: {port}. Must be between 1 and 65535.");
                return;
            }

            var pingUseCase = new PingUseCase(s_dispatcher);
            var pingHandler = new PingHandler(pingUseCase);

            var router = new RequestRouter();
            pingHandler.Register(router);

            s_server = new HttpListenerServer(router, port);
            s_server.Start();
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
