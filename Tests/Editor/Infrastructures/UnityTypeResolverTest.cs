using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class UnityTypeResolverTest
    {
        [Test]
        public void Resolve_ValidTypeAndAssembly_ReturnsType()
        {
            var assemblyName = typeof(AllPropertyTypesScriptableObject).Assembly.GetName().Name;

            var type = UnityTypeResolver.Resolve<ScriptableObject>(
                "UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject", assemblyName);

            Assert.That(type, Is.EqualTo(typeof(AllPropertyTypesScriptableObject)));
        }

        [Test]
        public void Resolve_BuiltInComponent_ReturnsType()
        {
            var type = UnityTypeResolver.Resolve<Component>("UnityEngine.Transform", "UnityEngine.CoreModule");

            Assert.That(type, Is.EqualTo(typeof(Transform)));
        }

        [Test]
        public void Resolve_WrongAssembly_ReturnsNull()
        {
            var type = UnityTypeResolver.Resolve<Component>("UnityEngine.Transform", "Assembly-CSharp");
            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_WrongBaseType_ReturnsNull()
        {
            var assemblyName = typeof(AllPropertyTypesScriptableObject).Assembly.GetName().Name;

            var type = UnityTypeResolver.Resolve<Component>(
                "UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject", assemblyName);

            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_UnknownName_ReturnsNull()
        {
            var type = UnityTypeResolver.Resolve<ScriptableObject>(
                "NoSuch.Namespace.NoSuchType", "NoSuchAssembly");
            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_NullOrEmpty_ReturnsNull()
        {
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>(null, "Some.Assembly"), Is.Null);
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>("", "Some.Assembly"), Is.Null);
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>("Some.Type", null), Is.Null);
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>("Some.Type", ""), Is.Null);
        }
    }
}
