# Plan técnico: S10.2 — Parámetros del listado de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- La página [Links.cshtml.cs](../../../src/Linkubator.Web/Pages/App/Links.cshtml.cs) llama a `PrivateListingQueryAnalyzer.Analyze` con la consulta HTTP y las colecciones del usuario identificado; si hay errores, construye el resumen accesible y no invoca `ILinkService.ListLinksPage`.
- El analizador [PrivateListingQueryAnalyzer.cs](../../../src/Linkubator.Web/Models/PrivateListing/PrivateListingQueryAnalyzer.cs) selecciona el primer valor de cada parámetro, interpreta `p`, `c` y `q`, descarta los filtros de colección no accesibles y produce estados de error para los valores inválidos y la búsqueda excesiva. Para S10.2 se comprobará el tratamiento de `e` como desconocido/no aplicable y la validación de la longitud de `q`, conservando lo que ya satisface CA02.
- La vista y el componente existente de [resumen de errores](../../../src/Linkubator.Web/Pages/Shared/_ErrorSummary.cshtml) son responsables de presentar errores de consulta; no se crearán controles ni estilos paralelos.
- La prueba focal de integración existente es [LinkListingIntegrationTests.cs](../../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs). Los tests unitarios del analizador están en `PrivateListingQueryAnalyzerTests`; adaptar o extender esos tests solo para la cobertura directa apropiada, manteniendo una comprobación HTTP para el comportamiento de la página.
- No modificar el contrato de Application ni Infrastructure: el comportamiento requerido pertenece al análisis de la consulta y su integración en Web.

### Flujos internos

1. El handler GET carga las colecciones asociadas al usuario identificado y analiza los primeros valores de consulta.
2. Los parámetros no aplicables se descartan; el filtro de colección válido que no identifica una colección del usuario no establece `CollectionId`.
3. Si el texto `q` excede su longitud máxima según el analizador, el handler construye el resumen de error accesible y retorna la página sin ejecutar la consulta de enlaces. Si el análisis es válido, mantiene el flujo de listado existente.
4. Razor presenta el error mediante el componente existente de resumen, codificando el texto según el comportamiento de vista actual.

### Persistencia y dependencias

- No se prevén cambios de esquema, consultas, transacciones, migraciones ni dependencias externas.
- Se reutilizan el host de integración, SQLite en memoria, datos de usuario/colección ya disponibles y los tests focalizados existentes.
- El test de CA03 debe distinguir un error de parámetro de una consulta ejecutada: añadir enlaces que coincidirían con la parte truncada o, alternativamente, confirmar con verificación unitaria que el valor no se entrega al servicio; la prueba HTTP debe acreditar además que el mensaje accesible aparece en el HTML.
- No esperar la integración transversal de S10.5 ni la revisión manual de S10.6 para cerrar este trabajo.

### Garantías técnicas

