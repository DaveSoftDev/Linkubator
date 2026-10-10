# Plan técnico: S2.2 — Ajuste de `Image`

## Estado y aprobación del plan

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Revisada»).
  - Plan: aprobado por DLG, 10-X-2026 («Aceptado»).

## Diseño de implementación

### Componentes y contratos

Se añade en `Linkubator.Domain/Policies` la política estática `ImageUrlPolicy`, con la estructura de `LinkUrlPolicy`, y se reutilizan sin duplicarlas las de S2.1:

- `ImageUrlPolicy`: aplica [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image). Su contrato propuesto es `Adjust(string input, string baseUrl)`, donde `baseUrl` es `Link.UrlOriginal`, ya ajustada y validada. Devuelve la URL ajustada o `""` cuando la imagen no está informada, que es la representación que usa la fuente; comunica cada rechazo con una excepción, como `LinkUrlPolicy`, para que el cliente decida el tratamiento según el origen. El contrato no recibe el origen de la imagen. Las excepciones, sin el texto introducido, son:
  - `ArgumentNullException` o `ArgumentException`: entrada o base nulas, o base no utilizable como URL absoluta. Son precondiciones del llamador.
  - `TextContainsUnsupportedCharactersException` (existente): caracteres de control o saltos de línea.
  - `UrlSchemeNotAllowedException` (existente): esquema explícito distinto de `http` y `https`.
  - `UrlAuthorityMissingException` (existente): esquema `http` o `https` sin `://` y una autoridad.
  - `TextExceedsMaximumLengthException` (existente): longitud superior al máximo.
  - `ImageHostNotAllowedException` (nueva, derivada de `DomainException`): host `localhost` o IP.
- `UriSchemePolicy` y `UrlHostPolicy` (S2.1): proporcionan la detección de esquema, la forma de host y puerto y la extracción de host. Para reconocer `localhost` y las IP se añade a `UrlHostPolicy` una comprobación sobre el host, sin cambiar sus contratos actuales; S2.3 la reutiliza al ampliar la validación.

Para el límite de longitud se reutiliza `UserTextPolicy.IsWithinMaximumLength` con el máximo de `Link.Image` de [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas), definido como constante interna de la política.

### Flujos internos

1. Comprueba que la entrada no es nula. Si está vacía o solo tiene espacios, devuelve `""` sin más comprobaciones. No recorta la entrada.
2. Rechaza los caracteres de control o saltos de línea, con el mismo criterio que `LinkUrlPolicy`.
3. Si empieza por `//`, antepone `https:`.
4. En otro caso consulta a `UriSchemePolicy`:
   - Esquema explícito: rechaza cualquier esquema distinto de `http` y `https` (sin distinguir mayúsculas), y un `http` o `https` sin `://` y una autoridad. `http` se cambia a `https` y `https` se conserva.
   - Forma de host y puerto: antepone `https://`.
   - Cualquier otra entrada: la trata como referencia relativa y la resuelve contra `baseUrl`; si el resultado usa `http`, lo cambia a `https`.
5. Extrae el host del resultado y rechaza `localhost` o una IP con `ImageHostNotAllowedException`. Una base con host `localhost` o IP produce el rechazo por esta misma comprobación.
6. Comprueba la longitud del resultado, con el esquema incluido, y rechaza si supera el máximo.
7. Devuelve el resultado sin más transformaciones.

### Persistencia y dependencias

- No hay persistencia, transacciones, red ni resolución de nombres. Solo se usan tipos de la BCL, sin paquetes NuGet.
- `Linkubator.Domain` mantiene su aislamiento respecto a Application, Infrastructure y Web.
- Depende de S2.1 (aceptada). S2.3 valida el resultado, S2.4 y S3 lo consumen y la decisión de tratamiento por origen corresponde a Application y al scraping; ninguno se adelanta en S2.2.

### Garantías técnicas

