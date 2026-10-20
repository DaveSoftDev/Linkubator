# Plan técnico: S6.4 — Sincronización de proyecciones FTS5

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Mantener la responsabilidad en los repositorios SQLite de Infrastructure: `SqliteLinkRepository` sincroniza la fila de `LinkSearch` y `SqliteCollectionRepository` la de `CollectionSearch`.
- La proyección de enlaces se forma con los campos que asigna la arquitectura. Para obtener el nombre actual de la colección al crear, editar o mover enlaces, la escritura debe resolverlo a partir del `CollectionId` de la propia entidad dentro del contexto de persistencia, no confiar en un nombre suministrado por la petición.
- Las escrituras de las entidades y las proyecciones deben ejecutarse con la misma conexión y transacción de `SqliteUnitOfWork`. No introducir triggers ni otra ruta de escritura automática.
- Usar `SearchTextNormalizer` de Infrastructure para normalizar a NFC solo los valores enviados a las proyecciones. No reemplazar ni mutar los valores de las entidades.
- Conservar los contratos de Application existentes. La propagación de fallos SQL debe permitir que la operación coordinada por el caso de uso revierta la transacción completa.

### Flujos internos

1. El caso de uso inicia la unidad de trabajo y la transacción antes de realizar la operación compuesta, siguiendo el patrón de escritura existente.
2. En una inserción de enlace, persistir la entidad, obtener su `Id` y crear su fila de búsqueda con `rowid` correspondiente, `UserId` y campos buscables en NFC.
3. En una actualización de enlace, reflejar en la fila de búsqueda el contenido persistido, incluyendo el nombre actual de la colección; una modificación de solo campos no buscables no debe dejar diferencias entre las fuentes.
4. En creación de colección, crear la entidad y su proyección después de obtener el `Id`. En renombrado, actualizar la proyección de la colección y las filas de enlaces que pertenecen a esa colección dentro de la misma transacción.
5. En eliminación, eliminar explícitamente la fila de proyección junto con la entidad, respetando la regla de no borrar una colección que aún contiene enlaces.
6. Si cualquier paso falla, la unidad de trabajo revierte tanto las modificaciones de entidad como las de proyección. Los tests inyectan el fallo en el camino entre ambas clases de escritura, sin introducir una interfaz productiva de fallos.

### Persistencia y dependencias

- No se modifica el esquema ni se añaden migraciones: S6.1 ya dispone `LinkSearch` y `CollectionSearch`.
- No se usan triggers. El repositorio escribe explícitamente las proyecciones, y la transacción existente garantiza atomicidad junto a los datos de dominio.
- Las inserciones necesitan el identificador asignado por SQLite. Las actualizaciones y eliminaciones deben limitarse al identificador y usuario de la entidad afectada, con los vínculos establecidos por el esquema.
- La actualización por renombrado de colección afecta a su fila en `CollectionSearch` y a las filas de `LinkSearch` cuyos enlaces referencian esa colección; no requiere reescribir campos de búsqueda ajenos a esa operación.
- Reutilizar .NET, Dapper, SQLite, los repositorios y la infraestructura de tests existente; no se anticipan nuevos paquetes.
- Prerrequisito: confirmar la aceptación renovada de S6.2 antes de implementar el consumo de su normalizador en escrituras, de acuerdo con el bloqueo registrado en [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos).

### Garantías técnicas

