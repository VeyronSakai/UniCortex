using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyPointerTargetOperations : IPointerTargetOperations
    {
        public int GetPointerTargetsCallCount { get; private set; }
        public int GetPointerTargetCallCount { get; private set; }
        public int LastInstanceId { get; private set; }
        public string LastPath { get; private set; }

        public List<PointerTarget> PointerTargetsToReturn { get; set; } = new();

        public PointerTarget PointerTargetToReturn { get; set; } = CreateTarget(0f, 0f, false, "");

        public Exception ExceptionToThrow { get; set; }

        public static PointerTarget CreateTarget(float centerX, float centerY, bool blocked, string blockedBy)
        {
            return new PointerTarget("Button", "Canvas/Button", 100, centerX, centerY,
                new ScreenRect(centerX - 50f, centerY - 20f, 100f, 40f),
                new List<string> { PointerEventName.Click }, true, true, blocked, blockedBy);
        }

        // Returns completed tasks so that tests can block on them (see FakeMainThreadDispatcher).
        public Task<List<PointerTarget>> GetPointerTargetsAsync(CancellationToken cancellationToken)
        {
            GetPointerTargetsCallCount++;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(PointerTargetsToReturn);
        }

        public Task<PointerTarget> GetPointerTargetAsync(int instanceId, string path,
            CancellationToken cancellationToken)
        {
            GetPointerTargetCallCount++;
            LastInstanceId = instanceId;
            LastPath = path;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(PointerTargetToReturn);
        }
    }
}
