# Especificación: S10.4 — Conectar el buscador del dashboard

## Estado y aprobación

- Estado: aprobada.
- Preparada el 27-X-2026 por el asistente, a petición de DLG («Crea la spec S10.4»).
- Aprobación: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Objetivo

Conectar el formulario de búsqueda del dashboard con el listado privado de enlaces para que una búsqueda iniciada desde allí llegue al listado con el texto enviado, sin añadir resultados de búsqueda al dashboard ni alterar el comportamiento propio del listado.

## Fuentes

- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): asignación de S10.4 y Definition of Done de S10.
- [AC-DASH-01 — Dashboard](../../plans/mvp0-acceptance-criteria.md#ac-dash-01-dashboard): criterio aprobado propietario del formulario y sus límites.
- [AC-LINK-06 — Listar enlaces del usuario](../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario): criterio aprobado propietario del destino de búsqueda.
- [requirements.md → «Páginas privadas»](../../context/requirements.md#páginas-privadas) y [«Búsqueda y filtros»](../../context/requirements.md#búsqueda-y-filtros): expectativa funcional del dashboard y la búsqueda.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../context/specifications.md#listados): semántica de la navegación, búsqueda y listado destino.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos): aislamiento de los datos privados por usuario.
- [S7.7 — Dashboard y respuestas privadas](../s7.7-dashboard-y-respuestas-privadas/spec.md): dashboard y formulario base ya entregados.
- [S10.1 — Listado y búsqueda de enlaces](../s10.1-listado-enlaces/spec.md), [S10.2 — Parámetros del listado de enlaces](../s10.2-parametros-listado-enlaces/spec.md) y [S10.3 — Conservar parámetros del listado de enlaces](../s10.3-conservar-parametros-listado-enlaces/spec.md): comportamiento del listado destino y su navegación, ya entregados.

## Alcance

- **Incluye:** verificar y, si hace falta, completar el enlace funcional entre el formulario de búsqueda del dashboard y el listado privado de enlaces; comprobar que el término enviado se procesa en el listado según las fuentes propietarias.
- **Excluye:** mostrar resultados de búsqueda en el dashboard; cambiar el formulario o los criterios de búsqueda del listado, sus parámetros, validación, orden, filtros o paginación; añadir búsqueda en otras entidades; ampliar las capacidades disponibles para etiquetas; la suite transversal de tests HTTP y de datos de S10.5; la revisión manual con datos de ejemplo de S10.6; cambios en reglas de producto, infraestructura o despliegue.
- **Trabajo vecino y responsable de sus entregables:** S7.7 aporta el dashboard y el formulario base; S10.1–S10.3 aportan el listado destino y sus parámetros/navegación. S10.5 es responsable de la suite transversal y S10.6 de la revisión manual con datos de ejemplo. Estos trabajos posteriores no son prerrequisito para cerrar esta especificación.

## Dependencias y prerrequisitos

- S7.7 está completada y aceptada; el dashboard y su formulario de búsqueda están disponibles.
- S10.1, S10.2 y S10.3 están completadas y aceptadas; el listado destino procesa la búsqueda según su comportamiento ya establecido.
- No se requiere que S10.5 o S10.6 estén iniciadas o completadas: la conexión puede verificarse dentro de esta especificación mediante una comprobación focalizada del flujo dashboard → listado.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-DASH-01](../../plans/mvp0-acceptance-criteria.md#ac-dash-01-dashboard), [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Un usuario identificado accede al dashboard, que presenta su formulario de búsqueda. | Envía un texto de búsqueda. | La navegación es una petición GET al listado privado de enlaces con el texto enviado en `q`; el dashboard no presenta resultados de búsqueda ni ejecuta el filtrado en su propia página. |
| CA02 | [AC-LINK-06](../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | El usuario llega al listado mediante la búsqueda iniciada desde el dashboard. | Se procesa la consulta en la ruta destino. | El listado presenta los resultados y el estado de búsqueda de acuerdo con sus reglas propietarias y únicamente con datos del usuario identificado; la conexión no altera el comportamiento general del listado. |
| CA03 | [AC-DASH-01](../../plans/mvp0-acceptance-criteria.md#ac-dash-01-dashboard), [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | El dashboard está renderizado y el formulario se inspecciona antes de enviarlo. | Se comprueba el destino, método y campos que envía. | El formulario identifica accesiblemente su propósito y envía únicamente la consulta funcional del buscador al destino establecido, sin convertir el dashboard en un listado ni incluir estado de paginación del listado destino. |

Los valores vacíos, límites, errores, filtros, orden, paginación y demás casos negativos aplicables al listado permanecen bajo sus fuentes propietarias y la cobertura de S10.1–S10.3/S10.5; esta especificación no los redefine.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para preparar esta propuesta. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG el 27-X-2026 («Vamos»); implementación técnica completada y resultado aceptado por DLG el 27-X-2026 («Completado»), con validación focalizada 42/42 y `git diff --check` satisfactorio.
