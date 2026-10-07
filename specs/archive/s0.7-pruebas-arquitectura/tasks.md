# Tareas: S0.7 — Pruebas de arquitectura y arranque Web

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 7-X-2026.
- [Plan técnico](plan.md): aprobado por DLG el 7-X-2026.
- Referencia: instrucción de DLG «Pruebas completadas» para cada documento S0.7.
- Las aprobaciones documentales no acreditan implementación ni aceptación del resultado. Las verificaciones siguientes documentan el estado actual.

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar especificación, plan y tareas. | [CA01 a CA05](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación de DLG registrada para los documentos. | completada | DLG, 7-X-2026: «Pruebas completadas». |
| T02 | Comprobar configuración xUnit y runner. | [CA01](spec.md#criterios-de-aceptación); [V06](plan.md#estrategia-de-validación) | T01 | Revisar csproj, paquetes, TFM y configuración global; verificar que el runner descubre tests. | completada | 7-X-2026: `Linkubator.Tests.csproj` declara xUnit 2.9.3, VSTest adapter 3.1.4 y `Microsoft.NET.Test.Sdk` 17.14.1; VSTest descubrió las pruebas al ejecutar filtros/suite. SDK .NET 10.0.401. |
| T03 | Validar la matriz de referencias. | [CA02](spec.md#criterios-de-aceptación); [V02](plan.md#estrategia-de-validación) | T01 | Ejecutar filtro del test de matriz; una prueba descubierta y correcta. | completada | 7-X-2026: `ProjectReferenceMatrixTests.ProjectReferencesMatchTheAllowedMatrix`: 1 pasada, 0 fallidas. |
| T04 | Validar límite de dependencia Web/Infrastructure. | [CA03](spec.md#criterios-de-aceptación); [V03](plan.md#estrategia-de-validación) | T01 | Ejecutar filtro de LayerDependencyTests y revisar el predicado/assert. | completada | 7-X-2026: `LayerDependencyTests.WebTypesOutsideTheCompositionRootDoNotDependOnInfrastructure`: 1 pasada, 0 fallidas; inspección confirma que todas las dependencias recogidas deben llamarse `Program`. No se inyectó una infracción artificial. |
| T05 | Validar arranque Web mínimo. | [CA04](spec.md#criterios-de-aceptación); [V04](plan.md#estrategia-de-validación) | T01 | Ejecutar filtro WebStartupTests; una prueba descubre y pasa. | completada | 7-X-2026: `WebStartupTests.HomePageReturnsSuccess`: 1 pasada, 0 fallidas. TestServer avisó que no tenía puerto HTTPS; no acredita HTTPS live. |
| T06 | Ejecutar build y suite integrada. | [CA05](spec.md#criterios-de-aceptación); [V01, V05](plan.md#estrategia-de-validación) | T02–T05 | Build sin advertencias/errors y toda la suite pasa. | completada | 7-X-2026, Windows/.NET SDK 10.0.401: `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 advertencias y 0 errores (incluido Tailwind); `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'`: 3 pasadas, 0 fallidas, 0 omitidas. |
| T07 | Revisar evidencia, aceptación y archivo. | [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T06 | Registrar aceptación expresa de DLG y mover tras verificar referencias. | completada | DLG, 7-X-2026: «Pruebas completadas». Carpeta trasladada a `specs/archive/s0.7-pruebas-arquitectura/`; enlaces locales, índice, presencia de los tres documentos y ausencia de la ubicación activa verificados. |

Las pruebas automáticas fueron establecidas por S0.7; su uso actual no prueba la fecha en que fueron escritas. La compilación y ejecución de `WebApplicationFactory` no verifican HTTPS real ni acceso a una base.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T06 | [V01, V06](plan.md#estrategia-de-validación) | Conforme | 7-X-2026; .NET SDK 10.0.401. | csproj revisado; runner descubrió y ejecutó los tests. |
| CA02 | T03, T06 | [V02](plan.md#estrategia-de-validación) | Conforme | 7-X-2026; VSTest/xUnit. | `ProjectReferencesMatchTheAllowedMatrix`: 1 pasada. |
| CA03 | T04, T06 | [V03](plan.md#estrategia-de-validación) | Conforme por ejecución y revisión de la aserción | 7-X-2026; NetArchTest 1.3.2. | `WebTypesOutsideTheCompositionRootDoNotDependOnInfrastructure`: 1 pasada; no se inyectó código infractor. |
| CA04 | T05, T06 | [V04](plan.md#estrategia-de-validación) | Conforme para el host de TestServer | 7-X-2026; ASP.NET Core Testing 10.0.0. | `HomePageReturnsSuccess`: 1 pasada; el aviso de puerto HTTPS excluye cualquier conclusión sobre HTTPS live. |
| CA05 | T06 | [V01, V05](plan.md#estrategia-de-validación) | Conforme | 7-X-2026; Windows/.NET SDK 10.0.401. | Build 0 warnings/0 errors; suite 3/3, 0 omitidas. |

## Aceptación y cierre

- Estado del trabajo: archivado; documentos aprobados; CA01 a CA05 verificados; T01–T07 completadas; resultado aceptado por DLG.
- Criterios sin evidencia satisfactoria: ninguno; CA03 fue revisado en su lógica, sin ejecutar mutación negativa artificial.
- Bloqueos y riesgos: `WebApplicationFactory` puede avisar de que no tiene puerto HTTPS; eso no invalida el arranque pero no acredita HTTPS live. No modificar código productivo para probar fallos artificiales.
- Aprobación documental: DLG, 7-X-2026, «Pruebas completadas» para `spec.md` y `plan.md`.
- Aceptación del resultado: DLG, 7-X-2026, «Pruebas completadas».
- Archivado: completado el 7-X-2026 en `specs/archive/s0.7-pruebas-arquitectura/`.
- Comprobación de enlaces: 7-X-2026; rutas locales entrantes y salientes verificadas en seis documentos; los tres archivos están en el destino y la carpeta activa no existe.
- Siguiente paso: ninguno para S0.7.
