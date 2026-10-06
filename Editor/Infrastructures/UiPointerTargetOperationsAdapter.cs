#if UNICORTEX_UGUI
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class UiPointerTargetOperationsAdapter : IUiPointerTargetOperations
    {
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;

        public UiPointerTargetOperationsAdapter(IPlayerLoopDispatcher playerLoopDispatcher)
        {
            _playerLoopDispatcher = playerLoopDispatcher;
        }

        public Task<List<UiPointerTargetEntry>> GetUiPointerTargetsAsync(CancellationToken cancellationToken)
        {
            return _playerLoopDispatcher.RunAsync(GetUiPointerTargets, cancellationToken);
        }

        public Task<(float x, float y)> GetTargetCenterAsync(int instanceId,
            CancellationToken cancellationToken)
        {
            return _playerLoopDispatcher.RunAsync(() => GetTargetCenter(instanceId), cancellationToken);
        }

        private static List<UiPointerTargetEntry> GetUiPointerTargets()
        {
            return UguiPointerTargetFinder.Find()
                .Select(target => new UiPointerTargetEntry(target.Path, target.GameObject.GetInstanceID(),
                    new ScreenRect(target.ScreenRect.x, target.ScreenRect.y, target.ScreenRect.width,
                        target.ScreenRect.height)))
                .ToList();
        }

        private static (float x, float y) GetTargetCenter(int instanceId)
        {
            var center = UguiPointerTargetFinder.GetCenter(FindByInstanceId(instanceId));
            return (center.x, center.y);
        }

        private static GameObject FindByInstanceId(int instanceId)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var gameObject = obj as GameObject;
            if (gameObject == null && obj is Component component)
            {
                gameObject = component.gameObject;
            }

            if (gameObject == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            return gameObject;
        }
    }
}
#endif
