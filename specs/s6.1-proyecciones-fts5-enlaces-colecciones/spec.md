# Especificación: S6.1 — Proyecciones FTS5 de enlaces y colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Disponer de proyecciones FTS5 independientes para enlaces y colecciones, compatibles con el motor de búsqueda y el aislamiento por usuario definidos en las fuentes, sin alterar los datos originales.

## Fuentes

- [architecture.md → «FTS5»](../../context/architecture.md#fts5): diseño técnico propietario de las proyecciones y la configuración del tokenizador.
- [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros): campos buscables y comportamiento de búsqueda.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos): límite de propiedad por usuario.
- [roadmap.md → «Checklist de calidad del MVP0»](../../context/roadmap.md#checklist-de-calidad-del-mvp0): cobertura prevista para el índice y las proyecciones de estas entidades.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación y alcance de S6.1.

## Alcance

- Incluye:
  - Crear las proyecciones FTS5 independientes para enlaces y colecciones, conforme a la definición de sus columnas, `rowid` y ámbito de usuario de la arquitectura.
  - Configurar el tokenizador de acuerdo con la arquitectura y las reglas de búsqueda aprobadas.
  - Dejar las proyecciones disponibles a la capa de persistencia, sin implementar en este trabajo la búsqueda completa ni su sincronización con las operaciones de escritura.
- Excluye:
  - Proyección de etiquetas y nombres de etiquetas dentro de la proyección de enlaces, asignadas al trabajo posterior de gestión de etiquetas.
  - Normalización NFC de indexación y consulta, constructor de consultas, consultas de búsqueda, actualización transaccional de proyecciones y reconstrucción manual; corresponden a S6.2–S6.6.
  - Modificar las reglas de producto, los campos buscables, el orden, la paginación o el aislamiento especificados en `context/`.
- Trabajo vecino y responsable de sus entregables:
  - S5 proporciona SQLite, migraciones, conexiones y esquema de entidades; sus artefactos archivados son prerrequisitos existentes.
  - S6.2–S6.6 implementan respectivamente la normalización, construcción de consulta, sincronización, consultas y reconstrucción.
  - La etapa posterior de gestión de etiquetas incorpora su proyección, integra etiquetas en la proyección de enlaces y actualiza las tres proyecciones.

## Dependencias y prerrequisitos

- S5.1–S5.8 están implementadas y aceptadas según el [roadmap](../../context/roadmap.md#persistencia); proporcionan conexión SQLite, scripts versionados, esquema y pruebas de integración.
- S5.2.1 separa los scripts de prueba del catálogo real de migraciones; la proyección se incorporará mediante la infraestructura de scripts versionados existente, sin incluir artefactos de prueba en el catálogo de producción.
- Las reglas de búsqueda y la arquitectura FTS5 están documentadas. No se requiere aprobación de nuevos requisitos para los criterios de esta especificación.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [mvp0-plan.md → S6.1](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una base SQLite creada mediante el flujo vigente de scripts versionados. | Se aplica la evolución de esquema que incorpora las proyecciones de búsqueda de las entidades incluidas. | Existen dos proyecciones FTS5 separadas, cada una asociada al identificador y al usuario de su propia entidad, sin colisiones entre los identificadores de enlaces y colecciones. |
| CA02 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Las proyecciones recién creadas. | Se inspecciona su definición de columnas y tokenización con ejemplos representativos de letras, números, mayúsculas y marcas diacríticas. | La proyección de enlaces contiene los campos textuales asignados a enlaces; la de colecciones contiene únicamente su nombre; ambas usan el tokenizador y las opciones de equivalencia definidos por las fuentes. |
| CA03 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Datos indexados de dos usuarios, incluidos identificadores coincidentes entre tipos de entidad cuando la base de prueba lo permita. | Se comprueba el contenido y el ámbito de cada proyección. | Cada fila queda vinculada únicamente al usuario y a la entidad que le corresponden; las proyecciones no combinan entidades ni introducen resultados de otro usuario. |
| CA04 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una base vacía y una base que ya aplicó el catálogo vigente. | Se aplica y se vuelve a aplicar el script versionado de las proyecciones según el ejecutor establecido. | La evolución de esquema se aplica correctamente y repetir el proceso no produce errores ni altera la estructura esperada; los scripts de prueba no se incorporan como migraciones de producción. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. El alcance de S6.1 se limita a enlaces y colecciones según el plan; la proyección de etiquetas y los nombres de etiquetas en enlaces se asignan al trabajo posterior de etiquetas. | [mvp0-plan.md → S6.1](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [roadmap.md → «Gestión de etiquetas»](../../context/roadmap.md#gestión-de-etiquetas) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 19-X-2026 («Siguiente»).
- [Tareas y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG («Vamos») y trabajo aceptado/cerrado con «Tenemos proyeciones», el 19-X-2026.
