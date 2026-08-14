using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace FunctionsMonolith.Tests
{
    public static class UiNavigationTest
    {
        public static int Run()
        {
            AppConfiguration configuration = AppConfiguration.CreateDefault();
            configuration.Port = 18083;
            FunctionApp app = new FunctionApp(
                configuration,
                new BuildContext("C#_net8.0", "8", BuildContext.DetectCompiledTfm(), "1 - Monolithic", "flat (single module)"));
            string sample = Path.Combine(FindRoot(), "data", "sample-app-data.json");
            app.Service.LoadSampleData(sample);

            string prefix = "http://127.0.0.1:18083/";
            using (FunctionAppHost host = new FunctionAppHost(app, prefix, FindWwwroot()))
            {
                host.Start();
                Thread.Sleep(250);
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(8);
                    string[] pages = new[] { "/", "/resources.html", "/stats.html", "/nodes.html", "/health.html", "/version.html" };
                    for (int i = 0; i < pages.Length; i++)
                    {
                        string html = client.GetStringAsync(prefix.TrimEnd('/') + pages[i]).GetAwaiter().GetResult();
                        Expect.True(html.IndexOf("Azure Functions", StringComparison.Ordinal) >= 0, "title " + pages[i]);
                        Expect.True(html.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) >= 0, "nav " + pages[i]);
                        Expect.True(html.IndexOf("id=\"sidebar\"", StringComparison.Ordinal) >= 0, "sidebar landmark " + pages[i]);
                    }

                    string resources = client.GetStringAsync(prefix.TrimEnd('/') + "/resources.html").GetAwaiter().GetResult();
                    Expect.True(resources.IndexOf("product:1001", StringComparison.Ordinal) >= 0, "key placeholder");
                    Expect.True(resources.IndexOf("Visvantha", StringComparison.Ordinal) >= 0, "value placeholder");
                    Expect.True(resources.IndexOf("placeholder=\"Name\"", StringComparison.OrdinalIgnoreCase) < 0, "no default Name placeholder");

                    string css = client.GetStringAsync(prefix.TrimEnd('/') + "/css/app.css").GetAwaiter().GetResult();
                    Expect.True(css.IndexOf(".sidebar", StringComparison.Ordinal) >= 0, "css");
                    string js = client.GetStringAsync(prefix.TrimEnd('/') + "/js/app.js").GetAwaiter().GetResult();
                    Expect.True(js.IndexOf("FunctionsUi", StringComparison.Ordinal) >= 0, "js");

                    string list = client.GetStringAsync(prefix.TrimEnd('/') + "/api/resources").GetAwaiter().GetResult();
                    Expect.True(list.IndexOf("product:1001", StringComparison.Ordinal) >= 0, "list sample data");
                }
            }
            return 0;
        }

        private static string FindRoot()
        {
            string cwd = Directory.GetCurrentDirectory();
            if (Directory.Exists(Path.Combine(cwd, "src"))) { return cwd; }
            if (Directory.Exists(Path.Combine(cwd, "..", "src"))) { return Path.GetFullPath(Path.Combine(cwd, "..")); }
            return cwd;
        }

        private static string FindWwwroot()
        {
            string[] candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                Path.Combine(FindRoot(), "src", "wwwroot")
            };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (Directory.Exists(candidates[i])) { return candidates[i]; }
            }
            return Path.Combine(FindRoot(), "src", "wwwroot");
        }
    }
}
