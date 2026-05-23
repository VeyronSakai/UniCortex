using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetProjectSettingsCategoriesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsCategories_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyProjectSettingsOperations
            {
                GetCategoriesResult = new GetProjectSettingsCategoriesResponse(
                    new List<ProjectSettingsCategoryEntry>
                    {
                        new ProjectSettingsCategoryEntry("Player", "ProjectSettings/ProjectSettings.asset")
                    })
            };
            var useCase = new GetProjectSettingsCategoriesUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, result.categories.Count);
            Assert.AreEqual("Player", result.categories[0].name);
            Assert.AreEqual(1, ops.GetCategoriesCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
