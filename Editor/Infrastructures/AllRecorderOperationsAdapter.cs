#if UNICORTEX_RECORDER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class AllRecorderOperationsAdapter : IAllRecorderOperations
    {
        public RecorderEntry[] GetRecorderList()
        {
            var settings = RecorderControllerSettings.GetGlobalSettings();
            var allRecorders = settings.RecorderSettings.ToArray();
            var entries = new RecorderEntry[allRecorders.Length];
            for (var i = 0; i < allRecorders.Length; i++)
            {
                var recorder = allRecorders[i];
                var recorderErrors = RecorderErrorHelper.GetErrors(recorder).ToArray();

                if (recorder is MovieRecorderSettings movie)
                {
                    var outputPath = string.IsNullOrEmpty(movie.OutputFile)
                        ? string.Empty
                        : $"{movie.OutputFile}.{movie.EncoderSettings.Extension}";
                    var encoder = movie.EncoderSettings switch
                    {
                        CoreEncoderSettings => MovieRecorderEncoderType.UnityMediaEncoder,
                        ProResEncoderSettings => MovieRecorderEncoderType.ProRes,
                        GifEncoderSettings => MovieRecorderEncoderType.Gif,
                        _ => movie.EncoderSettings.GetType().Name
                    };
                    var quality = string.Empty;
                    if (movie.EncoderSettings is CoreEncoderSettings core)
                    {
                        quality = core.EncodingQuality.ToString();
                    }

                    entries[i] = new RecorderEntry(i, recorder.name, "Movie", recorder.Enabled, outputPath,
                        encoder, quality, recorderErrors);
                }
                else
                {
                    var type = recorder switch
                    {
                        ImageRecorderSettings => "ImageSequence",
                        AnimationRecorderSettings => "AnimationClip",
                        _ => recorder.GetType().Name
                    };
                    entries[i] = new RecorderEntry(i, recorder.name, type, recorder.Enabled, string.Empty,
                        string.Empty, string.Empty, recorderErrors);
                }
            }

            return entries;
        }
    }

    internal static class RecorderErrorHelper
    {
        public static List<string> GetErrors(RecorderSettings recorder)
        {
            var errors = new List<string>();
            var method = typeof(RecorderSettings).GetMethod("GetErrors",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new System.InvalidOperationException(
                    "Failed to find RecorderSettings.GetErrors via reflection.");
            }

            method.Invoke(recorder, new object[] { errors });
            return errors;
        }
    }
}
#endif
