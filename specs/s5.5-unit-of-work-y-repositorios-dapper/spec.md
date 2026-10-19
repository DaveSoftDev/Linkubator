# Especificación: S5.5 — Unit of Work y repositorios Dapper

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Implementar en Infrastructure la unidad de trabajo SQLite y los repositorios Dapper requeridos por Application, para que las operaciones de datos usen el esquema aprobado, respeten la identidad recibida y puedan coordinar escrituras atómicas mediante una transacción `BEGIN IMMEDIATE`.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.5, trabajos vecinos y Definition of Done.
- [roadmap.md → «Persistencia»](../../context/roadmap.md#persistencia): objetivos y criterio de finalización de persistencia.
- [architecture.md → «Infrastructure»](../../context/architecture.md#infrastructure), [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) y [«Persistencia»](../../context/architecture.md#persistencia): responsabilidades, ciclo de acceso, transacciones y garantías técnicas.
- [architecture.md → «FTS5»](../../context/architecture.md#fts5): límites de las proyecciones y consultas de búsqueda asignadas a S6.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [«Público y privado»](../../context/domain-model.md#público-y-privado) y [«Collection»](../../context/domain-model.md#collection): invariantes que deben preservar los accesos a datos.
- [specifications.md → «Listados»](../../context/specifications.md#listados), [«Páginas públicas»](../../context/specifications.md#páginas-públicas) y [«Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas): orden y selección de resultados.
- [Catálogo de criterios de aceptación → «Colecciones»](../../plans/mvp0-acceptance-criteria.md#colecciones), [«Enlaces»](../../plans/mvp0-acceptance-criteria.md#enlaces) y [«Dashboard y páginas públicas»](../../plans/mvp0-acceptance-criteria.md#dashboard-y-páginas-públicas): comportamiento aprobado que los repositorios concretos sirven a Application.
- [S4.2 — contratos de repositorios, Unit of Work e identidad identificada](../archive/s4.2-contratos-repositorios-identidad/spec.md): contratos de persistencia propietarios.
- [S5.1 — configuración de SQLite y Dapper](../s5.1-configuracion-sqlite-dapper/spec.md), [S5.2 — ejecutor de scripts SQL versionados](../s5.2-ejecutor-scripts-sql-versionados/spec.md), [S5.3 — esquema de `User`, `Collection` y `Link`](../s5.3-esquema-user-collection-link/spec.md) y [S5.4 — colación `es-ES`](../s5.4-colacion-es-es/spec.md): mecanismos y esquema requeridos por este trabajo.
- [S4.6 — casos de uso de colecciones](../archive/s4.6-casos-uso-colecciones/spec.md), [S4.7 — casos de uso de enlaces](../archive/s4.7-casos-uso-enlaces/spec.md) y [S4.8 — dashboard y lecturas públicas](../archive/s4.8-dashboard-y-lecturas-publicas/spec.md): consumidores de los repositorios y sus criterios aprobados.

## Alcance

- Incluye:
  - Implementar `IUnitOfWork` e `IUnitOfWorkFactory` en Infrastructure sobre el mecanismo común de conexiones SQLite.
  - Implementar y registrar los repositorios concretos de los contratos de colecciones, enlaces, dashboard y lecturas públicas de Application.
  - Hacer que los repositorios obtenidos de una unidad de trabajo compartan su conexión y transacción; las lecturas y operaciones de repositorio fuera de una unidad usarán el mecanismo común de conexión.
  - Persistir y materializar `User`, `Collection` y `Link` según el esquema aprobado de S5.3, incluyendo las lecturas privadas y públicas permitidas por los contratos y criterios propietarios.
  - Aplicar la colación registrada por S5.4 en las consultas alfabéticas de repositorios que la requieran.
  - Mantener explícitos en la aplicación los cambios relacionados, sin trasladar reglas de negocio a SQL ni depender de cascadas o triggers.
- Excluye:
  - Cambiar contratos de Application, reglas de producto, criterios aprobados o esquema de S5.3.
  - Crear las proyecciones FTS5, sincronizarlas o implementar la búsqueda textual no vacía; corresponde a S6. Hasta entonces, esta especificación solo acepta y comprueba las operaciones de listado sin texto de búsqueda. La implementación no debe presentar una consulta temporal distinta como búsqueda aprobada.
  - El SQL local de desarrollo y el proveedor de identidad; corresponden a S5.6 y S5.7.
  - La validación integrada de las restricciones, las carreras de negocio y el orden completo de listados asignada a S5.8; la revisión manual del esquema aplicado asignada a S5.9.
  - La implementación o modificación de casos de uso de Application. Las garantías concurrentes de extremo a extremo se comprobarán en el límite correspondiente y no se atribuyen a la mera existencia del Unit of Work.
- Trabajo vecino y responsable de sus entregables:
  - S5.1–S5.4 proporcionan conexión, migraciones, esquema y colación; S5.6–S5.10 conservan sus entregables propios según el [plan MVP0 → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l).
  - S6 implementa las proyecciones y la consulta de búsqueda textual; S5.8 valida el comportamiento integrado de persistencia y las carreras contempladas por el roadmap.
  - S4.6–S4.8 son consumidores ya implementados; S5.5 no cambia sus reglas funcionales ni presume cubiertas por sus tests de Application las garantías de SQLite.

## Dependencias y prerrequisitos

- S4.2 está completada y aceptada; los contratos de repositorio y Unit of Work están disponibles en Application.
- S4.6–S4.8 están completadas y aceptadas; sus casos de uso y criterios aprobados constituyen el contrato de comportamiento que consumen los repositorios.
- S5.1–S5.4 están implementadas y aceptadas; proveen conexión configurada, migrador, esquema y colación.
- La validación de esta especificación utilizará una base SQLite temporal con las migraciones aprobadas aplicadas. No requiere S5.6, S5.7, S5.8, S6 ni Web.
- Las consultas de listado reciben `query` por los contratos de Application, mientras que el plan MVP0 asigna FTS5 a S6. La decisión de DLG «Vale, lo dejamos para S6» confirma que S5.5 no implementa búsqueda textual ni una alternativa temporal; la comprobación de esta etapa se limita a consultas sin términos.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Infrastructure»](../../context/architecture.md#infrastructure), [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos), [S4.2 → CA01, CA02 y CA07](../archive/s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación) y [S5 → tarea S5.5](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | El contenedor de servicios de la aplicación y los contratos de repositorio/UoW de Application. | Se resuelven las implementaciones concretas registradas por Infrastructure y se inspecciona la dirección de dependencias. | Se resuelven la fábrica y las implementaciones de todos los repositorios requeridos por los casos de uso MVP0, sin introducir dependencias de SQLite o Dapper en Application ni romper la matriz de referencias. |
| CA02 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [S4.2 → CA05](../archive/s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación) y [S5 → tarea S5.5](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una unidad de trabajo obtenida de la fábrica y una base SQLite temporal. | Se inicia la unidad para escritura y se ejecutan operaciones por más de un repositorio obtenido de ella. | El inicio usa semántica `BEGIN IMMEDIATE`; los repositorios de la unidad usan la misma conexión y transacción, y las operaciones quedan dentro de esa transacción hasta confirmar o revertir. |
| CA03 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [S5 → tarea S5.5](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una unidad de trabajo con escrituras ejecutadas por sus repositorios. | Se confirma, se revierte explícitamente o se dispone sin completar una confirmación. | La confirmación conserva las escrituras; la reversión y la disposición de una unidad no confirmada no dejan escrituras parciales, y en todos los casos se liberan la transacción, conexión y recursos de la unidad. |
| CA04 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [AC-COL-05](../../plans/mvp0-acceptance-criteria.md#ac-col-05--listar-colecciones-del-usuario) y [AC-LINK-06](../../plans/mvp0-acceptance-criteria.md#ac-link-06--listar-enlaces-del-usuario) | Una base temporal con usuarios distintos y colecciones/enlaces con identificadores coincidentes cuando el esquema lo permita. | Se usan las operaciones privadas de lectura, listado, actualización y borrado pasando el `userId` del contrato. | Cada operación queda acotada al `userId` recibido: no devuelve, actualiza ni elimina datos de otro usuario, incluso si un identificador de recurso coincide. |
| CA05 | [AC-COL-01, AC-COL-02 y AC-COL-03](../../plans/mvp0-acceptance-criteria.md#colecciones), [AC-LINK-01–AC-LINK-05](../../plans/mvp0-acceptance-criteria.md#enlaces), [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [S5.3 → CA01–CA05](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación) | Entidades de `User`, `Collection` y `Link` válidas según sus contratos y una base migrada. | Los repositorios insertan, consultan, actualizan y eliminan esas entidades usando sus operaciones contratadas. | Los datos materializados conservan las propiedades del esquema y los cambios explícitos se reflejan en SQLite; las operaciones no sustituyen las restricciones del esquema por lógica de persistencia implícita ni dependen de cascadas o triggers. |
| CA06 | [AC-DASH-01, AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#dashboard-y-páginas-públicas), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado), [specifications.md → «Páginas públicas»](../../context/specifications.md#páginas-públicas) y [«Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas) | Datos que incluyen registros privados, públicos, vacíos y de más de una página. | Se consultan dashboard y lecturas públicas mediante los contratos de repositorio. | Los resultados respetan la selección, privacidad, orden y paginación de los criterios aprobados, sin exponer registros privados mediante una lectura pública. |
| CA07 | [specifications.md → «Listados»](../../context/specifications.md#listados), [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [S5.4 → CA01–CA03](../s5.4-colacion-es-es/spec.md#criterios-de-aceptación) y [AC-COL-05](../../plans/mvp0-acceptance-criteria.md#ac-col-05--listar-colecciones-del-usuario) | Colecciones o enlaces listables cuyos textos requieran el orden alfabético definido. | Se solicitan listados alfabéticos sin texto de búsqueda desde sus repositorios. | Las consultas declaran la colación `LINKUBATOR_ES_ES` en el orden alfabético y respetan el desempate por identificador definido por la fuente propietaria. |
| CA08 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [«FTS5»](../../context/architecture.md#fts5) y [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Los contratos de listado que reciben una consulta textual y la frontera de responsabilidades S5/S6. | Se revisan las consultas del repositorio y sus pruebas en S5.5. | Esta especificación no declara ni prueba búsqueda textual FTS5 ni una implementación sustitutiva; los criterios de S5.5 cubren solo las variantes sin términos, y la consulta de términos queda para S6 sin cambiar el contrato ni las reglas propietarias. |

Los criterios de propiedad y aislamiento aquí verifican las implementaciones de repositorio, no sustituyen las pruebas integradas de restricciones, carreras concretas y orden completo asignadas a S5.8. El criterio de atomicidad de CA03 comprueba el ciclo de vida de la unidad con un fallo dentro de la transacción; no afirma que todos los flujos de Application ya sitúen sus lecturas de validación dentro de ella.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| `LinkService.CreateLink` comprueba actualmente colección/visibilidad y duplicidad antes de iniciar su unidad de trabajo. La arquitectura exige que las comprobaciones sujetas a carrera y su escritura estén dentro de la transacción `BEGIN IMMEDIATE`; un repositorio/UoW no puede garantizarlo si el consumidor realiza dichas lecturas fuera de la unidad. | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [AC-LINK-01b](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público) y [S4.7 → CA01](../archive/s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación) | No bloquea los criterios de componentes CA01–CA08; bloquea afirmar que S5.5 por sí sola prueba la carrera de creación pública | Resolver el ajuste de Application en el trabajo propietario antes de atribuirle la garantía transaccional y cubrir la carrera en la validación integrada de S5.8. No ampliar silenciosamente el alcance de S5.5 para cambiar un caso de uso. |

## Artefactos relacionados

[Plan técnico](plan.md) y [tareas](tasks.md), derivados de esta especificación aprobada. El estado de implementación y la evidencia actual constan en las tareas; no implican aceptación ni cierre.
