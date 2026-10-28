# Plan técnico: S11.3 — Páginas públicas de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 27-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Añadir las Razor Pages `Pages/Public/Collections.cshtml` y `Pages/Public/Collection.cshtml`, junto con sus respectivos `PageModel`, para las rutas públicas de colecciones y de colección definidas en [spec.md](spec.md).
- Reutilizar el servicio y los contratos de lectura pública ya registrados: `IPublicReadService.GetCollectionsPage`, `GetCollectionPage` y `GetCollectionPageCount`. Estos métodos ya devuelven colecciones visibles en público, una página de enlaces públicos y el total de páginas, respectivamente.
- Reutilizar `PublicCollectionSummaryDto`, `PublicCollectionDto` y `PublicLinkDto` para presentar los resultados en Razor. Las vistas no acceden directamente a persistencia.
- Seguir el patrón de [User.cshtml.cs](../../src/Linkubator.Web/Pages/Public/User.cshtml.cs) para traducir `ResourceNotAccessible` a `404`, propagar fallos técnicos y construir la canonical con `PublicOriginOptions`, nunca con `Request.Host`.
- Asignar en cada `PageModel` el título, descripción y canonical correspondientes; el layout compartido ya renderiza `ViewData["MetaTitle"]`, `ViewData["MetaDescription"]` y `ViewData["CanonicalUrl"]`.
- En la página de colección, interpretar el primer valor de `p` según la fuente propietaria. Normalizar a la primera página los valores ausentes, vacíos, inválidos o fuera de rango; invocar las lecturas existentes para obtener el número de páginas y los enlaces de la página elegida. Generar la canonical sin `p` en la primera página y con el número decimal normalizado en páginas posteriores, sin propagar parámetros desconocidos.
- Mantener en las vistas la codificación contextual de Razor para todo contenido procedente de los DTOs. Aplicar patrones de presentación existentes en la página pública de usuario; no reutilizar componentes que incorporen acciones privadas.
- La revisión visual solicitada por DLG el 27-X-2026 comparte el layout y los estilos específicos descritos en [S11.2 → «Componentes y contratos»](../s11.2-pagina-publica-usuario/plan.md#componentes-y-contratos). Las colecciones se presentan como tarjetas enlazadas; sus enlaces conservan imágenes opcionales, títulos con alternativa de URL y descripciones completas. La paginación reutiliza las clases existentes y mantiene sus destinos y el tratamiento del parámetro `p`.

### Flujos internos

1. Una petición a `/{alias}/colecciones/` invoca `GetCollectionsPage`. Si el alias no es accesible, la página responde `404`; si la lectura tiene éxito, Razor representa las colecciones devueltas, los metadatos y la canonical.
2. Una petición a `/{alias}/colecciones/{slug}` interpreta `p` y consulta `GetCollectionPageCount` para determinar el rango válido. Si el recurso no es accesible, responde `404`; si lo es, consulta `GetCollectionPage` con la página seleccionada.
3. La canonical de la página de colección se construye a partir del origen configurado y de la ruta resuelta, no de la query recibida directamente. Para la primera página se omite `p`; para páginas posteriores se usa su representación decimal normalizada.
4. Las dos páginas se renderizan en servidor. Los fallos técnicos siguen el tratamiento de errores de la aplicación y no se convierten en estados vacíos ni en respuestas `404`.

### Persistencia y dependencias

- `IPublicReadService`, `PublicReadService` y `SqlitePublicReadRepository` ya implementan las lecturas necesarias. El repositorio filtra colecciones y enlaces no públicos, ordena los resultados y pagina los enlaces; su cuenta de páginas se basa en los enlaces públicos.
- No se prevén cambios de esquema, migraciones, transacciones ni paquetes. Si las validaciones revelan una divergencia de la consulta con los criterios, se corregirá la lectura existente y sus pruebas dentro del alcance de esta spec.
- Las pruebas de integración Web usarán la fábrica existente, una base SQLite temporal con contenido público y privado, y un `PublicOrigin` controlado. No se requiere despliegue ni configuración de producción.

### Garantías técnicas

- Alias y slug se usan como localizadores de ruta; las lecturas existentes aplican los filtros de propiedad y visibilidad pública de las fuentes.
- Una colección privada, una colección sin enlaces públicos, un enlace privado y recursos inexistentes no se exponen como contenido ni como estados distinguibles fuera de las respuestas definidas por la spec.
- Las canonical usan el origen público configurado incluso cuando el `Host` de la solicitud difiere, de acuerdo con [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento).
- La validación de HTML y metadatos se hace sobre las respuestas HTTP renderizadas; las pruebas del repositorio por sí solas no demuestran el comportamiento de las rutas ni sus canonical.
- Los atributos transversales de enlaces externos siguen asignados a S11.4, como delimita [spec.md](spec.md); no se pospone por ello ningún criterio propio de S11.3.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Implementar ambas rutas como Razor Pages dentro de `Pages/Public/`, siguiendo la estructura de la página pública existente. | Confirmada e implementada | [S11.2 plan técnico](../s11.2-pagina-publica-usuario/plan.md), [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento) | Las rutas públicas se implementan como Razor Pages y la validación HTTP confirma el acceso a ambas sin interferir con la página de usuario. |
| Reutilizar el servicio y los DTOs de lectura pública existentes, sin añadir consultas paralelas en Web. | Confirmada e implementada | [IPublicReadService.cs](../../src/Linkubator.Application/Contracts/IPublicReadService.cs), [PublicReadService.cs](../../src/Linkubator.Application/UseCases/PublicReads/PublicReadService.cs) | Las páginas consumen las lecturas públicas existentes; las pruebas HTTP y de repositorio verifican el filtrado, orden y contenido servido. |
| Normalizar `p` en `PageModel` y usar los métodos existentes para contar páginas y obtener la página solicitada. | Confirmada e implementada | [spec.md](spec.md), [SqlitePublicReadRepository.cs](../../src/Linkubator.Infrastructure/Repositories/SqlitePublicReadRepository.cs) | La normalización HTTP y la lectura paginada quedan separadas; las pruebas cubren primer valor repetido, ceros iniciales, valor inválido, fuera de rango y canonical. |
| Construir canonical con el origen público configurado, nunca con el host de la petición. | Confirmado | [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento) | Las pruebas fijarán el origen en la fábrica Web y comprobarán que variar `Host` no altera la canonical. |
| Los enlaces externos reciben los atributos transversales en S11.4. | Confirmado | [spec.md](spec.md), [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | S11.3 puede cerrarse con su evidencia propia sin depender de la etapa transversal posterior. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración HTTP de la página de colecciones | Fábrica Web, SQLite temporal y usuario con colecciones públicas, privadas y sin enlaces públicos | Añadir casos a `WebStartupTests` y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.PublicCollectionsPage"` | La ruta devuelve HTML `200` con las colecciones públicas, descripciones, orden, metadatos y canonical esperados; no identifica colecciones privadas ni vacías. La ruta de un alias inexistente devuelve `404`. | Resultado de los tests focalizados y aserciones sobre el HTML recibido. |
| V02 | CA02, CA03 | Automatizada, integración HTTP de la página de colección y paginación | Fábrica Web con origen controlado y contenido de una colección con enlaces públicos y privados suficientes para recorrer páginas | Añadir casos a `WebStartupTests` y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.PublicCollectionPage"` | La ruta presenta solo enlaces públicos con sus metadatos y orden; colecciones no accesibles responden `404`. Los casos de `p` válido, repetido, inválido y fuera de rango muestran el contenido de página esperado y la canonical correspondiente, independiente de `Host` y parámetros desconocidos. | Resultado de cada caso HTTP y valores de canonical comprobados. |
| V03 | CA01, CA02, CA03 | Automatizada, integración de persistencia para lecturas públicas | SQLite de prueba con colecciones y enlaces públicos y privados, orden alfabético y límites entre páginas | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqlitePublicReadRepositoryTests"` y el filtro `FullyQualifiedName~GetPublicCollections_WhenNamesAreEquivalent_UsesIdentifierTieBreak|FullyQualifiedName~GetPublicCollection_WhenTitleIsMissing_OrdersByOriginalUrl` | Las lecturas conservan exclusión de datos privados, orden y partición de páginas previstos; los límites de página concuerdan con los resultados HTTP. | Resultado de la suite focalizada de repositorio y de los tests focalizados de orden. |
| V04 | CA04 | Automatizada, integración HTTP de ambas rutas | Fábrica Web disponible | Las respuestas verificadas en V01 y V02 se inspeccionan como HTML completo recibido por HTTP | El contenido de ambas páginas está renderizado en el servidor y no depende de JavaScript para formar el resultado probado. | Respuestas HTML y resultado de las aserciones de integración. |

Las pruebas HTTP verifican el contenido servido por la aplicación, no su indexación real por motores de búsqueda. Las pruebas de persistencia no sustituyen la comprobación HTTP de los metadatos, las canonical ni los `404`.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan.
- Las tareas propias cubrirán ambas rutas y sus `PageModel`, el tratamiento de `p`, el contenido renderizado y las comprobaciones HTTP y de persistencia. Ningún criterio dependerá de que S11.4, S11.5 o S11.6 aporte código, tests o evidencia posterior.
- El cierre requerirá evidencia satisfactoria de CA01–CA04, tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Si una implementación revela una contradicción en una regla funcional, detener la tarea afectada y solicitar su resolución en el documento propietario antes de continuar.
