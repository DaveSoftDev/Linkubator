# Tareas: S4.9 — Fakes en memoria y pruebas de casos de uso

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Plan: [plan.md](plan.md), aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Tareas: aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: no iniciada; las tareas están aprobadas, pero no se ha recibido una instrucción explícita para iniciar la implementación.
    - Inicio: pendiente; registrar fecha y referencia real al iniciarla.
    - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
    - Evidencia consolidada: pendiente.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 debe completar la matriz por cada criterio aprobado y subcriterio; solo después se separará lo ya cubierto de las brechas que requieren tests nuevos. T03–T05 son trabajos de pruebas/fakes por responsabilidad y podrían ejecutarse en paralelo una vez aprobada T02, pero cualquier extracción compartida debe coordinarse para no editar el mismo soporte en paralelo. T06 integra la cobertura, valida dependencias y registra evidencia; T07 sincroniza el roadmap y presenta el resultado para aceptación humana. Las tareas están aprobadas; la implementación queda no iniciada hasta que DLG indique explícitamente su inicio.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar esta descomposición, comprobando que cada criterio CA01–CA06 tiene tarea, validación y dependencia, y que la aceptación de tareas precede a cualquier cambio productivo. | CA01–CA06; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Revisar trazabilidad CA → tareas → V01–V06 y restricciones; resultado esperado: aprobación explícita separada antes de iniciar trabajo. | completada | Aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación. |
| T02 | Inventariar, para AC-COL, AC-LINK, AC-DASH, AC-USER, AC-COLLS, AC-COLLECTION, AC-TRANS-01 y AC-TRANS-02 aplicables, las pruebas existentes, sus nombres y brechas; entregar matriz criterio → test(s) → evidencia pendiente. | CA01–CA06; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), V01 | T01 | Inspeccionar `CollectionServiceTests`, `LinkServiceTests`, servicios S4.6–S4.8 y tests existentes; resultado esperado: cada criterio aplicable y los subcriterios AC-COL-01b/AC-LINK-01b quedan identificados, y se separan obligaciones de Application de persistencia/Web/S4.10. | pendiente | No ejecutada. |
| T03 | Completar o reorganizar los fakes/repositorios en memoria de colecciones y las pruebas necesarias para cubrir las brechas de criterios que determine T02, manteniendo los casos existentes. | CA01, CA04–CA06; [plan → «Fakes compartidos» y «Pruebas de colecciones y enlaces»](plan.md#componentes-y-contratos), V02 | T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~CollectionServiceTests"`; resultado esperado: todos los tests seleccionados pasan y la matriz muestra la cobertura real de criterios de Application. | pendiente | No ejecutada. |
| T04 | Completar o reorganizar los fakes/repositorios en memoria de enlaces y las pruebas necesarias para cubrir las brechas de criterios que determine T02, manteniendo los casos existentes. | CA02, CA04–CA06; [plan → «Fakes compartidos» y «Pruebas de colecciones y enlaces»](plan.md#componentes-y-contratos), V03 | T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~LinkServiceTests"`; resultado esperado: todos los tests seleccionados pasan y la matriz muestra la cobertura real de criterios de Application. | pendiente | No ejecutada. |
| T05 | Añadir pruebas de `DashboardService` y `PublicReadService` con dobles de identidad y repositorios públicos, cubriendo las ramas de Application requeridas por la matriz T02. | CA03–CA06; [plan → «Dashboard y lecturas públicas»](plan.md#componentes-y-contratos), V04 | T02 | Ejecutar las clases añadidas con sus filtros FullyQualifiedName reales; resultado esperado: se comprueban delegación, resultados de éxito/error y entradas inválidas según las responsabilidades de Application, sin atribuir consulta real ni comportamiento HTTP. | pendiente | No ejecutada. |
| T06 | Integrar la matriz final, revisar que todos los criterios aplicables tengan test ejecutable, que los fakes permanezcan en Tests y que no haya dependencias productivas; ejecutar suite, build y pruebas de arquitectura disponibles. | CA01–CA06; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), V01, V05–V06 | T03, T04, T05 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal`, `dotnet build Linkubator.sln --no-restore --verbosity minimal`, el filtro de `ProjectReferenceMatrixTests` y `git diff --check`; resultado esperado: validaciones pasan, se registra SDK/resultados reales y no se afirma cobertura ajena al alcance. | pendiente | No ejecutada. |
| T07 | Sincronizar el estado y la evidencia de S4.9 en este archivo y el roadmap, y presentar el resultado a DLG para aceptación humana separada. | CA01–CA06; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Revisar matriz, comprobaciones y enlaces; resultado esperado: aceptación pendiente hasta que DLG la registre explícitamente, sin declarar implementación aceptada por tareas completadas. | pendiente | No ejecutada; implementación no iniciada. |

Las tareas T03 y T04 pueden avanzar en paralelo tras T02 si no modifican los mismos fakes compartidos; cualquier extracción común se acuerda primero y se ejecuta en un único hilo de cambios. T05 también depende solo de la matriz, pero no debe introducir producción ni atribuir responsabilidades de persistencia o Web.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T03, T06 | V01–V02, V05–V06 | Pendiente | No ejecutada | Sin evidencia. |
| CA02 | T02, T04, T06 | V01, V03, V05–V06 | Pendiente | No ejecutada | Sin evidencia. |
| CA03 | T02, T05–T06 | V01, V04–V06 | Pendiente | No ejecutada | Sin evidencia. |
| CA04 | T02–T06 | V01–V06 | Pendiente | No ejecutada | Sin evidencia. |
| CA05 | T02–T06 | V01–V06 | Pendiente | No ejecutada | Sin evidencia. |
| CA06 | T02–T07 | V01, V05–V06 | Pendiente | No ejecutada | Sin evidencia. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales. Los tests con fakes no acreditan transacciones, consultas ni garantías de etapas externas.

## Aceptación y cierre

- Estado del trabajo: pendiente.
- Criterios sin evidencia satisfactoria: CA01–CA06.
- Bloqueos y riesgos restantes: implementación no iniciada; pendiente instrucción explícita de inicio. Persistencia, Web y registro de eventos permanecen fuera de esta entrega.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado: iniciar T02, inventario de cobertura, tras instrucción explícita de inicio; ninguna tarea productiva se ha comenzado.
