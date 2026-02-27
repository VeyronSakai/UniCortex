using System;
using System.Collections.Generic;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class ConsoleLogCollector : IConsoleLogCollector
    {
        private static readonly Type s_logEntriesType =
            Type.GetType("UnityEditor.LogEntries, UnityEditor");

        private static readonly Type s_logEntryType =
            Type.GetType("UnityEditor.LogEntry, UnityEditor");

        private static readonly MethodInfo s_startMethod =
            s_logEntriesType?.GetMethod("StartGettingEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly MethodInfo s_endMethod =
            s_logEntriesType?.GetMethod("EndGettingEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly MethodInfo s_getEntryMethod =
            s_logEntriesType?.GetMethod("GetEntryInternal",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly MethodInfo s_clearMethod =
            s_logEntriesType?.GetMethod("Clear",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo s_messageField =
            s_logEntryType?.GetField("message");

        private static readonly FieldInfo s_modeField =
            s_logEntryType?.GetField("mode");

        private static readonly FieldInfo s_callstackStartField =
            s_logEntryType?.GetField("callstackTextStartUTF16");

        // LogMessageFlags bit masks
        private const int Error = 1 << 0;
        private const int Assert = 1 << 1;
        private const int AssetImportError = 1 << 6;
        private const int AssetImportWarning = 1 << 7;
        private const int ScriptingError = 1 << 8;
        private const int ScriptingWarning = 1 << 9;
        private const int ScriptCompileError = 1 << 11;
        private const int ScriptCompileWarning = 1 << 12;
        private const int ScriptingException = 1 << 17;
        private const int ScriptingAssertion = 1 << 21;

        private const int ErrorMask = Error | Assert | ScriptingError | ScriptCompileError |
                                      ScriptingException | ScriptingAssertion | AssetImportError;

        private const int WarningMask = ScriptingWarning | ScriptCompileWarning | AssetImportWarning;

        public List<ConsoleLogEntry> GetLogs(int count, bool includeStackTrace = false,
            bool showLog = true, bool showWarning = true, bool showError = true)
        {
            if (count <= 0 || s_startMethod == null)
            {
                return new List<ConsoleLogEntry>();
            }

            var totalRows = (int)s_startMethod.Invoke(null, null);
            try
            {
                if (totalRows == 0)
                {
                    return new List<ConsoleLogEntry>();
                }

                var result = new List<ConsoleLogEntry>();
                var entry = Activator.CreateInstance(s_logEntryType);

                // Iterate from the end to collect the most recent entries first
                for (var i = totalRows - 1; i >= 0 && result.Count < count; i--)
                {
                    var success = (bool)s_getEntryMethod.Invoke(null, new[] { i, entry });
                    if (!success)
                    {
                        continue;
                    }

                    var mode = (int)s_modeField.GetValue(entry);
                    var type = MapMode(mode);

                    if (!ShouldInclude(type, showLog, showWarning, showError))
                    {
                        continue;
                    }

                    var message = (string)s_messageField.GetValue(entry);
                    var callstackStart = (int)s_callstackStartField.GetValue(entry);

                    var logMessage = callstackStart > 0 && callstackStart < message.Length
                        ? message[..callstackStart]
                        : message;

                    var stackTrace = includeStackTrace && callstackStart > 0 && callstackStart < message.Length
                        ? message[callstackStart..]
                        : string.Empty;

                    result.Add(new ConsoleLogEntry(logMessage, stackTrace, type, ""));
                }

                // Reverse to restore chronological order
                result.Reverse();
                return result;
            }
            finally
            {
                s_endMethod.Invoke(null, null);
            }
        }

        private static bool ShouldInclude(string type, bool showLog, bool showWarning, bool showError)
        {
            return type switch
            {
                "Log" => showLog,
                "Warning" => showWarning,
                "Error" => showError,
                _ => true
            };
        }

        public void Clear()
        {
            s_clearMethod?.Invoke(null, null);
        }

        private static string MapMode(int mode)
        {
            if ((mode & ErrorMask) != 0)
            {
                return "Error";
            }

            if ((mode & WarningMask) != 0)
            {
                return "Warning";
            }

            return "Log";
        }
    }
}
