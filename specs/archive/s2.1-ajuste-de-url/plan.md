# Plan técnico: S2.1 — Detección de esquema URI y ajuste de `UrlOriginal`

## Estado y aprobación del plan

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Aceptada revisión»).
  - Plan: aprobado por DLG, 10-X-2026 («Comprobado»).

## Diseño de implementación

### Componentes y contratos

Se añaden en `Linkubator.Domain/Policies` tres políticas estáticas, siguiendo la estructura de `AliasPolicy` y `SlugPolicy`:

- `UriSchemePolicy`: aplica [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) a una entrada y devuelve su clasificación (host y puerto, esquema explícito con su nombre, o sin esquema). Es la regla común que reutilizará S2.2. No recibe ningún parámetro que indique si la entrada es una URL de enlace o una `Image`.
- `UrlHostPolicy`: expone el reconocimiento de host válido que la detección necesita, con las condiciones de host de [specifications.md → «Validación»](../../context/specifications.md#validación). `ExtractHost(string entry)` recibe la entrada completa y devuelve el host tal como se escribió, sin puerto ni credenciales y sin cambiar mayúsculas; si no es un host válido, lanza `UrlHostInvalidException`. `TryExtractHost` es la variante que no lanza y la que usa `UriSchemePolicy`, porque una entrada sin host válido es un caso normal de la detección. S2.3 amplía esta política hasta la validación completa del host sin cambiar sus contratos.
- `LinkUrlPolicy`: aplica [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url). Su contrato es `Adjust(string input)`, que devuelve la URL ajustada y comunica cada rechazo con una excepción, como `AliasPolicy` y `SlugPolicy`, para que el cliente personalice el mensaje. La representación de «URL ajustada vacía» de la fuente corresponde al cliente que captura la excepción. Las excepciones, sin el texto introducido, son:
  - `ArgumentNullException` o `ArgumentException` (mediante `ArgumentException.ThrowIfNullOrWhiteSpace`): entrada nula, vacía o en blanco. Es una precondición del llamador, que además recorta el texto antes de llamar.
  - `TextContainsUnsupportedCharactersException` (existente): caracteres de control o saltos de línea.
  - `TextExceedsMaximumLengthException` (existente): longitud superior al máximo.
  - `UrlSchemeNotAllowedException` (nueva): esquema explícito distinto de `http` y `https`.
  - `UrlAuthorityMissingException` (nueva): esquema `http` o `https` sin `://` y una autoridad.
  - `UrlHostInvalidException` (nueva): la lanza `UrlHostPolicy.ExtractHost`; `LinkUrlPolicy` no la usa en S2.1.

Para el límite de longitud se reutiliza `UserTextPolicy.IsWithinMaximumLength` con el máximo de `Link.UrlOriginal` de [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas), definido como constante interna de la política.

### Flujos internos

1. `LinkUrlPolicy` comprueba la precondición de entrada no vacía ni en blanco y rechaza los caracteres de control o saltos de línea. No recorta la entrada.
2. Si empieza por `//`, antepone `https:` y pasa a la comprobación de longitud.
3. En otro caso consulta a `UriSchemePolicy`: una entrada de host y puerto se trata como entrada sin esquema; un esquema explícito `http` o `https`, sin distinguir mayúsculas, se conserva si cumple `://` con una autoridad; cualquier otro esquema se rechaza.
4. Sin esquema, se determina el host con `UrlHostPolicy.TryExtractHost` y se antepone `http://` si es `localhost`, o `https://` en cualquier otro caso. Si el host no es válido, como en `intranet`, se antepone `https://` y su rechazo corresponde a la validación de S2.3. La comparación con `localhost` no distingue mayúsculas ni cultura.
5. Se comprueba la longitud del resultado, con el esquema incluido. Si la supera, se rechaza.
6. El resultado se devuelve sin más transformaciones: no se normaliza el host, ni la ruta, ni la consulta, ni el fragmento.

`UriSchemePolicy` evalúa primero la forma de host y puerto, con `UrlHostPolicy.TryExtractHost` sobre el host (en IPv6, hasta `]`) y exigiendo que tras el puerto de solo dígitos termine la entrada o comience `/`, `?` o `#`. Si no la cumple, busca el prefijo de esquema antes del primer `/`, `?` o `#`.

### Persistencia y dependencias

- No hay persistencia, transacciones, red ni resolución de nombres. Solo se usan tipos de la BCL (`System`, `System.Net`, `System.Globalization`), sin paquetes NuGet.
- `Linkubator.Domain` mantiene su aislamiento respecto a Application, Infrastructure y Web.
- S2.2 reutiliza `UriSchemePolicy`; S2.3 amplía `UrlHostPolicy`; S2.4 y S3 consumen el resultado del ajuste. Ninguno se adelanta en S2.1.

### Garantías técnicas

- El ajuste y la detección son funciones puras y deterministas, sin dependencia de la cultura del entorno, comparaciones ordinales sin distinguir mayúsculas y expresiones regulares con `RegexOptions.CultureInvariant`.
- Una entrada con un host o puerto no válidos no se rechaza en el ajuste cuando la fuente delega esa comprobación en la validación (por ejemplo, un puerto fuera de rango); S2.3 es la responsable.
- Las pruebas comprueban los casos negativos y los límites de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), incluida la longitud igual al máximo y la inmediatamente superior.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| S2.1 implementa el reconocimiento de host necesario para la detección y S2.3 lo amplía | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes), DLG, 10-X-2026 | `UrlHostPolicy` se diseña para extenderse sin romper a sus consumidores. |
| Contrato `Adjust` que lanza excepciones, con precondición `ThrowIfNullOrWhiteSpace` y sin recorte | Confirmada | DLG, 10-X-2026 | Unifica la filosofía con alias y slugs. El cambio equivalente en alias y slugs se traslada a la tarea S2.8 del [mvp0-plan.md](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l). |
| `UrlHostPolicy.ExtractHost` recibe la entrada completa, devuelve el host tal como se escribió y lanza; `TryExtractHost` no lanza | Confirmada | DLG, 10-X-2026 | `ExtractHost` no tiene consumidor en S2.1, solo pruebas propias; lo usará S2.3. |
| Tres políticas separadas (`UriSchemePolicy`, `UrlHostPolicy`, `LinkUrlPolicy`) | Confirmada | [spec.md → «Alcance»](spec.md#alcance), CA12; DLG, 10-X-2026 («Comprobado») | Permite que S2.2 y S2.3 reutilicen y amplíen sin duplicar. |
| «Autoridad válida» tras `://` en el ajuste: autoridad no vacía anterior al primer `/`, `?` o `#`; host, puerto y credenciales se comprueban en S2.3 | Confirmada | [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri); DLG, 10-X-2026 («Comprobado») | La fuente no define con más detalle «autoridad válida»; se aplica la comprobación mínima para no adelantar la validación. |
| Considerar caracteres de control los de `char.IsControl` y, como saltos de línea, también U+2028 y U+2029 | Confirmada | [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url); DLG, 10-X-2026 («Comprobado») | La fuente no enumera los caracteres. |
| IPv4 reconocida solo con cuatro grupos decimales (no formas abreviadas aceptadas por `IPAddress.TryParse`) y IPv6 entre corchetes mediante `IPAddress.TryParse` | Confirmada | [specifications.md → «Validación»](../../context/specifications.md#validación); DLG, 10-X-2026 («Comprobado») | Evita aceptar formas como `127.1` como host; S2.3 puede revisarlo al ampliar. |
| Tratamiento de dominios internacionalizados en el reconocimiento de host (Unicode y `xn--`) | Pendiente | [specifications.md → «Validación»](../../context/specifications.md#validación) | Se comprobará en la implementación con ejemplos reales de IDN; si el resultado difiere de la fuente, detener la tarea y consultar a DLG. |

## Estrategia de validación

Las pruebas son unitarias de Domain, en `tests/Linkubator.Tests/Domain/Policies`: `UriSchemePolicyTests`, `UrlHostPolicyTests` y `LinkUrlPolicyTests`. No se requieren pruebas de UI, red, persistencia ni revisión manual.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada: detección de esquema | SDK .NET 10 y proyecto de tests | Tests parametrizados de `UriSchemePolicy` con host y puerto (IPv4, IPv6, `localhost`, dominio), esquemas explícitos y un host no válido con `:` | Cada entrada recibe la clasificación de la fuente | Tests ejecutados, resultado y SDK. |
| V02 | CA01, CA02 | Automatizada: reconocimiento de host | Igual que V01 | Tests de `UrlHostPolicy.ExtractHost` y `TryExtractHost` con `localhost`, IPv4, IPv6 entre corchetes, dominios con y sin punto, TLD de una letra y `xn--`, y entradas con puerto, ruta, consulta o credenciales | Se devuelve el host tal como se escribió para los hosts de la fuente; para los demás, `ExtractHost` lanza `UrlHostInvalidException` y `TryExtractHost` devuelve `false` | Tests ejecutados y resultado. |
| V03 | CA03, CA06 | Automatizada: esquemas | Igual que V01 | Tests de `LinkUrlPolicy.Adjust` con `http`/`https` en cualquier combinación de mayúsculas, `http:example.com`, `ftp:example.com`, `javascript:`, `mailto:` y similares | Solo `http` y `https` con `://` y autoridad se aceptan; `UrlSchemeNotAllowedException` o `UrlAuthorityMissingException` según el caso | Tests ejecutados y resultado. |
| V04 | CA04, CA05 | Automatizada: precondición, controles y `//` | Igual que V01 | Tests con entrada nula, vacía o solo con espacios, tabulador, saltos de línea y `//host/ruta`; y con espacios en los extremos para comprobar que no se recorta | La entrada nula, vacía o en blanco lanza `ArgumentNullException` o `ArgumentException`; los controles lanzan `TextContainsUnsupportedCharactersException`; `//` añade `https:` | Tests ejecutados y resultado. |
| V05 | CA07, CA10 | Automatizada: esquema por defecto y conservación | Igual que V01 | Tests con `localhost`, `LOCALHOST:5000`, host y puerto, dominios y mayúsculas en el host, ruta, consulta y fragmento | Se añade `http://` o `https://` y el resto de la entrada no cambia | Tests ejecutados y resultado. |
| V06 | CA08, CA09 | Automatizada: longitud y rechazo | Igual que V01 | Tests con una URL ajustada de longitud igual al máximo, otra inmediatamente superior y otra cuyo esquema añadido la hace superar el máximo | La primera se acepta y las otras lanzan `TextExceedsMaximumLengthException`; ningún rechazo incluye el texto introducido | Tests ejecutados y resultado. |
| V07 | CA11 | Automatizada: ejemplos de la fuente | Igual que V01 | Test parametrizado con todas las entradas de la tabla de [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Cada resultado coincide con el de la tabla | Tests ejecutados y resultado. |
| V08 | CA12 | Revisión técnica | Diseño implementado | Revisar que `UriSchemePolicy` no depende de `LinkUrlPolicy` y que el contrato no distingue el origen de la entrada | S2.2 puede usarla sin duplicar la regla | Revisión registrada. |
| V09 | CA13 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz; Domain no declara referencias de proyecto ni paquetes externos y no usa red ni resolución de nombres | Comando, código de salida, SDK y revisión. |
| V10 | CA01–CA13 | Automatizada selectiva | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~UriSchemePolicyTests\|FullyQualifiedName~UrlHostPolicyTests\|FullyQualifiedName~LinkUrlPolicyTests"` | Las pruebas de S2.1 pasan | Comando, código de salida y resumen. |
| V11 | CA01–CA13 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan, incluidos los de S1 | Comando, código de salida, resumen y advertencias. |
| V12 | CA01–CA13 | Build integrado | SDK y dependencias disponibles | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |

V01–V07 prueban la detección y el ajuste, pero no que un valor ajustado sea una URL válida ni que `Link` la use; eso corresponde a S2.3 y S3. V08 es una revisión del límite de responsabilidad. Como comprobación negativa de sensibilidad de V09, se añade temporalmente una referencia no permitida desde Domain para confirmar que la matriz la detecta, y el cambio se descarta. V10–V12 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá dependencias, paralelismo justificado y checks inmediatos.
- S2.1 se cierra cuando CA01–CA13 tengan evidencia satisfactoria, no queden bloqueos y DLG acepte el resultado en las tareas.
- Ante un cambio de regla o un caso no cubierto por la fuente, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
