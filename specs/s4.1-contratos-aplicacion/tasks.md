# Tareas: S4.1 — Result, errores, DTOs y validadores de aplicación

## Referencias y autorización

- Estado: aprobada y ejecutada.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Aprobado»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 15-X-2026 («Comprendido»).
    - Tareas: aprobada por DLG el 15-X-2026 («Vamos»).
- Implementación:
    - Estado: validada y cerrada para el alcance S4.1.
    - Inicio: 15-X-2026 («Vamos»).
    - Finalización y aceptación: DLG, 15-X-2026 («Completada»).
    - Validación más reciente: `dotnet test Linkubator.sln --no-restore --verbosity minimal`; compilación correcta y suite 737/737.
    - Evidencia consolidada: [ResultTests.cs](../../tests/Linkubator.Tests/Application/ResultTests.cs), [Result.cs](../../src/Linkubator.Application/Contracts/Result.cs), [ResultOfT.cs](../../src/Linkubator.Application/Contracts/ResultOfT.cs), [ApplicationError.cs](../../src/Linkubator.Application/Contracts/ApplicationError.cs), [CollectionCreateValidator.cs](../../src/Linkubator.Application/Validation/CollectionCreateValidator.cs), [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/LinkCreateValidator.cs).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 inventaría los contratos concretos cubiertos antes de implementarlos. T03–T05 separan el resultado/errores, los DTOs y los validadores para limitar el alcance de cada cambio; T06 integra las pruebas y garantías. No se inicia ninguna tarea productiva antes de la aprobación explícita de estas tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas; registrar la referencia explícita de DLG. | CA01–CA08; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Confirmar que todos los criterios tienen tareas y validaciones, y que el alcance excluye trabajo S4.2–S4.10 no autorizado aquí; resultado esperado: aprobación de tareas registrada separada de spec y plan. | completada | Aprobada el 15-X-2026 por DLG. |
