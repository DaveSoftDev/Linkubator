# Plan técnico: S3a.1 — Refactor de las políticas de URL

## Estado y aprobación del plan

- Estado: aprobado; implementación ejecutada. Refleja el diseño ejecutado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 13-X-2026, referencia «Cambios validados».
	- Plan: aprobado por DLG el 13-X-2026, referencia «Adelante».

## Diseño de implementación

### Componentes y contratos

Estructura resultante en `src/Linkubator.Domain/Policies`:

- `UriAuthorityPolicy`: único punto público que analiza la sintaxis de autoridad y valida su formato. Contratos: `Analyze(string)`, `IsAllowedScheme(string?)`, `IsValidHost(string)`, `IsValidPort(string?)`, `TryExtractHost(string, out string?)`, `IsValidAuthority(UriAuthority?)`, `IsHostAndPortForm(UriAuthority?)`. El record `UriAuthority` conserva `Scheme`, `Host`, `Port` e `IsProtocolRelative`; se retira `HasSchemeSeparator`, que nadie leía.
- `UriAuthoritySyntax` (interno): análisis sintáctico de la entrada. Su `Parse` sustituye al `Analyze` interno de la fachada y resuelve las cuatro formas admitidas; consume `UriHostFormat` y `UriPortFormat`.
- `UriHostFormat` (interno): formato de host admitido, antes repartido entre `UrlHostPolicy` y el interior de `UriAuthorityPolicy`.
- `UriPortFormat` (interno): formato de puerto admitido.

	Los tres colaboradores son `internal`. `UriAuthorityPolicy` delega el análisis, el formato de host y el formato de puerto; `UriAuthoritySyntax` también consume directamente `UriHostFormat` y `UriPortFormat` durante el análisis. Los consumidores productivos públicos siguen usando la fachada, cuyo contrato se conserva.
- `UrlInputPolicy`: higiene común. `Prepare(string? input, int? maximumLength = null)` recorta, rechaza vacío, espacios interiores, controles y texto con puntos de código no válidos, y comprueba la longitud cuando se indica. El parámetro de longitud preserva el orden de excepciones previo: `LinkUrlPolicy` la comprueba antes que los caracteres e `ImageUrlPolicy` después del ajuste.
- `UrlValidationPolicy`: sin cambios de contrato ni de orden de comprobaciones; sustituye su lista inline de esquemas por `UriAuthorityPolicy.IsAllowedScheme`.
- `UrlNormalizationPolicy`: conserva los resultados de normalización; conforme a T14, la entrada no absoluta pasa a lanzar `UrlNotAbsoluteException` en lugar de una excepción genérica.
- `UserTextPolicy`: sin cambios.
- `LinkUrlPolicy` e `ImageUrlPolicy`: consumen `UriAuthorityPolicy` y `UrlInputPolicy`; pierden sus copias privadas de `IsAllowedScheme`, `ContainsControls` y `ValidateAndTrim`.
- Eliminadas: `UrlHostPolicy` (absorbida), `UriSchemePolicy` y `UriSchemeDetection`.

