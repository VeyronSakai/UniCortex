using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    /// <summary>
    /// Inverse of <see cref="SerializedPropertyValueConverter.ToValueString"/>.
    /// Parses a string representation and writes the value back into a <see cref="SerializedProperty"/>.
    /// </summary>
    internal static class SerializedPropertyValueParser
    {
        /// <summary>
        /// Parses <paramref name="value"/> according to the <see cref="SerializedProperty.propertyType"/>
        /// and writes the result into <paramref name="property"/>.
        /// The caller is responsible for calling <c>SerializedObject.ApplyModifiedProperties()</c> afterwards.
        /// </summary>
        public static void ApplyValue(SerializedProperty property, string value)
        {
            switch (property.propertyType)
            {
                // LayerMask is stored as int internally in SerializedProperty.
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out var intVal))
                        property.intValue = intVal;
                    else
                        throw new ArgumentException(
                            $"Cannot parse '{value}' as int for property '{property.propertyPath}'.");
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = ParseBoolean(value);
                    break;

                case SerializedPropertyType.Float:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                            out var floatVal))
                        property.floatValue = floatVal;
                    else
                        throw new ArgumentException(
                            $"Cannot parse '{value}' as float for property '{property.propertyPath}'.");
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = value;
                    break;

                case SerializedPropertyType.Enum:
                    ApplyEnumValue(property, value);
                    break;

                case SerializedPropertyType.Vector2:
                {
                    var f = ParseFloats(StripParentheses(value), 2);
                    property.vector2Value = new Vector2(f[0], f[1]);
                    break;
                }

                case SerializedPropertyType.Vector3:
                {
                    var f = ParseFloats(StripParentheses(value), 3);
                    property.vector3Value = new Vector3(f[0], f[1], f[2]);
                    break;
                }

                case SerializedPropertyType.Vector4:
                {
                    var f = ParseFloats(StripParentheses(value), 4);
                    property.vector4Value = new Vector4(f[0], f[1], f[2], f[3]);
                    break;
                }

                case SerializedPropertyType.Color:
                {
                    var f = ParseFloats(StripParentheses(value), 4);
                    property.colorValue = new Color(f[0], f[1], f[2], f[3]);
                    break;
                }

                case SerializedPropertyType.Quaternion:
                {
                    var f = ParseFloats(StripParentheses(value), 4);
                    property.quaternionValue = new Quaternion(f[0], f[1], f[2], f[3]);
                    break;
                }

                case SerializedPropertyType.Rect:
                {
                    var f = ParseFloats(StripParentheses(value), 4);
                    property.rectValue = new Rect(f[0], f[1], f[2], f[3]);
                    break;
                }

                case SerializedPropertyType.RectInt:
                {
                    var n = ParseInts(StripParentheses(value), 4);
                    property.rectIntValue = new RectInt(n[0], n[1], n[2], n[3]);
                    break;
                }

                // Accepts two formats:
                //   Nested: "(center:(cx, cy, cz), size:(sx, sy, sz))"
                //   Flat:   "cx, cy, cz, sx, sy, sz"
                case SerializedPropertyType.Bounds:
                {
                    var stripped = StripParentheses(value);
                    float[] cf, sf;
                    if (stripped.IndexOf("center:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Nested format — extract each labeled group separately.
                        var centerStr = ExtractNestedGroup(stripped, "center");
                        var sizeStr = ExtractNestedGroup(stripped, "size");
                        cf = ParseFloats(centerStr, 3);
                        sf = ParseFloats(sizeStr, 3);
                    }
                    else
                    {
                        // Flat format — first 3 values are center, last 3 are size.
                        var f = ParseFloats(stripped, 6);
                        cf = new[] { f[0], f[1], f[2] };
                        sf = new[] { f[3], f[4], f[5] };
                    }

                    property.boundsValue = new Bounds(
                        new Vector3(cf[0], cf[1], cf[2]),
                        new Vector3(sf[0], sf[1], sf[2]));
                    break;
                }

                // Accepts two formats:
                //   Nested: "(position:(px, py, pz), size:(sx, sy, sz))"
                //   Flat:   "px, py, pz, sx, sy, sz"
                case SerializedPropertyType.BoundsInt:
                {
                    var stripped = StripParentheses(value);
                    int[] pn, sn;
                    if (stripped.IndexOf("position:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var posStr = ExtractNestedGroup(stripped, "position");
                        var sizeStr = ExtractNestedGroup(stripped, "size");
                        pn = ParseInts(posStr, 3);
                        sn = ParseInts(sizeStr, 3);
                    }
                    else
                    {
                        var n = ParseInts(stripped, 6);
                        pn = new[] { n[0], n[1], n[2] };
                        sn = new[] { n[3], n[4], n[5] };
                    }

                    property.boundsIntValue = new BoundsInt(
                        new Vector3Int(pn[0], pn[1], pn[2]),
                        new Vector3Int(sn[0], sn[1], sn[2]));
                    break;
                }

                case SerializedPropertyType.Vector2Int:
                {
                    var n = ParseInts(StripParentheses(value), 2);
                    property.vector2IntValue = new Vector2Int(n[0], n[1]);
                    break;
                }

                case SerializedPropertyType.Vector3Int:
                {
                    var n = ParseInts(StripParentheses(value), 3);
                    property.vector3IntValue = new Vector3Int(n[0], n[1], n[2]);
                    break;
                }

                case SerializedPropertyType.Hash128:
                    property.hash128Value = Hash128.Parse(value);
                    break;

                default:
                    throw new ArgumentException(
                        $"Unsupported property type '{property.propertyType}' for property '{property.propertyPath}'.");
            }
        }

        /// <summary>
        /// Removes the outermost parentheses if present.
        /// e.g. "(1, 2, 3)" → "1, 2, 3",  "1, 2" → "1, 2"  (no-op)
        /// </summary>
        private static string StripParentheses(string s)
        {
            s = s.Trim();
            if (s.Length >= 2 && s[0] == '(' && s[s.Length - 1] == ')')
                s = s.Substring(1, s.Length - 2);
            return s;
        }

        /// <summary>
        /// Splits a comma-separated string into <paramref name="count"/> floats.
        /// Each segment may have a label prefix (e.g. "x:1") which is stripped before parsing.
        /// </summary>
        private static float[] ParseFloats(string s, int count)
        {
            var parts = s.Split(',');
            if (parts.Length < count)
                throw new ArgumentException(
                    $"Expected {count} values but found {parts.Length} in '{s}'.");

            var result = new float[count];
            for (var i = 0; i < count; i++)
            {
                var part = StripLabel(parts[i]);
                if (!float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture,
                        out result[i]))
                    throw new ArgumentException($"Cannot parse '{part}' as float.");
            }

            return result;
        }

        /// <summary>
        /// Splits a comma-separated string into <paramref name="count"/> ints.
        /// Each segment may have a label prefix (e.g. "x:1") which is stripped before parsing.
        /// </summary>
        private static int[] ParseInts(string s, int count)
        {
            var parts = s.Split(',');
            if (parts.Length < count)
                throw new ArgumentException(
                    $"Expected {count} values but found {parts.Length} in '{s}'.");

            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                var part = StripLabel(parts[i]);
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out result[i]))
                    throw new ArgumentException($"Cannot parse '{part}' as int.");
            }

            return result;
        }

        /// <summary>
        /// Removes a label prefix from a single value segment.
        /// e.g. "x:1" → "1",  "  w:3.5 " → "3.5",  "42" → "42"  (no-op)
        /// </summary>
        private static string StripLabel(string part)
        {
            part = part.Trim();
            var colonIdx = part.IndexOf(':');
            if (colonIdx >= 0)
                part = part.Substring(colonIdx + 1).Trim();
            return part;
        }

        private static bool ParseBoolean(string value)
        {
            if (bool.TryParse(value, out var boolVal))
                return boolVal;
            if (value == "1") return true;
            if (value == "0") return false;
            throw new ArgumentException($"Cannot parse '{value}' as boolean.");
        }

        /// <summary>
        /// Sets an enum property. Tries numeric index first (e.g. "2"),
        /// then falls back to case-insensitive display name lookup (e.g. "Beta").
        /// </summary>
        private static void ApplyEnumValue(SerializedProperty property, string value)
        {
            // Numeric index takes priority — allows setting by raw index.
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var enumIdx))
            {
                property.enumValueIndex = enumIdx;
                return;
            }

            // Fall back to display name matching (case-insensitive).
            var names = property.enumDisplayNames;
            for (var i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    property.enumValueIndex = i;
                    return;
                }
            }

            throw new ArgumentException(
                $"Cannot parse '{value}' as enum for property '{property.propertyPath}'. Valid names: {string.Join(", ", names)}");
        }

        /// <summary>
        /// Extracts the content of a labeled, parenthesized group from a nested format string.
        /// Used for Bounds/BoundsInt parsing.
        ///
        /// Example: given "center:(1, 2, 3), size:(4, 5, 6)" with label "center",
        ///          returns "1, 2, 3" (the content inside the parentheses after "center:").
        ///
        /// Uses depth tracking to correctly match the closing parenthesis,
        /// handling cases where values themselves might not contain nested parens
        /// but ensuring robustness.
        /// </summary>
        private static string ExtractNestedGroup(string s, string label)
        {
            // Find "label:" in the string.
            var labelIdx = s.IndexOf(label + ":", StringComparison.OrdinalIgnoreCase);
            if (labelIdx < 0)
                throw new ArgumentException($"Label '{label}' not found in '{s}'.");

            var afterLabel = s.Substring(labelIdx + label.Length + 1).TrimStart();

            if (afterLabel.Length > 0 && afterLabel[0] == '(')
            {
                // Walk through characters tracking parenthesis depth.
                // When depth returns to 0, we've found the matching ')'.
                // Return the content between the outermost '(' and ')'.
                var depth = 0;
                for (var i = 0; i < afterLabel.Length; i++)
                {
                    if (afterLabel[i] == '(') depth++;
                    else if (afterLabel[i] == ')') depth--;
                    if (depth == 0)
                        return afterLabel.Substring(1, i - 1);
                }
            }

            // Fallback for non-parenthesized values — take until ")," or end of string.
            var commaOrEnd = afterLabel.IndexOf("),", StringComparison.Ordinal);
            return commaOrEnd >= 0 ? afterLabel.Substring(0, commaOrEnd) : afterLabel;
        }
    }
}
