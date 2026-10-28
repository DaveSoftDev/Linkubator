# Tareas: S11.3 — Páginas públicas de colecciones

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 («Siguiente»).
- Implementación:
  - Estado: completada.
  - Inicio: 27-X-2026, inicio autorizado por la aprobación de estas tareas y verificado con la implementación inicial de las páginas públicas de colecciones.
  - Finalización: 27-X-2026; compilación Web correcta y validaciones focalizadas completadas, según T06 y la evidencia por criterio.
  - Evidencia consolidada: [validación y aceptación](#aceptación-y-cierre), [evidencia por criterio](#evidencia-por-criterio) y validaciones V01–V04 en [plan.md](plan.md#estrategia-de-validación).
- La aprobación de estas tareas autoriza la implementación, pero no registra su inicio ni inventa evidencias.

## Descomposición

T02 verificó las lecturas públicas existentes. T03 y T04 implementaron de forma independiente sus rutas y vistas; T05 añadió las pruebas HTTP y T06 consolidó la validación técnica antes de registrar la aceptación humana en T07.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Al iniciar la implementación, actualizar el estado de implementación en este documento y sincronizar el roadmap y el índice con el inicio real de S11.3. | Seguimiento SDD; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar que el inicio está autorizado por la aprobación de estas tareas, registrar fecha y referencia reales, y reflejar S11.3 en el roadmap y el índice sin alterar el estado de otras etapas. | completada | Implementación iniciada el 27-X-2026 con la creación de las páginas públicas de colecciones y validación técnica focalizada. |
| T02 | Verificar las lecturas públicas existentes y su cobertura; corregir de forma acotada Application/Infrastructure y sus pruebas solo si no satisfacen los criterios de acceso, orden o paginación. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias), V03 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqlitePublicReadRepositoryTests"`; pasan los casos de visibilidad, orden y límites de página requeridos. | completada | V03: `SqlitePublicReadRepositoryTests` 19/19 y los dos tests de orden de colecciones/enlaces en `SqlitePersistenceIntegrationTests` 2/2. |
| T03 | Crear la Razor Page y el `PageModel` de `/{alias}/colecciones/`, enlazarlos a la lectura pública existente y renderizar colecciones, estado vacío, metadatos y canonical. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T02 | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; la Web compila con la ruta y la página implementadas. | completada | La compilación Web de cierre pasó con 0 advertencias y 0 errores. |
| T04 | Crear la Razor Page y el `PageModel` de `/{alias}/colecciones/{slug}`, interpretar `p`, resolver fuera de rango, renderizar enlaces públicos y construir metadatos y canonical normalizada. | CA02, CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T02 | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; la Web compila con la ruta, paginación y canonical implementadas. | completada | La compilación Web de cierre pasó con 0 advertencias y 0 errores; V02 verifica la normalización de `p` y la canonical. |
| T05 | Añadir pruebas de integración HTTP para las dos rutas, incluyendo estados vacíos, alias y colecciones no accesibles, contenido privado, metadatos, canonical, paginación y HTML renderizado. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01, V02 y V04 | T03, T04 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.PublicCollectionsPage|FullyQualifiedName~WebStartupTests.PublicCollectionPage"`; las respuestas HTTP satisfacen los casos cubiertos y no exponen contenido privado. | completada | 9/9 pruebas HTTP focalizadas correctas; V01, V02 y V04. |
| T06 | Ejecutar la validación focalizada completa y revisar la integración de páginas, lecturas, canonical y pruebas; registrar los resultados reales y sus límites. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas) | T05 | Ejecutar los filtros V01–V04 del plan y `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; todas las comprobaciones aplicables pasan y cada CA tiene evidencia. | completada | Validación de cierre: `WebStartupTests` 31/31; `SqlitePublicReadRepositoryTests` 19/19; dos tests específicos de orden de persistencia 2/2; build Web con 0 advertencias y 0 errores. `git diff --check` no disponible: esta carpeta no es un worktree Git. |
| T07 | Revisar la evidencia de CA01–CA04 y registrar la decisión humana de aceptación o las correcciones requeridas; actualizar el roadmap y el índice conforme al resultado real. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Comprobar resultados y evidencias de V01–V04; registrar aceptación humana explícita o dejar constancia del bloqueo y su siguiente acción. | completada | DLG aceptó el resultado el 27-X-2026 con la referencia «Completado». Roadmap e índice sincronizados con la finalización de S11.3. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T03, T05, T06 | V01, V03 y V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: listado público, estado vacío, alias inexistente, metadatos, canonical y exclusión de colecciones privadas comprobados por HTTP; lecturas, orden y visibilidad comprobados en persistencia. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 31/31, `SqlitePublicReadRepositoryTests` 19/19 y test de orden de colecciones 1/1; comprobaciones V01, V03 y V04. |
| CA02 | T02, T04, T05, T06 | V02, V03 y V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: detalle público renderizado con metadatos, canonical y enlaces públicos; colecciones privadas, inexistentes y sin enlaces públicos responden `404`; contenido privado excluido. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 31/31, `SqlitePublicReadRepositoryTests` 19/19 y test de orden de enlaces 1/1; comprobaciones V02, V03 y V04. |
| CA03 | T02, T04, T05, T06 | V02 y V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: comprobados ceros iniciales, primer valor repetido, parámetro inválido y página fuera de rango; canonical normalizada y ajena al Host y a parámetros desconocidos. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 31/31 y `SqlitePublicReadRepositoryTests` 19/19, incluidos límites de página; comprobaciones V02 y V03. |
| CA04 | T03, T04, T05, T06 | V01, V02 y V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: ambas rutas verificadas mediante respuestas HTML completas de integración HTTP, con encabezados y contenido renderizado por servidor. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 31/31; comprobaciones V01, V02 y V04. |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 cuentan con evidencia satisfactoria.
- Bloqueos y límites de validación: `git diff --check` no pudo ejecutarse porque `FEATURE\S11-web-5` no es un worktree Git; las pruebas focalizadas y la compilación Web sí concluyeron correctamente.
- Aceptación humana: DLG, 27-X-2026, referencia «Completado».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Seguimiento independiente: S11.4 conserva los atributos transversales asignados a esa etapa; no bloquean el cierre de S11.3.
