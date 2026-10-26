# Tareas: S8.1 — Listado de colecciones

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 25-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: 25-X-2026, DLG («Inicia la implementación»).
  - Actualizaciones autorizadas: DLG aprobó un resultado paginado de Application con total, preservando `ListAllCollections` (25-X-2026, «Sí: aplica el resultado paginado nuevo y conserva ListAllCollections intacto»); también autorizó sincronizar la regla de `p` inválido/fuera de rango a primera página sin error en `specifications.md`.
  - Finalización técnica: 25-X-2026; suite focalizada 112/112, build Web 0 advertencias/0 errores y `git diff --check` sin errores.
  - Evidencia consolidada: CA01–CA03 con tests automatizados; CA04 con revisión manual acotada en Chrome de escritorio, documentada en T04. La versión/resolución del navegador no quedó registrada y no se documentan comprobaciones manuales de paginación.
- Aceptación humana: DLG, 25-X-2026, referencia «Completado».
- Cierre: completado y aceptado el 25-X-2026.
- Archivado: completado el 26-X-2026.
- Ubicación archivada: `specs/archive/s8.1-listado-colecciones/`.
- Fecha efectiva de archivado y comprobación de enlaces: 26-X-2026; carpeta activa ausente y enlaces entrantes/salientes comprobados.

## Descomposición

