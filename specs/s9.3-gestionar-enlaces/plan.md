# Plan técnico: S9.3 — Gestionar enlaces

## Estado y aprobación del plan

- Estado: aprobado por DLG el 26-X-2026 («Aprobada»), en esta conversación.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Aprobar la spec y generar plan.md como propuesta»).
  - Plan anterior: DLG, 26-X-2026 («Aprobar el plan y generar tasks.md como borrador»); aprobación correspondiente al diseño anterior, sustituido por esta revisión.
  - Aclaración de alcance confirmada por DLG el 26-X-2026: edición permite cambiar la colección, aplica el resultado privado de la regla de movimiento y conserva la página independiente para mover.
  - Revisión solicitada por DLG el 26-X-2026: el estado público o privado se establece en el formulario integral de edición; no existe una operación independiente para cambiarlo.
- La implementación anterior incluía una ruta y un caso de uso independientes para el estado. DLG autorizó su retirada directa el 26-X-2026; esta revisión aprobada define el alcance vigente, cuya implementación y aceptación se registran en [tasks.md](tasks.md#aceptación-y-cierre).

## Diseño de implementación

### Componentes y contratos

- Añadir las páginas Razor privadas de edición, movimiento y confirmación de eliminación bajo `Pages/App/Links`, con modelos de entrada separados según sus campos. Mantener la URL fuera del formulario de edición.
- Añadir en `AppRoutes` únicamente las rutas de edición, movimiento y eliminación definidas por [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas); el estado no tiene una ruta ni un formulario independientes.
- Delegar las lecturas y escrituras en `ILinkService.GetLink`, `UpdateLink`, `MoveLink` y `DeleteLink`, con sus DTOs de petición. `UpdateLink` recibe también el estado indicado por el checkbox, valida la colección destino y, dentro de su transacción, combina la edición con `Link.MoveToCollection` cuando cambia el destino; la página `Move` sigue delegando en `MoveLink`. La validación de metadatos reutiliza las políticas de creación y comprueba `Image` contra la URL original cargada por Application.
- El checkbox forma parte de `LinkUpdateRequest`: marcado representa público y desmarcado representa privado. La edición rechaza una entrada que no pueda interpretarse como booleano, sin modificar ningún campo. No se añade un caso de uso ni un request específico para cambiar solo el estado.
- Reutilizar `PrivatePageModel.CollectionsResult` para las opciones completas de colecciones propias en edición y movimiento, como hace la página de creación; no duplicar consultas del sidebar. Application valida la colección seleccionada en la operación de edición o en `MoveLink`, según el formulario.
- Reutilizar los modelos de errores, alertas y recursos no accesibles de Web para traducir los resultados de Application. La vista de borrado obtiene el recurso en GET y solo el POST de confirmación invoca `DeleteLink`.
- Las tarjetas del dashboard trasladan el identificador real del enlace a `LinkCardViewModel.Id`; `_LinkCard` genera con Tag Helpers los enlaces a las páginas de edición, movimiento y confirmación de eliminación, en ese orden. Las acciones mantienen nombres accesibles, tooltips nativos mediante `title` y estilos de foco, sin ejecutar escrituras desde la tarjeta.
- `_LinkCard` incorpora el origen a los enlaces de gestión. `LinkManagementPageModel` comparte el binding del parámetro de retorno y su validación con las páginas de edición, movimiento y eliminación: comprueba `IsLocalUrl` y compara la ruta sin query ni fragmento con los destinos permitidos por [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), considerando `PathBase`. Las vistas envían solo el destino validado mediante un campo oculto, lo reutilizan al cancelar y los handlers usan `LocalRedirect`. El dashboard consume los avisos de gestión de `TempData` al recibir un retorno con error.
- Usar las páginas existentes de edición y borrado de colecciones como referencia de composición Web y de tratamiento de errores: [EditModel](../../src/Linkubator.Web/Pages/App/Collections/Edit.cshtml.cs) y [DeleteModel](../../src/Linkubator.Web/Pages/App/Collections/Delete.cshtml.cs). La identidad procede del contexto de servidor y no se añade `UserId` a los formularios.

### Flujos internos

1. En GET de edición, movimiento o eliminación, analizar el identificador de ruta y llamar a `GetLink`. Un identificador inválido no consulta el recurso; uno válido pero inexistente o ajeno recibe la respuesta común de recurso no accesible.
2. GET de edición precarga los campos editables y todas las colecciones propias, con la colección actual seleccionada; GET de movimiento carga las colecciones propias disponibles y selecciona la actual; GET de eliminación presenta la confirmación sin ejecutar escrituras.
3. En POST de edición, Web construye `LinkUpdateRequest` con la colección seleccionada y el estado del checkbox, y delega en Application. El checkbox desmarcado se interpreta como privado; un valor no interpretable como booleano invalida la edición. Application valida el destino, los metadatos y la entrada de estado, y persiste todos los campos en una única transacción. Si cambia la colección, `Link.MoveToCollection` deja el enlace privado y la petición no puede volver a hacerlo público. Los errores de validación regresan al formulario sin mutaciones; el rechazo de publicación en la misma colección privada usa el aviso definido por la fuente.
4. En POST de movimiento, Web construye `LinkMoveRequest`; Application verifica el destino con la identidad actual y aplica la transición de dominio. Web no determina por sí misma si la colección destino pertenece al usuario.
5. En POST de eliminación, Web llama a `DeleteLink` solo tras la confirmación. Los errores no accesibles se traducen sin revelar existencia o propiedad; los rechazos de dominio mantienen el tratamiento Web previsto.
6. Tras una operación correcta o un rechazo que así lo requiera, Web dirige al origen validado o al destino por defecto conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas). Como el listado de enlaces pertenece a S10 y aún no está disponible, los tests que vuelven a ese listado verifican la respuesta y `Location` sin seguir la redirección.

