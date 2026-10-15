namespace Linkubator.Domain.Enums;

/// <summary>
/// Estado del ciclo de scraping de un enlace. El valor indica en qué punto está la
/// petición, no si el scraping está disponible
/// (ver specifications.md → «Estados del scraping»).
/// </summary>
public enum ScrapingStatus
{
    /// <summary>No se ha solicitado scraping para el enlace.</summary>
    NotRequested,

    /// <summary>El usuario lo ha lanzado y el intento aún no ha empezado.</summary>
    Pending,

    /// <summary>Hay un intento en curso.</summary>
    Processing,

    /// <summary>El último intento terminó y devolvió metadatos.</summary>
    Completed,

    /// <summary>El último intento terminó sin conseguir metadatos.</summary>
    Failed
}
