namespace Linkubator.Web.Models.AppSidebar;

public sealed class AppSidebarUserViewModel
{
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[0].ToString().ToUpperInvariant();

    public string Name { get; init; } = string.Empty;
    public string Alias { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
