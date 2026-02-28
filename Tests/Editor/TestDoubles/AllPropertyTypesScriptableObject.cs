using System;
using UnityEngine;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal enum TestEnum
    {
        Alpha,
        Beta,
        Gamma
    }

    internal sealed class AllPropertyTypesScriptableObject : ScriptableObject
    {
        public int intField;
        public bool boolField;
        public float floatField;
        public string stringField;
        public TestEnum enumField;
        public Vector2 vector2Field;
        public Vector3 vector3Field;
        public Vector4 vector4Field;
        public Color colorField;
        public Rect rectField;
        public Bounds boundsField;
        public Quaternion quaternionField;
        public GameObject objectReferenceField;
        public LayerMask layerMaskField;
        public AnimationCurve animationCurveField;
        public Gradient gradientField;
        public Vector2Int vector2IntField;
        public Vector3Int vector3IntField;
        public RectInt rectIntField;
        public BoundsInt boundsIntField;
        public Hash128 hash128Field;

        [Serializable]
        public struct NestedStruct
        {
            public int value;
        }

        public NestedStruct[] arrayField;
    }
}
