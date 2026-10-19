# Plan técnico: S5.10 — Revisión manual de la aplicación repetida del script local

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- No se prevén cambios de código. La revisión la ejecuta una persona sobre el recurso SQL local ya implementado en S5.6.
- Se utilizará una herramienta SQLite que permita ejecutar el recurso con sus parámetros y consultar los datos resultantes. La herramienta y versión se registrarán en la evidencia.
- Las reglas de creación y preservación verificadas son las de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) y [specifications.md → «Identidad de desarrollo»](../../context/specifications.md#identidad-de-desarrollo); este plan no las redefine.

### Flujos internos

1. Preparar una base SQLite aislada con el esquema requerido, sin usar ni modificar la base de datos habitual del desarrollador.
2. Obtener el recurso SQL local de S5.6 y ejecutarlo manualmente con los mismos parámetros de configuración en ambas ejecuciones.
3. Tras la primera ejecución, comprobar la cuenta y sus colecciones. Registrar un perfil completado y una colección con valores reconocibles distintos de los configurados, conservando un estado válido para el esquema.
4. Ejecutar por segunda vez el mismo recurso SQL en esa misma base y volver a inspeccionar el número de cuentas y colecciones, junto con los valores reconocibles.
5. Registrar herramienta, versión, identificación de la base aislada, observaciones de ambas ejecuciones, hallazgos y resolución. No copiar credenciales, valores de parámetros sensibles ni hashes completos a la evidencia.

### Persistencia y dependencias

- La base SQLite es desechable y exclusiva para esta comprobación. Su esquema puede prepararse con el flujo de migraciones ya disponible; la revisión no vuelve a evaluar el contenido de las migraciones, responsabilidad cerrada en S5.9.
- La ejecución usa el SQL local y la configuración ya provistos por S5.6. El trabajo no modifica el catálogo ni el historial de migraciones.
- No se requieren dependencias externas nuevas ni cambios productivos.

### Garantías técnicas

- Cada observación debe corresponder a la misma base y a la misma identidad configurada en ambas ejecuciones.
- Los valores reconocibles se establecen después de la primera ejecución para que la segunda pueda revelar una alteración que una comprobación de conteos por sí sola no detectaría.
- La comprobación manual cubre solo el escenario observado; no demuestra concurrencia, orden de arranque ni rollback. Esas garantías se validaron por separado en [S5.6 → evidencia por criterio](../s5.6-sql-local-identidad-desarrollo/tasks.md#evidencia-por-criterio).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar una base aislada y desechable, nunca la base habitual del desarrollador. | propuesta | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Evita alterar datos ajenos al ensayo; confirmar en la aprobación del plan. |
| Registrar valores reconocibles en el perfil completado y en una colección antes de la segunda ejecución. | propuesta | [CA02](spec.md#criterios-de-aceptación) | Hace observable si el script reinicia datos existentes; seleccionar valores válidos durante la preparación. |
| Elegir la herramienta SQLite concreta para ejecutar el recurso y revisar los resultados. | pendiente | [CA03](spec.md#criterios-de-aceptación) | Registrar nombre y versión en la evidencia; confirmar antes de ejecutar la revisión. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual; dos ejecuciones del recurso en una misma base aislada. | Esquema disponible, configuración y herramienta SQLite elegida. | Ejecutar el SQL local dos veces con los mismos parámetros; inspeccionar la cuenta por el email configurado y sus colecciones tras cada ejecución. | Una cuenta para la identidad configurada y ninguna colección duplicada por la segunda ejecución. No demuestra concurrencia ni idempotencia en otros estados. | Herramienta y versión, base aislada identificada sin exponer datos sensibles, conteos observados y resultado de ambas ejecuciones. |
| V02 | CA02 | Manual; preservación de valores reconocibles. | V01 completada; cuenta completa y colección existente tras la primera ejecución. | Antes de la segunda ejecución, cambiar el perfil y una colección a valores válidos distinguibles de la configuración; después, comparar esos valores con los persistidos. | El perfil completado y la colección conservan sus valores tras la segunda ejecución. No demuestra preservación de todos los campos ni de todos los estados posibles. | Campos comparados y observaciones antes/después; omitir hashes, credenciales y parámetros sensibles. |
| V03 | CA03 | Revisión de evidencia. | V01 y V02 completadas. | Revisar que la evidencia identifica herramienta/versión, misma base, resultados, hallazgos y resolución. | Evidencia suficiente para que DLG valore la aceptación manual de S5.10, sin atribuirle validaciones automatizadas de S5.6 ni la revisión de esquema de S5.9. | Registro consolidado enlazado desde `tasks.md` cuando se creen las tareas tras aprobar este plan. |

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá dependencias, checks inmediatos y evidencia para V01–V03.
- Los criterios solo se cierran con evidencia satisfactoria de esta revisión, sin depender de cambios futuros ni de la aceptación de S5.6 o S5.9.
- Ante discrepancias, detener la revisión y registrar el hallazgo; no modificar el script ni ampliar el alcance desde este trabajo.