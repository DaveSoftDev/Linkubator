namespace Linkubator.Web.Models.Presentation;

public sealed class PageAlertViewModel
{
    public string Message { get; init; } = string.Empty;

    public string Kind { get; init; } = "info";

    public PageAlertViewModel AppendError(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new PageAlertViewModel
        {
            Message = string.IsNullOrWhiteSpace(Message) ? message : $"{Message} {message}",
            Kind = "error",
        };
    }
}
