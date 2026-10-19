# Especificación: S5.10 — Revisión manual de la aplicación repetida del script local

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Comprobar manualmente, sobre una misma base SQLite, que aplicar de nuevo el script local de aprovisionamiento no duplica ni altera la identidad de desarrollo ni sus colecciones existentes, dejando evidencia revisable del resultado.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.10 y Definition of Done de Persistencia.
- [roadmap.md → «Persistencia»](../../context/roadmap.md#persistencia): criterio de finalización de la etapa.
- [specifications.md → «Identidad de desarrollo»](../../context/specifications.md#identidad-de-desarrollo): comportamiento esperado al repetir el aprovisionamiento.
- [S5.6 — Aprovisionamiento SQL de identidad de desarrollo → criterios de aceptación](../s5.6-sql-local-identidad-desarrollo/spec.md#criterios-de-aceptación): estados que crea, completa o preserva el script local.
- [S5.6 — tareas → «Evidencia por criterio»](../s5.6-sql-local-identidad-desarrollo/tasks.md#evidencia-por-criterio): validación automatizada ya realizada, distinta de esta revisión humana.
- [S5.9 — Revisión manual de migraciones y esquema SQLite → alcance](../s5.9-revision-manual-migraciones-esquema/spec.md#alcance): separación de esta revisión y la inspección de migraciones/esquema.

## Alcance

- Incluye:
  - Aplicar dos veces el script local de aprovisionamiento sobre la misma base SQLite, con la misma identidad configurada.
  - Verificar que la segunda aplicación no crea duplicados ni reinicia datos de una cuenta completada o de sus colecciones existentes.
  - Registrar la herramienta, la base aislada utilizada, el estado comprobado antes y después de la segunda aplicación, los hallazgos y su resolución.
- Excluye:
  - Cambiar el script, el esquema, las reglas de identidad o el código de aprovisionamiento.
  - Repetir los tests automatizados de idempotencia y concurrencia de S5.6.
  - Aplicar o revisar migraciones y esquema, cubiertos por S5.9.
- Trabajo vecino y responsable de sus entregables:
  - S5.6 proporciona el script local y sus validaciones automatizadas; S5.9 revisa las migraciones y el esquema. Esta especificación solo aporta la comprobación manual de la aplicación repetida del script.

## Dependencias y prerrequisitos

- S5.1–S5.6 están implementadas y aceptadas; el recurso SQL local y el esquema requerido están disponibles.
- S5.9 está cerrada para su alcance. Su revisión no sustituye la aplicación repetida de este trabajo.
- Se dispone de una base SQLite aislada y de los parámetros de configuración necesarios para ejecutar manualmente el recurso SQL.
- La preparación exacta de la base, los valores reconocibles para detectar cambios y la herramienta de inspección se concretarán en el plan técnico.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) y [specifications.md → «Identidad de desarrollo»](../../context/specifications.md#identidad-de-desarrollo) | Una base SQLite aislada con el esquema requerido y una identidad de desarrollo configurada. | Se aplica el script local dos veces sobre esa misma base y se inspecciona el usuario identificado por el email configurado y sus colecciones. | Existe una sola cuenta para esa identidad y la segunda aplicación no duplica las colecciones que ya existían tras la primera. |
| CA02 | [specifications.md → «Identidad de desarrollo»](../../context/specifications.md#identidad-de-desarrollo) y [S5.6 → CA02–CA03](../s5.6-sql-local-identidad-desarrollo/spec.md#criterios-de-aceptación) | Tras la primera aplicación, la cuenta está completada y tiene al menos una colección existente; se registran valores reconocibles de su perfil y de una colección. | Se preparan valores reconocibles distintos de los valores configurados en el perfil y en la colección existentes, y se aplica por segunda vez el mismo script con la misma configuración. | Los valores preexistentes del perfil completado y de la colección permanecen sin cambios; la revisión no observa que el aprovisionamiento los reinicie. |
| CA03 | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) y [S5.6 → CA04](../s5.6-sql-local-identidad-desarrollo/spec.md#criterios-de-aceptación) | La revisión manual de CA01 y CA02 ha terminado. | Se consolida su evidencia. | La evidencia identifica la herramienta y versión, confirma que ambas ejecuciones usaron la misma base aislada, registra las observaciones antes/después y cualquier hallazgo y resolución, y no expone credenciales ni hashes completos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno conocido para redactar esta especificación. La herramienta y los valores concretos de comprobación se decidirán en el plan sin cambiar el alcance aprobado. | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Ninguno | Aprobar la especificación y revisar las propuestas del plan técnico. |

## Artefactos relacionados

[Plan técnico](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»). Las [tareas](tasks.md) fueron aprobadas por DLG el 19-X-2026 («Vamos»).