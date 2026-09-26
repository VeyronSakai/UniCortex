using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class AnimationClipOperationsAdapter : IAnimationClipOperations
    {
        private const float DefaultFrameRate = 60f;

        public CreateAnimationClipResponse Create(string assetPath, bool loop, float frameRate)
        {
            ValidateAnimPath(assetPath);

            var clip = new AnimationClip { frameRate = frameRate > 0f ? frameRate : DefaultFrameRate };
            AssetDatabase.CreateAsset(clip, assetPath);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Undo.RegisterCreatedObjectUndo(clip, "Create AnimationClip");
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new CreateAnimationClipResponse(true, assetPath);
        }

        public GetAnimationCurvesResponse GetCurves(string assetPath)
        {
            var clip = LoadClip(assetPath);

            var curves = new List<AnimationCurveEntry>();
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var keys = new List<AnimationCurveKeyEntry>();
                if (curve != null)
                {
                    for (var i = 0; i < curve.length; i++)
                    {
                        var key = curve[i];
                        keys.Add(new AnimationCurveKeyEntry(key.time, key.value,
                            ToSerializableTangent(key.inTangent), ToSerializableTangent(key.outTangent),
                            AnimationUtility.GetKeyLeftTangentMode(curve, i).ToString(),
                            AnimationUtility.GetKeyRightTangentMode(curve, i).ToString()));
                    }
                }

                curves.Add(new AnimationCurveEntry(binding.path, binding.type?.FullName,
                    binding.type?.Assembly.GetName().Name, binding.propertyName, keys));
            }

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            return new GetAnimationCurvesResponse(clip.frameRate, settings.loopTime, clip.length, curves);
        }

        public void SetCurve(string assetPath, string path, string componentType, string assemblyName,
            string propertyName, List<AnimationCurveKeyInput> keys)
        {
            var clip = LoadClip(assetPath);
            var binding = CreateCurveBinding(path, componentType, assemblyName, propertyName);
            var curve = BuildCurve(keys);

            Undo.RegisterCompleteObjectUndo(clip, "Set Animation Curve");
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
        }

        public void RemoveCurve(string assetPath, string path, string componentType, string assemblyName,
            string propertyName)
        {
            var clip = LoadClip(assetPath);
            var binding = CreateCurveBinding(path, componentType, assemblyName, propertyName);

            if (AnimationUtility.GetEditorCurve(clip, binding) == null)
            {
                throw new ArgumentException(
                    $"Curve '{binding.path}:{componentType}.{propertyName}' not found in AnimationClip at '{assetPath}'.");
            }

            Undo.RegisterCompleteObjectUndo(clip, "Remove Animation Curve");
            // Passing null removes the curve from the clip.
            AnimationUtility.SetEditorCurve(clip, binding, null);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
        }

        internal static AnimationCurve BuildCurve(List<AnimationCurveKeyInput> keys)
        {
            if (keys == null || keys.Count == 0)
            {
                throw new ArgumentException("At least one key is required.");
            }

            var sortedKeys = keys.OrderBy(k => k.time).ToList();
            for (var i = 1; i < sortedKeys.Count; i++)
            {
                if (Mathf.Approximately(sortedKeys[i - 1].time, sortedKeys[i].time))
                {
                    throw new ArgumentException($"Duplicate key time {sortedKeys[i].time}.");
                }
            }

            var modes = sortedKeys.Select(k => ParseTangentMode(k.tangentMode)).ToArray();
            var curve = new AnimationCurve(sortedKeys
                .Select(k => new Keyframe(k.time, k.value,
                    FromSerializableTangent(k.inTangent), FromSerializableTangent(k.outTangent)))
                .ToArray());

            // Tangent modes must be applied after all keys exist, because Auto / Linear
            // tangents are computed from neighbouring keys.
            for (var i = 0; i < curve.length; i++)
            {
                var mode = ToUnityTangentMode(modes[i]);

                // "Broken" decides whether the left and right tangents of a key are edited independently
                // (broken: a corner is allowed) or kept linked (not broken: the key stays smooth).
                // It only affects editing in the Animation window; runtime evaluation uses inTangent /
                // outTangent alone. It is set the same way the Animation window would, so that later
                // manual edits keep the intended shape (e.g. an intended corner is not re-joined when
                // a tangent handle is dragged).
                //   - Linear:   broken. Left points at the previous key and right at the next key, so the
                //               slopes usually differ ("Both Tangents > Linear" also breaks the key).
                //   - Constant: broken. Both sides are stepped and do not join smoothly.
                //   - Free:     broken only when inTangent != outTangent, i.e. the caller asked for a corner.
                //               Equal tangents (including omitted = 0) stay linked and smooth.
                //   - Auto / ClampedAuto: not broken; these modes compute smooth, linked tangents.
                var broken = mode == AnimationUtility.TangentMode.Linear ||
                             mode == AnimationUtility.TangentMode.Constant ||
                             (mode == AnimationUtility.TangentMode.Free &&
                              !Mathf.Approximately(curve[i].inTangent, curve[i].outTangent));

                // Set broken before the modes: setting a mode makes Unity recompute the tangents for
                // that mode, so the left / right relationship has to be fixed first.
                AnimationUtility.SetKeyBroken(curve, i, broken);
                AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
                AnimationUtility.SetKeyRightTangentMode(curve, i, mode);
            }

            return curve;
        }

        // Unity represents stepped (Constant) tangents as ±Infinity, but JSON has no literal for infinity and
        // JsonUtility would emit a bare "Infinity" token that standard JSON parsers reject. Infinite tangents
        // are therefore exchanged as ±float.MaxValue and converted back on input. The reverse conversion is
        // required: a finite float.MaxValue tangent is not treated as a step by Unity and makes
        // AnimationCurve.Evaluate return NaN.
        internal static float ToSerializableTangent(float tangent)
        {
            if (float.IsPositiveInfinity(tangent))
            {
                return float.MaxValue;
            }

            return float.IsNegativeInfinity(tangent) ? float.MinValue : tangent;
        }

        internal static float FromSerializableTangent(float tangent)
        {
            if (tangent >= float.MaxValue)
            {
                return float.PositiveInfinity;
            }

            return tangent <= float.MinValue ? float.NegativeInfinity : tangent;
        }

        private static string ParseTangentMode(string tangentMode)
        {
            if (string.IsNullOrEmpty(tangentMode))
            {
                return AnimationTangentModes.Free;
            }

            var supported = new[]
            {
                AnimationTangentModes.Free, AnimationTangentModes.Auto, AnimationTangentModes.ClampedAuto,
                AnimationTangentModes.Linear, AnimationTangentModes.Constant
            };
            var match = supported.FirstOrDefault(m => string.Equals(m, tangentMode, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                throw new ArgumentException(
                    $"Unsupported tangentMode '{tangentMode}'. Supported values: {string.Join(", ", supported)}.");
            }

            return match;
        }

        private static AnimationUtility.TangentMode ToUnityTangentMode(string tangentMode)
        {
            switch (tangentMode)
            {
                case AnimationTangentModes.Auto:
                    return AnimationUtility.TangentMode.Auto;
                case AnimationTangentModes.ClampedAuto:
                    return AnimationUtility.TangentMode.ClampedAuto;
                case AnimationTangentModes.Linear:
                    return AnimationUtility.TangentMode.Linear;
                case AnimationTangentModes.Constant:
                    return AnimationUtility.TangentMode.Constant;
                default:
                    return AnimationUtility.TangentMode.Free;
            }
        }

        private static EditorCurveBinding CreateCurveBinding(string path, string componentType, string assemblyName,
            string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName is required.");
            }

            var type = UnityTypeResolver.Resolve<UnityEngine.Object>(componentType, assemblyName);
            if (type == null)
            {
                throw new ArgumentException(
                    $"Type '{componentType}' not found in assembly '{assemblyName}'.");
            }

            return EditorCurveBinding.FloatCurve(path ?? string.Empty, type, propertyName);
        }

        private static AnimationClip LoadClip(string assetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip == null)
            {
                throw new ArgumentException($"AnimationClip not found at path '{assetPath}'.");
            }

            return clip;
        }

        private static void ValidateAnimPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"assetPath must end with '.anim' (got '{assetPath}').");
            }
        }
    }
}
