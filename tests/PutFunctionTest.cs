using System;
using System.Threading;

namespace FunctionsMonolith.Tests
{
    public static class PutFunctionTest
    {
        public static int Run()
        {
            FunctionApp app = GetFunctionTest.NewApp();
            CancellationToken token = CancellationToken.None;
            AppResponse put = app.Put.RunAsync("product:1001", "{\"value\":\"Visvantha\"}", token).GetAwaiter().GetResult();
            Expect.Equal("SUCCESS", put.Status);
            Expect.Equal("Visvantha", app.Get.RunAsync("product:1001", token).GetAwaiter().GetResult().Value);
            try
            {
                app.Put.RunAsync("", "{\"value\":\"x\"}", token).GetAwaiter().GetResult();
                throw new Exception("empty PUT key must ArgumentException");
            }
            catch (ArgumentException)
            {
            }
            return 0;
        }
    }
}
