using System.Collections;
using System.Reflection;
using Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.Logging;

public static class SafeLogger
{
    /// <summary>
    /// Registra un evento de aplicación con el contexto filtrado para evitar exponer datos sensibles.
    /// </summary>
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

    /// <summary>
    /// Añade el identificador de usuario al mensaje cuando el contexto lo proporciona.
    /// </summary>
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

    /// <summary>
    /// Crea una vista previa limitada y segura del texto, escapando sus caracteres de control.
    /// </summary>
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

    /// <summary>
    /// Construye el contexto de registro a partir del usuario, la solicitud y los datos adicionales.
    /// </summary>
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

    /// <summary>
    /// Añade al contexto las propiedades públicas de un objeto después de sanitizarlas.
    /// </summary>
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

    /// <summary>
    /// Sanitiza un valor para su inclusión segura en el contexto de registro.
    /// </summary>
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

    /// <summary>
    /// Sanitiza los elementos de una secuencia antes de incluirlos en el contexto de registro.
    /// </summary>
    private static List<object?> SanitizeEnumerable(IEnumerable enumerableValue)
    {
        List<object?> sanitizedItems = new List<object?>();

        foreach (object? item in enumerableValue)
        {
            sanitizedItems.Add(item is string itemAsString ? EscapeControls(itemAsString) : item);
        }

        return sanitizedItems;
    }

    /// <summary>
    /// Escapa los caracteres de control de una cadena.
    /// </summary>
    private static string EscapeControls(string value)
        => value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

    private readonly record struct SafeLogState(string Message, IReadOnlyDictionary<string, object?> Values)
        : IReadOnlyCollection<KeyValuePair<string, object?>>
    {
        public int Count => Values.Count;

        /// <summary>
        /// Obtiene un enumerador de los valores del estado de registro.
        /// </summary>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            => Values.GetEnumerator();

        /// <summary>
        /// Obtiene un enumerador de los valores del estado de registro.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
