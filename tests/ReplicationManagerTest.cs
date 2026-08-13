using System;
using System.Collections.Generic;

namespace FunctionsMonolith.Tests
{
    public static class ReplicationManagerTest
    {
        public static int Run()
        {
            StoreNode replica = new StoreNode("node-2");
            ResourceEntry entry = new ResourceEntry("product:1001", "Visvantha", DateTimeOffset.UtcNow, null, 1);
            ReplicationManager replication = new ReplicationManager();
            int copied = replication.ReplicatePut(entry, new List<StoreNode> { replica });
            Expect.Equal(1, copied);
            Expect.Equal("Visvantha", replica.Store.Get("product:1001").Value);
            replication.ReplicateDelete("product:1001", new List<StoreNode> { replica });
            Expect.True(!replica.Store.Contains("product:1001"), "replica delete");
            return 0;
        }
    }
}
