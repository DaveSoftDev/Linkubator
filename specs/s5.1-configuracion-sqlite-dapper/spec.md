# Especificación: S5.1 — Configuración de SQLite y Dapper

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»).

## Objetivo

Preparar en Infrastructure el acceso SQLite usado por la aplicación mediante `Microsoft.Data.Sqlite` y Dapper, asegurando que las conexiones abiertas por la aplicación usan la configuración de concurrencia y claves foráneas aprobada.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): alcance y tarea S5.1.
- [architecture.md → «Tecnologías»](../../context/architecture.md#tecnologías) y [«Infrastructure»](../../context/architecture.md#infrastructure): proveedor de SQLite, Dapper y responsabilidades de la capa.
- [architecture.md → «Persistencia»](../../context/architecture.md#persistencia): configuración de SQLite y ciclo de acceso a datos.
- [decisions.md → «Tecnología»](../../context/decisions.md#tecnología): elección de SQLite y Dapper.
- [roadmap.md → «Persistencia»](../../context/roadmap.md#persistencia): objetivos y criterio de finalización de la etapa.

## Alcance

- Incluye:
  - Incorporar y configurar `Microsoft.Data.Sqlite` y Dapper en Infrastructure.
  - Proporcionar el mecanismo común de apertura de conexiones SQLite de la aplicación.
  - Asegurar WAL, `busy_timeout` y claves foráneas activadas en las conexiones que abre la aplicación.
- Excluye:
  - Scripts SQL versionados, esquema, restricciones e índices; corresponden a S5.2 y S5.3.
  - Registro de la colación `es-ES`; corresponde a S5.4.
  - Unit of Work, transacciones y repositorios; corresponden a S5.5.
  - SQL local de desarrollo y proveedor de identidad; corresponden a S5.6 y S5.7.
  - Pruebas de integración de restricciones, carreras y orden alfabético de S5.8.
  - Proyecciones FTS5, correspondientes a S6.
- Trabajo vecino y responsable de sus entregables:
  - S5.2–S5.8 implementan y validan los entregables indicados en sus tareas del [plan MVP0](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l). Ninguno de ellos forma parte del cierre de esta spec.

## Dependencias y prerrequisitos

- Existe el proyecto `Linkubator.Infrastructure` y su extensión de registro de servicios.
- Las decisiones técnicas de valor de `busy_timeout` y ciclo de vida de la inicialización de WAL se concretarán en el plan; esta especificación no fija sus valores.
- Las validaciones de esta spec deben poder ejecutarse con conexiones SQLite independientes y no requieren el esquema de S5.3 ni el Unit of Work de S5.5.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Tecnologías»](../../context/architecture.md#tecnologías), [«Infrastructure»](../../context/architecture.md#infrastructure) y [S5 → tarea S5.1](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | El proyecto de Infrastructure y su registro de servicios. | Se revisan sus dependencias y se resuelve el mecanismo de conexión registrado. | Infrastructure utiliza `Microsoft.Data.Sqlite` para SQLite y Dapper para SQL explícito, y la aplicación dispone de un mecanismo común para abrir sus conexiones. |
| CA02 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [S5 → tarea S5.1 y Definition of Done](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una conexión SQLite abierta por el mecanismo común de la aplicación. | Se consultan sus opciones activas de SQLite. | La base de datos opera en modo WAL, `busy_timeout` está configurado explícitamente y las claves foráneas están activadas en esa conexión. |
| CA03 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [roadmap.md → «Persistencia»](../../context/roadmap.md#persistencia) | Dos conexiones SQLite abiertas de forma independiente por el mecanismo común. | Se comprueban las opciones activas de cada conexión. | Cada conexión satisface CA02; la configuración no depende de que una conexión anterior permanezca abierta. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| La fuente no fija el valor numérico de `busy_timeout` ni cómo se inicializa WAL respecto al ciclo de vida de la base de datos. | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [S5 → tarea S5.1](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Ninguno; CA02 y CA03 comprueban la configuración activa, sin prescribir esos detalles. | Revisar la propuesta técnica en `plan.md` antes de aprobarlo. |

## Artefactos relacionados

- [Plan técnico](plan.md), propuesta pendiente de aprobación.