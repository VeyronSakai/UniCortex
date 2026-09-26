using System.Collections.Generic;
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
    internal sealed class SetAnimationCurveHandlerTest
    {
        private SpyAnimationClipOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyAnimationClipOperations();
            var handler = new SetAnimationCurveHandler(
                new SetAnimationCurveUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        private static SetAnimationCurveRequest CreateValidRequest()
        {
            return new SetAnimationCurveRequest
            {
                assetPath = "Assets/Animations/FadeIn.anim",
                path = "Root/Child",
                componentType = "UnityEngine.UI.Image",
                assemblyName = "UnityEngine.UI",
                propertyName = "m_Color.a",
                keys = new List<AnimationCurveKeyInput>
                {
                    new AnimationCurveKeyInput { time = 0f, value = 0f },
                    new AnimationCurveKeyInput { time = 0.5f, value = 1f, tangentMode = "Linear" }
                }
            };
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipSetCurve,
                JsonUtility.ToJson(CreateValidRequest()));

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("true", context.ResponseBody);
            Assert.AreEqual(1, _ops.SetCurveCallCount);
            Assert.AreEqual("Root/Child", _ops.LastSetCurvePath);
            Assert.AreEqual("m_Color.a", _ops.LastSetCurvePropertyName);
            Assert.AreEqual(2, _ops.LastSetCurveKeys.Count);
            Assert.AreEqual(0.5f, _ops.LastSetCurveKeys[1].time);
            Assert.AreEqual("Linear", _ops.LastSetCurveKeys[1].tangentMode);
        }

        [Test]
        public void Handle_Returns400_WhenBodyEmpty()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipSetCurve, "");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.AreEqual(0, _ops.SetCurveCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenPropertyNameMissing()
        {
            var request = CreateValidRequest();
            request.propertyName = "";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipSetCurve,
                JsonUtility.ToJson(request));

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("propertyName", context.ResponseBody);
            Assert.AreEqual(0, _ops.SetCurveCallCount);
        }

        [Test]
        public void Handle_Returns400_WhenKeysEmpty()
        {
            var request = CreateValidRequest();
            request.keys = new List<AnimationCurveKeyInput>();
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipSetCurve,
                JsonUtility.ToJson(request));

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("keys", context.ResponseBody);
            Assert.AreEqual(0, _ops.SetCurveCallCount);
        }
    }
}
