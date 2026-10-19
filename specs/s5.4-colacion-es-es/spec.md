# Especificación: S5.4 — Registro de la colación `es-ES` en las conexiones SQLite

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»).

## Objetivo

Hacer disponible en cada conexión SQLite abierta por la aplicación la colación necesaria para ordenar alfabéticamente los listados conforme a las reglas aprobadas.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.4 y Definition of Done.
- [architecture.md → «Persistencia»](../../context/architecture.md#persistencia): registro por conexión, comparación y ámbito de uso de la colación.
- [specifications.md → «Listados»](../../context/specifications.md#listados): orden lingüístico y desempate de los listados.
- [decisions.md → «Tecnología»](../../context/decisions.md#tecnología): elección de una colación propia y equivalencia permitida entre `ñ` y `n`.
- [roadmap.md → «Persistencia»](../../context/roadmap.md#persistencia): objetivos y límites de la etapa.
- [S5.1 — configuración de SQLite y Dapper](../s5.1-configuracion-sqlite-dapper/spec.md): mecanismo común existente para abrir conexiones.

## Alcance

- Incluye:
  - Registrar la colación definida por la arquitectura en todas las conexiones SQLite que abre la aplicación.
  - Comprobar la disponibilidad y el comportamiento de comparación de la colación en conexiones independientes.
- Excluye:
  - Cambios en columnas, índices, restricciones de unicidad o esquemas SQL para incorporar la colación; la arquitectura la reserva para su uso explícito en consultas.
  - Aplicar la colación en consultas de repositorios y validar el orden completo de los listados; corresponde a S5.5 y S5.8, respectivamente.
  - Implementar Unit of Work, transacciones, repositorios o proyecciones FTS5.
- Trabajo vecino y responsable de sus entregables:
  - S5.5 consume la colación en las consultas alfabéticas de repositorios; S5.8 valida el orden de los listados integrados, incluido el desempate por `Id`. Esos entregables no forman parte del cierre de esta especificación.

## Dependencias y prerrequisitos

- La [S5.1](../s5.1-configuracion-sqlite-dapper/spec.md) está implementada y aceptada; proporciona el mecanismo común de apertura de conexiones.
- S5.2 y S5.3 están completadas, pero sus scripts y esquema no son necesarios para comprobar el registro y la comparación de la colación.
- Las pruebas de esta especificación deben poder usar conexiones independientes del mecanismo común y datos temporales; no dependen de repositorios ni de los trabajos posteriores de S5.
- El nombre SQL de la colación no está fijado por las fuentes; el plan aprobado lo establece como `LINKUBATOR_ES_ES`. El detalle de la llamada de comparación sigue siendo una decisión técnica que debe respetar la cultura y las opciones de la arquitectura.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [S5 → tarea S5.4](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una conexión SQLite abierta por el mecanismo común de la aplicación. | Se ejecuta una comparación SQL que declara explícitamente la colación registrada. | SQLite reconoce la colación y completa la comparación sin error. |
| CA02 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [specifications.md → «Listados»](../../context/specifications.md#listados) y [S5 → tarea S5.4](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Dos conexiones SQLite abiertas independientemente por el mecanismo común. | Se ejecutan en ambas comparaciones con la colación registrada, incluyendo valores que difieren por mayúsculas y marcas diacríticas. | Cada conexión aplica la comparación lingüística `es-ES` sin distinguir mayúsculas ni marcas diacríticas; la comparación puede equiparar `ñ` con `n`. |
| CA03 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [decisions.md → «Tecnología»](../../context/decisions.md#tecnología) | La colación registrada en una conexión de la aplicación. | Se inspecciona su integración y se compara el uso explícito con el comportamiento de una consulta que no declara la colación. | La colación queda disponible para uso explícito en consultas; no se incorpora a columnas, índices ni unicidad, ni cambia implícitamente las comparaciones que no la declaran. |

Los tests de orden de los listados completos y de sus desempates se mantienen en S5.8; no son prerrequisito para cerrar esta especificación.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno. El nombre SQL `LINKUBATOR_ES_ES` quedó confirmado al aprobar el plan; la forma concreta de implementar la comparación es técnica y debe respetar la arquitectura. | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Ninguno | Sin decisión funcional pendiente. |

## Artefactos relacionados

[Plan técnico](plan.md) y [tareas](tasks.md), aprobados por DLG el 19-X-2026 («Siguiente» en cada artefacto). La implementación y validación quedaron completadas y el resultado fue aceptado por DLG el 19-X-2026 («Colación añadida»); evidencia en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
