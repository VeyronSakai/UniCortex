using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class HttpListenerRequestContext : IRequestContext
    {
        private readonly HttpListenerContext _context;

        public HttpListenerRequestContext(HttpListenerContext context)
        {
            _context = context;
            HttpMethod = (HttpMethodType)Enum.Parse(typeof(HttpMethodType), context.Request.HttpMethod, true);
        }

        public HttpMethodType HttpMethod { get; }

        public string Path => _context.Request.Url.AbsolutePath;

        public string GetQueryParameter(string name)
        {
            return _context.Request.QueryString[name];
        }

        public IReadOnlyList<KeyValuePair<string, string>> GetQueryParameters()
        {
            var parameters = new List<KeyValuePair<string, string>>();
            foreach (var key in _context.Request.QueryString.AllKeys)
            {
                if (key == null)
                {
                    continue;
                }

                parameters.Add(new KeyValuePair<string, string>(key, _context.Request.QueryString[key] ?? string.Empty));
            }

            return parameters;
        }

        public async Task<string> ReadBodyAsync()
        {
            using var reader = new StreamReader(_context.Request.InputStream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        public async Task WriteResponseAsync(int statusCode, string json)
        {
            var response = _context.Response;
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;

            try
            {
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

    }
}
