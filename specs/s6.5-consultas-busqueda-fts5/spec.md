# Especificación: S6.5 — Consultas de búsqueda FTS5

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Integrar las proyecciones FTS5 con las consultas persistentes de enlaces y colecciones, aplicando el procesamiento de búsqueda existente y las condiciones de alcance, filtros, orden y paginación establecidas por las fuentes.

## Fuentes

- [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros): comportamiento de búsqueda y filtros por listado.
- [specifications.md → «Listados»](../../context/specifications.md#listados): orden y paginación de los listados.
- [architecture.md → «FTS5»](../../context/architecture.md#fts5): consultas sobre proyecciones, aislamiento, construcción de consulta y relevancia.
- [architecture.md → «Persistencia»](../../context/architecture.md#persistencia): consultas paginadas y transacciones.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos): propiedad de los datos privados.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación de S6.5 y relación con la verificación integrada posterior.
- [S6.1 — Proyecciones FTS5 de enlaces y colecciones](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md): proyecciones disponibles.
- [S6.2 — Normalización NFC de búsqueda](../s6.2-normalizacion-nfc-busqueda/spec.md): normalizador previo a la construcción de la consulta.
- [S6.3 — Constructor de consultas FTS5](../s6.3-constructor-consulta-fts5/spec.md): constructor de expresión literal y representación de ausencia de consulta.
- [S6.4 — Sincronización de proyecciones FTS5](../s6.4-sincronizacion-proyecciones-fts5/spec.md): consistencia de las filas consultadas con las escrituras de entidades.

## Alcance

- Incluye:
  - Ejecutar búsquedas de enlaces y colecciones sobre la proyección FTS5 correspondiente, sin mezclar tipos de entidades.
  - Integrar normalización NFC del texto recibido y su transformación por el constructor existente; si no hay términos, omitir el predicado de texto.
  - Limitar tanto la consulta FTS5 como la entidad devuelta al usuario identificado.
  - Combinar la búsqueda de enlaces con sus filtros existentes, y obtener resultados paginados con el orden definido por las fuentes.
  - Mantener para las colecciones el orden alfabético definido por la fuente y aplicar la búsqueda únicamente a su nombre.
  - Verificar con pruebas persistentes focalizadas el comportamiento de ambas consultas, incluidos aislamiento por usuario y por entidad, entradas sin términos, filtros y orden/paginación cuando correspondan.
- Excluye:
  - Crear o modificar las proyecciones FTS5, o sincronizar escrituras: corresponden a S6.1 y S6.4.
  - Implementar la normalización NFC o el constructor de expresiones FTS5: corresponden a S6.2 y S6.3.
  - Las consultas de etiquetas y su proyección, que pertenecen a la etapa de gestión de etiquetas.
  - El comando de reconstrucción de las proyecciones, asignado a S6.6.
  - La matriz integrada conjunta de sincronización, atomicidad, aislamiento, acentos y NFC, asignada a S6.7; no es requisito adicional para cerrar esta unidad.
  - Cambiar el contrato funcional de búsqueda, filtros, orden o paginación.
- Trabajo vecino y responsable de sus entregables:
  - S6.1–S6.4 aportan, respectivamente, las proyecciones, la normalización, el constructor y la sincronización.
  - S6.6 aporta el comando de reconstrucción.
  - S6.7 aporta la verificación integrada conjunta; sus resultados no se atribuyen ni se exigen a la implementación propia de estas consultas.

## Dependencias y prerrequisitos

- S6.1 está implementada y aceptada; existen las proyecciones FTS5 de enlaces y colecciones.
- S6.2 y S6.3 están implementadas y aceptadas; existen el normalizador NFC y el constructor de consulta.
- S6.4 está implementada y aceptada; las proyecciones quedan sincronizadas con las escrituras incluidas en su alcance.
- La implementación de repositorios y su Unit of Work está disponible según S5.5.
- El contrato funcional ya está establecido por las secciones enlazadas. No se necesita definir reglas nuevas.
- La cobertura de esta spec verifica directamente consultas de persistencia en SQLite. No depende del comando de reconstrucción ni de la matriz integrada de S6.7.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Una búsqueda de enlaces con términos que coinciden en la proyección de enlaces. | Se ejecuta el listado para el usuario identificado. | Solo se consideran coincidencias de la proyección de enlaces y la consulta devuelve enlaces de ese usuario, sin resultados de otras entidades ni usuarios. |
| CA02 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Proyecciones con coincidencias de más de un usuario y resultados de enlace ajenos al usuario de la consulta. | Se ejecuta la búsqueda por texto. | La restricción de usuario se aplica en la consulta de la proyección y en la entidad resultante, sin devolver datos privados ajenos. |
| CA03 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados) | Una búsqueda de enlaces con más de una coincidencia y un `q` que produce términos. | Se consulta el listado paginado. | El orden de relevancia y sus desempates, filtros disponibles y paginación coinciden con las fuentes y se aplican al conjunto filtrado, sin cargar resultados innecesarios. |
| CA04 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados) | `q` vacío, compuesto solo por separadores o sin términos tras el procesamiento. | Se ejecuta el listado de enlaces, con y sin sus otros filtros. | No se ejecuta una búsqueda FTS5 vacía; los resultados quedan limitados por los demás filtros y usan el orden no textual del listado. |
| CA05 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados) | Una búsqueda de colecciones para el usuario identificado, con coincidencias propias y ajenas. | Se ejecuta el listado filtrado por texto. | Solo se buscan nombres en la proyección de colecciones, se devuelven colecciones propias y se conserva el orden alfabético definido, no la relevancia FTS5. |
| CA06 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados) | Un listado de colecciones sin términos de búsqueda. | Se consulta la primera página y otra página del resultado. | No se ejecuta MATCH vacío; el listado mantiene su orden alfabético y paginación definidos, sin modificar el alcance del usuario. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. La verificación conjunta asignada a S6.7 no bloquea la comprobación directa de las consultas de esta unidad. | [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | — | — |

## Artefactos relacionados

El plan técnico se creará tras la aprobación explícita de esta especificación.