- La comprobación de accesibilidad de `c` se realiza respecto de las colecciones ya cargadas para el usuario identificado; no se utiliza un identificador de usuario del cliente para autorizar recursos ([domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)).
- Los valores de parámetros no aplicables no deben convertirse en búsqueda o filtro de Application. La prueba de `e` usará valores no numéricos/repetidos junto a datos que pudieran revelar un filtro aplicado, demostrando que no genera error ni modifica los resultados.
- Una búsqueda excesiva detiene el flujo antes de invocar el servicio de listado; el test inspecciona la respuesta de la página y una comprobación del punto de invocación o del analizador, no infiere la ausencia de consulta a partir del texto del mensaje solamente.
- No se cambian los errores o mensajes de la fuente propietaria ni se incluyen datos privados en la respuesta.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar la página y analizador de parámetros existentes; limitar los cambios a una carencia demostrada por las pruebas de CA01–CA03. | propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación); implementación inspeccionada para delimitar, no evidencia de cumplimiento. | Evita duplicar la interpretación de parámetros y la presentación de errores; actualizar el plan solo si los tests revelan divergencia concreta. |
| La semántica de `e` se implementa como parámetro no aplicable, sin extender el analizador con soporte para etiquetas. | confirmado | [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas), [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance). | Mantiene el alcance acordado de MVP0 sin una ruta de filtrado que dependa de gestión de etiquetas. |
| La verificación de CA03 combina una prueba HTTP del resumen de error con evidencia de que el servicio no se invoca o de que el analizador entrega error sin texto utilizable para consulta. | propuesta | [spec.md → CA03](spec.md#criterios-de-aceptación). | Hace que la prueba discrimine entre mostrar un aviso y realmente impedir ejecutar la búsqueda inválida. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración HTTP de listado con datos de más de una colección del mismo usuario y enlaces ajenos; `e` con valor inválido, distinto y/o repetido. | Host Web de pruebas y SQLite en memoria con fixtures aislados. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests.GetLinks_WithRepeatedAndUnsupportedParameters_UsesOnlyFirstApplicableValues --nologo` (ampliar el filtro al nombre del nuevo test de `e` si se añade). | Respuesta sin error de parámetros; los resultados coinciden con los otros filtros aplicables y `e` no afecta al contenido. La prueba no implica que se haya implementado soporte de etiquetas. | Nombres de tests, respuesta observada, resultados esperados y resultado/comando con fecha y SDK. |
| V02 | CA02 | Automatizada, integración HTTP para colección propia, inexistente y ajena; comparar que el filtro inaccesible se ignora y no filtra los resultados ni expone datos ajenos. | Host Web y colecciones/enlaces de usuario propio y usuario de fixture ajeno. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests.GetLinks_WithForeignOrMissingCollection_IgnoresFilterWithoutDisclosingPrivateData --nologo` | Para ambos identificadores no accesibles se muestra el listado del usuario identificado sin revelar la colección ni los enlaces ajenos; respuesta externa equivalente y otros parámetros aplicables conservados. | Nombres de casos, status, tarjetas mostradas/no mostradas y salida de test con fecha y SDK. |
| V03 | CA03 | Automatizada; prueba de integración HTTP con `q` sobre el máximo, además de test focal del analizador o verificación de que no se llama al servicio de listado. Incluir límite exacto válido como contraste. | Host Web de pruebas; fixture que haga observable si se ejecutó consulta con búsqueda. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests.GetLinks_WithOversizedSearch_ShowsErrorWithoutResults --nologo` (ampliar al test de límite/análisis que se agregue). | El valor superior al máximo muestra el mensaje fuente en resumen accesible, no renderiza resultados y no llega a una búsqueda de Application; el límite válido sigue siendo aceptado. | Valor límite/exceso usado, estado y HTML relevante, mecanismo que confirma no invocación, test y fecha/SDK. |
| V04 | CA01–CA03 | Validación técnica focalizada; tests modificados y revisión de diff/enlaces. | Implementación y pruebas de la spec. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests|FullyQualifiedName~PrivateListingQueryAnalyzerTests" --nologo`; `git diff --check` | Todos los tests focalizados pasan, diff sin errores de whitespace y cobertura de cada criterio vinculada a prueba; no se afirma ejecución de la suite completa. | Resultado exacto de ambos comandos, versión SDK, tests/casos vinculados y límites de la evidencia. |

La prueba HTTP valida la respuesta de página y el aislamiento visible; no es una revisión con lector de pantalla ni demuestra por sí sola que Application no se llamó. La ausencia de la búsqueda excedida debe acreditarse mediante test del punto de invocación o la salida analizada antes de la llamada. La suite completa y los escenarios amplios derivados para S10 pertenecen a otros trabajos y no condicionan el cierre de S10.2.

## Orden de ejecución y cierre

- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG el 27-X-2026 («Vamos»); implementación y validación focalizada completadas, y aceptación humana registrada como «Completado» el 27-X-2026.
- S10.2 se cierra con evidencia satisfactoria para CA01–CA03 y tareas propias completas; no depende de S10.3–S10.6 ni cierra la etapa S10.
- Si una prueba revela que la regla fuente no coincide con el resultado esperado, detener el cambio dependiente y tramitar primero la decisión en su documento propietario.
