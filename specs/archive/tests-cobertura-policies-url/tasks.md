# Tareas: Tests adicionales de policies URL

## Referencias y autorización

- Estado: aprobadas por DLG el 12-X-2026.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Aprobada»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Aprobada»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Aprobada»), conforme a su autorización para aprobar los siguientes pasos sin revisión intermedia.
- Implementación:
    - Estado: completada.
    - Inicio: 12-X-2026; DLG («Aprobada»), aprobación explícita de tareas.
    - Finalización: 12-X-2026; filtros individuales y suite completa 533/533, sin errores.
    - Evidencia consolidada: T02–T08 y V01–V06; cobertura global 91,53% de líneas y 88,43% de ramas.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02–T06 son independientes tras T01 porque cada tarea modifica una suite de tests distinta. T07 requiere las cinco suites; T08 revisa y registra la evidencia y sincroniza el roadmap; T09 registra la aceptación humana tras comprobar todos los criterios. El usuario autorizó expresamente que estas aprobaciones y pasos se tramiten en secuencia sin revisión intermedia; la autorización no sustituye los checks, que deben pasar antes de registrar la aceptación final.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar estas tareas y registrar el inicio autorizado. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Registrar «Aprobada» de DLG, completar T01 y marcar la implementación como no iniciada antes de comenzarla. | completada | Aprobada por DLG el 12-X-2026 («Aprobada»); implementación iniciada el mismo día. |
