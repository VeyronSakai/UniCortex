using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class AddGameViewSizeUseCase
    {
        // Unity clamps a fixed resolution to this range and cuts a longer name,
        // so such values are rejected instead of being changed silently.
        public const int MinResolution = 10;
        public const int MaxResolution = 99999;
        public const int MaxNameLength = 40;

        private const string FixedResolutionType = "FixedResolution";

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public AddGameViewSizeUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<AddGameViewSizeResponse> ExecuteAsync(string name, int width, int height,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty.");
            }

            if (name.Length > MaxNameLength)
            {
                throw new ArgumentException(
                    $"Name must be {MaxNameLength} characters or fewer, but was {name.Length} characters.");
            }

            ValidateResolution("Width", width);
            ValidateResolution("Height", height);

            return await _dispatcher.RunOnMainThreadAsync(() =>
            {
                var existingIndex = FindFixedResolution(_operations.GetGameViewSizes(), name, width, height);
                if (existingIndex >= 0)
                {
                    return new AddGameViewSizeResponse(existingIndex, false);
                }

                var index = _operations.AddGameViewSize(name, width, height);
                return new AddGameViewSizeResponse(index, true);
            }, cancellationToken);
        }

        private static void ValidateResolution(string label, int value)
        {
            if (value < MinResolution || value > MaxResolution)
            {
                throw new ArgumentException(
                    $"{label} must be between {MinResolution} and {MaxResolution}, but was {value}.");
            }
        }

        private static int FindFixedResolution(GameViewSizeEntry[] sizes, string name, int width, int height)
        {
            foreach (var size in sizes)
            {
                if (size.sizeType == FixedResolutionType && size.name == name &&
                    size.width == width && size.height == height)
                {
                    return size.index;
                }
            }

            return -1;
        }
    }
}
