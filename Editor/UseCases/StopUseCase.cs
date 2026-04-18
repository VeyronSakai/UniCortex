using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.UseCases
{
    internal sealed class StopUseCase
    {
        private static readonly Type s_playmodeTestsControllerType =
            Type.GetType("UnityEngine.TestTools.TestRunner.PlaymodeTestsController, UnityEngine.TestRunner");

        private static readonly MethodInfo s_isControllerOnSceneMethod =
            s_playmodeTestsControllerType?.GetMethod("IsControllerOnScene",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly MethodInfo s_getControllerMethod =
            s_playmodeTestsControllerType?.GetMethod("GetController",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorApplication _editorApplication;

        public StopUseCase(IMainThreadDispatcher dispatcher, IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _editorApplication = editorApplication;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                // Unity Test Framework's BackgroundWatcher looks up the PlayMode test runner
                // object by name during ExitingPlayMode. If a stale runner is present, make it
                // inactive so normal play mode stops do not trip its cleanup path.
                var hasController = (bool?)s_isControllerOnSceneMethod?.Invoke(null, null) == true;
                if (hasController)
                {
                    if (s_getControllerMethod?.Invoke(null, null) is Component controller)
                    {
                        controller.gameObject.SetActive(false);
                    }
                }

                _editorApplication.IsPlaying = false;
                Debug.Log("[UniCortex] Stop");
            }, cancellationToken);
        }
    }
}
