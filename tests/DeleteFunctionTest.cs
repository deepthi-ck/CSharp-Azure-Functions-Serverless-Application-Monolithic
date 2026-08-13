using System.Threading;

namespace FunctionsMonolith.Tests
{
    public static class DeleteFunctionTest
    {
        public static int Run()
        {
            FunctionApp app = GetFunctionTest.NewApp();
            CancellationToken token = CancellationToken.None;
            app.Put.RunAsync("product:1001", "{\"value\":\"Visvantha\"}", token).GetAwaiter().GetResult();
            AppResponse deleted = app.Delete.RunAsync("product:1001", token).GetAwaiter().GetResult();
            Expect.Equal("SUCCESS", deleted.Status);
            Expect.Equal("NOT_FOUND", app.Get.RunAsync("product:1001", token).GetAwaiter().GetResult().Status);
            return 0;
        }
    }
}
