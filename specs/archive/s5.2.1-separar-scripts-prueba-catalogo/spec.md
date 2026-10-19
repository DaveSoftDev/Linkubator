# Especificación: S5.2.1 — Separar scripts de prueba del catálogo de migraciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Evitar que los scripts SQL de prueba `V001`–`V003`, que crean y modifican `sample_data`, formen parte del catálogo aplicado por la aplicación, manteniendo la cobertura automatizada del ejecutor con scripts de prueba aislados.

## Fuentes

- [S5.2 — Ejecutor de scripts SQL versionados → «Alcance»](../s5.2-ejecutor-scripts-sql-versionados/spec.md#alcance) y [«Criterios de aceptación»](../s5.2-ejecutor-scripts-sql-versionados/spec.md#criterios-de-aceptación): el ejecutor admite scripts de prueba para validar su comportamiento, separados del esquema de entidades.
- [S5.2 → plan técnico → «Componentes y contratos»](../s5.2-ejecutor-scripts-sql-versionados/plan.md#componentes-y-contratos) y [«Estrategia de validación»](../s5.2-ejecutor-scripts-sql-versionados/plan.md#estrategia-de-validación): los tests crean su propio conjunto de scripts para comprobar orden, repetición y fallo.
- [S5.3 — Esquema de `User`, `Collection` y `Link` → CA01](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación): esquema versionado esperado para la base de persistencia.
- [S5.9 — Revisión manual de migraciones y esquema SQLite → «Decisiones técnicas y riesgos»](../s5.9-revision-manual-migraciones-esquema/plan.md#decisiones-técnicas-y-riesgos): decisión de DLG de excluir `V001`–`V003` del catálogo de producción («Excluir», 19-X-2026) y prerrequisito para la revisión manual.
- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): alcance de S5.2, S5.3 y Definition of Done de Persistencia.

## Alcance

- Incluye:
  - Separar los scripts de prueba `V001`–`V003` del conjunto SQL que el catálogo de Infrastructure descubre y aplica en la base de la aplicación.
  - Conservar las pruebas automatizadas del ejecutor para orden, idempotencia y fallo usando scripts de prueba ajenos al catálogo de producción.
  - Ajustar las comprobaciones que enumeran las versiones de producción para que reflejen el catálogo resultante.
  - Comprobar mediante la fábrica real de migraciones que una base vacía obtiene el esquema aprobado sin crear `sample_data`.
- Excluye:
  - Cambiar el comportamiento del ejecutor, su historial, ordenación o límites transaccionales.
  - Cambiar el esquema de `User`, `Collection` y `Link` o añadir migraciones de dominio.
  - Ejecutar la revisión manual del esquema, que corresponde a S5.9, o repetir el aprovisionamiento local, que corresponde a S5.10.
- Trabajo vecino y responsable de sus entregables:
  - S5.2 conserva la responsabilidad del ejecutor; S5.3, del esquema; S5.8, de las pruebas integradas de persistencia; S5.9, de la revisión manual del esquema aplicado. Esta especificación corrige únicamente la separación entre fixtures de prueba y catálogo de producción.

## Dependencias y prerrequisitos

- S5.2 y S5.3 están implementadas y aceptadas según sus artefactos; existe el catálogo de recursos incrustados y el esquema versionado.
- S5.8 está implementada y aceptada; sus bases temporales usan el catálogo real y permiten verificar que las integraciones continúan aplicando el esquema.
- DLG decidió excluir `V001`–`V003` del catálogo de producción en la conversación del 19-X-2026 («Excluir»), registrada en el plan aprobado de S5.9.
- S5.9 está aprobada en especificación y plan, pero sus tareas no están aprobadas y la revisión no ha comenzado; esta corrección debe completarse y aceptarse antes de iniciar S5.9.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S5.9 → decisión sobre `V001`–`V003`](../s5.9-revision-manual-migraciones-esquema/plan.md#decisiones-técnicas-y-riesgos), [S5.2 → «Alcance»](../s5.2-ejecutor-scripts-sql-versionados/spec.md#alcance) y [mvp0-plan.md → tarea S5.2](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | El ensamblado de Infrastructure y su catálogo de migraciones de producción. | Se cargan las migraciones disponibles para aplicar a la base de la aplicación. | El catálogo no contiene `V001`, `V002` ni `V003`, ni se aplican sentencias que creen o modifiquen `sample_data`. |
| CA02 | [S5.2 → CA01–CA03](../s5.2-ejecutor-scripts-sql-versionados/spec.md#criterios-de-aceptación) y [S5.2 → plan técnico → «Estrategia de validación»](../s5.2-ejecutor-scripts-sql-versionados/plan.md#estrategia-de-validación) | Los tests del ejecutor con scripts suministrados directamente al servicio. | Se ejecutan las pruebas de orden, aplicación repetida y fallo de una migración. | Las tres garantías siguen comprobándose sin depender de que los scripts de prueba estén incrustados en el ensamblado de producción. |
| CA03 | [S5.3 → CA01](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación), [S5.8 → CA01](../s5.8-tests-integracion-persistencia/spec.md#criterios-de-aceptación) y [mvp0-plan.md → tarea S5.3](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una base SQLite temporal vacía y el catálogo de producción. | Se aplican las migraciones mediante `SqliteMigrationService` y se comprueban las tablas e historial resultantes. | La base contiene el esquema aprobado de `User`, `Collection` y `Link`, no contiene `sample_data`, y el historial solo registra las versiones del catálogo aprobado. |
| CA04 | [S5.8 → «Dependencias y prerrequisitos»](../s5.8-tests-integracion-persistencia/spec.md#dependencias-y-prerrequisitos), [S5.9 → «Dependencias y prerrequisitos»](../s5.9-revision-manual-migraciones-esquema/spec.md#dependencias-y-prerrequisitos) y [mvp0-plan.md → tarea S5.2](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Los tests de Infrastructure que consumen el catálogo real, incluidas las comprobaciones de esquema e identidad. | Se ejecutan las pruebas focalizadas afectadas y las pruebas de integración SQLite que usan las migraciones reales. | Las comprobaciones esperan el conjunto de versiones aprobado y siguen verificando el esquema y el aprovisionamiento sin recurrir a migraciones de muestra en producción. |

CA01 y CA03 incluyen el caso negativo de que `sample_data` no se cree en una base nueva. CA02 conserva la prueba de fallo del ejecutor sin exigir que el catálogo de producción contenga scripts defectuosos.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno conocido para redactar la especificación. La decisión de excluir `V001`–`V003` ya fue confirmada por DLG; el plan concretará cómo mantener sus casos de prueba separados del catálogo. | [S5.9 → «Decisiones técnicas y riesgos»](../s5.9-revision-manual-migraciones-esquema/plan.md#decisiones-técnicas-y-riesgos) | Ninguno | Aprobar esta especificación y revisar la propuesta del plan técnico. |

## Artefactos relacionados

El [plan técnico](plan.md) fue aprobado por DLG el 19-X-2026 («Siguiente»). Las [tareas](tasks.md) están preparadas y pendientes de aprobación.