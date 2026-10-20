# Plan técnico: S6.5 — Consultas de búsqueda FTS5

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: aprobado por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Mantener las consultas de listado en los repositorios existentes de Infrastructure: `SqliteLinkRepository` y `SqliteCollectionRepository`, detrás de los contratos actuales `ILinkRepository` e `ICollectionRepository` de Application.
- Sustituir el rechazo actual de búsquedas por la integración de `SearchTextNormalizer` y `SearchQueryBuilder` ya existentes en Infrastructure. El procesamiento se aplica en los métodos de listado y de recuento para que páginas y total usen el mismo conjunto filtrado.
- Mantener filtros de enlaces en la consulta SQL de enlaces; no incorporar búsqueda o resultados de otras entidades. La integración de etiquetas permanece fuera del alcance definido.
- Mantener los DTOs de salida y firmas públicas actuales salvo que la inspección previa demuestre una incompatibilidad concreta con los criterios aprobados. No confiar en identificadores aportados por el cliente para delimitar propiedad: el identificador del usuario procede del contexto del caso de uso y se aplica en las consultas del índice y de la entidad.

### Flujos internos

1. Recibir el usuario, el texto, los filtros y los parámetros de paginación mediante el contrato existente del repositorio.
2. Normalizar el texto a NFC y generar la expresión con el constructor existente antes de consultar.
3. Cuando hay expresión, enlazar con la proyección propia del listado mediante `MATCH`, restringir la proyección por el usuario identificado y enlazarla con la tabla de dominio por identificador y usuario. En enlaces, usar la relevancia solo como orden de la consulta textual; en colecciones, no usarla.
4. Cuando no hay expresión, omitir el acceso al predicado de búsqueda y ejecutar el listado sin filtro textual, conservando los filtros de enlaces que correspondan.
5. Aplicar el orden completo y la paginación en SQLite, y hacer que los métodos de recuento apliquen exactamente los mismos filtros de usuario, texto y colección usados por la página.
6. Proyectar las filas a los DTOs ya previstos. Las consultas son de lectura y no añaden transacciones innecesarias.

### Persistencia y dependencias

- Reutilizar el esquema `LinkSearch` y `CollectionSearch` proporcionado por S6.1 y las escrituras sincronizadas por S6.4. No añadir migraciones, triggers ni tablas.
- Usar los parámetros enlazados de Dapper para valores de entrada, incluidos la expresión FTS5, el usuario, filtros y paginación; la expresión es producida por el constructor ya probado, no interpolada como SQL.
- Las consultas de página y de conteo deben compartir semántica de filtrado, aunque puedan ser sentencias separadas. No materializar en memoria todo el conjunto de coincidencias para paginar o contar.
- Aprovechar el registro de colación existente en la conexión para los órdenes alfabéticos del listado de colecciones.
- Reutilizar SQLite temporal de `SqliteRepositoryTestDatabase` y los tests existentes de repositorios; no se prevén dependencias nuevas.
- Las escrituras de sincronización y su atomicidad se validan por S6.4; la comprobación integrada de S6.7 es trabajo vecino, no prerrequisito de implementación para este plan.

### Garantías técnicas