### Persistencia y dependencias

- No se prevén cambios de esquema, migraciones ni SQL desde Web. La edición (incluido el estado del checkbox), movimiento y eliminación usan los casos de uso de Application.
- `LinkService` realiza las operaciones de escritura del enlace en transacciones y `SqliteLinkRepository.Update` y `Delete` actualizan la proyección FTS5 afectada. Mantener esta responsabilidad en Infrastructure, sin añadir escrituras desde Razor Pages.
- Prerrequisitos disponibles: contratos y operaciones de enlaces de Application, persistencia SQLite, identidad privada y páginas de creación de S9.1/S9.2. Las especificaciones y tareas de S9.1 y S9.2 constan como completadas y aceptadas.
- S9.4 añadirá sus pruebas HTTP transversales; S9.5 ejecutará su guía manual y S9.6 revisará presentación. Sus trabajos no son dependencias para demostrar ni cerrar los criterios propios de S9.3.

### Garantías técnicas

- Mantener el flujo Web → Application → Domain → Infrastructure; Web no accede directamente a repositorios ni implementa reglas de propiedad, duplicados o privacidad.
- No aceptar `UserId` desde ruta, query ni formulario. Las lecturas y escrituras se limitan al usuario identificado y a sus relaciones, conforme a [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- Mantener las modificaciones en POST protegidos por antiforgery; GET solo carga formularios y vistas de confirmación, sin mutar datos ([architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web)).
- El movimiento desde ambas superficies delega en Application/Domain la validación del destino y el cambio de privacidad; la edición integral delega en la misma capa la condición de publicación en la colección aplicable.
- La actualización o eliminación del enlace y la sincronización de las proyecciones afectadas se mantienen atómicas según [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [«FTS5»](../../context/architecture.md#fts5).
- Los tests HTTP focalizados de S9.3 cubren directamente sus operaciones. No se atribuirá a ellos la guía de URLs reales ni la revisión visual asignadas a etapas humanas posteriores.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Incluir el checkbox de estado en la operación existente de edición integral, sin caso de uso, request ni ruta independientes. | confirmado por la aclaración de alcance de DLG del 26-X-2026 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace) y [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación). | La ruta y el caso de uso del diseño anterior se retiraron por instrucción directa de DLG el 26-X-2026; la revisión del plan fue aprobada por DLG el 26-X-2026 («Aprobada»). |
| Validar la colección, el checkbox y los metadatos de forma conjunta y persistir la edición integral en una sola operación. | confirmado | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace) y [AC-LINK-03](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace). | Si la colección cambia, se aplica la regla de privacidad del movimiento; los valores del checkbox que no puedan interpretarse como booleanos no producen mutaciones. |
| El destino de redirección tras modificar un enlace, `/app/enlaces`, todavía no está implementado. | seguimiento no bloqueante | [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l). | Comprobar `Location` y el estado persistido sin seguir la redirección. La llegada visual al listado se comprobará cuando exista S10. |
| S9.4 también incluye pruebas HTTP de propiedad, petición manipulada y atomicidad. | seguimiento no bloqueante | [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l). | Las pruebas de S9.3 se limitan a demostrar sus criterios y rutas; las pruebas transversales de S9.4 no son prerrequisito de cierre. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; GET de edición, movimiento y confirmación de eliminación, incluidos identificador inválido y recurso ajeno/inexistente. | Fixture HTTP con SQLite, identidad autenticada y enlaces de dos usuarios. | Añadir casos a `LinkManagementIntegrationTests`; `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkManagementIntegrationTests --nologo` | El formulario de edición precarga la colección actual y opciones solo propias; GET no modifica enlace ni proyección; URL no editable; los recursos ajenos e inexistentes reciben la misma respuesta privada. | Casos ejecutados, HTML/estado HTTP relevante y comparación de datos antes/después. |
| V02 | CA02, CA03 | Automatizada y negativa; POST válido, cambio de colección, colección inválida/ajena, imagen inválida y solicitud directa que intenta publicar en una colección privada. | Fixture HTTP y enlaces propios en colecciones públicas y privadas, más una colección ajena. | Mismo filtro `LinkManagementIntegrationTests` de V01. | El cambio de colección y los otros campos válidos se confirman juntos, el enlace queda privado, y la URL se conserva; los destinos inválidos/ajenos y los metadatos inválidos no alteran enlace ni proyección. La petición manipulada no puede publicar al cambiar de colección. | Resultado de cada caso, campos y proyección antes/después, respuesta y aviso recibido. |
| V03 | CA04 | Automatizada; movimiento válido, destino inválido, inexistente y ajeno. | Fixture HTTP con dos colecciones propias y una ajena. | Mismo filtro `LinkManagementIntegrationTests` de V01. | El movimiento válido conserva identidad y datos y deja el enlace privado; cada destino no autorizado o inválido deja enlace y proyección sin cambios. | Respuesta, `Location`, datos del enlace y estado de búsqueda antes/después. |
| V04 | CA02 | Automatizada y negativa; POST de edición con checkbox marcado, desmarcado y un valor no interpretable como booleano. | Fixture HTTP de V02. | Mismo filtro `LinkManagementIntegrationTests` de V01 y `LinkServiceTests`. | El checkbox marcado establece público cuando las reglas de privacidad lo permiten; desmarcado establece privado. Un valor no interpretable rechaza la edición completa sin mutar ningún campo ni la proyección. | Valores de petición, respuesta, aviso y comparación de los campos y la proyección antes/después. |
| V05 | CA06, CA07 | Automatizada; GET de confirmación, POST confirmado y solicitudes inválidas, inexistentes o ajenas. | Fixture HTTP con enlace propio y enlace de otro usuario. | Mismo filtro `LinkManagementIntegrationTests` de V01. | GET no borra; el POST confirmado elimina enlace y relaciones; los casos inválidos o no accesibles no borran ni revelan la causa. | Respuestas, datos y recuentos antes/después, incluyendo relación y proyección. |
| V06 | CA08 | Automatizada; comprobación de integración de edición, cambio de colección, movimiento y borrado con la proyección de búsqueda SQLite dentro de la operación. | Base SQLite de integración y schema FTS5 del proyecto. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteLinkRepositoryTests|FullyQualifiedName~SqlitePersistenceIntegrationTests|FullyQualifiedName~LinkServiceTests" --nologo` | La proyección refleja los campos y la colección después de actualizar/mover; el borrado no deja una fila de búsqueda. Se conserva el tratamiento atómico de fallos de escritura. | Resultado de los tests relevantes y contenido de las filas de proyección comprobadas. |

Los tests focalizados de S9.3 verifican las operaciones y respuestas de este trabajo; no sustituyen la cobertura HTTP transversal asignada a S9.4 ni la revisión manual de S9.5/S9.6. Las comprobaciones de solicitud manipulada en V02 y V04 deben enviar el POST de edición directamente, sin depender del comportamiento de la interfaz.

## Orden de ejecución y cierre

- La aprobación de esta revisión y la aceptación del resultado están registradas en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre), junto con la trazabilidad y evidencia del alcance vigente.
- La implementación y validación del alcance vigente están completadas; su evidencia no depende de S9.4, S9.5 ni S9.6.
- Cerrar S9.3 con evidencia satisfactoria para todos los criterios de esta especificación, tareas propias completadas y aceptación humana registrada en `tasks.md`; no esperar a que exista el listado S10 ni a S9.4, S9.5 o S9.6.
- Si la implementación revela que los contratos existentes no pueden cumplir alguno de los criterios sin cambiar una regla funcional, bloquear la tarea afectada y tramitar primero la decisión en su documento propietario.
