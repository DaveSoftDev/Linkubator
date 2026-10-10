# Plan técnico: S1.5 — Tests de los ejemplos de alias y slugs

## Estado y aprobación del plan

- Estado: aprobado por DLG el 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 09-X-2026 («Ok»).
  - Plan: aprobado por DLG, 09-X-2026 («Entendido»).

## Diseño de implementación

### Componentes y contratos

- **Sin código de producción.** S1.5 no modifica `Linkubator.Domain`. Las pruebas consumen los puntos de entrada públicos ya aceptados: el generador común (`AsciiTransformationPolicy.Transform`), la política de alias (`AliasPolicy.Generate`) y la política de slugs (`SlugPolicy.Generate`).
- **Auditoría previa.** Antes de escribir pruebas se cruza cada ejemplo de [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii), [«Generación del alias»](../../../context/specifications.md#generación-del-alias), [«Generación de slugs»](../../../context/specifications.md#generación-de-slugs) y [«Tabla de conversión de símbolos»](../../../context/specifications.md#tabla-de-conversión-de-símbolos) con las pruebas de [AsciiTransformationPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs), [AliasPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs) y [SlugPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs). Cada ejemplo se clasifica como cubierto con ese texto exacto, cubierto de forma indirecta o sin prueba. La lectura preliminar sugiere que la mayoría de ejemplos de la transformación y de la tabla ya están cubiertos con su texto exacto, pero la clasificación definitiva y su evidencia se registran en `tasks.md`.
- **Clase de pruebas nueva.** Se añadirá `GenerationExamplesTests` en `tests/Linkubator.Tests/Domain/Policies`, siguiendo la estructura y las convenciones de nombres existentes. Contiene los ejemplos de las fuentes como datos de prueba, con la sección de la que proceden, y no sustituye ni reescribe las pruebas aceptadas de S1.3 y S1.4: solo se modifican las existentes si un ejemplo no queda cubierto con su texto exacto y es más claro completarlo allí.
- **Pruebas de ejemplos por punto de entrada.** Un grupo de pruebas parametrizadas por cada sección fuente:
  - Ejemplos de la transformación y de la tabla, contra el generador común (CA01, CA07).
  - Ejemplos de alias y palabras reservadas, contra la política de alias (CA02, CA03).
  - Ejemplos de slugs, colección por defecto y pares equivalentes, contra la política de slugs (CA04–CA06).
- **Prueba de coherencia entre usos.** Una prueba parametrizada recibe el texto, el resultado del generador y el resultado esperado de cada política, expresado explícitamente en los datos (el valor o el tipo de error de dominio) para no reimplementar las reglas de rango en el test. Comprueba que alias y slug devuelven el valor del generador cuando entra en su rango, que los rangos incumplidos lanzan el error de longitud y que los textos rechazados por el generador se rechazan igual con las dos políticas (CA08). Los ejemplos de la transformación que quedan fuera del rango del alias, como `Ana_López!` (`ana-lopez`), figuran con el rechazo por longitud esperado.
- **Excepciones reutilizadas.** `TextBelowMinimumLengthException`, `TextExceedsMaximumLengthException`, `AliasReservedException` y `TextContainsUnsupportedCharactersException`, sin tipos nuevos.
- **Trazabilidad.** `tasks.md` recoge la tabla ejemplo de la fuente → sección → prueba → resultado (CA09). No copia las reglas: identifica cada ejemplo por su texto de entrada y enlaza la sección.

### Flujos internos

1. Extraer de las cuatro secciones fuente la lista de ejemplos y de casos negativos que citan, sin añadir resultados que la fuente no indique.
2. Cruzar la lista con las pruebas existentes y clasificar cada ejemplo.
3. Añadir las pruebas que falten, usando como texto de entrada el de la fuente y como resultado esperado el de la fuente.
4. Ejecutar las pruebas. Si el resultado de un ejemplo no coincide con la fuente, detener la tarea afectada y registrar el bloqueo: no se ajusta la expectativa de la prueba ni el código de producción para que coincida.
5. Completar la tabla de trazabilidad y ejecutar las validaciones integradas.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet nuevos.
- `Linkubator.Tests` consume Domain mediante la referencia ya existente; `Linkubator.Domain.csproj` no cambia.
- La detección de colisiones, la asociación de la etiqueta existente y la integración con las entidades pertenecen a S3–S5; la revisión con nombres reales, a S1.6.

### Garantías técnicas

- Cada ejemplo se ejecuta con el texto de la fuente, sin recortarlo ni modificarlo, y su resultado se compara con el de la fuente.
- Las pruebas no dependen de la cultura del entorno ni de datos externos; las entradas con caracteres especiales se expresan de forma que su codificación no cambie el resultado.
- Los rechazos se comprueban por tipo de error de dominio, igual que las pruebas existentes.
- Los límites se comprueban con el conteo de puntos de código y sin truncado, conforme a [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| S1.5 cubre también los ejemplos de la transformación común y de la tabla de símbolos | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes), DLG, 09-X-2026 | Amplía la auditoría a las cuatro secciones fuente. |
| `Ana_López!` se trata como ejemplo del generador y su rechazo por longitud se comprueba en la política de alias | Confirmada | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes), DLG, 09-X-2026 | Evita presentarlo como alias válido. |
| Clase nueva `GenerationExamplesTests` en lugar de ampliar solo las clases existentes | Confirmada | Convenciones de [`tests/Linkubator.Tests/Domain/Policies`](../../../tests/Linkubator.Tests/Domain/Policies); aprobada con el plan por DLG, 09-X-2026 | Reúne los ejemplos de las fuentes en un único lugar trazable; puede haber ejemplos duplicados con las pruebas aceptadas, lo que no afecta a su validez. Si DLG prefiere ampliar las clases existentes, solo cambia la ubicación. |
| El resultado esperado de cada política se escribe explícitamente en los datos de la prueba de coherencia | Propuesta | Esta propuesta | Evita reimplementar en el test las reglas de rango y palabras reservadas; si la regla cambia, hay que actualizar los datos. |
| Una divergencia entre un ejemplo de la fuente y el comportamiento de Domain se registra como bloqueo | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | No se corrige código ni fuente por iniciativa propia; el cambio se tramita en el documento propietario. |
| La cobertura depende de la lista de ejemplos actual de las fuentes | Pendiente | [specifications.md → «Generación de alias y slugs»](../../../context/specifications.md#generación-de-alias-y-slugs) | Si la fuente cambia durante el trabajo, la lista y las pruebas se actualizan con ella. |

## Estrategia de validación

Las pruebas son automatizadas y unitarias, en Domain y sin UI, HTTP ni persistencia. Los comandos usan los filtros que ya empleó S1.4 con el runner del proyecto; si un filtro no se comporta como se espera, se registra la causa en `tasks.md` y se ajusta el comando sin ampliar el alcance.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA09 | Revisión técnica: auditoría de ejemplos | Fuentes y pruebas de S1.3 y S1.4 | Cruzar la lista de ejemplos de las cuatro secciones con las pruebas existentes | Cada ejemplo queda clasificado como cubierto, cubierto indirectamente o sin prueba | Tabla de auditoría en `tasks.md`. |
| V02 | CA01 | Automatizada: ejemplos del generador común | SDK .NET 10 y proyecto de tests | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~GenerationExamplesTests` | Cada ejemplo de la lista de la transformación, incluidos los dos rechazados, devuelve el resultado o el rechazo de la fuente | Comando, código de salida, resumen y casos. |
| V03 | CA07 | Automatizada: tabla de símbolos | V02 | Mismo comando y filtro | Cada entrada de la tabla, cada par de la fuente y los dos caracteres ausentes producen su resultado o su rechazo | Comando, código de salida y casos comprobados. |
| V04 | CA02, CA03 | Automatizada: alias | V02 | Mismo comando y filtro | El ejemplo del alias, cada palabra reservada con sus variantes, el vacío y los límites del rango producen el resultado o el error de la fuente | Comando, código de salida y casos comprobados. |
| V05 | CA04, CA05, CA06 | Automatizada: slugs | V02 | Mismo comando y filtro | Los ejemplos, la colección por defecto, el vacío, los límites y los pares equivalentes producen el resultado o el error de la fuente | Comando, código de salida y casos comprobados. |
| V06 | CA08 | Automatizada: coherencia entre usos | V02 | Mismo comando y filtro | El generador, el alias y el slug devuelven el mismo valor cuando entra en el rango de ambos; los rechazos del generador se repiten en ambos usos | Comando, código de salida y casos comprobados. |
| V07 | CA01–CA08 | Comprobación negativa de sensibilidad | V02–V06 pasando | Alterar de forma temporal una sola cosa en Domain, como una entrada de la tabla de símbolos o un límite del alias, ejecutar el filtro de V02, confirmar que alguna prueba nueva falla y restaurar exactamente el contenido anterior | La prueba nueva detecta la alteración; tras restaurar, el filtro vuelve a pasar y el diff de Domain queda vacío | Cambio probado, prueba que falla, restauración comprobada con `git diff` sobre Domain. |
| V08 | CA09 | Revisión técnica: trazabilidad | V01–V06 | Revisar la tabla ejemplo → sección → prueba → resultado | Ningún ejemplo queda sin prueba ni con bloqueo sin registrar, y la tabla no copia reglas | Tabla en `tasks.md`. |
| V09 | CA10 | Automatizada y revisión técnica: aislamiento | Solución y test de matriz | `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `git diff` de `src/Linkubator.Domain` | Pasa la matriz y no hay cambios en el código ni en el proyecto de Domain | Comando, código de salida y revisión del diff. |
| V10 | CA10 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida y resumen. |
| V11 | CA10 | Build integrado | SDK y dependencias | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |

V02–V06 comprueban el comportamiento de las políticas ante los ejemplos de la fuente, pero no demuestran que las entidades los apliquen, que el slug se regenere al renombrar, que las colisiones se rechacen, ni que los nombres reales de colecciones funcionen; esos puntos pertenecen a S3–S5 y a S1.6. V07 demuestra que las pruebas nuevas son sensibles a una alteración concreta, no a cualquier regresión. V10 y V11 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá la auditoría, las pruebas por sección, la prueba de coherencia, la comprobación negativa, la trazabilidad, las validaciones integradas, la sincronización del roadmap y la revisión humana, con dependencias, paralelismo justificado y checks inmediatos.
- S1.5 se cerrará cuando CA01–CA10 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La revisión con nombres reales (S1.6) y la aprobación del comportamiento (S1.7) son tareas humanas posteriores que este trabajo no cierra.
- Ante un cambio de regla o un ejemplo divergente, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
