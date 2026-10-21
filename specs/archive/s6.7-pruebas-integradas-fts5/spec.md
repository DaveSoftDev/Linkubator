# Especificación: S6.7 — Pruebas integradas de búsqueda FTS5

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Verificar conjuntamente, sobre SQLite, que las escrituras de enlaces y colecciones, sus proyecciones FTS5 y las consultas de búsqueda conservan la coherencia, atomicidad, propiedad y semántica de búsqueda definidas por las fuentes aprobadas.

## Fuentes

- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros): campos consultables, tratamiento de términos, acentos y NFC.
- [specifications.md → «Listados»](../../../context/specifications.md#listados): orden y paginación de resultados.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): atomicidad de las escrituras relacionadas.
- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): proyecciones, sincronización, aislamiento por usuario y entidad y normalización.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos): propiedad de las lecturas y escrituras privadas.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación de S6.7 y Definition of Done de la etapa.
- [S6.1 — Proyecciones FTS5 de enlaces y colecciones](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md): proyecciones disponibles.
- [S6.2 — Normalización NFC de búsqueda](../s6.2-normalizacion-nfc-busqueda/spec.md): normalización de los textos de búsqueda e indexados.
- [S6.3 — Constructor de consultas FTS5](../s6.3-constructor-consulta-fts5/spec.md): construcción de términos literales.
- [S6.4 — Sincronización de proyecciones FTS5](../s6.4-sincronizacion-proyecciones-fts5/spec.md): escrituras ordinarias y atomicidad de sus proyecciones.
- [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md): consultas persistentes de enlaces y colecciones.
- [S6.6 — Reconstrucción manual de proyecciones FTS5](../s6.6-reconstruccion-proyecciones/spec.md): operación manual de reconstrucción, fuera de las pruebas de sincronización ordinaria de este trabajo.

## Alcance

- Incluye:
  - Completar pruebas integradas de persistencia para que cambios de enlaces y colecciones se comprueben a través de sus proyecciones y consultas de búsqueda, incluidos los efectos de renombrar una colección en los enlaces asociados.
  - Comprobar que un fallo inducido durante una escritura compuesta no deja cambios parciales en las entidades ni en las proyecciones afectadas.
  - Comprobar aislamiento por usuario y por tipo de entidad en las búsquedas integradas.
  - Comprobar la búsqueda con acentos y formas Unicode NFC equivalentes tanto en el texto consultado como en los valores proyectados, sin alterar los valores originales de las entidades.
  - Usar datos de prueba sintéticos y las herramientas de persistencia y tests existentes.
- Excluye:
  - Cambiar reglas de producto, arquitectura, esquemas, producción o contratos de consulta; esos elementos pertenecen a sus fuentes y trabajos propietarios.
  - Repetir como objetivo de esta unidad la reconstrucción manual ya cubierta por S6.6.
  - Preparar el script local de datos de ejemplo o realizar la comprobación manual con él; corresponden a S6.8 y S6.9, respectivamente, y no condicionan el cierre de S6.7.
  - Probar la interfaz web o ampliar la búsqueda a etiquetas.
- Trabajo vecino y responsable de sus entregables:
  - S6.1–S6.6 entregan las proyecciones, normalizador, constructor, sincronización, consultas y reconstrucción que estas pruebas consumen.
  - S6.8 prepara el script local de datos de ejemplo; S6.9 realiza la comprobación manual de búsquedas y reconstrucción.

## Dependencias y prerrequisitos

- S6.1–S6.6 están implementadas y aceptadas; están disponibles las proyecciones, el normalizador, el constructor, la sincronización y las consultas que se integrarán.
- Existe el fixture de integración SQLite `SqliteRepositoryTestDatabase` y las pruebas de Infrastructure de la solución.
- S6.8 y S6.9 son tareas posteriores y no son prerrequisitos para validar ni cerrar esta unidad.
- La cobertura se mantiene en la capa de persistencia: comprueba el comportamiento real de SQLite y no atribuye a estas pruebas garantías de interfaz web o del procedimiento manual con datos locales.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [S6.4 — Sincronización de proyecciones FTS5](../s6.4-sincronizacion-proyecciones-fts5/spec.md), [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md) | Una base SQLite con enlaces y colecciones de prueba. | Se crean, modifican y eliminan enlaces o colecciones y se renombran colecciones con enlaces asociados, y después se consultan las búsquedas pertinentes. | Las consultas reflejan las entidades y proyecciones resultantes de las escrituras confirmadas; el renombrado se refleja en la búsqueda de colecciones y en la de sus enlaces, y no aparecen filas obsoletas de entidades eliminadas. |
| CA02 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.4 — Sincronización de proyecciones FTS5](../s6.4-sincronizacion-proyecciones-fts5/spec.md) | Una operación compuesta modifica una entidad y una o más proyecciones relacionadas. | Se induce un fallo después de que haya comenzado la escritura y antes de confirmar la operación. | La operación revierte todos los cambios del caso probado; las entidades y sus proyecciones conservan el estado anterior, y las aserciones fallarían si quedara aplicada solo una parte. |
| CA03 | [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos), [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md) | Enlaces y colecciones propios y ajenos con términos coincidentes, incluidos controles con una discrepancia entre el `UserId` de la proyección y el de la entidad. | Se ejecutan las consultas persistentes de cada listado para el usuario identificado. | Cada resultado contiene únicamente entidades propias del tipo consultado; una coincidencia de otra entidad o una discrepancia de propiedad en la proyección no permite devolver resultados ajenos. |
| CA04 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md) | Un mismo término aparece en campos buscables de enlaces y en nombres de colecciones, con datos de control que solo coinciden en el otro tipo. | Se ejecuta la búsqueda por separado en los listados de enlaces y colecciones. | Cada listado devuelve solo su entidad y solo coincide conforme a los campos asignados a su proyección, sin mezclar resultados entre entidades. |
| CA05 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.2 — Normalización NFC de búsqueda](../s6.2-normalizacion-nfc-busqueda/spec.md), [S6.4 — Sincronización de proyecciones FTS5](../s6.4-sincronizacion-proyecciones-fts5/spec.md), [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md) | Los campos buscables proyectados y las consultas contienen formas precompuestas y descompuestas de texto con acentos. | Se ejecutan búsquedas integradas en ambos listados con las formas Unicode equivalentes. | Las búsquedas producen coincidencias equivalentes conforme a las fuentes, y los valores originales de las entidades permanecen sin modificar por la indexación. |

## Bloqueos y preguntas pendientes

Ninguno comprobado. Los criterios ejercitan las capacidades implementadas en S6.1–S6.6 sin cambiar sus contratos; la técnica concreta de inducir fallos y la organización de los casos se definirán en el plan técnico tras aprobar esta especificación.

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 20-X-2026 («Aprobar el plan»).
- [Tareas y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG («Aprobar y autorizar implementación») y trabajo aceptado/cerrado para su alcance («Fase terminada»), el 20-X-2026.
