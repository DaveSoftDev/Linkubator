using System.Collections;
using System.Reflection;
using Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.Logging;

public static class SafeLogger
{
    public static void Log(ILogger logger, LogLevel level, Exception? exception, string message, object? userContext, object? request = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        string enrichedMessage = AppendUserIdToMessage(message, userContext);
        Dictionary<string, object?> context = BuildContext(userContext, request);
        if (exception is not null)
        {
            context["ExceptionType"] = exception.GetType().Name;

            if (exception is DomainException domainException)
            {
                context["DomainErrorCode"] = domainException.Code;
            }
        }

        SafeLogState state = new(enrichedMessage, context);

        logger.Log(level, new EventId(), state, exception, static (entry, _) => entry.Message);
    }

    private static string AppendUserIdToMessage(string message, object? userContext)
    {
        if (string.IsNullOrEmpty(message) || userContext is null)
        {
            return message;
        }

        PropertyInfo? userIdProperty = userContext.GetType().GetProperty("UserId", BindingFlags.Instance | BindingFlags.Public);
        if (userIdProperty is null)
        {
            return message;
        }

        object? userIdValue = userIdProperty.GetValue(userContext);
        if (userIdValue is null)
        {
            return message;
        }

        return $"{message} [UserId={userIdValue}]";
    }

    public static string SafePreview(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string escapedValue = value
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");

        if (escapedValue.Length <= maxLength)
        {
            return escapedValue;
        }

        if (maxLength <= 3)
        {
            return escapedValue[..maxLength];
        }

        return escapedValue[..(maxLength - 3)] + "...";
    }

    private static Dictionary<string, object?> BuildContext(object? userContext, object? request, object? additionalContext = null)
    {
        Dictionary<string, object?> context = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (userContext is not null)
        {
            AddPropertiesToContext(context, userContext);
        }

        if (request is not null)
        {
            AddPropertiesToContext(context, request);
        }

        if (additionalContext is not null)
        {
            AddPropertiesToContext(context, additionalContext);
        }

        if (context.Count == 0)
        {
            context["HasValue"] = false;
        }

        return context;
    }

    private static void AddPropertiesToContext(Dictionary<string, object?> context, object source)
    {
        Type sourceType = source.GetType();
        PropertyInfo[] properties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (PropertyInfo property in properties)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? propertyValue = property.GetValue(source);
            context[property.Name] = SanitizePropertyValue(propertyValue);
        }
    }

    private static object? SanitizePropertyValue(object? propertyValue)
    {
        return propertyValue switch
        {
            null => null,
            string stringValue => EscapeControls(stringValue),
            bool or byte or short or int or long or float or double or decimal or Guid or DateTime or DateTimeOffset => propertyValue,
            Enum => propertyValue,
            IEnumerable enumerableValue => SanitizeEnumerable(enumerableValue),
            _ => propertyValue.GetType().Name
        };
    }

    private static List<object?> SanitizeEnumerable(IEnumerable enumerableValue)
    {
        List<object?> sanitizedItems = new List<object?>();

        foreach (object? item in enumerableValue)
        {
            sanitizedItems.Add(item is string itemAsString ? EscapeControls(itemAsString) : item);
        }

        return sanitizedItems;
    }

    private static string EscapeControls(string value)
        => value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

    private readonly record struct SafeLogState(string Message, IReadOnlyDictionary<string, object?> Values)
        : IReadOnlyCollection<KeyValuePair<string, object?>>
    {
        public int Count => Values.Count;

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            => Values.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
