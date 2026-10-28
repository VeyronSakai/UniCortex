using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine.Audio;

namespace UniCortex.Editor.Infrastructures
{
    // Unity has no public Editor API for editing AudioMixer assets. The editor-side classes
    // (AudioMixerController, AudioMixerGroupController, ...) are internal, so this class calls the same
    // members the Audio Mixer window uses through reflection. The members were checked on Unity 2022.3 and 6000.3.
    internal static class AudioMixerControllerReflection
    {
        private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private const string ControllerTypeName = "UnityEditor.Audio.AudioMixerController";
        private const string ExposedParameterTypeName = "UnityEditor.Audio.ExposedAudioParameter";
        private const string GroupParameterPathTypeName = "UnityEditor.Audio.AudioGroupParameterPath";

        internal readonly struct ExposedParameter
        {
            public readonly GUID Guid;
            public readonly string Name;

            public ExposedParameter(GUID guid, string name)
            {
                Guid = guid;
                Name = name;
            }
        }

        // Creates the asset with a "Master" group and one snapshot, like Assets > Create > Audio Mixer.
        public static AudioMixer CreateMixerAt(string assetPath)
        {
            return (AudioMixer)InvokeStatic(GetEditorType(ControllerTypeName), "CreateMixerControllerAtPath",
                assetPath);
        }

        public static bool IsEditableMixer(AudioMixer mixer)
        {
            return GetEditorType(ControllerTypeName).IsInstanceOfType(mixer);
        }

        public static AudioMixerGroup GetMasterGroup(AudioMixer mixer)
        {
            return (AudioMixerGroup)GetProperty(mixer, "masterGroup");
        }

        public static AudioMixerGroup[] GetChildren(AudioMixerGroup group)
        {
            var children = (Array)GetProperty(group, "children");
            return children == null ? Array.Empty<AudioMixerGroup>() : children.Cast<AudioMixerGroup>().ToArray();
        }

        public static AudioMixerSnapshot[] GetSnapshots(AudioMixer mixer)
        {
            var snapshots = (Array)GetProperty(mixer, "snapshots");
            return snapshots == null ? Array.Empty<AudioMixerSnapshot>() : snapshots.Cast<AudioMixerSnapshot>().ToArray();
        }

        public static AudioMixerSnapshot GetStartSnapshot(AudioMixer mixer)
        {
            return (AudioMixerSnapshot)GetProperty(mixer, "startSnapshot");
        }

        // Creates a group with its Attenuation effect as sub-assets of the mixer (Undo recorded),
        // then attaches it to the parent and shows it in the current view, as the Audio Mixer window does.
        public static AudioMixerGroup AddGroup(AudioMixer mixer, string name, AudioMixerGroup parent)
        {
            var group = (AudioMixerGroup)Invoke(mixer, "CreateNewGroup", name, true);
            Invoke(mixer, "AddChildToParent", group, parent);

            // A mixer that has never been opened in the Audio Mixer window has no views yet; the window
            // creates them (showing every group) when it opens, so there is no view to add the group to.
            if (HasCurrentView(mixer))
            {
                Invoke(mixer, "AddGroupToCurrentView", group);
            }

            return group;
        }

        private static bool HasCurrentView(AudioMixer mixer)
        {
            var views = (Array)GetProperty(mixer, "views");
            var currentViewIndex = (int)GetProperty(mixer, "currentViewIndex");
            return views != null && currentViewIndex >= 0 && currentViewIndex < views.Length;
        }

        public static GUID GetVolumeGuid(AudioMixerGroup group)
        {
            return (GUID)Invoke(group, "GetGUIDForVolume");
        }

        public static GUID GetPitchGuid(AudioMixerGroup group)
        {
            return (GUID)Invoke(group, "GetGUIDForPitch");
        }

        // Exposes the parameter under a generated name (e.g. "MyExposedParam"); rename it with SetExposedParameters.
        public static void AddExposedParameter(AudioMixer mixer, AudioMixerGroup group, GUID parameter)
        {
            var path = Activator.CreateInstance(GetEditorType(GroupParameterPathTypeName), group, parameter);
            Invoke(mixer, "AddExposedParameter", path);
        }

        public static List<ExposedParameter> GetExposedParameters(AudioMixer mixer)
        {
            var (guidField, nameField) = GetExposedParameterFields();
            var parameters = (Array)GetProperty(mixer, "exposedParameters");
            var result = new List<ExposedParameter>();
            if (parameters == null)
            {
                return result;
            }

            foreach (var parameter in parameters)
            {
                result.Add(new ExposedParameter((GUID)guidField.GetValue(parameter),
                    (string)nameField.GetValue(parameter)));
            }

            return result;
        }

        public static void SetExposedParameters(AudioMixer mixer, IReadOnlyList<ExposedParameter> parameters)
        {
            var type = GetEditorType(ExposedParameterTypeName);
            var (guidField, nameField) = GetExposedParameterFields();
            var array = Array.CreateInstance(type, parameters.Count);
            for (var i = 0; i < parameters.Count; i++)
            {
                // Fields of a boxed struct are set on the box, which is then copied into the array.
                var boxed = Activator.CreateInstance(type);
                guidField.SetValue(boxed, parameters[i].Guid);
                nameField.SetValue(boxed, parameters[i].Name);
                array.SetValue(boxed, i);
            }

            SetProperty(mixer, "exposedParameters", array);
            // Refreshes the Audio Mixer window's list of exposed parameters.
            Invoke(mixer, "OnChangedExposedParameter");
        }

        private static (FieldInfo guid, FieldInfo name) GetExposedParameterFields()
        {
            var type = GetEditorType(ExposedParameterTypeName);
            return (GetField(type, "guid"), GetField(type, "name"));
        }

        private static Type GetEditorType(string fullName)
        {
            var type = typeof(AssetDatabase).Assembly.GetType(fullName) ??
                       typeof(UnityEditor.Editor).Assembly.GetType(fullName);
            if (type == null)
            {
                throw NotSupported(fullName);
            }

            return type;
        }

        private static FieldInfo GetField(Type type, string name)
        {
            return type.GetField(name, InstanceFlags) ?? throw NotSupported($"{type.FullName}.{name}");
        }

        private static object GetProperty(object target, string name)
        {
            var property = target.GetType().GetProperty(name, InstanceFlags) ??
                           throw NotSupported($"{target.GetType().FullName}.{name}");
            return property.GetValue(target);
        }

        private static void SetProperty(object target, string name, object value)
        {
            var property = target.GetType().GetProperty(name, InstanceFlags) ??
                           throw NotSupported($"{target.GetType().FullName}.{name}");
            property.SetValue(target, value);
        }

        private static object Invoke(object target, string name, params object[] args)
        {
            return InvokeMethod(FindMethod(target.GetType(), name, args.Length, InstanceFlags), target, args);
        }

        private static object InvokeStatic(Type type, string name, params object[] args)
        {
            return InvokeMethod(FindMethod(type, name, args.Length, StaticFlags), null, args);
        }

        private static MethodInfo FindMethod(Type type, string name, int parameterCount, BindingFlags flags)
        {
            return type.GetMethods(flags)
                       .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == parameterCount) ??
                   throw NotSupported($"{type.FullName}.{name}");
        }

        private static object InvokeMethod(MethodInfo method, object target, object[] args)
        {
            try
            {
                return method.Invoke(target, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        private static NotSupportedException NotSupported(string member)
        {
            return new NotSupportedException(
                $"Unity's internal AudioMixer API '{member}' was not found. " +
                "This Unity version is not supported by the AudioMixer tools.");
        }
    }
}
