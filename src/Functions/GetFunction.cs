using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith.Functions
{
    public sealed class GetFunction
    {
        private readonly AppService _service;
        public GetFunction(AppService service)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            _service = service;
        }

        public Task<AppResponse> RunAsync(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentException("Key is required.", "key"); }
            return _service.GetAsync(key, cancellationToken);
        }
    }
}
