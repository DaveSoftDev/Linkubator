# Tareas: S12.8 — Cierre del MVP0

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Secuencia revisada del plan: aprobada por DLG el 02-XI-2026 con la referencia «Aprobar el plan revisado».
  - Plan actualizado con `retrospectiva.md`: aprobado por DLG el 02-XI-2026 con la referencia «Aprobar esta revisión del plan».
  - Registro dedicado de retrospectiva: solicitado por DLG el 02-XI-2026 con la referencia «crear_archivo».
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
  - Secuencia revisada solicitada por DLG el 02-XI-2026 con la referencia «Sí, actualiza plan y tareas con esta secuencia».
  - Tareas revisadas: aprobadas por DLG el 02-XI-2026 con la referencia «Aprobar las tareas revisadas».
  - Tareas actualizadas con `retrospectiva.md`: aprobadas por DLG el 02-XI-2026 con la referencia «Aprobar las tareas actualizadas».
- Implementación:
  - Estado: en curso.
  - Inicio: 02-XI-2026, por solicitud de DLG en esta conversación: «Vamos comprobar que S12.8 está completado».
  - Finalización: pendiente.
  - Evidencia consolidada: pendiente.
- La preparación de estas tareas no inicia el cierre ni inventa aprobaciones o evidencias.

## Descomposición

