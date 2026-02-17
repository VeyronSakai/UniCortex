using System;
using System.Threading;
using System.Threading.Tasks;
using EditorBridge.Editor.Domains.Models;

namespace EditorBridge.Editor.Domains.Interfaces
{
    internal interface IRequestRouter
    {
        void Register(HttpMethodType method, string path,
            Func<IRequestContext, CancellationToken, Task> handler);

        Task HandleRequestAsync(IRequestContext context, CancellationToken cancellationToken);
    }
}
