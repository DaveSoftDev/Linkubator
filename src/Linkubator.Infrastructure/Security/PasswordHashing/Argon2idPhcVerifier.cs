using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Konscious.Security.Cryptography;

namespace Linkubator.Infrastructure.Security.PasswordHashing;

public static class Argon2idPhcVerifier
{
    private static readonly Regex _phcPattern = new(
        @"^\$argon2id\$v=19\$m=(\d+),t=(\d+),p=(\d+)\$([A-Za-z0-9+/_-]+)\$([A-Za-z0-9+/_-]+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Verifica una contraseña frente a un hash Argon2id en formato PHC.
    /// </summary>
    public static bool Verify(string password, string phcHash)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(phcHash);

        Match match = _phcPattern.Match(phcHash);
        if (!match.Success
            || !int.TryParse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture, out int memoryKiB)
            || memoryKiB < 19456
            || memoryKiB > 131072
            || !int.TryParse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture, out int iterations)
            || iterations < 2
            || iterations > 10
            || !int.TryParse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture, out int parallelism)
            || parallelism != 1)
        {
            return false;
        }

        byte[] salt = DecodeBase64(match.Groups[4].Value);
        byte[] expectedDigest = DecodeBase64(match.Groups[5].Value);
        if (salt.Length != 16 || expectedDigest.Length != 32)
        {
            CryptographicOperations.ZeroMemory(salt);
            CryptographicOperations.ZeroMemory(expectedDigest);
            return false;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] actualDigest = Array.Empty<byte>();
        try
        {
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                DegreeOfParallelism = parallelism,
                MemorySize = memoryKiB,
                Iterations = iterations,
            };

            actualDigest = argon2.GetBytes(expectedDigest.Length);
            return CryptographicOperations.FixedTimeEquals(actualDigest, expectedDigest);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
            CryptographicOperations.ZeroMemory(salt);
            CryptographicOperations.ZeroMemory(expectedDigest);
            CryptographicOperations.ZeroMemory(actualDigest);
        }
    }

    /// <summary>
    /// Decodifica una cadena Base64 segura para URL; devuelve un arreglo vacío si el formato no es válido.
    /// </summary>
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
}