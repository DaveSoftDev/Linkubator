# Especificación: S5.2 — Ejecutor de scripts SQL versionados

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»).

## Objetivo

Disponer en Infrastructure de un ejecutor que aplique scripts SQL versionados a la base de datos SQLite de la aplicación y permita reconocer cuáles ya se han aplicado, sin incluir en este trabajo el esquema de las entidades.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.2, tareas vecinas y Definition of Done.
- [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías) y [«Infrastructure»](../../../context/architecture.md#infrastructure): SQLite, Dapper y responsabilidades de persistencia.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): resolución de migraciones mediante scripts SQL versionados o mecanismo ligero equivalente.
- [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia): objetivos y criterio de finalización de la etapa.

## Alcance

- Incluye:
  - Ejecutar scripts SQL versionados sobre SQLite mediante Infrastructure.
  - Mantener información que permita distinguir los scripts ya aplicados de los pendientes y evitar reaplicarlos en ejecuciones posteriores.
  - Permitir comprobar el ejecutor con scripts de prueba, sin requerir el esquema de S5.3.
  - El ejecutor expone una operación invocable por el host; la secuencia y el momento de invocación en el arranque son responsabilidad de la composición de la aplicación, definidos en la especificación de aprovisionamiento de identidad.
- Excluye:
  - Definir el esquema de `User`, `Collection` y `Link`, sus restricciones e índices; corresponde a S5.3.
  - Configurar SQLite y Dapper; corresponde a [S5.1](../s5.1-configuracion-sqlite-dapper/spec.md).
  - Registrar la colación, implementar Unit of Work y repositorios, provisionar el usuario local o implementar su proveedor de identidad; corresponden a S5.4–S5.7.
  - Validar restricciones, concurrencia y orden alfabético; corresponde a S5.8.
  - La integración del ejecutor en la secuencia de arranque; corresponde a S5.6. La aplicación manual y revisión humana del conjunto final de scripts y del esquema corresponden a S5.9.
- Trabajo vecino y responsable de sus entregables:
  - S5.1 y S5.3–S5.10 implementan y validan sus entregables propios conforme a [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l). Ninguno forma parte del cierre de esta especificación.

## Dependencias y prerrequisitos

- Existe `Linkubator.Infrastructure` y el mecanismo de acceso a SQLite está definido por la arquitectura y la [S5.1](../s5.1-configuracion-sqlite-dapper/spec.md). La implementación de S5.1 aún no figura como iniciada en el roadmap; el plan de S5.2 deberá concretar la integración con su contrato sin duplicar la configuración de conexiones.
- Los scripts de esquema de S5.3 no son prerrequisito para comprobar este trabajo: se usarán scripts de prueba aislados.
- La ubicación y el formato de los scripts, la representación de sus versiones y el tratamiento transaccional de cada aplicación son detalles técnicos que debe concretar el plan. No se fijan aquí porque las fuentes no los determinan.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [mvp0-plan.md → tarea S5.2 y Definition of Done](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una base SQLite temporal y un conjunto de scripts de prueba con versiones distintas. | Se ejecuta el mecanismo de migración de Infrastructure sobre una base nueva. | Los scripts se aplican en orden de versión y sus cambios quedan disponibles en la base. |
| CA02 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [mvp0-plan.md → tarea S5.2 y Definition of Done](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una base en la que CA01 ya aplicó sus scripts. | Se vuelve a ejecutar el mecanismo de migración. | Los scripts ya registrados no se vuelven a aplicar y la base conserva el mismo estado resultante. |
| CA03 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [mvp0-plan.md → tarea S5.2](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Un conjunto de scripts de prueba que incluye uno cuyo SQL falla. | Se ejecuta la migración. | El fallo se comunica como tal y el script fallido no queda registrado como aplicado; la validación no presupone que los cambios parciales del SQL se reviertan. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno para redactar la especificación. La ubicación, el formato/versionado y la estrategia transaccional de los scripts quedan como propuestas del plan técnico. | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | Ninguno | Revisar y aprobar las propuestas al aprobar el plan. |

## Artefactos relacionados

[Plan técnico](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»). Las [tareas](tasks.md) fueron aprobadas por DLG con «Completado» y el resultado fue aceptado con «Migraciones ready», ambas referencias el 19-X-2026.
