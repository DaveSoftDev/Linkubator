# Plan técnico: S11.2 — Página pública de usuario

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 27-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Crear una Razor Page agrupada en `Pages/Public/` con la ruta explícita `/{alias}/` y su `PageModel` en el mismo módulo. La página recibe el alias de la ruta y consulta el servicio de lectura pública; no obtiene datos mediante un `UserId` del cliente.
- Registrar `IPublicReadService` con `PublicReadService` en el punto de composición Web. `IPublicReadRepository` y `SqlitePublicReadRepository` ya existen y forman parte de la persistencia.
- Reutilizar `PublicReadService.GetUserPage` y su `PublicUserDto` para cargar el perfil, las colecciones que se muestran en público y los enlaces recientes. Ajustar la consulta de perfil de `SqlitePublicReadRepository.GetUserPage`: hoy devuelve todas las colecciones públicas ordenadas por nombre, mientras que la página requiere solo las recientes ordenadas según [specifications.md → «Listados»](../../../context/specifications.md#listados). La consulta no debe cargar más colecciones de las que se renderizan.
- Preparar en `PageModel` los metadatos de la página a partir del alias y los datos resueltos, y renderizar el contenido en Razor. La canonical debe usar el origen público configurado y la ruta canónica, nunca un origen tomado de `Request.Host`; si falta la configuración requerida, no generar una canonical con un origen no confiable.
- La vista representa enlaces, colecciones, imágenes y destinos siguiendo las reglas específicas de [specifications.md → «Página de usuario»](../../../context/specifications.md#página-de-usuario). La revisión transversal asignada a S11.4 no pospone la conformidad de esta ruta.
- Reutilizar el layout, las clases y los recursos Web existentes cuando sean aplicables. No reutilizar directamente `_LinkCard`: incorpora acciones privadas de edición, movimiento y borrado. La presentación pública adapta el contenido a la fuente funcional sin añadir acciones privadas ni una abstracción visual general.
- La revisión visual solicitada por DLG el 27-X-2026 utiliza [\_PublicLayout.cshtml](../../../src/Linkubator.Web/Pages/Shared/_PublicLayout.cshtml), anidado en el layout existente, para compartir la cabecera, la navegación del perfil y el pie entre las páginas públicas. [public.css](../../../src/Linkubator.Web/wwwroot/css/public.css) adapta las variables del tema privado y los estilos existentes de tarjetas, acciones, paginación y estados vacíos sin modificar el área privada ni la landing. Las vistas conservan el contenido público real, los encabezados y los metadatos; las imágenes ausentes se representan mediante un icono decorativo y las descripciones se muestran completas.

### Flujos internos

Las vistas públicas proporcionan su banner mediante la sección Razor `PublicHeading`. `_PublicLayout.cshtml` la renderiza antes de las pestañas y después incorpora el cuerpo de cada página. `public-profile-heading` comparte la altura mínima y permite el crecimiento por contenido, sin modificar los banners de privacidad. La presentación sigue [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas).

El hueco de imagen de la tarjeta pública limita su fila de Grid con `minmax(0, 1fr)` y permite que la imagen reduzca su tamaño mínimo. La caja de la imagen ocupa el contenedor y utiliza `object-fit: cover` con `object-position: center`, igual que la miniatura privada. El tamaño intrínseco de una imagen cuadrada o vertical no amplía la fila ni desplaza el recorte; los símbolos decorativos sin imagen conservan sus dimensiones.

La presentación del origen de los enlaces reutiliza [UrlExtensions.cs](../../../src/Linkubator.Web/Extensions/UrlExtensions.cs), extraído del cálculo existente en `LinkCardViewModel.Domain` y compartido con el área privada. La composición visual sigue [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas).

La presentación del alias reutiliza [AliasExtensions.cs](../../../src/Linkubator.Web/Extensions/AliasExtensions.cs) en las vistas, sus metadatos y el enlace de perfil privado. No se transforma el DTO ni el valor usado para enrutar o consultar. El límite de enlaces recientes se aplica en la consulta del repositorio conforme a [specifications.md → «Página de usuario»](../../../context/specifications.md#página-de-usuario); las pruebas de repositorio cubren el corte y los desempates, y las pruebas HTTP comprueban la cantidad de tarjetas renderizadas.

1. La petición `GET /{alias}/` se resuelve en la Razor Page pública.
2. El `PageModel` invoca `IPublicReadService.GetUserPage` con el alias de ruta y el `CancellationToken` de la petición.
3. En caso de éxito, la vista renderiza el modelo público y establece los metadatos de la página. Los estados vacíos y el enlace al listado completo se presentan conforme a [spec.md](spec.md) y sus fuentes.
4. El resultado de usuario no accesible se traduce a `404`. Los fallos de infraestructura u otros errores no se presentan como una página inexistente; siguen el tratamiento de errores de la aplicación.
5. Razor produce el HTML de la respuesta en servidor.

### Persistencia y dependencias

- No se requieren cambios de esquema, migraciones, transacciones ni nuevos paquetes.
- El repositorio público ya filtra colecciones que no se muestran en público y enlaces que no pertenecen a colecciones públicas; conservar estas condiciones al ajustar la consulta del perfil.
- El origen público debe estar disponible en la configuración existente. No se incluye configuración de despliegue; las pruebas fijarán un origen conocido.
- No se añade una lectura de colecciones completas a la página: el enlace a su listado usa la ruta pública definida por la fuente propietaria.

### Garantías técnicas

- La respuesta del perfil contiene únicamente los datos públicos previstos; no representa nombre ni email del usuario ni datos de enlaces o colecciones privados, conforme a [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado).
- La lectura usa el servicio de Application y la consulta de Infrastructure existentes, sin introducir acceso a datos desde la vista.
- Los errores de usuario no accesible se distinguen de los fallos técnicos, y la canonical no depende de la cabecera `Host`, conforme a [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento).
- La vista reutiliza la codificación contextual de Razor para los valores representados en HTML y no interpreta títulos, descripciones ni URL como marcado.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el caso de uso y el contrato de lectura pública existentes; ampliar la consulta de perfil para devolver las colecciones recientes en vez de todas las colecciones ordenadas alfabéticamente. | Propuesta | [IPublicReadService.cs](../../../src/Linkubator.Application/Contracts/IPublicReadService.cs), [PublicReadService.cs](../../../src/Linkubator.Application/UseCases/PublicReads/PublicReadService.cs), [SqlitePublicReadRepository.cs](../../../src/Linkubator.Infrastructure/Repositories/SqlitePublicReadRepository.cs), [specifications.md → «Listados»](../../../context/specifications.md#listados) | Evita duplicar el caso de uso. La selección y el orden deben quedar cubiertos por pruebas del repositorio. |
| Añadir la página pública como módulo `Pages/Public/` con ruta explícita. | Propuesta | [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento), [AGENTS.md → «Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código) | Mantiene juntas la vista y su `PageModel`, aunque el segmento público de la URL no refleja la carpeta física Razor. |
| Exigir que `PublicOrigin` esté configurado para construir la canonical, sin recurrir a `Request.Host`. | Confirmado | [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento) | `PublicOrigin` está disponible en la configuración de desarrollo; los entornos sin ese valor deberán configurarlo antes de publicar la aplicación. La configuración de despliegue queda fuera de este trabajo. |
| El componente `_LinkCard` no es adecuado sin adaptación porque contiene acciones de gestión privada. | Confirmado | [specifications.md → «Página de usuario»](../../../context/specifications.md#página-de-usuario), [AGENTS.md → «Referencia de diseño Web»](../../../AGENTS.md#referencia-de-diseño-web) | Reutilizar patrones y estilos aplicables, sin exponer enlaces de gestión en la página pública. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada, integración HTTP de la Razor Page y comprobación del HTML recibido | Fábrica Web con SQLite temporal, datos públicos y `PublicOrigin` controlado | Añadir casos focalizados a `WebStartupTests` y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage"` | La ruta del alias existente devuelve HTML renderizado en servidor con el contenido, los metadatos, la canonical y las secciones exigidas por AC-USER-01. Un alias inexistente o antiguo devuelve `404`. La canonical conserva el origen configurado al variar el `Host` permitido de la petición. | Resultado del test focalizado y salida HTML inspeccionada por las aserciones. |
| V02 | CA01 | Automatizada, integración HTTP con datos públicos y privados mezclados | Fábrica Web y datos de prueba que incluyan elementos privados y una colección pública sin enlaces públicos | Añadir un caso negativo a `WebStartupTests` y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage"` | El HTML incluye únicamente los elementos públicos pertinentes; no contiene títulos, descripciones ni URL de los elementos privados ni identifica colecciones que no deban mostrarse en público. | Resultado del caso HTTP negativo; registrar qué datos privados se prepararon y comprobar que ninguno aparece en la respuesta. |
| V03 | CA01 | Automatizada, integración de persistencia del límite y orden de las colecciones del perfil | Base SQLite de prueba con más de cinco colecciones públicas, fechas distintas y empate de fecha; colecciones no publicables de control | Ampliar `SqlitePublicReadRepositoryTests` y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage"` | El resultado contiene el conjunto y orden de colecciones de perfil requeridos por la fuente propietaria; los casos no publicables quedan excluidos. | Resultado del test del repositorio con conjunto esperado y desempate comprobados. |
| V04 | CA01 | Automatizada, integración de persistencia de los enlaces recientes | Base SQLite de prueba con enlaces públicos y privados, fechas diferentes y empate | Ampliar `SqlitePublicReadRepositoryTests` y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage"` | Se devuelven únicamente los enlaces públicos aplicables y el orden determinista definido por la fuente propietaria; no se excede el límite previsto. | Resultado del test del repositorio con casos de privacidad, límite y desempate. |

Las pruebas HTTP verifican la salida observable, no el rastreo real por buscadores. Las pruebas del repositorio no sustituyen la comprobación HTTP de la respuesta `404`, los metadatos ni el HTML.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan.
- Las tareas propias cubrirán la ruta y su `PageModel`, el registro del servicio, la consulta de colecciones recientes, la vista y las comprobaciones automatizadas. La validación no dependerá de páginas públicas de otras etapas ni de despliegue.
- La revisión transversal de la etapa S11 definida en [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) no sustituye la evidencia necesaria para cerrar CA01 y CA02 de esta spec.
- El cierre requerirá evidencia satisfactoria de V01–V04, tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Si la lectura existente no puede distinguir de forma explícita un usuario inexistente de un fallo técnico, o si falta el origen público configurado, detener la tarea afectada y resolver el bloqueo sin devolver una respuesta con forma de éxito.
