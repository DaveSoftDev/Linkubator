# Plan técnico: S4.6 — Casos de uso de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 16-X-2026 («A planear»).
	- Plan: aprobado por DLG el 16-X-2026 («Aprovado»).

## Diseño de implementación

### Componentes y contratos

- **Casos de uso de Application.** Implementar servicios por responsabilidad, siguiendo el patrón `I*Service` → `*Service` de [architecture.md → «Application»](../../../context/architecture.md#application), para crear, renombrar, cambiar visibilidad, eliminar y listar colecciones. Ubicar cada tipo público en su archivo y módulo de casos de uso, siguiendo [AGENTS.md → «Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código). Las interfaces nuevas y su granularidad se concretarán en las tareas a partir del inventario de los contratos existentes.
- **Contratos existentes.** Consumir `ICurrentUser` para obtener el usuario de las operaciones privadas; `ICollectionRepository`, `ILinkRepository` y `IUnitOfWorkFactory`/`IUnitOfWork` para acceso a datos y escrituras compuestas; `CreateCollectionRequest`, `CollectionDto`, `CollectionCreateValidator`, `Result` y `ApplicationError` para entrada, salida y errores. No añadir contratos de repositorio ni DTOs salvo que el inventario de tareas detecte una carencia real; de ser así, registrar la propuesta y su justificación en tasks antes de modificar el alcance.
- **Dominio.** Crear y modificar `Collection` mediante sus operaciones de dominio. Application traduce excepciones de dominio al error de aplicación previsto; no replica transformaciones ni invariantes en servicios.
- **Colisiones.** Consultar el slug generado contra colecciones del mismo usuario mediante el repositorio. Mantener el nombre de la colección coincidente accesible para el consumidor según los criterios aprobados, sin depender solo de una restricción de persistencia ni crear una regla paralela. El tratamiento de una colisión concurrente se contrastará con la garantía de unicidad de la capa persistente; la implementación concreta de Infrastructure corresponde a S5.
- **Errores.** Usar las categorías y el mapeo desde Domain ya provistos por Application. Para identificadores no accesibles en eliminación, devolver la categoría y el código estable por operación que exige architecture.md; el literal exacto se fijará durante el diseño detallado de tareas al contrastar contratos y criterios, sin exponer la causa real. No transportar textos HTTP ni excepciones de persistencia.
- **Listados.** Delegar al repositorio la consulta paginada y el total de páginas, exponiendo el DTO de aplicación y los resultados necesarios para el consumidor. La normalización de parámetros de URL que corresponde a Web no se traslada al caso de uso; el contrato existente recibe los valores de búsqueda/paginación conforme a los límites aprobados.

### Flujos internos

1. Cada operación privada obtiene `UserId` desde `ICurrentUser`; nunca lo recibe como autoridad desde la solicitud.
2. En crear, validar la petición, crear la entidad mediante Domain, comprobar colisión en el ámbito del usuario y guardar el resultado con el patrón de unidad de trabajo correspondiente.
3. En renombrar, recuperar la colección dentro del ámbito del usuario, generar/validar el nuevo slug mediante Domain, comprobar colisión excluyendo la propia colección y aplicar los cambios de nombre y slug solo si toda la operación puede completarse.
4. En cambio de visibilidad, recuperar la colección y sus enlaces mediante los repositorios existentes, invocar la operación de dominio que preserva la invariante y guardar los cambios de forma atómica.
5. En eliminación, recuperar la colección bajo el usuario identificado, distinguir el recurso no accesible de una regla de dominio, y aplicar las verificaciones de enlaces y última colección dentro de la unidad de trabajo que protege la escritura.
6. En listado, validar entrada aplicable y solicitar la página y su recuento al repositorio con el ámbito de usuario identificado. Los criterios exactos y sus rechazos los determina el catálogo aprobado y la capa responsable de los parámetros.
7. Las escrituras terminan con commit cuando resultan satisfactorias y rollback ante error; la liberación de recursos se garantiza en todos los caminos. No se introducen esperas, llamadas externas ni lógica de Web en la unidad de trabajo.

La secuencia específica de cada operación y el tratamiento de excepciones se concretarán en tareas contra la firma real de contratos. Ante una discrepancia con una regla funcional o criterio aprobado, detener la tarea y resolver en el documento propietario antes de continuar.

### Persistencia y dependencias

- Application depende solo de Domain conforme a [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos). No se añaden paquetes ni referencias a Infrastructure, Web, SQLite o Dapper.
- Se utilizan los contratos existentes de repositorio y Unit of Work; no se implementan SQL ni repositorios concretos. Infrastructure/S5 es responsable de la persistencia y de sus restricciones.
- Las escrituras que involucren más de una entidad, o que deban coordinar cambios de privacidad/eliminación con relaciones, usan la unidad de trabajo definida por Application y la garantía transaccional de la implementación futura, según [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia).
- Los fakes y pruebas de comportamiento de consumidores son entregables separados de S4.9; esta etapa no los adelanta. No se requiere comunicación con servicios externos para estos casos.

### Garantías técnicas

- La identidad de cada operación privada proviene de `ICurrentUser`; la consulta y modificación siempre quedan limitadas a ese ámbito y comprueban propiedad de relaciones, según [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos).
- Las reglas de nombres, slugs, eliminación, última colección y privacidad se delegan a Domain o se coordinan por Application según sus fuentes propietarias; ningún fallo deja mutaciones parciales.
- Al hacer privada una colección, el cambio de estado de la colección y de sus enlaces se coordina como una única operación conforme a [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y al criterio AC-COL-03b.
- El listado nunca incluye colecciones de otro usuario ni entidades distintas, y consulta los filtros/orden/paginación mediante el contrato aprobado.
- Los resultados de Application no incluyen mensajes de presentación HTTP, datos ajenos, detalles SQL ni excepciones de Infrastructure.
- Las pruebas de comportamiento de cada criterio se atribuyen a S4.9; compilación o revisión estructural por sí solas no prueban el comportamiento funcional.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener los casos de uso en Application y consumir abstracciones existentes de identidad, repositorio y unidad de trabajo | Confirmado por arquitectura y contratos aceptados | [architecture.md → «Application»](../../../context/architecture.md#application), S4.1–S4.3 | No requiere cambiar los contratos salvo que el inventario de tareas demuestre una insuficiencia concreta. |
| Organizar un servicio por operación o grupo cohesivo, con tipos públicos en archivos separados | Propuesta técnica | [AGENTS.md → «Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código) y [architecture.md → «Application»](../../../context/architecture.md#application) | Determinar en tasks la granularidad que evita clases generales y mantiene una responsabilidad clara. |
| Usar `ILinkRepository` para obtener enlaces de una colección durante privacidad/eliminación | Propuesta derivada de firmas existentes | `ILinkRepository.GetAllByCollection`; AC-COL-03b y AC-COL-04b | Contrastar si la carga es suficiente para ejecutar las operaciones de Domain y no introduce escrituras fuera de la unidad de trabajo. |
| Las comprobaciones funcionales de consumidores y los fakes pertenecen a S4.9 | Confirmado por el plan MVP0 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | La aceptación de CA01–CA05 queda pendiente de evidencia conductual hasta que se ejecute S4.9; no inferirla de build/revisión. |
| Resolver colisión de slug concurrente solo con comprobación de Application | No confirmado; no se asume | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [domain-model.md → «Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad) | La garantía final recae en unicidad persistente. S4.6 debe traducir el conflicto solo si los contratos/error disponible lo permiten; concreto y manejo de Infrastructure se revisan en S5, sin inventar resultado funcional. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA06, CA08 | Técnica: revisión de estructura, referencias y límites de capa | Cambios de Application implementados | Revisar archivos/tipos públicos y `Linkubator.Application.csproj`; ejecutar `ProjectReferenceMatrixTests` existente | Tipos organizados por responsabilidad; no se añaden referencias de capa no permitidas | Lista de archivos revisados, test/resultado, SDK y fecha. No demuestra reglas de negocio. |
| V02 | CA06–CA08 | Automatizada: compilación integrada | SDK .NET 10 y dependencias restauradas | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Solución compila sin errores y warnings se registran | Comando, SDK, exit code y resumen. No prueba criterios funcionales. |
| V03 | CA01–CA05, CA07 | Técnica: inspección de cobertura del código frente al catálogo y fuentes | Cambios de casos de uso implementados; catálogo S4 aprobado | Revisar cada operación y cada camino de éxito/rechazo contra AC-COL-01, AC-COL-01b, AC-COL-02–AC-COL-05 y contratos S4.1–S4.3; comprobar identidad, límites de consulta, unidad de trabajo y ausencia de mutación antes de todas las comprobaciones | Trazabilidad de las cinco operaciones y sus decisiones; hallazgos bloqueantes quedan explícitos | Matriz operación/criterio → tipos y rutas revisadas, hallazgos y resolución. Revisión no sustituye los tests de S4.9. |
| V04 | CA01–CA05, CA07 | Automatizada: pruebas de comportamiento | Fakes y tests de consumidor de S4.9 implementados; casos de uso de S4.6 disponibles | Ejecutar las pruebas de aplicación S4.9 que cubran criterios de colecciones | Cada criterio cubierto supera sus escenarios positivos y negativos, incluida propiedad, colisiones, atomicidad simulada, última colección y listado; fallo de cualquiera bloquea la evidencia del criterio correspondiente | Nombres/filtro de tests, versión SDK, resultado y vínculo a la evidencia de S4.9. No atribuir estos tests a S4.6. |
| V05 | CA01–CA08 | Automatizada e integrada: suite y diff | Entorno restaurado, validaciones anteriores preparadas | `dotnet test Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` sobre los archivos del alcance | Tests disponibles pasan y diff no tiene errores de whitespace; informar claramente de pruebas de S4.9 todavía no disponibles | Comandos, SDK, exit code, resumen, diff revisado. Suite verde no equivale a aceptación humana. |

V04 es un prerrequisito de evidencia de comportamiento pero su trabajo pertenece a S4.9; si S4.9 todavía no está disponible, los criterios conductuales permanecen pendientes y no se declara S4.6 aceptada. No se crean fakes o tests en S4.6 para evitar duplicar o adelantar el trabajo asignado.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde la plantilla, desglosando inventario de contratos, cada operación de colección, validación inmediata, compilación y coordinación de evidencia pendiente de S4.9.
- Las tareas no modificarán reglas ni contratos aceptados por conveniencia. Una carencia concreta se documentará con criterio afectado, propuesta y decisión requerida antes de ampliar el alcance.
- La aceptación técnica de S4.6 requiere evidencia por criterio; CA01–CA05 no se declaran satisfechos hasta contar con pruebas de consumidor de S4.9. La aprobación de este plan o de las tareas no constituye aceptación de implementación.
- Ante una contradicción o hueco de regla, detener la tarea afectada y resolver primero el documento propietario; no iniciar cambios productivos hasta que `tasks.md` esté aprobado.
