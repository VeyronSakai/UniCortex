using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SetProjectSettingUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSetSetting_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyProjectSettingsOperations();
            var useCase = new SetProjectSettingUseCase(dispatcher, ops);

            useCase.ExecuteAsync("Time", "m_TimeScale", "2", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.SetSettingCallCount);
            Assert.AreEqual("Time", ops.LastSetSettingCategory);
            Assert.AreEqual("m_TimeScale", ops.LastSetSettingPropertyPath);
            Assert.AreEqual("2", ops.LastSetSettingValue);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
