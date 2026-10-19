using System.Reflection;
using System.Text.RegularExpressions;

namespace Linkubator.Infrastructure.Sqlite;

internal static partial class SqliteMigrationCatalog
{
    private static readonly Regex VersionRegex = VersionPattern();

    public static IReadOnlyList<SqliteMigrationScript> Load()
    {
        TypeInfo typeInfo = typeof(SqliteMigrationService).GetTypeInfo();
        Assembly assembly = typeInfo.Assembly;

        return assembly
            .GetManifestResourceNames()
            .Where(resourceName => resourceName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Where(resourceName => resourceName.Contains(".Sqlite.Migrations.", StringComparison.OrdinalIgnoreCase))
            .OrderBy(resourceName => resourceName, StringComparer.Ordinal)
            .Select(resourceName =>
            {
                using Stream stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Embedded migration resource '{resourceName}' could not be loaded.");
                using StreamReader reader = new(stream);
                string sql = reader.ReadToEnd();
                string resourceFileName = Path.GetFileNameWithoutExtension(resourceName);
                Match match = VersionRegex.Match(resourceFileName);
                if (!match.Success)
                {
                    throw new InvalidOperationException($"Embedded migration resource '{resourceName}' does not follow the expected VNNN naming convention.");
                }

                return new SqliteMigrationScript(match.Value, sql);
            })
            .ToArray();
    }

    [GeneratedRegex("(?i)V\\d+")]
    private static partial Regex VersionPattern();
}
