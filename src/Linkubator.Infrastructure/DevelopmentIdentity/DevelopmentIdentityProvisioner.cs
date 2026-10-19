using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Linkubator.Domain.Constants;
using Linkubator.Domain.Policies;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.Options;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.DevelopmentIdentity;

public sealed class DevelopmentIdentityProvisioner
{
    private const string DefaultInboxCollectionName = "Bandeja de entrada";
    private const string DefaultInboxCollectionSlug = "bandeja-de-entrada";
    private const string ProvisioningResourceName = "Linkubator.Infrastructure.DevelopmentIdentity.Sql.ProvisionDevelopmentIdentity.sql";
    private static readonly Regex _argon2idHashPattern = new(
        @"^\$argon2id\$v=19\$m=(\d+),t=(\d+),p=(\d+)\$([A-Za-z0-9+/_-]+)\$([A-Za-z0-9+/_-]+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly DevelopmentIdentityOptions _options;

    public DevelopmentIdentityProvisioner(ISqliteConnectionFactory connectionFactory, DevelopmentIdentityOptions options)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(options);

        _connectionFactory = connectionFactory;
        _options = options;
        ValidateOptions(_options);
    }

    public async Task Provision()
    {
        using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
        await connection.ExecuteAsync("BEGIN IMMEDIATE;");
        try
        {
            string sql = LoadProvisioningSql();
            await connection.ExecuteAsync(sql, new
            {
                Email = _options.Email,
                Name = _options.Name,
                Alias = _options.Alias,
                PasswordHash = _options.PasswordHash,
                EmailConfirmedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                CollectionName = DefaultInboxCollectionName,
                CollectionSlug = DefaultInboxCollectionSlug,
            });
            await connection.ExecuteAsync("COMMIT;");
        }
        catch
        {
            await connection.ExecuteAsync("ROLLBACK;");
            throw;
        }
    }

    private static void ValidateOptions(DevelopmentIdentityOptions options)
    {
        options.Email = EmailPolicy.Normalize(options.Email);
        options.Name = UserTextPolicy.EnsureRequiredText(options.Name, LengthLimitsConstants.UserName);
        options.Alias = AliasPolicy.Generate(options.Alias);

        Match hashMatch = string.IsNullOrWhiteSpace(options.PasswordHash)
            ? Match.Empty
            : _argon2idHashPattern.Match(options.PasswordHash);

        if (!hashMatch.Success
            || !int.TryParse(hashMatch.Groups[1].Value, out int memoryKiB)
            || memoryKiB < 19456
            || memoryKiB > 131072
            || !int.TryParse(hashMatch.Groups[2].Value, out int iterations)
            || iterations < 2
            || iterations > 10
            || !int.TryParse(hashMatch.Groups[3].Value, out int parallelism)
            || parallelism != 1)
        {
            throw new InvalidOperationException("Development identity password hash must be a PHC Argon2id hash with the configured parameters, a 16-byte salt and a 32-byte digest.");
        }

        if (DecodeBase64(hashMatch.Groups[4].Value).Length != 16 || DecodeBase64(hashMatch.Groups[5].Value).Length != 32)
        {
            throw new InvalidOperationException("Development identity password hash has invalid Base64 salt or digest data.");
        }
    }

    private static byte[] DecodeBase64(string value)
    {
        string standardBase64 = value.Replace('-', '+').Replace('_', '/');
        string paddedValue = standardBase64.PadRight(standardBase64.Length + ((4 - standardBase64.Length % 4) % 4), '=');
        try
        {
            return Convert.FromBase64String(paddedValue);
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }

    private static string LoadProvisioningSql()
    {
        Assembly assembly = typeof(DevelopmentIdentityProvisioner).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(ProvisioningResourceName)
            ?? throw new InvalidOperationException($"Development identity SQL resource '{ProvisioningResourceName}' could not be loaded.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
