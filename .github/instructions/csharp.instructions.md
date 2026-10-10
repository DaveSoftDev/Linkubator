## Nomenclatura específica (no expresable en .editorconfig)

### Qué debes tener en cuenta cuando propongas código

Todo el código propuesto para esta solución debe ser como si lo hiciera un desarrollador senior con experiencia en entornos seguros, escalables y de alta demanda.

## Organización de clases

- No añadir clases de producción directamente en la raíz del proyecto, salvo puntos de entrada y composición.
- Agruparlas en carpetas por responsabilidad y alinear el namespace con la ruta.
- Organizar los tests reflejando la capa y responsabilidad de la clase probada.
- No crear carpetas genéricas ni una carpeta por clase.

### Async
- Los métodos asíncronos de `Linkubator` NO terminan en `Async`. Todos son async por defecto.
- Sólo deben terminar en `Async` los propios métodos del framework de .NET.

**Evitar `async void`** — usar siempre `Task` o `Task<T>`

❌ Incorrecto:
```csharp
public async void ProcessUrl(string url) { }
```

✅ Correcto:
```csharp
public async Task ProcessUrl(string url) { }
public async Task<ShortenedUrl> GetUrl(int id) { }
```

### Sufijos por tipo de archivo/clase
- Razor Page models → sufijo `Model` (IndexModel, DisplayUrlsModel)
- OptionsPattern → sufijo `Options` (AppOptions)
- Extension methods → sufijo `Extensions` (WebRedirectorExtensions)
- DTOs de entrada → sufijo `Request` (ShortUrlRequest)
- DTOs de salida → sufijo `Response` (LongUrlResponse)
- Otros DTOs → sufijo `DTO`

### Métodos de repositorio
- Obtención: Get, GetAll, GetById, GetBy{Column}
- Creación: Insert, Add
- Actualización: Update
- Eliminación: Delete, Remove

### Namespaces

**NO usar usings generales a namespaces de la solución**
**Usar alias de using para los namespaces internos**

❌ Incorrecto:
```csharp
using Linkubator.Domain.Entities;
```

✅ Correcto — alias en el using:
```csharp
using Entities = Linkubator.Domain.Entities;
```

O bien, usar el nombre completo directamente en el código sin `using`.

### Inyección de Dependencias

- **Todas las dependencias deben inyectarse mediante el constructor**
- **Todos los servicios deben implementar interfaces**

✅ Ejemplo correcto:
```csharp
public class LinkubatorApplicationService : ILinkubatorApplicationService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<LinkubatorApplicationService> _logger;

    public LinkubatorApplicationService(IUnitOfWorkFactory unitOfWorkFactory, ILogger<LinkubatorApplicationService> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _logger = logger;
    }
}
```

### Recursos Descartables

**`IUnitOfWork` implementa `IAsyncDisposable` — usar `await using`**

✅ Correcto:
```csharp
await using (IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(Linkubator.Application.Constants.UnitOfWorkContexts.Linkubator))
{
    ILinkubatorRepository repository = unitOfWork.GetRepository<ILinkubatorRepository>();
    await repository.Insert(entity);
    await unitOfWork.Commit();
}
```

Para recursos síncronos (`IDisposable`), usar `using` convencional.

### Manejo de Errores

**En aplicaciones web (Razor Pages)**: el `GlobalExceptionHandler` centraliza el logging y la redirección a `/Errors/500` para cualquier excepción no controlada. **No añadir `try-catch` genérico** en los Page Model handlers — es código duplicado. Solo usar `try-catch` cuando haya que manejar un error de negocio específico (por ejemplo, mostrar un mensaje de validación).

✅ En Razor Pages — solo para manejo específico de negocio:
```csharp
public async Task<IActionResult> OnPostAsync()
{
    try
    {
        await _service.Link(Request);
        return RedirectToPage();
    }
    catch (LinkAlreadyExistsException)
    {
        ModelState.AddModelError(string.Empty, "El link ya existe.");
        return Page();
    }
    // Las excepciones no controladas las gestiona GlobalExceptionHandler
}
```
### Configuración y Options Pattern

#### Options Pattern para Configuración

**Usar Options Pattern para toda la configuración de la aplicación**

✅ Estructura:
```csharp

// En Program.cs o Bootstrap
builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection("AppOptions"));

// En el servicio
public class SomeService
{
    private readonly AppOptions _appOptions;

    public SomeService(IOptions<AppOptions> appOptions)
    {
        _appOptions = appOptions.Value;
    }
}
```

