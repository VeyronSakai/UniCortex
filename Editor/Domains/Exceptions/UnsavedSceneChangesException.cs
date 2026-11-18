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
                // Saving an untitled scene with File/Save opens the save panel, which blocks the Editor, and it has
                // no saved state that its changes could be discarded back to. Any other dirty scene is reported
                // again with the save / discard hint below when the caller retries.
                return message +
                       "Save the untitled scene to an asset path with save_scene_as (CLI: scene save-as), then " +
                       "retry. save would open the save panel, which blocks the Editor.";
            }

            return message +
                   "Save them with save (CLI: editor save) or discard them with discard_scene_changes " +
                   "(CLI: scene discard), then retry.";
        }
    }
}
