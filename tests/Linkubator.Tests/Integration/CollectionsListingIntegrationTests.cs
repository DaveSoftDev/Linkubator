using Dapper;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using TestHost = Microsoft.AspNetCore.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class CollectionsListingIntegrationTests
{
    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";

    [Fact]
    public async Task GetCollections_WhenNoCollectionsExist_RendersAccessibleEmptyState()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=no-matching-collection");
            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("action=\"/app/colecciones\"", html, StringComparison.Ordinal);
            Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
            Assert.Contains("No se han encontrado colecciones.", System.Net.WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenSearchAndPageAreProvided_RendersFilteredPagedResults()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Alpha", "Alpine", "Beta"]);

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=Alpha&p=1");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("value=\"Alpha\"", html, StringComparison.Ordinal);
            string listingHtml = GetCollectionListHtml(html);
            Assert.Contains("Alpha", listingHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Alpine", listingHtml, StringComparison.Ordinal);
            Assert.DoesNotContain(">Beta<", listingHtml, StringComparison.Ordinal);
            Assert.Contains("aria-current=\"page\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenPaginationIsAvailable_RendersLinksThatPreserveSearch()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, Enumerable.Range(1, 18).Select(index => $"Collection {index:D2}").ToArray());

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=Collection");
            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("/app/colecciones?q=Collection&amp;p=2", html, StringComparison.Ordinal);
            Assert.Contains("Siguiente", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenOtherUsersCollectionExists_DoesNotExposeItOrAcceptUserIdOverride()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            await SeedOtherUsersCollection(databasePath);

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?userId=2");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            string listingHtml = GetCollectionListHtml(html);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Owned collection", listingHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Foreign collection", listingHtml, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenPageIsInvalid_ShowsFirstPageWithoutError()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["First page collection"]);

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?p=0");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            string listingHtml = GetCollectionListHtml(html);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("First page collection", listingHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenQueryIsInvalid_ShowsValidationErrorWithoutEmptyState()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=" + new string('x', 101));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("El texto de búsqueda supera la longitud máxima permitida.", html, StringComparison.Ordinal);
            Assert.DoesNotContain("No se han encontrado colecciones.", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static string GetCollectionListHtml(string html)
    {
        int start = html.IndexOf("<table", StringComparison.Ordinal);
        if (start < 0)
        {
            return string.Empty;
        }

        int end = html.IndexOf("</table>", start, StringComparison.Ordinal);
        return end < 0 ? string.Empty : html[start..(end + "</table>".Length)];
    }

    private static async Task SeedOtherUsersCollection(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        long userId = await connection.ExecuteScalarAsync<long>(
            "INSERT INTO User (Email, Name, Alias, Password, EmailConfirmedAt, SecurityStamp) VALUES (@Email, @Name, @Alias, @Password, @ConfirmedAt, @Stamp); SELECT last_insert_rowid();",
            new
            {
                Email = "other-user@linkubator.test",
                Name = "Other User",
                Alias = "other-demo-user",
                Password = "not-a-real-password-hash",
                ConfirmedAt = DateTime.UtcNow,
                Stamp = Guid.NewGuid().ToString("N"),
            });
        long collectionId = await connection.ExecuteScalarAsync<long>(
            "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 0); SELECT last_insert_rowid();",
            new { UserId = userId, Name = "Foreign collection", Slug = "foreign-collection" });
        await connection.ExecuteAsync(
            "INSERT INTO CollectionSearch (rowid, UserId, Name) VALUES (@Id, @UserId, @Name);",
            new { Id = collectionId, UserId = userId, Name = "Foreign collection" });
    }

    private static async Task SeedCollections(string databasePath, string email, IReadOnlyList<string> names)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        long userId = await connection.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = email });
        for (int index = 0; index < names.Count; index++)
        {
            long collectionId = await connection.ExecuteScalarAsync<long>(
                "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 0); SELECT last_insert_rowid();",
                new { UserId = userId, Name = names[index], Slug = $"test-collection-{index + 1}" });
            await connection.ExecuteAsync(
                "INSERT INTO CollectionSearch (rowid, UserId, Name) VALUES (@Id, @UserId, @Name);",
                new { Id = collectionId, UserId = userId, Name = names[index] });
        }
    }

    private static string NewDatabasePath() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (string filePath in new[] { databasePath, $"{databasePath}-wal", $"{databasePath}-shm" })
        {
            if (!File.Exists(filePath))
            {
                continue;
            }

            try
            {
                File.Delete(filePath);
            }
            catch (IOException)
            {
                // SQLite can release temporary database files after the host shuts down.
            }
        }
    }

    private sealed class ConfiguredWebApplicationFactory(string databasePath) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(TestHost.IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
        }
    }
}
