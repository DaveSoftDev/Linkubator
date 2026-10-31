using System.Collections.Concurrent;
using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Sqlite;
using Linkubator.Web.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationUseCases = Linkubator.Application.UseCases.Dashboard;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public class WebStartupTests
{
    private const string PublicUserCanonicalPath = "/my-demo-user/";
    private const string PublicCollectionsCanonicalPath = "/my-demo-user/colecciones/";
    private const string PublicCollectionCanonicalPath = "/my-demo-user/colecciones/public-collection";

    [Theory]
    [InlineData("/app", true)]
    [InlineData("/app/enlaces", true)]
    [InlineData("/app/colecciones", true)]
    [InlineData("/app/colecciones/nueva", true)]
    [InlineData("/app/enlaces/nuevo", true)]
    [InlineData("/", false)]
    [InlineData("/Privacy", false)]
    [InlineData("/my-demo-user/", false)]
    [InlineData("/my-demo-user/colecciones/", false)]
    [InlineData("/my-demo-user/colecciones/public-collection", false)]
    public async Task PrivateTheme_IsLoadedOnlyByPrivatePages(string path, bool expectedPrivateTheme)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);

            string html = await client.GetStringAsync(path);

            Assert.Equal(expectedPrivateTheme, System.Text.RegularExpressions.Regex.IsMatch(html, "href=\"/css/private(?:\\.[a-z0-9]+)?\\.css"));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Privacy")]
    [InlineData("/my-demo-user/")]
    [InlineData("/my-demo-user/colecciones/")]
    [InlineData("/my-demo-user/colecciones/public-collection")]
    [InlineData("/404")]
    public async Task PublicHeaders_RenderSameLoginAndRegistrationActionsAsHome(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);

            string homeHtml = await client.GetStringAsync("/");
            using HttpResponseMessage response = await client.GetAsync(path);
            string html = await response.Content.ReadAsStringAsync();
            const string actionsPattern = "(?s)<div class=\"landing-nav__auth\">.*?</div>";
            string homeActions = System.Text.RegularExpressions.Regex.Match(homeHtml, actionsPattern).Value;
            string pageActions = System.Text.RegularExpressions.Regex.Match(html, actionsPattern).Value;

            Assert.Equal(path == "/404" ? System.Net.HttpStatusCode.NotFound : System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("href=\"/app\"", pageActions, StringComparison.Ordinal);
            Assert.Contains("href=\"#\"", pageActions, StringComparison.Ordinal);
            Assert.NotEmpty(homeActions);
            Assert.Matches("<header class=\"[^\"]*\\bpublic-header\\b[^\"]*\">", html);
            Assert.Equal(homeActions, pageActions);
            Assert.Contains(">Login <span", pageActions, StringComparison.Ordinal);
            Assert.Contains("title=\"Registro no disponible\"", pageActions, StringComparison.Ordinal);
            Assert.Contains("aria-label=\"Registro: no disponible\"", pageActions, StringComparison.Ordinal);
            Assert.Matches("href=\"/css/public-actions(?:\\.[a-z0-9]+)?\\.css", html);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/404")]
    [InlineData("/esta/ruta/no/existe")]
    [InlineData("/unknown-user-alias/")]
    [InlineData("/my-demo-user/colecciones/private-collection")]
    [InlineData("/my-demo-user/colecciones/missing-collection")]
    public async Task PublicNotFound_RendersBrandedGenericPageWithoutRedirect(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedPublicUserPageContent(databasePath);

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.Contains("Este enlace se ha", html, StringComparison.Ordinal);
            Assert.Contains("quedado suelto.", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"robots\" content=\"noindex, follow\" />", html, StringComparison.Ordinal);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<main\\b"));
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<h1\\b"));
            Assert.Contains("Volver al inicio", html, StringComparison.Ordinal);
            Assert.Contains("Copyright 2026.", html, StringComparison.Ordinal);
            Assert.Matches("href=\"/css/not-found(?:\\.[a-z0-9]+)?\\.css", html);
            Assert.DoesNotContain("PRIVATE_COLLECTION", html, StringComparison.Ordinal);
            Assert.DoesNotContain("unknown-user-alias", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UserPage_WithManyPublicLinks_RendersSixMostRecentCards()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await SeedPublicCollectionPaginationContent(databasePath);

            string html = await client.GetStringAsync(PublicUserCanonicalPath);

            Assert.Equal(6, System.Text.RegularExpressions.Regex.Matches(html, "<article class=\"link-card public-link\">").Count);
            Assert.Equal(
                new[] { "15", "14", "13", "12", "11", "10" },
                System.Text.RegularExpressions.Regex.Matches(
                    html, "<h3><a href=\"https://links.example/page-\\d{2}\"[^>]*>Public page link (\\d{2})")
                    .Select(match => match.Groups[1].Value));
            Assert.DoesNotContain("Public page link 09", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_PUBLIC_LINK_TITLE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_COLLECTION_LINK_TITLE", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/my-demo-user/", "Perfil", "page")]
    [InlineData("/my-demo-user/colecciones/", "Colecciones", "page")]
    [InlineData("/my-demo-user/colecciones/public-collection", "Public collection", "location")]
    public async Task PublicProfileNavigation_FollowsSharedBannerWithoutRedundantLabels(string path, string currentTab, string currentState)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                Assert.True(await connection.ExecuteScalarAsync<long>(
                    @"SELECT COUNT(*) FROM Collection c
                      JOIN Link l ON l.CollectionId = c.Id AND l.IsPublic = 1
                      WHERE c.UserId = (SELECT Id FROM User WHERE Email = @Email) AND c.IsPublic = 1;",
                    new { Email = DemoEmail }) > 0, "El test requiere una colección pública con al menos un enlace público.");
            }

            string html = System.Net.WebUtility.HtmlDecode(await client.GetStringAsync(path));
            System.Text.RegularExpressions.Match navigation = System.Text.RegularExpressions.Regex.Match(
                html, "(?s)<nav class=\"public-navigation\".*?</nav>");

            Assert.Matches("(?s)<header class=\"public-heading public-profile-heading\">.*?</header>\\s*<nav class=\"public-navigation\"", html);
            Assert.True(navigation.Success);
            Assert.Contains($"aria-current=\"{currentState}\">{currentTab}</a>", navigation.Value, StringComparison.Ordinal);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(navigation.Value, "aria-current="));
            Assert.Contains("href=\"/my-demo-user/\"", navigation.Value, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/colecciones/\"", navigation.Value, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"public-owner\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Espacio público", html, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"public-back\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PrivacyPage_RendersGenericInformationWithoutIndexing()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/Privacy");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<title>Privacidad - Linkubator</title>", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"robots\" content=\"noindex, follow\" />", html, StringComparison.Ordinal);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<main\\b"));
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<h1\\b"));
            Assert.Contains("Texto orientativo, pendiente de revisión", html, StringComparison.Ordinal);
            Assert.Contains("Contacto del responsable: pendiente de definir", html, StringComparison.Ordinal);
            Assert.Contains("aria-label=\"Contenido de privacidad\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"datos\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"uso\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"contenido\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"cookies\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"conservacion\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"derechos\"", html, StringComparison.Ordinal);
            Assert.Matches("href=\"/css/privacy(?:\\.[a-z0-9]+)?\\.css", html);
            Assert.DoesNotContain("Use this page", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Privacy")]
    [InlineData("/my-demo-user/")]
    [InlineData("/my-demo-user/colecciones/")]
    [InlineData("/my-demo-user/colecciones/public-collection")]
    [InlineData("/app")]
    public async Task Pages_WithSharedPresentation_RenderBrandFaviconsAndPrivacyLink(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Matches("(?s)<footer\\b[^>]*>(?:(?!</footer>).)*href=\"/Privacy\"", html);
            Assert.Matches("Copyright 2026\\.\\s*<a[^>]*href=\"/Privacy\"", html);
            Assert.Matches("href=\"/css/footer(?:\\.[a-z0-9]+)?\\.css", html);
            Assert.Matches("href=\"/css/brand(?:\\.[a-z0-9]+)?\\.css", html);
            Assert.Matches("<img class=\"brand-icon\"[^>]*src=\"/brand/symbol(?:\\.[a-z0-9]+)?\\.svg[^>]*alt=\"\"", html);
            Assert.Matches("rel=\"icon\"[^>]*type=\"image/svg\\+xml\"[^>]*href=\"/favicon(?:\\.[a-z0-9]+)?\\.svg", html);
            Assert.Matches("rel=\"icon\"[^>]*type=\"image/x-icon\"[^>]*href=\"/favicon(?:\\.[a-z0-9]+)?\\.ico", html);
            Assert.Matches("rel=\"apple-touch-icon\"[^>]*sizes=\"180x180\"[^>]*href=\"/apple-touch-icon(?:\\.[a-z0-9]+)?\\.png", html);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/brand/symbol.svg")]
    [InlineData("/brand/logo-dark.svg")]
    [InlineData("/brand/logo-light.svg")]
    [InlineData("/brand/logo-monochrome-dark.svg")]
    [InlineData("/brand/logo-monochrome-light.svg")]
    [InlineData("/favicon.svg")]
    public async Task BrandSvgAssets_ReturnSelfContainedVectors(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync(path);
            string svg = await response.Content.ReadAsStringAsync();
            System.Xml.Linq.XElement document = System.Xml.Linq.XElement.Parse(svg);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("{http://www.w3.org/2000/svg}svg", document.Name.ToString());
            Assert.DoesNotContain("<text", svg, StringComparison.Ordinal);
            Assert.DoesNotContain("<image", svg, StringComparison.Ordinal);
            Assert.DoesNotContain("href=", svg, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/favicon-16x16.png", 16)]
    [InlineData("/favicon-32x32.png", 32)]
    [InlineData("/apple-touch-icon.png", 180)]
    [InlineData("/brand/icon-192.png", 192)]
    [InlineData("/brand/icon-512.png", 512)]
    public async Task BrandPngAssets_ReturnExpectedPixelDimensions(string path, int size)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync(path);
            byte[] png = await response.Content.ReadAsByteArrayAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("89504E470D0A1A0A", Convert.ToHexString(png[..8]));
            Assert.Equal(size, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)));
            Assert.Equal(size, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task FaviconIco_ReturnsFiveResolutionFrames()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/favicon.ico");
            byte[] ico = await response.Content.ReadAsByteArrayAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/x-icon", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("000001000500", Convert.ToHexString(ico[..6]));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task HomePage_Renders_PublicLandingMetadataAndActions()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<html lang=\"es\">", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"robots\" content=\"index, follow\" />", html, StringComparison.Ordinal);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<h1\\b"));
            Assert.Contains("<title>Linkubator, donde gestionar tus enlaces favoritos</title>", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"title\" content=\"Linkubator, donde gestionar tus enlaces favoritos\" />", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"description\" content=\"Linkubator es un servicio web que te permite gestionar tus enlaces favoritos en colecciones y etiquetas. Para que nunca más pierdas los enlaces importantes\" />", html, StringComparison.Ordinal);
            Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:5001/\" />", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"#\"", html, StringComparison.Ordinal);
            Assert.Contains("Linkubator, para gestionar tus enlaces favoritos y no perderlos", html, StringComparison.Ordinal);
            Assert.Contains("id=\"caracteristicas\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"numbers-title\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"comunidad\"", html, StringComparison.Ordinal);
            Assert.Contains("marcas ficticias de muestra, sin relación comercial.", html, StringComparison.Ordinal);
            Assert.Contains("No son métricas reales de usuarios ni del servicio.", html, StringComparison.Ordinal);
            Assert.Contains("personas y opiniones ficticias, no reseñas verificadas.", html, StringComparison.Ordinal);
            Assert.Contains("El registro todavía no está disponible.", html, StringComparison.Ordinal);
            Assert.Matches("(?s)class=\"landing-nav__auth\">\\s*<a[^>]*href=\"/app\"[^>]*>.*?</a>\\s*<a[^>]*href=\"#\"[^>]*title=\"Registro no disponible\"[^>]*>", html);
            Assert.Equal(6, System.Text.RegularExpressions.Regex.Matches(html, "<article class=\"landing-feature(?: landing-feature--highlight)?\">").Count);
            Assert.Contains("04 / ENCONTRAR", html, StringComparison.Ordinal);
            Assert.Contains("05 / PERSONALIZAR", html, StringComparison.Ordinal);
            Assert.Contains("06 / REDESCUBRIR", html, StringComparison.Ordinal);
            Assert.Matches("href=\"/css/landing(?:\\.[a-z0-9]+)?\\.css", html);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task HomePage_ProductPreview_UsesPublicStaticDashboardCaptureWithExampleData()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            string html = System.Net.WebUtility.HtmlDecode(await client.GetStringAsync("/"));
            using HttpResponseMessage image = await client.GetAsync("/brand/dashboard-preview.webp");

            Assert.Matches("<img[^>]*src=\"/brand/dashboard-preview(?:\\.[a-z0-9]+)?\\.webp[^>]*width=\"1440\"[^>]*height=\"1000\"[^>]*alt=\"Dashboard real de Linkubator:", html);
            Assert.Matches("href=\"/brand/dashboard-preview(?:\\.[a-z0-9]+)?\\.webp\"", html);
            Assert.Contains("Captura real del producto · datos de ejemplo · pulsa para ampliar", html, StringComparison.Ordinal);
            Assert.DoesNotContain("landing-mini-card", html, StringComparison.Ordinal);
            Assert.DoesNotContain("<iframe", html, StringComparison.Ordinal);
            Assert.Equal(System.Net.HttpStatusCode.OK, image.StatusCode);
            Assert.Equal("image/webp", image.Content.Headers.ContentType?.MediaType);
            Assert.NotEmpty(await image.Content.ReadAsByteArrayAsync());
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UserPage_RendersPublicContentMetadataAndConfiguredCanonical()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            client.DefaultRequestHeaders.Host = "127.0.0.1";

            using HttpResponseMessage response = await client.GetAsync("/my-demo-user/");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<title>Los enlaces favoritos de my demo user en Linkubator</title>", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"description\" content=\"Los enlaces favoritos más importantes de my demo user en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página\" />", html, StringComparison.Ordinal);
            Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:5001/my-demo-user/\" />", html, StringComparison.Ordinal);
            Assert.Contains("<h1", html, StringComparison.Ordinal);
            Assert.Matches("<h1[^>]*>Enlaces favoritos de my demo user</h1>", html);
            Assert.Contains("<h2 id=\"latest-links-title\"", html, StringComparison.Ordinal);
            Assert.Contains("Los últimos enlaces de my demo user", html, StringComparison.Ordinal);
            Assert.Contains("<h2 id=\"latest-collections-title\"", html, StringComparison.Ordinal);
            Assert.Contains("Las últimas colecciones de my demo user", html, StringComparison.Ordinal);
            Assert.Contains("Visible link", html, StringComparison.Ordinal);
            Assert.Contains("Visible description", html, StringComparison.Ordinal);
            Assert.Contains("src=\"https://images.example/public.jpg\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"https://links.example/public\" rel=\"nofollow ugc noopener noreferrer\" target=\"_blank\"", html, StringComparison.Ordinal);
            Assert.Contains("Public collection", html, StringComparison.Ordinal);
            Assert.Contains("Public collection description", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/colecciones/public-collection\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/colecciones/\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_PUBLIC_LINK_TITLE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_PUBLIC_LINK_DESCRIPTION", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_COLLECTION_LINK_TITLE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_COLLECTION_LINK_DESCRIPTION", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Demo User", html, StringComparison.Ordinal);
            Assert.DoesNotContain(DemoEmail, html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/my-demo-user/")]
    [InlineData("/my-demo-user/colecciones/public-collection")]
    public async Task PublicPage_WithImages_RendersAltAsText(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await SeedPublicImageAltCases(databasePath);

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("alt=\"Visible link\"", html, StringComparison.Ordinal);
            Assert.Contains("alt=\"https://links.example/no-title\"", html, StringComparison.Ordinal);
            Assert.Contains("alt=\"&lt;em&gt;Marked title&lt;/em&gt;\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/my-demo-user/")]
    [InlineData("/my-demo-user/colecciones/")]
    [InlineData("/my-demo-user/colecciones/public-collection")]
    public async Task PublicPages_WithPublicContent_RenderSharedLibraryPresentation(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<html lang=\"es\">", html, StringComparison.Ordinal);
            Assert.Matches("href=\"/css/public(?:\\.[a-z0-9]+)?\\.css", html);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<main\\b"));
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<h1\\b"));
            Assert.Contains("href=\"#public-content\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-label=\"Navegación del perfil\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/colecciones/\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("/css/landing", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Demo User", html, StringComparison.Ordinal);
            Assert.DoesNotContain(DemoEmail, html, StringComparison.Ordinal);
            Assert.DoesNotContain("/app/enlaces/", html, StringComparison.Ordinal);
            Assert.DoesNotContain("public-avatar", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Enlace público", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Colección pública", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/my-demo-user/")]
    [InlineData("/my-demo-user/colecciones/public-collection")]
    public async Task PublicPages_WithoutOptionalLinkContent_RenderUrlFallbackAndEncodeDescription(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        const string linkUrl = "https://links.example/public";
        const string description = "<script>alert('example')</script>";
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                await connection.ExecuteAsync(
                    "UPDATE Link SET Title = NULL, Image = NULL, Description = @Description WHERE UrlOriginal = @Url;",
                    new { Description = description, Url = linkUrl });
            }

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"target=\"_blank\">{linkUrl}<span", html, StringComparison.Ordinal);
            Assert.Contains("&lt;script&gt;", html, StringComparison.Ordinal);
            Assert.DoesNotContain(description, html, StringComparison.Ordinal);
            Assert.DoesNotMatch("class=\"public-link__visual\">\\s*<img", html);
            Assert.Contains("class=\"public-link__visual\"", html, StringComparison.Ordinal);
            Assert.Matches("class=\"brand-mark public-link__placeholder\" aria-hidden=\"true\">\\s*<img class=\"brand-icon\"[^>]*src=\"/brand/symbol(?:\\.[a-z0-9]+)?\\.svg[^>]*alt=\"\"", html);
            Assert.Contains("<span class=\"public-link__url\">links.example</span>", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/my-demo-user/")]
    [InlineData("/my-demo-user/colecciones/public-collection")]
    public async Task PublicPages_WithFullLinkUrl_ShowOnlyDomainWithoutChangingDestination(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        const string linkUrl = "https://docs.example.com:8443/articles/read?source=home#intro";
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                await connection.ExecuteAsync(
                    "UPDATE Link SET UrlOriginal = @Url, UrlNormalized = @Url WHERE UrlOriginal = @OriginalUrl;",
                    new { Url = linkUrl, OriginalUrl = "https://links.example/public" });
            }

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<span class=\"public-link__url\">docs.example.com</span>", html, StringComparison.Ordinal);
            Assert.Contains($"href=\"{linkUrl}\" rel=\"nofollow ugc noopener noreferrer\" target=\"_blank\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain($"<span class=\"public-link__url\">{linkUrl}</span>", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionsPage_RendersPublicCollectionsMetadataAndCanonical()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            client.DefaultRequestHeaders.Host = "127.0.0.1";

            using HttpResponseMessage response = await client.GetAsync("/my-demo-user/colecciones/");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<title>Las colecciones de my demo user en Linkubator</title>", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"description\" content=\"Las colecciones de enlaces favoritos de my demo user en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página\" />", html, StringComparison.Ordinal);
            Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:5001/my-demo-user/colecciones/\" />", html, StringComparison.Ordinal);
            Assert.Contains("<h1", html, StringComparison.Ordinal);
            Assert.Matches("<h1[^>]*>Colecciones de my demo user</h1>", html);
            Assert.Contains("Public collection", html, StringComparison.Ordinal);
            Assert.Contains("Public collection description", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/colecciones/public-collection\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_COLLECTION_NAME", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_COLLECTION_DESCRIPTION", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionsPage_ReturnsNotFoundForUnknownAlias()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            using HttpResponseMessage response = await client.GetAsync("/unknown-user/colecciones/");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Null(response.Headers.Location);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionsPage_ShowsEmptyStateWhenUserHasNoPublicCollections()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/my-demo-user/colecciones/");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("my demo user todavía no ha creado colecciones", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/my-demo-user/\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("href=\"/my-demo-user/colecciones/\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionPage_RendersPublicLinksMetadataAndCanonical()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            client.DefaultRequestHeaders.Host = "127.0.0.1";

            using HttpResponseMessage response = await client.GetAsync("/my-demo-user/colecciones/public-collection?p=99");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("<title>Los enlaces de my demo user para Public collection en Linkubator</title>", html, StringComparison.Ordinal);
            Assert.Contains("<meta name=\"description\" content=\"Los enlaces favoritos de my demo user en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página\" />", html, StringComparison.Ordinal);
            Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:5001/my-demo-user/colecciones/public-collection\" />", html, StringComparison.Ordinal);
            Assert.Contains("<h1", html, StringComparison.Ordinal);
            Assert.Matches("<h1[^>]*>Enlaces en Public collection de my demo user</h1>", html);
            Assert.Contains("Visible link", html, StringComparison.Ordinal);
            Assert.Contains("href=\"https://links.example/public\" rel=\"nofollow ugc noopener noreferrer\" target=\"_blank\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_PUBLIC_LINK_TITLE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_PUBLIC_LINK_DESCRIPTION", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("?p=02&unknown=ignored", "Visible link", "Public page link 00", 2, 1, "?p=2")]
    [InlineData("?p=2&p=1", "Visible link", "Public page link 00", 2, 1, "?p=2")]
    [InlineData("?p=1&p=2", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=0001", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=%2B2", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=0", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=999", "Visible link", "Public page link 00", 2, 1, "?p=2")]
    [InlineData("?p=999&p=1", "Visible link", "Public page link 00", 2, 1, "?p=2")]
    [InlineData("?p=99999999999999999999", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=-1", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=%202%20", "Public page link 00", "Visible link", 1, 16, "")]
    [InlineData("?p=%D9%A2", "Public page link 00", "Visible link", 1, 16, "")]
    public async Task PublicCollectionPage_NormalizesPageParameterAndCanonical(
        string query,
        string expectedLinkTitle,
        string excludedLinkTitle,
        int expectedPage,
        int expectedLinkCount,
        string canonicalQuery)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await SeedPublicCollectionPaginationContent(databasePath);
            client.DefaultRequestHeaders.Host = "127.0.0.1";

            using HttpResponseMessage response = await client.GetAsync($"/my-demo-user/colecciones/public-collection{query}");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"<link rel=\"canonical\" href=\"https://localhost:5001{PublicCollectionCanonicalPath}{canonicalQuery}\" />", html, StringComparison.Ordinal);
            Assert.Contains($"Página {expectedPage} de 2", html, StringComparison.Ordinal);
            Assert.Equal(expectedLinkCount, System.Text.RegularExpressions.Regex.Matches(html, "<article class=\"link-card public-link\">").Count);
            string[] expectedTitles = expectedPage == 1
                ? Enumerable.Range(0, 16).Select(index => $"Public page link {index:D2}").ToArray()
                : ["Visible link"];
            Assert.Equal(expectedTitles, PublicCollectionTitles(html));
            string navigation = System.Text.RegularExpressions.Regex.Match(html,
                "(?s)<nav aria-label=\"Paginación de enlaces\".*?</nav>").Value;
            string expectedNavigation = expectedPage == 1
                ? $"href=\"{PublicCollectionCanonicalPath}?p=2\" class=\"pagination__item\">Siguiente</a>"
                : $"href=\"{PublicCollectionCanonicalPath}\" class=\"pagination__item\">Anterior</a>";
            Assert.Contains(expectedNavigation, navigation, StringComparison.Ordinal);
            string disabledLabel = expectedPage == 1 ? "Anterior" : "Siguiente";
            Assert.Contains($"aria-disabled=\"true\">{disabledLabel}</span>", navigation, StringComparison.Ordinal);
            Assert.Contains(expectedLinkTitle, html, StringComparison.Ordinal);
            Assert.DoesNotContain(excludedLinkTitle, html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/MY-DEMO-USER/", PublicUserCanonicalPath)]
    [InlineData("/my-demo-user", PublicUserCanonicalPath)]
    [InlineData("/MY-DEMO-USER/COLECCIONES/", PublicCollectionsCanonicalPath)]
    [InlineData("/my-demo-user/colecciones", PublicCollectionsCanonicalPath)]
    [InlineData("/MY-DEMO-USER/COLECCIONES/PUBLIC-COLLECTION", PublicCollectionCanonicalPath)]
    [InlineData("/my-demo-user/colecciones/public-collection/", PublicCollectionCanonicalPath)]
    public async Task PublicPages_WithNonCanonicalPaths_RedirectToCanonicalPaths(string path, string canonicalPath)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedPublicUserPageContent(databasePath);

            using HttpResponseMessage response = await client.GetAsync(path);

            Assert.Equal(System.Net.HttpStatusCode.Found, response.StatusCode);
            Assert.Equal(canonicalPath, response.Headers.Location?.OriginalString);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionPage_WithNonCanonicalPathAndPageQuery_PreservesPageAndCanonical()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedPublicUserPageContent(databasePath);
            await SeedPublicCollectionPaginationContent(databasePath);
            client.DefaultRequestHeaders.Host = "127.0.0.1";

            using HttpResponseMessage redirectResponse = await client.GetAsync("/MY-DEMO-USER/COLECCIONES/PUBLIC-COLLECTION/?p=2");

            Assert.Equal(System.Net.HttpStatusCode.Found, redirectResponse.StatusCode);
            Assert.Equal($"{PublicCollectionCanonicalPath}?p=2", redirectResponse.Headers.Location?.OriginalString);

            string location = redirectResponse.Headers.Location?.OriginalString ?? string.Empty;
            using HttpResponseMessage canonicalResponse = await client.GetAsync(location);
            string html = System.Net.WebUtility.HtmlDecode(await canonicalResponse.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, canonicalResponse.StatusCode);
            Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:5001/my-demo-user/colecciones/public-collection?p=2\" />", html, StringComparison.Ordinal);
            Assert.Contains("Página 2 de 2", html, StringComparison.Ordinal);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "<article class=\"link-card public-link\">"));
            Assert.Contains("Visible link", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Public page link 00", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionPage_ReturnsNotFoundForPrivateMissingAndEmptyCollections()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedPublicUserPageContent(databasePath);
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                long userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;",
                    new { Email = DemoEmail });
                await connection.ExecuteAsync(
                    "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 1);",
                    new { UserId = userId, Name = "Empty public collection", Slug = "empty-public" });
                await connection.ExecuteAsync(
                    "UPDATE Collection SET Slug = @CurrentSlug WHERE UserId = @UserId AND Slug = @FormerSlug;",
                    new { UserId = userId, FormerSlug = "public-collection", CurrentSlug = "renamed-public-collection" });
            }

            using HttpResponseMessage privateResponse = await client.GetAsync("/my-demo-user/colecciones/private-collection");
            using HttpResponseMessage missingResponse = await client.GetAsync("/my-demo-user/colecciones/missing-collection");
            using HttpResponseMessage emptyResponse = await client.GetAsync("/my-demo-user/colecciones/empty-public");
            using HttpResponseMessage formerSlugResponse = await client.GetAsync("/my-demo-user/colecciones/PUBLIC-COLLECTION");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, privateResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, missingResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, emptyResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, formerSlugResponse.StatusCode);
            Assert.Null(emptyResponse.Headers.Location);
            Assert.Null(formerSlugResponse.Headers.Location);
            Assert.DoesNotContain("Empty public collection", await emptyResponse.Content.ReadAsStringAsync(), StringComparison.Ordinal);
            Assert.DoesNotContain("Visible link", await formerSlugResponse.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UserPage_WithoutRecentPublicLinks_ShowsEmptyStatesAndProfileOnlyNavigation()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/my-demo-user/");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("my demo user todavía no ha creado enlaces", html, StringComparison.Ordinal);
            Assert.Contains("my demo user todavía no ha creado colecciones", html, StringComparison.Ordinal);
            Assert.Matches("(?s)<header class=\"public-heading public-profile-heading\">.*?</header>\\s*<nav class=\"public-navigation\"", html);
            Assert.Contains("aria-current=\"page\">Perfil</a>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("href=\"/my-demo-user/colecciones/\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UserPage_WithPublicCollectionWithoutPublicLinks_HidesCollectionAndNavigation()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                long userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;",
                    new { Email = DemoEmail });
                long collectionId = await connection.ExecuteScalarAsync<long>(
                    @"INSERT INTO Collection (UserId, Name, Slug, IsPublic)
                      VALUES (@UserId, @Name, @Slug, 1);
                      SELECT last_insert_rowid();",
                    new { UserId = userId, Name = "Empty public collection", Slug = "empty-public" });
                await connection.ExecuteAsync(
                    @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt)
                      VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 0, @CreatedAt);",
                    new
                    {
                        UserId = userId,
                        CollectionId = collectionId,
                        Url = "https://links.example/unpublished",
                        Title = "Unpublished link",
                        CreatedAt = DateTime.UtcNow,
                    });
            }

            using HttpResponseMessage profileResponse = await client.GetAsync(PublicUserCanonicalPath);
            string profileHtml = System.Net.WebUtility.HtmlDecode(await profileResponse.Content.ReadAsStringAsync());
            using HttpResponseMessage collectionsResponse = await client.GetAsync(PublicCollectionsCanonicalPath);
            string collectionsHtml = System.Net.WebUtility.HtmlDecode(await collectionsResponse.Content.ReadAsStringAsync());
            using HttpResponseMessage collectionResponse = await client.GetAsync("/my-demo-user/colecciones/empty-public");

            Assert.Equal(System.Net.HttpStatusCode.OK, profileResponse.StatusCode);
            Assert.Contains("my demo user todavía no ha creado enlaces", profileHtml, StringComparison.Ordinal);
            Assert.Contains("my demo user todavía no ha creado colecciones", profileHtml, StringComparison.Ordinal);
            Assert.Contains("aria-current=\"page\">Perfil</a>", profileHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("href=\"/my-demo-user/colecciones/\"", profileHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Empty public collection", profileHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Unpublished link", profileHtml, StringComparison.Ordinal);
            Assert.Equal(System.Net.HttpStatusCode.OK, collectionsResponse.StatusCode);
            Assert.Contains("my demo user todavía no ha creado colecciones", collectionsHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("href=\"/my-demo-user/colecciones/\"", collectionsHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("Empty public collection", collectionsHtml, StringComparison.Ordinal);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, collectionResponse.StatusCode);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UserPage_ReturnsNotFoundForUnknownAndFormerAliases()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            using HttpResponseMessage originalResponse = await client.GetAsync(PublicUserCanonicalPath);
            Assert.Equal(System.Net.HttpStatusCode.OK, originalResponse.StatusCode);

            using HttpResponseMessage unknownAliasResponse = await client.GetAsync("/unknown-user-alias/");
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                await connection.ExecuteAsync(
                    "UPDATE User SET Alias = @Alias WHERE Email = @Email;",
                    new { Alias = "current-demo-alias", Email = DemoEmail });
            }
            using HttpResponseMessage formerAliasResponse = await client.GetAsync("/MY-DEMO-USER/");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, unknownAliasResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, formerAliasResponse.StatusCode);
            Assert.Null(formerAliasResponse.Headers.Location);
            using HttpResponseMessage currentResponse = await client.GetAsync("/current-demo-alias/");
            Assert.Equal(System.Net.HttpStatusCode.OK, currentResponse.StatusCode);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/MY-DEMO-USER/colecciones/", PublicCollectionsCanonicalPath)]
    [InlineData("/MY-DEMO-USER/colecciones/public-collection", PublicCollectionCanonicalPath)]
    public async Task PublicCollectionPages_WhenAliasChanges_ReturnNotFoundAtFormerAlias(string formerPath, string originalPath)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedPublicUserPageContent(databasePath);

            using HttpResponseMessage originalResponse = await client.GetAsync(originalPath);
            Assert.Equal(System.Net.HttpStatusCode.OK, originalResponse.StatusCode);

            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                await connection.ExecuteAsync(
                    "UPDATE User SET Alias = @Alias WHERE Email = @Email;",
                    new { Alias = "current-demo-alias", Email = DemoEmail });
            }

            using HttpResponseMessage formerResponse = await client.GetAsync(formerPath);

            Assert.Equal(System.Net.HttpStatusCode.NotFound, formerResponse.StatusCode);
            Assert.Null(formerResponse.Headers.Location);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/app?alias=another-user")]
    public async Task PrivateSidebar_AliasLinksToCurrentUsersPublicPage(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        const string alias = "my-demo-user";
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync(path);
            string html = await response.Content.ReadAsStringAsync();
            System.Text.RegularExpressions.Match aliasLink = System.Text.RegularExpressions.Regex.Match(
                html, "<small>\\s*<a\\b[^>]*href=\"([^\"]+)\"[^>]*>([^<]+)<svg[^>]*aria-hidden=\"true\"><path d=\"M7 17L17 7\"></path><path d=\"M8 7H17V16\"></path></svg></a>");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.True(aliasLink.Success);
            Assert.Equal($"/{alias}/", aliasLink.Groups[1].Value);
            Assert.Equal("my demo user", aliasLink.Groups[2].Value.Trim());

            using HttpResponseMessage publicResponse = await client.GetAsync(aliasLink.Groups[1].Value);

            Assert.Equal(System.Net.HttpStatusCode.OK, publicResponse.StatusCode);
            Assert.Contains("Enlaces favoritos de my demo user", await publicResponse.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PrivateResourceUnavailable_RendersGenericAccessibleNoticeWithoutInternalDetails()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using PrivateResourceUnavailableWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app/test-resource-unavailable");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("No se ha podido acceder al elemento solicitado.", html, StringComparison.Ordinal);
            Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/enlaces\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_RESOURCE_CODE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_RESOURCE_MESSAGE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("999999", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppNotFoundPage_RendersActiveUserSidebar()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/app/404");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("Demo User", html, StringComparison.Ordinal);
            Assert.Contains("my-demo-user", html, StringComparison.Ordinal);
            Assert.Contains("No se ha podido acceder al recurso solicitado dentro del área privada.", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_RendersRecentLinksFromDashboardUseCase()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                long userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;",
                    new { Email = DemoEmail });
                long collectionId = await connection.ExecuteScalarAsync<long>(
                    "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 0); SELECT last_insert_rowid();",
                    new { UserId = userId, Name = "Dashboard Collection", Slug = "dashboard-collection" });
                await connection.ExecuteAsync(
                    "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, @Description, 0, @CreatedAt);",
                    new
                    {
                        UserId = userId,
                        CollectionId = collectionId,
                        Url = "https://dashboard-link.example",
                        Title = "Example Dashboard Link",
                        Description = "Seeded from dashboard integration test",
                        CreatedAt = DateTime.UtcNow,
                    });
            }

            using HttpResponseMessage response = await client.GetAsync("/app");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Example Dashboard Link", html, StringComparison.Ordinal);
            Assert.Contains("dashboard-link.example", html, StringComparison.Ordinal);
            Assert.Contains("Seeded from dashboard integration test", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppCollections_RendersSearchAndDatabaseBackedEmptyState()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app/colecciones?q=no-matching-collection");
            string html = await response.Content.ReadAsStringAsync();
            string decodedHtml = System.Net.WebUtility.HtmlDecode(html);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("action=\"/app/colecciones\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"q\"", html, StringComparison.Ordinal);
            Assert.Contains("No se han encontrado colecciones.", decodedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ul class=\"collection-listing\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public void RequestLogging_IncludesPrivateSearch()
    {
        const string privateSearch = "private-search-sentinel-4f9c";

        HttpContext privateSearchContext = new DefaultHttpContext();
        privateSearchContext.Request.Path = "/app/enlaces";
        privateSearchContext.Request.QueryString = new QueryString($"?q={privateSearch}");

        CapturingDiagnosticContext privateContext = new();
        RequestLoggingDiagnosticContext.Enrich(privateContext, privateSearchContext);

        Assert.Equal("/app/enlaces", privateContext.Properties["RequestPath"]);
        Assert.Equal($"?q={privateSearch}", privateContext.Properties["RequestQueryString"]);
        Assert.DoesNotContain(privateSearch, privateContext.Properties["RequestPath"]?.ToString() ?? string.Empty, StringComparison.Ordinal);
        Assert.Contains(privateSearch, privateContext.Properties["RequestQueryString"]?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RequestLoggingMiddleware_EmitsEventToIsolatedTestSink()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using CapturingRequestLoggingWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app/enlaces");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            LogEvent requestLogEvent = Assert.Single(factory.RequestLogEvents);
            Assert.Equal("GET", Assert.IsType<ScalarValue>(requestLogEvent.Properties["RequestMethod"]).Value);
            Assert.Equal("/app/enlaces", Assert.IsType<ScalarValue>(requestLogEvent.Properties["RequestPath"]).Value);
            Assert.Equal(200, Assert.IsType<ScalarValue>(requestLogEvent.Properties["StatusCode"]).Value);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task RequestLoggingMiddleware_IncludesSearchButOmitsOtherQueryParameters()
    {
        const string searchText = "private-search-sentinel-ca02";
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using CapturingRequestLoggingWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage searchResponse = await client.GetAsync($"/app/enlaces?q={searchText}&p=3&other=other-sentinel");
            using HttpResponseMessage pagingResponse = await client.GetAsync("/app/enlaces?p=2");
            using HttpResponseMessage tokenSearchResponse = await client.GetAsync("/app/enlaces?q=token%3Dexternal-token");

            Assert.Equal(System.Net.HttpStatusCode.OK, searchResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, pagingResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, tokenSearchResponse.StatusCode);

            LogEvent[] requestEvents = factory.RequestLogEvents.ToArray();
            Assert.Equal(3, requestEvents.Length);
            Assert.Equal("/app/enlaces", Assert.IsType<ScalarValue>(requestEvents[0].Properties["RequestPath"]).Value);
            Assert.Equal($"?q={searchText}", Assert.IsType<ScalarValue>(requestEvents[0].Properties["RequestQueryString"]).Value);
            Assert.Contains(searchText, GetRenderedLogEvent(requestEvents[0]), StringComparison.Ordinal);
            Assert.DoesNotContain("other-sentinel", GetRenderedLogEvent(requestEvents[0]), StringComparison.Ordinal);
            Assert.Equal("/app/enlaces", Assert.IsType<ScalarValue>(requestEvents[1].Properties["RequestPath"]).Value);
            Assert.Equal(string.Empty, Assert.IsType<ScalarValue>(requestEvents[1].Properties["RequestQueryString"]).Value);
            Assert.DoesNotContain("?p=2", GetRenderedLogEvent(requestEvents[1]), StringComparison.Ordinal);
            Assert.Equal("/app/enlaces", Assert.IsType<ScalarValue>(requestEvents[2].Properties["RequestPath"]).Value);
            Assert.Contains("token=external-token", Uri.UnescapeDataString(
                Assert.IsType<string>(Assert.IsType<ScalarValue>(requestEvents[2].Properties["RequestQueryString"]).Value)), StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/app/enlaces")]
    [InlineData("/app/colecciones")]
    public void RequestLogging_DoesNotMistakeSearchContentForTokenParameter(string path)
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.QueryString = new QueryString("?q=token%3Dexternal-token");
        CapturingDiagnosticContext diagnosticContext = new();

        RequestLoggingDiagnosticContext.Enrich(diagnosticContext, context);

        Assert.Equal(path, diagnosticContext.Properties["RequestPath"]);
        Assert.Contains("token=external-token", Uri.UnescapeDataString(
            Assert.IsType<string>(diagnosticContext.Properties["RequestQueryString"])), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/app/enlaces")]
    [InlineData("/app/colecciones")]
    public void RequestLogging_PreservesSearchWhenAnotherParameterLooksLikeToken(string path)
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.QueryString = new QueryString("?q=ordinary-search&access_token=external-value");
        CapturingDiagnosticContext diagnosticContext = new();

        RequestLoggingDiagnosticContext.Enrich(diagnosticContext, context);

        Assert.Equal(path, diagnosticContext.Properties["RequestPath"]);
        Assert.Equal("?q=ordinary-search", diagnosticContext.Properties["RequestQueryString"]);
    }

    [Fact]
    public void RequestLogging_OmitsQueryOnOtherPrivatePaths()
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Path = "/app/enlaces/verify";
        context.Request.QueryString = new QueryString("?token=external-value");
        CapturingDiagnosticContext diagnosticContext = new();

        RequestLoggingDiagnosticContext.Enrich(diagnosticContext, context);

        Assert.Equal("/app/enlaces/verify", diagnosticContext.Properties["RequestPath"]);
        Assert.Equal(string.Empty, diagnosticContext.Properties["RequestQueryString"]);
    }

    [Theory]
    [InlineData("/monkey", "?signature=external-value")]
    [InlineData("/code-examples", "?token=external-value")]
    public void RequestLogging_PreservesPublicPathAndQueryWithoutTokenHeuristics(string path, string query)
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        CapturingDiagnosticContext diagnosticContext = new();

        RequestLoggingDiagnosticContext.Enrich(diagnosticContext, context);

        Assert.Equal(path, diagnosticContext.Properties["RequestPath"]);
        Assert.Equal(query, diagnosticContext.Properties["RequestQueryString"]);
    }

    [Fact]
    public async Task RequestLoggingMiddleware_PreservesPublicPathAndQueryWithoutTokenHeuristics()
    {
        const string token = "token-sentinel-ca03";
        const string password = "password-sentinel-ca04";
        const string hash = "unnecessary-hash-sentinel-ca04";
        const string credential = "url-credential-sentinel-ca04";
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using CapturingRequestLoggingWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync(
                $"/test-token-endpoint?token={token}&password={password}&hash={hash}&credential={credential}");
            const string pathToken = "path-token-sentinel-ca03";
            using HttpResponseMessage pathTokenResponse = await client.GetAsync($"/token-endpoint/{pathToken}");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, pathTokenResponse.StatusCode);

            LogEvent[] requestEvents = factory.RequestLogEvents.ToArray();
            Assert.Equal(2, requestEvents.Length);
            Assert.Equal("/test-token-endpoint", Assert.IsType<ScalarValue>(requestEvents[0].Properties["RequestPath"]).Value);
            Assert.Contains($"token={token}", Assert.IsType<string>(Assert.IsType<ScalarValue>(requestEvents[0].Properties["RequestQueryString"]).Value), StringComparison.Ordinal);
            string queryTokenEvent = GetRenderedLogEvent(requestEvents[0]);
            Assert.Contains(token, queryTokenEvent, StringComparison.Ordinal);
            Assert.Equal($"/token-endpoint/{pathToken}", Assert.IsType<ScalarValue>(requestEvents[1].Properties["RequestPath"]).Value);
            Assert.Equal(string.Empty, Assert.IsType<ScalarValue>(requestEvents[1].Properties["RequestQueryString"]).Value);
            string pathTokenEvent = GetRenderedLogEvent(requestEvents[1]);
            Assert.Contains($"/token-endpoint/{pathToken}", pathTokenEvent, StringComparison.Ordinal);
            Assert.Contains(pathToken, pathTokenEvent, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static string GetRenderedLogEvent(LogEvent logEvent)
        => $"{logEvent.RenderMessage()} {string.Join(" ", logEvent.Properties.Values)} {logEvent.Exception}";

    private sealed class CapturingDiagnosticContext : IDiagnosticContext
    {
        public Dictionary<string, object?> Properties { get; } = [];

        public void Set(string name, object? value, bool destructureObjects = false)
            => Properties[name] = value;

        public void SetException(Exception exception)
            => Properties["Exception"] = exception;
    }

    [Fact]
    public async Task AppDashboard_RendersSearchAndDatabaseBackedEmptyState()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app");
            string html = await response.Content.ReadAsStringAsync();
            string decodedHtml = System.Net.WebUtility.HtmlDecode(html);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("action=\"/app/enlaces\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"q\"", html, StringComparison.Ordinal);
            Assert.Contains("Todavía no tienes enlaces guardados.", decodedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("React Documentation", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Crear etiqueta", html, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PresentationPage_RendersAccessiblePresentationComponents()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app/presentacion");
            string html = await response.Content.ReadAsStringAsync();
            string decodedHtml = System.Net.WebUtility.HtmlDecode(html);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("role=\"alert\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-live=\"assertive\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"#email\"", html, StringComparison.Ordinal);
            Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-current=\"page\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/presentacion?q=diseno&status=all&page=1\"", decodedHtml, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/presentacion?q=diseno&status=all&page=3\"", decodedHtml, StringComparison.Ordinal);
            Assert.Contains("method=\"get\"", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("action=\"/app/presentacion\"", html, StringComparison.Ordinal);
            Assert.Contains("for=\"search-links\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"search-links\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"q\"", html, StringComparison.Ordinal);
            Assert.Contains("value=\"diseno\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"status\" value=\"all\"", html, StringComparison.Ordinal);
            Assert.Contains("<button type=\"submit\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"page\"", html, StringComparison.Ordinal);

            using HttpResponseMessage submitResponse = await client.GetAsync("/app/presentacion?q=diseno&status=all");
            Assert.Equal(System.Net.HttpStatusCode.OK, submitResponse.StatusCode);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PutAsync("/app", new StringContent(string.Empty));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("No se ha podido acceder al recurso solicitado dentro del área privada.", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/", System.Net.HttpStatusCode.OK)]
    [InlineData("/app", System.Net.HttpStatusCode.OK)]
    [InlineData("/missing/security-header-check", System.Net.HttpStatusCode.NotFound)]
    public async Task Pages_WithSuccessOrNotFound_IncludeSecurityHeaders(string path, System.Net.HttpStatusCode expectedStatus)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync(path);

            Assert.Equal(expectedStatus, response.StatusCode);
            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
            Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_UsesResolvedIdentityEvenWhenRequestContainsUserId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using IdentityTestWebApplicationFactory factory = new(databasePath, expectedUserId: 42);
            using HttpClient client = factory.CreateClient();
            factory.ResetProviderCallCount();
            using HttpResponseMessage response = await client.GetAsync("/app?userId=99");
            string html = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("Dashboard", html, StringComparison.Ordinal);
            Assert.Equal(42L, factory.CapturedUserIds.Single());
            Assert.Equal(1, factory.ProviderCallCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Privacy")]
    public async Task AnonymousPages_DoNotResolvePrivateIdentity(string path)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using IdentityTestWebApplicationFactory factory = new(databasePath, expectedUserId: 42);
            using HttpClient client = factory.CreateClient();
            factory.ResetProviderCallCount();
            using HttpResponseMessage response = await client.GetAsync(path);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(0, factory.ProviderCallCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenDatabaseIsNew_AppliesMigrationsBeforeProvisioningAndServing()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/");

            Assert.True(response.IsSuccessStatusCode);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
            Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);", new { Email = DemoEmail }));
            Assert.Equal(SqliteMigrationCatalog.Load().Count(), await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenProvisioningConfigurationIsInvalid_DoesNotServeRequests()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using ConfiguredWebApplicationFactory factory = new(databasePath, "invalid-hash");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains("Development identity password hash must be a PHC Argon2id hash", exception.Message, StringComparison.Ordinal);

            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal(SqliteMigrationCatalog.Load().Count(), await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
            Assert.Equal(0L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenEnvironmentIsProduction_ProvisionsIdentityAndServesPrivateArea()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash, environment: "Production");
            using HttpClient client = factory.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/app");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            long users = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail });
            long collections = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);", new { Email = DemoEmail });

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Demo User", html, StringComparison.Ordinal);
            Assert.Contains(DemoEmail, html, StringComparison.Ordinal);
            Assert.Equal((1L, 1L), (users, collections));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PrivateProfile_WhenPersistedNameAndAliasChange_RendersThemAfterRestart()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            long originalUserId;
            await using (ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash))
            {
                using HttpClient client = factory.CreateClient();
                await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
                originalUserId = await connection.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail });
                await connection.ExecuteAsync(
                    "UPDATE User SET Name = @Name, Alias = @Alias WHERE Id = @UserId;",
                    new { Name = "Ana Pérez", Alias = "ana-perez", UserId = originalUserId });

                string html = System.Net.WebUtility.HtmlDecode(await client.GetStringAsync("/app"));

                Assert.Contains("Ana Pérez", html, StringComparison.Ordinal);
                Assert.Contains(DemoEmail, html, StringComparison.Ordinal);
                Assert.Contains("href=\"/ana-perez/\"", html, StringComparison.Ordinal);
            }

            await using (ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash))
            {
                using HttpClient client = factory.CreateClient();
                string html = System.Net.WebUtility.HtmlDecode(await client.GetStringAsync("/app"));
                await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();

                Assert.Contains("href=\"/ana-perez/\"", html, StringComparison.Ordinal);
                Assert.Contains(DemoEmail, html, StringComparison.Ordinal);
                Assert.Equal(originalUserId, await connection.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
                Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User;"));
            }
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenDevelopmentIdentityCannotBeResolved_DoesNotServeRequests()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash, failIdentityResolution: true);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Equal("The configured development identity cannot be resolved.", exception.Message);

            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal(SqliteMigrationCatalog.Load().Count(), await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
            Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";

    [Theory]
    [InlineData("Cafe", "cafe", System.Net.HttpStatusCode.OK)]
    [InlineData("Different", "different", System.Net.HttpStatusCode.NotFound)]
    public async Task PublicCollectionPage_AfterRename_UsesGeneratedSlug(string name, string slug, System.Net.HttpStatusCode oldStatus)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            await SeedPublicUserPageContent(databasePath);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            long id = await connection.QuerySingleAsync<long>("UPDATE Collection SET Name = 'Café', Slug = 'cafe' WHERE Slug = 'public-collection' RETURNING Id;");
            using HttpResponseMessage before = await client.GetAsync("/my-demo-user/colecciones/cafe");
            Assert.Equal(System.Net.HttpStatusCode.OK, before.StatusCode);
            string form = await client.GetStringAsync($"/app/colecciones/{id}/editar");
            string token = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(form,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
            Assert.NotEmpty(token);

            using HttpResponseMessage renamed = await client.PostAsync($"/app/colecciones/{id}/editar", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Input.Name"] = name,
                ["Input.IsPublic"] = "true",
                ["__RequestVerificationToken"] = token,
            }));
            using HttpResponseMessage former = await client.GetAsync("/my-demo-user/colecciones/cafe");
            using HttpResponseMessage current = await client.GetAsync($"/my-demo-user/colecciones/{slug}");
            string html = System.Net.WebUtility.HtmlDecode(await current.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.Redirect, renamed.StatusCode);
            Assert.Equal(oldStatus, former.StatusCode);
            Assert.Null(former.Headers.Location);
            Assert.Equal(System.Net.HttpStatusCode.OK, current.StatusCode);
            Assert.Contains($"Enlaces en {name} de my demo user</h1>", html, StringComparison.Ordinal);
            Assert.Contains($"href=\"https://localhost:5001/my-demo-user/colecciones/{slug}\"", html, StringComparison.Ordinal);
            Assert.Equal(new[] { "Visible link" }, PublicCollectionTitles(html));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PublicCollectionPage_WithThreePages_RendersMiddlePageAndBothNavigationLinks()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await SeedPublicCollectionPaginationContent(databasePath);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            await connection.ExecuteAsync(
                @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt)
                  SELECT UserId, CollectionId, @Url, @Url, @Title, 1, CreatedAt FROM Link WHERE UrlOriginal = 'https://links.example/public';",
                Enumerable.Range(16, 16).Select(index => new { Url = $"https://links.example/page-{index:D2}", Title = $"Public page link {index:D2}" }));

            string html = System.Net.WebUtility.HtmlDecode(await client.GetStringAsync(PublicCollectionCanonicalPath + "?p=2"));

            Assert.Equal(Enumerable.Range(16, 16).Select(index => $"Public page link {index:D2}"), PublicCollectionTitles(html));
            Assert.Contains("Página 2 de 3", html, StringComparison.Ordinal);
            Assert.Contains($"href=\"{PublicCollectionCanonicalPath}\" class=\"pagination__item\">Anterior</a>", html, StringComparison.Ordinal);
            Assert.Contains($"href=\"{PublicCollectionCanonicalPath}?p=3\" class=\"pagination__item\">Siguiente</a>", html, StringComparison.Ordinal);
            Assert.Contains($"rel=\"canonical\" href=\"https://localhost:5001{PublicCollectionCanonicalPath}?p=2\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UserPage_RecentLinksAcrossCollections_IncludeCollectionOutsideRecentFive()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await SeedPublicUserPageContent(databasePath);
            await SeedDistributedPublicContent(databasePath);

            string html = await client.GetStringAsync(PublicUserCanonicalPath);

            Assert.Equal(Enumerable.Range(0, 6).Reverse().Select(index => $"Distributed {index}"),
                System.Text.RegularExpressions.Regex.Matches(html, "<h3><a[^>]*>(Distributed [0-9])<span").Select(match => match.Groups[1].Value));
            Assert.DoesNotContain("href=\"/my-demo-user/colecciones/distributed-5\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"https://links.example/distributed-5\"", html, StringComparison.Ordinal);
            Assert.Equal(5, System.Text.RegularExpressions.Regex.Matches(html, "<article class=\"public-collection\">").Count);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static async Task SeedDistributedPublicContent(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        long userId = await connection.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail });
        DateTime origin = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int index = 0; index < 6; index++)
        {
            long id = await connection.ExecuteScalarAsync<long>(
                "INSERT INTO Collection (UserId, Name, Slug, IsPublic, CreatedAt) VALUES (@UserId, @Name, @Slug, 1, @CreatedAt); SELECT last_insert_rowid();",
                new { UserId = userId, Name = $"Distributed {index}", Slug = $"distributed-{index}", CreatedAt = origin.AddDays(-index) });
            await connection.ExecuteAsync(
                "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 1, @CreatedAt);",
                new { UserId = userId, CollectionId = id, Url = $"https://links.example/distributed-{index}", Title = $"Distributed {index}", CreatedAt = origin.AddDays(index) });
        }
    }

    private static string[] PublicCollectionTitles(string html)
        => System.Text.RegularExpressions.Regex.Matches(html, "<h2><a[^>]*>([^<]*)<span")
            .Select(match => System.Net.WebUtility.HtmlDecode(match.Groups[1].Value)).ToArray();

    private static async Task SeedPublicUserPageContent(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        long userId = await connection.ExecuteScalarAsync<long>(
            "SELECT Id FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });
        long publicCollectionId = await connection.ExecuteScalarAsync<long>(
            @"INSERT INTO Collection (UserId, Name, Slug, Description, IsPublic)
              VALUES (@UserId, @Name, @Slug, @Description, 1);
              SELECT last_insert_rowid();",
            new
            {
                UserId = userId,
                Name = "Public collection",
                Slug = "public-collection",
                Description = "Public collection description",
            });
        long privateCollectionId = await connection.ExecuteScalarAsync<long>(
            @"INSERT INTO Collection (UserId, Name, Slug, Description, IsPublic)
              VALUES (@UserId, @Name, @Slug, @Description, 0);
              SELECT last_insert_rowid();",
            new
            {
                UserId = userId,
                Name = "PRIVATE_COLLECTION_NAME",
                Slug = "private-collection",
                Description = "PRIVATE_COLLECTION_DESCRIPTION",
            });

        await connection.ExecuteAsync(
            @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt)
              VALUES (@UserId, @CollectionId, @Url, @Url, @Title, @Description, @Image, @IsPublic, @CreatedAt);",
            new[]
            {
                new
                {
                    UserId = userId,
                    CollectionId = publicCollectionId,
                    Url = "https://links.example/public",
                    Title = "Visible link",
                    Description = "Visible description",
                    Image = (string?)"https://images.example/public.jpg",
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-3),
                },
                new
                {
                    UserId = userId,
                    CollectionId = publicCollectionId,
                    Url = "https://links.example/private",
                    Title = "PRIVATE_PUBLIC_LINK_TITLE",
                    Description = "PRIVATE_PUBLIC_LINK_DESCRIPTION",
                    Image = (string?)null,
                    IsPublic = false,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                },
                new
                {
                    UserId = userId,
                    CollectionId = privateCollectionId,
                    Url = "https://links.example/private-collection",
                    Title = "PRIVATE_COLLECTION_LINK_TITLE",
                    Description = "PRIVATE_COLLECTION_LINK_DESCRIPTION",
                    Image = (string?)null,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                },
            });
    }

    private static async Task SeedPublicImageAltCases(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        long userId = await connection.ExecuteScalarAsync<long>(
            "SELECT Id FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });
        long collectionId = await connection.ExecuteScalarAsync<long>(
            "SELECT Id FROM Collection WHERE UserId = @UserId AND Slug = @Slug;",
            new { UserId = userId, Slug = "public-collection" });

        await connection.ExecuteAsync(
            @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt)
              VALUES (@UserId, @CollectionId, @Url, @Url, @Title, @Description, @Image, 1, @CreatedAt);",
            new[]
            {
                new
                {
                    UserId = userId,
                    CollectionId = collectionId,
                    Url = "https://links.example/no-title",
                    Title = (string?)null,
                    Description = (string?)null,
                    Image = (string?)"https://images.example/no-title.jpg",
                    CreatedAt = DateTime.UtcNow.AddMinutes(2),
                },
                new
                {
                    UserId = userId,
                    CollectionId = collectionId,
                    Url = "https://links.example/marked-title",
                    Title = (string?)"<em>Marked title</em>",
                    Description = (string?)null,
                    Image = (string?)"https://images.example/marked-title.jpg",
                    CreatedAt = DateTime.UtcNow.AddMinutes(1),
                },
            });
    }

    private static async Task SeedPublicCollectionPaginationContent(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        long userId = await connection.ExecuteScalarAsync<long>(
            "SELECT Id FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });
        long collectionId = await connection.ExecuteScalarAsync<long>(
            "SELECT Id FROM Collection WHERE UserId = @UserId AND Slug = @Slug;",
            new { UserId = userId, Slug = "public-collection" });
        await connection.ExecuteAsync(
            @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt)
              VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 1, @CreatedAt);",
            Enumerable.Range(0, 16).Select(index => new
            {
                UserId = userId,
                CollectionId = collectionId,
                Url = $"https://links.example/page-{index:D2}",
                Title = $"Public page link {index:D2}",
                CreatedAt = DateTime.UtcNow.AddMinutes(index),
            }));
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (string filePath in new[] { databasePath, $"{databasePath}-wal", $"{databasePath}-shm" })
        {
            if (File.Exists(filePath))
            {
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
    }

    private sealed class IdentityTestWebApplicationFactory(string databasePath, long expectedUserId) : WebApplicationFactory<Program>
    {
        private readonly CapturingDashboardRepository _dashboardRepository = new();
        private readonly CountingDevelopmentIdentityProvider _identityProvider = new(expectedUserId);

        public IReadOnlyCollection<long> CapturedUserIds => _dashboardRepository.UserIds;
        public int ProviderCallCount => _identityProvider.CallCount;

        public void ResetProviderCallCount() => _identityProvider.ResetCallCount();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
            builder.ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IDevelopmentIdentityProvider>(_identityProvider));
                services.RemoveAll<ApplicationContracts.IDashboardRepository>();
                services.AddScoped<ApplicationContracts.IDashboardRepository>(_ => _dashboardRepository);
                services.AddScoped<ApplicationContracts.IDashboardService, ApplicationUseCases.DashboardService>();
            });
        }
    }

    private sealed class CountingDevelopmentIdentityProvider(long userId) : IDevelopmentIdentityProvider
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public void ResetCallCount() => Interlocked.Exchange(ref _callCount, 0);

        public Task<Linkubator.Application.Models.DevelopmentIdentityProfile> GetProfile(CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(new Linkubator.Application.Models.DevelopmentIdentityProfile(userId, "Demo User", "my-demo-user", "user@linkubator.test"));
        }
    }

    private sealed class CapturingDashboardRepository : ApplicationContracts.IDashboardRepository
    {
        private readonly ConcurrentQueue<long> _userIds = new();

        public IReadOnlyCollection<long> UserIds => _userIds.ToArray();

        public Task<ApplicationDtos.DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default)
        {
            _userIds.Enqueue(userId);
            return Task.FromResult(new ApplicationDtos.DashboardDto([]));
        }
    }

    private sealed class PrivateResourceUnavailableWebApplicationFactory(string databasePath) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
            builder.ConfigureTestServices(services =>
                services.AddSingleton<IStartupFilter, PrivateResourceUnavailableTestEndpointFilter>());
        }
    }

    private sealed class PrivateResourceUnavailableTestEndpointFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => app =>
            {
                next(app);
                app.UseEndpoints(endpoints => endpoints.MapGet("/app/test-resource-unavailable", async context =>
                {
                    Linkubator.Application.Results.ApplicationError error = Linkubator.Application.Results.ApplicationError.ResourceNotAccessible(
                        "PRIVATE_RESOURCE_CODE",
                        "999999",
                        "PRIVATE_RESOURCE_MESSAGE");
                    Linkubator.Web.Models.Presentation.PrivateResourceUnavailableViewModel? viewModel =
                        Linkubator.Web.Models.Presentation.PrivateResourceUnavailablePresenter.Create(error, "/app/enlaces");
                    Linkubator.Web.Models.Presentation.PrivateResourceUnavailableViewModel? validationErrorViewModel =
                        Linkubator.Web.Models.Presentation.PrivateResourceUnavailablePresenter.Create(
                            Linkubator.Application.Results.ApplicationError.Validation("PRIVATE_VALIDATION_CODE", "PRIVATE_FIELD", "PRIVATE_VALIDATION_MESSAGE"),
                            "/app/enlaces");
                    if (validationErrorViewModel is not null)
                    {
                        throw new InvalidOperationException("Validation errors must not use the resource-unavailable presenter.");
                    }
                    Microsoft.AspNetCore.Mvc.Razor.IRazorViewEngine viewEngine = context.RequestServices
                        .GetRequiredService<Microsoft.AspNetCore.Mvc.Razor.IRazorViewEngine>();
                    Microsoft.AspNetCore.Mvc.ViewEngines.ViewEngineResult viewResult = viewEngine.GetView(
                        executingFilePath: null,
                        viewPath: "~/Pages/Shared/_PrivateResourceUnavailable.cshtml",
                        isMainPage: false);
                    if (!viewResult.Success)
                    {
                        throw new InvalidOperationException("The private resource notice partial was not found.");
                    }

                    Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState = new();
                    Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary viewData = new(
                        context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider>(),
                        modelState)
                    {
                        Model = viewModel,
                    };
                    Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider tempDataProvider = context.RequestServices
                        .GetRequiredService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>();
                    Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary tempData = new(context, tempDataProvider);
                    Microsoft.AspNetCore.Mvc.ActionContext actionContext = new(
                        context,
                        new Microsoft.AspNetCore.Routing.RouteData(),
                        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                        modelState);
                    using StringWriter writer = new();
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = new(
                        actionContext,
                        viewResult.View,
                        viewData,
                        tempData,
                        writer,
                        new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions());
                    await viewResult.View.RenderAsync(viewContext);
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(writer.ToString());
                }));
            };
    }

    private sealed class CapturingRequestLoggingWebApplicationFactory(string databasePath, string passwordHash) : WebApplicationFactory<Program>
    {
        private readonly ConcurrentQueue<LogEvent> _requestLogEvents = new();

        public IReadOnlyCollection<LogEvent> RequestLogEvents => _requestLogEvents;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", passwordHash);
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IStartupFilter, RequestLoggingTestEndpointFilter>();
                services.AddSingleton<Serilog.ILogger>(_ => new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Sink(new CapturingLogEventSink(_requestLogEvents))
                    .CreateLogger());
                // Other test hosts can replace Serilog's global logger while these requests run.
                services.Replace(ServiceDescriptor.Singleton<Serilog.Extensions.Hosting.DiagnosticContext>(provider =>
                    new Serilog.Extensions.Hosting.DiagnosticContext(provider.GetRequiredService<Serilog.ILogger>())));
                services.AddOptions<Serilog.AspNetCore.RequestLoggingOptions>()
                    .Configure<Serilog.ILogger>((options, logger) => options.Logger = logger);
            });
        }

        private sealed class CapturingLogEventSink(ConcurrentQueue<LogEvent> target) : ILogEventSink
        {
            public void Emit(LogEvent logEvent) => target.Enqueue(logEvent);
        }

        private sealed class RequestLoggingTestEndpointFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
                => app =>
                {
                    next(app);
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/app/enlaces", context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            return context.Response.WriteAsync("ok");
                        });

                        endpoints.MapGet("/app/enlaces/verify", context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            return context.Response.WriteAsync("ok");
                        });

                        endpoints.MapGet("/test-token-endpoint", context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            return context.Response.WriteAsync("ok");
                        });

                        endpoints.MapGet("/token-endpoint/{tokenValue}", context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            return context.Response.WriteAsync("ok");
                        });
                    });
                };
        }
    }

    private sealed class ConfiguredWebApplicationFactory(string databasePath, string passwordHash, bool failIdentityResolution = false, string? environment = null) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (environment is not null)
            {
                builder.UseEnvironment(environment);
            }
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", passwordHash);

            if (failIdentityResolution)
            {
                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IDevelopmentIdentityProvider, UnresolvableDevelopmentIdentityProvider>());
            }
        }
    }

    private sealed class UnresolvableDevelopmentIdentityProvider : IDevelopmentIdentityProvider
    {
        public Task<Linkubator.Application.Models.DevelopmentIdentityProfile> GetProfile(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("The configured development identity cannot be resolved.");
    }
}