- El ajuste es una función pura y determinista, sin dependencia de la cultura del entorno y con comparaciones ordinales sin distinguir mayúsculas.
- El resultado conserva el host, la ruta, la consulta y el fragmento tal como resultan del ajuste, según [spec.md → CA15](spec.md#criterios-de-aceptación).
- Una entrada con un host o puerto no válidos que la fuente delega en la validación no se rechaza en el ajuste; S2.3 es la responsable.
- Las pruebas comprueban los casos negativos y los límites de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), incluida la longitud igual al máximo y la inmediatamente superior.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Contrato `Adjust(input, baseUrl)` que devuelve `""` para la imagen no informada y lanza excepciones para los rechazos | Confirmada | [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image); filosofía de [S2.1 → plan](../s2.1-ajuste-de-url/plan.md#componentes-y-contratos); DLG, 10-X-2026 («Aceptado») | Unifica con `LinkUrlPolicy`. |
| Una entrada vacía o de solo espacios es imagen no informada; solo la nula incumple la precondición | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes), DLG, 10-X-2026 | Sin recorte en Domain; solo las entradas en blanco se tratan como no informadas. |
| Precondición sobre `baseUrl`: no nula ni en blanco y URL absoluta; su incumplimiento lanza una excepción de argumento | Confirmada | [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image): la base llega ya ajustada y validada; DLG, 10-X-2026 («Aceptado») | La spec no cubre una base inválida porque la fuente la da por validada; se comprueba en las pruebas. |
| Excepción nueva `ImageHostNotAllowedException` para `localhost` e IP | Confirmada | [spec.md → CA11](spec.md#criterios-de-aceptación); DLG, 10-X-2026 («Aceptado») | Distingue este rechazo de los demás sin incluir el texto introducido. |
| Resolución de referencias relativas con `System.Uri`, que aplica la resolución URI estándar | Confirmada | [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image); DLG, 10-X-2026 («Aceptado») | `System.Uri` puede normalizar el resultado (mayúsculas del host, puertos por defecto, codificación). Se comprueba con los ejemplos y con casos de conservación; si altera el resultado de forma incompatible con CA15, se implementa la resolución estándar sin `System.Uri` y se informa a DLG. |
| Reconocimiento de IP: IPv4 con cuatro grupos decimales e IPv6 entre corchetes, como en el plan de S2.1 | Confirmada | [S2.1 → plan](../s2.1-ajuste-de-url/plan.md#decisiones-técnicas-y-riesgos); [specifications.md → «Validación»](../../context/specifications.md#validación); DLG, 10-X-2026 («Aceptado») | Se comprobará que `UrlHostPolicy.TryExtractHost` extrae un host IPv6 entre corchetes con y sin puerto; si no lo hace, se amplía sin romper a sus consumidores y se registra. |
| Base de resolución cuando la página del scraper ha redirigido | Pendiente, fuera de alcance | [decisions.md → «Pendientes de scraping»](../../context/decisions.md#pendientes-de-scraping) | La política recibe la base como parámetro y no decide su origen. |

## Estrategia de validación

Las pruebas son unitarias de Domain, en `tests/Linkubator.Tests/Domain/Policies`, en la clase `ImageUrlPolicyTests` y, para la comprobación de IP, en las de `UrlHostPolicy`. No se requieren pruebas de UI, red, persistencia ni revisión manual.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02, CA03 | Automatizada: precondición, imagen no informada y controles | SDK .NET 10 y proyecto de tests | Tests de `ImageUrlPolicy.Adjust` con entrada nula, vacía, solo con espacios, tabulador y saltos de línea dentro de una URL | La nula lanza `ArgumentNullException`; la vacía o en blanco devuelve `""`; los controles lanzan `TextContainsUnsupportedCharactersException` | Tests ejecutados, resultado y SDK. |
| V02 | CA04, CA05, CA06, CA07 | Automatizada: `//`, esquemas y host y puerto | Igual que V01 | Tests con `//host/img.jpg`, `http`/`https` en cualquier combinación de mayúsculas, `http:example.com`, `ftp:`, `javascript:`, `data:` y `localhost:5000/img.jpg` o `example.com:8080/x.jpg` | `//` añade `https:`; `http` pasa a `https`; los demás esquemas lanzan `UrlSchemeNotAllowedException`, o `UrlAuthorityMissingException` sin `://`; host y puerto añade `https://` | Tests ejecutados y resultado. |
| V03 | CA08, CA09, CA10 | Automatizada: referencias relativas | Igual que V01 | Tests con ruta relativa, ruta desde la raíz, `..`, ruta con consulta, solo consulta y solo fragmento, sobre bases `http` y `https` | El resultado coincide con la resolución estándar descrita en la fuente y usa `https` | Tests ejecutados y resultado. |
| V04 | CA11 | Automatizada: host prohibido | Igual que V01 | Tests con `localhost`, IPv4 e IPv6 entre corchetes por cada vía (esquema explícito, `//`, host y puerto) y con una base de host `localhost` o IP; y los de `UrlHostPolicy` sobre la comprobación de IP | Todos lanzan `ImageHostNotAllowedException` sin incluir el texto introducido; un dominio que contiene `localhost` en otra posición no se rechaza | Tests ejecutados y resultado. |
| V05 | CA12, CA13, CA15 | Automatizada: longitud, rechazo y conservación | Igual que V01 | Tests con una URL ajustada de longitud igual al máximo, otra inmediatamente superior y otra cuyo esquema añadido la hace superar el máximo; casos de conservación de mayúsculas del host, ruta, consulta y fragmento | La primera se acepta y las otras lanzan `TextExceedsMaximumLengthException`; ningún rechazo devuelve una URL; no hay transformaciones adicionales | Tests ejecutados y resultado. |
| V06 | CA16 | Automatizada: ejemplos de la fuente | Igual que V01 | Test parametrizado con todas las entradas y bases de la tabla de [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Cada resultado coincide con el de la tabla | Tests ejecutados y resultado. |
| V07 | CA14, CA17 | Revisión técnica | Diseño implementado | Revisar que el contrato no recibe el origen de la imagen y que `ImageUrlPolicy` usa `UriSchemePolicy` y `UrlHostPolicy` sin copiar su lógica | El resultado no depende del origen y no hay lógica duplicada | Revisión registrada. |
| V08 | CA18 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz; Domain no declara referencias de proyecto ni paquetes externos y no usa red ni resolución de nombres | Comando, código de salida, SDK y revisión. |
| V09 | CA01–CA18 | Automatizada selectiva | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~ImageUrlPolicyTests\|FullyQualifiedName~UrlHostPolicyTests"` | Las pruebas de S2.2 pasan | Comando, código de salida y resumen. |
| V10 | CA01–CA18 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan, incluidos los de S1 y S2.1 | Comando, código de salida, resumen y advertencias. |
| V11 | CA01–CA18 | Build integrado | SDK y dependencias disponibles | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |

V01–V06 prueban el ajuste, pero no que un valor ajustado sea una URL válida ni que `Link` lo use; eso corresponde a S2.3 y S3. V07 es una revisión del límite de responsabilidad. Como comprobación negativa de sensibilidad de V08, se añade temporalmente una referencia no permitida desde Domain para confirmar que la matriz la detecta, y el cambio se descarta. V09–V11 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá dependencias, paralelismo justificado y checks inmediatos.
- S2.2 se cierra cuando CA01–CA18 tengan evidencia satisfactoria, no queden bloqueos y DLG acepte el resultado en las tareas.
- Ante un cambio de regla o un caso no cubierto por la fuente, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
