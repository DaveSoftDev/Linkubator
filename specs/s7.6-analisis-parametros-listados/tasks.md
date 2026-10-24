# Tareas: S7.6 — Análisis de parámetros de listados

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 23-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 23-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 23-X-2026 («Vamos»).
- Implementación:
  - Estado: implementada como base técnica; no integrada en rutas de listados ni validada mediante pruebas HTTP transversales.
  - Inicio: DLG, 23-X-2026 («Inicia la implementación»).
  - Resultado técnico: analizador reutilizable y pruebas unitarias focalizadas; el dashboard actual no consume el analizador tras cambios paralelos posteriores.
  - Evidencia consolidada: [T01–T06](#descomposición) y [evidencia por criterio](#evidencia-por-criterio).
- Aceptación humana: DLG, 23-X-2026 («parámetros validados»); se acepta como base técnica, no como integración funcional terminada ni como cierre de S7.6.

## Descomposición

La implementación se acepta como base técnica reutilizable: analizador puro de consulta y pruebas unitarias focalizadas. La comprobación de accesibilidad de `c` queda delegada a un predicado proporcionado por el llamador; no se integra con Application porque la interfaz de colección disponible no ofrece lectura de un solo elemento sin invocar el caso de uso de edición, y no se introduce consulta ad hoc en Web. La integración en rutas, identidad autenticada para comprobar `c` y respuestas HTTP siguen pendientes de sus puntos de integración; el dashboard no es una ruta de listado y no conserva esta integración experimental.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar PageModels, contratos Application y harness de pruebas; fijar el límite seguro de validación de `c`. | CA01–CA07; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias), V01–V05 | Ninguna | Revisar interfaces disponibles y confirmar que no se introduce consulta ad hoc ni se usa el caso de uso de edición como acceso de lectura para un filtro. | completada | El 23-X-2026 se confirmó que `ICollectionService.GetCollection` realiza el caso de uso de obtención de una colección con identidad de servidor, pero no se integra desde este analizador puro; falta un contrato de validación de filtro específico. Se conserva la delegación al llamador y se deja integración pendiente. |
| T02 | Definir tipos Web propios para el tipo de listado y el resultado tipado del análisis. | CA01–CA07; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos) | T01 | Build de Web y revisión de tipo/carpeta por responsabilidad, sin lógica de consultas ni respuestas HTTP. | completada | `PrivateListingKind` y `PrivateListingQueryAnalysisResult` en [Models/PrivateListing](../../src/Linkubator.Web/Models/PrivateListing/); compilación validada por la suite en T06. |
| T03 | Implementar análisis de parámetros aplicables `p`, `c` y `q`, primer valor, números ASCII positivos, vacíos, valores desconocidos/no aplicables, límites y errores. | CA01–CA06; [plan.md → «Flujos internos»](plan.md#flujos-internos) | T02 | Pruebas focalizadas cubren valores por defecto, ceros iniciales, repetidos, inválidos, no aplicables, valores Unicode y longitud de búsqueda en puntos de código. | completada | Implementado en [PrivateListingQueryAnalyzer.cs](../../src/Linkubator.Web/Models/PrivateListing/PrivateListingQueryAnalyzer.cs); predicado de accesibilidad opcional para `c`, cuyo valor se omite cuando no se proporciona comprobación o no es accesible. Véase V01–V03. |
| T04 | Establecer la frontera de comprobación de propiedad de `c` sin introducir acceso ad hoc a persistencia o aceptar identidad del cliente. | CA04; [plan.md → «Persistencia y dependencias»](plan.md#persistencia-y-dependencias), V02 | T01, T02 | Confirmar comportamiento: solo un filtro validado por llamador puede producir `CollectionId`; no accesible/no verificado se omite sin revelar la causa y preserva otros parámetros. | completada con alcance limitado | El analizador recibe `Func<long,bool>` del llamador; sin ese predicado omite `c`. No se integró con Application ni se probó consulta de colección real; permanece como trabajo de integración posterior, sin bloquear la aceptación de base técnica. |
| T05 | Añadir pruebas focalizadas de análisis, preservación de parámetros válidos cuando hay errores y parámetros no aplicables. | CA01–CA07; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V04 | T03, T04 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter PrivateListingQueryAnalyzerTests`; las pruebas deben cubrir solo comportamiento del analizador y no afirmar navegación o búsqueda de resultados. | completada | 15 pruebas focalizadas ejecutadas correctamente (`15/15`) en [PrivateListingQueryAnalyzerTests.cs](../../tests/Linkubator.Tests/Web/PrivateListingQueryAnalyzerTests.cs), con resultado de la ejecución 23-X-2026; véase V01–V04. |
| T06 | Ejecutar validación integrada disponible y sincronizar la documentación con la condición de base técnica, límites y alcance. | CA01–CA07; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V05 | T05 | Ejecutar suite focalizada, suite del proyecto y `git diff --check`; revisar que no se presente `e`, integración HTTP, UI consumidora o logging como entregados. | completada con limitación | Suite focalizada final: 15/15 y `git diff --check` limpio el 23-X-2026; aviso no bloqueante de conversión LF/CRLF en `styles.css`. Suite completa posterior: 1.130 tests, 1.129 pasaron y 1 falló en `WebStartupTests.AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound` (esperado NotFound, recibido BadRequest); fallo de integración/dashboard ajeno al analizador, no clasificado como regresión de S7.6. Suite completa previa de base: 1.119/1.119. No se hizo sensibilidad negativa. |
| T07 | Registrar aceptación humana de la base técnica y los límites expresos de cierre. | CA01–CA07; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Registrar responsable, fecha y referencia real sin equiparar aceptación de base técnica a integración funcional. | completada como base técnica | DLG, 23-X-2026 («parámetros validados»). Aceptación limitada al analizador y su validación focalizada; quedan pendientes integración de `c` con identidad del servidor, consumidores de rutas de listados y validación HTTP transversal. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03, T05–T06 | V01, V05 de [plan.md](plan.md) | Parcialmente demostrado por pruebas unitarias: claves desconocidas y `e` se ignoran; no existe integración en rutas consumidoras | 23-X-2026; .NET SDK del entorno local | `PrivateListingQueryAnalyzerTests`; cobertura HTTP transversal pendiente de S7.9 |
| CA02 | T01, T03, T05–T06 | V01, V05 de [plan.md](plan.md) | Demostrado: primera ocurrencia de `p`, `c` y `q` se usa | 23-X-2026; .NET SDK del entorno local | `Analyze_WhenParametersAreRepeated_UsesFirstValueAndIgnoresUnknownKeys` |
| CA03 | T01–T03, T05–T06 | V02, V05 de [plan.md](plan.md) | Demostrado a nivel de análisis: `p` ausente/vacío, ceros iniciales, valor positivo ASCII y valores inválidos/fuera de rango | 23-X-2026; .NET SDK del entorno local | `PrivateListingQueryAnalyzerTests`; la navegación/canonical HTTP queda fuera de S7.6 |
| CA04 | T01–T05 | V02, V05 de [plan.md](plan.md) | Demostrado solo como contrato del analizador: vacío, no accesible o sin verificación se omite y se conservan otros valores; no se prueba propiedad en Application | 23-X-2026; .NET SDK del entorno local | `Analyze_WhenCollectionIsEmptyOrNotAccessible_IgnoresItAndPreservesOtherValues`, `Analyze_WhenCollectionIsValidButNoAccessCheckIsProvided_IgnoresCollection` |
| CA05 | T01–T05 | V02, V05 de [plan.md](plan.md) | Demostrado a nivel de análisis para `p` y `c` malformados/fuera de rango; no se ejercita consulta alguna | 23-X-2026; .NET SDK del entorno local | `PrivateListingQueryAnalyzerTests`; respuesta accesible final a integrar con las páginas |
| CA06 | T01–T03, T05–T06 | V03, V05 de [plan.md](plan.md) | Demostrado: vacío/espacios se omiten, el texto se recorta y el máximo cuenta puntos de código Unicode | 23-X-2026; .NET SDK del entorno local | `Analyze_WhenSearchExceedsUnicodeCodePointLimit_ReportsErrorWithoutSearchValue` y `Analyze_WhenSearchHasText_TrimsExtremesAndPreservesInternalSpaces` |
| CA07 | T01–T06 | V04–V05 de [plan.md](plan.md) | Demostrado en combinación dentro del resultado del analizador; no se valida renderizado de UI | 23-X-2026; .NET SDK del entorno local | `Analyze_WhenPageIsInvalid_ReportsErrorAndKeepsValidValues`, `Analyze_WhenCollectionIsMalformed_ReportsErrorButPreservesPageAndSearch` |

La aceptación «parámetros validados» corresponde a la base técnica indicada, no declara integración de producto ni cobertura HTTP completa. Las limitaciones por criterio se conservan explícitas.

## Aceptación y cierre

- Estado del trabajo: base técnica implementada y aceptada; integración funcional S7.6 pendiente.
- Criterios con evidencia parcial o límites: CA01 no tiene consumidores de rutas; CA04 no tiene verificación Application de propiedad real; CA05/CA07 no tienen presentación final accesible integrada.
- Bloqueo de integración: falta un contrato de Application específico para comprobar que `c` es accesible sin invocar el caso de uso de obtención/edición ni crear consulta ad hoc en Web. El analizador omite `c` cuando no recibe comprobación.
- Aceptación humana: DLG, 23-X-2026 («parámetros validados»); alcance aceptado: base técnica, sin integración de rutas ni cierre funcional completo.
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso: en una tarea posterior, proporcionar el contrato de validación de `c` y conectar el analizador a los listados consumidores cuando estén disponibles; después completar la validación HTTP correspondiente.
