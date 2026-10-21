# Especificación: S6.9 — Validación manual de búsqueda y reconstrucción FTS5

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Apruebo la spec y preparar el plan»), decisión explícita mediante formulario en esta conversación.
- Revisión: 20-X-2026.

## Objetivo

Comprobar manualmente que las búsquedas FTS5 pueden recorrerse con los datos de ejemplo locales preparados y que la herramienta de reconstrucción repara las proyecciones alteradas sin perder los resultados derivados de las entidades.

## Fuentes

- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): encargo manual de S6.9 y definición de finalización de S6.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros): semántica observable para búsquedas con términos, acentos y operadores literales.
- [specifications.md → «Listados»](../../../context/specifications.md#listados): filtros, orden y paginación.
- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): proyecciones por entidad, aislamiento y reconstrucción manual.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): límites transaccionales de persistencia.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos): lectura privada limitada al usuario identificado.
- [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md): consultas persistentes que se comprueban.
- [S6.6 — Reconstrucción manual de proyecciones FTS5](../s6.6-reconstruccion-proyecciones/spec.md): operación de reconstrucción disponible.
- [S6.7 — Pruebas integradas de búsqueda FTS5](../s6.7-pruebas-integradas-fts5/spec.md): validación automatizada ya cerrada, distinta de esta comprobación manual.
- [S6.8 — Datos locales de ejemplo para búsqueda](../s6.8-datos-local-busqueda/spec.md): conjunto de datos local y repetible que se consume.

## Alcance

- Incluye:
  - Usar la base local de ejemplo ya preparada para realizar manualmente búsquedas diferenciadas de enlaces y colecciones.
  - Comprobar los escenarios de búsqueda asignados al conjunto de ejemplo: acentos/formas Unicode equivalentes, varios términos obligatorios, operadores literales, filtro por colección y resultados paginados.
  - Alterar de forma controlada las proyecciones de enlaces y colecciones de la base local, ejecutar la herramienta existente de reconstrucción y comprobar que los datos de entidad vuelven a ser buscables.
  - Registrar los pasos realmente ejecutados, resultados observados y cualquier limitación del recorrido.
- Excluye:
  - Cambiar código productivo, datos de ejemplo, consultas, reglas funcionales, esquema o herramienta de reconstrucción.
  - Probar comportamiento HTTP, páginas Razor, formularios o parámetros Web; no es parte de S6.9.
  - Repetir la matriz automatizada de integración ya cubierta por S6.7.
  - Ejecutar la comprobación de búsqueda de enlaces asignada a S10.6; es trabajo independiente y no condiciona el cierre de S6.9.
  - Probar proyecciones de etiquetas, fuera del alcance de búsqueda actual.
- Trabajo vecino y responsable de sus entregables:
  - S6.8 proporciona la base local con los datos de ejemplo; S6.9 solo la consume.
  - S6.5 proporciona las consultas de persistencia; S6.6 proporciona la utilidad de reconstrucción; S6.7 aporta las pruebas automatizadas.
  - S10.6 realiza una comprobación manual propia de búsqueda de enlaces cuando la etapa de listado esté disponible, sin ser prerrequisito ni evidencia de cierre de S6.9.

## Dependencias y prerrequisitos

- S6.5, S6.6, S6.7 y S6.8 están implementadas y aceptadas según sus artefactos y el roadmap.
- Existe la base local `data/database/Linkubator-S6.8-20261020.db`, ignorada por Git, y contiene los datos sintéticos y proyecciones de S6.8. Antes de modificar una proyección, se debe conservar una copia recuperable de la base para no perder los datos locales.
- La interfaz Web de listados no forma parte de S6.9; esta comprobación se realiza directamente sobre SQLite/FTS5 y se cierra sin depender de trabajo posterior.
- La comprobación no depende de S10.6 ni de su implementación futura.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.5](../s6.5-consultas-busqueda-fts5/spec.md), [S6.8](../s6.8-datos-local-busqueda/spec.md) | La base local contiene ejemplos para ambos tipos de entidad. | Se ejecutan consultas de búsqueda separadas para enlaces y colecciones, incluyendo un término que tenga controles de coincidencia en ambos tipos. | Cada consulta devuelve solo el tipo de entidad consultado y los resultados observados concuerdan con los campos buscables de la fuente. |
| CA02 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [specifications.md → «Listados»](../../../context/specifications.md#listados), [S6.8](../s6.8-datos-local-busqueda/spec.md) | Los datos locales contienen escenarios discriminantes de texto y filtro. | Se comprueban búsquedas con acentos/formas Unicode equivalentes, varios términos, operadores literales, filtro por colección y más de una página de resultados. | El comportamiento observado coincide con las reglas fuente y los ejemplos no sugieren falsos positivos en los escenarios preparados. |
| CA03 | [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos), [architecture.md → «FTS5»](../../../context/architecture.md#fts5) | La base local contiene filas de enlaces y colecciones asociadas a la identidad de desarrollo y sus proyecciones. | Se inspeccionan los resultados acotados al usuario de desarrollo y se comparan con las entidades semilla. | Las búsquedas solo exponen los ejemplos propios de cada tipo de entidad; no aparecen filas ajenas ni mezcla entre proyecciones. |
| CA04 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.6](../s6.6-reconstruccion-proyecciones/spec.md), [S6.8](../s6.8-datos-local-busqueda/spec.md) | Se conserva una copia recuperable de la base de ejemplo y se han verificado resultados de búsqueda antes de alterar proyecciones. | Se elimina o altera de forma controlada una fila de cada proyección; luego se ejecuta la herramienta manual de reconstrucción sobre la base de validación y se repiten las búsquedas correspondientes. | El comando termina correctamente y los resultados buscables se restauran desde las entidades persistidas, sin pérdida de los datos de ejemplo. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. La base preparada y la utilidad existente permiten realizar la comprobación manual en persistencia, aislada de S10.6 y de la futura interfaz Web. | [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [S6.6](../s6.6-reconstruccion-proyecciones/spec.md), [S6.8](../s6.8-datos-local-busqueda/spec.md) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): propuesta; pendiente de aprobación.
