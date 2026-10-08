namespace Linkubator.Domain;

public static class UserTextPolicy
{
    public static string? TrimToNull(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static int CountCodePoints(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        int count = 0;
        int index = 0;
        while (index < value.Length)
        {
            count++;

            if (char.IsHighSurrogate(value[index]) && index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
            {
                index++;
            }

            index++;
        }

        return count;
    }

    public static bool IsWithinMaximumLength(string value, int maximum)
    {
        return CountCodePoints(value) <= maximum;
    }
}