#### Ubicación de Options
- `Linkubator.Web/Options/` — opciones específicas de la Web.
- `Linkubator.Application/Options/` — opciones compartidas entre capas.
- Inyectar usando `IOptions<T>`, `IOptionsSnapshot<T>` o `IOptionsMonitor<T>`.

### Uso de Records

#### Records para Datos Inmutables

**Usar `record` para DTOs inmutables, errores y mensajes**

- Ubicación: `Linkubator.Application/Records/`
- Usar para errores y trazas de log
- Usar para respuestas de API inmutables

✅ Ejemplos:
```csharp
// En Linkubator.Application/Records/Errors.cs
public record ErrorRecord(string Code, string Message, DateTime Timestamp);

// En Linkubator.Application/Records/Traces.cs
public record TraceRecord(string EventId, string Description);
```

### Métodos de Extensión
  
#### Estructura y Nomenclatura

**Crear métodos de extensión para funcionalidad reutilizable**

##### Ubicación por Capa
```
Linkubator.Domain/Extensions/                              # Extensiones de dominio
Linkubator.Application/Extensions/                         # Extensiones de aplicación
Linkubator.Infrastructure.Repositories/Extensions/         # Extensiones de repositorios
Linkubator.Web/Extensions/                             # Extensiones de presentación web
```

##### Nomenclatura de Clases de Extensión
- Nombrar como: `[Tipo]Extensions`
- Ejemplos: `ServiceCollectionExtensions`, `WebApplicationBuilderExtensions`

##### Convenciones para `IServiceCollection`
- Métodos para registrar servicios: comenzar con `Add`
- Métodos para configurar opciones: comenzar con `Configure`

✅ Ejemplo:
```csharp
// En Linkubator.Infrastructure.Repositories/Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ILinkubatorRepository, LinkubatorRepository>();
        return services;
    }
}
```

##### Convenciones para `IApplicationBuilder` / `WebApplicationBuilder`
- Métodos deben comenzar con `Use`
  - `UseCustomMiddleware()`
  - `UseErrorHandling()`

### Logging y Trazabilidad

#### Inyección y Uso de Logger

**Siempre inyectar `ILogger<T>` donde T es la clase actual**

✅ Ejemplo:
```csharp
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
}
```

#### Niveles de Log

- **LogError**: Excepciones y errores críticos
  ```csharp
  _logger.LogError(ex, "Error loading Index page");
  ```

- **LogWarning**: Situaciones anómalas no críticas
  ```csharp
  _logger.LogWarning("URL {ShortUrl} está a punto de expirar", shortUrl);
  ```

- **LogInformation**: Eventos importantes del flujo
  ```csharp
  _logger.LogInformation("URL shortened successfully: {ShortUrl}", shortUrl);
  ```

- **LogDebug**: Información de depuración
  ```csharp
  _logger.LogDebug("Processing request for URL: {Url}", url);
  ```

### Entidades y Estructura de Domain

#### Entidades Base

**Todas las entidades deben heredar de `BaseEntity`**

- `BaseEntity` contiene propiedades comunes: `Id`, `CreatedAt`, etc.
- Ubicación: `Linkubator.Domain/Entities/`

✅ Ejemplo:
```csharp
public class Collection : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

### Organización del proyecto Domain

Todo vive en `Linkubator.Domain` (no existe un proyecto `Domain.Entities` separado):

```
Linkubator.Domain/
├── Entities/       # Entidades del dominio (heredan de BaseEntity)
│   └── Bases/      # BaseEntity
├── DTOs/
│   ├── Requests/   # DTOs de entrada (sufijo Request)
│   └── Responses/  # DTOs de salida (sufijo Response)
├── POCOs/          # ViewModels y otros objetos planos
├── Enums/          # HitType, ChartTimeScale, CreatedFrom
└── Exceptions/     # LinkAlreadyExistsException, etc.
```

### Razor Pages

#### Convenciones de PageModel

#### Nomenclatura

- El modelo de la página debe terminar con `Model`: `IndexModel`, `DashboardModel`
- Los ViewModels se ubican en el proyecto con sufijo `Model`

#### Propiedades Enlazadas

**Usar `[BindProperty]` para propiedades que se enlazan desde formularios**

✅ Ejemplo:
```csharp
public class IndexModel : PageModel
{
    [BindProperty]
    public Entities.Link Link { get; set; } = new();

