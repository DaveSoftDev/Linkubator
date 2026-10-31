using Dapper;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using TestHost = Microsoft.AspNetCore.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class CollectionsListingIntegrationTests
{
    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";

    [Theory]
    [InlineData(false, "Privada")]
    [InlineData(true, "Pública")]
    public async Task GetCollections_WithCollection_RendersCompactListWithDetailsAndManagementActions(bool isPublic, string status)
    {
        string databasePath = NewDatabasePath();
        const string collectionName = "Ideas <favoritas>";
        const string description = "Recursos <importantes>";
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, [collectionName]);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            long collectionId = await connection.QuerySingleAsync<long>(
                "UPDATE Collection SET Description = @Description, IsPublic = @IsPublic WHERE Name = @Name RETURNING Id;",
                new { Description = description, IsPublic = isPublic, Name = collectionName });

            string html = await client.GetStringAsync("/app/colecciones");
            string listingHtml = GetCollectionListHtml(html);

            Assert.Contains("links-grid links-grid--list collections-list", listingHtml, StringComparison.Ordinal);
            Assert.Contains("Ideas &lt;favoritas&gt;", listingHtml, StringComparison.Ordinal);
            Assert.Contains("Recursos &lt;importantes&gt;", listingHtml, StringComparison.Ordinal);
            Assert.Contains(status, System.Net.WebUtility.HtmlDecode(listingHtml), StringComparison.Ordinal);
            Assert.Contains("0 enlaces", listingHtml, StringComparison.Ordinal);
            Assert.Contains($"href=\"/app/colecciones/{collectionId}/editar\"", listingHtml, StringComparison.Ordinal);
            Assert.Contains($"href=\"/app/colecciones/{collectionId}/eliminar\"", listingHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("<table", html, StringComparison.Ordinal);
            Assert.Matches("href=\"/css/collection-list(?:\\.[a-z0-9]+)?\\.css", html);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

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
            Assert.Matches("<a class=\"view-switch__button toolbar-tool\" href=\"/app/colecciones/nueva\">\\s*<svg[^>]*aria-hidden=\"true\"[^>]*><path d=\"M12 5V19M5 12H19\"></path></svg>\\s*Crear colección\\s*</a>", System.Net.WebUtility.HtmlDecode(html));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenSearchIsProvided_FiltersResultsAndPreservesSearchText()
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
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenDescriptionMatchesSearch_RendersMatchingCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Recipes", "Unrelated"]);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            await connection.ExecuteAsync(
                @"UPDATE Collection SET Description = @Description WHERE Name = @Name;
                  UPDATE CollectionSearch SET Description = @Description WHERE rowid = (SELECT Id FROM Collection WHERE Name = @Name);",
                new { Name = "Recipes", Description = "Canción favorita" });

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=canc");
            string listingHtml = GetCollectionListHtml(System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Recipes", listingHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Unrelated", listingHtml, StringComparison.Ordinal);
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
            long foreignId = await SeedOtherUsersCollection(databasePath);
            await using SqliteConnection ownerConnection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            long foreignUserId = await ownerConnection.ExecuteScalarAsync<long>("SELECT UserId FROM Collection WHERE Id = @Id;", new { Id = foreignId });

            using HttpResponseMessage response = await client.GetAsync($"/app/colecciones?userId={foreignUserId}&c=not-a-collection-id");
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

    [Theory]
    [InlineData("abc", 1)]
    [InlineData("0", 1)]
    [InlineData("-1", 1)]
    [InlineData("abc&p=2", 1)]
    [InlineData("000000000000000000000000000002", 2)]
    [InlineData("999", 2)]
    [InlineData("2147483648", 2)]
    [InlineData("999999999999999999999999999", 2)]
    public async Task GetCollections_WhenPageIsInvalidOrOutOfRange_UsesExpectedFilteredPage(string page, int expectedPage)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, Enumerable.Range(1, 18).Select(index => $"Collection {index:D2}").Append("Unrelated").ToArray());
            using HttpResponseMessage baselineResponse = await client.GetAsync($"/app/colecciones?q=Collection&p={expectedPage}");
            string baseline = System.Net.WebUtility.HtmlDecode(await baselineResponse.Content.ReadAsStringAsync());

            using HttpResponseMessage response = await client.GetAsync($"/app/colecciones?q=Collection&p={page}");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            string listingHtml = GetCollectionListHtml(html);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedPage == 1 ? 16 : 2, System.Text.RegularExpressions.Regex.Matches(listingHtml, "<li class=\"link-card\">").Count);
            Assert.Equal(GetCollectionListHtml(baseline), listingHtml);
            Assert.Equal(expectedPage == 1 ? Enumerable.Range(1, 16).Select(index => $"Collection {index:D2}") : new[] { "Collection 17", "Collection 18" },
                System.Text.RegularExpressions.Regex.Matches(listingHtml, "aria-label=\"Colecci\u00f3n: (Collection [0-9]{2})\"").Select(match => match.Groups[1].Value));
            Assert.Contains("value=\"Collection\"", html, StringComparison.Ordinal);
            Assert.Contains($"aria-current=\"page\">{expectedPage}</span>", html, StringComparison.Ordinal);
            Assert.Contains("/app/colecciones?q=Collection", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollections_WhenPageExceedsEmptySearch_ShowsFilteredEmptyState()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Collection"]);

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=inexistente&p=999");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("No se han encontrado colecciones.", html, StringComparison.Ordinal);
            Assert.Contains("value=\"inexistente\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollectionOperations_WhenIdIsInvalid_ReturnsValidationWithoutExposingCollectionData()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);

            using HttpResponseMessage editResponse = await client.GetAsync("/app/colecciones/not-an-id/editar");
            string editHtml = System.Net.WebUtility.HtmlDecode(await editResponse.Content.ReadAsStringAsync());
            using HttpResponseMessage deleteResponse = await client.GetAsync("/app/colecciones/not-an-id/eliminar");
            string deleteHtml = System.Net.WebUtility.HtmlDecode(await deleteResponse.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, editResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, deleteResponse.StatusCode);
            Assert.Contains("La colección es obligatoria.", editHtml, StringComparison.Ordinal);
            Assert.Contains("La colección es obligatoria.", deleteHtml, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/colecciones\">Volver al listado</a>", editHtml, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/colecciones\">Volver al listado</a>", deleteHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"Input.Name\"", editHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"collection-form collection-form--danger\"", deleteHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Owned collection", editHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Owned collection", deleteHtml, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetCollectionOperations_WhenCollectionIsMissingOrForeign_ReturnsNonDisclosingResponses()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            long foreignCollectionId = await SeedOtherUsersCollection(databasePath);
            long missingCollectionId = foreignCollectionId + 1000;

            using HttpResponseMessage missingEditResponse = await client.GetAsync($"/app/colecciones/{missingCollectionId}/editar");
            string missingEditHtml = System.Net.WebUtility.HtmlDecode(await missingEditResponse.Content.ReadAsStringAsync());
            using HttpResponseMessage foreignEditResponse = await client.GetAsync($"/app/colecciones/{foreignCollectionId}/editar");
            string foreignEditHtml = System.Net.WebUtility.HtmlDecode(await foreignEditResponse.Content.ReadAsStringAsync());
            using HttpResponseMessage missingDeleteResponse = await client.GetAsync($"/app/colecciones/{missingCollectionId}/eliminar");
            string missingDeleteHtml = System.Net.WebUtility.HtmlDecode(await missingDeleteResponse.Content.ReadAsStringAsync());
            using HttpResponseMessage foreignDeleteResponse = await client.GetAsync($"/app/colecciones/{foreignCollectionId}/eliminar");
            string foreignDeleteHtml = System.Net.WebUtility.HtmlDecode(await foreignDeleteResponse.Content.ReadAsStringAsync());

            Assert.Equal(missingEditResponse.StatusCode, foreignEditResponse.StatusCode);
            Assert.Equal(NormalizeAntiforgeryTokens(missingEditHtml), NormalizeAntiforgeryTokens(foreignEditHtml));
            Assert.Contains("No se ha podido acceder al elemento solicitado.", missingEditHtml, StringComparison.Ordinal);
            Assert.Contains("No se ha podido acceder al elemento solicitado.", foreignEditHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Foreign collection", missingEditHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Foreign collection", foreignEditHtml, StringComparison.Ordinal);
            Assert.Equal(missingDeleteResponse.StatusCode, foreignDeleteResponse.StatusCode);
            Assert.Equal(missingDeleteHtml, foreignDeleteHtml);
            Assert.Contains("No se ha podido eliminar la colección", missingDeleteHtml, StringComparison.Ordinal);
            Assert.Contains("No se ha podido eliminar la colección", foreignDeleteHtml, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/colecciones\"", missingDeleteHtml, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/colecciones\"", foreignDeleteHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Foreign collection", missingDeleteHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Foreign collection", foreignDeleteHtml, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task DeleteCollection_WhenIdIsInvalid_ReturnsValidationAndLeavesCollectionsUnchanged()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            long ownedCollectionId = await GetCollectionId(databasePath, DemoEmail, "Owned collection");
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);
            string token = await GetAntiforgeryToken(client, ownedCollectionId);

            string before = await CollectionSnapshot(databasePath);

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/not-an-id/eliminar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = "not-an-id",
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(before, await CollectionSnapshot(databasePath));
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("La colección es obligatoria.", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/colecciones\">Volver al listado</a>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"collection-form collection-form--danger\"", html, StringComparison.Ordinal);
            Assert.Equal(initialCount, finalCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task DeleteCollection_WhenIdIsMissingOrForeign_RedirectsWithSameNoticeAndDoesNotChangeData()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            long ownedCollectionId = await GetCollectionId(databasePath, DemoEmail, "Owned collection");
            long foreignCollectionId = await SeedOtherUsersCollection(databasePath);
            long missingCollectionId = foreignCollectionId + 1000;
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);
            long initialForeignCount = await GetCollectionCount(databasePath, "other-user@linkubator.test");
            string token = await GetAntiforgeryToken(client, ownedCollectionId);

            using HttpResponseMessage missingResponse = await PostDelete(client, missingCollectionId, token);
            using HttpResponseMessage foreignResponse = await PostDelete(client, foreignCollectionId, token);
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);
            long finalForeignCount = await GetCollectionCount(databasePath, "other-user@linkubator.test");
            using HttpResponseMessage noticeResponse = await client.GetAsync("/app/colecciones");
            string noticeHtml = System.Net.WebUtility.HtmlDecode(await noticeResponse.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.Redirect, missingResponse.StatusCode);
            Assert.Equal("/app/colecciones", missingResponse.Headers.Location?.OriginalString);
            Assert.Equal(missingResponse.StatusCode, foreignResponse.StatusCode);
            Assert.Equal(missingResponse.Headers.Location?.OriginalString, foreignResponse.Headers.Location?.OriginalString);
            Assert.Equal(initialCount, finalCount);
            Assert.Equal(initialForeignCount, finalForeignCount);
            Assert.Equal(System.Net.HttpStatusCode.OK, noticeResponse.StatusCode);
            Assert.Contains("No se ha podido eliminar la colección", noticeHtml, StringComparison.Ordinal);
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

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?p=abc&q=" + new string('x', 101));
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

    [Fact]
    public async Task PostCreateCollection_WhenNameIsRequired_AssociatesErrorWithoutWriting()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            string token = await GetAntiforgeryToken(client, "/app/colecciones/nueva");

            string before = await CollectionSnapshot(databasePath);

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/nueva",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Input.Name"] = string.Empty,
                    ["Input.Description"] = "Texto de prueba",
                    ["Input.IsPublic"] = "true",
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("El nombre es obligatorio.", html, StringComparison.Ordinal);
            Assert.Contains("class=\"input-validation-error\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-invalid=\"true\"", html, StringComparison.Ordinal);
            Assert.Matches("<input(?=[^>]*id=\"collection-name\")(?=[^>]*class=\"[^\"]*input-validation-error)[^>]*>", html);
            Assert.Contains("data-valmsg-for=\"Input.Name\"", html, StringComparison.Ordinal);
            Assert.Equal(before, await CollectionSnapshot(databasePath));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostDeleteCollection_WhenCollectionHasLinks_ShowsDomainRuleErrorAndKeepsState()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Owned collection");
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                await connection.ExecuteAsync(
                    "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, @Description, 0, @CreatedAt);",
                    new
                    {
                        UserId = await connection.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail }),
                        CollectionId = collectionId,
                        Url = "https://collection-link.example",
                        Title = "Example collection link",
                        Description = "Seeded link for deletion rejection",
                        CreatedAt = DateTime.UtcNow,
                    });
            }

            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{collectionId}/eliminar");
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);
            await using SqliteConnection observer = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            string originalLink = await observer.QuerySingleAsync<string>("SELECT json_object('Id', Id, 'CollectionId', CollectionId, 'Title', Title, 'IsPublic', IsPublic) FROM Link WHERE CollectionId = @Id;", new { Id = collectionId });

            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{collectionId}/eliminar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("No se puede eliminar una colección con enlaces.", html, StringComparison.Ordinal);
            Assert.Contains("¿Seguro que quieres eliminar la colección <strong>Owned collection</strong>?", html, StringComparison.Ordinal);
            Assert.Equal(initialCount, finalCount);
            Assert.Equal(originalLink, await observer.QuerySingleAsync<string>("SELECT json_object('Id', Id, 'CollectionId', CollectionId, 'Title', Title, 'IsPublic', IsPublic) FROM Link WHERE CollectionId = @Id;", new { Id = collectionId }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostCreateCollection_WhenAntiforgeryTokenIsMissing_RejectsRequestWithoutCreatingCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/nueva",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Input.Name"] = "Antiforgery collection",
                    ["Input.Description"] = "",
                    ["Input.IsPublic"] = "false",
                }));
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(initialCount, finalCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData(new string[] { "true" }, 1L)]
    [InlineData(new string[] { "false" }, 0L)]
    [InlineData(new string[] { "true", "false" }, 1L)]
    [InlineData(new string[] { "false", "false" }, 0L)]
    [InlineData(new string[] { }, 0L)]
    public async Task PostCreateCollection_WhenInputIsValid_CreatesCollectionAndRedirectsToListing(string[] publicValues, long expectedIsPublic)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            string token = await GetAntiforgeryToken(client, "/app/colecciones/nueva");
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/nueva",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Input.Name"] = "Created over HTTP",
                    ["Input.Description"] = "Created by integration test",
                    ["__RequestVerificationToken"] = token,
                }.Concat(publicValues.Select(value => new KeyValuePair<string, string>("Input.IsPublic", value)))));
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);
            long createdCollectionId = await GetCollectionId(databasePath, DemoEmail, "Created over HTTP");

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            Assert.Equal(initialCount + 1, finalCount);
            Assert.Equal("Created over HTTP", await GetCollectionName(databasePath, createdCollectionId));
            Assert.Equal(new PersistedCollectionState("Created over HTTP", "created-over-http", "Created by integration test", expectedIsPublic),
                await GetCollectionState(databasePath, createdCollectionId));
            await using SqliteConnection observer = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal("Created over HTTP|Created by integration test", await observer.QuerySingleAsync<string>(
                "SELECT Name || '|' || Description FROM CollectionSearch WHERE rowid = @Id;", new { Id = createdCollectionId }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData(true, "Updated collection")]
    [InlineData(false, "Updated collection")]
    [InlineData(true, "")]
    [InlineData(false, "")]
    public async Task PostEditCollection_WhenCollectionIsMissingOrForeign_RedirectsWithNotice(bool foreign, string name)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            long ownedId = await GetCollectionId(databasePath, DemoEmail, "Owned collection");
            long foreignId = await SeedOtherUsersCollection(databasePath);
            PersistedCollectionState original = await GetCollectionState(databasePath, foreignId);
            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{ownedId}/editar");
            long targetId = foreign ? foreignId : foreignId + 1000;

            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{targetId}/editar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Input.Name"] = name,
                    ["__RequestVerificationToken"] = token,
                }));
            using HttpResponseMessage listing = await client.GetAsync("/app/colecciones");
            string html = System.Net.WebUtility.HtmlDecode(await listing.Content.ReadAsStringAsync());
            using HttpResponseMessage secondListing = await client.GetAsync("/app/colecciones");
            string secondHtml = System.Net.WebUtility.HtmlDecode(await secondListing.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            Assert.Matches("role=\"status\" aria-live=\"polite\">\\s*<p>No se ha podido acceder al elemento solicitado\\.</p>", html);
            Assert.DoesNotContain("Foreign collection", html, StringComparison.Ordinal);
            Assert.DoesNotContain("No se ha podido acceder al elemento solicitado.", secondHtml, StringComparison.Ordinal);
            Assert.Equal(original, await GetCollectionState(databasePath, foreignId));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostEditCollection_WhenIdIsInvalid_ReturnsValidationWithoutChangingData()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Owned collection"]);
            long ownedId = await GetCollectionId(databasePath, DemoEmail, "Owned collection");
            PersistedCollectionState original = await GetCollectionState(databasePath, ownedId);
            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{ownedId}/editar");

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/not-an-id/editar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Input.Name"] = "Updated collection",
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("La colección es obligatoria.", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/colecciones\">Volver al listado</a>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"Input.Name\"", html, StringComparison.Ordinal);
            Assert.Equal(original, await GetCollectionState(databasePath, ownedId));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostEditCollection_WhenAntiforgeryTokenIsMissing_RejectsRequestWithoutChangingCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Collection before edit"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Collection before edit");

            string before = await CollectionSnapshot(databasePath);

            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{collectionId}/editar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["Input.Name"] = "Tampered collection",
                    ["Input.Description"] = "Must not persist",
                    ["Input.IsPublic"] = "true",
                }));

            Assert.Equal(before, await CollectionSnapshot(databasePath));
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Collection before edit", await GetCollectionName(databasePath, collectionId));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData(new string[] { "true" }, 1L)]
    [InlineData(new string[] { "false" }, 0L)]
    [InlineData(new string[] { "true", "false" }, 1L)]
    [InlineData(new string[] { "false", "false" }, 0L)]
    [InlineData(new string[] { }, 0L)]
    public async Task PostEditCollection_WhenInputIsValid_UpdatesCollectionAndRedirectsToListing(string[] publicValues, long expectedIsPublic)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Collection before edit"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Collection before edit");
            long userId;
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;",
                    new { Email = DemoEmail });
                await connection.ExecuteAsync(
                    "UPDATE Collection SET IsPublic = 1 WHERE Id = @CollectionId;",
                    new { CollectionId = collectionId });
                await connection.ExecuteAsync(
                    "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 1, @CreatedAt);",
                    new
                    {
                        UserId = userId,
                        CollectionId = collectionId,
                        Url = "https://collection-edit.example",
                        Title = "Public link before edit",
                        CreatedAt = DateTime.UtcNow,
                    });
            }

            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{collectionId}/editar");

            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{collectionId}/editar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["Input.Name"] = "Collection after edit",
                    ["Input.Description"] = "Edited by integration test",
                    ["__RequestVerificationToken"] = token,
                }.Concat(publicValues.Select(value => new KeyValuePair<string, string>("Input.IsPublic", value)))));

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            PersistedCollectionState persistedCollection = await GetCollectionState(databasePath, collectionId);
            int persistedLinkVisibility = await GetLinkVisibility(databasePath, userId, collectionId);

            Assert.Equal("Collection after edit", persistedCollection.Name);
            Assert.Equal("collection-after-edit", persistedCollection.Slug);
            Assert.Equal("Edited by integration test", persistedCollection.Description);
            Assert.Equal(expectedIsPublic, persistedCollection.IsPublic);
            Assert.Equal((int)expectedIsPublic, persistedLinkVisibility);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("not-a-boolean")]
    [InlineData("1")]
    [InlineData("")]
    public async Task PostCreateCollection_WhenPrivacyIsInvalid_ShowsFieldErrorWithoutCreatingCollection(string publicValue)
    {
        const string name = "Invalid privacy collection";
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            string token = await GetAntiforgeryToken(client, "/app/colecciones/nueva");
            long originalCount = await GetCollectionCount(databasePath, DemoEmail);

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/nueva",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["Input.Name"] = name,
                    ["Input.Description"] = "Submitted description",
                    ["Input.IsPublic"] = publicValue,
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.Equal(originalCount, await GetCollectionCount(databasePath, DemoEmail));
            Assert.Contains("El estado público o privado no es válido.", html, StringComparison.Ordinal);
            Assert.Contains("href=\"#collection-public\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-describedby=\"collection-public-error\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"collection-public-error\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-invalid=\"true\"", html, StringComparison.Ordinal);
            Assert.Contains(name, html, StringComparison.Ordinal);
            Assert.Contains("Submitted description", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("not-a-boolean")]
    [InlineData("1")]
    [InlineData("")]
    public async Task PostEditCollection_WhenPrivacyIsInvalid_LeavesCollectionAndPublicLinksUnchanged(string publicValue)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Public collection before edit"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Public collection before edit");
            long userId;
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail });
                await connection.ExecuteAsync(
                    "UPDATE Collection SET Description = 'Original description', IsPublic = 1 WHERE Id = @CollectionId;",
                    new { CollectionId = collectionId });
                await connection.ExecuteAsync(
                    "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 1, @CreatedAt);",
                    new
                    {
                        UserId = userId,
                        CollectionId = collectionId,
                        Url = "https://invalid-privacy.example",
                        Title = "Public link before invalid edit",
                        CreatedAt = DateTime.UtcNow,
                    });
            }
            string route = $"/app/colecciones/{collectionId}/editar";
            string token = await GetAntiforgeryToken(client, route);
            PersistedCollectionState originalCollection = await GetCollectionState(databasePath, collectionId);

            using HttpResponseMessage response = await client.PostAsync(route,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["Input.Name"] = "Submitted new name",
                    ["Input.Description"] = "Submitted new description",
                    ["Input.IsPublic"] = publicValue,
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.Equal(originalCollection, await GetCollectionState(databasePath, collectionId));
            Assert.Equal(1, await GetLinkVisibility(databasePath, userId, collectionId));
            Assert.Contains("El estado público o privado no es válido.", html, StringComparison.Ordinal);
            Assert.Contains("href=\"#collection-public\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-describedby=\"collection-public-error\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"collection-public-error\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-invalid=\"true\"", html, StringComparison.Ordinal);
            Assert.Contains("Submitted new name", html, StringComparison.Ordinal);
            Assert.Contains("Submitted new description", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostEditCollection_WhenNameIsAlreadyUsed_LeavesCollectionAndLinksUnchanged()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Collection before edit", "Already taken"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Collection before edit");
            long userId;
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;",
                    new { Email = DemoEmail });
                await connection.ExecuteAsync(
                    "UPDATE Collection SET Description = 'Original description', IsPublic = 1 WHERE Name = 'Collection before edit' AND UserId = @UserId;",
                    new { UserId = userId });
                await connection.ExecuteAsync(
                    "UPDATE Collection SET Slug = 'already-taken' WHERE Name = 'Already taken' AND UserId = @UserId;",
                    new { UserId = userId });
                await connection.ExecuteAsync(
                    "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 1, @CreatedAt);",
                    new
                    {
                        UserId = userId,
                        CollectionId = collectionId,
                        Url = "https://collection-rejection.example",
                        Title = "Public link before rejection",
                        CreatedAt = DateTime.UtcNow,
                    });
            }

            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{collectionId}/editar");
            PersistedCollectionState originalCollection = await GetCollectionState(databasePath, collectionId);
            int originalLinkVisibility = await GetLinkVisibility(databasePath, userId, collectionId);
            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{collectionId}/editar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["Input.Name"] = "ALREADY TAKEN!",
                    ["Input.Description"] = "Tampered description",
                    ["Input.IsPublic"] = "false",
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            PersistedCollectionState finalCollection = await GetCollectionState(databasePath, collectionId);
            int finalLinkVisibility = await GetLinkVisibility(databasePath, userId, collectionId);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Matches("(?s)<span[^>]*id=\"collection-name-error\"[^>]*>.*?Ya existe una colección con ese nombre: Already taken\\..*?</span>", html);
            Assert.Equal(originalCollection, finalCollection);
            Assert.Equal(originalLinkVisibility, finalLinkVisibility);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task GetDeleteCollection_WhenConfirmationPageIsRequested_DoesNotDeleteCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            await SeedCollections(databasePath, DemoEmail, ["Collection awaiting confirmation"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Collection awaiting confirmation");
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);

            string before = await CollectionSnapshot(databasePath);

            using HttpResponseMessage response = await client.GetAsync($"/app/colecciones/{collectionId}/eliminar");
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(before, await CollectionSnapshot(databasePath));
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Collection awaiting confirmation", html, StringComparison.Ordinal);
            Assert.Equal(initialCount, finalCount);
            Assert.Equal("Collection awaiting confirmation", await GetCollectionName(databasePath, collectionId));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostDeleteCollection_WhenAntiforgeryTokenIsMissing_RejectsRequestWithoutDeletingCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Collection protected from deletion"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Collection protected from deletion");
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);

            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{collectionId}/eliminar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                }));
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(initialCount, finalCount);
            Assert.Equal("Collection protected from deletion", await GetCollectionName(databasePath, collectionId));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostDeleteCollection_WhenAntiforgeryTokenIsValid_DeletesCollectionAndRedirectsToListing()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Collection to delete"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Collection to delete");
            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{collectionId}/eliminar");
            long initialCount = await GetCollectionCount(databasePath, DemoEmail);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            long[] remainingIds = (await connection.QueryAsync<long>(
                "SELECT Id FROM Collection WHERE Id <> @Id ORDER BY Id;", new { Id = collectionId })).ToArray();
            Assert.Equal(1, await connection.ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collectionId }));

            using HttpResponseMessage response = await PostDelete(client, collectionId, token);
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            Assert.Equal(initialCount - 1, finalCount);
            Assert.Equal(remainingIds, (await connection.QueryAsync<long>("SELECT Id FROM Collection ORDER BY Id;")).ToArray());
            Assert.Equal(0, await connection.ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collectionId }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostDeleteCollection_WhenItIsLastEmptyCollection_ShowsErrorAndPreservesCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            long collectionId = await connection.QuerySingleAsync<long>(
                "SELECT c.Id FROM Collection c JOIN User u ON u.Id = c.UserId WHERE u.Email = @Email;",
                new { Email = DemoEmail });
            PersistedCollectionState original = await GetCollectionState(databasePath, collectionId);
            Assert.Equal(0, await connection.ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM Link WHERE CollectionId = @Id;", new { Id = collectionId }));
            string token = await GetAntiforgeryToken(client, $"/app/colecciones/{collectionId}/eliminar");

            using HttpResponseMessage response = await PostDelete(client, collectionId, token);
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("No se puede eliminar la última colección del usuario.", html, StringComparison.Ordinal);
            Assert.Contains("¿Seguro que quieres eliminar la colección <strong>Bandeja de entrada</strong>?", html, StringComparison.Ordinal);
            Assert.Equal(1, await GetCollectionCount(databasePath, DemoEmail));
            Assert.Equal(original, await GetCollectionState(databasePath, collectionId));
            Assert.Equal(1, await connection.ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collectionId }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task PostEditCollection_WhenPrivacyIsPreservedOrMadePublic_KeepsPrivateLinksPrivate(bool wasPublic, bool isPublic)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Privacy matrix"]);
            long collectionId = await GetCollectionId(databasePath, DemoEmail, "Privacy matrix");
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            await connection.ExecuteAsync("UPDATE Collection SET IsPublic = @Public WHERE Id = @Id;", new { Public = wasPublic, Id = collectionId });
            await connection.ExecuteAsync(
                @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt)
                  SELECT UserId, Id, @Url, @Url, @Title, @Public, CreatedAt FROM Collection WHERE Id = @Id;",
                new[]
                {
                    new { Id = collectionId, Url = "https://example.test/private", Title = "Private", Public = false },
                    new { Id = collectionId, Url = "https://example.test/conditional", Title = "Conditional", Public = wasPublic },
                });
            string route = $"/app/colecciones/{collectionId}/editar";
            string token = await GetAntiforgeryToken(client, route);

            using HttpResponseMessage response = await client.PostAsync(route, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Input.Name"] = "Privacy matrix",
                ["Input.IsPublic"] = isPublic.ToString(),
                ["__RequestVerificationToken"] = token,
            }));

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(isPublic ? 1L : 0L, (await GetCollectionState(databasePath, collectionId)).IsPublic);
            Assert.Equal(new[] { wasPublic ? 1L : 0L, 0L }, (await connection.QueryAsync<long>(
                "SELECT IsPublic FROM Link WHERE CollectionId = @Id ORDER BY Title;", new { Id = collectionId })).ToArray());
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task CollectionForms_WithTextAtMaximum_AcceptAndPersist(bool edit, bool description)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            long id = await GetCollectionId(databasePath, DemoEmail, "Bandeja de entrada");
            string route = edit ? $"/app/colecciones/{id}/editar" : "/app/colecciones/nueva";
            string name = description ? "Boundary collection" : new string('n', 50);
            string text = description ? new string('d', 250) : "Description";
            string token = await GetAntiforgeryToken(client, route);

            using HttpResponseMessage response = await client.PostAsync(route, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Name"] = name,
                ["Input.Description"] = text,
                ["Input.IsPublic"] = "false",
                ["__RequestVerificationToken"] = token,
            }));

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            long persistedId = await GetCollectionId(databasePath, DemoEmail, name);
            PersistedCollectionState result = await GetCollectionState(databasePath, persistedId);
            Assert.Equal(name, result.Name);
            Assert.Equal(text, result.Description);
        }
        finally { DeleteDatabaseFiles(databasePath); }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task CollectionForms_WithTextAboveMaximum_AssociateErrorAndPreserveState(bool edit, bool description)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            long id = await GetCollectionId(databasePath, DemoEmail, "Bandeja de entrada");
            string route = edit ? $"/app/colecciones/{id}/editar" : "/app/colecciones/nueva";
            string name = description ? "Boundary collection" : new string('n', 51);
            string text = description ? new string('d', 251) : "Description";
            string token = await GetAntiforgeryToken(client, route);
            string before = await CollectionSnapshot(databasePath);

            using HttpResponseMessage response = await client.PostAsync(route, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Name"] = name,
                ["Input.Description"] = text,
                ["Input.IsPublic"] = "true",
                ["__RequestVerificationToken"] = token,
            }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            string field = description ? "Description" : "Name";
            Assert.Matches($"<span[^>]*data-valmsg-for=\"Input.{field}\"[^>]*>[^<]+</span>", html);
            Assert.Contains(name, html, StringComparison.Ordinal);
            Assert.Contains(text, html, StringComparison.Ordinal);
            Assert.Equal(before, await CollectionSnapshot(databasePath));
        }
        finally { DeleteDatabaseFiles(databasePath); }
    }

    [Theory]
    [InlineData("nueva")]
    [InlineData("editar")]
    [InlineData("eliminar")]
    public async Task CollectionForms_WithUnsupportedMethod_ReturnNotFoundWithoutChanges(string operation)
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            long id = await GetCollectionId(databasePath, DemoEmail, "Bandeja de entrada");
            string before = await CollectionSnapshot(databasePath);
            string route = operation == "nueva" ? "/app/colecciones/nueva" : $"/app/colecciones/{id}/{operation}";
            using HttpRequestMessage request = new(HttpMethod.Put, route);

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(before, await CollectionSnapshot(databasePath));
        }
        finally { DeleteDatabaseFiles(databasePath); }
    }

    [Fact]
    public async Task PostEditCollection_WithDifferentHiddenId_UpdatesOnlyCanonicalRouteCollection()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Target", "Untouched"]);
            long targetId = await GetCollectionId(databasePath, DemoEmail, "Target");
            long otherId = await GetCollectionId(databasePath, DemoEmail, "Untouched");
            PersistedCollectionState other = await GetCollectionState(databasePath, otherId);
            string route = $"/app/colecciones/{targetId}/editar";
            string token = await GetAntiforgeryToken(client, route);

            using HttpResponseMessage response = await client.PostAsync(route, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["id"] = otherId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Input.Name"] = "Canonical target",
                ["Input.IsPublic"] = "false",
                ["__RequestVerificationToken"] = token,
            }));

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("Canonical target", await GetCollectionName(databasePath, targetId));
            Assert.Equal(other, await GetCollectionState(databasePath, otherId));
        }
        finally { DeleteDatabaseFiles(databasePath); }
    }

    [Fact]
    public async Task PostCreateCollection_WithDifferentNameAndSameOwnedSlug_RejectsWithoutWriting()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedCollections(databasePath, DemoEmail, ["Existing"]);
            long id = await GetCollectionId(databasePath, DemoEmail, "Existing");
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            await connection.ExecuteAsync("UPDATE Collection SET Slug = 'existing' WHERE Id = @Id;", new { Id = id });
            string token = await GetAntiforgeryToken(client, "/app/colecciones/nueva");
            string before = await CollectionSnapshot(databasePath);

            using HttpResponseMessage response = await client.PostAsync("/app/colecciones/nueva", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Name"] = "EXISTING!",
                ["Input.IsPublic"] = "false",
                ["__RequestVerificationToken"] = token,
            }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Matches("<span[^>]*id=\"collection-name-error\"[^>]*>[^<]*Existing[^<]*</span>", html);
            Assert.Equal(before, await CollectionSnapshot(databasePath));
        }
        finally { DeleteDatabaseFiles(databasePath); }
    }

    [Fact]
    public async Task CollectionForms_WithForeignSharedSlug_AllowOwnCreationAndUnchangedSlugEdit()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            long foreignId = await SeedOtherUsersCollection(databasePath);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            await connection.ExecuteAsync("UPDATE Collection SET Slug = 'shared' WHERE Id = @Id;", new { Id = foreignId });
            PersistedCollectionState foreign = await GetCollectionState(databasePath, foreignId);
            string token = await GetAntiforgeryToken(client, "/app/colecciones/nueva");

            using HttpResponseMessage created = await client.PostAsync("/app/colecciones/nueva", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Name"] = "Shared",
                ["Input.IsPublic"] = "false",
                ["__RequestVerificationToken"] = token,
            }));
            Assert.Equal(System.Net.HttpStatusCode.Redirect, created.StatusCode);
            long ownId = await GetCollectionId(databasePath, DemoEmail, "Shared");
            string route = $"/app/colecciones/{ownId}/editar";
            string editToken = await GetAntiforgeryToken(client, route);
            using HttpResponseMessage edited = await client.PostAsync(route, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Name"] = "SHARED!",
                ["Input.Description"] = "Updated",
                ["Input.IsPublic"] = "false",
                ["__RequestVerificationToken"] = editToken,
            }));

            Assert.Equal(System.Net.HttpStatusCode.Redirect, edited.StatusCode);
            Assert.Equal(new PersistedCollectionState("SHARED!", "shared", "Updated", 0), await GetCollectionState(databasePath, ownId));
            Assert.Equal(foreign, await GetCollectionState(databasePath, foreignId));
        }
        finally { DeleteDatabaseFiles(databasePath); }
    }

    private static async Task<string> CollectionSnapshot(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        string[] collections = (await connection.QueryAsync<string>(
            "SELECT json_object('Id', Id, 'UserId', UserId, 'Name', Name, 'Slug', Slug, 'Description', Description, 'IsPublic', IsPublic, 'CreatedAt', CreatedAt) FROM Collection ORDER BY Id;")).ToArray();
        string[] projections = (await connection.QueryAsync<string>(
            "SELECT json_object('Id', rowid, 'UserId', UserId, 'Name', Name, 'Description', Description) FROM CollectionSearch ORDER BY rowid;")).ToArray();
        return string.Join("\n", collections.Concat(projections));
    }

    private static string GetCollectionListHtml(string html)
    {
        int start = html.IndexOf("<ul id=\"saved-collections\"", StringComparison.Ordinal);
        if (start < 0)
        {
            return string.Empty;
        }

        int end = html.IndexOf("</ul>", start, StringComparison.Ordinal);
        return end < 0 ? string.Empty : html[start..(end + "</ul>".Length)];
    }

    private static async Task<long> SeedOtherUsersCollection(string databasePath)
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
            "INSERT INTO CollectionSearch (rowid, UserId, Name, Description) VALUES (@Id, @UserId, @Name, @Description);",
            new { Id = collectionId, UserId = userId, Name = "Foreign collection", Description = "" });
        return collectionId;
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
                "INSERT INTO CollectionSearch (rowid, UserId, Name, Description) VALUES (@Id, @UserId, @Name, @Description);",
                new { Id = collectionId, UserId = userId, Name = names[index], Description = string.Empty });
        }
    }

    private static string NormalizeAntiforgeryTokens(string html)
    {
        const string tokenName = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        int tokenStart = html.IndexOf(tokenName, StringComparison.Ordinal);
        if (tokenStart < 0)
        {
            return html;
        }

        tokenStart += tokenName.Length;
        int tokenEnd = html.IndexOf('"', tokenStart);
        return html[..tokenStart] + "<token>" + html[tokenEnd..];
    }

    private static async Task<long> GetCollectionId(string databasePath, string email, string name)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        return await connection.ExecuteScalarAsync<long>(
            "SELECT Collection.Id FROM Collection INNER JOIN User ON User.Id = Collection.UserId WHERE User.Email = @Email AND Collection.Name = @Name;",
            new { Email = email, Name = name });
    }

    private static async Task<long> GetCollectionCount(string databasePath, string email)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        return await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Collection INNER JOIN User ON User.Id = Collection.UserId WHERE User.Email = @Email;",
            new { Email = email });
    }

    private static async Task<string?> GetCollectionName(string databasePath, long collectionId)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        return await connection.ExecuteScalarAsync<string>(
            "SELECT Name FROM Collection WHERE Id = @CollectionId;",
            new { CollectionId = collectionId });
    }

    private static async Task<PersistedCollectionState> GetCollectionState(string databasePath, long collectionId)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        return await connection.QuerySingleAsync<PersistedCollectionState>(
            "SELECT Name, Slug, Description, IsPublic FROM Collection WHERE Id = @CollectionId;",
            new { CollectionId = collectionId });
    }

    private static async Task<int> GetLinkVisibility(string databasePath, long userId, long collectionId)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Link WHERE UserId = @UserId AND CollectionId = @CollectionId;",
            new { UserId = userId, CollectionId = collectionId });
    }

    private static async Task<string> GetAntiforgeryToken(HttpClient client, long collectionId)
    {
        using HttpResponseMessage response = await client.GetAsync($"/app/colecciones/{collectionId}/eliminar");
        string html = await response.Content.ReadAsStringAsync();
        const string tokenName = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        int tokenStart = html.IndexOf(tokenName, StringComparison.Ordinal);
        if (tokenStart < 0)
        {
            throw new InvalidOperationException("The antiforgery token was not rendered.");
        }

        tokenStart += tokenName.Length;
        int tokenEnd = html.IndexOf('"', tokenStart);
        return System.Net.WebUtility.HtmlDecode(html[tokenStart..tokenEnd]);
    }

    private static async Task<string> GetAntiforgeryToken(HttpClient client, string pagePath)
    {
        using HttpResponseMessage response = await client.GetAsync(pagePath);
        string html = await response.Content.ReadAsStringAsync();
        const string tokenName = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        int tokenStart = html.IndexOf(tokenName, StringComparison.Ordinal);
        if (tokenStart < 0)
        {
            throw new InvalidOperationException("The antiforgery token was not rendered.");
        }

        tokenStart += tokenName.Length;
        int tokenEnd = html.IndexOf('"', tokenStart);
        return System.Net.WebUtility.HtmlDecode(html[tokenStart..tokenEnd]);
    }

    private static Task<HttpResponseMessage> PostDelete(HttpClient client, long collectionId, string token)
        => client.PostAsync(
            $"/app/colecciones/{collectionId}/eliminar",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["__RequestVerificationToken"] = token,
            }));

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
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
        }
    }

    private sealed record PersistedCollectionState(string Name, string Slug, string? Description, long IsPublic);
}
