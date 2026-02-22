using System.IO;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Settings
{
    internal static class ServerUrlFile
    {
        private static string FilePath =>
            Path.Combine(
                Path.GetDirectoryName(Application.dataPath)!,
                "Library", "UniCortex", "config.json");

        internal static void Write(int port)
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            var json = JsonUtility.ToJson(new UnityServerConfig($"http://localhost:{port}"));
            File.WriteAllText(FilePath, json);
        }

        internal static void Delete()
        {
            try { File.Delete(FilePath); } catch { /* 無視 */ }
        }
    }
}
