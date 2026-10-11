# Plan técnico: S2.6 — Unificación de las validaciones de entrada de Domain

## Estado y aprobación del plan

- Estado: aprobado por DLG el 11-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 11-X-2026 («Aprobada»).
  - Plan: aprobado por DLG, 11-X-2026 («Listo»).

## Diseño de implementación

### Componentes y contratos

- **Error de precondición.** `NotValidInputException` en `Linkubator.Domain/Exceptions`, derivada de `DomainException`, con mensaje fijo que no incluye la entrada recibida (CA06). Es el único tipo nuevo.
- **Comprobación única.** `UserTextPolicy.ThrowIfNullOrWhiteSpace(string? value)` lanza `NotValidInputException` si el valor es nulo, vacío o en blanco y no modifica la entrada. Es el único punto donde se decide la precondición (CA02, CA03, CA07).
- **Métodos que la invocan** al inicio, antes de cualquier otra lógica (CA01):
  - `AsciiTransformationPolicy.Transform`, `AliasPolicy.Generate` y `SlugPolicy.Generate`.
  - `LinkUrlPolicy.Adjust`, `UriSchemePolicy.Detect`, `UrlValidationPolicy.Validate` y `UrlNormalizationPolicy.Normalize`.
  - `UrlHostPolicy.ExtractHost`.
  - `ImageUrlPolicy.Adjust`, para `input` y para `baseUrl` (CA03, CA04).
