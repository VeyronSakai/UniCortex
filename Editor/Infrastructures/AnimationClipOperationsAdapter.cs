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
                        keys.Add(new AnimationCurveKeyEntry(key.time, key.value, key.inTangent, key.outTangent,
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
            var keyframes = new Keyframe[sortedKeys.Count];
            for (var i = 0; i < sortedKeys.Count; i++)
            {
                var key = sortedKeys[i];
                var isEaseInOut = modes[i] == AnimationTangentModes.EaseInOut;
                keyframes[i] = new Keyframe(key.time, key.value,
                    isEaseInOut ? 0f : key.inTangent,
                    isEaseInOut ? 0f : key.outTangent);
            }

            var curve = new AnimationCurve(keyframes);

            // Tangent modes must be applied after all keys exist, because Auto / Linear
            // tangents are computed from neighbouring keys.
            for (var i = 0; i < curve.length; i++)
            {
                var mode = ToUnityTangentMode(modes[i]);
                var broken = mode == AnimationUtility.TangentMode.Linear ||
                             mode == AnimationUtility.TangentMode.Constant ||
                             (mode == AnimationUtility.TangentMode.Free &&
                              !Mathf.Approximately(curve[i].inTangent, curve[i].outTangent));

                AnimationUtility.SetKeyBroken(curve, i, broken);
                AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
                AnimationUtility.SetKeyRightTangentMode(curve, i, mode);
            }

            return curve;
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
                AnimationTangentModes.Linear, AnimationTangentModes.Constant, AnimationTangentModes.EaseInOut
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
