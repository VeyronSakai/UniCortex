using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetSimulatorDeviceUseCase
    {
        // Value of an omitted index or rotation, which keeps the current one.
        public const int Unchanged = -1;

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayModeViewOperations _operations;

        public SetSimulatorDeviceUseCase(IMainThreadDispatcher dispatcher, IPlayModeViewOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<SetSimulatorDeviceResponse> ExecuteAsync(int index, int rotation,
            CancellationToken cancellationToken)
        {
            if (index == Unchanged && rotation == Unchanged)
            {
                throw new ArgumentException("Specify index, rotation, or both.");
            }

            if (index != Unchanged && index < 0)
            {
                throw new ArgumentException($"Index must be 0 or greater, but was {index}.");
            }

            if (rotation != Unchanged && rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
            {
                throw new ArgumentException($"Rotation must be 0, 90, 180 or 270, but was {rotation}.");
            }

            var (deviceName, currentRotation) = await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (index != Unchanged)
                {
                    _operations.SetSimulatorDevice(index);
                }

                if (rotation != Unchanged)
                {
                    _operations.SetSimulatorRotation(rotation);
                }

                return _operations.GetSimulatorDevice();
            }, cancellationToken);
            return new SetSimulatorDeviceResponse(deviceName, currentRotation);
        }
    }
}
