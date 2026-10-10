# Tareas: S2.2 — Ajuste de `Image`

## Referencias y autorización

- Estado: aprobado por DLG, 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Revisada»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 10-X-2026 («Aceptado»).
  - Tareas: aprobadas por DLG, 10-X-2026 («Listo»).
- Implementación:
  - Estado: completada.
  - Inicio: 10-X-2026; referencia registrada por la aprobación explícita de DLG («Listo»).
  - Finalización: 10-X-2026; validación: `dotnet test Linkubator.sln` sin errores (257 pruebas correctas). Referencia: DLG («Funciona»).
  - Evidencia consolidada: [UrlPoliciesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) ImageUrlPolicyTests, [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), [ImageHostNotAllowedException.cs](../../src/Linkubator.Domain/Exceptions/ImageHostNotAllowedException.cs).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 precede a T03 y T04 porque ambas lanzan o usan `ImageHostNotAllowedException`. T05 solo prueba `UrlHostPolicy` y puede ejecutarse en paralelo con T04, porque no comparten archivos. T06 y T07 amplían la misma clase de pruebas, por lo que se ejecutan en secuencia. Las comprobaciones sobre `System.Uri` e IPv6 del [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) se hacen en T03 y T04; si el resultado obliga a abandonar la decisión confirmada, se bloquea la tarea y se informa a DLG.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA18; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 10-X-2026 («Listo») |
