namespace Linkubator.Web.DevelopmentIdentity;

public interface IWebUserProfile
{
    string Name { get; }

    string Alias { get; }
    string Email { get; }
}
