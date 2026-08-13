using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionsMonolith
{
    public sealed class PartitionRouter
    {
        private readonly IList<StoreNode> _nodes;
        public PartitionRouter(IList<StoreNode> nodes)
        {
            if (nodes == null) { throw new ArgumentNullException("nodes"); }
            if (!nodes.Any()) { throw new ArgumentException("At least one node is required.", "nodes"); }
            _nodes = nodes;
        }
        public StoreNode PrimaryFor(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentException("Key is required.", "key"); }
            return _nodes[Math.Abs(key.GetHashCode()) % _nodes.Count];
        }
        public IList<StoreNode> ReplicasFor(string key)
        {
            StoreNode primary = PrimaryFor(key);
            return _nodes.Where(n => !string.Equals(n.Id, primary.Id, StringComparison.Ordinal)).ToList();
        }
        public IList<StoreNode> AllNodes() { return _nodes.ToList(); }
    }
}
