# Especificación: S12.1 — Matriz de trazabilidad de checklist y tests

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Dejar una matriz verificable que relacione cada punto de la checklist de calidad del MVP0 con sus tests automatizados o con la revisión manual que le corresponde, y cerrar los huecos de tests incluidos en esta tarea.

## Fuentes

- [roadmap.md → «Checklist de calidad del MVP0»](../../context/roadmap.md#checklist-de-calidad-del-mvp0)
- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [test-audit-plan.md → «Control de completitud»](../../plans/test-audit-plan.md#control-de-completitud), como antecedente de inventario y revisión; no sustituye la comprobación actual de la matriz.

## Alcance

- Incluye:
  - Desglosar la checklist hasta el nivel de escenarios comprobables y enlazar cada uno con su regla fuente.
  - Relacionar cada escenario con un test automatizado concreto o con una revisión manual registrada, diferenciando cobertura existente, pendiente y huecos.
  - Añadir o reforzar los tests automatizados que falten dentro del alcance de S12.1 y validar los cambios.
  - Registrar resultados y referencias de evidencia sin inferir cobertura por el nombre de un test o porque la suite pase.
- Excluye:
  - Ejecutar las revisiones manuales de accesibilidad y navegadores; la matriz las identifica y conserva como pendientes de sus tareas propietarias.
  - Los tests específicos de registro de eventos y ausencia de datos prohibidos en logs, cuya ejecución corresponde a S12.2.
  - La auditoría de invariantes críticas en código y SQL, la sincronización documental general, la aceptación de riesgos y el cierre del roadmap, asignados a otras tareas de S12.
  - Cambiar reglas funcionales o añadir comportamiento de producto. Si un test revela una discrepancia que requiere una decisión o un cambio fuera de esta cobertura, se registra y se detiene el trabajo afectado hasta su resolución.
- Trabajo vecino y responsable de sus entregables: S12.2 cubre los tests de eventos y logs; S12.3 la auditoría de invariantes críticas; S12.4 la sincronización documental; S12.5 y S12.6 las revisiones manuales; S12.7 y S12.8 la aceptación de riesgos y el cierre. La matriz registra sus comprobaciones sin depender de que estén ejecutadas para cerrar S12.1.

## Dependencias y prerrequisitos

- La fuente de alcance es la checklist aceptada del roadmap y su descomposición en el plan del MVP0.
- [test-audit-plan.md](../../plans/test-audit-plan.md) contiene inventarios y resultados previos. Antes de atribuir cobertura, se debe reconciliar esa evidencia con los tests y el código vigentes; los resultados históricos no acreditan por sí solos el estado actual.
- El [roadmap](../../context/roadmap.md#now) sitúa S12 como etapa actual tras completar S11a. La aprobación de esta spec no aprueba el plan ni las tareas, ni autoriza por sí sola el inicio de la implementación.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Checklist de calidad del MVP0](../../context/roadmap.md#checklist-de-calidad-del-mvp0) | La checklist y sus escenarios descritos | Se elabora la matriz | Cada escenario comprobable tiene una entrada propia, enlaza el punto de checklist y la regla fuente correspondiente, y no quedan puntos ni subescenarios sin clasificar. |
| CA02 | [Checklist de calidad del MVP0](../../context/roadmap.md#checklist-de-calidad-del-mvp0), [control de completitud de la auditoría](../../plans/test-audit-plan.md#control-de-completitud) | Un escenario se declara cubierto automáticamente | Se contrasta la matriz con el test existente y su resultado de ejecución | La entrada identifica el caso de test concreto y su aserción relevante; el test demuestra el escenario atribuido y la evidencia indica la validación realizada. Un nombre, una clase o un resultado global verde por sí solos no cuentan como cobertura. |
| CA03 | [Checklist de calidad del MVP0](../../context/roadmap.md#checklist-de-calidad-del-mvp0), [S12](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | Un escenario está asignado a revisión manual o a otra tarea de S12 | Se completa la matriz | La entrada identifica el tipo de comprobación, su responsable o tarea propietaria y su estado real; una comprobación pendiente no se presenta como ejecutada ni bloquea el cierre de S12.1. |
| CA04 | [Checklist de calidad del MVP0](../../context/roadmap.md#checklist-de-calidad-del-mvp0) | La reconciliación identifica un hueco de cobertura automatizada dentro de S12.1 | Se implementa y valida la cobertura aprobada para ese hueco | Se añade o refuerza el test pertinente, la validación focalizada y la regresión aplicable pasan, y la matriz enlaza el resultado con evidencia actual. Las discrepancias que exijan cambiar una regla quedan registradas y bloqueadas, no se resuelven cambiando la fuente por iniciativa propia. |
| CA05 | [S12 — Definition of Done](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | Se termina la reconciliación de la checklist | Se revisa la matriz para cerrar S12.1 | Todos los puntos tienen una ruta de comprobación y un estado explícito; no quedan huecos automatizados asignados a S12.1 ni resultados o aprobaciones sin evidencia. Las revisiones manuales y tareas vecinas pueden seguir pendientes. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna pregunta de alcance identificada en las fuentes disponibles. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
- [Tareas](tasks.md) — pendientes de aprobación.
