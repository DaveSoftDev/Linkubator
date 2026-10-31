using System.Reflection;
using System.Text.RegularExpressions;

namespace Linkubator.Infrastructure.Sqlite;

internal static partial class SqliteMigrationCatalog
{
    private static readonly Regex _versionRegex = VersionPattern();

    /// <summary>
    /// Carga y ordena los scripts de migración SQL incrustados en el ensamblado.
    /// </summary>
    public static IReadOnlyList<SqliteMigrationScript> Load()
    {
        TypeInfo typeInfo = typeof(SqliteMigrationService).GetTypeInfo();
        Assembly assembly = typeInfo.Assembly;

        return assembly
            .GetManifestResourceNames()
            .Where(resourceName => resourceName.StartsWith("Linkubator.Infrastructure.Sqlite.Migrations.", StringComparison.Ordinal)
                && resourceName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(resourceName => resourceName, StringComparer.Ordinal)
            .Select(resourceName =>
            {
                using Stream stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Embedded migration resource '{resourceName}' could not be loaded.");
                using StreamReader reader = new(stream);
                string sql = reader.ReadToEnd();
                string resourcePath = resourceName.Replace('\\', '/');
                string resourceFileName = Path.GetFileNameWithoutExtension(resourcePath);
                Match match = _versionRegex.Match(resourceFileName);
                if (!match.Success)
                {
                    throw new InvalidOperationException($"Embedded migration resource '{resourceName}' does not follow the expected VNNN naming convention.");
                }

                return new SqliteMigrationScript(match.Value, sql);
            })
            .ToArray();
    }

    /// <summary>
    /// Proporciona la expresión regular generada para extraer versiones de migración.
    /// </summary>
    [GeneratedRegex("(?i)V\\d+")]
    private static partial Regex VersionPattern();
}
