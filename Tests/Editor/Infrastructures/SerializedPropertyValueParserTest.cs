using System;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SerializedPropertyValueParserTest
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
            UnityEngine.Object.DestroyImmediate(_so);
        }

        // --- Integer ---

        [Test]
        public void Integer_PositiveValue()
        {
            ApplyAndVerifyInt("intField", "42", 42);
        }

        [Test]
        public void Integer_NegativeValue()
        {
            ApplyAndVerifyInt("intField", "-7", -7);
        }

        // --- Boolean ---

        [Test]
        public void Boolean_True()
        {
            ApplyAndVerifyBool("boolField", "true", true);
        }

        [Test]
        public void Boolean_False()
        {
            ApplyAndVerifyBool("boolField", "false", false);
        }

        [Test]
        public void Boolean_TrueCaseInsensitive()
        {
            ApplyAndVerifyBool("boolField", "True", true);
        }

        [Test]
        public void Boolean_One()
        {
            ApplyAndVerifyBool("boolField", "1", true);
        }

        [Test]
        public void Boolean_Zero()
        {
            ApplyAndVerifyBool("boolField", "0", false);
        }

        // --- Float ---

        [Test]
        public void Float_Decimal()
        {
            ApplyAndVerifyFloat("floatField", "3.14", 3.14f);
        }

        [Test]
        public void Float_Negative()
        {
            ApplyAndVerifyFloat("floatField", "-1.5", -1.5f);
        }

        [Test]
        public void Float_IntegerFormat()
        {
            ApplyAndVerifyFloat("floatField", "10", 10f);
        }

        // --- String ---

        [Test]
        public void String_Normal()
        {
            var prop = FindProperty("stringField");
            SerializedPropertyValueParser.ApplyValue(prop, "hello");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual("hello", _so.stringField);
        }

        [Test]
        public void String_Empty()
        {
            _so.stringField = "initial";
            _serializedObject.Update();
            var prop = FindProperty("stringField");
            SerializedPropertyValueParser.ApplyValue(prop, "");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual("", _so.stringField);
        }

        // --- Enum ---

        [Test]
        public void Enum_ByName()
        {
            var prop = FindProperty("enumField");
            SerializedPropertyValueParser.ApplyValue(prop, "Beta");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(TestEnum.Beta, _so.enumField);
        }

        [Test]
        public void Enum_ByIndex()
        {
            var prop = FindProperty("enumField");
            SerializedPropertyValueParser.ApplyValue(prop, "2");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(TestEnum.Gamma, _so.enumField);
        }

        [Test]
        public void Enum_CaseInsensitive()
        {
            var prop = FindProperty("enumField");
            SerializedPropertyValueParser.ApplyValue(prop, "gamma");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(TestEnum.Gamma, _so.enumField);
        }

        // --- Vector2 ---

        [Test]
        public void Vector2_WithParentheses()
        {
            var prop = FindProperty("vector2Field");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 2)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector2(1f, 2f), _so.vector2Field);
        }

        [Test]
        public void Vector2_WithoutParentheses()
        {
            var prop = FindProperty("vector2Field");
            SerializedPropertyValueParser.ApplyValue(prop, "3, 4");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector2(3f, 4f), _so.vector2Field);
        }

        // --- Vector3 ---

        [Test]
        public void Vector3_WithParentheses()
        {
            var prop = FindProperty("vector3Field");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 2, 3)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector3(1f, 2f, 3f), _so.vector3Field);
        }

        [Test]
        public void Vector3_WithoutParentheses()
        {
            var prop = FindProperty("vector3Field");
            SerializedPropertyValueParser.ApplyValue(prop, "4, 5, 6");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector3(4f, 5f, 6f), _so.vector3Field);
        }

        // --- Vector4 ---

        [Test]
        public void Vector4_WithParentheses()
        {
            var prop = FindProperty("vector4Field");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 2, 3, 4)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector4(1f, 2f, 3f, 4f), _so.vector4Field);
        }

        [Test]
        public void Vector4_WithoutParentheses()
        {
            var prop = FindProperty("vector4Field");
            SerializedPropertyValueParser.ApplyValue(prop, "5, 6, 7, 8");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector4(5f, 6f, 7f, 8f), _so.vector4Field);
        }

        // --- Color ---

        [Test]
        public void Color_FourValues()
        {
            var prop = FindProperty("colorField");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 0, 0.5, 1)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Color(1f, 0f, 0.5f, 1f), _so.colorField);
        }

        // --- Quaternion ---

        [Test]
        public void Quaternion_FourValues()
        {
            var prop = FindProperty("quaternionField");
            SerializedPropertyValueParser.ApplyValue(prop, "(0, 0, 0, 1)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(Quaternion.identity, _so.quaternionField);
        }

        // --- Rect ---

        [Test]
        public void Rect_WithLabels()
        {
            var prop = FindProperty("rectField");
            SerializedPropertyValueParser.ApplyValue(prop, "(x:1, y:2, w:3, h:4)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Rect(1f, 2f, 3f, 4f), _so.rectField);
        }

        [Test]
        public void Rect_WithoutLabels()
        {
            var prop = FindProperty("rectField");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 2, 3, 4)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Rect(1f, 2f, 3f, 4f), _so.rectField);
        }

        // --- RectInt ---

        [Test]
        public void RectInt_WithLabels()
        {
            var prop = FindProperty("rectIntField");
            SerializedPropertyValueParser.ApplyValue(prop, "(x:1, y:2, w:3, h:4)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new RectInt(1, 2, 3, 4), _so.rectIntField);
        }

        [Test]
        public void RectInt_WithoutLabels()
        {
            var prop = FindProperty("rectIntField");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 2, 3, 4)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new RectInt(1, 2, 3, 4), _so.rectIntField);
        }

        // --- Bounds ---

        [Test]
        public void Bounds_FlatSixValues()
        {
            var prop = FindProperty("boundsField");
            SerializedPropertyValueParser.ApplyValue(prop, "0, 0, 0, 1, 1, 1");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Bounds(Vector3.zero, Vector3.one), _so.boundsField);
        }

        [Test]
        public void Bounds_NestedFormat()
        {
            var prop = FindProperty("boundsField");
            SerializedPropertyValueParser.ApplyValue(prop, "(center:(0, 0, 0), size:(1, 1, 1))");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Bounds(Vector3.zero, Vector3.one), _so.boundsField);
        }

        // --- BoundsInt ---

        [Test]
        public void BoundsInt_FlatSixValues()
        {
            var prop = FindProperty("boundsIntField");
            SerializedPropertyValueParser.ApplyValue(prop, "0, 0, 0, 1, 1, 1");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new BoundsInt(Vector3Int.zero, Vector3Int.one), _so.boundsIntField);
        }

        [Test]
        public void BoundsInt_NestedFormat()
        {
            var prop = FindProperty("boundsIntField");
            SerializedPropertyValueParser.ApplyValue(prop, "(position:(0, 0, 0), size:(1, 1, 1))");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new BoundsInt(Vector3Int.zero, Vector3Int.one), _so.boundsIntField);
        }

        // --- LayerMask ---

        [Test]
        public void LayerMask_ParsesInt()
        {
            var prop = FindProperty("layerMaskField");
            SerializedPropertyValueParser.ApplyValue(prop, "5");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(5, _so.layerMaskField.value);
        }

        // --- Hash128 ---

        [Test]
        public void Hash128_ParsesHashString()
        {
            var hash = Hash128.Compute("test");
            var prop = FindProperty("hash128Field");
            SerializedPropertyValueParser.ApplyValue(prop, hash.ToString());
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(hash, _so.hash128Field);
        }

        // --- Vector2Int ---

        [Test]
        public void Vector2Int_WithParentheses()
        {
            var prop = FindProperty("vector2IntField");
            SerializedPropertyValueParser.ApplyValue(prop, "(3, 4)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector2Int(3, 4), _so.vector2IntField);
        }

        // --- Vector3Int ---

        [Test]
        public void Vector3Int_WithParentheses()
        {
            var prop = FindProperty("vector3IntField");
            SerializedPropertyValueParser.ApplyValue(prop, "(1, 2, 3)");
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(new Vector3Int(1, 2, 3), _so.vector3IntField);
        }

        // --- Unsupported type ---

        [Test]
        public void ObjectReference_ThrowsArgumentException()
        {
            var prop = FindProperty("objectReferenceField");
            Assert.Throws<ArgumentException>(() =>
                SerializedPropertyValueParser.ApplyValue(prop, "anything"));
        }

        // --- Helpers ---

        private SerializedProperty FindProperty(string name)
        {
            _serializedObject.Update();
            return _serializedObject.FindProperty(name);
        }

        private void ApplyAndVerifyInt(string fieldName, string value, int expected)
        {
            var prop = FindProperty(fieldName);
            SerializedPropertyValueParser.ApplyValue(prop, value);
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(expected, _so.intField);
        }

        private void ApplyAndVerifyBool(string fieldName, string value, bool expected)
        {
            var prop = FindProperty(fieldName);
            SerializedPropertyValueParser.ApplyValue(prop, value);
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(expected, _so.boolField);
        }

        private void ApplyAndVerifyFloat(string fieldName, string value, float expected)
        {
            var prop = FindProperty(fieldName);
            SerializedPropertyValueParser.ApplyValue(prop, value);
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
            Assert.AreEqual(expected, _so.floatField);
        }
    }
}
