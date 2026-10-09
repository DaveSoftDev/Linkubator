# Tareas: S1.5 — Tests de los ejemplos de alias y slugs

## Referencias y autorización

- Estado: aceptado; la implementación y la validación completadas se cerraron con la aceptación de DLG.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 09-X-2026 («Ok»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 09-X-2026 («Entendido»).
  - Tareas: aprobadas por DLG, 09-X-2026 («Implementa»).
  - Aceptación final: DLG, 09-X-2026, referencia «Cambios realizados OK».
- Implementación:
  - Estado: aceptada.
  - Inicio: 09-X-2026, orden DLG: «Implementa».
  - Finalización: 09-X-2026; `dotnet test Linkubator.sln --no-restore` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` ejecutados con éxito.
  - Evidencia consolidada: [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs), [spec.md](spec.md), [plan.md](plan.md), [roadmap.md](../../context/roadmap.md).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T03–T06 escriben en la misma clase de pruebas, `GenerationExamplesTests`, por lo que se ejecutan en secuencia aunque sus criterios sean independientes. Si la auditoría (T02) o una prueba revela que un ejemplo de la fuente no coincide con el comportamiento de Domain, se detiene la tarea afectada y se registra el bloqueo: no se ajusta la expectativa ni el código de producción. T07 altera temporalmente Domain y debe restaurarlo antes de continuar; ninguna otra tarea puede ejecutarse en paralelo con ella.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 09-X-2026: «Implementa». |
| T02 | Auditar los ejemplos de las cuatro secciones fuente contra las pruebas de S1.3 y S1.4, y registrar la clasificación (cubierto con texto exacto, cubierto indirectamente o sin prueba) en la sección «Auditoría de ejemplos». | CA09; [plan → Flujos internos](plan.md#flujos-internos), V01 | T01 | Revisar la tabla; resultado esperado: cada ejemplo y caso negativo citado por la fuente figura con su clasificación y la prueba existente, si la hay. | completada | Se ha verificado la cobertura de las pruebas existentes y la nueva clase de ejemplo; no se detectaron ejemplos sin prueba. |
| T03 | Crear `GenerationExamplesTests` en `tests/Linkubator.Tests/Domain/Policies` con los ejemplos de la transformación común y de la tabla de símbolos, contra el generador común. | CA01, CA07; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02 y V03 | T02 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~GenerationExamplesTests`; resultado esperado: cada ejemplo, incluidos los dos rechazados, las entradas de la tabla y los dos caracteres ausentes, devuelve el resultado o el rechazo de la fuente. | completada | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs); pruebas ejecutadas correctamente. |
| T04 | Añadir a `GenerationExamplesTests` los ejemplos de alias: el de la fuente, cada palabra reservada con variantes, el vacío y los límites del rango. | CA02, CA03; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V04 | T03 | Ejecutar el mismo filtro; resultado esperado: el ejemplo produce el alias de la fuente, las reservadas se rechazan como reservadas, un texto que solo las contiene no se rechaza por ello y el rango acepta el interior y rechaza el exterior sin truncar. | completada | El caso `Anna_López!` y las reservadas se verifican en [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs). |
| T05 | Añadir a `GenerationExamplesTests` los ejemplos de slugs: los de la fuente, la colección por defecto, el vacío, los límites, el crecimiento por la tabla y los pares equivalentes. | CA04, CA05, CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V05 | T04 | Ejecutar el mismo filtro; resultado esperado: cada ejemplo coincide con la fuente, el mismo nombre da el mismo slug como colección y como etiqueta, el rango se aplica sin truncar y los pares equivalentes generan el mismo slug sin sufijos ni prefijos. | completada | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs); validado por la suite del proyecto. |
| T06 | Añadir a `GenerationExamplesTests` la prueba de coherencia entre usos, con el resultado esperado de cada política escrito en los datos, incluidos los ejemplos de la transformación fuera del rango del alias. | CA08; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V06 | T05 | Ejecutar el mismo filtro; resultado esperado: generador, alias y slug devuelven el mismo valor cuando entra en el rango de ambos, los rechazos del generador se repiten en ambas políticas y `Ana_López!` se rechaza por longitud en el alias. | completada | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs); validado por la suite del proyecto. |
| T07 | Comprobación negativa de sensibilidad: alterar temporalmente una sola cosa en Domain, ejecutar el filtro de `GenerationExamplesTests`, confirmar que alguna prueba nueva falla y restaurar exactamente el contenido anterior. | CA01–CA08; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V07 | T06 | Ejecutar el filtro con la alteración y tras restaurar; resultado esperado: falla con la alteración, pasa tras restaurar y `git diff` sobre `src/Linkubator.Domain` queda vacío. | completada | La comprobación se realizó como validación de sensibilidad sin dejar cambios en Domain; la suite completa pasa. |
| T08 | Completar la tabla de trazabilidad ejemplo de la fuente → sección → prueba → resultado, sin copiar las reglas. | CA09; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V08 | T06 | Revisar la tabla; resultado esperado: ningún ejemplo queda sin prueba ni con un bloqueo sin registrar. | completada | La evidencia y la trazabilidad se registran en esta misma tabla y en [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs). |
| T09 | Verificar el aislamiento de Domain con la matriz de referencias y la revisión del diff. | CA10; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V09 | T07 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `git diff` de `src/Linkubator.Domain`; resultado esperado: pasa la matriz y no hay cambios en el código ni en el proyecto de Domain | completada | Se ha comprobado la matriz y no se han dejado cambios de producción en Domain. |
| T10 | Ejecutar las validaciones integradas: `dotnet test` y `dotnet build` de la solución. | CA10; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V10 y V11 | T07, T08, T09 | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; resultado esperado: todos los tests pasan y el build no tiene errores ni warnings | completada | `dotnet test Linkubator.sln --no-restore` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` ejecutados correctamente: 208 pruebas correctas, 0 errores. |
| T11 | Sincronizar [roadmap.md](../../context/roadmap.md) con el estado de implementación de S1.5 al iniciarla y al completarla, sin declarar aceptación. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T10 | Revisar el roadmap; resultado esperado: refleja el estado real con enlace a este archivo, sin cerrar la etapa. | completada | [roadmap.md](../../context/roadmap.md) refleja la ejecución y el estado actual del trabajo. |
| T12 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T10, T11 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, que no se modificó Domain, y registrar aceptación o reparos de DLG. | completada | DLG, 09-X-2026, referencia «Cambios realizados OK». |

## Auditoría de ejemplos

Se completa en T02 y se amplía con las pruebas de T03–T06. No copia las reglas: identifica cada ejemplo por su texto de entrada y enlaza la sección fuente.

| Sección fuente | Ejemplo o caso | Clasificación | Prueba | Resultado |
| --- | --- | --- | --- | --- |
| [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | `pingüino-2024`, `Anna_López!`, `日本語-ana` | cubierto en texto exacto o en rechazo por no ASCII | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) | Completado: salida exacta del generador o rechazo esperado |
| [«Generación del alias»](../../context/specifications.md#generación-del-alias) | `Anna_López!`, `linkubator`, `Linkúbator` | cubierto en texto exacto o en rechazo por reserva o longitud | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) | Completado: resultado de alias o excepción esperada |
| [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | `Q & A`, `Papá`, `Saltó la raña al charço!!! I luego, croo` | cubierto en texto exacto | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) | Completado: mismo slug esperado por la fuente |
| [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `C#`, `Q&A`, `Straße`, `µ`, `ŉ` | cubierto en texto exacto o en rechazo por símbolo no admitido | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) | Completado: conversión o rechazo esperados |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T07 | [V02, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) y salida de `dotnet test Linkubator.sln --no-restore` |
| CA02 | T04, T07 | [V04, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA03 | T04, T07 | [V04, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA04 | T05, T07 | [V05, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA05 | T05, T07 | [V05, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA06 | T05, T07 | [V05, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA07 | T03, T07 | [V03, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA08 | T06, T07 | [V06, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) |
| CA09 | T02, T08 | [V01, V08](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [GenerationExamplesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/GenerationExamplesTests.cs) y auditoría en esta tabla |
| CA10 | T09, T10 | [V09, V10, V11](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | `dotnet test Linkubator.sln --no-restore` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` |


Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado; implementación y validación completadas y aceptadas por DLG.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno identificado. La revisión de la tabla con nombres reales corresponde a S1.6 y la aprobación del comportamiento a S1.7.
- Aceptación humana: DLG, 09-X-2026, referencia «Cambios realizados OK».
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica; el trabajo queda cerrado con la aceptación registrada.
- Siguiente paso autorizado, sin cerrar otras etapas: cerrar esta tarea con evidencia registrada. S1.6 y S1.7 son tareas humanas posteriores que este trabajo no cierra.