La implementación del puerto está centralizada en `UriPortFormat`; la forma admitida y sus límites se remiten a [specifications.md → «Detección de esquema URI»](../../../context/specifications.md#detección-de-esquema-uri). Esto sustituye a las implementaciones previas divergentes.

### Flujos internos

- `LinkUrlPolicy.Adjust`: `Prepare` (con longitud) → rama `//` → análisis de autoridad → esquema explícito (admitido + autoridad válida) o autoridad sin esquema (añade `http://` en `localhost`, `https://` en el resto) o `UrlHostInvalidException`.
- `ImageUrlPolicy.Adjust`: precondiciones de entrada y base → `Prepare` (sin longitud) → análisis antes de mutar la entrada → rama `//` → esquema admitido → forma host y puerto → resolución relativa → upgrade de `http` a `https` sin reserializar → longitud y `UrlValidationPolicy.Validate`. La comprobación de esquema usa el esquema tal cual lo devuelve el análisis, sin exigir autoridad válida; la forma host y puerto sí está filtrada por `IsValidAuthority`.
- `UrlValidationPolicy.Validate`: precondición → `Uri.TryCreate` → esquema admitido → host de IPv4 escrito válido → host válido → credenciales → puerto. Orden invariante.

### Persistencia y dependencias

Sin persistencia ni dependencias nuevas: solo tipos de la BCL. La comprobación negativa de aislamiento arquitectónico sigue cubierta por `ProjectReferenceMatrixTests` y `LayerDependencyTests`.

### Garantías técnicas

- Una sola implementación de las condiciones de host y de puerto, consumida por la detección, el ajuste y la validación.
- Una sola implementación de la higiene de entrada, con el mismo tipo de excepción por caso.
- La validación conserva su apoyo en `System.Uri` y sus bordes documentados en [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados); la compensación del host escrito en IPv4 se mantiene.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Colapsar seis clases en tres y absorber `UrlHostPolicy` | Confirmada | DLG, 13-X-2026 | Reapuntar los tests de `UrlHostPolicy` a `UriAuthorityPolicy`; hecho en `UriAuthorityPolicyTests.cs` |
| Dividir `UriAuthorityPolicy` en fachada pública y tres colaboradores internos (`UriAuthoritySyntax`, `UriHostFormat`, `UriPortFormat`) | Confirmada | DLG, 13-X-2026, revisión de S3a | Movimiento de código sin cambio del contrato público ni de comportamiento: los consumidores productivos (`LinkUrlPolicy`, `ImageUrlPolicy` y `UrlValidationPolicy`) entran por la fachada; `UriAuthoritySyntax` delega directamente en los formatos internos de host y puerto. Un test fija que los colaboradores no se hagan públicos |
| Adecuar la nomenclatura de los rechazos del ajuste de URL a su causa | Confirmada | DLG, 13-X-2026 («adecuar nomenclaturas»), punto 6 de la revisión de `src/` | `UrlHostInvalidException` cuando el host está presente con un formato no admitido (`https://intranet`), `UrlPortInvalidException` cuando el host es válido y el puerto no, y `UrlAuthorityMissingException` solo para la autoridad ausente (esquema sin `://`). Cambia el código devuelto en entradas ya rechazadas; ninguna pasa de rechazada a aceptada |
| Alinear con la regla de Domain las dos excepciones genéricas que quedaban | Confirmada | DLG, 13-X-2026, revisión de `src/` | La base en blanco de `ImageUrlPolicy` lanza `NotValidInputException`, como el resto de las entradas de texto, y la entrada no absoluta de `UrlNormalizationPolicy`, `UrlNotAbsoluteException`, que es la que ya usa `UrlValidationPolicy` para la misma condición. Ninguna entrada cambia de aceptada a rechazada |
| Regla de puerto estricta (solo dígitos) | Confirmada | [specifications.md → «Detección de esquema URI»](../../../context/specifications.md#detección-de-esquema-uri) | Entradas como `:+80` cambian de clasificación; ningún test previo las fijaba y quedan fijadas por los casos nuevos |
| xUnit altera los sustitutos sueltos en `[InlineData]` | Descubierta en la implementación | Comprobación del 13-X-2026 | El rechazo de texto no válido se fija con `[Fact]`, no con teoría; descubierta al fallar `PrepareRejectsUnsupportedCharacters` con `\uD800` |
| `Normalize()` de forma C lanza `ArgumentException` con sustituto suelto | Confirmada | Comprobación del 13-X-2026 | La higiene de entrada conserva el `try/catch` que lo convierte en `TextContainsUnsupportedCharactersException` |
| Sin commit del WIP previo | Confirmada | Decisión de DLG | El diff de refactor se revisa junto al WIP hasta que el humano decida |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA06 | Automatizada: suite completa | Solución restaurada | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todas las pruebas pasan; línea base 582 antes del refactor | Conteo, código de salida y resumen |
| V02 | CA01 | Automatizada: focalizada | V01 | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~Domain.Policies" --verbosity minimal` tras cada tarea | Sin fallos tras cada incremento | Conteo por incremento |
| V03 | CA06 | Build | Solución restaurada | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | 0 errores y 0 warnings | Resumen del build |
| V04 | CA02, CA03 | Revisión técnica | Código fusionado | Buscar `UriSchemePolicy`, `UrlHostPolicy`, `IsAllowedScheme` y `IsValidPort` en `src`; inspeccionar consumidores de `UriAuthoritySyntax`, `UriHostFormat` y `UriPortFormat` | Una sola fuente de lógica por regla compartida, cero referencias a las clases eliminadas y dependencias internas coherentes con el diseño | Resultado de la búsqueda y revisión del árbol de llamadas |
| V05 | CA04, CA05 | Automatizada: nuevos casos | V02 | Tests de `UrlInputPolicyTests`, `AdjustRejectsInteriorSpaces` e `IsValidPortAcceptsOnlyDigitsWithinRange` | Los casos nuevos pasan y los bordes quedan fijados | Conteo y nombre de los tests |
| V06 | CA05 | Revisión técnica | Código fusionado | Recorrer el árbol de llamadas de cada miembro público de las políticas de URL | Ninguno sin consumidor productivo ni test | Revisión registrada |

## Orden de ejecución y cierre

- Tareas: [tasks.md](tasks.md) con la ejecución real y su evidencia.
- Criterios con evidencia satisfactoria, sin bloqueos y aceptación humana registrada en las tareas. CA06 quedó validado en la ejecución del 15-X-2026; evidencia en [evidence/ca06-2026-10-15.trx](evidence/ca06-2026-10-15.trx) y en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio).
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar. Este trabajo no cambió ninguna regla.
