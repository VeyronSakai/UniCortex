using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal static class TestResultStore
    {
        private const string PendingKey = "UniCortex.TestRunPending";
        private const string ResultJsonKey = "UniCortex.TestResultJson";

        internal static bool IsPending => SessionState.GetBool(PendingKey, false);

        internal static void MarkPending()
        {
            SessionState.SetBool(PendingKey, true);
            SessionState.SetString(ResultJsonKey, "");
        }

        internal static void StoreResult(string json)
        {
            SessionState.SetString(ResultJsonKey, json);
            SessionState.SetBool(PendingKey, false);
        }

        internal static string GetResult()
        {
            return SessionState.GetString(ResultJsonKey, "");
        }
    }
}