Las tareas son secuenciales. T02 y T03 están completadas; T04–T07 siguen el orden revisado y aprobado el 02-XI-2026. La etiqueta requiere tanto un identificador acordado como el commit que contiene la aceptación previa del cierre.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras aprobar las tareas e iniciar S12.8, registrar la fecha y referencia reales en este resumen y actualizar la referencia de S12.8 en el roadmap. Reunir los enlaces a las evidencias anteriores necesarias para la revisión. | Preparación; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Comprobar que la aprobación de tareas y el inicio están registrados, y que las fuentes de evidencia están disponibles. Resultado: inicio trazable, sin afirmar que los prerrequisitos ya se han satisfecho. | completada | Inicio registrado el 02-XI-2026; tareas aprobadas por DLG el 31-X-2026; disponibles la [matriz checklist → test](../../plans/mvp0-checklist-test-matrix.md) y las tareas S12.1–S12.7 para contrastar sus evidencias. No implica que los prerrequisitos estén satisfechos. |
| T02 | Verificar los puntos previos de la Definition of Done de S12 y la evidencia de sus tareas responsables; registrar cualquier ausencia como bloqueo. | CA01; [plan.md → «Flujos internos»](plan.md#flujos-internos), V01 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Contrastar cada punto aplicable con su evidencia enlazada. Resultado: todos los prerrequisitos quedan acreditados o el cierre se detiene con los puntos pendientes identificados. | completada | DLG confirmó que todas las filas de checklist tienen test/revisión y evidencia, que no quedan hallazgos de auditoría sin disposición ni bloqueos del MVP0, y que accesibilidad/navegadores están registrados y aceptados con límites. Reejecuciones el 02-XI-2026: `LoggingTests|WebStartupTests|LoggingSinksIntegrationTests`, 150 passed/0 failed/0 skipped; `SqliteSearchProjectionRebuilderTests`, 6 passed/0 failed/0 skipped (cobertura del servicio, no de un comando inexistente); suite completa, 1.858 passed/0 failed/0 skipped. Para documentación↔código, DLG consideró suficiente la evidencia combinada de S12.3, la matriz y los tests («suficiente»), teniendo presente que S12.4 no audita código. Evidencias: [S12.2](../s12.2-eventos-y-logs/tasks.md), [matriz](../../plans/mvp0-checklist-test-matrix.md), [S12.3](../s12.3-auditoria-invariantes-criticas/tasks.md), [S12.4](../s12.4-revision-sincronizacion-documentacion/tasks.md), [S12.5](../s12.5-revision-accesibilidad/tasks.md), [S12.6](../s12.6-pruebas-manuales-navegadores/tasks.md) y [S12.7](../s12.7-revision-hallazgos-riesgos/tasks.md). |
| T03 | Actualizar el roadmap para reflejar el cierre del MVP0 y marcar como completado en `mvp0-plan.md` el punto correspondiente de la Definition of Done. | CA02; [plan.md → «Flujos internos»](plan.md#flujos-internos), V02 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T02 | Revisar «Hecho», «Now» y el último punto de la Definition of Done. Resultado: el MVP0 figura como terminado, S12 deja de ser la etapa actual y el punto queda marcado con evidencia, sin alterar comprobaciones anteriores. | completada | Actualizados el roadmap y la Definition of Done el 02-XI-2026. V02 comprobó: MVP0 figura en «Hecho», S12 ya no aparece como etapa actual, «Now» no declara una etapa activa y el punto final de la DoD está marcado; los siete puntos enlazan a la evidencia de S12.8. S12.8 permanece abierta por T04–T07. |
| T04 | Realizar la retrospectiva final y registrar su contenido en `retrospectiva.md`, enlazándolo desde la evidencia de cierre. | CA04; [plan.md → «Flujos internos»](plan.md#flujos-internos), V04 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T03 | Confirmar que el documento registra la retrospectiva y que `tasks.md` incluye la fecha y el enlace. Resultado: evidencia localizable, sin confundir la retrospectiva con la aceptación del cierre. | completada | DLG informó que la retrospectiva se realizó el 02-XI-2026; el contenido está en [retrospectiva.md](retrospectiva.md). |
| T05 | Presentar a DLG el contenido del cierre y registrar la aceptación previa para CA01, CA02 y CA04, dejando explícito que la etiqueta aún no existe. | CA01, CA02, CA04; [plan.md → «Flujos internos»](plan.md#flujos-internos) | T02, T03, T04 | Registrar una decisión humana explícita sobre el contenido aceptado antes de etiquetar. Resultado: aceptación previa trazable que no se presenta como cierre final; DLG deberá proporcionar el commit que la contiene para T06. | pendiente | Sin aceptación previa |
| T06 | Acordar con DLG el identificador de versión, crear la etiqueta sobre el commit que contiene la aceptación previa y verificar su destino. | CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), V03 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T05 | Comparar el commit resuelto por la etiqueta con el commit de cierre aceptado previamente. Resultado: ambos coinciden y quedan registrados el identificador y el commit; si falta el identificador o el commit, registrar bloqueo y no crear etiqueta. | pendiente | No ejecutada; identificador y commit con aceptación previa pendientes |
| T07 | Revisar la trazabilidad de CA01–CA04 y registrar la aceptación o el rechazo humano final del resultado. | CA01–CA04; V05 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T02, T03, T04, T05, T06 | Contrastar cada criterio con su evidencia y registrar la decisión explícita final de DLG. Resultado: aceptación con fecha y referencia reales o rechazo con los bloqueos y correcciones necesarios; no cerrar si falta evidencia. | pendiente | Sin aceptación final |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02 | V01 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfecho por revisión DLG: todos los prerrequisitos anteriores a S12.8 tienen evidencia; no se identificaron bloqueos abiertos. Los riesgos y límites aceptados siguen documentados y no se presentan como mitigados. Suite completa reejecutada: 1.858 passed, 0 failed, 0 skipped. | 02-XI-2026; revisión documental y ejecución de tests | [Evidencia T02](#descomposición), [matriz](../../plans/mvp0-checklist-test-matrix.md) y tareas S12.2–S12.7 enlazadas arriba |
| CA02 | T03 | V02 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfecho: roadmap y plan muestran el MVP0 en «Hecho», S12 fuera de «Now» y el punto final de la DoD completado con evidencia. | 02-XI-2026; revisión documental | [Evidencia T03](#descomposición), [roadmap.md](../../context/roadmap.md) y [mvp0-plan.md](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) |
| CA03 | T06 | V03 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA04 | T04 | V04 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfecho: retrospectiva realizada por DLG el 02-XI-2026 y contenido registrado en [retrospectiva.md](retrospectiva.md). | 02-XI-2026 | [retrospectiva.md](retrospectiva.md) |

Un resultado no disponible o fallido seguirá pendiente o bloqueado; no se registrarán resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: en curso; T01–T04 completadas y T05–T07 pendientes.
- Criterios sin evidencia satisfactoria: CA03, todavía no comprobado.
- Bloqueos y riesgos restantes: T06 requiere el identificador acordado y el commit que contiene la aceptación previa; el worktree actual tiene cambios sin commit.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Aceptación previa: pendiente; no equivale a la aceptación final de S12.8.
- Siguiente paso: aprobar por separado el plan y las tareas actualizadas para añadir `retrospectiva.md`; después, ejecutar T05.
