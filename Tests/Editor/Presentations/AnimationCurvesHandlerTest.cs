using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.AnimationClip;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class AnimationCurvesHandlerTest
    {
        private SpyAnimationClipOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyAnimationClipOperations();
            var handler = new AnimationCurvesHandler(
                new GetAnimationCurvesUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200WithCurves_WhenValid()
        {
            _ops.GetCurvesResult = new GetAnimationCurvesResponse(60f, false, 1f, new List<AnimationCurveEntry>
            {
                new AnimationCurveEntry("Root", "UnityEngine.Transform", "UnityEngine.CoreModule",
                    "m_LocalScale.x", new List<AnimationCurveKeyEntry>
                    {
                        new AnimationCurveKeyEntry(0f, 1f, 0f, 0f, "Free", "Free")
                    })
            });
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.AnimationClipCurves);
            context.SetQueryParameter(nameof(GetAnimationCurvesRequest.assetPath), "Assets/Animations/FadeIn.anim");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("m_LocalScale.x", context.ResponseBody);
            Assert.AreEqual("Assets/Animations/FadeIn.anim", _ops.LastGetCurvesAssetPath);
        }

        [Test]
        public void Handle_Returns400_WhenAssetPathMissing()
        {
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.AnimationClipCurves);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.GetCurvesCallCount);
        }
    }
}
