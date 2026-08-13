using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith.Functions
{
    public sealed class VersionFunction
    {
        private readonly BuildContext _build;
        public VersionFunction(BuildContext build)
        {
            if (build == null) { throw new ArgumentNullException("build"); }
            _build = build;
        }
        public Task<VersionInfo> RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_build.ToVersionInfo());
        }
    }
}
