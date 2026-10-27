# Plan técnico: S10.1 — Listado y búsqueda de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- La página Razor `Pages/App/Links.cshtml` y su modelo `Links.cshtml.cs` ya componen el listado con `ILinkService.ListLinksPage`, las colecciones disponibles, el analizador de parámetros y los modelos compartidos de búsqueda, aviso, errores, estado vacío, tarjetas y paginación. Mantener esa separación Web → Application y reutilizar los modelos existentes.
- Conservar la selección visual del menú lateral, el campo de búsqueda y los enlaces de paginación basados en la consulta analizada. Los cambios de esta spec se limitan a completar los criterios de CA01 y CA02 y a confirmar la integración de CA03; no duplicar reglas de análisis que pertenecen al analizador ni adelantar el alcance de S10.2–S10.4.
- S9.2 guarda el mensaje de rechazo de privacidad en TempData y devuelve al listado. La vista de enlaces ya incluye `_LinkCreationNotice.cshtml` para avisos de creación y `_PageAlert.cshtml` para avisos de gestión. Inspeccionar el modelo de aviso y el contenido compartido para garantizar que el error de privacidad transferido se muestra en una región accesible con su texto intacto; reutilizar o ajustar el componente compartido solo si la implementación actual no satisface CA03. No crear un canal de notificación paralelo.
- La vista ya representa el caso de error de listado y el estado sin resultados mediante `_EmptyState.cshtml`, y las tarjetas y paginación mediante componentes de presentación existentes. Preservar el comportamiento que ya satisface esas responsabilidades.
- La prueba focal de integración Web es `LinkListingIntegrationTests`; el transporte del aviso y la ausencia de creación se comprueban desde `LinkCreationIntegrationTests` de S9.2. Añadir o adaptar las pruebas de integración propias para que el POST rechazado seguido de GET al listado demuestre que el aviso transferido realmente se representa de manera accesible, no solo que se almacena o redirige.

### Flujos internos

1. En GET, `PrivatePageModel` resuelve usuario y colecciones propias; `LinksModel` analiza los parámetros que correspondan al estado actual y consulta Application mediante `ListLinksPage`.
2. Web mapea los resultados del caso de uso a `LinkCardViewModel`, forma enlaces de paginación y renderiza el estado de resultados o vacío mediante los componentes existentes.
3. Para el rechazo de privacidad, el POST de creación ya redirige al origen permitido y almacena el mensaje en TempData. En el GET subsiguiente, la página de listado lo pasa a la región de aviso y Razor lo codifica contextualmente. El test de integración debe seguir la redirección y verificar el HTML final, el rol/semántica accesible del aviso y que no se insertó enlace.
4. No cambiar el contrato del servicio de listado, la persistencia, las reglas de privacidad ni el comportamiento del POST que ya cubre S9.2.

### Persistencia y dependencias

- No se prevén cambios de persistencia, esquema, migraciones, índices ni consultas SQL. Application e Infrastructure ya aportan la consulta paginada y la proyección FTS5 del listado.
- Dependencias disponibles: ruta privada, identidad de desarrollo, `ILinkService.ListLinksPage`, analizador Web de parámetros, componentes de presentación, página de creación y aviso transferido de S9.2.
- No esperar S10.2–S10.6: sus reglas de parámetros, persistencia de estado en controles, dashboard y suite transversal/manual se mantienen fuera de este alcance. S10.1 debe incluir en su prueba cualquier caso mínimo propio necesario para demostrar los criterios de este trabajo.

### Garantías técnicas

