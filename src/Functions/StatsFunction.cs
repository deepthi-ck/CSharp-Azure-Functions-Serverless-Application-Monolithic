using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith.Functions
{
    public sealed class StatsFunction
    {
        private readonly AppService _service;
        public StatsFunction(AppService service)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            _service = service;
        }
        public Task<AppStatistics> RunAsync(CancellationToken cancellationToken)
        {
            return _service.GetStatsAsync(cancellationToken);
        }
    }
}
