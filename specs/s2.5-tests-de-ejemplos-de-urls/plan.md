# Plan técnico: S2.5 — Tests de los ejemplos de URL, separando `UrlOriginal` e `Image`

## Estado y aprobación del plan

- Estado: aprobado por DLG el 11-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 11-X-2026 («Ready»).
  - Plan: aprobado por DLG, 11-X-2026 («Gooo»).

## Diseño de implementación

### Componentes y contratos

- **Sin código de producción.** S2.5 no modifica `Linkubator.Domain`. Las pruebas consumen los puntos de entrada públicos ya aceptados: `LinkUrlPolicy.Adjust(input)` para `UrlOriginal`, `ImageUrlPolicy.Adjust(input, baseUrl)` para `Image`, `UrlValidationPolicy.Validate(input)` común a ambas y `UrlNormalizationPolicy.Normalize(url)` para `UrlNormalized`.
- **Auditoría previa.** Antes de escribir pruebas se cruza cada ejemplo de [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image), [«Validación»](../../context/specifications.md#validación) y [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) con las pruebas de [UrlPoliciesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs), [ImageUrlPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/ImageUrlPolicyTests.cs), [UrlValidationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlValidationPolicyTests.cs) y [UrlNormalizationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationPolicyTests.cs). Cada ejemplo se clasifica como cubierto con ese texto exacto, cubierto de forma indirecta o sin prueba. La lectura preliminar sugiere que los ejemplos de normalización ya están cubiertos, salvo la colisión aceptada de dos entradas sin esquema y las derivaciones `?q=c++ 2` y `/c++2`, y que algunos ejemplos de las tablas de ajuste y validación (como `https://192.168.1.1/pagina` o `localhost:5000/pagina-interna`) no figuran con su texto exacto. La clasificación definitiva y su evidencia se registran en `tasks.md`.
- **Clases de pruebas nuevas.** En `tests/Linkubator.Tests/Domain/Policies`, siguiendo la estructura y la etiqueta `Category` de las existentes, y con una clase por campo para que la separación sea visible:
  - `LinkUrlExamplesTests`: ajuste de `UrlOriginal` y su validación (CA01, CA03).
  - `ImageUrlExamplesTests`: ajuste de `Image` con su URL base y su validación (CA02, CA04).
  - `UrlNormalizationExamplesTests`: ejemplos de la clave, derivaciones citadas y colisión aceptada (CA06–CA08).
  - Las pruebas aceptadas de S2.1 a S2.4 no se reescriben; solo se modifican si un ejemplo es más claro de completar allí.
- **Prueba de contraste entre campos.** En `ImageUrlExamplesTests` o `LinkUrlExamplesTests`, una prueba parametrizada con el resultado esperado de cada campo escrito en los datos (el valor o el tipo de error de dominio), para no reimplementar reglas en el test (CA05).
- **Colisión aceptada.** La prueba encadena `LinkUrlPolicy.Adjust`, `UrlValidationPolicy.Validate` y `UrlNormalizationPolicy.Normalize` partiendo de las dos entradas sin esquema de la fuente (CA07). Es una cadena de llamadas dentro de la prueba, no un componente nuevo; la orquestación pertenece a S3 y posteriores.
- **Excepciones reutilizadas.** Los tipos de error de dominio ya existentes de ajuste y validación de URL, sin tipos nuevos.
- **Trazabilidad.** `tasks.md` recoge la tabla ejemplo de la fuente → sección → campo → prueba → resultado (CA09). No copia las reglas: identifica cada ejemplo por su texto de entrada y enlaza la sección.

### Flujos internos

1. Extraer de las cuatro secciones fuente la lista de ejemplos y de casos citados, sin añadir resultados que la fuente no indique.
2. Cruzar la lista con las pruebas existentes y clasificar cada ejemplo.
3. Añadir las pruebas que falten, con el texto de entrada, la base (en `Image`) y el resultado de la fuente.
4. Ejecutar las pruebas. Si el resultado de un ejemplo no coincide con la fuente, detener la tarea afectada y registrar el bloqueo: no se ajusta la expectativa de la prueba ni el código de producción para que coincida.
5. Completar la tabla de trazabilidad y ejecutar las validaciones integradas.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet nuevos.
- `Linkubator.Tests` consume Domain mediante la referencia ya existente; `Linkubator.Domain.csproj` no cambia.
- La detección de duplicados entre enlaces guardados y la integración con `Link` pertenecen a S3 y a las etapas posteriores; la revisión con URLs reales, a S2.6 y S2.7.

### Garantías técnicas

- Cada ejemplo se ejecuta con el texto de la fuente, sin recortarlo ni modificarlo, salvo que la propia fuente indique el recorte, y su resultado se compara con el de la fuente.
- Las pruebas no dependen de la cultura del entorno, de la red ni de datos externos; los textos con caracteres especiales se expresan de forma que su codificación no cambie el resultado.
- Los rechazos se comprueban por tipo de error de dominio, igual que las pruebas existentes.
- El aislamiento de Domain se mantiene conforme a [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Una clase de pruebas por campo (`LinkUrlExamplesTests`, `ImageUrlExamplesTests`) y una de normalización | Propuesta | Esta propuesta, según [spec.md → CA05](spec.md#criterios-de-aceptación) | Hace visible la separación entre `UrlOriginal` e `Image`; si DLG prefiere ampliar las clases existentes, solo cambia la ubicación. |
| La colisión aceptada se prueba con la cadena ajuste, validación y normalización dentro de la prueba | Propuesta | Esta propuesta, según [spec.md → CA07](spec.md#criterios-de-aceptación) | Las entradas de la fuente no llevan esquema; no se crea ningún componente de orquestación. |
| El resultado esperado de cada campo se escribe explícitamente en los datos de la prueba de contraste | Propuesta | Esta propuesta | Evita reimplementar reglas en el test; si la regla cambia, hay que actualizar los datos. |
| Una divergencia entre un ejemplo de la fuente y el comportamiento de Domain se registra como bloqueo | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | No se corrige código ni fuente por iniciativa propia; el cambio se tramita en el documento propietario. |
| La cobertura depende de la lista de ejemplos actual de las fuentes | Pendiente | [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces) | Si la fuente cambia durante el trabajo, la lista y las pruebas se actualizan con ella. |

## Estrategia de validación

Las pruebas son automatizadas y unitarias, en Domain y sin UI, HTTP ni persistencia. Los comandos usan filtros por nombre de clase compatibles con el runner del proyecto; si un filtro no se comporta como se espera, se registra la causa en `tasks.md` y se ajusta el comando sin ampliar el alcance.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA09 | Revisión técnica: auditoría de ejemplos | Fuentes y pruebas de S2.1 a S2.4 | Cruzar la lista de ejemplos de las cuatro secciones con las pruebas existentes | Cada ejemplo queda clasificado como cubierto, cubierto indirectamente o sin prueba | Tabla de auditoría en `tasks.md`. |
| V02 | CA01 | Automatizada: ajuste de `UrlOriginal` | SDK .NET 10 y proyecto de tests | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~LinkUrlExamplesTests` | Cada ejemplo de la tabla y cada esquema no permitido devuelve el resultado o el rechazo de la fuente | Comando, código de salida, resumen y casos. |
| V03 | CA02 | Automatizada: ajuste de `Image` | V02 | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ImageUrlExamplesTests` | Cada ejemplo con su base y cada esquema no permitido devuelve el resultado o el rechazo de la fuente | Comando, código de salida y casos comprobados. |
| V04 | CA03 | Automatizada: validación como `UrlOriginal` | V02 | Mismo comando y filtro de V02 | Cada ejemplo de la tabla de validación termina con el resultado de la fuente y los rechazos usan el error que corresponde, sin el texto validado | Comando, código de salida y casos comprobados. |
| V05 | CA04 | Automatizada: validación como `Image` | V03 | Mismo comando y filtro de V03 | Los ejemplos absolutos de la tabla de validación dan el mismo resultado que para `UrlOriginal` | Comando, código de salida y casos comprobados. |
| V06 | CA05 | Automatizada: contraste entre campos | V02, V03 | Filtros de V02 y V03 | Un mismo texto produce el resultado propio de cada campo, con las pruebas de cada campo diferenciadas | Comando, código de salida y casos comprobados. |
| V07 | CA06, CA07, CA08 | Automatizada: normalización | SDK y proyecto de tests | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~UrlNormalizationExamplesTests` | Cada ejemplo, la colisión aceptada y las derivaciones citadas producen la clave de la fuente | Comando, código de salida y casos comprobados. |
| V08 | CA01–CA08 | Comprobación negativa de sensibilidad | V02–V07 pasando | Alterar de forma temporal una sola cosa en Domain (por ejemplo, el esquema por defecto de `localhost` o un parámetro de marketing), ejecutar los filtros de V02, V03 y V07, confirmar que alguna prueba nueva falla y restaurar exactamente el contenido anterior | La prueba nueva detecta la alteración; tras restaurar, los filtros pasan y el diff de Domain queda vacío | Cambio probado, prueba que falla, restauración comprobada con `git diff` sobre Domain. |
| V09 | CA09 | Revisión técnica: trazabilidad | V01–V07 | Revisar la tabla ejemplo → sección → campo → prueba → resultado | Ningún ejemplo queda sin prueba ni con bloqueo sin registrar, y la tabla no copia reglas | Tabla en `tasks.md`. |
| V10 | CA10 | Automatizada y revisión técnica: aislamiento | Solución y test de matriz | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `git diff` de `src/Linkubator.Domain` | Pasa la matriz y no hay cambios en el código ni en el proyecto de Domain | Comando, código de salida y revisión del diff. |
| V11 | CA10 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida y resumen. |
| V12 | CA10 | Build integrado | SDK y dependencias | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |

V02–V07 comprueban el comportamiento de las políticas ante los ejemplos de la fuente, pero no demuestran que la entidad `Link` los aplique, que el flujo completo de creación detecte duplicados, ni que las URLs reales funcionen; esos puntos pertenecen a S3 y posteriores y a S2.6. V08 demuestra que las pruebas nuevas son sensibles a una alteración concreta, no a cualquier regresión. V11 y V12 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá la auditoría, las pruebas por campo y de normalización, el contraste entre campos, la comprobación negativa, la trazabilidad, las validaciones integradas, la sincronización del roadmap y la revisión humana, con dependencias, paralelismo justificado y checks inmediatos.
- S2.5 se cerrará cuando CA01–CA10 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La revisión con URLs reales (S2.6) y la decisión de los casos dudosos (S2.7) son tareas humanas posteriores que este trabajo no cierra.
- Ante un cambio de regla o un ejemplo divergente, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