    public CollectionsModel CollectionsModel { get; set; } = new();
}
```

#### Métodos Handler
- `OnGetAsync()`: Para peticiones GET
- `OnPostAsync()`: Para peticiones POST
- `OnPost[Action]Async()`: Para handlers específicos (ej: `OnPostDeleteAsync()`)

### Uso de Constants

### Evitar Strings y Números Mágicos

**Nunca usar strings o números mágicos en el código**

❌ Incorrecto:
```csharp
IUnitOfWork uow = _uowFactory.Get("Linkubator");
```

✅ Correcto:
```csharp
IUnitOfWork uow = _uowFactory.Get(Linkubator.Application.Constants.UnitOfWorkContexts.Linkubator);
```

#### Organización de Constants

- Ubicación: `Linkubator.Application/Constants/`
- Crear clases estáticas agrupadas por contexto

✅ Ejemplo:
```csharp
// En Linkubator.Application/Constants/UnitOfWorkContexts.cs
public static class UnitOfWorkContexts
{
    public const string Linkubator = "Linkubator";
}
```

### Gestión de Unit of Work

#### Uso de Factory Pattern

**`IUnitOfWork` es `IAsyncDisposable` — usar `await using`**

✅ Patrón correcto:
```csharp
await using (IUnitOfWork uow = _unitOfWorkFactory.Get(Linkubator.Application.Constants.UnitOfWorkContexts.Linkubator))
{
    ILinkRepository repository = uow.GetRepository<ILinkRepository>();
    Entities.Link link = await repository.GetById(id);

    entity.Description = "Updated";
    await repository.Update(entity);

    await uow.Commit();
}
```

#### Múltiples Contextos

**Usar bloques `await using` separados para diferentes contextos**

✅ Ejemplo:
```csharp
await using (IUnitOfWork uowCommons = _unitOfWorkFactory.Get(Linkubator.Application.Constants.UnitOfWorkContexts.Linkubator))
{
    IEnumerable<Entities.Link> links = await uowCommons.GetRepository<ILinkRepository>().GetAll();
}

await using (IUnitOfWork uowShortener = _unitOfWorkFactory.Get(Linkubator.Application.Constants.UnitOfWorkContexts.OtherContext))
{
    IEnumerable<Entities.OtherEntity> otherEntities = await uowShortener.GetRepository<IOtherEntityRepository>().GetAll();
}
```


### Patrones de Diseño

#### Repository Pattern
- Las interfaces base genéricas viven en `Redarbor.Infrastructure.Repositories.Base/Contracts/`
- Repositorios específicos por entidad en `Redarbor.UrlShortener.Infrastructure.Repositories`
- Repository Pattern: `I*Repository` → `*Repository`
- Los repositorios reciben POCOs/DTOs de entrada y retornan entidades de dominio
- Ubicación de contratos: `Redarbor.UrlShortener.Application/Contracts/Infrastructure/Repositories/`

#### Unit of Work Pattern
- `IUnitOfWork` implementa `IAsyncDisposable`
- **Usar `await using`** (no `using` simple) para garantizar el dispose asíncrono
- Factory para crear instancias: `IUnitOfWorkFactory`
- Implementaciones: `ShortenerUnitOfWork`, `CommonsUnitOfWork`

#### Factory Pattern
- `IConnectionFactory` para crear conexiones a base de datos
- `IUnitOfWorkFactory` para obtener instancias de UnitOfWork según contexto
- Factory Pattern: `I*Factory` → `*Factory`
- No crear conexiones directas en repositorios o servicios

### Resumen de Mejores Prácticas

#### ✅ Hacer Siempre
1. Usar tipos explícitos (no `var`)
2. Usar `await using` para `IUnitOfWork` (`IAsyncDisposable`); `using` para `IDisposable`
3. Inyectar dependencias por constructor
4. Todos los servicios implementan interfaces
5. Usar `try-catch` en endpoints de API; en Razor Pages solo para errores de negocio específicos
6. Usar alias de using para namespaces internos
7. Heredar entidades de `BaseEntity`
8. Usar Factory Pattern para UnitOfWork y conexiones
9. Usar Options Pattern para configuración
10. Inyectar `ILogger<T>` donde T es la clase actual
11. Usar constants en lugar de strings mágicos o números mágicos
12. Sufijos `Request`/`Response` para DTOs
13. Sufijo `Model` para modelos de Razor Pages

#### ❌ Evitar Siempre
1. `async void`
2. `var` para declarar variables
3. Crear conexiones directas sin Factory
4. Strings o números mágicos en el código
5. `using` generales a namespaces de la solución (usar alias)
6. Múltiples responsabilidades en una clase
7. Dependencias sin interfaces
8. `try-catch` genérico en Page Model handlers
