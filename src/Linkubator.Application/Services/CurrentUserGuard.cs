using Linkubator.Application.Logging;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.Services;

public static class CurrentUserGuard
{
    public static bool EnsureAuthenticated(ICurrentUser currentUser, ILogger logger, Func<ApplicationError> notAuthenticatedFactory, string message, out Result failure)
    {
        if (!currentUser.IsAuthenticated)
        {
            SafeLogger.Log(logger, LogLevel.Warning, null, message, new { UserId = currentUser.UserId });
            failure = Result.Failure(notAuthenticatedFactory());
            return false;
        }

        failure = default!;
        return true;
    }

    public static bool EnsureAuthenticated<T>(ICurrentUser currentUser, ILogger logger, Func<ApplicationError> notAuthenticatedFactory, string message, out Result<T> failure)
    {
        if (!currentUser.IsAuthenticated)
        {
            SafeLogger.Log(logger, LogLevel.Warning, null, message, new { UserId = currentUser.UserId });
            failure = Result<T>.Failure(notAuthenticatedFactory());
            return false;
        }

        failure = default!;
        return true;
    }
}
