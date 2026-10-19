# Especificación: S5.3 — Esquema de `User`, `Collection` y `Link`

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»).

## Objetivo

Definir mediante scripts versionados el esquema SQLite de `User`, `Collection` y `Link`, incluidas las restricciones de unicidad, la garantía de propiedad entre enlaces y colecciones, y los índices previstos para su acceso.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.3, tareas vecinas y Definition of Done.
- [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [«Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad): propiedades, invariantes y unicidades propietarias.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): representación de identificadores y fechas, restricciones, índices y garantías de relaciones.
- [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías) e [«Infrastructure»](../../../context/architecture.md#infrastructure): SQLite, scripts versionados y responsabilidad de Infrastructure.
- [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia): objetivo y criterio de finalización de la etapa.
- [S5.1 — configuración de SQLite y Dapper](../s5.1-configuracion-sqlite-dapper/spec.md) y [S5.2 — ejecutor de scripts SQL versionados](../s5.2-ejecutor-scripts-sql-versionados/spec.md): prerrequisitos técnicos existentes.

## Alcance

- Incluye:
  - Añadir al catálogo de migraciones el esquema de `User`, `Collection` y `Link`, de acuerdo con las propiedades persistidas de sus entidades.
  - Representar en SQLite las restricciones únicas documentadas para esas entidades y los índices previstos por la arquitectura.
  - Garantizar mediante claves foráneas que los datos referenciados existen y que un enlace solo puede pertenecer a una colección del mismo usuario.
  - Mantener las relaciones sin borrados ni actualizaciones en cascada y sin triggers, conforme a la arquitectura.
- Excluye:
  - Configuración de conexiones SQLite y Dapper, correspondiente a [S5.1](../s5.1-configuracion-sqlite-dapper/spec.md).
  - Catálogo, registro y ejecución de migraciones, correspondientes a [S5.2](../s5.2-ejecutor-scripts-sql-versionados/spec.md).
  - Colación `es-ES`, Unit of Work y repositorios, correspondientes a S5.4 y S5.5.
  - SQL local de desarrollo y proveedor de identidad, correspondientes a S5.6 y S5.7.
  - Pruebas de integración completas de restricciones, concurrencia y orden alfabético, correspondientes a S5.8; y revisión manual del esquema aplicado, correspondiente a S5.9.
  - Tablas de `Tag`, `LinkTag` y `UserToken`, y proyecciones FTS5, que no forman parte del esquema de estas tres entidades.
- Trabajo vecino y responsable de sus entregables:
  - S5.1–S5.2 proporcionan los mecanismos de conexión y migración; S5.4–S5.10 desarrollan y validan sus entregables propios según el [plan MVP0 → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l). Ninguno de esos trabajos amplía el cierre de esta especificación.

## Dependencias y prerrequisitos

- La configuración de conexiones de [S5.1](../s5.1-configuracion-sqlite-dapper/spec.md) está implementada y aceptada; su estado y evidencia constan en sus [tareas](../s5.1-configuracion-sqlite-dapper/tasks.md).
- El ejecutor de scripts de [S5.2](../s5.2-ejecutor-scripts-sql-versionados/spec.md) está implementado, validado y aceptado; el registro consta en sus [tareas](../s5.2-ejecutor-scripts-sql-versionados/tasks.md).
- Los índices concretos y el mapeo físico de propiedades a columnas no están prescritos por las fuentes; se concretarán como decisiones técnicas del plan sin cambiar las invariantes documentadas.
- Las pruebas de esta especificación pueden usar una base SQLite temporal y el ejecutor disponible; no dependen de S5.4–S5.10.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [S5 → tarea S5.3](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una base SQLite temporal vacía y los scripts de Infrastructure. | Se aplican las migraciones disponibles mediante el ejecutor de S5.2 y se inspecciona el esquema resultante. | Existen las tablas y columnas necesarias para persistir las propiedades de las tres entidades, con nulabilidad compatible con sus invariantes; los identificadores y fechas siguen las convenciones de persistencia aprobadas. |
| CA02 | [domain-model.md → «Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad), [«User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link) y [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | El esquema de CA01. | Se inspeccionan las restricciones e índices únicos definidos en SQLite. | El esquema expresa las unicidades documentadas para email, alias, slug de colección por usuario y URL normalizada de enlace por usuario; admite los valores nulos de alias que corresponden a registros sin completar. |
| CA03 | [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [S5 → tarea S5.3](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | El esquema de CA01 y la conexión abierta por la fábrica de S5.1. | Se inspeccionan las claves foráneas e índices de soporte declarados. | La clave foránea compuesta de `Link` referencia conjuntamente el usuario y la colección, y el esquema contiene la clave única de soporte de `Collection`; la inspección confirma además la integridad referencial de los enlaces a usuarios. |
| CA04 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [S5 → tarea S5.3](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | El esquema de las tres entidades. | Se inspeccionan los índices no únicos del esquema. | Están representados los accesos previstos por usuario, colección, estado público y fecha; la selección y composición exactas de índices quedan justificadas en el plan técnico. |
| CA05 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | El esquema de CA01 ya aplicado. | Se inspecciona el DDL y el catálogo SQLite de restricciones y triggers. | Las relaciones no definen cascadas de actualización o borrado y el esquema no introduce triggers; las modificaciones y eliminaciones relacionadas permanecen explícitas en la aplicación. |

Los casos de violación funcional de unicidad y propiedad, las carreras y el orden alfabético se validan en S5.8; su evidencia no es prerrequisito para cerrar esta especificación.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno para redactar la especificación. La distribución física de columnas e índices se concretará en el plan, dentro de las propiedades e invariantes aprobadas. | [domain-model.md](../../../context/domain-model.md), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | Ninguno | Revisar y aprobar las propuestas técnicas al aprobar el plan. |

## Artefactos relacionados

[Plan técnico](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»). La implementación fue aceptada y cerrada por DLG el 19-X-2026 («Tenemos schema»); el estado y la evidencia constan en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
