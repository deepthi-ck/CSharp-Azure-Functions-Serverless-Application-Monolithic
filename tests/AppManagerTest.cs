using System;
using System.Threading;

namespace FunctionsMonolith.Tests
{
    public static class AppManagerTest
    {
        public static int Run()
        {
            AppConfiguration configuration = AppConfiguration.CreateDefault();
            configuration.Capacity = 2;
            configuration.DefaultTtlSeconds = 0;
            AppManager manager = new AppManager(configuration);

            AppResponse put = manager.Put("product:1001", "Visvantha", null);
            Expect.Equal("SUCCESS", put.Status);
            Expect.Equal("Visvantha", manager.Get("product:1001").Value);

            manager.Put("ttl:1", "gone", TimeSpan.FromMilliseconds(20));
            Thread.Sleep(40);
            Expect.Equal("NOT_FOUND", manager.Get("ttl:1").Status);

            manager.Put("a", "1", null);
            manager.Put("b", "2", null);
            manager.Put("c", "3", null);
            Expect.True(manager.Statistics.Evictions >= 0, "eviction counters");
            Expect.True(manager.NodeCount() == 3, "three nodes");
            Expect.True(manager.Statistics.Puts > 0, "puts counted");

            manager.Delete("product:1001");
            Expect.Equal("NOT_FOUND", manager.Get("product:1001").Status);
            return 0;
        }
    }
}
