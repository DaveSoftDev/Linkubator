# Tareas: S3.2 — Invariantes de propiedad

## Referencias y autorización

- Estado: aprobado por DLG el 12-X-2026 («Confirmado»).
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («VAlidado»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Listo»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Adelante»).
    - Implementación: aprobada por DLG el 12-X-2026 («Confirmado»).
- Implementación:
    - Estado: aprobada y validada.
    - Inicio: 12-X-2026 («Adelante»).
    - Finalización: 12-X-2026; validación técnica completa con `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal`.
    - Evidencia consolidada: [T02](#descomposición), [T03](#descomposición), [T04](#descomposición) y la validación técnica del proyecto de tests.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

No iniciar ninguna tarea de implementación hasta que DLG apruebe explícitamente este desglose.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Comprobar la línea base con V01; al iniciar la implementación tras la aprobación de estas tareas, registrar su fecha y referencia y actualizar el estado trazable en [roadmap.md](../../context/roadmap.md#now). | Preparación; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01. | Ninguna | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal`; suite existente verde antes de los cambios. | completada | Verificada con la regresión reproducida y la suite final del proyecto de pruebas. |
| T02 | Adaptar la creación de `Link` para comprobar la propiedad de la colección y añadir tests de creación válida y rechazo por propietarios distintos. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V02](plan.md#estrategia-de-validación). | T01 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~LinkTests" --verbosity minimal`; pasan los casos de creación propia y ajena. | completada | `Link.Create(long userId, Collection collection, ...)` validado por `LinkTests`. |
| T03 | Añadir el movimiento de `Link` a una colección y tests para destino propio, rechazo de destino ajeno y conservación del estado tras rechazo. | CA03, CA04; [plan.md → «Flujos internos»](plan.md#flujos-internos), [V03](plan.md#estrategia-de-validación). | T02 | Ejecutar el filtro de `LinkTests`; el movimiento válido actualiza la colección y el inválido conserva `CollectionId` y `UserId`. | completada | `MoveToCollection(Collection destinationCollection)` validado por `LinkTests`. |
| T04 | Ejecutar la suite completa y el build con advertencias como errores. | CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V04. | T02, T03 | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; suite y build correctos. | completada | Ejecutado con `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal` y 540 tests correctos. |
| T05 | Consolidar evidencia por criterio, registrar la finalización de la implementación y sincronizar el estado y los enlaces de S3.2 en el roadmap. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre), V05. | T04 | Revisar la trazabilidad criterio → test → evidencia; todos los criterios tienen resultado real satisfactorio y el roadmap apunta al estado vigente. | completada | Evidencia técnica disponible y aprobada por DLG con la referencia «Confirmado». |
| T06 | Revisar el resultado y registrar la aceptación o los bloqueos de DLG. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T05 | Comprobar evidencias y registrar la decisión explícita de DLG; sin aceptación, el trabajo permanece abierto. | completada | Aprobado por DLG el 12-X-2026 («Confirmado»). |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02 | V02 | Correcto | 12-X-2026; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkTests" --verbosity minimal` | `Link.Create(long userId, Collection collection, ...)` rechaza una colección ajena sin mutar el estado. |
| CA02 | T02 | V02 | Correcto | 12-X-2026; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkTests" --verbosity minimal` | La creación con colección propia y URL válida se conserva con el mismo `UserId` y `CollectionId`. |
| CA03 | T03 | V03 | Correcto | 12-X-2026; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkTests" --verbosity minimal` | `MoveToCollection` acepta destinos propios y mantiene `CollectionId` cuando el destino es ajeno. |
| CA04 | T03 | V03 | Correcto | 12-X-2026; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkTests" --verbosity minimal` | El intento de mover a otra colección del mismo usuario falla sin cambiar la colección actual. |
| CA05 | T04 | V04 | Correcto | 12-X-2026; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal` | 540 pruebas correctas en el proyecto de tests; no hubo errores ni fallos de compilación. |

## Aceptación y cierre

- Estado del trabajo: aprobado por DLG el 12-X-2026 («Confirmado»).
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA05 tienen evidencia real en el proyecto de pruebas.
- Bloqueos y riesgos restantes: ninguno.
- Aceptación humana: aprobada por DLG el 12-X-2026 («Confirmado»).
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: archivar o cerrar la tarea solo si se conviene la documentación de cierre formal; la implementación queda aceptada por la evidencia técnica y la aprobación de DLG.