# Tareas: S7.10 — Revisión manual del dashboard

## Referencias y autorización

- Estado: pendiente de aprobación.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 24-X-2026 («Vamos»).
  - Tareas: pendiente; registrar responsable, fecha y referencia real cuando se apruebe.
- Implementación:
  - Estado: no iniciada.
  - Inicio: pendiente; registrar fecha y referencia real al iniciarla.
  - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar la revisión y consolidar su evidencia.
  - Evidencia consolidada: pendiente.
- La creación de estas tareas no registra la ejecución de la revisión ni aceptación del resultado.

## Descomposición

Las revisiones manuales de teclado/foco, contraste y encabezados son comprobaciones independientes sobre la misma página disponible; la consolidación depende de las tres. El alcance no incluye arreglos del producto ni una auditoría global de la accesibilidad.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Preparar el entorno manual: confirmar host, acceso al dashboard, navegador y herramientas disponibles; registrar versión y contexto de la página. | CA01–CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V01–V04. | Ninguna | Abrir el dashboard disponible y confirmar que puede inspeccionarse sin modificar el producto; registrar navegador/versión, entorno y cualquier limitación. | pendiente | No ejecutada. |
| T02 | Recorrer el dashboard mediante teclado y documentar tabulación y foco visible de los controles disponibles. | CA01; [plan.md → «Flujos internos»](plan.md#flujos-internos), V01. | T01 | Completar el recorrido sin ratón, comparar orden de tabulación con presentación y documentar foco y hallazgos reproducibles. | pendiente | No ejecutada. |
| T03 | Inspeccionar y medir el contraste de las combinaciones visibles aplicables al dashboard. | CA02; [plan.md → «Flujos internos»](plan.md#flujos-internos), V02. | T01 | Registrar cada combinación revisada, colores observados, método/herramienta y resultado respecto de la fuente; anotar incumplimientos con contexto. | pendiente | No ejecutada. |
| T04 | Revisar la jerarquía semántica y el orden de encabezados del dashboard renderizado. | CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), V03. | T01 | Inspeccionar el documento renderizado y comparar estructura semántica con las secciones visibles; registrar niveles y hallazgos reproducibles. | pendiente | No ejecutada. |
| T05 | Consolidar la evidencia CA01–CA03, describir límites/hallazgos y solicitar o registrar la decisión humana. | CA01–CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V04. | T02, T03, T04 | Verificar que cada criterio tiene evidencia actual, contexto y resultado; registrar aceptación o solicitud de ajustes sin afirmar cambios no realizados. | pendiente | Sin aceptación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T02, T05 | V01 y V04 de [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA02 | T01, T03, T05 | V02 y V04 de [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA03 | T01, T04–T05 | V03 y V04 de [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |
| CA04 | T01–T05 | V04 de [plan.md](plan.md#estrategia-de-validación) | Pendiente | No ejecutada | Sin evidencia |

## Aceptación y cierre

- Estado del trabajo: pendiente.
- Criterios sin evidencia satisfactoria: CA01, CA02, CA03, CA04.
- Bloqueos y riesgos restantes: revisión manual no iniciada; entorno, navegador y herramienta de contraste se registrarán en T01.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: obtener aprobación explícita de estas tareas; luego iniciar la revisión registrando fecha y referencia real.
