# Plan técnico: S10.3 — Conservar parámetros del listado de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- La página [Links.cshtml.cs](../../../src/Linkubator.Web/Pages/App/Links.cshtml.cs) construye el modelo del formulario de búsqueda y genera los enlaces de paginación. Mantener la responsabilidad en la presentación Web, reutilizando `SearchFormViewModel`, `PaginationViewModelFactory` y el analizador de parámetros existente.
- El formulario compartido [_SearchForm.cshtml](../../../src/Linkubator.Web/Pages/Shared/_SearchForm.cshtml) representa `q` y los campos ocultos aplicables. Verificar que `c` acompaña al formulario, que `p` no se añade y que el valor de búsqueda actual queda precargado.
- La función `BuildPageUrl` de `LinksModel` forma la navegación paginada con los filtros analizados. Mantener la codificación de consulta existente y no copiar parámetros desconocidos ni valores que el analizador haya descartado.
- La cobertura focal se sitúa en [LinkListingIntegrationTests.cs](../../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs). El test `GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection` ya comprueba navegación paginada bajo una búsqueda, conserva el filtro `c` en los enlaces y verifica que el formulario lleva `c` y `q`, pero no `p`. A partir de esta inspección no se da por demostrado el envío real de una búsqueda desde una página posterior; añadir o adaptar pruebas para discriminar cada interacción requerida por los criterios.

### Flujos internos

1. En GET, `LinksModel` analiza la consulta y determina búsqueda, página y colección accesible; conserva en el modelo del formulario solo los valores aplicables.
2. El envío del formulario GET presenta la búsqueda con su filtro contextual, sin arrastrar el número de página actual; la nueva consulta se analiza mediante el flujo existente.
3. La paginación genera destinos con la página seleccionada y los criterios de búsqueda/filtro del resultado actual.
4. Application sigue recibiendo la consulta normalizada por el análisis existente. No cambiar contratos ni reglas del listado.

### Persistencia y dependencias

- No se esperan cambios en Application, Infrastructure, esquema, consultas, transacciones, migraciones ni dependencias externas.
- Prerrequisitos disponibles: listado de enlaces, análisis de parámetros y controles compartidos entregados por S10.1 y S10.2; suite de integración HTTP con SQLite en memoria.
- No esperar el buscador del dashboard de S10.4, la suite transversal de S10.5 ni la revisión manual de S10.6 para cerrar este trabajo.

### Garantías técnicas

- Mantener la autorización y el aislamiento existentes: el identificador de usuario procede de la identidad fiable y el filtro `c` solo se aplica conforme al análisis para el usuario identificado ([domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)).
- No serializar ni propagar parámetros desconocidos, no aplicables o descartados al construir la navegación; el analizador existente permanece como responsable de esa decisión ([specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas)).
- Codificar correctamente los valores de consulta al renderizar campos/enlaces y evitar que una nueva búsqueda incluya accidentalmente `p`.
- No cambiar la búsqueda, el orden, los límites ni las respuestas definidos por las fuentes propietarias.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar los modelos y componentes actuales del formulario y paginación, y cambiar solo los puntos que no satisfagan los criterios comprobados. | propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación); inspección de implementación para delimitar el trabajo, no evidencia de cierre. | Evita duplicar lógica de navegación; confirmar brechas con pruebas focalizadas antes de alterar código. |
| Los destinos de paginación se construyen con los valores analizados, no con la consulta HTTP en bruto. | propuesta | [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas); diseño actual de `LinksModel`. | Conserva solo filtros aplicables y evita transportar valores rechazados o desconocidos. |
| El formulario conserva `c` y excluye `p` para que una búsqueda nueva comience en la primera página. | confirmado | [spec.md → CA01 y CA03](spec.md#criterios-de-aceptación), [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l). | Cubre el comportamiento asignado sin cambiar análisis ni reglas funcionales. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA03 | Automatizada, integración HTTP: abrir una página posterior filtrada, inspeccionar el formulario y enviar una nueva búsqueda desde ese contexto. Verifica que el filtro aplicable se mantiene y que el formulario no lleva `p`; el test no amplía la suite transversal de S10.5. | Host de integración y fixture aislado con más de una página en la colección propia. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests.GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection --nologo` (actualizar el filtro si se renombra el test o se añade un caso específico). | El formulario de búsqueda incluye la colección y el término visible, no incluye la página actual; al seguir la navegación de búsqueda se inicia en página 1 con el contexto de colección conservado. | Nombre de test/casos, URL o campos observados, resultado de ejecución, fecha y SDK. |
| V02 | CA02 | Automatizada, integración HTTP: resultado de búsqueda paginado con elementos en más de una página y filtro de colección. Comprobar los `href` emitidos y solicitar una página siguiente. | Host de integración; fixture con suficientes coincidencias en la colección filtrada y datos no coincidentes en otra colección y otro usuario. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests.GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection --nologo` | El enlace a la página destino conserva `q` y `c`, cambia `p` según la navegación y la respuesta contiene únicamente los resultados correspondientes al mismo filtro. | URL emitida, títulos esperados por página, status y salida focalizada con fecha/SDK. |
| V03 | CA01–CA03 | Revisión técnica focalizada del modelo/vista y cobertura de los tests; comprobar también navegación sin colección, para confirmar que no se inventa un filtro. | Tests focalizados añadidos o adaptados y cambios integrados. | `git diff --check`; revisar los casos de `LinkListingIntegrationTests` asociados a CA01–CA03. | Diff limpio; los tests comprueban el formulario y los destinos reales de paginación sin atribuir a HTML aislado la verificación de una interacción de lector de pantalla ni la cobertura de S10.5. | Resultado de `git diff --check`, enlaces a los tests y límites explícitos de la comprobación. |

La cobertura actual de `GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection` inspecciona el formulario en la primera página y los enlaces paginados. No prueba por sí sola el envío de una nueva búsqueda desde una página posterior; V01 debe hacer observable esa condición mediante el test ajustado. Estas comprobaciones no cubren la integración del buscador del dashboard ni la revisión manual, que quedan fuera de esta spec.

## Orden de ejecución y cierre

- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG el 27-X-2026 («Vamos»), implementadas y aceptadas por DLG el 27-X-2026 («Completado»). Sus tareas asignan a esta spec los cambios y comprobaciones que cubren CA01–CA03.
- Cierre técnico: evidencia satisfactoria de todos los criterios, tareas propias completadas y aceptación humana registrada; no depende de S10.4–S10.6 ni cierra por sí solo la etapa S10.
- Si una prueba revela discrepancia con una regla propietaria, detener el cambio afectado y resolver primero esa regla en su documento fuente.
