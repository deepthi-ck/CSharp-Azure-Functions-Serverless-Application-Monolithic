using System.Collections.Generic;

namespace FunctionsMonolith.Tests
{
    public static class PartitionRouterTest
    {
        public static int Run()
        {
            List<StoreNode> nodes = new List<StoreNode>
            {
                new StoreNode("node-1"),
                new StoreNode("node-2"),
                new StoreNode("node-3")
            };
            PartitionRouter router = new PartitionRouter(nodes);
            StoreNode a = router.PrimaryFor("product:1001");
            StoreNode b = router.PrimaryFor("product:1001");
            Expect.Equal(a.Id, b.Id);
            Expect.Equal(2, router.ReplicasFor("product:1001").Count);
            return 0;
        }
    }
}
