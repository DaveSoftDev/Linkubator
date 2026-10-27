# Especificación: S10.2 — Parámetros del listado de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 27-X-2026 por el asistente, a petición de DLG («Crea la spec S10.2»).
- Aprobación: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Objetivo

Completar el tratamiento Web de los parámetros del listado privado de enlaces para que los parámetros de capacidades no disponibles se ignoren, los filtros de colección no aplicables se descarten y una búsqueda que exceda el límite fuente produzca el error accesible previsto.

## Fuentes

- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): asignación de S10.2 y Definition of Done de S10.
- [AC-LINK-06 — Listar enlaces del usuario](../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario): criterio aprobado propietario del listado y sus parámetros.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas): parámetros aplicables, comportamiento de filtros inválidos/no accesibles, texto de búsqueda y respuesta de error.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): límite de `q`.
- [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros): significado de la búsqueda y del filtro por colección.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos): aislamiento por usuario.
- [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad): presentación accesible de errores.
- [S7.6 — Análisis de parámetros de listados](../s7.6-analisis-parametros-listados/spec.md): base de análisis de parámetros de la frontera Web, cuya funcionalidad de `e` quedó expresamente fuera de alcance.
- [S10.1 — Listado y búsqueda de enlaces](../s10.1-listado-enlaces/spec.md): listado, búsqueda y filtro base ya completados.

## Alcance

- **Incluye:** ignorar `e` en el listado de enlaces sin validarlo ni ejecutar un filtro; ignorar un filtro `c` válido que identifica una colección inexistente o ajena al usuario identificado; rechazar `q` que, tras el recorte indicado por la fuente, excede su longitud máxima, presentando el error accesible previsto y sin ejecutar la búsqueda con ese valor.
- **Excluye:** implementar filtros de etiquetas o cualquier capacidad de gestión de etiquetas; nuevas reglas para `p` o `c` malformados/fuera de rango; conservación de filtros entre el formulario de búsqueda y la paginación (S10.3); búsqueda desde el dashboard (S10.4); suite transversal de tests HTTP y de datos (S10.5); revisión manual de la búsqueda con datos de ejemplo (S10.6); cambios en las reglas de producto o en infraestructura.
- **Trabajo vecino y responsable de sus entregables:** S10.1 aporta el listado base. S10.3 gestiona la conservación de parámetros de navegación; S10.5, la suite transversal derivada de AC-LINK-06; S10.6, la revisión manual con datos de ejemplo. La evidencia de esta spec se limita a sus criterios y no depende de esos trabajos futuros.

## Dependencias y prerrequisitos

- S10.1 está completada y aceptada; el listado privado de enlaces y su búsqueda/filtro base están disponibles.
- S7.6 aporta el analizador técnico de parámetros de listados, pero su aceptación funcional no se presupone ni es prerrequisito: los criterios de esta spec se validan desde el flujo propio de S10.2.
- La decisión de alcance que mantiene S10.2 y excluye el filtro de etiquetas de MVP0 fue confirmada por DLG el 27-X-2026. En consecuencia, se corrigieron la regla propietaria de parámetros en [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y la justificación correspondiente en [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance). Los parámetros de listados ahora incluyen `p`, `c` y `q`; `e` se trata como parámetro no aplicable según la regla general de esa sección.
- No se requiere S10.3, S10.5 ni S10.6 para implementar o demostrar los criterios de esta especificación.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Una petición al listado de enlaces incluye el parámetro `e`, con cualquier contenido. | Web interpreta la consulta del listado. | `e` se ignora por completo: no se valida, no produce error ni altera los resultados mediante un filtro de etiquetas. Los demás parámetros aplicables conservan su comportamiento fuente. |
| CA02 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | La petición incluye un filtro `c` con identificador válido que no corresponde a una colección del usuario identificado, bien porque no existe o porque pertenece a otro usuario. | Se resuelve el filtro y se presenta el listado. | El filtro se ignora sin revelar cuál de las condiciones se da, y se conservan los demás filtros válidos; los resultados no incluyen datos de otro usuario. |
| CA03 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas), [«Accesibilidad»](../../context/requirements.md#accesibilidad) | El listado recibe un valor `q` cuyo texto, tras el recorte de espacios extremos prescrito por la fuente, supera el máximo permitido. | Web valida la consulta antes de ejecutar la búsqueda. | Se muestra el error accesible previsto por la fuente y no se ejecuta una búsqueda con el valor excedido. |

Los criterios cubren los casos negativos y límites propios de S10.2. El análisis de valores malformados o fuera de rango de `p` y `c` pertenece a la regla fuente, pero su implementación y prueba quedan fuera de esta especificación; no se redefinen aquí.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado tras la decisión explícita de mantener S10.2 y excluir el filtro de etiquetas. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas, evidencia y aceptación](tasks.md#aceptación-y-cierre), aprobadas por DLG el 27-X-2026 («Vamos»); implementación y validación técnica completadas y resultado aceptado por DLG el 27-X-2026 («Completado»).
