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
                // Saving an untitled scene opens the save panel, which blocks the Editor, and it has no saved state
                // that its changes could be discarded back to.
                return message + "An untitled scene can be neither saved nor discarded with UniCortex.";
            }

            return message +
                   "Save them with save (CLI: editor save) or discard them with discard_scene_changes " +
                   "(CLI: scene discard), then retry.";
        }
    }
}
