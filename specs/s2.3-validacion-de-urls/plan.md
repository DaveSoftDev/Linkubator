# Plan técnico: S2.3 — Validación de `UrlOriginal` e `Image`

## Estado y aprobación del plan

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Listo»).
  - Plan: aprobado por DLG, 10-X-2026 («Entendido»).

## Diseño de implementación

### Componentes y contratos

Se añade en `Linkubator.Domain/Policies` la política estática `UrlValidationPolicy` y se amplía `UrlHostPolicy` (S2.1) con las condiciones completas de host. Ambas se reutilizan, sin duplicar, en la validación y en la detección de esquema.

- `UrlValidationPolicy`: aplica [specifications.md → «Validación»](../../context/specifications.md#validación). Su contrato propuesto es `Validate(string url)`, sin valor de retorno: si la URL es válida no hace nada y el llamador conserva la URL ajustada tal cual, y si no, lanza una excepción de dominio sin incluir el texto validado. No recibe el campo de destino, por lo que `UrlOriginal` e `Image` obtienen el mismo resultado. Excepciones:
  - `ArgumentNullException` o `ArgumentException`: entrada nula, vacía o en blanco. Es una precondición del llamador y no se recorta.
  - `UrlNotAbsoluteException` (nueva): la entrada no es una URI absoluta válida para `System.Uri`.
  - `UrlSchemeNotAllowedException` (existente): esquema distinto de `http` y `https`.
  - `UrlHostInvalidException` (existente): host que no cumple ningún formato permitido.
  - `UrlCredentialsNotAllowedException` (nueva): credenciales en la autoridad.
  - `UrlPortInvalidException` (nueva): puerto no válido.
  - Las nuevas derivan de `DomainException`, como las existentes.
- `UrlHostPolicy` (S2.1): sin cambiar los contratos de `ExtractHost` y `TryExtractHost`, se le añaden:
  - `IsValidHost(string host)`: comprueba las condiciones de host de [«Validación»](../../context/specifications.md#validación): `localhost`, IPv4, IPv6 entre corchetes, o dominio con al menos un punto cuyo último segmento tiene 2 o más letras o está en formato `xn--`. Los dominios internacionales se evalúan en su forma punycode (`IdnHost`), de modo que Unicode y punycode dan el mismo resultado.
  - `TryExtractValidHost(string input, out string? host)`: extrae el host como `TryExtractHost` y exige además `IsValidHost`. Es el reconocimiento de «host válido» de [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri).
  - `TryExtractHost` se amplía para reconocer un host IPv6 entre corchetes, con y sin puerto, que hoy no reconoce porque separa el puerto por el primer `:`.
- `UriSchemePolicy` y `LinkUrlPolicy` (S2.1): sus decisiones de forma host y puerto pasan a `TryExtractValidHost`; el resto de su lógica no cambia. `ImageUrlPolicy` (S2.2) consume la detección de `UriSchemePolicy` y no necesita cambios propios.

### Flujos internos

`UrlValidationPolicy.Validate`, con el orden del [diagrama](../../context/diagrams/process-url-validation.md):

1. Comprueba la precondición de entrada no nula ni en blanco.
2. Crea la URI con `Uri.TryCreate(url, UriKind.Absolute, ...)`; si falla, lanza `UrlNotAbsoluteException`.
3. Rechaza un esquema distinto de `http` y `https` con `UrlSchemeNotAllowedException`.
4. Rechaza el host con `UrlHostInvalidException` si no cumple `UrlHostPolicy.IsValidHost`, aplicado a `IdnHost` para dominios y a la forma escrita para IPv4.
5. Rechaza con `UrlCredentialsNotAllowedException` si hay información de usuario.
6. Rechaza con `UrlPortInvalidException` un puerto fuera del rango 1–65535; un puerto sin especificar es válido.
7. Termina sin transformar ni devolver la URL.

Detección de esquema: una entrada con `:` solo se trata como host y puerto si `TryExtractValidHost` tiene éxito; en otro caso se evalúa por la regla de esquema explícito, como indica [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri).

### Persistencia y dependencias

- No hay persistencia, transacciones, red ni resolución de nombres. Solo se usan tipos de la BCL (`System.Uri`, `System.Net.IPAddress`), sin paquetes NuGet.
- `Linkubator.Domain` mantiene su aislamiento respecto a Application, Infrastructure y Web.
- Depende de S2.1 y S2.2 (aceptadas). S2.4 y S3 consumen el resultado; la orquestación de ajuste y validación y la decisión por origen no se adelantan en S2.3.

### Garantías técnicas

- La validación es una función pura y determinista, sin dependencia de la cultura del entorno y con comparaciones ordinales sin distinguir mayúsculas.
- No modifica la entrada: `Validate` no devuelve ninguna cadena, por lo que la URL guardada es exactamente la ajustada, según [spec.md → CA11](spec.md#criterios-de-aceptación).
- La validación no repite el rechazo de `localhost` e IP de `Image`, que pertenece al ajuste de S2.2.
- Una única implementación de las condiciones de host sirve a la validación y a la detección, según [spec.md → CA14](spec.md#criterios-de-aceptación).
- Las pruebas comprueban los casos negativos y los límites de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Contrato `Validate(string url)` sin retorno, que lanza excepciones de dominio para los rechazos | Propuesta | [specifications.md → «Validación»](../../context/specifications.md#validación); filosofía de [S2.1 → plan](../archive/s2.1-ajuste-de-url/plan.md#componentes-y-contratos) | Garantiza que no se transforma la URL. Pendiente de aprobación del plan. |
| Excepciones nuevas `UrlNotAbsoluteException`, `UrlCredentialsNotAllowedException` y `UrlPortInvalidException`; se reutilizan `UrlSchemeNotAllowedException` y `UrlHostInvalidException` | Propuesta | [spec.md → CA02, CA03, CA05, CA06, CA07](spec.md#criterios-de-aceptación) | Distinguen cada rechazo sin incluir el texto validado. |
| Un puerto no numérico o mayor que 65535 hace que `System.Uri` no cree la URI y se informa como `UrlNotAbsoluteException`; `UrlPortInvalidException` cubre los puertos que `System.Uri` acepta y la fuente no (por ejemplo, el 0) | Propuesta | [specifications.md → «Validación»](../../context/specifications.md#validación) | Ambos son rechazos con error de dominio específico. Se confirma con pruebas el comportamiento real de `System.Uri` y, si difiere, se ajusta el reparto sin cambiar la regla. |
| El host de `UrlHostPolicy.TryExtractHost` actual no reconoce IPv6 entre corchetes: `http://[2001:db8::1]/` se rechazaría en el ajuste de S2.1, aunque la tabla de «Validación» la acepta | Confirmada por lectura de `UrlHostPolicy` | [specifications.md → «Validación»](../../context/specifications.md#validación) | Se corrige al ampliar `TryExtractHost`, sin cambiar sus contratos. |
| Con `IsValidHost`, una entrada como `intranet:80/x` deja de ser host y puerto y se rechaza como esquema no permitido en el ajuste, en lugar de pasar a validación | Confirmada por la fuente | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri); [S2.1 → CA02](../archive/s2.1-ajuste-de-url/spec.md#criterios-de-aceptación) | Si una prueba existente dependía del comportamiento anterior, se detiene la tarea y se informa a DLG antes de modificarla. |
| IPv4 reconocida solo con cuatro grupos decimales (no `127.1`), como en S2.1 | Propuesta | [S2.1 → plan](../archive/s2.1-ajuste-de-url/plan.md#decisiones-técnicas-y-riesgos); [specifications.md → «Validación»](../../context/specifications.md#validación) | Las formas abreviadas que acepta `System.Uri` se rechazan; los casos dudosos se registran para S2.7. |
| `LinkUrlPolicy` duplica la detección de esquema de `UriSchemePolicy` | Pendiente, fuera de alcance | [S2.1 → plan](../archive/s2.1-ajuste-de-url/plan.md) y S2.8 | S2.3 solo sustituye el reconocimiento de host en ambas; unificarlas corresponde a S2.8. |

## Estrategia de validación

Las pruebas son unitarias de Domain, en `tests/Linkubator.Tests/Domain/Policies`: `UrlValidationPolicyTests` (nueva) y ampliaciones de `UrlPoliciesTests` e `ImageUrlPolicyTests` para la detección y la regresión. No se requieren pruebas de UI, red ni persistencia.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada: precondición y URI absoluta | SDK .NET 10 y proyecto de tests | Tests de `Validate` con entrada nula, vacía, en blanco, referencia relativa y texto que `System.Uri` no admite | Nula lanza `ArgumentNullException`; vacía o en blanco `ArgumentException`; las demás `UrlNotAbsoluteException` | Tests ejecutados, resultado y SDK. |
| V02 | CA03 | Automatizada: esquema | Igual que V01 | Tests con `http`, `https` en distintas mayúsculas, `ftp`, `file`, `javascript`, `mailto` | `http` y `https` pasan; los demás lanzan `UrlSchemeNotAllowedException` | Tests ejecutados y resultado. |
| V03 | CA04, CA05 | Automatizada: host | Igual que V01 | Tests con `localhost`, IPv4, IPv6 entre corchetes, dominio con TLD de 2 o más letras, dominio Unicode y su punycode, `xn--` como TLD; y rechazos con `intranet`, TLD de una letra, TLD con dígitos y host con caracteres no permitidos | Los primeros pasan y el dominio Unicode y su punycode dan el mismo resultado; los demás lanzan `UrlHostInvalidException` | Tests ejecutados y resultado. |
| V04 | CA06, CA07 | Automatizada: credenciales y puerto | Igual que V01 | Tests con `usuario:clave@`, sin puerto, puerto válido, puerto 0, puerto fuera de rango y no numérico | Las credenciales lanzan `UrlCredentialsNotAllowedException` sin incluirlas; las dos primeras pasan; los puertos no válidos lanzan una excepción de dominio específica | Tests ejecutados y resultado. |
| V05 | CA08, CA09, CA10, CA11 | Automatizada y revisión técnica: rechazo, igualdad, no repetición y no transformación | Igual que V01 | Comprobar que todo rechazo lanza una `DomainException` o `ArgumentException` y no incluye el texto; que `Validate` devuelve `void` y que `localhost` e IP pasan; y que el resultado es el mismo para la misma URL en cualquiera de los dos usos | Sin devolución de cadena; igualdad de resultado; `localhost` e IP aceptados | Tests ejecutados, resultado y revisión. |
| V06 | CA12 | Automatizada: ejemplos de la fuente | Igual que V01 | Test parametrizado con todas las entradas de la tabla de [specifications.md → «Validación»](../../context/specifications.md#validación): recortar, ajustar con `LinkUrlPolicy`, y validar si no se rechaza en el ajuste | Cada resultado coincide con el de la tabla | Tests ejecutados y resultado. |
| V07 | CA13, CA15 | Automatizada: detección y regresión | Igual que V01 | Tests de `UrlHostPolicy` (`IsValidHost`, `TryExtractValidHost`, IPv6 con y sin puerto), entradas `host:puerto` con host no válido (por ejemplo `intranet:80/x`) y las pruebas existentes de las tablas de «Ajuste de URL» y «Ajuste de `Image`» | Un host no válido con `:` se evalúa como esquema; todos los ejemplos de ambas tablas siguen coincidiendo | Tests ejecutados y resultado. |
| V08 | CA14 | Revisión técnica | Diseño implementado | Revisar que `UrlValidationPolicy`, `UriSchemePolicy` y `LinkUrlPolicy` usan `UrlHostPolicy` sin copiar sus condiciones | Una sola implementación de las condiciones de host | Revisión registrada. |
| V09 | CA16 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz; Domain no declara referencias de proyecto ni paquetes externos y no usa red ni resolución de nombres | Comando, código de salida, SDK y revisión. |
| V10 | CA01–CA16 | Automatizada selectiva | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~UrlValidationPolicyTests\|FullyQualifiedName~UrlPoliciesTests\|FullyQualifiedName~ImageUrlPolicyTests"` | Las pruebas de URL pasan | Comando, código de salida y resumen. |
| V11 | CA01–CA16 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan, incluidos los de S1, S2.1 y S2.2 | Comando, código de salida, resumen y advertencias. |
| V12 | CA01–CA16 | Build integrado | SDK y dependencias disponibles | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |

V01–V07 prueban la validación y el reconocimiento de host, pero no que `Link` use el resultado ni la orquestación de ajuste y validación; eso corresponde a S3 y posteriores. V08 es una revisión del límite de responsabilidad. Como comprobación negativa de sensibilidad de V09, se añade temporalmente una referencia no permitida desde Domain para confirmar que la matriz la detecta, y el cambio se descarta. V10–V12 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: [tasks.md](tasks.md), con dependencias, paralelismo justificado y checks inmediatos.
- S2.3 se cierra cuando CA01–CA16 tengan evidencia satisfactoria, no queden bloqueos y DLG acepte el resultado en las tareas.
- Ante un cambio de regla o un caso no cubierto por la fuente, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
