using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    // Fallback used when the uGUI package (com.unity.ugui) is not installed.
    internal sealed class PointerTargetNotSupportedAdapter : IPointerTargetOperations
    {
        public Task<List<PointerTarget>> GetPointerTargetsAsync(CancellationToken cancellationToken)
        {
            throw CreateException();
        }

        public Task<PointerTarget> GetPointerTargetAsync(int instanceId, string path,
            CancellationToken cancellationToken)
        {
            throw CreateException();
        }

        private static NotSupportedException CreateException()
        {
            return new NotSupportedException(
                "uGUI package (com.unity.ugui) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
    }
}
