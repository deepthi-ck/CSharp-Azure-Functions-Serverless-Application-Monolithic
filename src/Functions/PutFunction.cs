using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith.Functions
{
    public sealed class PutFunction
    {
        private readonly AppService _service;
        public PutFunction(AppService service)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            _service = service;
        }

        public Task<AppResponse> RunAsync(string key, string jsonBody, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentException("Key is required.", "key"); }
            string value = jsonBody;
            if (!string.IsNullOrWhiteSpace(jsonBody) && jsonBody.TrimStart().StartsWith("{"))
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonBody))
                {
                    JsonElement root = doc.RootElement;
                    JsonElement valueElement;
                    if (root.TryGetProperty("value", out valueElement)) { value = valueElement.GetString(); }
                }
            }
            return _service.PutAsync(new AppRequest("PUT", key, value ?? string.Empty, null), cancellationToken);
        }
    }
}
