# Tareas: S4.2 — Contratos de repositorios, Unit of Work e identidad identificada

## Referencias y autorización

- Estado: trabajo cerrado para el alcance S4.2.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Planifica»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 15-X-2026 («Adelante»).
    - Tareas: aprobadas por DLG el 15-X-2026 («Vamos»).
- Implementación:
    - Estado: completada y aceptada por DLG.
    - Inicio: 15-X-2026 («Vamos»).
    - Finalización: 15-X-2026; contratos revisados, build correcto y suite 736/736.
    - Evidencia consolidada: build y suite integradas correctas; detalle en T06 y en la evidencia por criterio.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 contrasta el catálogo S4 con los contratos S4.1 aceptados y las reglas propietarias disponibles; los criterios S4.4 se incorporan solo si ya existen, no son prerrequisito. T03 y T04 completan los contratos necesarios a partir de ese alcance aprobado. T05 define la abstracción de identidad. T06 integra las validaciones; los casos de uso S4.6–S4.10 siguen sujetos a la puerta S4.5.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas; registrar la referencia explícita de DLG. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Confirmar cobertura de todos los criterios, dependencias y límites respecto a S4.3–S4.10; resultado esperado: aprobación de tareas separada de spec y plan. | completada | Aprobación registrada por DLG el 15-X-2026 («Vamos»). |
