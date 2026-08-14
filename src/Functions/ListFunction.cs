using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith.Functions
{
    public sealed class ListFunction
    {
        private readonly AppService _service;
        public ListFunction(AppService service)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            _service = service;
        }
        public Task<object> RunAsync(CancellationToken cancellationToken)
        {
            return _service.ListResourcesAsync(cancellationToken);
        }
    }
}
