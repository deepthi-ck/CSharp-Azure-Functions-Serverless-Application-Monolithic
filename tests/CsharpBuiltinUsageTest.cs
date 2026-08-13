using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FunctionsMonolith.Functions;

namespace FunctionsMonolith.Tests
{
    public static class CsharpBuiltinUsageTest
    {
        public static int Run()
        {
            string src = Directory.GetCurrentDirectory();
            string root = src;
            if (Directory.Exists(Path.Combine(src, "src"))) { root = src; }
            else if (Directory.Exists(Path.Combine(src, "..", "src"))) { root = Path.GetFullPath(Path.Combine(src, "..")); }

            string store = File.ReadAllText(Path.Combine(root, "src", "InMemoryStore.cs"));
            string expiration = File.ReadAllText(Path.Combine(root, "src", "ExpirationManager.cs"));
            string eviction = File.ReadAllText(Path.Combine(root, "src", "EvictionManager.cs"));
            string service = File.ReadAllText(Path.Combine(root, "src", "AppService.cs"));
            string putFn = File.ReadAllText(Path.Combine(root, "src", "Functions", "PutFunction.cs"));
            string getFn = File.ReadAllText(Path.Combine(root, "src", "Functions", "GetFunction.cs"));

            Expect.True(store.Contains("ConcurrentDictionary") && store.Contains("Dictionary") && store.Contains("FirstOrDefault"), "store BCL");
            Expect.True(expiration.Contains("DateTimeOffset") && expiration.Contains("TimeSpan"), "TTL BCL");
            Expect.True(eviction.Contains("FirstOrDefault") && eviction.Contains("List"), "eviction LINQ");
            Expect.True(service.Contains("Task") && service.Contains("CancellationToken") && service.Contains("JsonSerializer"), "service async JSON");
            Expect.True(putFn.Contains("async") || putFn.Contains("Task"), "put Task");
            Expect.True(getFn.Contains("string.IsNullOrWhiteSpace"), "get key validation");

            InMemoryStore memory = new InMemoryStore();
            try
            {
                memory.Get(" ");
                throw new Exception("blank key must fail via string.IsNullOrWhiteSpace");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                memory.Get("missing");
                throw new Exception("missing key must KeyNotFoundException");
            }
            catch (KeyNotFoundException)
            {
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            memory.Put(new ResourceEntry("k", "v", now, now.Add(TimeSpan.FromMilliseconds(-1)), 1));
            Expect.True(memory.Get("k").IsExpired(DateTimeOffset.UtcNow), "DateTime TTL required");

            GetFunction get = new GetFunction(new AppService(new AppManager(AppConfiguration.CreateDefault())));
            try
            {
                get.RunAsync("", CancellationToken.None).GetAwaiter().GetResult();
                throw new Exception("empty GET key must ArgumentException");
            }
            catch (ArgumentException)
            {
            }

            return 0;
        }
    }
}
