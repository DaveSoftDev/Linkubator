# Especificación: S5.9 — Revisión manual de migraciones y esquema SQLite

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Aplicar el conjunto vigente de migraciones sobre una base SQLite vacía y revisar manualmente el SQL y el esquema resultante con una herramienta SQLite, dejando evidencia de que corresponden al diseño aprobado.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.9 y Definition of Done de Persistencia.
- [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia): criterio de finalización de la etapa y alcance pendiente de S5.9.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): convenciones del esquema, migraciones y garantías de persistencia.
- [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [«Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad): propiedades, relaciones e invariantes que debe reflejar el esquema.
- [S5.1 — Configuración de SQLite y Dapper](../s5.1-configuracion-sqlite-dapper/spec.md), [S5.2 — Ejecutor de scripts SQL versionados](../s5.2-ejecutor-scripts-sql-versionados/spec.md) y [S5.3 — Esquema de `User`, `Collection` y `Link`](../s5.3-esquema-user-collection-link/spec.md): configuración, migraciones y diseño de esquema aprobados.
- [S5.6 — Aprovisionamiento SQL de identidad de desarrollo](../s5.6-sql-local-identidad-desarrollo/spec.md): separación entre la revisión de migraciones/esquema y la repetición manual del aprovisionamiento local.
- [S5.8 — Tests de integración de persistencia](../s5.8-tests-integracion-persistencia/spec.md): límites de las pruebas automatizadas ya cubiertas.

## Alcance

- Incluye:
  - Aplicar el conjunto vigente de migraciones a una base SQLite vacía mediante el flujo disponible de la aplicación.
  - Revisar el SQL versionado y el esquema realmente aplicado con una herramienta SQLite, contrastándolos con el modelo y las decisiones de persistencia aprobadas.
  - Registrar la herramienta utilizada, la base inspeccionada, las migraciones aplicadas, los hallazgos y el resultado de la revisión humana.
- Excluye:
  - Cambiar reglas de producto, el modelo de dominio o las decisiones de arquitectura.
  - Repetir manualmente la aplicación del SQL local de aprovisionamiento; esa comprobación corresponde a S5.10.
  - Sustituir o repetir las pruebas automatizadas de integración de restricciones, transacciones, carreras y orden alfabético de S5.8.
  - Revisar las proyecciones FTS5 y los scripts de búsqueda de S6.
- Trabajo vecino y responsable de sus entregables:
  - S5.1–S5.8 proporcionan y validan los componentes técnicos; S5.10 comprueba la aplicación repetida del aprovisionamiento local. Estos trabajos no se vuelven a ejecutar ni se aceptan desde esta especificación.

## Dependencias y prerrequisitos

- S5.1–S5.8 están implementadas y aceptadas según sus artefactos; están disponibles la configuración SQLite, el ejecutor de migraciones y el esquema versionado.
- Se dispone de una base SQLite vacía aislada para la comprobación y de una herramienta capaz de inspeccionar el esquema aplicado.
- La comprobación se limita a las migraciones y al esquema de persistencia disponibles en esta etapa; no depende de S5.10 ni de S6.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [S5.2 → criterios de aceptación](../s5.2-ejecutor-scripts-sql-versionados/spec.md#criterios-de-aceptación) | Una base SQLite vacía y el conjunto vigente de migraciones versionadas. | Se ejecutan las migraciones mediante el flujo disponible y se inspecciona el registro de versiones y el estado final de la base. | Las migraciones se aplican sin error y la base queda en el estado esperado por el conjunto vigente, con el registro de versiones coherente con las migraciones aplicadas. |
| CA02 | [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [S5.3 → CA01](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación) | La base migrada de CA01 y el SQL versionado que la produjo. | Se inspeccionan con una herramienta SQLite las tablas, columnas, claves, nulabilidad, restricciones e índices, y se contrastan con el diseño aprobado. | El esquema aplicado y su SQL corresponden a las propiedades persistidas, convenciones, unicidades, relaciones e índices previstos por las fuentes; cualquier discrepancia queda registrada como hallazgo y bloquea la aceptación hasta resolverse. |
| CA03 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [S5.3 → CA03–CA05](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación) | El esquema aplicado de CA01. | Se revisan las definiciones de claves foráneas y el catálogo de objetos SQLite. | La clave foránea compuesta conserva la propiedad entre enlace y colección; el SQL no introduce cascadas ni triggers contrarios a la arquitectura. Los hallazgos y su resolución quedan registrados. |
| CA04 | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) y [S5.1 → criterios de aceptación](../s5.1-configuracion-sqlite-dapper/spec.md#criterios-de-aceptación) | El SQL versionado y la base SQLite inspeccionada. | Se completa la revisión humana de esta etapa y se registra su evidencia. | La evidencia identifica la herramienta y versión, la base y migraciones comprobadas, los hallazgos y su resolución; DLG puede decidir explícitamente la aceptación de S5.9 sin depender de S5.10 ni de trabajo posterior. |

Los tests de imposición efectiva de restricciones, carreras y orden alfabético son responsabilidad de S5.8; esta revisión manual no los sustituye. La configuración activa de cada conexión se valida según S5.1 y no se infiere únicamente de la inspección del archivo SQLite.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno conocido para redactar la especificación. La herramienta SQLite concreta y el formato de evidencia se precisarán en el plan técnico sin alterar el alcance aprobado. | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Ninguno | Aprobar la especificación y revisar las propuestas del plan técnico. |

## Artefactos relacionados

El plan original y su revisión de V01/V02 fueron aprobados por DLG («Siguiente», 19-X-2026). La exclusión de `V001`–`V003` se implementó y aceptó en [S5.2.1](../s5.2.1-separar-scripts-prueba-catalogo/tasks.md#aceptación-y-cierre) («Completada», 19-X-2026). Las tareas originales de S5.9 fueron aprobadas por DLG («Aprobado», 19-X-2026) y re-aprobadas tras la revisión de V01/V02 con «Aprobado», el 19-X-2026. V01 pasó 1/1, V02 5/5 y V004 se revisó sin discrepancias; queda pendiente la aceptación humana del resultado.