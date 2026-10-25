# Tareas: S8.5 — Pruebas de integración HTTP de colecciones

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
  - Tareas: DLG, 25-X-2026 («Vamos»).
- Implementación:
  - Estado: no iniciada.
  - Inicio: pendiente; registrar fecha y referencia real al iniciarla.
  - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
  - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustentan el estado.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Las pruebas pueden repartirse por comportamientos HTTP independientes, pero todas dependen de completar primero el inventario de cobertura existente y confirmar el harness disponible. CA01–CA05 se implementan en tareas de tests por responsabilidad; la validación integrada espera a todos esos bloques. S8.5 solo añade o ajusta pruebas automatizadas y su trazabilidad documental; no modifica comportamiento productivo.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inventariar las pruebas HTTP de colecciones ya presentes y mapearlas a CA01–CA05; identificar casos faltantes, fixture, esquema SQLite y filtro de ejecución. | CA01–CA05; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V01–V05](plan.md#estrategia-de-validación). | ninguna | Revisar los tests y ejecutar un descubrimiento o filtro focalizado disponible; entregar matriz CA → test existente/faltante y comando comprobado. | pendiente | No ejecutada |
| T02 | Completar o ajustar tests del listado para búsqueda, paginación, estado vacío, parámetros aplicables/no aplicables y aislamiento entre usuarios sin duplicar casos ya cubiertos. | CA01; [plan.md → «Flujos internos»](plan.md#flujos-internos), [V01](plan.md#estrategia-de-validación). | T01 | Ejecutar los casos de listado afectados; pasan y demuestran mediante respuesta que solo se listan datos del usuario identificado y que los parámetros se procesan según las fuentes. | pendiente | No ejecutada |
| T03 | Completar tests HTTP de creación y edición, incluidos el resultado persistido de operaciones válidas, la protección antiforgery y rechazos relevantes de validación/dominio. | CA02, CA05; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [V02 y V05](plan.md#estrategia-de-validación). | T01 | Ejecutar los tests focalizados de alta/edición; las respuestas corresponden a los contratos fuente y los datos se crean o permanecen sin cambios según corresponda. | pendiente | No ejecutada |
| T04 | Completar tests HTTP de eliminación, incluidos GET no mutante, POST protegido, resultado persistido y respuesta ante reglas de rechazo. | CA03, CA05; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [V03 y V05](plan.md#estrategia-de-validación). | T01 | Ejecutar los tests focalizados de eliminación; GET no modifica datos y el POST los elimina solo cuando el resultado propietario lo permite. | pendiente | No ejecutada |
| T05 | Completar pruebas de identificadores inválidos y recursos válidos inexistentes/ajenos en las rutas cubiertas, contrastando respuestas y estado de ambos usuarios. | CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [V04](plan.md#estrategia-de-validación). | T01 | Ejecutar tests focalizados; el inválido conserva su tratamiento y la inaccesibilidad no distingue inexistente de ajeno ni altera datos de la otra persona. | pendiente | No ejecutada |
| T06 | Ejecutar una comprobación negativa reversible sobre una aserción representativa de antiforgery o aislamiento; restaurar la aserción original y ejecutar el test normal. | CA06; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [V06](plan.md#estrategia-de-validación). | T02, T03, T04, T05 | La perturbación produce un fallo de aserción funcional del test objetivo; tras restaurarla, el test pasa. Dejar el archivo sin la perturbación. | pendiente | No ejecutada |
| T07 | Ejecutar suite de colecciones focalizada, `git diff --check`, y revisar cobertura CA → tests → evidencia; sincronizar el estado real del trabajo en esta tabla y en el roadmap si cambia su avance. | CA01–CA06; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [V07](plan.md#estrategia-de-validación). | T02, T03, T04, T05, T06 | El filtro focalizado pasa, diff check queda limpio y todos los criterios tienen evidencia satisfactoria o bloqueo explícito; no se modificó código productivo. | pendiente | No ejecutada |
| T08 | Revisar la evidencia consolidada, solicitar y registrar aceptación humana explícita; actualizar el estado de cierre y referencias documentales cuando exista la decisión. | CA01–CA06; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T07 | La aceptación se registra con responsable, fecha y referencia real; sin ella, el estado de trabajo sigue pendiente y no se declara cerrado. | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T07 | V01 y V07 en [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA02 | T01, T03, T07 | V02 y V07 en [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA03 | T01, T04, T07 | V03 y V07 en [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA04 | T01, T05, T07 | V04 y V07 en [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA05 | T01, T03, T04, T07 | V05 y V07 en [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA06 | T06, T07 | V06 y V07 en [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |

## Aceptación y cierre

- Estado del trabajo: pendiente.
- Criterios sin evidencia satisfactoria: CA01, CA02, CA03, CA04, CA05 y CA06.
- Bloqueos y riesgos restantes: ninguno identificado al preparar estas tareas; cualquier bloqueo técnico se registrará al aparecer.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: iniciar la implementación de pruebas y validación conforme a la especificación, plan y tareas aprobados.