- En cada commit, las filas de las proyecciones reflejan las entidades confirmadas según [architecture.md → «FTS5»](../../context/architecture.md#fts5); los valores originales permanecen inalterados.
- Ninguna escritura de proyección se confirma separada de la operación de entidad. Un fallo antes del commit revierte ambos lados según [architecture.md → «Persistencia»](../../context/architecture.md#persistencia).
- Los filtros de usuario y entidad de las operaciones de búsqueda se mantienen como responsabilidad de S6.5; S6.4 no altera la visibilidad ni introduce lecturas de búsqueda.
- Las pruebas de S6.4 comprueban persistencia y atomicidad con SQLite, pero no prueban consultas de listado, orden, paginación, relevancia, aislamiento integrado de consultas ni el comando de reconstrucción de S6.6. La cobertura integrada conjunta se asigna a S6.7.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Escribir las proyecciones explícitamente desde los repositorios, sin triggers. | confirmado | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [architecture.md → «FTS5»](../../context/architecture.md#fts5) | Mantiene la lógica de escrituras relacionadas en la aplicación y bajo la transacción del Unit of Work. |
| Mantener la sincronización en las mismas operaciones de repositorio y unidad de trabajo, sin un servicio de sincronización separado. | propuesta | Repositorios SQLite existentes y contratos del Unit of Work; validar en la revisión del plan y al inspeccionar los tests de persistencia. | Reduce coordinación adicional; verificar que la operación de renombrado mantiene todos los cambios agrupados y que los fallos se propagan para rollback. |
| Actualizar las filas de enlaces asociadas durante el renombrado de colección en la misma transacción. | confirmado | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [mvp0-plan.md → S6.4](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Evita que la búsqueda de enlaces conserve un nombre obsoleto; la prueba debe incluir varios enlaces y otra colección como control negativo. |
| Confirmar la aceptación renovada de S6.2 antes de integrar la normalización en las escrituras. | pendiente | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos), tareas S6.2 | CA03 no se implementa antes de resolver el estado del prerrequisito; evitar registrar como aprobación un test técnico. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: integración SQLite de las operaciones de enlace, comprobando fila indexada tras inserción, edición de campos buscables y movimiento entre colecciones. | Aceptación de S6.2; repositorios, Unit of Work y fixture SQLite. | Detectar el filtro existente de tests de repositorios/integración y añadir pruebas focalizadas al patrón hallado; comando exacto por registrar en tareas antes de ejecutar. | La fila de `LinkSearch` corresponde al `Id` y `UserId` del enlace y refleja los valores actuales, incluida la colección destino; los valores de entidad son los mismos que antes de indexar. | Salida de pruebas, escenarios cubiertos y versión SQLite/.NET. |
| V02 | CA02 | Automatizada: integración SQLite de creación y renombrado de colección, con varios enlaces en esa colección y al menos un enlace en otra colección. | V01 y esquema de S6.1. | Mismo proyecto de pruebas; identificar el test/filtro concreto durante la tarea de inspección. | `CollectionSearch` refleja el nombre vigente; solo los enlaces de la colección renombrada reflejan ese nombre en `LinkSearch`. | Salida de pruebas y datos de fixture no sensibles. |
| V03 | CA03 | Automatizada: pruebas de persistencia con pares de texto NFC/descompuesto en campos proyectados. | Aceptación de S6.2 y V01–V02. | Tests focalizados del repositorio y lectura de las filas FTS/proyección para verificar el contenido almacenado y la entidad original. | La representación de proyección está en NFC y las propiedades de la entidad conservan la cadena original. | Salida de tests y valores de entrada de prueba. |
| V04 | CA04 | Automatizada: integración de eliminación de enlace y de una colección eliminable. | Esquema S6.1 y fixtures con colección vacía. | Tests focalizados de persistencia, incluyendo comprobación directa de que no queda fila asociada en cada proyección. | Entidad y fila FTS desaparecen juntas; no se cubren aquí los rechazos funcionales ajenos al borrado permitido. | Salida de pruebas y consultas de aserción. |
| V05 | CA05 | Automatizada: fallo inyectado dentro de la operación de persistencia tras una escritura de entidad y antes de completar su proyección, incluyendo renombrado de colección con enlaces. | V01–V02 y mecanismo de pruebas SQLite existente que permita forzar una excepción en el límite de persistencia. | Usar el punto de inyección ya disponible si existe; si no, optar por restricción/función SQLite de prueba u otro mecanismo local, sin añadir puntos de fallo a producción. | La operación informa el fallo y el rollback deja entidad y todas las proyecciones en el estado previo; test negativo falla si se confirma solo una de las partes. | Salida de prueba, punto de inyección y comparación antes/después. |
| V06 | CA01–CA05 | Revisión técnica/documental del diff, trazabilidad y transacciones. | V01–V05. | Tests focalizados descubiertos durante T01; `git diff --check`. | Todos los criterios tienen evidencia; no hay triggers, SQL de migración innecesario ni cambios en reglas fuera de alcance. | Salida de comandos, archivos revisados y fecha. |

Las comprobaciones de esta spec no sustituyen los escenarios de consulta, propiedad y búsqueda integrada de S6.7. La aserción de sincronización debe leer las filas subyacentes, pues una búsqueda del motor no es necesaria para demostrar por sí sola que se almacenó el contenido esperado. Las validaciones no deben registrar valores privados reales; usar solo fixtures sintéticos.

## Orden de ejecución y cierre

- [Tareas](tasks.md#aceptación-y-cierre): aprobadas por DLG el 20-X-2026 («Vamos»). T01–T09 completadas; tests focalizados 35/35, suite completa 1073/1073 y `git diff --check` limpio el 20-X-2026. DLG aceptó y cerró S6.4 con «Tenemos proyecciones» el 20-X-2026. T09 y aceptación humana pendientes.
- Antes de implementar el consumo de NFC, desbloquear CA03 comprobando el estado aceptado de S6.2.
- Mantener la implementación de proyecciones y pruebas directas acotada a S6.4; reservar la prueba de integración de búsqueda/aislamiento para S6.7.
- El trabajo puede cerrarse cuando CA01–CA05 tengan evidencia satisfactoria, no haya bloqueos y la aceptación humana quede registrada en las tareas.
- Ante una contradicción con una fuente propietaria, detener la parte afectada y solicitar aprobación en su documento antes de cambiar el comportamiento.
