# Tareas: [Identificador y nombre del trabajo]

## Referencias y autorización

- Estado: pendiente / aprobado / bloqueado.
- Aprobaciones:
    - Especificación: `spec.md`, aprobada por [responsable, fecha]. Al generar las tareas en `specs/<slug>/`, enlazarla solo si existe en su ubicación final.
    - Plan: `plan.md`, aprobado por [responsable, fecha]. Enlazarlo solo si existe en su ubicación final.
	- Tareas: pendiente; registrar responsable, fecha y referencia real cuando se apruebe.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Estados: pendiente, en curso, completada o bloqueada. Conservar identificadores al actualizar. Enlazar al criterio y a la sección de diseño o validación correspondiente.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | [Preparación o cambio acotado] | [CA y enlace a diseño] | [ID o ninguna] | [Comprobación y resultado] | pendiente | No ejecutada |
| T02 | [Siguiente incremento] | [CA y enlace a diseño] | T01 | [Comprobación y resultado] | pendiente | No ejecutada |
| T03 | [Validación integrada] | [CA y enlace a validación] | T02 | [Comprobación y resultado] | pendiente | No ejecutada |
| T04 | [Revisión y aceptación humana] | [Criterios del trabajo] | T03 | [Revisión de evidencia y decisión registrada] | pendiente | Sin aceptación |

Adaptar el desglose al trabajo. Explicitar tareas independientes que puedan ejecutarse en paralelo. Registrar siempre evidencia obtenida en la comprobación actual, con fecha y límites; no inventar fechas, aprobaciones ni resultados históricos.

La tarea de aceptación solo se completa con aceptación humana explícita y registrada. Presentar evidencia o terminar una revisión sin aceptación no cierra el trabajo; si se rechaza, registrar el bloqueo y las correcciones necesarias.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | [ID] | [ID y enlace al plan] | Pendiente | No ejecutada | Sin evidencia |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: pendiente / bloqueado / aceptado / archivado. No confundirlo con los estados de cada tarea.
- Criterios sin evidencia satisfactoria:
- Bloqueos y riesgos restantes:
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: pendiente / no solicitado / completado (solo tras verificar traslado y enlaces).
- Ubicación archivada: pendiente / `specs/archive/<slug>/` (solo si existe y fue verificada).
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado, sin cerrar otras etapas: