using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    [InitializeOnLoad]
    internal static class TestCallbackRegistrar
    {
        static TestCallbackRegistrar()
        {
            if (!TestResultStore.IsPending)
            {
                return;
            }

            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new SessionStoreTestCallbacks());
            Debug.Log("[UniCortex] Re-registered test callbacks after domain reload");
        }
    }
}
