using System.Threading;

namespace FunctionsMonolith.Tests
{
    public static class AppServiceTest
    {
        public static int Run()
        {
            AppService service = new AppService(new AppManager(AppConfiguration.CreateDefault()));
            CancellationToken token = CancellationToken.None;
            AppResponse put = service.PutAsync(new AppRequest("PUT", "order:2001", "pending", null), token).GetAwaiter().GetResult();
            Expect.Equal("SUCCESS", put.Status);
            Expect.Equal("pending", service.GetAsync("order:2001", token).GetAwaiter().GetResult().Value);
            service.DeleteAsync("order:2001", token).GetAwaiter().GetResult();
            Expect.Equal("NOT_FOUND", service.GetAsync("order:2001", token).GetAwaiter().GetResult().Status);
            return 0;
        }
    }
}
