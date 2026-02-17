using System.IO;
using System.Net;
using System.Text;
using EditorBridge.Editor.Domains.Interfaces;

namespace EditorBridge.Editor.Infrastructures
{
    internal sealed class HttpListenerRequestContext : IRequestContext
    {
        private readonly HttpListenerContext _context;

        public HttpListenerRequestContext(HttpListenerContext context)
        {
            _context = context;
        }

        public string HttpMethod => _context.Request.HttpMethod;

        public string Path => _context.Request.Url.AbsolutePath;

        public string ReadBody()
        {
            using var reader = new StreamReader(_context.Request.InputStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public void WriteResponse(int statusCode, string json)
        {
            var response = _context.Response;
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            try
            {
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                // client may have disconnected during write
            }
            finally
            {
                try
                {
                    response.OutputStream.Close();
                }
                catch
                {
                    // client may have already disconnected
                }
            }
        }
    }
}
