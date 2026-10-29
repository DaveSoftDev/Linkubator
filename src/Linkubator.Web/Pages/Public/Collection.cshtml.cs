using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationEnums = Linkubator.Application.Enums;
using ApplicationResults = Linkubator.Application.Results;
using WebOptions = Linkubator.Web.Options;

namespace Linkubator.Web.Pages.Public;

public sealed class CollectionModel : PageModel
{
    private readonly ApplicationContracts.IPublicReadService _publicReadService;
    private readonly WebOptions.PublicOriginOptions _publicOriginOptions;

    public CollectionModel(
        ApplicationContracts.IPublicReadService publicReadService,
        IOptions<WebOptions.PublicOriginOptions> publicOriginOptions)
    {
        ArgumentNullException.ThrowIfNull(publicReadService);
        ArgumentNullException.ThrowIfNull(publicOriginOptions);
        _publicReadService = publicReadService;
        _publicOriginOptions = publicOriginOptions.Value;
    }

    public string Alias { get; private set; } = string.Empty;

    public string CollectionName { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public IReadOnlyList<ApplicationDtos.PublicLinkDto> Links { get; private set; } = [];

    public int PageNumber { get; private set; } = 1;

    public int PageCount { get; private set; } = 1;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < PageCount;

    public async Task<IActionResult> OnGetAsync(string alias, string slug, CancellationToken cancellationToken)
    {
        Alias = alias.ToLowerInvariant();
        Slug = slug.ToLowerInvariant();

        ApplicationResults.Result<int> countResult = await _publicReadService.GetCollectionPageCount(Alias, Slug, cancellationToken);
        if (countResult.Errors.Any(error => error.Category == ApplicationEnums.ApplicationErrorCategory.ResourceNotAccessible))
        {
            return NotFound();
        }

        if (!countResult.IsSuccess)
        {
            throw new InvalidOperationException("The public collection page count could not be loaded.");
        }

        if (countResult.Value < 1)
        {
            return NotFound();
        }

        PageCount = countResult.Value;
        int requestedPage = TryGetRequestedPage();
        if (requestedPage > PageCount)
        {
            requestedPage = PageCount;
        }

        PageNumber = requestedPage;

        ApplicationResults.Result<ApplicationDtos.PublicCollectionDto> result =
            await _publicReadService.GetCollectionPage(Alias, Slug, PageNumber, cancellationToken);

        if (result.Errors.Any(error => error.Category == ApplicationEnums.ApplicationErrorCategory.ResourceNotAccessible))
        {
            return NotFound();
        }

        if (!result.IsSuccess || result.Value is null)
        {
            throw new InvalidOperationException("The public collection page could not be loaded.");
        }

        ApplicationDtos.PublicCollectionDto collection = result.Value;
        if (collection.Links.Count == 0)
        {
            return NotFound();
        }

        CollectionName = collection.CollectionName;
        Slug = collection.Slug;
        Links = collection.Links;

        string canonicalPath = $"/{Uri.EscapeDataString(Alias)}/colecciones/{Uri.EscapeDataString(Slug)}";
        if (!string.Equals(Request.Path.Value, canonicalPath, StringComparison.Ordinal))
        {
            return LocalRedirect($"{canonicalPath}{Request.QueryString}");
        }

        string displayAlias = Linkubator.Web.Extensions.AliasExtensions.GetDisplayName(Alias);
        ViewData["Title"] = $"Los enlaces de {displayAlias} para {CollectionName} en Linkubator";
        ViewData["MetaTitle"] = $"Los enlaces de {displayAlias} para {CollectionName} en Linkubator";
        ViewData["MetaDescription"] = $"Los enlaces favoritos de {displayAlias} en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página";
        ViewData["CanonicalUrl"] = GetCanonicalUrl(Alias, Slug, PageNumber);

        return Page();
    }

    private int TryGetRequestedPage()
    {
        if (!Request.Query.TryGetValue("p", out var pageValues) || pageValues.Count == 0)
        {
            return 1;
        }

        string? candidate = pageValues[0];
        if (string.IsNullOrEmpty(candidate)
            || candidate.Any(character => character is < '0' or > '9')
            || !int.TryParse(candidate, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedPage)
            || parsedPage < 1)
        {
            return 1;
        }

        return parsedPage;
    }

    private string GetCanonicalUrl(string alias, string slug, int page)
    {
        if (!Uri.TryCreate(_publicOriginOptions.Origin, UriKind.Absolute, out Uri? publicOrigin)
            || (publicOrigin.Scheme != Uri.UriSchemeHttp && publicOrigin.Scheme != Uri.UriSchemeHttps)
            || publicOrigin.AbsolutePath != "/"
            || !string.IsNullOrEmpty(publicOrigin.Query)
            || !string.IsNullOrEmpty(publicOrigin.Fragment)
            || !string.IsNullOrEmpty(publicOrigin.UserInfo))
        {
            throw new InvalidOperationException("PublicOrigin must be configured as an absolute HTTP(S) origin.");
        }

        string canonicalPath = $"/{Uri.EscapeDataString(alias)}/colecciones/{Uri.EscapeDataString(slug)}";
        return page <= 1
            ? $"{publicOrigin.GetLeftPart(UriPartial.Authority)}{canonicalPath}"
            : $"{publicOrigin.GetLeftPart(UriPartial.Authority)}{canonicalPath}?p={page.ToString(CultureInfo.InvariantCulture)}";
    }
}
