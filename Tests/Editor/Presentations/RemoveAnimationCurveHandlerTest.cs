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
    internal sealed class RemoveAnimationCurveHandlerTest
    {
        private SpyAnimationClipOperations _ops;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _ops = new SpyAnimationClipOperations();
            var handler = new RemoveAnimationCurveHandler(
                new RemoveAnimationCurveUseCase(new FakeMainThreadDispatcher(), _ops));
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void Handle_Returns200_WhenValid()
        {
            var request = new RemoveAnimationCurveRequest
            {
                assetPath = "Assets/Animations/FadeIn.anim",
                animatorRelativePath = "",
                componentType = "UnityEngine.Transform",
                assemblyName = "UnityEngine.CoreModule",
                propertyName = "m_LocalScale.x"
            };
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipRemoveCurve,
                JsonUtility.ToJson(request));

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _ops.RemoveCurveCallCount);
            Assert.AreEqual("m_LocalScale.x", _ops.LastRemoveCurvePropertyName);
        }

        [Test]
        public void Handle_Returns400_WhenComponentTypeMissing()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.AnimationClipRemoveCurve,
                "{\"assetPath\":\"Assets/A.anim\",\"assemblyName\":\"UnityEngine.CoreModule\",\"propertyName\":\"m_LocalScale.x\"}");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("componentType", context.ResponseBody);
            Assert.AreEqual(0, _ops.RemoveCurveCallCount);
        }
    }
}