| T02 | Ampliar `UrlPoliciesTests.cs` con casos directos de clasificación de `UriSchemePolicy` y host:puerto de `LinkUrlPolicy` seguido por query o fragmento. | CA01, CA03; [plan → Flujos internos](plan.md#flujos-internos), V01 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlPoliciesTests" --verbosity minimal`; resultado esperado: pasan los resultados de clasificación y ajuste esperados, junto con los casos actuales. | completada | 12-X-2026; V01: 57/57 correctas. |
| T03 | Ampliar `UrlHostPolicyTests.cs` con éxitos y fallos de extracción, autoridades IPv6 con puerto malformado y salida `null` al fallar. | CA02; [plan → Flujos internos](plan.md#flujos-internos), V02 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlHostPolicyTests" --verbosity minimal`; resultado esperado: host y estado de salida correctos para cada caso. | completada | 12-X-2026; V02: 44/44 correctas. |
| T04 | Ampliar `ImageUrlPolicyTests.cs` con una referencia compuesta solo por fragmento sobre una base HTTP válida. | CA04; [plan → Flujos internos](plan.md#flujos-internos), V03 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~ImageUrlPolicyTests" --verbosity minimal`; resultado esperado: ruta y consulta de la base se conservan, cambia el fragmento y el esquema final es HTTPS. | completada | 12-X-2026; V03: 35/35 correctas. |
| T05 | Completar `UrlValidationPolicyTests.cs` con los casos de autoridad aún no diferenciados por la excepción esperada. | CA05; [plan → Flujos internos](plan.md#flujos-internos), V04 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlValidationPolicyTests" --verbosity minimal`; resultado esperado: aceptaciones y excepciones específicas pasan sin exponer la entrada. | completada | 12-X-2026; V04: 89/89 correctas. `System.Uri` rechaza el puerto IPv6 sobrelímite antes de la comprobación propia. |
| T06 | Ampliar `UrlNormalizationPolicyTests.cs` para parámetros repetidos, clave de marketing con mayúsculas y la diferencia entre `+` en consulta y espacio codificado. | CA06; [plan → Flujos internos](plan.md#flujos-internos), V05 | T01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlNormalizationPolicyTests" --verbosity minimal`; resultado esperado: los duplicados se conservan/ordenan, marketing se elimina y el fragmento no cambia la clave. | completada | 12-X-2026; V05: 21/21 correctas. |
| T07 | Ejecutar la suite completa con Coverlet y registrar cobertura por policy. | CA07; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06 | T02, T03, T04, T05, T06 | `dotnet test Linkubator.sln --no-restore --collect:"XPlat Code Coverage" --verbosity minimal`; resultado esperado: suite verde y reporte Cobertura actualizado para las seis policies. | completada | 12-X-2026, .NET 10.0.12; 533/533 correctas, 0 errores ni omitidas. Cobertura global 91,53% líneas y 88,43% ramas. |
| T08 | Revisar el diff, completar la trazabilidad CA01–CA07 y sincronizar el roadmap con el resultado, sin reabrir S2. | CA01–CA07; [plan → Garantías técnicas](plan.md#garantías-técnicas), V06 | T07 | Confirmar que solo se modificaron tests, registrar métricas de línea/rama por policy y actualizar el estado de implementación en roadmap. | completada | 12-X-2026; suites de tests y documentos de trazabilidad actualizados; no se modificó `src/Linkubator.Domain`; roadmap sincronizado. |
| T09 | Registrar la aceptación final de DLG si y solo si pasan todos los criterios y no hay discrepancias bloqueantes con las fuentes. | CA01–CA07 | T08 | Aplicar la autorización explícita de DLG («Aprobada») después de verificar la evidencia; si algún criterio falla o contradice fuente, dejar el trabajo bloqueado y no aceptarlo. | completada | Cierre final confirmado por DLG el 12-X-2026 («Completada»), tras V01–V06 satisfactorias y la corrección posterior validada. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02 | [V01](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlPoliciesTests, 57/57; clasificación de esquemas, candidatos inválidos y entradas no clasificables. |
| CA02 | T03 | [V02](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlHostPolicyTests, 44/44; éxito y fallo de extracción, incluyendo salida nula. |
| CA03 | T02 | [V01](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlPoliciesTests, 57/57; host:puerto con ruta, query y fragmento. |
| CA04 | T04 | [V03](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | ImageUrlPolicyTests, 35/35; referencia fragment-only contra base HTTP. |
| CA05 | T05 | [V04](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlValidationPolicyTests, 89/89; excepciones específicas y rechazo por orden de `System.Uri`. |
| CA06 | T06 | [V05](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | UrlNormalizationPolicyTests, 21/21; repetidos, marketing case-insensitive, `+` y fragmentos. |
| CA07 | T07, T08 | [V06](plan.md#estrategia-de-validación) | Pasado | 12-X-2026; .NET 10.0.12 | Suite completa 533/533; cambios solo en suites de tests y documentación. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA07 tienen evidencia satisfactoria.
- Bloqueos y riesgos restantes: ninguno comprobado. La cobertura actual de `UrlValidationPolicy` después de la corrección posterior está registrada debajo.
- Aceptación humana: DLG, 12-X-2026 («Completada»), conforme a la autorización explícita de completar la secuencia sin revisión intermedia, aplicada tras validar todos los criterios y la corrección posterior.
- Archivado: sí, trasladado el 12-X-2026.
- Ubicación archivada: [specs/archive/tests-cobertura-policies-url](../tests-cobertura-policies-url).
- Fecha efectiva de archivado y comprobación de enlaces: 12-X-2026; artefactos presentes y enlaces internos recalculados.
- Siguiente paso autorizado, sin cerrar otras etapas: trabajo aceptado; archivado no solicitado.

### Corrección posterior a la aceptación

- Autorización: DLG, 12-X-2026 («Autorizar refactor mínimo» y petición posterior de alcanzar el 93%).
- Alcance: se retiraron de `UrlValidationPolicy` dos defensas inalcanzables tras aceptar `Uri.TryCreate` una URI absoluta HTTP/HTTPS: la comprobación de host vacío y la comprobación de autoridad vacía dentro de `HasInvalidPort`. No se añadieron pruebas por reflexión.
- Tests añadidos: referencia `//` interpretada como esquema no permitido, puertos vacíos para DNS e IPv6, y autoridad IPv6 conforme al host que devuelve `System.Uri`.
- Validación focalizada: `UrlValidationPolicyTests`, 93/93 correctas.
- Validación integrada: `dotnet test Linkubator.sln --no-restore --collect:"XPlat Code Coverage" --verbosity minimal -p:BaseOutputPath=bin/UrlValidation93/`; 538/538 correctas, 0 errores, 0 omitidas.
- Cobertura final de `UrlValidationPolicy`: 94,50% de líneas y 91,54% de ramas. Cobertura global: 92,62% de líneas y 89,43% de ramas.
- Limitación de la validación: el build usó una ruta de salida dentro de cada proyecto para evitar una DLL bloqueada por una sesión de PowerShell y mantener resoluble la raíz del repositorio para los tests de arquitectura.