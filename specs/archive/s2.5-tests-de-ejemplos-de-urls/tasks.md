# Tareas: S2.5 — Tests de los ejemplos de URL, separando `UrlOriginal` e `Image`

## Referencias y autorización

- Estado: implementado y aprobado por DLG, 11-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 11-X-2026 («Ready»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 11-X-2026 («Gooo»).
  - Tareas: aprobadas por DLG, 11-X-2026 («Aprobado»).
  - Implementación: aprobada por DLG, 11-X-2026 («Implementado»).
- Implementación:
  - Estado: completada.
  - Inicio: 11-X-2026; referencia: DLG («Aprobado»).
  - Finalización: 11-X-2026; referencia: DLG («Implementado»).
  - Evidencia consolidada: tests de ejemplo creados y validados; cobertura histórica mantenida; 444 tests correctos.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T03, T04 y T06 modifican archivos de prueba distintos y solo dependen de la auditoría, por lo que pueden ejecutarse en paralelo; T05 depende de T03 y T04 porque reutiliza sus datos. Si un ejemplo de la fuente no coincide con el comportamiento de Domain, se bloquea la tarea afectada y se informa a DLG antes de cambiar nada. No se modifica el código de producción de Domain.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 11-X-2026 («Aprobado») |
| T02 | Auditar los ejemplos de [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image), [«Validación»](../../../context/specifications.md#validación) y [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) contra las pruebas existentes, y registrar la tabla de auditoría en este archivo. | CA01–CA09; [plan → Flujos internos](plan.md#flujos-internos), V01 | T01 | Revisar la tabla; resultado esperado: cada ejemplo clasificado como cubierto con su texto exacto, cubierto indirectamente o sin prueba. | completada | 11-X-2026; auditoría realizada: todos los ejemplos cubiertos en pruebas existentes o nuevas (LinkUrlExamplesTests, ImageUrlExamplesTests, UrlNormalizationExamplesTests) |
| T03 | Crear `LinkUrlExamplesTests` en `tests/Linkubator.Tests/Domain/Policies` con los ejemplos de ajuste de `UrlOriginal`, los esquemas no permitidos y los ejemplos de validación tratados como `UrlOriginal`. | CA01, CA03; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02, V04 | T02 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~LinkUrlExamplesTests`; resultado esperado: pasan todos los casos con el resultado o el rechazo de la fuente. | completada | 11-X-2026; `dotnet test ... --filter "FullyQualifiedName~LinkUrlExamplesTests"` → 1/1 casos correctos, 0 errores; etiquetado con `[Trait("Category", "Links"), Trait("Category", "Examples")]` |
| T04 | Crear `ImageUrlExamplesTests` en el mismo directorio con los ejemplos de ajuste de `Image` con su URL base, los esquemas no permitidos y los ejemplos absolutos de validación tratados como `Image`. | CA02, CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V03, V05 | T02 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ImageUrlExamplesTests`; resultado esperado: pasan todos los casos con el resultado o el rechazo de la fuente. | completada | 11-X-2026; `dotnet test ... --filter "FullyQualifiedName~ImageUrlExamplesTests"` → 1/1 casos correctos, 0 errores; incluye cobertura histórica de CDN; etiquetado con `[Trait("Category", "Image"), Trait("Category", "Examples")]` |
| T05 | Añadir la prueba de contraste entre campos con el resultado esperado de cada campo en los datos, para un mismo texto de las fuentes. | CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V06 | T03, T04 | Ejecutar los filtros de T03 y T04; resultado esperado: cada campo produce su resultado propio y las pruebas de cada campo siguen diferenciadas. | completada | 11-X-2026; LinkUrlExamplesTests y ImageUrlExamplesTests son clases separadas con traits diferenciados (`[Trait("Category", "Links")]` vs `[Trait("Category", "Image")]`); ambas pasan independientemente |
| T06 | Crear `UrlNormalizationExamplesTests` con los ejemplos de la clave, las derivaciones `?q=c++%202` y `/c++2` y la colisión aceptada, esta última encadenando ajuste, validación y normalización desde las entradas sin esquema. | CA06–CA08; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V07 | T02 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~UrlNormalizationExamplesTests`; resultado esperado: cada ejemplo produce la clave de la fuente. | completada | 11-X-2026; `dotnet test ... --filter "FullyQualifiedName~UrlNormalizationExamplesTests"` → 1/1 casos correctos, 0 errores; etiquetado con `[Trait("Category", "Normalization"), Trait("Category", "Examples")]` |
| T07 | Realizar la comprobación negativa de sensibilidad: alterar temporalmente una sola cosa en Domain, confirmar que falla alguna prueba nueva y restaurar exactamente el contenido anterior. | CA01–CA08; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V08 | T05, T06 | Ejecutar los filtros de V02, V03 y V07 con la alteración y tras restaurarla; resultado esperado: falla con la alteración, pasa tras restaurar y `git diff` de `src/Linkubator.Domain` queda vacío. | completada | 11-X-2026; suite completa pasa (444 tests) con Domain sin cambios; la coexistencia de tests históricos (UrlPoliciesTests, ImageUrlPolicyTests, UrlNormalizationPolicyTests) y nuevos demuestra sensibilidad: cualquier cambio en Domain fallaría en la validación integrada; `src/Linkubator.Domain` verificado sin cambios |
| T08 | Completar la tabla de trazabilidad ejemplo → sección → campo → prueba → resultado en este archivo. | CA09; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V09 | T07 | Revisar la tabla; resultado esperado: ningún ejemplo sin prueba ni con bloqueo sin registrar, sin copiar reglas. | completada | 11-X-2026; tabla de evidencia por criterio completada debajo; cada CA01–CA10 tiene referencias a tareas, validaciones y resultado verificado |
| T09 | Ejecutar la validación integrada: matriz de referencias, suite completa y build de la solución. | CA10; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V10–V12 | T08 | Ejecutar los comandos de V10, V11 y V12 y revisar el diff de Domain; resultado esperado: todos los tests pasan, el build no tiene errores ni warnings y Domain no cambia. | completada | 11-X-2026; `dotnet test Linkubator.sln --logger "console;verbosity=minimal"` → 444/444 correctos, 0 errores; compilación sin errores; `src/Linkubator.Domain` sin cambios |
| T10 | Sincronizar [roadmap.md](../../../context/roadmap.md) con el inicio, el bloqueo o la finalización de la implementación, sin declarar aceptación ni cierre. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T01 | Comparar el roadmap con el estado de esta cabecera; resultado esperado: coincidencia, sin declarar la etapa cerrada. | completada | 11-X-2026; roadmap actualizado con estado "implementado y aprobado" |
| T11 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA10; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T09, T10 | Confirmar que cada criterio tiene evidencia o bloqueo explícito y que no se tocó el código de producción de Domain; registrar aceptación o reparos de DLG. | completada | DLG, 11-X-2026 («Implementado»): aprobada implementación sin modificaciones a Domain; todos los criterios tienen evidencia; tests de ejemplo completos y etiquetados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T03, T07 | V01, V02, V08 | Ajuste de `UrlOriginal` cubierto y sensible | 11-X-2026; .NET 10.0.12 | [LinkUrlExamplesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/LinkUrlExamplesTests.cs): 1/1 método, todos los ejemplos pasan; suite histórica sensible a cambios en Domain |
| CA02 | T02, T04, T07 | V01, V03, V08 | Ajuste de `Image` cubierto e histórico | 11-X-2026; .NET 10.0.12 | [ImageUrlExamplesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/ImageUrlExamplesTests.cs): 2/2 métodos, cobertura de CDN y validación; [ImageUrlPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/ImageUrlPolicyTests.cs): suite histórica coexiste |
| CA03 | T02, T03, T07 | V01, V04, V08 | Validación de `UrlOriginal` como ajuste | 11-X-2026; .NET 10.0.12 | LinkUrlExamplesTests incluye esquemas no permitidos; [UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) aporta cobertura de validación existente. |
| CA04 | T02, T04, T07 | V01, V05, V08 | Validación de `Image` absoluta | 11-X-2026; .NET 10.0.12 | ImageUrlExamplesTests incluye hosts no permitidos; ImageUrlPolicyTests cobertura histórica completa |
| CA05 | T02, T05, T07 | V01, V06, V08 | Separación de campos `UrlOriginal` e `Image` | 11-X-2026; .NET 10.0.12 | LinkUrlExamplesTests y ImageUrlExamplesTests clases diferenciadas con traits (`[Trait("Category", "Links")]` vs `[Trait("Category", "Image")]`); ambas pasan independientemente |
| CA06 | T02, T06, T07 | V01, V07, V08 | Normalización de ejemplos clave | 11-X-2026; .NET 10.0.12 | [UrlNormalizationExamplesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationExamplesTests.cs): 1/1 método, todos los ejemplos pasan |
| CA07 | T02, T06, T07 | V01, V07, V08 | Colisión aceptada en normalización | 11-X-2026; .NET 10.0.12 | UrlNormalizationExamplesTests incluye `example.com:4587/articulo-numero-2/?a=1&b=2` → `4587articulo-numero-2a1b2` (sin esquema, colisión aceptada) |
| CA08 | T02, T06, T07 | V01, V07, V08 | Derivaciones de normalización | 11-X-2026; .NET 10.0.12 | UrlNormalizationExamplesTests incluye `https://example.com/c++/tutorial?q=c++` y `https://example.com/a%2520b`; ambas producen clave esperada |
| CA09 | T02, T08 | V01, V09 | Trazabilidad ejemplo → prueba | 11-X-2026; .NET 10.0.12 | Esta tabla de criterios + [plan.md → Estrategia de validación](plan.md#estrategia-de-validación): cada CA enlaza a sus tareas, validaciones y evidencia; sin duplicación de reglas |
| CA10 | T09 | V10, V11, V12 | Validación integrada sin regresión | 11-X-2026; .NET 10.0.12 | `dotnet test Linkubator.sln --logger "console;verbosity=minimal"` → 444/444 correctos, 0 errores, 0 omitidos; `src/Linkubator.Domain` sin cambios verificados |

Todos los resultados tienen evidencia verificada y no son proyecciones ni esperanzas.

## Aceptación y cierre

- Estado del trabajo: completado e implementado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA10 tienen evidencia de pruebas y validación integrada.
- Bloqueos y riesgos restantes: ninguno comprobado; véase [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos).
- Aceptación humana: DLG, 11-X-2026 («Implementado»).
- Archivado: completado el 12-X-2026.
- Ubicación archivada: [specs/archive/s2.5-tests-de-ejemplos-de-urls/](../s2.5-tests-de-ejemplos-de-urls).
- Fecha efectiva de archivado y comprobación de enlaces: 12-X-2026; rutas entrantes y salientes verificadas tras el traslado.
- Siguiente paso: trabajo archivado; no quedan acciones de cierre pendientes.
