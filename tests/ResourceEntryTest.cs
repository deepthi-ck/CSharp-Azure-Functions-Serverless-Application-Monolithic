using System;

namespace FunctionsMonolith.Tests
{
    public static class ResourceEntryTest
    {
        public static int Run()
        {
            DateTimeOffset created = DateTimeOffset.UtcNow;
            ResourceEntry entry = new ResourceEntry("product:1001", "Visvantha", created, created.Add(TimeSpan.FromMilliseconds(1)), 1);
            Expect.Equal("product:1001", entry.Key);
            Expect.True(entry.IsExpired(created.AddSeconds(1)), "TTL DateTime comparison must expire the entry");
            try
            {
                new ResourceEntry("  ", "x", created, null, 1);
                throw new Exception("empty key must throw");
            }
            catch (ArgumentException)
            {
            }
            return 0;
        }
    }
}
