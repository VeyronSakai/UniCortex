using UnityEditor;

namespace UniCortex.Editor.Settings
{
    internal static class UniCortexSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() =>
            new("Project/UniCortex", SettingsScope.Project)
            {
                label = "UniCortex",
                guiHandler = _ =>
                {
                    var settings = UniCortexSettings.instance;

                    EditorGUI.BeginChangeCheck();
                    settings.AutoStart = EditorGUILayout.Toggle("Auto Start", settings.AutoStart);
                    if (EditorGUI.EndChangeCheck())
                        settings.Save();

                    var port = SessionState.GetInt("UniCortex.Port", 0);
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.TextField("Current Port", port == 0 ? "-" : port.ToString());
                }
            };
    }
}
