using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SwitchPlatformUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IBuildTargetOperations _operations;
        private readonly IEditorApplication _editorApplication;

        public SwitchPlatformUseCase(IMainThreadDispatcher dispatcher, IBuildTargetOperations operations,
            IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _operations = operations;
            _editorApplication = editorApplication;
        }

        public async Task<SwitchPlatformResponse> ExecuteAsync(string buildTarget,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot switch platform during play mode. Exit play mode first.");
                }

                var resolvedBuildTarget = _operations.ResolveBuildTarget(buildTarget);
                if (resolvedBuildTarget == null)
                {
                    throw new ArgumentException(
                        $"Unknown build target: '{buildTarget}'. {FormatSupportedBuildTargets()}");
                }

                if (!_operations.IsBuildTargetSupported(resolvedBuildTarget))
                {
                    throw new ArgumentException(
                        $"Build target '{resolvedBuildTarget}' is not supported by this Editor. " +
                        $"Install its platform module from Unity Hub. {FormatSupportedBuildTargets()}");
                }

                var previousBuildTarget = _operations.GetActiveBuildTarget();

                // Switching to the active build target is a no-op. This also makes a resent request harmless
                // when the response was lost in the domain reload that follows a switch.
                if (previousBuildTarget == resolvedBuildTarget)
                {
                    return new SwitchPlatformResponse(previousBuildTarget, resolvedBuildTarget);
                }

                Debug.Log($"[UniCortex] Switch Platform: {previousBuildTarget} -> {resolvedBuildTarget}");

                // Assets are reimported and scripts are recompiled synchronously here.
                // The domain reload starts after this call returns, and the server writes the response before it stops.
                if (!_operations.SwitchActiveBuildTarget(resolvedBuildTarget))
                {
                    throw new InvalidOperationException(
                        $"Failed to switch platform from '{previousBuildTarget}' to '{resolvedBuildTarget}'.");
                }

                return new SwitchPlatformResponse(previousBuildTarget, _operations.GetActiveBuildTarget());
            }, cancellationToken);
        }

        private string FormatSupportedBuildTargets()
        {
            return $"Supported build targets: {string.Join(", ", _operations.GetSupportedBuildTargets())}";
        }
    }
}
