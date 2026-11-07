using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class AudioMixerUseCasesTest
    {
        private const string AssetPath = "Assets/Audio/Main.mixer";

        private FakeMainThreadDispatcher _dispatcher;
        private SpyAudioMixerOperations _ops;

        [SetUp]
        public void SetUp()
        {
            _dispatcher = new FakeMainThreadDispatcher();
            _ops = new SpyAudioMixerOperations();
        }

        [Test]
        public void CreateAudioMixer_CallsCreate_And_DispatchesToMainThread()
        {
            // Arrange
            var useCase = new CreateAudioMixerUseCase(_dispatcher, _ops);

            // Act
            var result = useCase.ExecuteAsync(AssetPath, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, _ops.CreateCallCount);
            Assert.AreEqual(AssetPath, _ops.LastCreateAssetPath);
            Assert.AreEqual(1, _dispatcher.CallCount);
            Assert.IsTrue(result.success);
        }

        [Test]
        public void GetAudioMixer_CallsGet_And_DispatchesToMainThread()
        {
            // Arrange
            var useCase = new GetAudioMixerUseCase(_dispatcher, _ops);

            // Act
            var result = useCase.ExecuteAsync(AssetPath, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, _ops.GetCallCount);
            Assert.AreEqual(AssetPath, _ops.LastGetAssetPath);
            Assert.AreEqual(1, _dispatcher.CallCount);
            Assert.AreSame(_ops.GetResult, result);
        }

        [Test]
        public void AddAudioMixerGroup_CallsAddGroup_And_DispatchesToMainThread()
        {
            // Arrange
            var useCase = new AddAudioMixerGroupUseCase(_dispatcher, _ops);

            // Act
            var result = useCase.ExecuteAsync(AssetPath, "Music", "Master", CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, _ops.AddGroupCallCount);
            Assert.AreEqual(AssetPath, _ops.LastAddGroupAssetPath);
            Assert.AreEqual("Music", _ops.LastAddGroupName);
            Assert.AreEqual("Master", _ops.LastAddGroupParent);
            Assert.AreEqual(1, _dispatcher.CallCount);
            Assert.AreEqual("Master/Music", result.path);
        }

        [Test]
        public void ExposeAudioMixerParameter_CallsExposeParameter_And_DispatchesToMainThread()
        {
            // Arrange
            var useCase = new ExposeAudioMixerParameterUseCase(_dispatcher, _ops);

            // Act
            useCase.ExecuteAsync(AssetPath, "Music", "volume", "MusicVolume", CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, _ops.ExposeParameterCallCount);
            Assert.AreEqual(AssetPath, _ops.LastExposeAssetPath);
            Assert.AreEqual("Music", _ops.LastExposeGroup);
            Assert.AreEqual("volume", _ops.LastExposeParameter);
            Assert.AreEqual("MusicVolume", _ops.LastExposeName);
            Assert.AreEqual(1, _dispatcher.CallCount);
        }

        [Test]
        public void SetAudioSourceOutput_CallsSetAudioSourceOutput_And_DispatchesToMainThread()
        {
            // Arrange
            var useCase = new SetAudioSourceOutputUseCase(_dispatcher, _ops);

            // Act
            useCase.ExecuteAsync(123, 1, AssetPath, "Master/Music", CancellationToken.None)
                .GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, _ops.SetOutputCallCount);
            Assert.AreEqual(123, _ops.LastSetOutputInstanceId);
            Assert.AreEqual(1, _ops.LastSetOutputComponentIndex);
            Assert.AreEqual(AssetPath, _ops.LastSetOutputAssetPath);
            Assert.AreEqual("Master/Music", _ops.LastSetOutputGroup);
            Assert.AreEqual(1, _dispatcher.CallCount);
        }
    }
}
