using System;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class EditorWindowOperationsAdapterTest
    {
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void SetSceneViewCamera_RejectsNonFiniteSize(float invalidSize)
        {
            var adapter = new EditorWindowOperationsAdapter();
            var request = new SetSceneViewCameraRequest
            {
                position = new Vector3Data { x = 0f, y = 0f, z = 0f },
                rotation = new QuaternionData { x = 0f, y = 0f, z = 0f, w = 1f },
                size = invalidSize
            };

            var ex = Assert.Throws<ArgumentException>(() => adapter.SetSceneViewCamera(request));

            Assert.That(ex!.Message, Does.Contain("size"));
            Assert.That(ex.Message, Does.Contain("finite"));
        }
    }
}
