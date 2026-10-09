# Plan técnico: S1.4 — Transformación común a ASCII y generación de slugs

## Estado y aprobación del plan

- Estado: aprobado por DLG el 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 09-X-2026 («Se han estudidado todos los casos»).
  - Plan: aprobado por DLG, 09-X-2026 («Plan aprobado»).

## Diseño de implementación

### Componentes y contratos

- **Generador común.** Se añadirá en `Linkubator.Domain/Policies` un componente estático que aplica la [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) y devuelve el texto ASCII resultante. Recibe solo el texto: no lleva parámetro que indique si la llamada procede de un alias, una colección o una etiqueta. No recorta el texto y no valida longitud ni palabras reservadas.
- **Política de slugs.** Una política estática nueva recibe el nombre, llama al generador común y valida el resultado: vacío o por debajo del mínimo y por encima del máximo, con rango de [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas). Es el único punto de entrada para `Collection.Slug` y `Tag.Slug`.
- **Política de alias (S1.3).** `AliasPolicy.Generate` deja de recortar y de transformar por su cuenta: llama al generador común y conserva su validación propia (rango y palabras reservadas). Sigue siendo pública para que Application pueda mostrar la vista previa antes de que exista la entidad.
- **Excepción nueva.** Un subtipo sellado de `DomainException`, `TextContainsUnsupportedCharactersException`, con código estable `TextContainsUnsupportedCharacters`, para controles, separadores distintos del espacio normal y letras o números no ASCII. Su constructor no recibe el texto introducido, siguiendo el patrón de las excepciones existentes (constructor sin parámetros y código fijo).
- **Excepciones reutilizadas.** `TextBelowMinimumLengthException` y `TextExceedsMaximumLengthException` (S1.2) para los rechazos de longitud de alias y slug; `AliasReservedException` (S1.3) para las palabras reservadas.
- **Datos de la tabla.** La tabla de sustituciones de [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) se mantiene como datos dentro del generador común, sin duplicarla fuera del código ni de los tests derivados de la fuente.
- No se definirá contrato de repositorio ni abstracción para detectar colisiones de alias o de slugs: requieren `User`, `Collection`, `Tag` y persistencia, que pertenecen a los trabajos señalados en [spec.md → «Alcance»](spec.md#alcance).

### Flujos internos

1. El consumidor proporciona el texto, ya recortado, al generador común directamente o a través de la política de alias o de slugs.
2. El generador aplica en el orden de la fuente: NFKD, comprobación de controles y separadores, minúsculas, eliminación de marcas diacríticas, tabla, espacios a guiones, comprobación de letras y números no ASCII, eliminación del resto, y consolidación y recorte de guiones.
3. Si detecta controles, separadores o letras o números no ASCII, lanza `TextContainsUnsupportedCharactersException` sin generar un resultado parcial.
4. La política de alias o de slugs valida el resultado: vacío o fuera de rango lanza la excepción de longitud sin truncar; en el alias, una palabra reservada lanza `AliasReservedException`.
5. Si es admisible localmente, entrega el valor para que S3 lo asigne a `User.Alias`, `Collection.Slug` o `Tag.Slug` y los trabajos posteriores comprueben la unicidad.
6. Ninguna política añade sufijos ni prefijos, consulta existencia, recorta ni distingue colección de etiqueta.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet en este trabajo. La normalización y las categorías Unicode usan la biblioteca base de .NET.
- `Linkubator.Domain` mantiene su aislamiento respecto a Application, Infrastructure y Web; `Linkubator.Tests` consume Domain mediante la referencia ya existente.
- S3 integrará los valores en las entidades, regenerará el slug al renombrar y gestionará las excepciones de dominio; S4 detectará las colisiones y la excepción de asociar la etiqueta existente; S5 impondrá la unicidad en SQLite. Ninguno de esos componentes se adelanta en S1.4.

### Garantías técnicas

- Las pruebas cubren los ejemplos de la transformación y de la tabla, tanto con nombres de colección como de etiqueta y con alias, y los casos negativos que pertenecen a S1.4; S1.5 completa la cobertura conjunta.
- Los rechazos no incluyen el valor original ni el transformado en su contrato, conforme a [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación).
- La longitud se valida sobre el resultado de la transformación, con el conteo de puntos de código de `UserTextPolicy` y sin truncado, conforme a [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas).
- Las conversiones y comparaciones no dependen de la cultura del entorno.
- El generador es puro y determinista: el mismo texto produce siempre el mismo resultado.
- Quien llama debe recortar el texto antes (por ejemplo, con `UserTextPolicy.TrimToNull`); un salto de línea o un tabulador al final se rechazan.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Un único generador, sin parámetro de origen, para alias, slug de colección y slug de etiqueta | Confirmada | [spec.md → «Alcance»](spec.md#alcance), DLG, 09-X-2026 | Un solo punto de entrada evita divergencias entre las tres transformaciones. |
| `AliasPolicy` deja de recortar y llama al generador común | Confirmada | [spec.md → «Alcance»](spec.md#alcance), DLG, 09-X-2026 | Cambia el comportamiento aceptado en S1.3; se valida con la actualización de sus tests (CA09). |
| Política de slugs separada del generador, que valida el rango | Confirmada | DLG, 09-X-2026 | El generador solo transforma y rechaza caracteres; cada política añade su validación. |
| Normalización con `string.Normalize(NormalizationForm.FormKD)` y categorías Unicode (`UnicodeCategory`) | Propuesta | [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | Permite reducir ancho completo, superíndices y compatibilidad; los efectos sorprendentes (`™` a `tm`, `½` a `1-2`) están aceptados en [decisions.md → «Datos, URLs y búsqueda»](../../context/decisions.md#datos-urls-y-búsqueda). |
| Criterio de rechazo previo a minúsculas: toda categoría de control (`Cc`) y de separador distinta del espacio U+0020 tras NFKD; y posterior a la tabla: toda categoría de letra (`L*`) o número (`N*`) no ASCII | Propuesta | [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | Concreta las categorías para implementar los pasos 2 y 7 de la fuente. Si la revisión de S1.6 o S1.7 detecta un caso ambiguo, se tramita en el documento propietario antes de cambiar el comportamiento. |
| Un solo subtipo de excepción para controles, separadores y no ASCII | Propuesta | DLG, 09-X-2026 (nombre general) | El motivo concreto no se distingue por tipo; si S3 lo necesita, se separa entonces. |
| Mantener la detección de colisión y la asociación de la etiqueta existente fuera de S1.4 | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | No se añade dependencia de repositorio ni se simula unicidad antes de S3–S5. |
| Los tests de alias de S1.3 cambian de resultado (`Ana_López!123` pasa a `ana-lopez123`; los alias con otros alfabetos pasan de eliminarse a rechazarse) | Confirmada | [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias) | Se actualizan los tests afectados y se registran en las tareas; la evidencia de S1.3 corresponde al comportamiento anterior. |

## Estrategia de validación

Las pruebas automatizadas se añadirán como tests unitarios de Domain en `tests/Linkubator.Tests/Domain/Policies` y `tests/Linkubator.Tests/Domain/Exceptions`, siguiendo la estructura existente. No se requieren pruebas de UI, integración HTTP ni persistencia para validar este bloque aislado.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada: transformación general | SDK .NET 10 y proyecto de tests disponible | Tests parametrizados del generador con los ejemplos de la fuente y entradas con mayúsculas, diacríticos, espacios normales, puntuación, emojis y guiones duplicados o extremos; repetición del mismo texto | Cada entrada genera exactamente el resultado de la fuente y es estable | Tests ejecutados, resultado y SDK. |
| V02 | CA03 | Automatizada: normalización de compatibilidad | Generador disponible | Tests con ancho completo, superíndices, `℃`, `™`, `½`, `1½` y espacios de no separación e ideográfico | El resultado coincide con la fuente (por ejemplo, `Ａna１` genera `ana1` y `½` genera `1-2`) | Tests ejecutados y casos comprobados. |
| V03 | CA04 | Automatizada: tabla de símbolos | Generador disponible | Tests parametrizados con cada sustitución de la tabla, incluidas las letras latinas, y los pares texto/resultado de la fuente | Cada par genera el resultado esperado; `µ` y `ŉ` se rechazan | Tests ejecutados y casos comprobados. |
| V04 | CA05 | Automatizada: rechazo de caracteres no admitidos | Generador y excepción disponibles | Tests con tabulador, salto de línea, otro carácter de control, letras y números no ASCII solos y mezclados con ASCII (incluidos `日本語` y `日本語 Tokyo`) | Cada caso lanza `TextContainsUnsupportedCharactersException`, sin resultado parcial | Tests ejecutados, resultado y código comprobado. |
| V05 | CA06 | Automatizada: slug vacío, límites y ausencia de truncado | Política de slugs y excepciones de longitud disponibles | Tests con nombres que resultan vacíos y con resultados de longitud máxima y una más | Los casos fuera de rango lanzan la excepción de longitud esperada; el límite se acepta; nada se trunca | Tests ejecutados y límites comprobados. |
| V06 | CA07 | Automatizada: colección por defecto y nombres equivalentes | Política de slugs disponible | Tests con el nombre de la colección por defecto, el mismo nombre como colección y como etiqueta, y pares como «Papá» y «papa» | El primero genera el slug fijado por la fuente; el mismo nombre genera el mismo slug; los pares generan el mismo slug, sin sufijos ni prefijos | Tests ejecutados y casos comprobados. |
| V07 | CA01, CA05 | Revisión técnica y automatizada: contrato público y de errores | Generador, políticas y excepción implementados | Revisar la firma pública del generador y de las políticas, y los constructores y propiedades de la excepción; construir la excepción en tests | El generador solo recibe el texto, sin parámetro de origen; los errores no aceptan ni exponen texto de usuario | Revisión, tests y resultado. |
| V08 | CA08 | Automatizada: política de alias adaptada | Política de alias adaptada | Tests de alias con ejemplos válidos, vacíos, fuera de rango, reservados, con caracteres no admitidos y con espacios en los extremos y salto de línea final | Devuelve el resultado de la fuente sin recortar, y lanza la excepción esperada en cada rechazo; `Ana_López!` genera `ana-lopez` | Tests ejecutados y casos comprobados. |
| V09 | CA09 | Automatizada: validación de S1.3 | Tests de S1.3 actualizados a la fuente vigente | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~AliasPolicyTests` y `FullyQualifiedName~DomainExceptionTests` | Pasan; los casos cuyo resultado cambia están identificados y no hay consulta de ocupación | Comandos, código de salida, SDK y lista de casos actualizados. |
| V10 | CA10 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz y Domain no declara referencias de proyecto ni paquetes externos | Comando, código de salida, SDK y revisión. |
| V11 | CA01–CA10 | Automatizada integrada | Dependencias restauradas | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida, resumen y advertencias. |
| V12 | CA01–CA10 | Build integrado | SDK y dependencias disponibles | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings conforme a la DoD común | Comando, SDK, código de salida, errores y warnings. |

V01–V09 prueban la transformación y los rechazos locales de S1.4, pero no prueban que `User`, `Collection` o `Tag` las apliquen, que el slug se regenere al renombrar ni que otro valor del usuario colisione; esos comportamientos pertenecen a S3–S5. Los tests con nombres de colección, de etiqueta y de alias usan el mismo punto de entrada y no demuestran cómo lo invocarán las entidades. Como comprobación negativa de sensibilidad, se añadirá temporalmente una referencia no permitida desde Domain para confirmar que la matriz arquitectónica la detecta; el cambio temporal se descartará. V11 y V12 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá las tareas del generador, la política de slugs, la excepción, la adaptación de `AliasPolicy` y la actualización de sus tests, la validación de S1.3, las comprobaciones integradas, la sincronización documental del roadmap y la revisión humana.
- S1.4 se cerrará cuando CA01–CA10 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La integración con las entidades y la garantía de colisión se trazarán en S3, S4 y S5.
- Si durante la implementación se requiere transformar o rechazar un caso no cubierto por la fuente, detener la tarea afectada y tramitar una decisión en el documento propietario antes de continuar.
