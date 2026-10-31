using System.Globalization;

namespace Linkubator.Web.Models.Presentation;

public static class PaginationViewModelFactory
{
    /// <summary>
    /// Crea un modelo para presentar de forma genérica que un recurso privado no está disponible.
    /// </summary>
    public static PaginationViewModel Create(int currentPage, int pageCount, Func<int?, string> buildPageUrl)
    {
        if (pageCount <= 1)
        {
            return new PaginationViewModel();
        }

        List<PaginationPageLinkViewModel> pages = [];
        if (currentPage > 1)
        {
            pages.Add(new PaginationPageLinkViewModel { Label = "Anterior", Url = buildPageUrl(currentPage == 2 ? null : currentPage - 1) });
        }

        for (int page = 1; page <= pageCount; page++)
        {
            pages.Add(new PaginationPageLinkViewModel
            {
                Label = page.ToString(CultureInfo.InvariantCulture),
                Url = page == currentPage ? null : buildPageUrl(page == 1 ? null : page),
                IsCurrent = page == currentPage,
            });
        }

        if (currentPage < pageCount)
        {
            pages.Add(new PaginationPageLinkViewModel { Label = "Siguiente", Url = buildPageUrl(currentPage + 1) });
        }

        return new PaginationViewModel { Pages = pages };
    }
}
