using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationEnums = Linkubator.Application.Enums;
using ApplicationResults = Linkubator.Application.Results;
using WebOptions = Linkubator.Web.Options;

namespace Linkubator.Web.Pages.Public;

public sealed class UserModel : PageModel
{
    private readonly ApplicationContracts.IPublicReadService _publicReadService;
    private readonly WebOptions.PublicOriginOptions _publicOriginOptions;

    public UserModel(
        ApplicationContracts.IPublicReadService publicReadService,
        IOptions<WebOptions.PublicOriginOptions> publicOriginOptions)
    {
        ArgumentNullException.ThrowIfNull(publicReadService);
        ArgumentNullException.ThrowIfNull(publicOriginOptions);
        _publicReadService = publicReadService;
        _publicOriginOptions = publicOriginOptions.Value;
    }

    public ApplicationDtos.PublicUserDto PublicUser { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string alias, CancellationToken cancellationToken)
    {
        string normalizedAlias = alias.ToLowerInvariant();
        ApplicationResults.Result<ApplicationDtos.PublicUserDto> result =
            await _publicReadService.GetUserPage(normalizedAlias, cancellationToken);

        if (result.Errors.Any(error => error.Category == ApplicationEnums.ApplicationErrorCategory.ResourceNotAccessible))
        {
            return NotFound();
        }

        if (!result.IsSuccess || result.Value is null)
        {
            throw new InvalidOperationException("The public user page could not be loaded.");
        }

        PublicUser = result.Value;
        string canonicalPath = $"/{Uri.EscapeDataString(PublicUser.Alias)}/";
        if (!string.Equals(Request.Path.Value, canonicalPath, StringComparison.Ordinal))
        {
            return LocalRedirect($"{canonicalPath}{Request.QueryString}");
        }

        string displayAlias = Linkubator.Web.Extensions.AliasExtensions.GetDisplayName(PublicUser.Alias);
        ViewData["Title"] = $"Los enlaces favoritos de {displayAlias} en Linkubator";
        ViewData["MetaTitle"] = $"Los enlaces favoritos de {displayAlias} en Linkubator";
        ViewData["MetaDescription"] = $"Los enlaces favoritos más importantes de {displayAlias} en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página";
        ViewData["CanonicalUrl"] = GetCanonicalUrl(PublicUser.Alias);

        return Page();
    }

    private string GetCanonicalUrl(string alias)
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

        return $"{publicOrigin.GetLeftPart(UriPartial.Authority)}/{Uri.EscapeDataString(alias)}/";
    }
}
