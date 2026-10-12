# Plan técnico: S2.4 — Normalización de URL (`UrlNormalized`)

## Estado y aprobación del plan

- Estado: aprobado por DLG el 11-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 11-X-2026 («Comprendida»).
  - Plan: aprobado por DLG, 11-X-2026 («Aprobado»).

## Diseño de implementación

### Componentes y contratos

Se añade en `Linkubator.Domain/Policies` la política estática `UrlNormalizationPolicy`, con el mismo estilo que las demás políticas de URL. No se modifica ninguna política existente.

- `Normalize(string url)`: devuelve `UrlNormalized` según [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados). No modifica la entrada, que sigue siendo la URL ajustada y validada que se guarda. Excepciones:
  - `ArgumentNullException` o `ArgumentException`: entrada nula, vacía o en blanco. Es una precondición del llamador y no se recorta ([spec.md → CA01](spec.md#criterios-de-aceptación)).
  - `ArgumentException`: la entrada no es una URI absoluta que `System.Uri` pueda analizar. No hay excepción de dominio ni se revalida: queda fuera del contrato, según la decisión de DLG del 11-X-2026 recogida en [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes), y solo evita un fallo no controlado.
- La lista de parámetros de marketing se guarda como conjunto privado de la política, comparado sin distinguir mayúsculas. Su contenido es el de [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados); no se redefine aquí.

### Flujos internos

`Normalize` sigue el orden del [diagrama](../../../context/diagrams/process-url-normalization.md):

1. Comprueba la precondición y crea la URI con `Uri.TryCreate(url, UriKind.Absolute, ...)`.
2. Fase 1, sobre los componentes que expone `System.Uri` ya codificados:
   - Descarta `Fragment`.
   - Incluye el puerto solo si `Uri.IsDefaultPort` es `false`, lo que equivale a eliminar `80` en HTTP y `443` en HTTPS y conservar cualquier otro.
   - Toma el host de `Uri.IdnHost`. El esquema no se incluye.
   - Decodifica `AbsolutePath` una vez con `Uri.UnescapeDataString`, donde `+` es literal.
   - Separa `Query` (sin el `?` inicial) en parámetros por `&` y cada uno por el primer `=`; un parámetro sin `=` tiene valor vacío. Decodifica una vez nombre y valor con `WebUtility.UrlDecode` (BCL), donde `+` equivale a un espacio.
   - Elimina los parámetros cuyo nombre decodificado figura en la lista de marketing, sin distinguir mayúsculas.
   - Pasa a minúsculas con cultura invariante nombre y valor de los restantes, los ordena por nombre y valor con `StringComparer.Ordinal` y conserva los repetidos.
   - Une host, puerto si se conserva, ruta y parámetros con separadores (`:`, `?`, `&`, `=`) que la fase 2 elimina siempre, de modo que no influyen en la clave.
3. Fase 2, sobre la cadena unida y sin decodificar de nuevo:
   - `ToLowerInvariant`.
   - Descomposición `NormalizationForm.FormD` y eliminación de las marcas `NonSpacingMark`, conservando la letra base.
   - Sustitución explícita de `ç` por `c` y de `ñ` por `n`. Quedan cubiertas también por la descomposición, pero se mantienen para que la correspondencia con los pasos de la fuente sea trazable.
   - Conversión del espacio (U+0020) en guion medio.
   - Conservación de los puntos de código (`Rune`) de categoría Unicode Letter (`Lu`, `Ll`, `Lt`, `Lm`, `Lo`) y Number (`Nd`, `Nl`, `No`), y del guion medio; el resto se elimina.
   - Colapso de guiones medios consecutivos y eliminación de los iniciales y finales.
4. Devuelve la cadena. No hay red, resolución de nombres ni seguimiento de redirecciones.

### Persistencia y dependencias

- No hay persistencia, transacciones, red ni resolución de nombres. Solo se usan tipos de la BCL (`System.Uri`, `System.Net.WebUtility`, `System.Text`, `System.Globalization`), sin paquetes NuGet.
- `Linkubator.Domain` mantiene su aislamiento respecto a Application, Infrastructure y Web.
- Depende de S2.1–S2.3 solo en el sentido de que recibe su resultado; no reutiliza su código. S3 integra la clave en `Link`; Application y persistencia usarán `Normalize` y `UserId + UrlNormalized` para detectar el duplicado entre enlaces guardados.

### Garantías técnicas

- La normalización es una función pura y determinista, sin dependencia de la cultura del entorno (`ToLowerInvariant`, comparaciones ordinales) ([spec.md → CA14](spec.md#criterios-de-aceptación)).
- Cada componente se decodifica exactamente una vez y la fase 2 no decodifica; los componentes se separan sobre la URL codificada, por lo que `%23` y `%26` no cortan la URL ni separan parámetros ([spec.md → CA10 y CA11](spec.md#criterios-de-aceptación)).
- La política no devuelve ni altera la URL de entrada ([spec.md → CA16](spec.md#criterios-de-aceptación)).
- Las pruebas incluyen los ejemplos de la fuente, los casos negativos y los límites de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Nueva política estática `UrlNormalizationPolicy` con `Normalize` | Propuesta | [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | Mantiene el estilo de políticas de Domain. Aprobada con el plan. |
| La consulta de enlaces guardados y la comparación de claves no están en Domain | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) (DLG, 11-X-2026) | La consulta y comparación corresponden a Application y persistencia. |
| No se revalida una URL ajustada y validada; solo se comprueba que el parámetro tenga valor | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) (DLG, 11-X-2026) | Una entrada que `System.Uri` no analiza lanza `ArgumentException`, fuera del contrato. |
| Se decodifican la ruta con `Uri.UnescapeDataString` y la consulta con `WebUtility.UrlDecode`; `System.Uri` ya normaliza la ruta (por ejemplo, elimina segmentos `.` y `..`, y expone codificada la parte no ASCII) antes de la decodificación | Propuesta | [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados), que prescribe analizar con `System.Uri` | Se confirma con pruebas que `+` es literal en la ruta y un espacio en la consulta, y que las rutas en otros alfabetos se conservan. Si el comportamiento real difiere de la fuente, se detiene la tarea y se informa a DLG. |
| Una secuencia de escape mal formada (por ejemplo, `%zz`) la deja la decodificación como texto y la fase 2 elimina el `%` | Propuesta | [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | Resultado determinista del comportamiento de la BCL; la fuente no define otro tratamiento. Se registra para S2.8 si DLG quiere decidir otro. |
| Solo el espacio U+0020 se convierte en guion medio; otros espacios decodificados (por ejemplo, U+00A0) y los caracteres de control se eliminan en la fase 2 | Propuesta | [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | La fuente dice «los espacios» sin precisar. Se registra para S2.8 como caso dudoso. |
| Parámetros sin `=` y segmentos vacíos (`a&&b`) | Propuesta | [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | No cambian la clave, pues los separadores se eliminan y un valor vacío no aporta texto. |
| Las colisiones de la clave plana (marcas diacríticas en otros alfabetos, separadores eliminados) | Aceptado | [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) | No se mitiga en este trabajo. |
| La lista de parámetros de marketing se duplica en código y tests | Propuesta | [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | El test recorre cada nombre de la lista de la fuente; si la fuente cambia, se actualizan código y test. |

## Estrategia de validación

Las pruebas son unitarias de Domain, en `tests/Linkubator.Tests/Domain/Policies`: `UrlNormalizationPolicyTests` (nueva). No se requieren pruebas de UI, red ni persistencia.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: precondición | SDK .NET 10 y proyecto de tests | Tests de `Normalize` con entrada nula, vacía y en blanco | Nula lanza `ArgumentNullException`; vacía o en blanco `ArgumentException`, sin recortar | Tests ejecutados, resultado y SDK. |
| V02 | CA02, CA03, CA04 | Automatizada: fragmento, puerto y esquema | Igual que V01 | Tests con y sin fragmento, `80` en HTTP, `443` en HTTPS, `443` en HTTP, `80` en HTTPS, sin puerto, otro puerto, y `http` frente a `https` | Fragmento y esquema no influyen; solo se conserva el puerto no predeterminado | Tests ejecutados y resultado. |
| V03 | CA05 | Automatizada: host internacional | Igual que V01 | Test con dominio Unicode y su punycode, y ruta en otro alfabeto | Claves iguales para ambos dominios; la ruta en otro alfabeto se conserva | Tests ejecutados y resultado. |
| V04 | CA06, CA07, CA10, CA11 | Automatizada: decodificación única | Igual que V01 | Tests con `+` en ruta y en consulta, `%20` y `%2520`, `%23` y `%26` en ruta y consulta | `+` literal en la ruta y espacio en la consulta; `%2520` decodificado una vez; `%23` y `%26` no cortan ni separan | Tests ejecutados y resultado. |
| V05 | CA08, CA09 | Automatizada: parámetros | Igual que V01 | Test parametrizado con cada parámetro de marketing en mayúsculas y minúsculas y con un nombre no incluido; tests con orden distinto, mayúsculas distintas y repetidos | Los de la lista se eliminan; los demás se conservan en minúsculas y orden ordinal; el orden original no altera la clave | Tests ejecutados y resultado. |
| V06 | CA12 | Automatizada: fase 2 | Igual que V01 | Tests con mayúsculas, acentos, `ç`, `ñ`, espacios, caracteres de otros alfabetos, signos, guiones duplicados e iniciales o finales | Resultado conforme a la fuente, sin volver a decodificar | Tests ejecutados y resultado. |
| V07 | CA13 | Automatizada: ejemplos de la fuente | Igual que V01 | Test parametrizado con todos los ejemplos de [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados), incluidas las colisiones aceptadas | Cada resultado coincide con el del ejemplo | Tests ejecutados y resultado. |
| V08 | CA14 | Automatizada: cultura | Igual que V01 | Test que ejecuta `Normalize` con `CultureInfo.CurrentCulture` en una cultura con reglas de mayúsculas especiales (por ejemplo, `tr-TR`) y en la invariante, restaurando la cultura al terminar | Claves idénticas | Tests ejecutados y resultado. |
| V09 | CA15 | Automatizada: regresión | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~UrlValidationPolicyTests\|FullyQualifiedName~UrlPoliciesTests\|FullyQualifiedName~ImageUrlPolicyTests\|FullyQualifiedName~AliasPolicyTests\|FullyQualifiedName~SlugPolicyTests"` | Las pruebas existentes pasan | Comando, código de salida y resumen. |
| V10 | CA16 | Automatizada y revisión técnica | Diseño implementado | Test que comprueba que la entrada no cambia y revisión de que la política no usa red ni resolución de nombres | Entrada intacta; sin acceso a red | Tests ejecutados y revisión registrada. |
| V11 | CA17 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisión de `Linkubator.Domain.csproj` | Pasa la matriz; Domain no declara referencias de proyecto ni paquetes externos | Comando, código de salida, SDK y revisión. |
| V12 | CA01–CA17 | Automatizada selectiva | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~UrlNormalizationPolicyTests` | Pasan todas las pruebas de S2.4 | Comando, código de salida y resumen. |
| V13 | CA01–CA17 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida, resumen y advertencias. |
| V14 | CA01–CA17 | Build integrado | SDK y dependencias disponibles | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |

V01–V08 prueban la normalización, pero no que `Link` use el resultado ni la detección entre enlaces guardados; eso corresponde a S3, Application y persistencia. V09 y V10 son comprobaciones de límite de responsabilidad; V11 solo demuestra la matriz de referencias, no la ausencia de acceso a red, que cubre la revisión técnica. Como comprobación negativa de sensibilidad de V11, se añade temporalmente una referencia no permitida desde Domain para confirmar que la matriz la detecta, y el cambio se descarta. V12–V14 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá dependencias, paralelismo justificado y checks inmediatos.
- Criterios con evidencia satisfactoria, sin bloqueos y aceptación humana registrada en las tareas.
- Ante un cambio de regla o un caso no cubierto por la fuente, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