| T02 | Contrastar el estado y aceptación de S4.1 y elaborar la matriz operación → contrato/entidad de Domain/DTO de Application, usando el catálogo S4 y los criterios disponibles de S4.4. | CA02, CA07; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02 | T01 | Verificar las operaciones de colecciones, enlaces, dashboard y lecturas públicas previstas por S4; resultado esperado: matriz de cobertura, sin operaciones fuera de alcance, con dependencias de S4.1 aceptadas identificadas. | completada | S4.1 consta cerrada; matriz contrastada con catálogo S4 y fuentes aprobadas. S4.4 aún no está disponible y no es prerrequisito de S4.2. |
| T03 | Definir las interfaces específicas de repositorio de Application que exige la matriz T02, sin añadir operaciones no justificadas. | CA01–CA04, CA07; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01–V04 | T02 | Compilar Application y revisar cada firma respecto a la matriz; resultado esperado: contratos específicos, separados por responsabilidades necesarias y sin tipos de persistencia, infraestructura o presentación. | completada | Contratos de colecciones y enlaces cubren acceso privado, escrituras, listados MVP0 con búsqueda/paginación y filtro de colección para enlaces; contratos separados para dashboard y lecturas públicas compuestas. Revisión de firmas y namespaces; build Application correcto; `ProjectReferenceMatrixTests` 1/1. |
| T04 | Definir el contrato de Unit of Work de Application para coordinar escrituras compuestas, compatible con los contratos de repositorio de T03. | CA01, CA05, CA07; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01, V05 | T03 | Compilar Application y revisar ciclo de vida/finalización ante éxito y fallo; resultado esperado: frontera explícita sin exponer transacciones concretas ni incorporar trabajo externo. | completada | `IUnitOfWork` expone `Begin`, `Commit`, `Rollback`, resolución de repositorios y liberación síncrona/asíncrona; `IUnitOfWorkFactory` resuelve el contexto. Build de Application correcto y `ProjectReferenceMatrixTests` 1/1. V05 ejercitará éxito/fallo cuando haya consumidores autorizados. |
| T05 | Definir la abstracción de usuario identificado consumida por Application, diferenciada del proveedor de identidad de desarrollo S4.3. | CA01, CA03, CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01, V03 | T01, T02 | Revisar firmas y comprobar que la identidad no procede de parámetros/DTOs de cliente; resultado esperado: fuente de identidad independiente y sin implementación del proveedor S4.3. | completada | `ICurrentUser` y `CurrentUser` están en Application; `CurrentUserTracksTheIdentifiedUser` verifica el identificador y el estado autenticado. No se implementa el proveedor de S4.3. |
| T06 | Ejecutar validaciones de arquitectura, contratos, compilación y suite; corregir solo incidencias del alcance S4.2. | CA01–CA07; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V07 | T03–T05 | Ejecutar las comprobaciones contractuales de V01–V05, V06 si procede, y V07; resultado esperado: firmas revisadas, dependencias permitidas y compilación/suite correctas, con limitaciones registradas por etapa. | completada para el alcance S4.2; V06 pendiente | V01, V02–V05 y V07 verificadas a nivel de estructura/firma. SDK 10.0.401; build correcto; suite 736/736; matriz arquitectónica 1/1. V06 queda como mejora de sensibilidad del test arquitectónico, no bloquea la suficiencia de contratos. Las pruebas de consumidores se asignan a las etapas consumidoras. |
| T07 | Revisar trazabilidad criterio → evidencia, sincronizar el roadmap y solicitar aceptación humana del resultado. | CA01–CA07; [plan → Garantías técnicas](plan.md#garantías-técnicas) y [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Comprobar evidencia satisfactoria de los criterios dentro del alcance S4.2 y no presentar la aprobación de tareas como aprobación de S4.5; resultado esperado: decisión explícita de DLG sobre el resultado o bloqueo concreto documentado. | completada | Trazabilidad CA01–CA07 revisada abajo. DLG acepta el resultado con la referencia «Completado» el 15-X-2026. Las pruebas de comportamiento se difieren a los trabajos de consumidores; V06 es una comprobación adicional de sensibilidad arquitectónica. Roadmap sincronizado. |

## Matriz de operaciones MVP0 → contratos

Inventario realizado en T02 contra el catálogo S4, los artefactos aceptados de S4.1 y las fuentes funcionales aprobadas disponibles. La ausencia de S4.4 no bloquea este trabajo: esa etapa podrá añadir detalle de aceptación sin reabrir las reglas propietarias.

| Área / operación S4 | Fuente funcional | DTO(s) de Application | Contrato(s) de acceso a datos | Estado |
| --- | --- | --- | --- | --- |
| Colecciones: crear, renombrar, visibilidad y eliminar | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | `CreateCollectionRequest`, `CollectionDto` | `ICollectionRepository`: `GetById`, `Insert`, `Update`, `Delete` | Cubiertas a nivel contractual; las comprobaciones de colisión, última colección y enlaces pertenecen a la orquestación/transacción |
| Colecciones: listar y lecturas públicas | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), [specifications.md → «Listados»](../../context/specifications.md#listados), [«Páginas públicas»](../../context/specifications.md#páginas-públicas) | `CollectionDto`, `PublicCollectionDto` | `ICollectionRepository`: `GetPageByUser`, `GetPageCountByUser`; `IPublicReadRepository` | Cubiertas; listado privado por usuario y consultas públicas compuestas separadas |
| Enlaces: crear, editar, mover, visibilidad y eliminar | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | `CreateLinkRequest`, `LinkDto` | `ILinkRepository`: `GetById`, `GetByNormalizedUrl`, `Insert`, `Update`, `Delete`; `ICollectionRepository.GetById` | Cubiertas a nivel contractual; las comprobaciones de duplicados, propiedad y visibilidad pertenecen a la orquestación/transacción |
| Enlaces: listar privado y lecturas públicas | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), [specifications.md → «Listados»](../../context/specifications.md#listados), [«Páginas públicas»](../../context/specifications.md#páginas-públicas) | `LinkDto`, `PublicUserDto`, `PublicCollectionDto` | `ILinkRepository`: `GetPageByUser` (texto y filtro de colección), `GetPageCountByUser`; `IPublicReadRepository` | Cubiertas; sin filtros de etiqueta, fuera del alcance MVP0 |
| Dashboard | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | `DashboardDto` | `IDashboardRepository.GetByUser` | Cubierta con consulta explícita por identidad |
| Identidad y unidad de trabajo (transversales) | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [architecture.md → «Autenticación»](../../context/architecture.md#autenticación), [«Persistencia»](../../context/architecture.md#persistencia) | No aplica | `ICurrentUser`; `IUnitOfWorkFactory`; `IUnitOfWork` | Abstracciones presentes; el ciclo del Unit of Work corresponde a T04 |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03–T06 | V01, V06–V07 | Verificado para la matriz y dependencias actuales; V06 pendiente como comprobación de sensibilidad | 15-X-2026, .NET SDK 10.0.401 | [ProjectReferenceMatrixTests.cs](../../tests/Linkubator.Tests/Structure/ProjectReferenceMatrixTests.cs) 1/1; build de solución correcto; suite 736/736; V06 pendiente |
| CA02 | T02–T03, T06 | V02, V07 | Verificado a nivel contractual con la matriz T02 | 15-X-2026, .NET SDK 10.0.401 | Matriz de operaciones y contratos en este archivo; Application build correcto; suite 736/736 |
| CA03 | T03, T05–T06 | V03, V07 | Verificado a nivel contractual: identidad expuesta por abstracción y ámbito privado explícito; pruebas de casos de uso diferidas | 15-X-2026, .NET SDK 10.0.401 | `ICurrentUser.cs`, `CurrentUser.cs`, contratos de repositorios y DTOs revisados; `CurrentUserTracksTheIdentifiedUser`; suite 736/736 |
| CA04 | T02–T03, T06 | V04, V07 | Verificado a nivel contractual: acceso privado por separado de lecturas públicas compuestas | 15-X-2026, .NET SDK 10.0.401 | `ICollectionRepository.cs`, `ILinkRepository.cs`, `IPublicReadRepository.cs`; inspección de firmas; suite 736/736 |
| CA05 | T04, T06 | V05, V07 | Verificado a nivel contractual: ciclo explícito sin tipos de infraestructura; ejecución concreta diferida | 15-X-2026, .NET SDK 10.0.401 | `IUnitOfWork.cs` (`Begin`, `Commit`, `Rollback` y disposables), `IUnitOfWorkFactory.cs`; build y suite correctos |
| CA06 | T05–T06 | V03, V07 | Verificado a nivel contractual: Application consume abstracción independiente del proveedor S4.3 | 15-X-2026, .NET SDK 10.0.401 | `ICurrentUser.cs`, `CurrentUser.cs`; [ContractsTests.cs](../../tests/Linkubator.Tests/Application/Contracts/ContractsTests.cs), suite 736/736 |
| CA07 | T02–T04, T06 | V02, V05, V07 | Verificado por revisión de responsabilidades contractuales y compilación | 15-X-2026, .NET SDK 10.0.401 | Matriz de operaciones, interfaces Application sin detalles de persistencia; build y suite correctos |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: cerrado para el alcance de S4.2; implementación, validación contractual y aceptación completadas.
- Criterios con evidencia satisfactoria a nivel contractual: CA01–CA07, con las comprobaciones indicadas en la tabla de evidencia.
- Validaciones diferidas: pruebas de comportamiento de consumidores V03–V05 para las etapas que los implementen; V06 es una comprobación adicional de sensibilidad arquitectónica y no bloquea estos contratos.
- Bloqueos y riesgos restantes: ninguno para el alcance de S4.2. Las pruebas de comportamiento de consumidores quedan en sus etapas correspondientes; V06 es una comprobación adicional, no un bloqueo del alcance. La puerta S4.5 sigue siendo necesaria para implementar S4.6–S4.10; no existe dependencia de S4.2 respecto a S4.4.
- Aprobación de tareas: DLG, 15-X-2026 («Vamos»); esto no es aceptación del resultado.
- Aceptación humana del resultado: aceptada por DLG el 15-X-2026 con la referencia «Completado».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: continuar con la siguiente etapa según el roadmap. Las pruebas de comportamiento V03–V05 corresponden a sus etapas consumidoras. No iniciar S4.6–S4.10 antes de la puerta S4.5.
