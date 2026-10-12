# Plan técnico: Tests adicionales de policies URL

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Aprobada»).
	- Plan: aprobado por DLG el 12-X-2026 («Aprobada»), conforme a su autorización para aprobar los siguientes pasos sin revisión intermedia.

## Diseño de implementación

### Componentes y contratos

| Suite existente | Policies verificadas | Responsabilidad | Criterios |
| --- | --- | --- | --- |
| `UrlPoliciesTests.cs` | `UriSchemePolicy`, `UrlHostPolicy`, `LinkUrlPolicy` | Clasificación directa de entradas, extracción básica y delimitadores de host/puerto en ajuste de URL. | CA01, CA03 |
| `UrlHostPolicyTests.cs` | `UrlHostPolicy` | Resultados positivos y negativos de extracción, formatos de autoridad y salida nula cuando no se extrae un host. | CA02 |
| `ImageUrlPolicyTests.cs` | `ImageUrlPolicy` | Resolución de una referencia que solo contiene fragmento contra una base `http`. | CA04 |
| `UrlValidationPolicyTests.cs` | `UrlValidationPolicy` | Aceptación y rechazo de entradas de autoridad con las excepciones de dominio específicas. | CA05 |
| `UrlNormalizationPolicyTests.cs` | `UrlNormalizationPolicy` | Consulta con parámetros repetidos, clave de marketing en mayúsculas y diferencia entre `+` literal y espacio codificado. | CA06 |

Se amplían estas suites existentes, sin crear clases de prueba, utilidades ni paquetes. No se modifica `src/Linkubator.Domain`.

### Flujos internos

1. **Detección y ajuste.** Comprobar resultados de `UriSchemePolicy.Detect` para host y puerto con los delimitadores definidos, un esquema sin autoridad y una entrada que no clasifica. En la misma suite, ajustar host y puerto cuando la entrada termina en consulta o fragmento y verificar que el sufijo se conserva.
2. **Extracción y validación de host.** Ejercitar éxito y fracaso de `TryExtractHost` y `TryExtractValidHost`, incluyendo IPv6 entre corchetes con puerto malformado, y verificar el estado del parámetro de salida en cada resultado.
3. **Resolución de imagen.** Resolver una referencia formada solo por fragmento contra una URL base pública con esquema `http`; comprobar ruta, consulta y fragmento resultantes junto con el cambio de esquema.
4. **Validación.** Añadir solo combinaciones todavía ausentes para formatos válidos e inválidos de esquema, host, credenciales y puerto, exigiendo el tipo de excepción especificado.
5. **Normalización.** Añadir consultas con claves repetidas y con una clave de marketing en mayúsculas. Comparar URLs con distinto fragmento y demostrar la distinción del `+` en consulta frente al espacio codificado mediante el resultado normalizado.

### Persistencia y dependencias

- No hay persistencia, red, resolución DNS, integraciones ni cambios de dependencias.
- Se reutilizan xUnit, Coverlet y las suites actuales.
- Los casos nuevos se derivan de las reglas propietarias; no duplican matrices completas de ejemplos.

### Garantías técnicas

- Los tests comprueban comportamiento observable de las policies, no métodos privados ni detalles de instrumentación.
- La cobertura se compara por línea y rama, pero no se fuerza un 100% si las ramas restantes no representan entradas válidas del contrato o requieren inventar reglas.
- Si un test alineado con la fuente revela una discrepancia en producción, se registra como bloqueo; no se cambia la policy bajo este alcance.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Ampliar únicamente las suites existentes y conservar sus categorías y filtros. | Confirmado | Aprobación de DLG, 12-X-2026 («Aprobada»); alcance de la spec | Sin clases ni infraestructura adicional. |
| D02. Priorizar ramas observables derivadas de la especificación y no perseguir cobertura total mediante entradas ajenas al contrato. | Confirmado | Aprobación de DLG, 12-X-2026 («Aprobada»); CA01–CA07 | Una discrepancia con la fuente bloquea el cambio de producción y se eleva a DLG. |
| D03. Línea base Cobertura anterior a este trabajo: Image 83,95%/93,75%; Link 95,65%/96,22%; UriScheme 96,55%/68%; UrlHost 89,92%/84,48%; UrlNormalization 95,06%/91,66%; UrlValidation 82,47%/76% (líneas/ramas). | Confirmado | Reporte `coverage.cobertura.xml` existente, ejecución completa de 12-X-2026 | Comparar con el reporte final y conservar la métrica por policy, además de la global. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA03 | Automatizada: detección, extracción básica y ajuste de enlace | SDK y dependencias existentes | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlPoliciesTests" --verbosity minimal` | Pasan los tests nuevos y los casos existentes de host, esquema y ajuste. | Comando, conteo y código de salida. |
| V02 | CA02 | Automatizada: host | V01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlHostPolicyTests" --verbosity minimal` | Los formatos válidos e inválidos producen el resultado y salida esperados. | Comando, conteo y código de salida. |
| V03 | CA04 | Automatizada: imagen | V01–V02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~ImageUrlPolicyTests" --verbosity minimal` | La referencia fragment-only conserva ruta/consulta, reemplaza fragmento y cambia `http` a `https`. | Comando, conteo y código de salida. |
| V04 | CA05 | Automatizada: validación | V01–V03 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlValidationPolicyTests" --verbosity minimal` | Los casos válidos se aceptan y cada rechazo usa la excepción esperada. | Comando, conteo y código de salida. |
| V05 | CA06 | Automatizada: normalización | V01–V04 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlNormalizationPolicyTests" --verbosity minimal` | Se conservan y ordenan duplicados; marketing y fragmento no alteran la clave; `+` sigue la regla de consulta. | Comando, conteo y código de salida. |
| V06 | CA01–CA07 | Automatizada integrada y cobertura | V01–V05 | `dotnet test Linkubator.sln --no-restore --collect:"XPlat Code Coverage" --verbosity minimal` | Suite completa verde; el reporte Cobertura muestra las métricas por policy y no se modifica código de producción. | Comando, conteo, código de salida, porcentajes de línea/rama y revisión del diff. |

V01–V05 prueban las rutas objetivo sin demostrar exhaustividad de todas las combinaciones posibles. V06 registra cambios de cobertura, pero el porcentaje no sustituye la revisión de que cada test deriva de una fuente aprobada.

## Orden de ejecución y cierre

- Tareas: [tasks.md](tasks.md), aprobado por DLG el 12-X-2026 («Aprobada»); desglosa las cinco suites en incrementos independientes, validación integrada, registro de cobertura y sincronización del roadmap.
- Todos los criterios CA01–CA07 deben tener evidencia satisfactoria antes de presentar el resultado a DLG.
- Ante una contradicción con las reglas fuente, detener la tarea afectada y esperar una decisión antes de cambiar producción o especificaciones.