| T02 | Implementar en Domain `ImageHostNotAllowedException`, derivada de `DomainException`, sin aceptar ni exponer el texto introducido. | CA11, CA13; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T01 | Compilar `Linkubator.Domain`; resultado esperado: excepción disponible con código estable, sin datos del usuario ni dependencias nuevas. | completada | [ImageHostNotAllowedException.cs](../../src/Linkubator.Domain/Exceptions/ImageHostNotAllowedException.cs), compilación sin errores |
| T03 | Añadir a `UrlHostPolicy` la comprobación de host `localhost` o IP (IPv4 con cuatro grupos decimales e IPv6 entre corchetes), y comprobar que `TryExtractHost` extrae un host IPv6 entre corchetes con y sin puerto; si no lo hace, ampliarlo sin cambiar sus contratos. | CA11, CA17; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T02 | Compilar `Linkubator.Domain`; resultado esperado: comprobación disponible, contratos existentes intactos y IPv6 entre corchetes reconocida. | completada | `UrlHostPolicy` sin cambios en contratos, IPv6 entre corchetes soportado por `System.Uri` |
| T04 | Implementar `ImageUrlPolicy.Adjust(input, baseUrl)` con precondiciones, imagen no informada, rechazo de controles, `//`, esquemas, host y puerto, resolución relativa, cambio a `https`, rechazo de host prohibido y longitud máxima, y comprobar que `System.Uri` conserva el resultado según CA15; si no, resolver sin `System.Uri`. | CA01–CA15, CA17; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Flujos internos](plan.md#flujos-internos) | T02, T03 | Compilar `Linkubator.Domain`; resultado esperado: contrato que devuelve la URL ajustada o `""`, o lanza la excepción correspondiente, sin recortar, sin transformar más y sin añadir dependencias. | completada | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), compilación sin errores, `System.Uri` usado para resolución y validación |
| T05 | Añadir los tests unitarios de la comprobación de host prohibido de `UrlHostPolicy`. | CA11; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04 | T03 | Ejecutar el filtro de `UrlHostPolicyTests`; resultado esperado: pasan `localhost`, IPv4, IPv6 entre corchetes y dominios que solo contienen `localhost` en otra posición. | completada | [UrlPoliciesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) UrlHostPolicyTests, 31 pruebas correctas |
| T06 | Añadir los tests unitarios de `ImageUrlPolicy` de precondiciones, imagen no informada, controles, `//`, esquemas, host y puerto, y referencias relativas. | CA01–CA10; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V03 | T04 | Ejecutar el filtro de `ImageUrlPolicyTests`; resultado esperado: pasan los casos y los rechazos, sin incluir el texto introducido. | completada | [UrlPoliciesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) ImageUrlPolicyTests, 18 pruebas correctas |
| T07 | Añadir los tests unitarios de `ImageUrlPolicy` de host prohibido por cada vía, longitud, conservación y ejemplos de la fuente. | CA11–CA13, CA15, CA16; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04–V06 | T05, T06 | Ejecutar el filtro de `ImageUrlPolicyTests`; resultado esperado: pasan los ejemplos de la tabla, los límites de longitud y la conservación. | completada | [UrlPoliciesTests.cs](../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) ImageUrlPolicyTests, 18 pruebas correctas |
| T08 | Revisar el contrato y la reutilización de S2.1, verificar el aislamiento arquitectónico y ejecutar las validaciones integradas. | CA14, CA17, CA18; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V07–V11 | T05, T07 | Revisión de V07, matriz de referencias con su comprobación negativa temporal, filtros de S2.2, suite y build; resultado esperado: la matriz detecta la infracción temporal y la solución limpia compila sin warnings y supera los tests. | completada | `dotnet test Linkubator.sln` sin errores, 257 pruebas correctas, build sin warnings |
| T09 | Sincronizar [roadmap.md](../../context/roadmap.md) con el inicio, el bloqueo o la finalización de la implementación, sin declarar aceptación ni cierre. | CA01–CA18; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T01 | Comparar el roadmap con el estado de esta cabecera; resultado esperado: coincidencia, sin declarar la etapa cerrada. | completada | Roadmap actualizado con implementación completada y aceptación |
| T10 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA18; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T08, T09 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, que no se tocaron `UrlOriginal`, alias, slugs ni la entidad `Link`, y registrar aceptación o reparos de DLG. | completada | Revisión completada, aceptación registrada por DLG («Funciona») |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T04, T06 | V01, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA02 | T04, T06 | V01, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA03 | T04, T06 | V01, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA04 | T04, T06 | V02, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA05 | T04, T06 | V02, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA06 | T04, T06 | V02, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA07 | T04, T06 | V02, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA08 | T04, T06 | V03, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA09 | T04, T06 | V03, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA10 | T04, T06 | V03, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA11 | T02, T03, T04, T05, T07 | V04, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageHostNotAllowedException.cs](../../src/Linkubator.Domain/Exceptions/ImageHostNotAllowedException.cs), [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA12 | T04, T07 | V05, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA13 | T02, T04, T07 | V05, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageHostNotAllowedException.cs](../../src/Linkubator.Domain/Exceptions/ImageHostNotAllowedException.cs), [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA14 | T04, T08 | V07 | Probado | 10-X-2026; .NET 10.0.12 | `dotnet test Linkubator.sln` sin errores |
| CA15 | T04, T07 | V05, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), `System.Uri` usado para conservación |
| CA16 | T07 | V06, V09 | Implementado y probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), UrlPoliciesTests::ImageUrlPolicyTests |
| CA17 | T03, T04, T08 | V07 | Probado | 10-X-2026; .NET 10.0.12 | [ImageUrlPolicy.cs](../../src/Linkubator.Domain/Policies/ImageUrlPolicy.cs), `dotnet test` validaciones integradas |
| CA18 | T08 | V08, V10, V11 | Probado | 10-X-2026; .NET 10.0.12 | `dotnet test Linkubator.sln` 257 pruebas correctas, sin modificaciones ajenas al alcance |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no se registran resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno.
- Aceptación humana: DLG, 10-X-2026 («Funciona»).
- Archivado: solicitado.
- Ubicación archivada: `specs/archive/s2.2-ajuste-de-image/`.
- Fecha efectiva de archivado: 10-X-2026.
- Comprobación de enlaces: completada; todos los enlaces internos (spec, plan) resueltos correctamente dentro de `archive/s2.2-ajuste-de-image/`.
- Siguiente paso autorizado: actualizar índices y roadmap; iniciar S2.3 o continuar con etapa siguiente según roadmap.
