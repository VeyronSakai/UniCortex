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
        public int GetTargetCenterCallCount { get; private set; }
        public int LastInstanceId { get; private set; }

        public List<PointerTarget> PointerTargetsToReturn { get; set; } = new();

        public (float x, float y) TargetCenterToReturn { get; set; }

        // Centers by instanceId. TargetCenterToReturn is used for an instanceId not in it.
        public Dictionary<int, (float x, float y)> TargetCentersToReturn { get; } = new();

        public Exception ExceptionToThrow { get; set; }

        // Returns completed tasks so that tests can block on them (see FakeMainThreadDispatcher).
        public Task<List<PointerTarget>> GetPointerTargetsAsync(CancellationToken cancellationToken)
        {
            GetPointerTargetsCallCount++;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(PointerTargetsToReturn);
        }

        public Task<(float x, float y)> GetTargetCenterAsync(int instanceId,
            CancellationToken cancellationToken)
        {
            GetTargetCenterCallCount++;
            LastInstanceId = instanceId;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(TargetCentersToReturn.TryGetValue(instanceId, out var center)
                ? center
                : TargetCenterToReturn);
        }
    }
}
