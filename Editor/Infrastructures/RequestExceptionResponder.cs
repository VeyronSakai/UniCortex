using System;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal static class RequestExceptionResponder
    {
        public static Task RespondAsync(IRequestContext context, Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                return context.WriteResponseAsync(HttpStatusCodes.RequestTimeout,
                    JsonUtility.ToJson(new ErrorResponse(ErrorMessages.RequestWasCancelled)));
            }

            return context.WriteResponseAsync(HttpStatusCodes.InternalServerError,
                JsonUtility.ToJson(new ErrorResponse("Internal server error")));
        }
    }
}
