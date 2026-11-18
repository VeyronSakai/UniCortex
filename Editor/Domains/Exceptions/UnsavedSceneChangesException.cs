using System;
using System.Collections.Generic;
using System.Linq;

namespace UniCortex.Editor.Domains.Exceptions
{
    // Thrown when an operation would have to save or drop unsaved scene changes.
    // UniCortex never saves scenes on its own, so the caller has to save or discard them first.
    internal sealed class UnsavedSceneChangesException : InvalidOperationException
    {
        public UnsavedSceneChangesException(string operation, IReadOnlyList<string> dirtyScenePaths)
            : base(BuildMessage(operation, dirtyScenePaths))
        {
        }

        private static string BuildMessage(string operation, IReadOnlyList<string> dirtyScenePaths)
        {
            var hasUntitledScene = dirtyScenePaths.Any(string.IsNullOrEmpty);
            var sceneNames = string.Join(", ",
                dirtyScenePaths.Select(path => string.IsNullOrEmpty(path) ? "Untitled" : path));
            var message = $"{operation} while these scenes have unsaved changes: {sceneNames}. ";

            if (hasUntitledScene)
            {
                // Saving an untitled scene opens the save panel, which blocks the Editor.
                return message +
                       "An untitled scene cannot be saved without the save panel, so discard the changes with " +
                       "discard_scene_changes (CLI: scene discard), or ask the user to save the scene, then retry.";
            }

            return message +
                   "Save them with save (CLI: editor save) or discard them with discard_scene_changes " +
                   "(CLI: scene discard), then retry.";
        }
    }
}
