using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith
{
    public sealed class AppService
    {
        private readonly AppManager _manager;
        public AppService(AppManager manager)
        {
            if (manager == null) { throw new ArgumentNullException("manager"); }
            _manager = manager;
        }
        public AppManager Manager { get { return _manager; } }

        public Task<AppResponse> PutAsync(AppRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request == null) { throw new ArgumentNullException("request"); }
            return Task.FromResult(_manager.Put(request.Key, request.Value, request.Ttl));
        }

        public Task<AppResponse> GetAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_manager.Get(key));
        }

        public Task<AppResponse> DeleteAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_manager.Delete(key));
        }

        public Task<AppStatistics> GetStatsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_manager.Statistics);
        }

        public Task<object> GetHealthAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_manager.Health());
        }

        public Task<object> ListResourcesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult((object)_manager.ListResources());
        }

        public Task<object> ListNodesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult((object)_manager.ListNodes());
        }

        public void LoadSampleData(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) { return; }
            Dictionary<string, string> items = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            if (items == null) { return; }
            foreach (KeyValuePair<string, string> pair in items) { _manager.Put(pair.Key, pair.Value, null); }
        }
    }
}
