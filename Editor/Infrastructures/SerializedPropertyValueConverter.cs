using System.Globalization;
using UnityEditor;

namespace UniCortex.Editor.Infrastructures
{
    internal static class SerializedPropertyValueConverter
    {
        public static string ToValueString(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString().ToLower();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return property.stringValue ?? "";
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 &&
                           property.enumValueIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:
                    var v2 = property.vector2Value;
                    return $"({v2.x}, {v2.y})";
                case SerializedPropertyType.Vector3:
                    var v3 = property.vector3Value;
                    return $"({v3.x}, {v3.y}, {v3.z})";
                case SerializedPropertyType.Vector4:
                    var v4 = property.vector4Value;
                    return $"({v4.x}, {v4.y}, {v4.z}, {v4.w})";
                case SerializedPropertyType.Color:
                    var c = property.colorValue;
                    return $"({c.r}, {c.g}, {c.b}, {c.a})";
                case SerializedPropertyType.Rect:
                    var r = property.rectValue;
                    return $"(x:{r.x}, y:{r.y}, w:{r.width}, h:{r.height})";
                case SerializedPropertyType.Bounds:
                    var b = property.boundsValue;
                    return $"(center:{b.center}, size:{b.size})";
                case SerializedPropertyType.Quaternion:
                    var q = property.quaternionValue;
                    return $"({q.x}, {q.y}, {q.z}, {q.w})";
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null
                        ? property.objectReferenceValue.name
                        : "null";
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString();
                case SerializedPropertyType.Character:
                    return ((char)property.intValue).ToString();
                case SerializedPropertyType.AnimationCurve:
                    var curve = property.animationCurveValue;
                    return $"AnimationCurve(keys:{curve.length})";
                case SerializedPropertyType.Gradient:
                    return "Gradient";
                case SerializedPropertyType.Vector2Int:
                    var v2I = property.vector2IntValue;
                    return $"({v2I.x}, {v2I.y})";
                case SerializedPropertyType.Vector3Int:
                    var v3I = property.vector3IntValue;
                    return $"({v3I.x}, {v3I.y}, {v3I.z})";
                case SerializedPropertyType.RectInt:
                    var ri = property.rectIntValue;
                    return $"(x:{ri.x}, y:{ri.y}, w:{ri.width}, h:{ri.height})";
                case SerializedPropertyType.BoundsInt:
                    var bi = property.boundsIntValue;
                    return $"(position:{bi.position}, size:{bi.size})";
                case SerializedPropertyType.Hash128:
                    return property.hash128Value.ToString();
                case SerializedPropertyType.ExposedReference:
                    return property.objectReferenceValue != null
                        ? property.objectReferenceValue.name
                        : "null";
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString();
                default:
                    return property.propertyType.ToString();
            }
        }
    }
}
