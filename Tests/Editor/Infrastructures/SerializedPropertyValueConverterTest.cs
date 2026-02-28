using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SerializedPropertyValueConverterTest
    {
        private AllPropertyTypesScriptableObject _so;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _so = ScriptableObject.CreateInstance<AllPropertyTypesScriptableObject>();
            _serializedObject = new SerializedObject(_so);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_so);
        }

        [Test]
        public void Integer_ReturnsIntValueAsString()
        {
            _so.intField = 42;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("intField");
            Assert.AreEqual("42", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Boolean_ReturnsLowercaseString()
        {
            _so.boolField = true;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("boolField");
            Assert.AreEqual("true", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Boolean_False_ReturnsLowercaseFalse()
        {
            _so.boolField = false;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("boolField");
            Assert.AreEqual("false", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Float_ReturnsFloatValueAsString()
        {
            _so.floatField = 3.14f;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("floatField");
            Assert.AreEqual(3.14f.ToString(), SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void String_ReturnsStringValue()
        {
            _so.stringField = "hello";
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("stringField");
            Assert.AreEqual("hello", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void String_Null_ReturnsEmpty()
        {
            _so.stringField = null;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("stringField");
            Assert.AreEqual("", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Enum_ReturnsDisplayName()
        {
            _so.enumField = TestEnum.Beta;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("enumField");
            Assert.AreEqual("Beta", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Vector2_ReturnsFormattedString()
        {
            _so.vector2Field = new Vector2(1f, 2f);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("vector2Field");
            Assert.AreEqual("(1, 2)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Vector3_ReturnsFormattedString()
        {
            _so.vector3Field = new Vector3(1f, 2f, 3f);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("vector3Field");
            Assert.AreEqual("(1, 2, 3)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Vector4_ReturnsFormattedString()
        {
            _so.vector4Field = new Vector4(1f, 2f, 3f, 4f);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("vector4Field");
            Assert.AreEqual("(1, 2, 3, 4)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Color_ReturnsFormattedString()
        {
            _so.colorField = new Color(1f, 0f, 0.5f, 1f);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("colorField");
            Assert.AreEqual("(1, 0, 0.5, 1)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Rect_ReturnsFormattedString()
        {
            _so.rectField = new Rect(1f, 2f, 3f, 4f);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("rectField");
            Assert.AreEqual("(x:1, y:2, w:3, h:4)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Bounds_ReturnsFormattedString()
        {
            _so.boundsField = new Bounds(Vector3.zero, Vector3.one);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("boundsField");
            Assert.AreEqual($"(center:{Vector3.zero}, size:{Vector3.one})",
                SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Quaternion_ReturnsFormattedString()
        {
            _so.quaternionField = Quaternion.identity;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("quaternionField");
            Assert.AreEqual("(0, 0, 0, 1)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void ObjectReference_Null_ReturnsNullString()
        {
            _so.objectReferenceField = null;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("objectReferenceField");
            Assert.AreEqual("null", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void ObjectReference_NonNull_ReturnsName()
        {
            var go = new GameObject("TestObj");
            try
            {
                _so.objectReferenceField = go;
                _serializedObject.Update();
                var prop = _serializedObject.FindProperty("objectReferenceField");
                Assert.AreEqual("TestObj", SerializedPropertyValueConverter.ToValueString(prop));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LayerMask_ReturnsIntValueAsString()
        {
            _so.layerMaskField = 5;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("layerMaskField");
            Assert.AreEqual("5", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void AnimationCurve_ReturnsKeysCount()
        {
            _so.animationCurveField = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("animationCurveField");
            Assert.AreEqual("AnimationCurve(keys:2)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Gradient_ReturnsGradientString()
        {
            _so.gradientField = new Gradient();
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("gradientField");
            Assert.AreEqual("Gradient", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Vector2Int_ReturnsFormattedString()
        {
            _so.vector2IntField = new Vector2Int(1, 2);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("vector2IntField");
            Assert.AreEqual("(1, 2)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Vector3Int_ReturnsFormattedString()
        {
            _so.vector3IntField = new Vector3Int(1, 2, 3);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("vector3IntField");
            Assert.AreEqual("(1, 2, 3)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void RectInt_ReturnsFormattedString()
        {
            _so.rectIntField = new RectInt(1, 2, 3, 4);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("rectIntField");
            Assert.AreEqual("(x:1, y:2, w:3, h:4)", SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void BoundsInt_ReturnsFormattedString()
        {
            _so.boundsIntField = new BoundsInt(Vector3Int.zero, Vector3Int.one);
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("boundsIntField");
            Assert.AreEqual($"(position:{Vector3Int.zero}, size:{Vector3Int.one})",
                SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void Hash128_ReturnsHashString()
        {
            var hash = Hash128.Compute("test");
            _so.hash128Field = hash;
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("hash128Field");
            Assert.AreEqual(hash.ToString(), SerializedPropertyValueConverter.ToValueString(prop));
        }

        [Test]
        public void ArraySize_ReturnsCount()
        {
            _so.arrayField = new AllPropertyTypesScriptableObject.NestedStruct[3];
            _serializedObject.Update();
            var prop = _serializedObject.FindProperty("arrayField.Array.size");
            Assert.AreEqual("3", SerializedPropertyValueConverter.ToValueString(prop));
        }
    }
}
