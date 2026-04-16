using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyEditorWindowOperations : IEditorWindowOperations
    {
        public int FocusSceneViewCallCount { get; private set; }
        public int GetSceneViewCameraCallCount { get; private set; }
        public int SetSceneViewCameraCallCount { get; private set; }
        public int FocusGameViewCallCount { get; private set; }
        public int GetGameViewSizeCallCount { get; private set; }
        public int GetGameViewSizeListCallCount { get; private set; }
        public int SetGameViewSizeCallCount { get; private set; }
        public int GameViewWidth { get; set; } = 800;
        public int GameViewHeight { get; set; } = 600;
        public int LastSetIndex { get; private set; } = -1;
        public SetSceneViewCameraRequest? LastSceneViewCameraRequest { get; private set; }
        public GetSceneViewCameraResponse SceneViewCameraResponse { get; set; } =
            new(
                new Vector3Data { x = 0f, y = 1f, z = -10f },
                new QuaternionData { x = 0f, y = 0f, z = 0f, w = 1f },
                8f,
                false);

        public GameViewSizeEntry[] SizeListEntries { get; set; } = new[]
        {
            new GameViewSizeEntry { index = 0, name = "Free Aspect", width = 0, height = 0, sizeType = "AspectRatio" },
            new GameViewSizeEntry { index = 1, name = "1920x1080", width = 1920, height = 1080, sizeType = "FixedResolution" }
        };

        public int SelectedSizeIndex { get; set; }

        public void FocusSceneView()
        {
            FocusSceneViewCallCount++;
        }

        public GetSceneViewCameraResponse GetSceneViewCamera()
        {
            GetSceneViewCameraCallCount++;
            return SceneViewCameraResponse;
        }

        public void SetSceneViewCamera(SetSceneViewCameraRequest request)
        {
            SetSceneViewCameraCallCount++;
            LastSceneViewCameraRequest = request;
        }

        public void FocusGameView()
        {
            FocusGameViewCallCount++;
        }

        public (int width, int height) GetGameViewSize()
        {
            GetGameViewSizeCallCount++;
            return (GameViewWidth, GameViewHeight);
        }

        public GetGameViewSizeListResponse GetGameViewSizeList()
        {
            GetGameViewSizeListCallCount++;
            return new GetGameViewSizeListResponse
            {
                sizes = SizeListEntries,
                selectedIndex = SelectedSizeIndex
            };
        }

        public void SetGameViewSize(int index)
        {
            SetGameViewSizeCallCount++;
            LastSetIndex = index;
        }
    }
}
