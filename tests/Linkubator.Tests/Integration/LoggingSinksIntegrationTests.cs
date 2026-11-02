using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Linkubator.Tests.Integration;

[Collection(ConsoleLoggingCollection.Name)]
public sealed class LoggingSinksIntegrationTests
{
    [Fact]
    public async Task WebApplication_EmitsRequestLogsToConsoleAndRollingFile()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"linkubator-logging-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string databasePath = Path.Combine(directory, "test.db");
        string logPath = Path.Combine(directory, "linkubator-.log");
        using StringWriter capturedConsole = new();
        TextWriter originalConsole = Console.Out;

        try
        {
            Console.SetOut(capturedConsole);
            await using (LoggingSinksWebApplicationFactory factory = new(databasePath, logPath))
            {
                using HttpClient client = factory.CreateClient();
                using HttpResponseMessage response = await client.GetAsync("/");

                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            }

            string[] logFiles = Directory.GetFiles(directory, "linkubator-*.log");
            string logFile = Assert.Single(logFiles);
            string fileOutput = await File.ReadAllTextAsync(logFile);

            Assert.Matches(@"^linkubator-\d{8}\.log$", Path.GetFileName(logFile));
            Assert.Contains("HTTP GET / responded 200", capturedConsole.ToString(), StringComparison.Ordinal);
            Assert.Contains("HTTP GET / responded 200", fileOutput, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalConsole);
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class LoggingSinksWebApplicationFactory(string databasePath, string logPath) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Linkubator"] = $"Data Source={databasePath}",
                    ["Serilog:WriteTo:1:Args:path"] = logPath
                }));
        }
    }
}