- No aceptar ni propagar un `UserId` seleccionado por cliente; la consulta privada se mantiene vinculada al usuario identificado ([domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos)).
- La presentación del aviso no modifica la decisión de privacidad que tomó Application ni convierte el rechazo en operación exitosa ([domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado)).
- El aviso mantiene contenido textual codificado contextualmente por Razor; el test verifica el rol/semántica de anuncio accesible sin atribuir al HTML estático la prueba de funcionamiento de un lector de pantalla.
- El estado vacío debe distinguirse visual y semánticamente de error de carga y de error de parámetros, usando componentes existentes y sin suprimir dichos errores.
- No añadir reglas de producto ni cambiar mensajes definidos por sus fuentes.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar la ruta, página, analizador, servicio de listado y componentes compartidos ya existentes; realizar cambios quirúrgicos solo donde CA01–CA03 no estén cubiertos. | propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación); implementación disponible inspeccionada para delimitar el cambio, no considerada evidencia de cierre. | Evita duplicar el listado o abstraer lógica existente; el plan se actualizará si los tests dirigidos muestran una carencia concreta. |
| El aviso de rechazo de privacidad se presenta mediante el mecanismo TempData ya establecido por S9.2 y el componente accesible común. | propuesta | [S9.2 → «Criterios de aceptación»](../s9.2-publicacion-enlaces/spec.md#criterios-de-aceptación), [«Errores controlados»](../../context/specifications.md#errores-controlados), [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad). | Comprobar el GET destino tras la redirección; una prueba que solo verifica el estado TempData no acredita su representación accesible. |
| La integración existente muestra tarjetas, estado vacío y aviso, y el flujo completo de redirección de S9.2 ha sido comprobado con tests focalizados nuevos. | confirmado en validación focalizada de S10.1 | [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs); ejecución 153/153 el 27-X-2026 con .NET SDK 10.0.401. | El HTML final incluye el mensaje en región `role="status"` y `aria-live="polite"`; no se requirió cambio productivo. La evidencia no demuestra interacción real de lector de pantalla. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; integración HTTP del listado con enlaces propios y ajenos, consulta textual y filtro de colección dentro del alcance ya disponible. | Host de integración con SQLite en memoria y datos de prueba aislados. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests --nologo` | Los resultados mostrados corresponden al usuario y filtros aplicables, y se conserva el orden/paginación del contrato. No pretende validar aquí todos los parámetros negativos o no disponibles de S10.2–S10.3. | Resultado focalizado y casos concretos CA01; salida de tests con fecha y SDK. |
| V02 | CA02 | Automatizada; integración HTTP con listado general vacío y filtro de colección sin resultados. | Host de integración y colección propia vacía. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests --nologo` | Se renderiza el componente vacío accesible sin tarjetas ni paginación presentada como resultado; los errores conservan tratamiento distinto. | Casos de estado vacío, resultado y fecha/SDK. |
| V03 | CA03 | Automatizada; flujo HTTP end-to-end de creación manipulada seguido de GET tras la redirección, inspeccionando el mensaje que llega al documento final y ausencia de persistencia. Incluye comprobación negativa al inspeccionar el HTML antes del flujo o un caso sin aviso: el mensaje no debe aparecer espuriamente. | Host de integración y colección propia privada; token antiforgery válido; cookie de TempData entre peticiones. | Añadir o adaptar caso en `LinkCreationIntegrationTests` (filtro `FullyQualifiedName~LinkCreationIntegrationTests`) o `LinkListingIntegrationTests` (filtro `FullyQualifiedName~LinkListingIntegrationTests`), ejecutado con `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkListingIntegrationTests" --nologo`. | POST rechazado no crea enlace y redirige; GET destino contiene exactamente el mensaje fuente en una región de aviso semántica/accesible. El caso sin TempData no presenta el aviso. La inspección HTTP no demuestra anuncio real por un lector de pantalla. | Nombre del test, status/Location, comprobaciones de HTML/semántica, conteo de enlaces y resultado negativo, fecha/SDK. |
| V04 | CA01–CA03 | Integración técnica; compilar y ejecutar pruebas focalizadas combinadas, revisar diff y enlaces documentales. | Cambios de implementación y pruebas dentro de S10.1. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests|FullyQualifiedName~LinkCreationIntegrationTests" --nologo` y `git diff --check` | Las pruebas dirigidas pasan, build de proyecto sin errores, diff limpio y las evidencias corresponden solo a los criterios de esta spec. | Salida real de comandos, fecha/SDK, resumen de diff y rutas de pruebas. |

Las pruebas HTTP acreditan el renderizado y el flujo de TempData observado en el servidor; no reemplazan una auditoría con lector de pantalla. S10.1 no tiene asignada revisión manual del buscador con datos de ejemplo, que corresponde a S10.6.

## Orden de ejecución y cierre

- [Tareas](tasks.md): pendientes de aprobación; incluyen cambios y tests directos de CA01–CA03, comprobación integrada y revisión humana.
- Cerrar S10.1 cuando sus criterios tengan evidencia satisfactoria y tareas completadas, sin esperar ningún entregable posterior de S10.2–S10.6, y con aceptación humana registrada.
- Si los tests muestran que las reglas fuente requieren cambios, detener la tarea afectada y tramitar la decisión en el documento propietario antes de modificarlo.
