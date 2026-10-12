# Tareas: Tests adicionales de `EmailPolicy`, `ImageUrlPolicy` y `LinkUrlPolicy`

## Referencias y autorización

- Estado: aprobadas por DLG el 12-X-2026.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Adelante»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Adelante»).
- Implementación:
    - Estado: completada.
    - Inicio: 12-X-2026; DLG («Adelante»), aprobación explícita de tareas.
    - Finalización: 12-X-2026; filtros focalizados 95/95 y suite completa 501/501, sin errores.
    - Evidencia consolidada: T02–T06 y V01–V04; cobertura global 89,81% de líneas y 84,62% de ramas.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02, T03 y T04 son independientes después de T01 porque amplían archivos de tests distintos. T05 requiere las tres suites actualizadas; T06 revisa la trazabilidad y sincroniza el roadmap; T07 presenta la evidencia para aceptación humana.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA08; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas sin alterar los estados de spec ni plan. | completada | Aprobada por DLG el 12-X-2026 («Adelante»). |
| T02 | Ampliar `EmailPolicyTests.cs` con las alternativas de sintaxis aún no cubiertas, las fronteras de longitud y aserciones de excepción diferenciadas. | CA01–CA03; [plan → Flujos internos](plan.md#flujos-internos), V01 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~EmailPolicyTests" --verbosity minimal`; resultado esperado: pasan los casos admitidos, ambos lados del límite y cada error tipado. | completada | 12-X-2026, .NET 10.0.12; V01: 16/16 correctas. |
| T03 | Ampliar `ImageUrlPolicyTests.cs` con el máximo exacto, el exceso y el recorte de espacios exteriores. | CA04–CA05; [plan → Flujos internos](plan.md#flujos-internos), V02 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~ImageUrlPolicyTests" --verbosity minimal`; resultado esperado: frontera y recorte coinciden con la especificación. | completada | 12-X-2026, .NET 10.0.12; V02: 32/32 correctas. |
| T04 | Ampliar los casos de `LinkUrlPolicy` en `UrlPoliciesTests.cs` con el máximo exacto, el exceso y el recorte de espacios exteriores. | CA06–CA07; [plan → Flujos internos](plan.md#flujos-internos), V03 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlPoliciesTests" --verbosity minimal`; resultado esperado: frontera y recorte coinciden con la especificación, y pasan los casos existentes de host y esquema. | completada | 12-X-2026, .NET 10.0.12; V03: 47/47 correctas. |
| T05 | Ejecutar la suite completa y revisar que no se modificó código de producción. | CA08; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04 | T02, T03, T04 | `dotnet test Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: todos los tests pasan y `src/Linkubator.Domain` no tiene cambios de este trabajo. | completada | 12-X-2026, .NET 10.0.12; V04: 501/501 correctas, 0 errores, 0 omitidas. Con Coverlet: 89,81% de líneas y 84,62% de ramas. |
| T06 | Revisar trazabilidad CA01–CA08 → tests → validaciones, registrar evidencia y sincronizar el roadmap con el resultado de implementación sin declarar aceptación. | CA01–CA08; V01–V04 | T05 | Revisar los archivos y el diff; resultado esperado: todos los criterios enlazan a evidencia real, los cambios solo afectan tests y el roadmap refleja el estado comprobado. | completada | 12-X-2026; cambios de código limitados a las tres suites de tests; roadmap sincronizado con implementación y validación completadas. |
| T07 | Presentar evidencia a DLG y registrar la aceptación humana o las correcciones solicitadas. | CA01–CA08 | T06 | Decisión explícita de DLG registrada; resultado esperado: aceptación o bloqueo con las correcciones necesarias. | completada | DLG acepta el resultado el 13-X-2026 con la referencia «Aceptado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02 | [V01](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | EmailPolicyTests, 16/16 correctas; `quoted-string` y `domain-literal` aceptados. |
| CA02 | T02 | [V01](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | EmailPolicyTests, 16/16 correctas; email de 250 caracteres aceptado y de 251 rechazado por longitud. |
| CA03 | T02 | [V01](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | EmailPolicyTests, 16/16 correctas; entrada vacía, longitud y sintaxis verificadas con excepciones específicas. |
| CA04 | T03 | [V02](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | ImageUrlPolicyTests, 32/32 correctas; límite ajustado de 2048 aceptado y exceso rechazado. |
| CA05 | T03 | [V02](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | ImageUrlPolicyTests, 32/32 correctas; URL absoluta con espacios exteriores produce el resultado esperado. |
| CA06 | T04 | [V03](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlPoliciesTests, 47/47 correctas; límite ajustado de 2048 aceptado y exceso rechazado. |
| CA07 | T04 | [V03](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlPoliciesTests, 47/47 correctas; URL absoluta con espacios exteriores produce el resultado esperado. |
| CA08 | T05, T06 | [V04](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | Suite completa: 501/501 correctas; este trabajo solo modificó las tres suites de tests. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado por DLG.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA08 tienen evidencia satisfactoria.
- Bloqueos y riesgos restantes: ninguno comprobado.
- Aceptación humana: DLG, 13-X-2026 («Aceptado»).
- Archivado: sí, trasladado el 13-X-2026.
- Ubicación archivada: [specs/archive/tests-adicionales-policies-email-url](.).
- Fecha efectiva de archivado y comprobación de enlaces: 13-X-2026; artefactos presentes y referencias recalculadas.
- Siguiente paso autorizado: ninguno para este trabajo; queda aceptado, cerrado y archivado.
