namespace Linkubator.Web.Models.Pagination;

internal static class PageNumberParser
{
    public static bool TryParse(string value, out int page)
    {
        page = 0;
        foreach (char character in value)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }

            int digit = character - '0';
            page = page > (int.MaxValue - digit) / 10 ? int.MaxValue : (page * 10) + digit;
        }

        return page > 0;
    }
}
