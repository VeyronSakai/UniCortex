using System;
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
        public void Resolve_NamespaceQualifiedName_ReturnsType()
        {
            var type = UnityTypeResolver.Resolve<ScriptableObject>(
                "UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject");

            Assert.That(type, Is.EqualTo(typeof(AllPropertyTypesScriptableObject)));
        }

        [Test]
        public void Resolve_AssemblyQualifiedName_ReturnsType()
        {
            var assemblyQualified =
                $"UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject, {typeof(AllPropertyTypesScriptableObject).Assembly.GetName().Name}";

            var type = UnityTypeResolver.Resolve<ScriptableObject>(assemblyQualified);

            Assert.That(type, Is.EqualTo(typeof(AllPropertyTypesScriptableObject)));
        }

        [Test]
        public void Resolve_WrongBaseType_ReturnsNull()
        {
            // AllPropertyTypesScriptableObject is a ScriptableObject, not a Component.
            var type = UnityTypeResolver.Resolve<Component>(
                "UniCortex.Editor.Tests.TestDoubles.AllPropertyTypesScriptableObject");

            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_UnknownName_ReturnsNull()
        {
            var type = UnityTypeResolver.Resolve<ScriptableObject>("NoSuch.Namespace.NoSuchType");
            Assert.That(type, Is.Null);
        }

        [Test]
        public void Resolve_NullOrEmpty_ReturnsNull()
        {
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>(null), Is.Null);
            Assert.That(UnityTypeResolver.Resolve<ScriptableObject>(""), Is.Null);
        }

        [Test]
        public void Resolve_BuiltInComponent_ReturnsType()
        {
            var type = UnityTypeResolver.Resolve<Component>("UnityEngine.Transform");
            Assert.That(type, Is.EqualTo(typeof(Transform)));
        }
    }
}
