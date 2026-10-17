# Tareas: S4.8 — Dashboard y lecturas públicas

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Plan: [plan.md](plan.md), aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Tareas: aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: no iniciada.
    - Inicio: pendiente; registrar fecha y referencia real al iniciarla.
    - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
    - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustenten el estado.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 verifica la suficiencia de los contratos y el encaje de resultados anulables antes de fijar tareas de implementación. Si descubre una carencia que exige cambiar un contrato o introducir una regla, la tarea afectada se bloquea y solicita una decisión antes de editar código. T03 y T04 son independientes una vez resuelta T02; ambas pueden avanzar en paralelo, al igual que sus validaciones específicas posteriores. Los fakes y tests de comportamiento corresponden a S4.9, no a este trabajo.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas, confirmando cobertura CA → tarea → validación y la separación respecto a S4.9/S4.10 y a las etapas Web/persistencia. | CA01–CA06; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Comprobar trazabilidad, dependencias, decisiones pendientes y límites; resultado esperado: aprobación explícita de tareas antes de cambios productivos. | completada | Aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación. |
| T02 | Inspeccionar contratos, DTOs y servicios aceptados S4.6/S4.7 para confirmar que expresan los criterios de dashboard y lecturas públicas; registrar suficiencia o bloqueo antes de implementar. | CA01–CA06; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), V01 | T01 | Revisar `IDashboardRepository`, `IPublicReadRepository`, `ICurrentUser`, DTOs públicos, errores y precedentes de servicios; resultado esperado: mapa contrato/servicio → criterio, sin cambios de contrato no aprobados. | pendiente | No ejecutada. |
| T03 | Implementar el caso de uso privado de dashboard conforme al contrato y al usuario identificado, con interfaz y tipos públicos organizados según el diseño aprobado. | CA01, CA05–CA06; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T02 | Compilar Application y revisar que una identidad no autenticada no consulta el repositorio y que la identidad consultada proviene de `ICurrentUser`; resultado esperado: flujo limitado al usuario identificado y sin dependencia Web/Infrastructure. | pendiente | No ejecutada. |
| T04 | Implementar los casos de uso de lectura pública para usuario, colecciones y colección, usando los contratos aprobados y preservando la separación entre ausencia y colección vacía donde las fuentes la requieren. | CA02–CA06; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T02 | Compilar Application y revisar cada método contra AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01; resultado esperado: no autenticación obligatoria, ningún dato privado expuesto y ausencia modelada según el contrato sin cambiar reglas. | pendiente | No ejecutada. |
| T05 | Revisar la implementación integrada de dashboard y lecturas frente a criterios, privacidad, errores, organización de tipos y dependencias. | CA01–CA06; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), V01–V02 | T03, T04 | Ejecutar `ProjectReferenceMatrixTests` y revisar criterio → servicio → contrato/DTO; resultado esperado: ninguna dependencia prohibida ni resultado que revele datos privados o causas no autorizadas. | pendiente | No ejecutada. |
| T06 | Validar build, suite disponible y diff; registrar qué pruebas de comportamiento quedan asignadas a S4.9. | CA01–CA06; [plan → «Estrategia de validación»](plan.md#estrategia-de-validación), V03–V05 | T05 | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal`, `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `git diff --check`; resultado esperado: build/suite disponibles correctos y evidencia de S4.9 explícitamente pendiente si aún no existe. | pendiente | No ejecutada. |
| T07 | Actualizar el estado de implementación en este documento y sincronizar el roadmap cuando comience o termine el trabajo; al final presentar evidencias a DLG para aceptación separada. | CA01–CA06; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05, T06 | Verificar que roadmap, estado de implementación y evidencias reflejan el estado real; resultado esperado: aceptación humana registrada por separado o bloqueo explícito, sin cerrar trabajo por build/tests verdes. | pendiente | No ejecutada. |

No se inicia ninguna tarea productiva hasta la aprobación explícita de T01. Si T02 encuentra que los contratos no expresan un resultado requerido, se bloquean las tareas dependientes de ese criterio y se requiere resolución aprobada antes de cambiar contratos o implementación. La tarea T07 puede registrar sincronización de estado durante el flujo, no solo al cierre.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T03, T05–T07 | V01–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA02 | T02, T04–T07 | V01–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA03 | T02, T04–T07 | V01–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA04 | T02, T04–T07 | V01–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA05 | T02–T07 | V01–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA06 | T02–T07 | V01–V05 | Pendiente | No ejecutada | Sin evidencia |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales. La validación de consumidor S4.9, la persistencia S5/S6 y las respuestas/renderizado Web se registrarán solo en sus etapas propietarias.

## Aceptación y cierre

- Estado del trabajo: aprobado para iniciar implementación; implementación no iniciada.
- Criterios sin evidencia satisfactoria: CA01–CA06.
- Bloqueos y riesgos restantes: suficiencia funcional de contratos por confirmar en T02; evidencia conductual de fakes corresponde a S4.9.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado: iniciar T02 y confirmar la suficiencia de los contratos antes de implementar T03–T04. La aprobación de tareas no equivale a inicio de implementación; registrar la referencia explícita de inicio cuando ocurra.