La operación de listado original de Application aplicaba búsqueda, página y ámbito del usuario identificado, pero no exponía el total de páginas. La implementación añadió una operación paginada dedicada para este listado y la integró en la ruta Web, preservando la operación `ListAllCollections` preexistente y sin depender del cierre de otros consumidores de S7.6.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar PageModels privadas, analizador S7.6, componentes de búsqueda/estado vacío/paginación y los tests/fixtures de Web existentes; identificar el filtro focalizado real y los casos HTTP reutilizables. | CA01–CA04; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación) | Ninguna | Revisar contratos, componentes, fixtures y tests; resultado: contrato y piezas reutilizables identificados y filtro HTTP que debe añadirse determinado. | completada | 25-X-2026: `ICollectionService.ListCollections(query,page,token)` existe y `CollectionService` limita la consulta a `ICurrentUser.UserId`; `PrivateListingQueryAnalyzer` admite listados de colecciones; existen parciales de búsqueda, paginación y estado vacío; no hay tests HTTP del listado. Baseline `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~Linkubator.Tests.Web"`: PASS 17/17. El contrato original no entregaba el total. Hay cambios preexistentes a `ListAllCollections` y a su uso en `PrivatePageModel`; se preservan, pero no cumplen el contrato paginado aprobado y requieren revisión separada. |
| T02 | Implementar la Razor Page GET `/app/colecciones`, con operación de Application que devuelva filas y total de páginas usando consultas paginadas; mantener intacto `ListAllCollections` del sidebar. | CA01–CA03; [plan → «Flujos internos»](plan.md#flujos-internos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | T01 | Build Web/Application e integración del listado; resultado: se obtiene el total sin materializar todas las colecciones y la ruta renderiza navegación, búsqueda y estado vacío. | completada | 25-X-2026: página `/app/colecciones`, nueva operación `ListCollectionsPage` y modelo paginado implementados. Build Web correcto y suite focalizada final 112/112. Incluye estado vacío, búsqueda, parámetros, paginación y aislamiento; no reemplaza la revisión manual CA04. |
| T03 | Añadir o completar pruebas focalizadas HTTP para aislamiento, búsqueda, parámetros, orden, paginación, conservación de filtros y estados vacíos, incluyendo caso negativo de identidad/propiedad. | CA01–CA03; [plan → «Garantías técnicas»](plan.md#garantías-técnicas) y V01–V03 | T02 | Ejecutar filtro de test identificado; resultado: todas las pruebas focalizadas pasan y el caso negativo detecta el intento de variar el usuario desde la petición. | completada | 25-X-2026: pruebas HTTP de estado vacío, filtro, paginación, búsqueda demasiado larga, `p=0` a primera página e aislamiento frente a `userId` manipulado. Suite focalizada final 112/112 tras corregir el caso de búsqueda excesiva; build Web 0 warnings/0 errors. `git diff --check` limpio, con aviso de conversión LF→CRLF en `styles.css`. |
| T04 | Revisar manualmente operación por teclado y accesibilidad del listado y el estado vacío; registrar navegador, entorno, pasos, resultado y límites. | CA04; [plan → V04](plan.md#estrategia-de-validación) | T02 | Seguir pasos concretos: abrir `/app/colecciones`; recorrer con Tab/Shift+Tab y observar foco visible en la búsqueda y estado vacío; registrar navegador, viewport, resultado y límites. | completada | 25-X-2026, Chrome de escritorio: DLG informa haber probado Tab y Shift+Tab y observado foco visible en la búsqueda y el estado vacío. Se considera satisfactoria para estas comprobaciones. No se comunicaron versión/resolución, revisión manual de resultados/paginación, ni comprobaciones independientes de encabezados/nombres accesibles; esos aspectos no se afirman como revisados. |
| T05 | Ejecutar validación integrada, revisar el diff y comprobar la trazabilidad de cada criterio; actualizar el resumen de implementación y las referencias de roadmap. | CA01–CA04; [plan → V05](plan.md#estrategia-de-validación) | T03, T04 | Ejecutar test focalizado, build Web y `git diff --check`; resultado: checks satisfactorios y sin cambios no autorizados de fuentes funcionales. | completada | 25-X-2026: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~CollectionsListingIntegrationTests|FullyQualifiedName~CollectionServiceTests|FullyQualifiedName~SqliteCollectionRepositoryTests|FullyQualifiedName~Linkubator.Tests.Web"` PASS 112/112; `dotnet build src\Linkubator.Web\Linkubator.Web.csproj --no-restore` PASS, 0 warnings/0 errors; `git diff --check` PASS, sin errores (aviso de normalización LF→CRLF en `styles.css`). Al inicio de esta validación hubo 3 fallos en los tests por un helper que buscaba un `<ul>` antiguo tras el cambio a tabla; se corrigió el selector a `<table>` y la ejecución recompilada quedó 112/112. |
| T06 | Revisar evidencia y registrar aceptación humana del resultado o los cambios solicitados. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Confirmar evidencia por criterio y decisión humana explícita; resultado: aceptación registrada o bloqueo con acciones requeridas. | completada | Aceptación explícita de DLG el 25-X-2026 con referencia «Completado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | V01, V05 en [plan.md](plan.md#estrategia-de-validación) | La integración HTTP comprueba el aislamiento frente a un `userId` manipulado y colecciones ajenas; suite final 112/112 | 25-X-2026; .NET 10, SQLite por prueba | `CollectionsListingIntegrationTests`; suite focalizada final 112/112 |
| CA02 | T01–T03 | V02, V05 en [plan.md](plan.md#estrategia-de-validación) | Suite focalizada final 112/112; incluye `p=0` a primera página sin error y búsqueda excesiva con validación | 25-X-2026; .NET 10, suite Linkubator.Tests | `dotnet test` focalizado recompilado, 112/112; build Web 0 warnings/0 errors |
| CA03 | T02–T03 | V03, V05 en [plan.md](plan.md#estrategia-de-validación) | Estado vacío renderizado sin filas y comprobado por integración HTTP; suite final 112/112 | 25-X-2026; .NET 10, SQLite por prueba | `CollectionsListingIntegrationTests` y suite focalizada final 112/112 |
| CA04 | T02, T04–T05 | V04, V05 en [plan.md](plan.md#estrategia-de-validación) | Revisión manual satisfactoria y limitada a teclado Tab/Shift+Tab y foco visible en búsqueda/estado vacío; alcance y detalles faltantes anotados en T04 | 25-X-2026; Chrome de escritorio (versión/viewport no informados) | Declaración manual de DLG registrada en T04; aceptación general «Completado» registrada en T06 |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado.
- Criterios con evidencia satisfactoria: CA01–CA04; el alcance acotado de la revisión manual CA04 se describe en T04.
- Bloqueos y riesgos restantes: ninguno que impida el cierre de S8.1. La versión y viewport de Chrome no se registraron y la revisión manual se limita a teclado/foco de búsqueda y estado vacío. Los cambios preexistentes a `ListAllCollections` y su uso en el sidebar se preservaron y no se consideran evidencia funcional de S8.1; cualquier revisión de esos cambios queda separada.
- Aceptación humana: DLG, 25-X-2026, referencia «Completado».
- Evidencia de cierre: T04 registra la comprobación manual; T05 registra suite focalizada 112/112, build Web 0 warnings/0 errors y `git diff --check` sin errores.
- Archivado: completado el 26-X-2026.
- Ubicación archivada: `specs/archive/s8.1-listado-colecciones/`.
- Fecha efectiva de archivado y comprobación de enlaces: 26-X-2026; carpeta activa ausente y enlaces entrantes/salientes comprobados.