- Las consultas de texto usan exclusivamente la proyección del tipo listado y comparan su usuario con el usuario identificado; la fila de entidad se comprueba también con el mismo usuario, según [architecture.md → «FTS5»](../../context/architecture.md#fts5) y [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- La consulta sin términos no emite `MATCH` vacío ni altera el resultado funcional de no buscar por texto, conforme a [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros).
- Filtros, orden, desempates, paginación y recuento permanecen coherentes con [specifications.md → «Listados»](../../context/specifications.md#listados).
- Las pruebas focalizadas comprueban consultas persistentes y DTOs; por sí solas no acreditan la sincronización para todos los caminos de escritura, el rollback de S6.4 ni la reconstrucción manual de S6.6. No se atribuye esa cobertura a este trabajo.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Implementar las búsquedas en los métodos de página y conteo de los repositorios existentes, sin un servicio de consulta paralelo. | propuesta | Contratos actuales y [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Mantiene una única superficie de repositorio; validar en la inspección de T01 que ambas consultas comparten el mismo predicado y usuario. |
| Reutilizar el normalizador y el constructor de S6.2/S6.3 en el orden indicado. | confirmado | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [S6.2](../s6.2-normalizacion-nfc-busqueda/spec.md), [S6.3](../s6.3-constructor-consulta-fts5/spec.md) | Evita mantener una segunda semántica; cubrir en pruebas el paso sin términos y una búsqueda textual real de SQLite. |
| Mantener queries de página y conteo separados si ambos conservan filtros idénticos. | propuesta | Firmas existentes de `ILinkRepository` e `ICollectionRepository` | La interfaz devuelve la página y el total en llamadas separadas; las pruebas deben detectar divergencias de filtros. No introducir una transacción que se extienda entre llamadas. |
| La proyección de etiquetas no participa en esta unidad de consulta, pese a que el alcance funcional global describe listados de etiquetas. | confirmado | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), delimitación S6.5 del [plan de implementación](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | La integración de búsqueda de etiquetas queda reservada a su etapa propietaria, sin fingir que este trabajo cubre ese listado. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada, integración SQLite: probar términos que coinciden en `LinkSearch`, con datos propios y de otro usuario, y control de ausencia de otros tipos de entidad. | Migraciones existentes y `SqliteRepositoryTestDatabase`; builder y normalizador disponibles. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteLinkRepositoryTests` | Solo salen enlaces coincidentes y pertenecientes al usuario indicado; la proyección de colecciones no produce resultados de enlace. | Salida focalizada, casos añadidos y SDK/runtime usado. |
| V02 | CA03, CA04 | Automatizada, integración SQLite: verificar términos, resultados sin términos, filtro de colección, orden de relevancia/desempates, orden no textual, página y conteo. | V01 y fixture de tests con datos deterministas. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteLinkRepositoryTests` | Resultados y total concuerdan; la paginación sigue al filtrado y orden establecidos; la consulta sin términos omite MATCH y aplica el filtro restante. | Salida focalizada, matrices de datos y comprobación de igualdad entre página y conteo. |
| V03 | CA05, CA06 | Automatizada, integración SQLite: coincidencias de colección propia y ajena, solo el campo `Name`, orden lingüístico, consulta sin términos y páginas. | Fixture de repositorio; colación configurada por la factoría de conexión de test. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteCollectionRepositoryTests` | El listado aplica búsqueda únicamente sobre `CollectionSearch`, permanece aislado y mantiene orden y paginación de colecciones con o sin texto. | Salida focalizada y casos de orden/aislamiento. |
| V04 | CA01–CA06 | Revisión técnica de consultas y comandos de prueba. | V01–V03. | `git diff --check`; revisión de SQL, filtros, DTOs y consultas de recuento. | Sin interpolación insegura de la entrada, sin mezcla de índices o usuarios y sin cambios de esquema; cada criterio tiene validación discriminante. | Resultado del comando y lista de archivos revisados. |

Las pruebas de repositorio con SQLite verifican la ejecución real de FTS5 y las filas devueltas en el fixture, pero no recorren todos los casos de escritura ni restauran una proyección dañada. Los controles negativos deben incluir coincidencias ajenas y resultados no coincidentes que fallen si se omite el usuario, la condición de entidad, un filtro o un desempate. El comando de prueba usa el filtro ya existente; T01 confirmará la invocación exacta y el nombre final de cada test.

## Orden de ejecución y cierre

- [Tareas](tasks.md): preparadas tras la aprobación explícita de este plan; pendientes de aprobación y contienen dependencias y checks inmediatos por criterio.
- Cada criterio requiere evidencia satisfactoria de su consulta correspondiente; no condicionar cierre a S6.7, S6.6 ni al listado de etiquetas.
- La aceptación humana del resultado se registra en `tasks.md` y no se infiere de los tests.
- Ante una contradicción futura con una regla fuente, detener la tarea afectada y solicitar su resolución en el documento propietario antes de cambiar el contrato.
