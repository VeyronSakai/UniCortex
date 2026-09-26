using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.AnimationClip;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class CreateAnimationClipHandlerTest
    {
        private SpyAnimationClipOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyAnimationClipOperations();
            var handler = new CreateAnimationClipHandler(
                new CreateAnimationClipUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var request = new CreateAnimationClipRequest
                { assetPath = "Assets/Animations/FadeIn.anim", loop = true, frameRate = 30f };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipCreate,
                JsonUtility.ToJson(request));

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("Assets/Animations/FadeIn.anim", context.ResponseBody);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", _ops.LastCreateAssetPath);
            Assert.IsTrue(_ops.LastCreateLoop);
            Assert.AreEqual(30f, _ops.LastCreateFrameRate);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipCreate, "");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.CreateCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenAssetPathMissing()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipCreate,
                "{\"loop\":true}");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("assetPath", context.ResponseBody);
            Assert.AreEqual(0, _ops.CreateCallCount);
        }
    }
}
