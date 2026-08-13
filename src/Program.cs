using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionsMonolith
{
    /// <summary>
    /// Isolated worker Function App host bootstrap (Program.cs).
    /// Uses a local HTTP test-host equivalent when Azure Functions Core Tools are not installed.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            try { return Run(args).GetAwaiter().GetResult(); }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<int> Run(string[] args)
        {
            BuildContext build = BuildContext.FromAssembly();
            build.ValidateMonolithSameVersion();

            AppConfiguration configuration = LoadConfiguration();
            int portOverride;
            string portEnv = Environment.GetEnvironmentVariable("APP_PORT");
            if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out portOverride))
            {
                configuration.Port = portOverride;
            }

            string prefix = "http://" + configuration.Host + ":" + configuration.Port + "/";

            if (HasFlag(args, "--client-e2e"))
            {
                return await SelfTest(prefix).ConfigureAwait(false);
            }

            FunctionApp app = new FunctionApp(configuration, build);
            app.Service.LoadSampleData(FindFile("data", "sample-app-data.json"));

            using (FunctionAppHost host = new FunctionAppHost(app, prefix))
            {
                host.Start();
                Console.WriteLine("C# Azure Functions Serverless Application (Scenario 1 - Monolithic)");
                Console.WriteLine("FUNCTIONS_WORKER_RUNTIME: dotnet-isolated");
                Console.WriteLine("Branch: " + build.Branch);
                Console.WriteLine("Customer Version: " + build.CustomerVersion);
                Console.WriteLine("TFM: " + build.TargetFramework);
                Console.WriteLine("Listening: " + prefix);

                if (HasFlag(args, "--self-test"))
                {
                    int code = await SelfTest(prefix).ConfigureAwait(false);
                    host.Stop();
                    return code;
                }

                if (HasFlag(args, "--once"))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    host.Stop();
                    return 0;
                }

                Console.WriteLine("Press Ctrl+C to stop.");
                ManualResetEventSlim done = new ManualResetEventSlim(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    done.Set();
                };
                done.Wait();
                host.Stop();
                return 0;
            }
        }

        private static async Task<int> SelfTest(string prefix)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string resource = prefix.TrimEnd('/') + "/api/resources/product:1001";

                HttpResponseMessage put = await client.PutAsync(resource, new StringContent("{\"value\":\"Visvantha\"}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
                string putBody = await put.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!put.IsSuccessStatusCode || putBody.IndexOf("SUCCESS", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("PUT failed: " + putBody);
                    return 1;
                }

                HttpResponseMessage get = await client.GetAsync(resource).ConfigureAwait(false);
                string getBody = await get.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!get.IsSuccessStatusCode || getBody.IndexOf("Visvantha", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("GET failed: " + getBody);
                    return 1;
                }

                await client.DeleteAsync(resource).ConfigureAwait(false);
                HttpResponseMessage missing = await client.GetAsync(resource).ConfigureAwait(false);
                string missingBody = await missing.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (missingBody.IndexOf("NOT_FOUND", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("DELETE failed: " + missingBody);
                    return 1;
                }

                Console.WriteLine("Self-test PASS");
                return 0;
            }
        }

        private static AppConfiguration LoadConfiguration()
        {
            string path = FindFile("config", "appsettings.json");
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText(path)) ?? AppConfiguration.CreateDefault();
            }
            return AppConfiguration.CreateDefault();
        }

        private static string FindFile(string folder, string name)
        {
            string[] roots = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "src")
            };
            foreach (string root in roots)
            {
                string candidate = Path.Combine(root, folder, name);
                if (File.Exists(candidate)) { return candidate; }
            }
            return Path.Combine(Directory.GetCurrentDirectory(), folder, name);
        }

        private static bool HasFlag(string[] args, string flag)
        {
            if (args == null) { return false; }
            foreach (string arg in args)
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
            return false;
        }
    }
}
