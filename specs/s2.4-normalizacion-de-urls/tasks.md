# Tareas: S2.4 — Normalización de URL (`UrlNormalized`)

## Referencias y autorización

- Estado: aprobado por DLG, 11-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 11-X-2026 («Comprendida»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 11-X-2026 («Aprobado»).
  - Tareas: aprobadas por DLG, 11-X-2026 («Listo»).
- Implementación:
  - Estado: completada y aceptada.
  - Inicio: 11-X-2026; referencia: DLG («Listo»).
  - Finalización: 11-X-2026; suite completa 420/420 pruebas correctas y build sin errores ni warnings (.NET 10.0.12).
  - Evidencia consolidada: tabla «Evidencia por criterio» y [«Aceptación y cierre»](#aceptación-y-cierre).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 y T03 modifican el mismo archivo (`UrlNormalizationPolicy`) y T04 y T05 la misma clase de pruebas, por lo que se ejecutan en secuencia. Si el comportamiento real de `System.Uri`, `Uri.UnescapeDataString` o `WebUtility.UrlDecode` difiere de lo previsto en [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos), o un ejemplo de la fuente no coincide, se bloquea la tarea afectada y se informa a DLG antes de cambiar nada. No se modifican las políticas existentes ni la entidad `Link`.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA17; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 11-X-2026 («Listo») |
| T02 | Crear `UrlNormalizationPolicy` en `Linkubator.Domain/Policies` con `Normalize`: precondición, creación de la URI y fase 1 (fragmento, puerto, host, ruta, consulta, parámetros de marketing, minúsculas, orden y unión), con la lista de parámetros como conjunto privado comparado sin distinguir mayúsculas. | CA01–CA11, CA14, CA16; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Flujos internos](plan.md#flujos-internos) | T01 | Compilar `Linkubator.Domain`; resultado esperado: compila sin warnings, solo con tipos de la BCL, y la lista de parámetros coincide con la de [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados). | completada | [UrlNormalizationPolicy.cs](../../src/Linkubator.Domain/Policies/UrlNormalizationPolicy.cs) validado con la suite de URL |
| T03 | Completar `Normalize` con la fase 2 sobre la cadena unida: minúsculas invariantes, descomposición y eliminación de marcas, `ç` y `ñ`, espacio a guion, conservación de letras, números y guion, y colapso y recorte de guiones. | CA12, CA14; [plan → Flujos internos](plan.md#flujos-internos) | T02 | Compilar `Linkubator.Domain`; resultado esperado: compila sin warnings, sin segunda decodificación y sin dependencias nuevas. | completada | [UrlNormalizationPolicy.cs](../../src/Linkubator.Domain/Policies/UrlNormalizationPolicy.cs) con la fase 2 verificada |
| T04 | Crear `UrlNormalizationPolicyTests` en `tests/Linkubator.Tests/Domain/Policies` con los tests de precondición, fragmento, puerto, esquema, host internacional, decodificación única (`+`, `%20`, `%2520`, `%23`, `%26`) y parámetros (lista de marketing, orden, mayúsculas y repetidos). | CA01–CA11; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V05 | T03 | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~UrlNormalizationPolicyTests`; resultado esperado: pasan todos los casos. | completada | [UrlNormalizationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationPolicyTests.cs); 14/14 pruebas correctas |
| T05 | Ampliar `UrlNormalizationPolicyTests` con la fase 2, todos los ejemplos de la fuente (incluida la colisión aceptada), la cultura (`tr-TR` frente a la invariante) y la no modificación de la entrada. | CA12–CA14, CA16; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06–V08, V10 | T04 | Ejecutar el mismo filtro de `UrlNormalizationPolicyTests`; resultado esperado: pasan todos los casos y cada ejemplo coincide con la fuente. | completada | [UrlNormalizationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationPolicyTests.cs) ejecutado con éxito |
| T06 | Verificar el aislamiento de Domain y la regresión: matriz de referencias con su comprobación negativa temporal (revertida), revisión de que la política no usa red ni resolución de nombres y ejecución de las pruebas existentes de ajuste, validación, alias y slugs. | CA15–CA17; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V09–V11 | T05 | Ejecutar los filtros de V09 y V11 del plan y revisar el código; resultado esperado: la matriz detecta la infracción temporal, el cambio se descarta y las pruebas existentes pasan. | completada | Regresión: `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~UrlValidationPolicyTests|FullyQualifiedName~UrlHostPolicyTests|FullyQualifiedName~UrlPoliciesTests|FullyQualifiedName~ImageUrlPolicyTests|FullyQualifiedName~UrlNormalizationPolicyTests"`; 212/212 correctas. Comprobación negativa (11-X-2026): con una `ProjectReference` temporal de Domain a Web, `dotnet test --no-build --filter FullyQualifiedName~ProjectReferenceMatrixTests` falla (esperado sin referencias, real con la referencia); el `.csproj` se restauró desde git sin diferencias y la suite completa pasa 420/420. Revisión de código: `UrlNormalizationPolicy` solo usa tipos de la BCL, sin red ni resolución de nombres |
| T07 | Ejecutar la validación integrada: suite completa y build de la solución. | CA01–CA17; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V12–V14 | T06 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: todos los tests pasan y el build no tiene errores ni warnings. | completada | Build correcto sin errores ni warnings; suite completa 420/420 pruebas correctas (11-X-2026) |
| T08 | Sincronizar [roadmap.md](../../context/roadmap.md) con el inicio, el bloqueo o la finalización de la implementación, sin declarar aceptación ni cierre. | CA01–CA17; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T01 | Comparar el roadmap con el estado de esta cabecera; resultado esperado: coincidencia, sin declarar la etapa cerrada. | completada | [roadmap.md](../../context/roadmap.md) actualizado el 11-X-2026 |
| T09 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA17; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T07, T08 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, que no se tocaron políticas existentes ni la entidad `Link`, y registrar aceptación o reparos de DLG. | completada | DLG, 11-X-2026 («Implementado») |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | [UrlNormalizationPolicy.cs](../../src/Linkubator.Domain/Policies/UrlNormalizationPolicy.cs), UrlNormalizationPolicyTests |
| CA02 | T02, T04 | V02, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA03 | T02, T04 | V02, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA04 | T02, T04 | V02, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA05 | T02, T04 | V03, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA06 | T02, T04 | V04, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA07 | T02, T04 | V04, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA08 | T02, T04 | V05, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA09 | T02, T04 | V05, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA10 | T02, T04 | V04, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA11 | T02, T04 | V04, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA12 | T03, T05 | V06, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA13 | T05 | V07, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests (ejemplos de la fuente) |
| CA14 | T02, T03, T05 | V08, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests (cultura invariante) |
| CA15 | T06 | V09, V13 | Implementado y probado | 11-X-2026; .NET 10.0.12 | Regresión verificada: 212 tests de URL correctos |
| CA16 | T02, T05, T06 | V10, V12 | Implementado y probado | 11-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests |
| CA17 | T06, T07 | V11, V13, V14 | Implementado y probado | 11-X-2026; .NET 10.0.12 | [UrlNormalizationPolicy.cs](../../src/Linkubator.Domain/Policies/UrlNormalizationPolicy.cs) sin referencias de proyecto nuevas; build exitoso; comprobación negativa de la matriz registrada en T06 |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completada.
- Criterios con evidencia: CA01–CA17 (cobertura completa de normalización, invariancia de cultura y aislamiento de Domain).
- Criterios sin evidencia satisfactoria: ninguno; todas las tareas de implementación y validación completadas.
- Bloqueos y riesgos restantes: ninguno comprobado; los riesgos técnicos están en el [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos).
- Aceptación humana: aceptada por DLG, 11-X-2026 («Implementado»).
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado, sin cerrar otras etapas: archivar S2.4 si DLG lo solicita.
