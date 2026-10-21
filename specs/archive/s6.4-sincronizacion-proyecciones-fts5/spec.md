# Especificación: S6.4 — Sincronización de proyecciones FTS5

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Mantener las proyecciones FTS5 de enlaces y colecciones coherentes con sus entidades después de cada escritura correspondiente, como parte de la misma unidad atómica, sin cambiar el comportamiento funcional de esas operaciones.

## Fuentes

- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): entidades proyectadas, campos, normalización y sincronización.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): escrituras relacionadas y proyecciones en una misma transacción.
- [domain-model.md → «Link»](../../../context/domain-model.md#link): propiedades editables y eliminación física del enlace.
- [domain-model.md → «Collection»](../../../context/domain-model.md#collection): renombrado y eliminación de colección.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros): campos buscables y normalización NFC.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación, alcance y dependencia de S6.4.
- [S6.1 — «Proyecciones FTS5 de enlaces y colecciones»](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md): proyecciones disponibles.
- [S6.2 — «Normalización NFC de búsqueda»](../s6.2-normalizacion-nfc-busqueda/spec.md): normalizador reusable para contenido proyectado.

## Alcance

- Incluye:
  - Sincronizar la proyección de enlaces al crear, editar metadatos buscables, mover y eliminar un enlace.
  - Sincronizar la proyección de colecciones al crear, renombrar y eliminar una colección.
  - Al renombrar una colección, actualizar también el nombre proyectado en cada fila de enlaces asociado, conservando la atomicidad con el renombrado.
  - Aplicar el normalizador NFC ya existente al texto que se escribe en las proyecciones; conservar sin cambios los valores originales de las entidades.
  - Comprobar los resultados con pruebas focalizadas de persistencia, incluidas escrituras compuestas y fallos inyectados dentro de la unidad de trabajo.
- Excluye:
  - Crear o cambiar las proyecciones y su esquema, ya entregado en S6.1.
  - Implementar el normalizador NFC, entregado por S6.2.
  - Construir consultas FTS5, ejecutarlas o cambiar la búsqueda, filtros, orden o paginación.
  - Implementar el comando de reconstrucción manual de proyecciones, asignado a S6.6.
  - Añadir etiquetas o la proyección de etiquetas; su gestión y su integración de nombres en la proyección de enlaces pertenecen a la etapa de gestión de etiquetas.
- Trabajo vecino y responsable de sus entregables:
  - S6.1 aporta las proyecciones base y S6.2 el normalizador NFC.
  - S6.3 aporta el constructor de consultas; S6.5 consulta las proyecciones y S6.6 las reconstruye.
  - S6.7 es responsable de la cobertura integrada conjunta de sincronización, atomicidad, aislamiento, acentos y NFC; no es prerrequisito para comprobar directamente el comportamiento de escritura de esta unidad.

## Dependencias y prerrequisitos

- S6.1 está implementada y aceptada; el esquema contiene las proyecciones FTS5 de enlaces y colecciones.
- S6.2 dispone de un normalizador NFC implementado. La spec existe y sus tests directos pasaron, pero su aceptación renovada está pendiente en las tareas; confirmar esa aceptación antes de iniciar la implementación dependiente.
- S6.3 está implementada y aceptada; no es necesaria para las escrituras, pero es trabajo precedente de la secuencia S6.
- La sincronización se integra en las operaciones de persistencia ya existentes, sin modificar sus reglas ni trasladar la autoridad de escritura fuera de la transacción actual.
- La verificación propia de S6.4 se limita a las escrituras y sus proyecciones. Las búsquedas de extremo a extremo, su aislamiento conjunto y la recuperación mediante reconstrucción quedan a cargo de S6.5–S6.7.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [mvp0-plan.md → S6.4](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una operación de creación o actualización de un enlace que afecta a sus campos buscables o a su colección. | La operación se confirma. | La proyección de enlaces refleja el estado confirmado de los campos asignados al enlace, incluyendo el nombre de la colección actual, y los valores originales de la entidad no se alteran por la indexación. |
| CA02 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [mvp0-plan.md → S6.4](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una colección creada o renombrada, con cero o más enlaces asociados. | La escritura se confirma. | La proyección de colecciones refleja el nombre actual y las filas de proyección de enlaces asociadas reflejan el nombre renombrado, sin afectar filas de otras colecciones. |
| CA03 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) | Texto original con una forma Unicode canónicamente equivalente y las operaciones que lo incorporan a una proyección. | Se persiste la proyección. | El texto indexado se procesa con el normalizador NFC común, mientras las propiedades originales de `Link` y `Collection` permanecen intactas. |
| CA04 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [mvp0-plan.md → S6.4](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una operación de eliminación de enlace o colección válida según sus reglas existentes. | Se confirma la operación. | Se elimina la fila correspondiente de la proyección; no quedan entradas huérfanas para la entidad eliminada. |
| CA05 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [mvp0-plan.md → S6.4](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una operación compuesta de escritura que actualiza una entidad y su proyección, incluido el renombrado con filas de enlaces asociadas. | Se inyecta un fallo antes de confirmar. | La transacción revierte la modificación de la entidad y todas sus modificaciones de proyección, sin estado parcial. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado tras la aceptación renovada de S6.2 por DLG («Sí, acepto S6.2», 20-X-2026). | [S6.2 — «Normalización NFC de búsqueda»](../s6.2-normalizacion-nfc-busqueda/spec.md) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): propuesta; pendiente de aprobación.
