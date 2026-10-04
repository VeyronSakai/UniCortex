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

        public (float x, float y, bool blocked) TargetCenterToReturn { get; set; }

        public Exception ExceptionToThrow { get; set; }

        // Returns completed tasks so that tests can block on them (see FakeMainThreadDispatcher).
        public Task<List<PointerTarget>> GetPointerTargetsAsync(CancellationToken cancellationToken)
        {
            GetPointerTargetsCallCount++;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(PointerTargetsToReturn);
        }

        public Task<(float x, float y, bool blocked)> GetTargetCenterAsync(int instanceId,
            CancellationToken cancellationToken)
        {
            GetTargetCenterCallCount++;
            LastInstanceId = instanceId;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(TargetCenterToReturn);
        }
    }
}
