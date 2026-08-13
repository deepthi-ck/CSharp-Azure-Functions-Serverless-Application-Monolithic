using System;
using System.Threading;

namespace FunctionsMonolith.Tests
{
    public static class GetFunctionTest
    {
        public static int Run()
        {
            FunctionApp app = NewApp();
            CancellationToken token = CancellationToken.None;
            app.Put.RunAsync("product:1001", "{\"value\":\"Visvantha\"}", token).GetAwaiter().GetResult();
            AppResponse get = app.Get.RunAsync("product:1001", token).GetAwaiter().GetResult();
            Expect.Equal("Visvantha", get.Value);
            try
            {
                app.Get.RunAsync(" ", token).GetAwaiter().GetResult();
                throw new Exception("blank key must ArgumentException");
            }
            catch (ArgumentException)
            {
            }
            return 0;
        }

        internal static FunctionApp NewApp()
        {
            return new FunctionApp(
                AppConfiguration.CreateDefault(),
                new BuildContext("C#_net8.0", "8", BuildContext.DetectCompiledTfm(), "1 - Monolithic", "flat (single module)"));
        }
    }
}
