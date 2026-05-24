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
        public void Resolve_AssemblyQualifiedName_ReturnsType()
        {
            var assemblyQualified =
                $"UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject, {typeof(AllPropertyTypesScriptableObject).Assembly.GetName().Name}";

            var type = UnityTypeResolver.Resolve<ScriptableObject>(assemblyQualified);

            Assert.That(type, Is.EqualTo(typeof(AllPropertyTypesScriptableObject)));
        }

        [Test]
        public void Resolve_BuiltInComponent_AssemblyQualified_ReturnsType()
        {
            var type = UnityTypeResolver.Resolve<Component>(
                "UnityEngine.Transform, UnityEngine.CoreModule");

            Assert.That(type, Is.EqualTo(typeof(Transform)));
        }

        [Test]
        public void Resolve_NamespaceQualifiedOnly_ReturnsNull()
        {
            // Without an assembly suffix, Type.GetType cannot locate types outside the
            // executing assembly / mscorlib, so resolution fails.
            var type = UnityTypeResolver.Resolve<Component>("UnityEngine.Transform");
            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_WrongBaseType_ReturnsNull()
        {
            var assemblyQualified =
                $"UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject, {typeof(AllPropertyTypesScriptableObject).Assembly.GetName().Name}";

            var type = UnityTypeResolver.Resolve<Component>(assemblyQualified);

            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_UnknownName_ReturnsNull()
        {
            var type = UnityTypeResolver.Resolve<ScriptableObject>(
                "NoSuch.Namespace.NoSuchType, NoSuchAssembly");
            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_NullOrEmpty_ReturnsNull()
        {
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>(null), Is.Null);
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>(""), Is.Null);
        }
    }
}
