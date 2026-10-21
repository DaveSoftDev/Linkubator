# Especificación: S6.6 — Reconstrucción manual de proyecciones FTS5

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Proporcionar una operación manual que reconstruya las proyecciones de búsqueda existentes a partir de sus entidades persistidas, recuperando su coherencia sin dejar una reconstrucción parcial ante un fallo.

## Fuentes

- [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance): alcance MVP0 limitado a enlaces y colecciones; gestión de etiquetas reservada para MVP1.
- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): diseño de las proyecciones, campos indexados y reconstrucción técnica.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): límites y atomicidad de las transacciones.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación de S6.6 y comprobación manual posterior de S6.9.
- [S6.1 — Proyecciones FTS5 de enlaces y colecciones](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md): proyecciones disponibles actualmente.
- [S6.4 — Sincronización de proyecciones FTS5](../s6.4-sincronizacion-proyecciones-fts5/spec.md): sincronización ordinaria que no sustituye esta operación de reparación.

## Alcance

- Incluye:
  - Ofrecer una operación de mantenimiento iniciada manualmente para regenerar las proyecciones FTS5 existentes de enlaces y colecciones a partir de las tablas de sus entidades.
  - Ejecutar el vaciado y la regeneración como una unidad atómica.
  - Verificar la recuperación de proyecciones alteradas y el rollback ante un fallo durante la reconstrucción.
- Excluye:
  - La gestión y reconstrucción de etiquetas, reservadas al alcance de MVP1 según [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance).
  - Crear o cambiar esquemas de proyección, o sincronizarlas con escrituras ordinarias.
  - Ejecutar la comprobación manual con los datos de ejemplo asignada a S6.9.
- Trabajo vecino y responsable de sus entregables:
  - S6.1 aporta las proyecciones de enlaces y colecciones; S6.4 las mantiene sincronizadas con las escrituras ordinarias.
  - S6.9 ejecuta manualmente la búsqueda con datos de ejemplo y el comando de reconstrucción.

## Dependencias y prerrequisitos

- S6.1 está implementada y aceptada; existen las proyecciones FTS5 de enlaces y colecciones.
- S6.4 está implementada y aceptada; la reconstrucción es una operación separada de la sincronización ordinaria.
- La mención arquitectónica de etiquetas en la reconstrucción no amplía el alcance del MVP0: [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance) reserva su gestión para MVP1, y [S6.1](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md) explícitamente excluye la proyección de etiquetas.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance), [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.1 — Proyecciones FTS5 de enlaces y colecciones](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md) | Proyecciones de enlaces y colecciones alteradas o incompletas respecto de sus tablas de entidades. | Se ejecuta manualmente la reconstrucción. | Las proyecciones FTS5 de enlaces y colecciones se regeneran desde sus entidades y vuelven a representar sus datos persistidos; no se incluye una proyección de etiquetas. |
| CA02 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [architecture.md → «FTS5»](../../../context/architecture.md#fts5) | Una reconstrucción que ha empezado a modificar las proyecciones. | Se produce un fallo antes de completar la operación. | Se revierte la transacción y no queda aplicada una reconstrucción parcial. |
| CA03 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [mvp0-plan.md → S6.6](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | La aplicación está en uso normal, sin una petición explícita de mantenimiento. | Se inicia o se ejecuta una operación ordinaria de la aplicación. | La reconstrucción se ejecuta únicamente mediante una acción manual de mantenimiento, no como parte del arranque ni de las escrituras ordinarias. |

## Bloqueos y preguntas pendientes

Ninguno comprobado. El alcance se limita a enlaces y colecciones conforme a [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance) y a la exclusión explícita de etiquetas en [S6.1](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md). La mención de etiquetas en la descripción arquitectónica general de la reconstrucción no se considera parte de S6.6/MVP0.

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 20-X-2026 («Siguiente»).
- [Tareas y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG («Vamos») y trabajo aceptado/cerrado para su alcance («Tenemos reconstrucción de proyecciones»), el 20-X-2026.
