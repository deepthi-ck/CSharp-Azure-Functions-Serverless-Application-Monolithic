using System;
using FunctionsMonolith.Functions;

namespace FunctionsMonolith
{
    /// <summary>
    /// Isolated-worker composition root: registers shared services and HTTP function classes.
    /// Functions Core Tools are not required; FunctionAppHost is the local test-host equivalent.
    /// </summary>
    public sealed class FunctionApp
    {
        public FunctionApp(AppConfiguration configuration, BuildContext build)
        {
            if (configuration == null) { throw new ArgumentNullException("configuration"); }
            if (build == null) { throw new ArgumentNullException("build"); }
            Configuration = configuration;
            Build = build;
            Manager = new AppManager(configuration);
            Service = new AppService(Manager);
            Get = new GetFunction(Service);
            Put = new PutFunction(Service);
            Delete = new DeleteFunction(Service);
            List = new ListFunction(Service);
            Nodes = new NodesFunction(Service);
            Health = new HealthFunction(Service);
            Version = new VersionFunction(build);
            Stats = new StatsFunction(Service);
        }

        public AppConfiguration Configuration { get; private set; }
        public BuildContext Build { get; private set; }
        public AppManager Manager { get; private set; }
        public AppService Service { get; private set; }
        public GetFunction Get { get; private set; }
        public PutFunction Put { get; private set; }
        public DeleteFunction Delete { get; private set; }
        public ListFunction List { get; private set; }
        public NodesFunction Nodes { get; private set; }
        public HealthFunction Health { get; private set; }
        public VersionFunction Version { get; private set; }
        public StatsFunction Stats { get; private set; }
    }
}
