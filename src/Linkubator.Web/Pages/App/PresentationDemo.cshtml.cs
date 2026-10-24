using Linkubator.Web.Models.Presentation;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Linkubator.Web.Pages.App;

public sealed class PresentationDemoModel : PageModel
{
    public PageErrorSummaryViewModel ErrorSummary { get; private set; } = new()
    {
        Title = "Hay errores en el formulario",
        Items =
        [
            new PageErrorSummaryItemViewModel { Message = "La dirección de correo es obligatoria.", FieldId = "email" },
            new PageErrorSummaryItemViewModel { Message = "El nombre no puede estar vacío." }
        ]
    };

    public PageAlertViewModel Alert { get; private set; } = new()
    {
        Message = "Se ha guardado la configuración correctamente.",
        Kind = "success"
    };

    public EmptyStateViewModel EmptyState { get; private set; } = new()
    {
        Title = "No hay enlaces todavía",
        Description = "Añade tu primer recurso para empezar a organizarlo."
    };

    public PaginationViewModel Pagination { get; private set; } = new()
    {
        Pages =
        [
            new PaginationPageLinkViewModel { Label = "Anterior", Url = "/app/presentacion?page=1", IsCurrent = false, IsDisabled = false },
            new PaginationPageLinkViewModel { Label = "1", Url = "/app/presentacion?page=1", IsCurrent = false, IsDisabled = false },
            new PaginationPageLinkViewModel { Label = "2", Url = "/app/presentacion?page=2", IsCurrent = true, IsDisabled = false },
            new PaginationPageLinkViewModel { Label = "3", Url = "/app/presentacion?page=3", IsCurrent = false, IsDisabled = false },
            new PaginationPageLinkViewModel { Label = "Siguiente", Url = "/app/presentacion?page=3", IsCurrent = false, IsDisabled = false }
        ]
    };

    public SearchFormViewModel SearchForm { get; private set; } = new()
    {
        Action = "/app/presentacion",
        Method = "get",
        Label = "Buscar enlaces",
        Placeholder = "Busca por título o URL",
        ButtonText = "Buscar",
        QueryParameterName = "q",
        QueryValue = "diseno",
        QueryInputId = "search-links",
        HiddenFields =
        [
            new KeyValuePair<string, string>("status", "all")
        ]
    };

    public void OnGet()
    {
    }
}
