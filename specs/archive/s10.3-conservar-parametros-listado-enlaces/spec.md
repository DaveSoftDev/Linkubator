# Especificación: S10.3 — Conservar parámetros del listado de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 27-X-2026 por el asistente, a petición de DLG («Crea la spec S10.3»).
- Aprobación: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Objetivo

Completar la navegación del listado privado de enlaces para que la búsqueda y la paginación conserven los filtros aplicables y el formulario de búsqueda no envíe la página actual.

## Fuentes

- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): asignación de S10.3 y Definition of Done de S10.
- [AC-LINK-06 — Listar enlaces del usuario](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario): criterio aprobado propietario del listado, búsqueda, filtros y paginación.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados) y [«Rutas privadas»](../../../context/specifications.md#rutas-privadas): semántica de los filtros, orden y paginación del listado privado.
- [requirements.md → «Búsqueda y filtros»](../../../context/requirements.md#búsqueda-y-filtros): expectativa funcional de búsqueda y filtros en los listados.
- [S10.1 — Listado y búsqueda de enlaces](../s10.1-listado-enlaces/spec.md): listado base y comportamiento ya entregado.
- [S10.2 — Parámetros del listado de enlaces](../s10.2-parametros-listado-enlaces/spec.md): tratamiento ya entregado de parámetros no aplicables, filtros de colección no accesibles y búsqueda excesiva.

## Alcance

- **Incluye:** conservar los filtros aplicables al enviar una búsqueda desde el listado de enlaces; conservar la búsqueda y los filtros al cambiar de página; excluir la página actual del envío del formulario de búsqueda.
- **Excluye:** alterar las reglas de análisis o validación de `q`, `c` o `p`; cambiar la consulta, el orden, el tamaño de página o los resultados del listado; conectar el buscador del dashboard (S10.4); crear la suite transversal de tests HTTP y de datos (S10.5); realizar la revisión manual con datos de ejemplo (S10.6); cambiar reglas de producto o infraestructura.
- **Trabajo vecino y responsable de sus entregables:** S10.1 aporta el listado y sus controles base; S10.2 aporta el tratamiento de parámetros ya cubierto por su especificación. S10.4 es responsable de la integración del buscador del dashboard; S10.5, de la suite transversal; S10.6, de la revisión manual. Ninguno de estos trabajos futuros es prerrequisito para demostrar los criterios de esta spec.

## Dependencias y prerrequisitos

- S10.1 está completada y aceptada; el listado privado de enlaces y su búsqueda, filtros y paginación base están disponibles.
- S10.2 está completada y aceptada; el tratamiento de los parámetros existentes no se redefine en este trabajo.
- Las comprobaciones se limitan a la consulta generada por los controles de búsqueda y paginación y a su efecto observable en la navegación. No dependen de S10.4–S10.6.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas) | Un listado de enlaces está abierto con un filtro de colección aplicable y una página distinta de la primera. | El usuario envía una nueva búsqueda desde el formulario del listado. | La navegación conserva el filtro de colección aplicable, usa el texto de búsqueda enviado y no arrastra la página previa. La interpretación de valores y la búsqueda siguen las fuentes propietarias. |
| CA02 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados) | El listado de enlaces muestra resultados paginados con una búsqueda y, cuando corresponda, un filtro de colección aplicable. | El usuario navega a otra página del resultado. | El enlace de paginación conserva la búsqueda y el filtro de colección aplicable y solicita la página de destino; el listado resultante corresponde a esos mismos criterios. |
| CA03 | [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [«Listados»](../../../context/specifications.md#listados) | El formulario de búsqueda se presenta desde una página del listado distinta de la primera. | Se inspeccionan los datos enviados al enviar una nueva búsqueda. | El formulario no envía el valor de la página actual; la búsqueda comienza en la primera página según las reglas fuente. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar esta propuesta. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG el 27-X-2026 («Vamos») e implementadas y aceptadas por DLG el 27-X-2026 («Completado»); validación focalizada 40/40 y `git diff --check` satisfactorio.
