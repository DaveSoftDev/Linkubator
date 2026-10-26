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
            Assert.Contains("class=\"view-switch__button toolbar-tool\" href=\"/app/colecciones/nueva\"", html, StringComparison.Ordinal);
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

            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?userId=2&c=not-a-collection-id");
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

            using HttpResponseMessage response = await client.PostAsync(
                "/app/colecciones/not-an-id/eliminar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = "not-an-id",
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("La colección es obligatoria.", html, StringComparison.Ordinal);
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

    [Fact]
    public async Task PostCreateCollection_WhenNameIsRequired_RendersFieldErrorAndFirstInvalidFocus()
    {
        string databasePath = NewDatabasePath();
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            string token = await GetAntiforgeryToken(client, "/app/colecciones/nueva");

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
            Assert.Contains("querySelectorAll", html, StringComparison.Ordinal);
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
            Assert.Equal(initialCount, finalCount);
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

    [Fact]
    public async Task PostCreateCollection_WhenInputIsValid_CreatesCollectionAndRedirectsToListing()
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
                    ["Input.IsPublic"] = "false",
                    ["__RequestVerificationToken"] = token,
                }));
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);
            long createdCollectionId = await GetCollectionId(databasePath, DemoEmail, "Created over HTTP");

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            Assert.Equal(initialCount + 1, finalCount);
            Assert.Equal("Created over HTTP", await GetCollectionName(databasePath, createdCollectionId));
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

            using HttpResponseMessage response = await client.PostAsync(
                $"/app/colecciones/{collectionId}/editar",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["id"] = collectionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["Input.Name"] = "Tampered collection",
                    ["Input.Description"] = "",
                    ["Input.IsPublic"] = "false",
                }));

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Collection before edit", await GetCollectionName(databasePath, collectionId));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PostEditCollection_WhenInputIsValid_UpdatesCollectionAndRedirectsToListing()
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
                    ["Input.IsPublic"] = "false",
                    ["__RequestVerificationToken"] = token,
                }));

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            PersistedCollectionState persistedCollection = await GetCollectionState(databasePath, collectionId);
            int persistedLinkVisibility = await GetLinkVisibility(databasePath, userId, collectionId);

            Assert.Equal("Collection after edit", persistedCollection.Name);
            Assert.Equal("collection-after-edit", persistedCollection.Slug);
            Assert.Equal("Edited by integration test", persistedCollection.Description);
            Assert.Equal(0L, persistedCollection.IsPublic);
            Assert.Equal(0, persistedLinkVisibility);
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
                    ["Input.Name"] = "Already taken",
                    ["Input.Description"] = "Tampered description",
                    ["Input.IsPublic"] = "false",
                    ["__RequestVerificationToken"] = token,
                }));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            PersistedCollectionState finalCollection = await GetCollectionState(databasePath, collectionId);
            int finalLinkVisibility = await GetLinkVisibility(databasePath, userId, collectionId);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Already taken", html, StringComparison.Ordinal);
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

            using HttpResponseMessage response = await client.GetAsync($"/app/colecciones/{collectionId}/eliminar");
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

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

            using HttpResponseMessage response = await PostDelete(client, collectionId, token);
            long finalCount = await GetCollectionCount(databasePath, DemoEmail);

            Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/app/colecciones", response.Headers.Location?.OriginalString);
            Assert.Equal(initialCount - 1, finalCount);
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
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
        }
    }

    private sealed record PersistedCollectionState(string Name, string Slug, string? Description, long IsPublic);
}
