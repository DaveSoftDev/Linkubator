using Linkubator.Application.Results;
using LinkErrorMessages = Linkubator.Web.Constants.LinkErrorMessages;

namespace Linkubator.Web.Models.Presentation;

internal static class LinkValidationErrorMessagePresenter
{
    public static string ResolveFieldMessage(ApplicationError error, string fallbackMessage)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(fallbackMessage);

        return error.Code == LinkErrorMessages.UrlSchemeNotAllowedCode
            ? LinkErrorMessages.UrlSchemeNotAllowed
            : fallbackMessage;
    }
}
