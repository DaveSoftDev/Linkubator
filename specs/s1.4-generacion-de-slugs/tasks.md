# Tareas: S1.4 — Transformación común a ASCII y generación de slugs

## Referencias y autorización

- Estado: aprobado por DLG, 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 09-X-2026 («Se han estudidado todos los casos»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 09-X-2026 («Plan aprobado»).
  - Tareas: aprobadas por DLG, 09-X-2026 («Vamos al lío... Implementa»).
- Implementación:
  - Estado: completada y aceptada por DLG el 09-X-2026 («Implementación correcta»).
  - Inicio: 09-X-2026; autorización registrada con la orden explícita de DLG («Vamos al lío... Implementa»).
  - Finalización: 09-X-2026; T02–T11 completadas. Build sin advertencias ni errores y 205 pruebas correctas en la solución.
  - Evidencia consolidada: [AsciiTransformationPolicy.cs](../../src/Linkubator.Domain/Policies/AsciiTransformationPolicy.cs), [SlugPolicy.cs](../../src/Linkubator.Domain/Policies/SlugPolicy.cs), [AliasPolicy.cs](../../src/Linkubator.Domain/Policies/AliasPolicy.cs), [TextContainsUnsupportedCharactersException.cs](../../src/Linkubator.Domain/Exceptions/TextContainsUnsupportedCharactersException.cs), [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs), [SlugPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs), [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs) y [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 precede al generador porque este lanza la excepción. Tras T03, las tareas T04 y T05 son independientes y pueden ejecutarse en paralelo, igual que T06 respecto a T04 y T05. T08 modifica tests aceptados en S1.3: solo cambian los casos cuyo resultado cambia por la fuente vigente, y se dejan identificados en la sección «Casos de alias que cambian respecto a S1.3».

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 09-X-2026: «Vamos al lío... Implementa». |
| T02 | Crear `TextContainsUnsupportedCharactersException` como subtipo sellado de `DomainException`, con código `TextContainsUnsupportedCharacters` y constructor sin texto de usuario. | CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T01 | Compilar `Linkubator.Domain`; resultado esperado: la excepción existe con el patrón de las demás, sin dependencias nuevas. | completada | [TextContainsUnsupportedCharactersException.cs](../../src/Linkubator.Domain/Exceptions/TextContainsUnsupportedCharactersException.cs); build correcto el 09-X-2026. |
| T03 | Implementar el generador común de Domain que aplica la transformación de la fuente, con la tabla como datos, sin parámetro de origen y sin recortar. | CA01–CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Flujos internos](plan.md#flujos-internos) | T02 | Compilar `Linkubator.Domain`; resultado esperado: devuelve el texto ASCII o lanza la excepción de T02, sin consultar nada ni añadir dependencias. | completada | [AsciiTransformationPolicy.cs](../../src/Linkubator.Domain/Policies/AsciiTransformationPolicy.cs); build correcto el 09-X-2026; `Linkubator.Domain.csproj` sin cambios. |
| T04 | Implementar la política de slugs: llama al generador y valida vacío y rango con las excepciones de longitud, sin truncar. | CA06, CA07; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T03 | Compilar `Linkubator.Domain`; resultado esperado: un único punto de entrada que solo recibe el nombre. | completada | [SlugPolicy.cs](../../src/Linkubator.Domain/Policies/SlugPolicy.cs); contrato comprobado por [SlugPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs). |
| T05 | Adaptar `AliasPolicy` para que llame al generador común sin recortar, conservando su rango y sus palabras reservadas y manteniéndose pública. | CA08, CA09; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T03 | Compilar `Linkubator.Domain`; resultado esperado: `AliasPolicy` no transforma por su cuenta y no consulta ocupación. | completada | [AliasPolicy.cs](../../src/Linkubator.Domain/Policies/AliasPolicy.cs): sin `Trim`, sin transformación propia y con el mismo punto de entrada público. |
| T06 | Escribir los tests del generador común y de la excepción: ejemplos de la fuente, compatibilidad, tabla, rechazos y contrato. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V04 y V07 | T03 | Ejecutar los filtros de los tests nuevos; resultado esperado: pasan los ejemplos, `Ａna１`, `½`, `1½`, `µ` y `ŉ` rechazados, controles y no ASCII rechazados, y el contrato sin datos. | completada | [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs) y [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs); `dotnet test`: 205 correctas, 0 con errores, 09-X-2026. |
| T07 | Escribir los tests de la política de slugs: vacío, límites, ausencia de truncado, colección por defecto, mismo nombre como colección y etiqueta, y nombres equivalentes. | CA06, CA07; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V05 y V06 | T04 | Ejecutar los filtros de los tests nuevos; resultado esperado: los casos fuera de rango lanzan la excepción esperada y los pares equivalentes generan el mismo slug. | completada | [SlugPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs); `dotnet test`: 205 correctas, 0 con errores, 09-X-2026. |
| T08 | Actualizar los tests de alias de S1.3 a la fuente vigente, identificando los casos que cambian, y añadir los de espacios en los extremos, salto de línea final y caracteres no admitidos. | CA08, CA09; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V08 y V09 | T05 | Ejecutar `AliasPolicyTests` y `DomainExceptionTests`; resultado esperado: pasan, `Ana_López!` genera `ana-lopez` y la lista de casos cambiados queda registrada. | completada | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs); `dotnet test`: 205 correctas, 0 con errores, 09-X-2026; casos cambiados en la sección siguiente. |
| T09 | Verificar el aislamiento de Domain con la matriz de referencias y su comprobación negativa temporal. | CA10; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V10 | T04, T05 | Ejecutar `ProjectReferenceMatrixTests`; resultado esperado: la matriz detecta la referencia temporal no permitida, se descarta y la solución limpia la supera. | completada | 09-X-2026: con una referencia temporal de Domain a Application el test falló (`Expected: []`); la referencia se revirtió, `Linkubator.Domain.csproj` se restauró con `git checkout` y la matriz pasa en la suite completa. |
| T10 | Ejecutar las validaciones integradas: `dotnet test` y `dotnet build` de la solución. | CA01–CA10; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V11 y V12 | T06, T07, T08, T09 | Ejecutar ambos comandos; resultado esperado: todos los tests pasan y el build no tiene errores ni warnings. | completada | 09-X-2026, .NET SDK con xUnit 3.1.4 (net10.0): `dotnet build Linkubator.sln --no-restore -warnaserror`: 0 advertencias, 0 errores; `dotnet test Linkubator.sln --no-restore`: 205 correctas, 0 con errores, 0 omitidas. |
| T11 | Sincronizar [roadmap.md](../../context/roadmap.md) con el estado de implementación de S1.4 al iniciarla y al completarla, sin declarar aceptación. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T10 | Revisar el roadmap; resultado esperado: refleja el estado real de la implementación con enlace a este archivo, sin cerrar la etapa. | completada | [roadmap.md](../../context/roadmap.md) indica inicio y finalización de la implementación el 09-X-2026, sin aceptación. |
| T12 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T10, T11 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, que no se integraron entidades ni persistencia, y registrar aceptación o reparos de DLG. | completada | Revisión del 09-X-2026: CA01–CA10 con evidencia, enlaces de spec, plan y tareas resueltos, sin entidades ni persistencia en `src`. Aceptación de DLG, 09-X-2026: «Implementación correcta». |

## Casos de alias que cambian respecto a S1.3

La evidencia de [S1.3](../s1.3-generacion-de-alias/tasks.md#aceptación-y-cierre) corresponde al comportamiento anterior. Por la fuente vigente cambian estos casos, y no se reescribe su aceptación:

| Caso | Antes (S1.3) | Ahora |
| --- | --- | --- |
| `Ana_López!123` | `analopez123` (el `_` se eliminaba) | `ana-lopez123` (el `_` es separador de la tabla) |
| Letras o números no ASCII, como `日本語-ana-lopez` | Se eliminaban en silencio | `TextContainsUnsupportedCharactersException` |
| Tabulador, salto de línea y otros controles o separadores | Los blancos pasaban a guion | `TextContainsUnsupportedCharactersException` |
| Símbolos de la tabla, como `Straße` o `C#` | Se eliminaban | Se convierten (`strasse`, `c-sharp`) |
| Ancho completo, superíndices y compatibilidad | Se eliminaban | Se reducen a ASCII (`Ａna` → `ana`) |
| `Trim()` previo | Recortaba el texto | No recorta; los espacios en los extremos siguen sin afectar, pero un salto de línea al final se rechaza |

## Evidencia por criterio

Entorno de todas las comprobaciones: 09-X-2026, Windows, `dotnet` con net10.0 y xUnit 3.1.4 (resultado de la solución: 205 correctas, 0 con errores).

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T06 | [V01, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs) (determinismo y punto de entrada único) |
| CA02 | T03, T06 | [V01](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs) (`TransformAppliesTheCommonStepsOfTheSource`) |
| CA03 | T03, T06 | [V02](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs) (`TransformReducesCompatibilityCharactersToAscii`) |
| CA04 | T03, T06 | [V03](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs) (tabla y letras latinas) |
| CA05 | T02, T03, T06 | [V04, V07](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs), [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs) |
| CA06 | T04, T07 | [V05](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [SlugPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs) (vacío, 150 y 151, y crecimiento por símbolos) |
| CA07 | T04, T07 | [V06](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [SlugPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs) (`bandeja-de-entrada`, mismo nombre como colección y etiqueta, y nombres equivalentes) |
| CA08 | T05, T08 | [V08](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs); `Ana_López!123` genera `ana-lopez123` |
| CA09 | T05, T08 | [V09](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs), [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs); casos cambiados en la sección anterior |
| CA10 | T09, T10 | [V10, V11, V12](plan.md#estrategia-de-validación) | Completado | 09-X-2026; net10.0 / xUnit | Fallo esperado de la matriz con la referencia temporal, matriz correcta tras revertir, y 0 advertencias y 0 errores en el build |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno identificado. Los tests de S1.3 se actualizaron con los casos de la sección «Casos de alias que cambian respecto a S1.3»; la revisión de la tabla con nombres reales corresponde a S1.6.
- Aceptación humana: DLG, 09-X-2026, referencia «Implementación correcta».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado, sin cerrar otras etapas: continuar con S1.5 y S1.6 del plan de S1. El archivado de este trabajo solo se realiza si DLG lo pide expresamente.
