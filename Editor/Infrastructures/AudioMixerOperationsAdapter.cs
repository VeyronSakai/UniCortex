using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class AudioMixerOperationsAdapter : IAudioMixerOperations
    {
        private const string MixerExtension = ".mixer";
        private const char GroupPathSeparator = '/';

        public CreateAudioMixerResponse Create(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.EndsWith(MixerExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"assetPath must end with '{MixerExtension}' (got '{assetPath}').");
            }

            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            {
                throw new ArgumentException($"An asset already exists at '{assetPath}'.");
            }

            AssetUtility.EnsureParentFolderExists(assetPath);
            AudioMixerControllerReflection.CreateMixerAt(assetPath);
            AssetDatabase.SaveAssets();

            if (AssetDatabase.LoadAssetAtPath<AudioMixer>(assetPath) == null)
            {
                throw new ArgumentException($"Failed to create AudioMixer at '{assetPath}'.");
            }

            return new CreateAudioMixerResponse(true, assetPath);
        }

        public GetAudioMixerResponse Get(string assetPath)
        {
            var mixer = LoadMixer(assetPath);
            var groups = CollectGroups(mixer);

            var groupEntries = groups.Select(g => new AudioMixerGroupEntry(g.Group.name, g.Path)).ToList();

            var startSnapshot = AudioMixerControllerReflection.GetStartSnapshot(mixer);
            var snapshotEntries = AudioMixerControllerReflection.GetSnapshots(mixer)
                .Select(s => new AudioMixerSnapshotEntry(s.name, s == startSnapshot))
                .ToList();

            var parameterEntries = AudioMixerControllerReflection.GetExposedParameters(mixer)
                .Select(p => DescribeExposedParameter(p, groups))
                .ToList();

            return new GetAudioMixerResponse(groupEntries, snapshotEntries, parameterEntries);
        }

        public AddAudioMixerGroupResponse AddGroup(string assetPath, string groupName, string parentGroup)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("groupName must not be empty.");
            }

            if (groupName.IndexOf(GroupPathSeparator) >= 0)
            {
                throw new ArgumentException(
                    $"groupName must not contain '{GroupPathSeparator}' (got '{groupName}'). " +
                    "Use parentGroup to choose where the group is added.");
            }

            var mixer = LoadMixer(assetPath);
            var parent = string.IsNullOrEmpty(parentGroup)
                ? CollectGroups(mixer)[0]
                : ResolveGroup(mixer, parentGroup, assetPath);
            var parentPath = parent.Path;

            // Keep paths unique so that groups can be found again by path.
            if (AudioMixerControllerReflection.GetChildren(parent.Group).Any(c => c.name == groupName))
            {
                throw new ArgumentException($"Group '{parentPath}' already has a child group named '{groupName}'.");
            }

            Undo.RecordObjects(new UnityEngine.Object[] { mixer, parent.Group }, "Add Audio Mixer Group");
            AudioMixerControllerReflection.AddGroup(mixer, groupName, parent.Group);
            Save(mixer);

            return new AddAudioMixerGroupResponse(true, parentPath + GroupPathSeparator + groupName);
        }

        public void ExposeParameter(string assetPath, string group, string parameter, string exposedName)
        {
            if (string.IsNullOrWhiteSpace(exposedName))
            {
                throw new ArgumentException("exposedName must not be empty.");
            }

            var mixer = LoadMixer(assetPath);
            var target = ResolveGroup(mixer, group, assetPath);
            var parameterGuid = GetParameterGuid(target.Group, parameter);

            var parameters = AudioMixerControllerReflection.GetExposedParameters(mixer);
            if (parameters.Any(p => p.Name == exposedName && p.Guid != parameterGuid))
            {
                throw new ArgumentException(
                    $"Another parameter is already exposed as '{exposedName}' in AudioMixer at '{assetPath}'.");
            }

            // Group exposing and renaming so a single Undo reverts both.
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.RecordObject(mixer, "Expose Audio Mixer Parameter");

            if (parameters.All(p => p.Guid != parameterGuid))
            {
                AudioMixerControllerReflection.AddExposedParameter(mixer, target.Group, parameterGuid);
                parameters = AudioMixerControllerReflection.GetExposedParameters(mixer);
            }

            // Exposing an already exposed parameter just renames it.
            var renamed = parameters
                .Select(p => p.Guid == parameterGuid
                    ? new AudioMixerControllerReflection.ExposedParameter(p.Guid, exposedName)
                    : p)
                .ToList();
            AudioMixerControllerReflection.SetExposedParameters(mixer, renamed);

            Undo.SetCurrentGroupName("Expose Audio Mixer Parameter");
            Undo.CollapseUndoOperations(undoGroup);
            Save(mixer);
        }

        public void SetAudioSourceOutput(int instanceId, int componentIndex, string assetPath, string group)
        {
            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            var sources = go.GetComponents<AudioSource>();
            if (sources.Length == 0)
            {
                throw new ArgumentException($"GameObject '{go.name}' has no AudioSource.");
            }

            if (componentIndex < 0 || componentIndex >= sources.Length)
            {
                throw new ArgumentException(
                    $"componentIndex {componentIndex} is out of range. '{go.name}' has {sources.Length} AudioSource(s).");
            }

            var mixer = LoadMixer(assetPath);
            var target = ResolveGroup(mixer, group, assetPath);

            var source = sources[componentIndex];
            Undo.RecordObject(source, "Set AudioSource Output");
            source.outputAudioMixerGroup = target.Group;
            // Keeps the change as an override when the AudioSource belongs to a prefab instance.
            PrefabUtility.RecordPrefabInstancePropertyModifications(source);
        }

        private static AudioMixer LoadMixer(string assetPath)
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(assetPath);
            if (mixer == null)
            {
                throw new ArgumentException($"AudioMixer not found at path '{assetPath}'.");
            }

            if (!AudioMixerControllerReflection.IsEditableMixer(mixer))
            {
                throw new ArgumentException($"AudioMixer at '{assetPath}' cannot be edited.");
            }

            return mixer;
        }

        private static void Save(AudioMixer mixer)
        {
            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();
        }

        private static GUID GetParameterGuid(AudioMixerGroup group, string parameter)
        {
            if (string.Equals(parameter, AudioMixerParameters.Volume, StringComparison.OrdinalIgnoreCase))
            {
                return AudioMixerControllerReflection.GetVolumeGuid(group);
            }

            if (string.Equals(parameter, AudioMixerParameters.Pitch, StringComparison.OrdinalIgnoreCase))
            {
                return AudioMixerControllerReflection.GetPitchGuid(group);
            }

            throw new ArgumentException(
                $"Unsupported parameter '{parameter}'. Supported: {AudioMixerParameters.Volume}, {AudioMixerParameters.Pitch}.");
        }

        private static AudioMixerExposedParameterEntry DescribeExposedParameter(
            AudioMixerControllerReflection.ExposedParameter parameter, List<GroupWithPath> groups)
        {
            foreach (var g in groups)
            {
                if (AudioMixerControllerReflection.GetVolumeGuid(g.Group) == parameter.Guid)
                {
                    return new AudioMixerExposedParameterEntry(parameter.Name, g.Path, AudioMixerParameters.Volume);
                }

                if (AudioMixerControllerReflection.GetPitchGuid(g.Group) == parameter.Guid)
                {
                    return new AudioMixerExposedParameterEntry(parameter.Name, g.Path, AudioMixerParameters.Pitch);
                }
            }

            return new AudioMixerExposedParameterEntry(parameter.Name, string.Empty, AudioMixerParameters.Other);
        }

        // Accepts a full path from the master group (e.g. "Master/Music") or a group name (e.g. "Music").
        private static GroupWithPath ResolveGroup(AudioMixer mixer, string group, string assetPath)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentException("group must not be empty.");
            }

            var groups = CollectGroups(mixer);
            var byPath = groups.FirstOrDefault(g => g.Path == group);
            if (byPath.Group != null)
            {
                return byPath;
            }

            var byName = groups.Where(g => g.Group.name == group).ToList();
            if (byName.Count == 1)
            {
                return byName[0];
            }

            var available = string.Join(", ", groups.Select(g => g.Path));
            if (byName.Count == 0)
            {
                throw new ArgumentException(
                    $"Group '{group}' not found in AudioMixer at '{assetPath}'. Available groups: {available}.");
            }

            throw new ArgumentException(
                $"Group name '{group}' is ambiguous in AudioMixer at '{assetPath}'. " +
                $"Specify the path instead: {string.Join(", ", byName.Select(g => g.Path))}.");
        }

        // Lists every group depth-first from the master group, with paths like "Master/Music/BGM".
        private static List<GroupWithPath> CollectGroups(AudioMixer mixer)
        {
            var result = new List<GroupWithPath>();
            var master = AudioMixerControllerReflection.GetMasterGroup(mixer);
            if (master != null)
            {
                CollectGroups(master, master.name, result);
            }

            return result;
        }

        private static void CollectGroups(AudioMixerGroup group, string path, List<GroupWithPath> result)
        {
            result.Add(new GroupWithPath(group, path));
            foreach (var child in AudioMixerControllerReflection.GetChildren(group))
            {
                CollectGroups(child, path + GroupPathSeparator + child.name, result);
            }
        }

        private readonly struct GroupWithPath
        {
            public readonly AudioMixerGroup Group;
            public readonly string Path;

            public GroupWithPath(AudioMixerGroup group, string path)
            {
                Group = group;
                Path = path;
            }
        }
    }
}