| T02 | Elaborar la matriz de operaciones MVP0 incluidas en S4.1 y asignar a cada una sus DTOs de entrada/salida y validadores, enlazando su fuente funcional; incorporarla a esta sección o como tabla documental en `tasks.md`. | CA04–CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V03–V04 | T01 | Contrastar todas las operaciones de S4 del plan MVP0 con el catálogo; resultado esperado: ninguna operación aplicable omitida y ninguna capacidad posterior añadida. | completada | Evidencia: se incorpora la matriz documental en este mismo archivo con el mapeo a [CreateCollectionRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateCollectionRequest.cs), [CreateLinkRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateLinkRequest.cs), [CollectionCreateValidator.cs](../../src/Linkubator.Application/Validation/CollectionCreateValidator.cs) y [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/LinkCreateValidator.cs). |
| T03 | Implementar `Result`, `Result<T>` y los errores tipados, con invariantes de estados y categorías aprobadas. | CA02–CA03, CA08; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02, V06 | T02 | Ejecutar los tests unitarios de contratos; resultado esperado: éxito/error distinguibles, errores consultables por código/categoría/campo y sin datos sensibles. | completada | Evidencia: [Result.cs](../../src/Linkubator.Application/Contracts/Result.cs), [ResultOfT.cs](../../src/Linkubator.Application/Contracts/ResultOfT.cs), [ApplicationError.cs](../../src/Linkubator.Application/Contracts/ApplicationError.cs), [ResultTests.cs](../../tests/Linkubator.Tests/Application/ResultTests.cs). |
| T04 | Añadir los DTOs de entrada y salida de la matriz T02, solo con valores de transporte requeridos. | CA01, CA04–CA05, CA08; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01, V03 | T02 | Compilar Application y revisar las firmas y propiedades de todos los DTOs; resultado esperado: cubren las operaciones inventariadas sin entidades de persistencia, contratos Web ni identidad de usuario proporcionada por el cliente. | completada | Evidencia: [CreateCollectionRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateCollectionRequest.cs), [CreateLinkRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateLinkRequest.cs), [CollectionDto.cs](../../src/Linkubator.Application/Dtos/Models/CollectionDto.cs), [LinkDto.cs](../../src/Linkubator.Application/Dtos/Models/LinkDto.cs). |
| T05 | Implementar validadores para los DTOs de entrada de T04 y traducir a errores de aplicación los rechazos conocidos de las políticas de Domain que correspondan. | CA03, CA06–CA08; [plan → Flujos internos](plan.md#flujos-internos), V04–V06 | T03, T04 | Ejecutar tests con entradas válidas e inválidas y traducción de rechazos Domain; resultado esperado: errores asociados a campos y reglas propietarias reutilizadas, sin mensajes de presentación ni valores sensibles. | completada | Evidencia: [CollectionCreateValidator.cs](../../src/Linkubator.Application/Validation/CollectionCreateValidator.cs), [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/LinkCreateValidator.cs), [ResultTests.cs](../../tests/Linkubator.Tests/Application/ResultTests.cs). |
| T06 | Ejecutar validaciones integradas de contratos, dependencias, build y suite; corregir solo incidencias pertenecientes al alcance S4.1. | CA01–CA08; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V07 | T03–T05 | Ejecutar la prueba de referencias, los tests específicos y los comandos integrados V07; resultado esperado: todos pasan y se registra la evidencia real, SDK y warnings. | completada | Primera ejecución registrada: 730/730. Revalidación del 15-X-2026: `dotnet test Linkubator.sln --no-restore --verbosity minimal`, compilación correcta, suite 737/737 y 0 errores. |
| T07 | Revisar trazabilidad de criterio a evidencia, sincronizar el roadmap y registrar aceptación humana del resultado. | CA01–CA08; [plan → Garantías técnicas](plan.md#garantías-técnicas) y [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Comprobar que cada criterio tiene validación satisfactoria y evidencia fechada, y que no se presenta esta aceptación como aprobación de S4.5; resultado esperado: decisión explícita de DLG registrada o bloqueo documentado. | completada para el alcance S4.1 sin abrir S4.5 | DLG acepta el resultado el 15-X-2026 mediante la instrucción «Completada»; roadmap sincronizado. No se anuncia aprobación de S4.5. |

## Matriz de operaciones MVP0 → DTOs

Se completa el inventario coherente con el alcance del S4.1 implementado: creación y validación de colecciones, creación y validación de enlaces, y modelos de salida para dashboard y páginas públicas.

| Área / operación S4 | Fuente funcional | DTO de entrada | DTO(s) de salida | Validador | Estado |
| --- | --- | --- | --- | --- | --- |
| Crear colección | [requirements.md → «Colecciones»](../../context/requirements.md#colecciones) | [CreateCollectionRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateCollectionRequest.cs) | [CollectionDto.cs](../../src/Linkubator.Application/Dtos/Models/CollectionDto.cs) | [CollectionCreateValidator.cs](../../src/Linkubator.Application/Validation/CollectionCreateValidator.cs) | Completado |
| Crear enlace | [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) | [CreateLinkRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateLinkRequest.cs) | [LinkDto.cs](../../src/Linkubator.Application/Dtos/Models/LinkDto.cs) | [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/LinkCreateValidator.cs) | Completado |
| Dashboard privado | [requirements.md → «Páginas privadas»](../../context/requirements.md#páginas-privadas) | No aplica | [DashboardDto.cs](../../src/Linkubator.Application/Dtos/Models/DashboardDto.cs) | No aplica | Completado |
| Página pública de usuario | [requirements.md → «Páginas públicas»](../../context/requirements.md#páginas-públicas) | No aplica | [PublicUserDto.cs](../../src/Linkubator.Application/Dtos/Models/PublicUserDto.cs) | No aplica | Completado |
| Página pública de colección | [requirements.md → «Páginas públicas»](../../context/requirements.md#páginas-públicas) | No aplica | [PublicCollectionDto.cs](../../src/Linkubator.Application/Dtos/Models/PublicCollectionDto.cs) | No aplica | Completado |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T04, T06 | V01, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: [ProjectReferenceMatrixTests.cs](../../tests/Linkubator.Tests/ProjectReferenceMatrixTests.cs), [Linkubator.Application.csproj](../../src/Linkubator.Application/Linkubator.Application.csproj), `dotnet test Linkubator.sln --no-restore --verbosity minimal` |
| CA02 | T03, T06 | V02, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: [Result.cs](../../src/Linkubator.Application/Contracts/Result.cs), [ResultOfT.cs](../../src/Linkubator.Application/Contracts/ResultOfT.cs), [ResultTests.cs](../../tests/Linkubator.Tests/Application/ResultTests.cs) |
| CA03 | T03, T05, T06 | V02, V04, V05, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: [ApplicationError.cs](../../src/Linkubator.Application/Contracts/ApplicationError.cs), [CollectionCreateValidator.cs](../../src/Linkubator.Application/Validation/CollectionCreateValidator.cs), [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/LinkCreateValidator.cs) |
| CA04 | T02, T04, T06 | V03, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: DTOs en [CreateCollectionRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateCollectionRequest.cs), [CreateLinkRequest.cs](../../src/Linkubator.Application/Dtos/Requests/CreateLinkRequest.cs), [CollectionDto.cs](../../src/Linkubator.Application/Dtos/Models/CollectionDto.cs), [LinkDto.cs](../../src/Linkubator.Application/Dtos/Models/LinkDto.cs), [DashboardDto.cs](../../src/Linkubator.Application/Dtos/Models/DashboardDto.cs), [PublicUserDto.cs](../../src/Linkubator.Application/Dtos/Models/PublicUserDto.cs), [PublicCollectionDto.cs](../../src/Linkubator.Application/Dtos/Models/PublicCollectionDto.cs) |
| CA05 | T02, T04, T06 | V03, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: los DTOs de entrada no contienen `UserId` y se mantienen en Application; la identidad sigue fuera del cliente según [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) |
| CA06 | T02, T05, T06 | V04, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: [CollectionCreateValidator.cs](../../src/Linkubator.Application/Validation/CollectionCreateValidator.cs), [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/LinkCreateValidator.cs), [ResultTests.cs](../../tests/Linkubator.Tests/Application/ResultTests.cs) |
| CA07 | T05, T06 | V05, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: los validadores devuelven errores de aplicación sin convertir el rechazo en éxito; la regla sigue protegida por Domain y no se duplica en Application |
| CA08 | T02–T06 | V02, V03, V06, V07 | Completado | 15-X-2026, .NET 10.0.12 | Evidencia: [ApplicationError.cs](../../src/Linkubator.Application/Contracts/ApplicationError.cs), [ResultTests.cs](../../tests/Linkubator.Tests/Application/ResultTests.cs), `dotnet test Linkubator.sln --no-restore --verbosity minimal` |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: cerrado para el alcance de S4.1.
- Criterios con evidencia satisfactoria: CA01–CA08.
- Bloqueos y riesgos restantes: ninguno en el alcance S4.1; la puerta S4.5 sigue siendo obligatoria antes de implementar casos de uso S4.6–S4.10.
- Aceptación humana: DLG, 15-X-2026, referencia «Completada». Es distinta de la aprobación de tareas «Vamos» y no aprueba S4.5.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: continuar con la siguiente etapa de S4 según se acuerde, manteniendo la puerta S4.5 obligatoria para los casos de uso posteriores.
