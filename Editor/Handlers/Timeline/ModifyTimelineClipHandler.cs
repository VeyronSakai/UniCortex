using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.UseCases;
using UnityEngine;

namespace UniCortex.Editor.Handlers.Timeline
{
    internal sealed class ModifyTimelineClipHandler
    {
        private readonly ModifyTimelineClipUseCase _useCase;

        public ModifyTimelineClipHandler(ModifyTimelineClipUseCase useCase)
        {
            _useCase = useCase;
        }

        public void Register(IRequestRouter router)
        {
            router.Register(HttpMethodType.Post, ApiRoutes.TimelineModifyClip, HandleAsync);
        }

        // JsonUtility does not support Nullable<T>, so use a non-nullable helper for deserialization.
        // Field presence is detected from the JSON body; this class is only used for value extraction.
        [Serializable]
        private class RawModifyTimelineClipRequest
        {
            public int instanceId;
            public int trackIndex;
            public int clipIndex;
            public string displayName;
            public double start;
            public double duration;
            public double timeScale;
            public double clipIn;
            public double easeInDuration;
            public double easeOutDuration;
            public string preExtrapolation;
            public string postExtrapolation;
        }

        private async Task HandleAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var body = await context.ReadBodyAsync();

            if (string.IsNullOrEmpty(body))
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(ModifyTimelineClipRequest.instanceId)}, {nameof(ModifyTimelineClipRequest.trackIndex)}, and {nameof(ModifyTimelineClipRequest.clipIndex)} are required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var raw = JsonUtility.FromJson<RawModifyTimelineClipRequest>(body);

            if (raw.instanceId == 0)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(
                    $"{nameof(ModifyTimelineClipRequest.instanceId)} is required."));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var request = new ModifyTimelineClipRequest
            {
                instanceId = raw.instanceId,
                trackIndex = raw.trackIndex,
                clipIndex = raw.clipIndex,
                displayName = HasField(body, nameof(raw.displayName)) ? raw.displayName : null,
                start = HasField(body, nameof(raw.start)) ? raw.start : null,
                duration = HasField(body, nameof(raw.duration)) ? raw.duration : null,
                timeScale = HasField(body, nameof(raw.timeScale)) ? raw.timeScale : null,
                clipIn = HasField(body, nameof(raw.clipIn)) ? raw.clipIn : null,
                easeInDuration = HasField(body, nameof(raw.easeInDuration)) ? raw.easeInDuration : null,
                easeOutDuration = HasField(body, nameof(raw.easeOutDuration)) ? raw.easeOutDuration : null,
                preExtrapolation = HasField(body, nameof(raw.preExtrapolation)) ? raw.preExtrapolation : null,
                postExtrapolation = HasField(body, nameof(raw.postExtrapolation)) ? raw.postExtrapolation : null
            };

            try
            {
                await _useCase.ExecuteAsync(request, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                var errorJson = JsonUtility.ToJson(new ErrorResponse(ex.Message));
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest, errorJson);
                return;
            }

            var json = JsonUtility.ToJson(new ModifyTimelineClipResponse(true));
            await context.WriteResponseAsync(HttpStatusCodes.Ok, json);
        }

        private static bool HasField(string body, string fieldName)
        {
            return Regex.IsMatch(body, $"\"{fieldName}\"\\s*:");
        }
    }
}
