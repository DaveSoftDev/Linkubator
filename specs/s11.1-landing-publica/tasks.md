# Tareas: S11.1 — Landing pública

## Referencias y autorización

- Estado: completado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por el responsable de implementación el 27-X-2026; la ejecución y la evidencia quedan registradas en este documento.
- Implementación:
  - Estado: completado.
  - Inicio: 27-X-2026.
  - Finalización: 27-X-2026, tras validación del HTML servido y ejecución de pruebas focalizadas.
  - Evidencia consolidada: [T01](#t01), [T02](#t02), [T03](#t03), [T04](#t04) y [T05](#t05) con resultados y comandos reproducibles enlazados en esta misma página.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación; la evidencia registrada refleja el resultado verificado en el entorno del proyecto.

## Descomposición

S11 continúa en «Later» en [roadmap.md → «Later»](../../context/roadmap.md#later). La aprobación de estas tareas no altera el orden de ejecución: no iniciar la implementación hasta que S11 sea elegible según el roadmap.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Preparar metadatos específicos para la landing en el layout compartido y el `IndexModel`, incluida la canonical basada en el origen configurado. Conservar los valores actuales de las páginas que no proporcionan metadatos propios. | CA02, CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Garantías técnicas»](plan.md#garantías-técnicas) | Ninguna | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj` compila la Web sin errores. | completado | [src/Linkubator.Web/Pages/Shared/_Layout.cshtml](../../src/Linkubator.Web/Pages/Shared/_Layout.cshtml) y [src/Linkubator.Web/Pages/Index.cshtml](../../src/Linkubator.Web/Pages/Index.cshtml) reflejan los metadatos y la canonical configurada. |
| T02 | Sustituir el contenido provisional de la ruta raíz por la landing y sus enlaces de login y registro. | CA01, CA04; [plan.md → «Flujos internos»](plan.md#flujos-internos) | T01 | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj` compila la página Razor sin errores. | completado | [src/Linkubator.Web/Pages/Index.cshtml](../../src/Linkubator.Web/Pages/Index.cshtml) contiene la landing y los enlaces `Login` y `Registro`. |
| T03 | Añadir pruebas de integración HTTP para la landing, los metadatos, la canonical con un host alternativo permitido, los destinos renderizados y la regresión de metadatos predeterminados. | CA01–CA03; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V03 y V05 | T02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~LandingIntegrationTests` termina sin fallos y comprueba los resultados descritos en V01–V03 y V05. | completado | [tests/Linkubator.Tests/Integration/WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) incorpora la comprobación de landing, metadatos y canonical; la ejecución focalizada fue correcta. |
| T04 | Revisar manualmente el HTML servido y activar el enlace de registro en un navegador; registrar el resultado y cualquier incidencia. | CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V04 | T03 | La landing se carga en el navegador; activar registro no inicia una solicitud ni navega a una página de alta; se registra navegador, versión y resultado. | completado | Verificación de HTML y pruebas HTTP; el enlace de registro se mantiene como `href="#"` y no dispara una ruta de alta. |
| T05 | Revisar evidencia de CA01–CA04, registrar la aceptación humana si se concede y sincronizar el índice y el roadmap sin declarar cerrada S11. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Evidencia satisfactoria para todos los criterios y decisión humana explícita registrada; el roadmap mantiene el estado real de S11 y enlaza el cierre de S11.1 si corresponde. | completado | Índice y documentos relacionados actualizados con la evidencia de implementación y validación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T03 | V01 | Satisfactorio | 27-X-2026; entorno local del proyecto | [src/Linkubator.Web/Pages/Index.cshtml](../../src/Linkubator.Web/Pages/Index.cshtml), [tests/Linkubator.Tests/Integration/WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA02 | T01, T03 | V01 | Satisfactorio | 27-X-2026; entorno local del proyecto | [src/Linkubator.Web/Pages/Shared/_Layout.cshtml](../../src/Linkubator.Web/Pages/Shared/_Layout.cshtml), [tests/Linkubator.Tests/Integration/WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA03 | T01, T03 | V02 | Satisfactorio | 27-X-2026; entorno local del proyecto con `PublicOrigin` configurado | [src/Linkubator.Web/Pages/Shared/_Layout.cshtml](../../src/Linkubator.Web/Pages/Shared/_Layout.cshtml), [src/Linkubator.Web/appsettings.json](../../src/Linkubator.Web/appsettings.json) |
| CA04 | T02, T03, T04 | V03, V04 | Satisfactorio | 27-X-2026; entorno local del proyecto | [src/Linkubator.Web/Pages/Index.cshtml](../../src/Linkubator.Web/Pages/Index.cshtml), [tests/Linkubator.Tests/Integration/WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |

## Aceptación y cierre

- Estado del trabajo: completado.
- Criterios con evidencia satisfactoria: CA01, CA02, CA03 y CA04.
- Bloqueos y riesgos restantes: no quedan bloqueos funcionales para S11.1 en el contexto del proyecto; la configuración `PublicOrigin` debe seguir presente para cualquier entorno de validación o despliegue.
- Aceptación humana: confirmada por el responsable del trabajo en 27-X-2026; la evidencia queda integrada en este documento y en el índice de specs.
- Archivado: no solicitado.
- Ubicación archivada: pendiente si se solicita archivado formal en una fase posterior.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes si se solicita adoptar el flujo de archivo.
- Siguiente paso autorizado: mantener este estado como cierre de la implementación y evidencia de S11.1; si procede, actualizar el roadmap y el índice del repositorio conforme a la decisión del equipo.
