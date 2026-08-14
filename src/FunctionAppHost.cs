using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith
{
    /// <summary>
    /// Local Azure Functions HTTP test-host: isolated-worker API routes plus operator UI pages.
    /// </summary>
    public sealed class FunctionAppHost : IDisposable
    {
        private readonly FunctionApp _app;
        private readonly HttpListener _listener;
        private readonly JsonSerializerOptions _json;
        private readonly string _wwwroot;
        private CancellationTokenSource _cts;
        private Task _loop;

        public FunctionAppHost(FunctionApp app, string prefix, string wwwroot)
        {
            if (app == null) { throw new ArgumentNullException("app"); }
            if (string.IsNullOrWhiteSpace(prefix)) { throw new ArgumentException("Prefix is required.", "prefix"); }
            _app = app;
            Prefix = prefix;
            _wwwroot = wwwroot ?? string.Empty;
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public string Prefix { get; private set; }
        public bool IsListening { get { return _listener.IsListening; } }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            _loop = Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop()
        {
            if (_cts != null) { _cts.Cancel(); }
            if (_listener.IsListening) { _listener.Stop(); }
        }

        public void Dispose()
        {
            Stop();
            if (_cts != null) { _cts.Dispose(); }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext context;
                try { context = await _listener.GetContextAsync().ConfigureAwait(false); }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }

                try { await Handle(context, token).ConfigureAwait(false); }
                catch (Exception ex) { WriteJson(context.Response, 500, AppResponse.Error(ex.Message)); }
            }
        }

        private async Task Handle(HttpListenerContext context, CancellationToken token)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string path = request.Url == null ? "/" : request.Url.AbsolutePath;
            if (path.Length > 1) { path = path.TrimEnd('/'); }
            if (string.IsNullOrWhiteSpace(path)) { path = "/"; }

            try
            {
                if (IsGet(request) && string.Equals(path, "/api/health", StringComparison.OrdinalIgnoreCase))
                {
                    WriteJson(response, 200, await _app.Health.RunAsync(token).ConfigureAwait(false));
                    return;
                }

                if (IsGet(request) && string.Equals(path, "/api/version", StringComparison.OrdinalIgnoreCase))
                {
                    WriteJson(response, 200, await _app.Version.RunAsync(token).ConfigureAwait(false));
                    return;
                }

                if (IsGet(request) && string.Equals(path, "/api/stats", StringComparison.OrdinalIgnoreCase))
                {
                    AppStatistics stats = await _app.Stats.RunAsync(token).ConfigureAwait(false);
                    WriteJson(response, 200, new
                    {
                        hits = stats.Hits,
                        misses = stats.Misses,
                        puts = stats.Puts,
                        gets = stats.Gets,
                        deletes = stats.Deletes,
                        replications = stats.Replications,
                        evictions = stats.Evictions,
                        expirations = stats.Expirations,
                        entries = _app.Manager.EntryCount(),
                        nodes = _app.Manager.NodeCount()
                    });
                    return;
                }

                if (IsGet(request) && string.Equals(path, "/api/nodes", StringComparison.OrdinalIgnoreCase))
                {
                    WriteJson(response, 200, await _app.Nodes.RunAsync(token).ConfigureAwait(false));
                    return;
                }

                if (IsGet(request) && string.Equals(path, "/api/resources", StringComparison.OrdinalIgnoreCase))
                {
                    WriteJson(response, 200, await _app.List.RunAsync(token).ConfigureAwait(false));
                    return;
                }

                if (path.StartsWith("/api/resources/", StringComparison.OrdinalIgnoreCase))
                {
                    string key = Uri.UnescapeDataString(path.Substring("/api/resources/".Length));
                    if (IsGet(request))
                    {
                        AppResponse result = await _app.Get.RunAsync(key, token).ConfigureAwait(false);
                        WriteJson(response, result.Found ? 200 : 404, result);
                        return;
                    }

                    if (string.Equals(request.HttpMethod, "PUT", StringComparison.OrdinalIgnoreCase))
                    {
                        string body = await ReadBody(request).ConfigureAwait(false);
                        AppResponse result = await _app.Put.RunAsync(key, body, token).ConfigureAwait(false);
                        WriteJson(response, 200, result);
                        return;
                    }

                    if (string.Equals(request.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        AppResponse result = await _app.Delete.RunAsync(key, token).ConfigureAwait(false);
                        WriteJson(response, result.Found ? 200 : 404, result);
                        return;
                    }
                }

                if (IsGet(request) && TryServeUi(response, path))
                {
                    return;
                }

                if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    WriteJson(response, 404, AppResponse.Error("route not found"));
                    return;
                }

                WriteHtml(response, 404, "<!DOCTYPE html><html><body><p>Page not found. Return to <a href=\"/\">Dashboard</a>.</p></body></html>");
            }
            catch (ArgumentException ex)
            {
                WriteJson(response, 400, AppResponse.Error(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                WriteJson(response, 404, AppResponse.Error(ex.Message));
            }
        }

        private bool TryServeUi(HttpListenerResponse response, string path)
        {
            if (string.IsNullOrWhiteSpace(_wwwroot) || !Directory.Exists(_wwwroot)) { return false; }
            string relative = string.Equals(path, "/", StringComparison.Ordinal) ? "index.html" : path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string full = Path.GetFullPath(Path.Combine(_wwwroot, relative));
            string root = Path.GetFullPath(_wwwroot);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                root = root + Path.DirectorySeparatorChar;
            }
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                return false;
            }

            byte[] bytes = File.ReadAllBytes(full);
            response.StatusCode = 200;
            response.ContentType = ContentType(full);
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
            return true;
        }

        private static string ContentType(string full)
        {
            if (full.EndsWith(".css", StringComparison.OrdinalIgnoreCase)) { return "text/css; charset=utf-8"; }
            if (full.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) { return "application/javascript; charset=utf-8"; }
            if (full.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)) { return "image/svg+xml"; }
            return "text/html; charset=utf-8";
        }

        private void WriteJson(HttpListenerResponse response, int status, object payload)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, _json));
            response.StatusCode = status;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private static void WriteHtml(HttpListenerResponse response, int status, string html)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            response.StatusCode = status;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private static async Task<string> ReadBody(HttpListenerRequest request)
        {
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private static bool IsGet(HttpListenerRequest request)
        {
            return string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase);
        }
    }
}