- **Fuera de la comprobación única** (según [spec.md → «Alcance»](spec.md#alcance)): `UserTextPolicy.TrimToNull`, `CountCodePoints`, `IsWithinMaximumLength`, y los predicados `IsValidHost`, `TryExtractHost` y `TryExtractValidHost`, que devuelven falso en lugar de lanzar.
- **Resultado vacío tras transformar.** `AliasPolicy` y `SlugPolicy` conservan su comprobación de longitud tras la transformación y su error de dominio de longitud; la precondición solo cubre la entrada (CA05).
- **Limpieza de comprobaciones duplicadas** (CA07): eliminar la comprobación manual inalcanzable de `ImageUrlPolicy.ResolveRelative` (su `baseUrl` ya está validado al inicio) y los `using` de `DomainExceptions` que dejen de usarse en `UriSchemePolicy` y `UrlNormalizationPolicy`. La precondición de URI absoluta de `UrlNormalizationPolicy.Normalize` no se toca (contrato de S2.4).
- **Pruebas.** Se actualizan las existentes de cada política en `tests/Linkubator.Tests/Domain/Policies` en lugar de crear clases nuevas, y se añade una prueba directa del helper y de la excepción:
  - Para cada método incluido: nulo, vacío y en blanco con el tipo de error exacto (CA09), y la entrada con espacios en los extremos sin rechazo por precondición (CA03).
  - Los casos de vacío o en blanco que antes esperaban un error de longitud (alias, slug) o un resultado vacío (`Transform`, `Image`) pasan a esperar `NotValidInputException`; se registran en `tasks.md` como cambios explícitos de CA03 y CA04, no como regresiones (CA08).
  - Una prueba de que el mensaje de `NotValidInputException` no contiene el texto introducido (CA06).
- **Inventario.** `tasks.md` recoge la tabla método → tratamiento anterior → tratamiento posterior o motivo de exclusión (CA01), sin copiar reglas.

### Flujos internos

1. Entrada pública de cada método incluido: llamar a la comprobación única; si falla, `NotValidInputException` y no se ejecuta nada más.
2. Superada la precondición, la lógica de cada política no cambia, incluidos sus errores de dominio de longitud, esquema, host y demás rechazos.
3. Si una prueba de ejemplos de S1.5 o S2.5 cambia de resultado por una causa distinta de CA03 y CA04, detener la tarea afectada y registrar el bloqueo: no se ajusta la expectativa ni el código para que pase.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet nuevos; `Linkubator.Domain.csproj` no cambia (CA10).
- Las integraciones con entidades y con las capas que reciben la entrada pertenecen a S3 y posteriores; la refactorización de las políticas, a S3a; la revisión con URLs reales y los casos dudosos, a S2.7 y S2.8.

### Garantías técnicas

- Las reglas de las fuentes de [spec.md → «Fuentes»](spec.md#fuentes) no cambian: el cambio se limita al tipo de error de la precondición y a los dos casos de CA03 y CA04.
- El aislamiento de Domain se mantiene conforme a [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura).
- Los mensajes de error no incluyen la entrada ni datos sensibles.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Tipo de error único `NotValidInputException` para nulo, vacío o en blanco | Confirmada | [spec.md → CA02](spec.md#criterios-de-aceptación), decisión de DLG en la sesión de la spec | Sustituye a los errores genéricos de argumento en las precondiciones de Domain. |
| `AsciiTransformationPolicy.Transform` e `ImageUrlPolicy.Adjust` lanzan en entrada vacía o en blanco en lugar de devolver cadena vacía | Confirmada | [spec.md → CA03 y CA04](spec.md#criterios-de-aceptación) | Cambio explícito de comportamiento; los llamadores deben no invocar con vacío. |
| La comprobación vive como método estático en `UserTextPolicy` | Propuesta | Esta propuesta, según [spec.md → CA07](spec.md#criterios-de-aceptación) | Evita repetir la condición en cada método; si DLG prefiere otro lugar o nombre, solo cambia la ubicación. |
| Actualizar las pruebas existentes en lugar de crear clases nuevas | Propuesta | Esta propuesta, según [spec.md → CA09](spec.md#criterios-de-aceptación) | Mantiene cada prueba junto a su política; si DLG prefiere una clase dedicada, solo cambia la ubicación. |
| Código y pruebas de la unificación escritos antes de aprobar este plan y las tareas | Pendiente | Orden de [sdd-writer](../../skills/sdd-writer/SKILL.md) | Se tratan como borrador: `tasks.md` los verifica contra este plan, completa la limpieza de CA07 y, si DLG ajusta el diseño, se rehacen. |
| La detección de esquema de `LinkUrlPolicy` sigue duplicando la de `UriSchemePolicy` | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) | Queda para S3a; no se toca aquí. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica: inventario | Políticas de `Linkubator.Domain/Policies` | Revisar cada método público que recibe texto y clasificarlo como incluido o excluido con motivo | Ningún método queda sin clasificar | Tabla de inventario en `tasks.md`. |
| V02 | CA02, CA03, CA04, CA09 | Automatizada: precondición por método | SDK .NET 10 y proyecto de tests | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~Domain.Policies` | Cada método incluido lanza `NotValidInputException` con nulo, vacío y en blanco, y no rechaza por precondición la entrada con espacios en los extremos | Comando, código de salida, resumen y casos. |
| V03 | CA05 | Automatizada: resultado vacío tras transformar | V02 | Mismo comando y filtro de V02 | Un texto no vacío que se transforma en vacío sigue lanzando el error de dominio de longitud, no `NotValidInputException` | Comando, código de salida y casos. |
| V04 | CA06 | Automatizada: mensaje sin datos | V02 | Mismo comando y filtro de V02 | El mensaje de la excepción no contiene el texto introducido | Comando, código de salida y caso. |
| V05 | CA07 | Revisión técnica: código | Cambios en Domain | Revisar cada método incluido: una sola llamada al helper al inicio y ninguna comprobación manual equivalente ni inalcanzable | Sin duplicados ni código inalcanzable | Resultado de la revisión del diff de `src/Linkubator.Domain`. |
| V06 | CA08 | Automatizada: ejemplos de S1.5 y S2.5 | V02 | `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~GenerationExamplesTests\|FullyQualifiedName~UrlPoliciesTests\|FullyQualifiedName~UrlNormalizationPolicyTests"` | Pasan salvo los casos que CA03 cambia de forma registrada | Comando, código de salida y lista de casos cambiados. |
| V07 | CA02, CA03 | Comprobación negativa de sensibilidad | V02 pasando | Quitar de forma temporal la llamada al helper en un método incluido, ejecutar el filtro de V02, confirmar que falla la prueba de ese método y restaurar exactamente el contenido anterior | La prueba detecta la alteración; tras restaurar, el filtro pasa y el diff coincide con el anterior | Cambio probado, prueba que falla y restauración comprobada. |
| V08 | CA10 | Build integrado | SDK y dependencias | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |
| V09 | CA10 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan y `Linkubator.Domain.csproj` no cambia | Comando, código de salida, resumen y diff del proyecto. |
| V10 | CA11 | Revisión técnica: inconsistencias | V01–V09 | Revisar las inconsistencias surgidas y las preguntas de [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) | Cada una está resuelta por DLG o registrada como propuesta, y ninguna implementada sin aprobación | Registro en `tasks.md`. |

V02 a V04 comprueban el comportamiento de las políticas ante entradas nulas, vacías o en blanco, pero no demuestran que las capas que reciben la entrada del usuario eviten llamar con vacío, ni que las entidades las usen; eso pertenece a S3 y posteriores. V07 demuestra la sensibilidad a la ausencia del helper en un método concreto, no a cualquier regresión. V08 y V09 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá el inventario, la verificación del código ya escrito contra este plan, la limpieza de CA07, la actualización de pruebas, la comprobación negativa, las validaciones integradas, el registro de cambios explícitos de CA03 y CA04, la sincronización del roadmap y la revisión humana, con dependencias, paralelismo justificado y checks inmediatos.
- S2.6 se cerrará cuando CA01–CA11 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado.
- Ante un cambio de regla o un resultado divergente de las fuentes, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
