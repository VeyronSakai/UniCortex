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
            // The server's only cancellation source is its stop token,
            // so a cancelled request means the server is going away (e.g. for a domain reload).
            if (exception is OperationCanceledException)
            {
                return context.WriteResponseAsync(HttpStatusCodes.ServiceUnavailable,
                    JsonUtility.ToJson(new ErrorResponse(ErrorMessages.ServerStopped)));
            }

            return context.WriteResponseAsync(HttpStatusCodes.InternalServerError,
                JsonUtility.ToJson(new ErrorResponse("Internal server error")));
        }
    }
}
