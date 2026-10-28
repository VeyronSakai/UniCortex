using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.AudioMixer;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class AudioMixerHandlersTest
    {
        private const string AssetPath = "Assets/Audio/Main.mixer";

        private SpyAudioMixerOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyAudioMixerOperations();
            var dispatcher = new FakeMainThreadDispatcher();
            _router = new RequestRouter();
            new CreateAudioMixerHandler(new CreateAudioMixerUseCase(dispatcher, _ops)).Register(_router);
            new AudioMixerHandler(new GetAudioMixerUseCase(dispatcher, _ops)).Register(_router);
            new AddAudioMixerGroupHandler(new AddAudioMixerGroupUseCase(dispatcher, _ops)).Register(_router);
            new ExposeAudioMixerParameterHandler(new ExposeAudioMixerParameterUseCase(dispatcher, _ops))
                .Register(_router);
            new SetAudioSourceOutputHandler(new SetAudioSourceOutputUseCase(dispatcher, _ops)).Register(_router);
        }

        private FakeRequestContext Post(string route, string body)
        {
            var context = new FakeRequestContext(HttpMethodType.Post, route, body);
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();
            return context;
        }

        [Test]
        public void Create_Returns200_WhenValid()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerCreate, "{\"assetPath\":\"" + AssetPath + "\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains(AssetPath, context.ResponseBody);
            Assert.AreEqual(AssetPath, _ops.LastCreateAssetPath);
        }

        [Test]
        public void Create_Returns400_WhenBodyEmpty()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerCreate, "");

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.CreateCallCount);
        }

        [Test]
        public void Create_Returns400_WhenAssetPathMissing()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerCreate, "{}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath", context.ResponseBody);
            Assert.AreEqual(0, _ops.CreateCallCount);
        }

        [Test]
        public void Get_Returns200WithGroups_WhenValid()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.AudioMixer);
            context.SetQueryParameter(nameof(GetAudioMixerRequest.assetPath), AssetPath);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"path\":\"Master\"", context.ResponseBody);
            StringAssert.Contains("\"isStartSnapshot\":true", context.ResponseBody);
            Assert.AreEqual(AssetPath, _ops.LastGetAssetPath);
        }

        [Test]
        public void Get_Returns400_WhenAssetPathMissing()
        {
            // Arrange
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.AudioMixer);

            // Act
            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetCallCount);
        }

        [Test]
        public void AddGroup_Returns200_WhenValid()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerAddGroup,
                "{\"assetPath\":\"" + AssetPath + "\",\"groupName\":\"Music\",\"parentGroup\":\"Master\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("Master/Music", context.ResponseBody);
            Assert.AreEqual("Music", _ops.LastAddGroupName);
            Assert.AreEqual("Master", _ops.LastAddGroupParent);
        }

        [Test]
        public void AddGroup_Returns400_WhenGroupNameMissing()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerAddGroup, "{\"assetPath\":\"" + AssetPath + "\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("groupName", context.ResponseBody);
            Assert.AreEqual(0, _ops.AddGroupCallCount);
        }

        [Test]
        public void ExposeParameter_Returns200_WhenValid()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerExposeParameter,
                "{\"assetPath\":\"" + AssetPath +
                "\",\"group\":\"Music\",\"parameter\":\"volume\",\"exposedName\":\"MusicVolume\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("Music", _ops.LastExposeGroup);
            Assert.AreEqual("volume", _ops.LastExposeParameter);
            Assert.AreEqual("MusicVolume", _ops.LastExposeName);
        }

        [Test]
        public void ExposeParameter_Returns400_WhenExposedNameMissing()
        {
            // Act
            var context = Post(ApiRoutes.AudioMixerExposeParameter,
                "{\"assetPath\":\"" + AssetPath + "\",\"group\":\"Music\",\"parameter\":\"volume\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("exposedName", context.ResponseBody);
            Assert.AreEqual(0, _ops.ExposeParameterCallCount);
        }

        [Test]
        public void SetAudioSourceOutput_Returns200_WhenValid()
        {
            // Act
            var context = Post(ApiRoutes.AudioSourceSetOutput,
                "{\"instanceId\":123,\"componentIndex\":1,\"assetPath\":\"" + AssetPath +
                "\",\"group\":\"Master/Music\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(123, _ops.LastSetOutputInstanceId);
            Assert.AreEqual(1, _ops.LastSetOutputComponentIndex);
            Assert.AreEqual("Master/Music", _ops.LastSetOutputGroup);
        }

        [Test]
        public void SetAudioSourceOutput_Returns400_WhenInstanceIdMissing()
        {
            // Act
            var context = Post(ApiRoutes.AudioSourceSetOutput,
                "{\"assetPath\":\"" + AssetPath + "\",\"group\":\"Master\"}");

            // Assert
            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("instanceId", context.ResponseBody);
            Assert.AreEqual(0, _ops.SetOutputCallCount);
        }
    }
}
