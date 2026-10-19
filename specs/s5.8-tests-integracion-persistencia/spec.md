# Especificación: S5.8 — Tests de integración de persistencia

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Comprobar con una base SQLite temporal que las restricciones, las operaciones concurrentes y los listados alfabéticos de persistencia conservan las garantías aprobadas en las fronteras integradas de Application, Infrastructure y SQLite.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): tarea S5.8 y Definition of Done.
- [roadmap.md → «Persistencia»](../../context/roadmap.md#persistencia): criterio de finalización y responsabilidad de S5.8.
- [architecture.md → «Persistencia»](../../context/architecture.md#persistencia): garantías finales de SQLite, `BEGIN IMMEDIATE` y reglas que deben comprobarse dentro de la transacción.
- [specifications.md → «Listados»](../../context/specifications.md#listados): órdenes y desempates propietarios de los listados.
- [domain-model.md → «Restricciones de unicidad»](../../context/domain-model.md#restricciones-de-unicidad), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): restricciones e invariantes comprobadas.
- [Catálogo de criterios de aceptación → «Colecciones»](../../plans/mvp0-acceptance-criteria.md#colecciones) y [«Enlaces»](../../plans/mvp0-acceptance-criteria.md#enlaces): comportamiento aprobado de eliminación de colecciones, publicación y movimiento de enlaces.
- [S5.3 — esquema de `User`, `Collection` y `Link`](../s5.3-esquema-user-collection-link/spec.md): restricciones únicas y clave foránea compuesta implementadas por el esquema.
- [S5.4 — colación `es-ES`](../s5.4-colacion-es-es/spec.md): registro y disponibilidad de la colación.
- [S5.5 — Unit of Work y repositorios Dapper](../s5.5-unit-of-work-y-repositorios-dapper/spec.md): contratos integrados de repositorio y transacción; reserva explícita de estas comprobaciones para S5.8.
- [S5.7 — proveedor de identidad de desarrollo](../s5.7-proveedor-identidad-desarrollo/spec.md): límites de validación integrada asignados a este trabajo.

## Alcance

- Incluye:
  - Añadir tests de integración que usen una base SQLite temporal con las migraciones aplicadas.
  - Comprobar las restricciones únicas de `User`, `Collection` y `Link` y la clave foránea compuesta que impide asociar un enlace a una colección de otro usuario.
  - Comprobar operaciones concurrentes de eliminación de la última colección y de hacer privada una colección mientras se intenta hacer público uno de sus enlaces.
  - Comprobar la creación concurrente de un enlace público mientras se hace privada su colección y ajustar el flujo de Application si es necesario para que las validaciones y escrituras respeten la transacción.
  - Comprobar que mover un enlace a una colección ajena se rechaza en Application y que SQLite conserva la garantía final ante una relación manipulada directamente.
  - Comprobar el orden completo y los desempates por identificador de los listados alfabéticos de colecciones y enlaces cubiertos por los repositorios disponibles.
- Excluye:
  - Cambiar reglas de producto, contratos de Application o esquema SQL. Los ajustes de implementación de Application necesarios para cumplir las garantías transaccionales existentes sí forman parte del alcance.
  - Implementar FTS5 o validar resultados de búsquedas con texto, que corresponden a S6.
  - Repetir las pruebas unitarias de colación, ciclo de vida de Unit of Work o repositorios ya cubiertas por S5.1–S5.5; aquí se comprueba su comportamiento integrado.
  - Ejecutar la revisión manual del esquema y del script local, asignada a S5.9 y S5.10.
- Trabajo vecino y responsable de sus entregables:
  - S5.1–S5.5 proporcionan la conexión, el esquema, la colación, la transacción y los repositorios; S5.6–S5.7 proporcionan el aprovisionamiento y la identidad de desarrollo.
  - S5.9 revisa manualmente el SQL y el esquema aplicado; S5.10 revisa la aplicación repetida del aprovisionamiento local.
  - S6 implementa las proyecciones FTS5 y la búsqueda textual.

## Dependencias y prerrequisitos

- S5.1–S5.7 están implementadas y aceptadas según sus artefactos; están disponibles la fábrica SQLite, el esquema, la colación, los repositorios y los flujos necesarios para preparar las operaciones.
- Los criterios de los casos de uso de colecciones, enlaces y lecturas cubiertos por los tests están aprobados y disponibles en el catálogo de aceptación.
- Las pruebas pueden crear una base aislada, aplicar las migraciones vigentes y controlar los datos iniciales sin depender de una base local persistente, de S5.9, S5.10 ni de S6.
- La cobertura de orden se limita a variantes sin búsqueda textual; los órdenes concretos proceden de [specifications.md → «Listados»](../../context/specifications.md#listados).
- S5.5 registra que una comprobación de `LinkService.CreateLink` ocurre antes de iniciar la unidad de trabajo; CA07 incluye esta ruta en la validación integrada y cubre el ajuste de implementación si hace falta.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «Restricciones de unicidad»](../../context/domain-model.md#restricciones-de-unicidad), [S5.3 → CA02](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación) y [S5 → tarea S5.8](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una base temporal con el esquema aprobado y datos de `User`, `Collection` y `Link`. | Los tests intentan guardar valores duplicados para las claves únicas aplicables a esas entidades y, además, más de un alias nulo para cuentas incompletas. | SQLite rechaza los duplicados definidos por el esquema y permite los alias nulos admitidos; los tests verifican el estado persistido y no dependen solo de validaciones previas de Application. |
| CA02 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [S5.3 → CA03](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación) | Una base temporal con dos usuarios y colecciones pertenecientes a usuarios distintos. | Se intenta insertar directamente un enlace cuya pareja de usuario y colección no corresponde. | La clave foránea compuesta impide persistir la relación ajena y la base conserva su estado previo. |
| CA03 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [AC-COL-04b](../../plans/mvp0-acceptance-criteria.md#ac-col-04b--eliminar-una-colección-con-enlaces-o-la-última-colección) y [S5 → tarea S5.8](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Un usuario con dos colecciones que intenta eliminar ambas concurrentemente desde operaciones independientes. | Las dos operaciones compiten por eliminar la última colección restante. | Como resultado, el usuario conserva al menos una colección; la operación que no puede respetar la regla se rechaza sin dejar escrituras parciales. |
| CA04 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado), [AC-COL-03](../../plans/mvp0-acceptance-criteria.md#colecciones), [AC-LINK-04](../../plans/mvp0-acceptance-criteria.md#enlaces) y [S5 → tarea S5.8](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una colección pública con un enlace privado y dos operaciones concurrentes: hacer privada la colección y hacer público el enlace. | Las operaciones se ejecutan concurrentemente mediante los flujos de Application. | El estado confirmado no contiene un enlace público en una colección privada; los efectos relacionados se mantienen atómicos y cualquier rechazo deja intacto el estado previo de esa operación. |
| CA05 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AC-LINK-03](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace), [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [S5.3 → CA03](../s5.3-esquema-user-collection-link/spec.md#criterios-de-aceptación) | Un enlace de un usuario y una colección destino de otro usuario. | Se intenta mover el enlace mediante Application y se repite la operación manipulando directamente la relación en SQLite. | Application rechaza el movimiento sin modificar el enlace y la clave foránea compuesta impide persistir la relación manipulada. |
| CA06 | [specifications.md → «Listados»](../../context/specifications.md#listados), [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [S5.4 → CA01–CA03](../s5.4-colacion-es-es/spec.md#criterios-de-aceptación), [S5.5 → CA06–CA07](../s5.5-unit-of-work-y-repositorios-dapper/spec.md#criterios-de-aceptación) y [S5 → tarea S5.8](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Datos temporales que cubren empates lingüísticos y empates de clave en los listados privados de colecciones, públicos de colecciones y públicos de enlaces de una colección. | Se consultan esos listados completos sin términos de búsqueda a través de los repositorios correspondientes. | Los nombres de colecciones y el texto mostrado de los enlaces respetan el orden lingüístico y el desempate por `Id` definidos por la fuente propietaria; las pruebas ejercitan cada consulta integrada, no solo la función de comparación de la colación. |
| CA07 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado), [AC-COL-03](../../plans/mvp0-acceptance-criteria.md#colecciones), [AC-LINK-01b](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público) y [S5.5 → «Bloqueos y preguntas pendientes»](../s5.5-unit-of-work-y-repositorios-dapper/spec.md#bloqueos-y-preguntas-pendientes) | Una colección pública y la operación de crear en ella un enlace público, concurrente con la operación que hace privada esa colección. | Ambas operaciones se ejecutan mediante Application y compiten por la transacción de escritura. | El resultado equivale a un orden serial válido: si la creación se confirma primero, hacer privada la colección deja privados sus enlaces; si se confirma primero la privatización, la creación pública se rechaza. Nunca queda confirmado un enlace público en una colección privada ni una escritura parcial. |

Los casos negativos y las condiciones de carrera se cubren en CA01–CA05 y CA07. CA06 no incorpora búsqueda FTS5 ni redefine los valores de orden de los listados.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno conocido. La observación de S5.5 sobre `LinkService.CreateLink` queda incluida en el alcance y CA07; el plan deberá concretar la coordinación reproducible de las operaciones concurrentes. | [S5.5 → «Bloqueos y preguntas pendientes»](../s5.5-unit-of-work-y-repositorios-dapper/spec.md#bloqueos-y-preguntas-pendientes), [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Ninguno | Ninguna decisión funcional pendiente. |

## Artefactos relacionados

El [plan técnico](plan.md) está aprobado por DLG («Siguiente», 19-X-2026). Las [tareas](tasks.md) están preparadas y pendientes de aprobación.