# Plan técnico: S9.2 — Publicación de enlaces en colecciones privadas

## Estado y aprobación del plan

- Estado: aprobado; la comprobación de presentación del aviso queda como seguimiento de S10.1 y no bloquea S9.2.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: DLG, 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Extender el flujo existente de creación en `Pages/App/Links/New.cshtml` y `New.cshtml.cs`; conservar los contratos del formulario y el uso de `ILinkService.CreateLink`.
- Mantener el selector basado en colecciones propias y su atributo `data-public`. La vista Razor y su sincronización de cliente deben conservar el comportamiento definido por [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas): selección pública habilitable, sin selección automática, y selección privada o vacía deshabilitada y desmarcada con su explicación.
- No confiar en el estado del checkbox ni en los atributos HTML para autorizar la publicación. `LinkService.CreateLink` ya comprueba la privacidad de la colección y el agregado `Link` mantiene la invariante de dominio. El POST de Web debe seguir delegando la decisión en Application.
- La página de creación ya mapea errores del caso de uso a errores de campo o avisos. Para el rechazo de privacidad debe aplicar el mecanismo de aviso accesible tras redirección que establecen las fuentes, reutilizando el patrón Web de TempData usado por otras operaciones privadas.
- La clase de integración existente `LinkCreationIntegrationTests` será el punto focal para cubrir el POST manipulado y conservar las regresiones de creación, propiedad y antiforgery.

### Flujos internos

1. En GET, representar las opciones de colección con la privacidad que entrega Application; iniciar la opción pública desmarcada y sincronizarla al cargar la página, al cambiar la colección y al restaurar la página.
2. En POST, Web entrega el request a `ILinkService.CreateLink`. Una solicitud que pide publicar en una colección privada debe ser rechazada por la validación existente de Application antes de persistir.
3. En caso de rechazo de privacidad, Web debe conservar el mensaje definido por la fuente, transportarlo mediante el mecanismo de aviso existente y redirigir a `/app/enlaces` conforme a [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados).
4. S9.2 verifica la respuesta del POST, su destino, el transporte del aviso y la ausencia de escritura sin seguir la redirección. S10.1 comprobará que su nueva página consume y presenta ese aviso accesiblemente; esa verificación futura no bloquea el cierre de S9.2.

### Persistencia y dependencias

- No se prevén cambios de esquema, migraciones ni SQL. La regla de privacidad se comprueba en Application/Domain y la creación rechazada no debe insertar un enlace.
- No se amplía el caso de uso de Application: la guarda y el error de dominio para publicación en colección privada ya existen. Se debe verificar su ejecución desde el formulario Web.
- Prerrequisitos disponibles: página de creación de S9.1, identidad privada, `ILinkService.CreateLink` y guarda de dominio/Application.
- Seguimiento posterior: S10.1, en [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l), verificará el consumo y la presentación accesible del aviso que S9.2 transporta al redirigir. S9.2 no espera a que la ruta de destino esté implementada.

### Garantías técnicas

- La selección de colección se obtiene en el servidor dentro del ámbito del usuario identificado; no se confía en `UserId` ni en la privacidad enviada por el cliente ([domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos)).
- El estado visual del control es una ayuda de interfaz, no una barrera de seguridad. El POST manipulado debe seguir fallando en Application/Domain ([domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado)).
- El rechazo no persiste un enlace ni revela datos de otra persona; el comportamiento para colecciones inválidas, inexistentes o ajenas permanece bajo el criterio aprobado CA04 de S9.1.
- No se añade una implementación alternativa de la regla de privacidad en Web. Las escrituras siguen el flujo Web → Application → Domain y los formularios de modificación usan POST con antiforgery ([architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web)).
- La evidencia de la interfaz dinámica no se atribuirá a una prueba HTTP que solo inspeccione HTML estático; se separa de la comprobación del servidor.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el selector, su sincronización actual y la guarda de `ILinkService.CreateLink`; verificar ambos como parte de S9.2 sin trasladar la autorización al cliente. | propuesta | [spec.md → CA01–CA03](spec.md#criterios-de-aceptación); código de S9.1 inspeccionado, sin validación de cierre para S9.2. | Mantiene la lógica existente y centra el trabajo en demostrar el comportamiento y completar el tratamiento Web que falte. |
| La ruta de destino `/app/enlaces` no está implementada todavía; S10.1 debe comprobar el aviso recibido por la ruta. | seguimiento futuro; no bloqueante | [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas); [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l). | S9.2 puede verificar Location, transporte del aviso y no persistencia sin seguir la redirección. S10.1 comprueba que el aviso se presenta accesiblemente. |
| El test Web del POST manipulado comprueba ahora la redirección y el transporte del aviso, pero todavía no puede comprobar su presentación en una página de destino inexistente. | mitigado en S9.2; presentación pendiente de S10.1 | `LinkCreationIntegrationTests`; [mvp0-plan.md → S10.1](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l). | S9.2 demuestra rechazo, no persistencia y transporte; S10.1 validará el aviso accesible al consumir TempData. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada; inspección de la respuesta GET para comprobar que las opciones expresan la privacidad correcta y el estado inicial del control. | Base SQLite de integración y formulario de S9.1. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkCreationIntegrationTests --nologo` | HTML con `data-public` correcto por opción y control inicialmente desmarcado; comprobar estructura de ayuda asociada. No demuestra la ejecución de JavaScript. | Resultado del test focalizado y casos concretos ejecutados. |
| V02 | CA01, CA02 | Manual; cambiar entre colección pública, privada y ninguna selección, incluidos cambios sucesivos y restauración de página. | Aplicación local iniciada con colecciones de ambos estados y navegador disponible. | Recorrido manual del formulario `/app/enlaces/nuevo`; no requiere S9.6. | El control se habilita solo para colección pública, no se marca automáticamente, y se desmarca y explica al quedar privada o vacía. | Navegador/versión, escenarios comprobados y resultado; registrar limitaciones, sin atribuirlo a V01. |
| V03 | CA03 | Automatizada y comprobación negativa; enviar directamente el POST con `Input.IsPublic=true` y una colección privada propia, eludiendo el checkbox. | Formulario y base SQLite de integración; seguir redirecciones desactivado para comprobar el POST sin necesitar la página de destino. | Añadir/ajustar el caso de `LinkCreationIntegrationTests`; ejecutar el mismo filtro de V01. | El servidor rechaza la petición, no persiste ningún enlace y responde redirigiendo a `/app/enlaces` con el aviso transferido. El test debe fallar si se elimina la guarda o se crea el enlace. La presentación accesible del aviso se verifica como seguimiento en S10.1. | Status y `Location`, evidencia del transporte del aviso, recuento de enlaces antes/después y resultado del test. |
| V04 | CA04 | Automatizada; regresión de colección inválida, inexistente o ajena al usuario en el POST. | Fixture de integración de S9.1 con identidades aisladas. | Mismo filtro `LinkCreationIntegrationTests` de V01. | Se conserva sin cambios el rechazo y la no divulgación ya aceptados por S9.1; no se crea enlace. | Casos ejecutados y resultado; no sustituye la validación específica de privacidad de V03. |

La implementación y validación de rechazo de V03 pertenecen a S9.2 y no se delegan a S10.1 ni a S9.5. La presentación accesible del aviso se registra como una comprobación concreta de S10.1 sobre el resultado producido por S9.2; no mantiene abierta esta especificación.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` con checks inmediatos, dependencias explícitas y evidencia por criterio.
- No empezar cambios productivos antes de que `tasks.md` esté aprobado.
- Para cerrar S9.2, reunir evidencia propia para CA01–CA04, incluida la respuesta y el transporte del aviso de V03, y registrar la aceptación humana en las tareas. La comprobación de presentación accesible del aviso queda anotada en S10.1.
- S9.5 y la revisión manual general S9.6 no son prerrequisitos de cierre de esta spec.
