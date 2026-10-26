# Tareas: S9.1 — Crear enlace

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 26-X-2026 («Siguiente»).
  - Tareas: DLG, 26-X-2026 («Ok, ahora inicia la implementación de S9.1»), autorización explícita de ejecución.
- Implementación:
  - Estado: completada y aceptada.
  - Inicio: 26-X-2026, por instrucción de DLG («Ok, ahora inicia la implementación de S9.1»).
  - Finalización: 26-X-2026; T03–T07 verificadas mediante build y pruebas focalizadas.
  - Evidencia consolidada: [«Validación ejecutada»](#validación-ejecutada).
- Aceptación humana registrada: T08, DLG, 26-X-2026 («Comprobado»); T09, DLG, 26-X-2026 («Completado»).

## Descomposición

T01–T09 están completadas y aceptadas. La evidencia técnica y la aceptación humana quedan registradas; el cierre de S9.1 queda documentado en esta tarea y en el roadmap.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Alinear en `specifications.md` la presentación de los formularios canónicos de enlaces y colecciones: conservar las páginas canónicas y permitir los diálogos como presentación alternativa, sin alterar rutas ni comportamiento. | CA01; [plan.md → «Decisiones técnicas y riesgos»](plan.md#decisiones-técnicas-y-riesgos). | Ninguna | Revisar el texto y las referencias en «Rutas privadas»; la especificación funcional queda coherente con la decisión y el roadmap, sin ampliar el alcance. | completada | Sincronización documental aplicada el 26-X-2026 por instrucción de DLG («Sincroniza la contradicción documental. MVP0 funciona con formularios»); enlaces verificados. |
| T02 | Al iniciar la implementación, registrar fecha y referencia real en esta cabecera y sincronizar el estado de S9.1 en roadmap. | Trazabilidad de implementación; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T01 | La cabecera distingue aprobación de tareas e inicio de implementación; roadmap refleja el estado real, sin anticipar cierre. | completada | Inicio y autorización registrados el 26-X-2026 en la cabecera y en roadmap. |
| T03 | Crear la ruta canónica y el PageModel GET del enlace; cargar opciones de colección del usuario identificado y compartir la carga con el sidebar sin una consulta duplicada ni un fallback vacío ante error. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos). | T02 | Build del proyecto Web; GET obtiene datos del servicio de colecciones con identidad de servidor y conserva las rutas de las páginas privadas existentes. | completada | [New.cshtml.cs](../../src/Linkubator.Web/Pages/App/Links/New.cshtml.cs), carga compartida en [PrivatePageModel.cs](../../src/Linkubator.Web/Pages/App/PrivatePageModel.cs); V01 y regresión focalizada satisfactorias. |
| T04 | Crear el markup del formulario y su input model, incluidos campos opcionales, colección, privacidad desmarcada, etiquetas accesibles y presentación de errores reutilizable. | CA01, CA05; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Garantías técnicas»](plan.md#garantías-técnicas). | T03 | Build Web; inspección del HTML Razor confirma método POST, labels asociados y controles con identificadores estables para errores. | completada | [New.cshtml](../../src/Linkubator.Web/Pages/App/Links/New.cshtml) reutiliza el diseño de colecciones. CSS generado y selectores comprobados; V01 y V05 satisfactorias. |
| T05 | Implementar el POST que delega la creación en `ILinkService.CreateLink`, mapea sus resultados al formulario y redirige al listado solo en caso de éxito. | CA02–CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos). | T04 | Build Web; la página no accede a repositorios, no recibe `UserId` y distingue errores de campo de avisos generales. | completada | Handler, registro de servicio y acceso desde Dashboard implementados; [LinkCreateValidator.cs](../../src/Linkubator.Application/Validation/Links/LinkCreateValidator.cs) atribuye errores reutilizando políticas de Domain. V02–V05 satisfactorias. |
| T06 | Añadir tests Web de integración para opciones propias y completas, estado inicial, creación válida, creación pública permitida, URL/imagen inválidas, duplicados, colección inválida/ajena y antiforgery. | CA01–CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación). | T05 | Compila la clase focalizada `LinkCreationIntegrationTests`; sus casos expresan tanto resultados HTTP como ausencia de altas indebidas. | completada | [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs): 25 casos HTTP; [LinkCreateValidatorTests.cs](../../tests/Linkubator.Tests/Application/LinkCreateValidatorTests.cs): 5 casos de campos. Los 30 pasan. |
| T07 | Ejecutar validación focalizada, build, comprobación negativa de sensibilidad del test de propiedad y revisión final de enlaces/documentación; registrar resultados y límites reales. | CA01–CA05; validaciones V01–V05 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación). | T06 | Los comandos focalizados y el build terminan satisfactoriamente; la comprobación negativa detecta la infracción y se restaura el estado correcto antes de repetir la suite. | completada | 143 pruebas focalizadas satisfactorias; build sin advertencias ni errores; mutación detectada y restaurada. Detalles y límites en [«Validación ejecutada»](#validación-ejecutada). |
| T08 | Revisar la evidencia completa y solicitar/registrar la aceptación humana del resultado, sin inferirla de tests verdes. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T07 | La aceptación queda registrada explícitamente con responsable, fecha y referencia, o se anotan el rechazo y las correcciones necesarias. | completada | Aceptación humana registrada por DLG el 26-X-2026 («Comprobado»). |
| T09 | Tras la aceptación, actualizar el roadmap con el estado y enlace a la evidencia de cierre en estas tareas; verificar que los enlaces sigan resolviendo. | Trazabilidad de cierre; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T08 | Roadmap e índice reflejan únicamente el estado aceptado y los enlaces locales resuelven. | completada | Roadmap e índice actualizados con cierre aceptado; enlaces locales verificados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T03, T04, T06, T07 | V01, V05 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: selector completo y ordenado, solo propio, privacidad inicial desmarcada y GET sin escrituras. | 26-X-2026; Windows, .NET SDK 10.0.402, SQLite en memoria | Casos GET de [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs). |
| CA02 | T05–T07 | V02, V05 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: altas privadas/públicas permitidas, opcionales, URL ajustada, propietario de servidor y proyección FTS persistida. | 26-X-2026; mismo entorno | Casos de datos válidos y opcionales de [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs). |
| CA03 | T05–T07 | V03 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: URL, imagen, texto y duplicado rechazados sin alta; datos conservados y colección del duplicado identificada. | 26-X-2026; mismo entorno | Casos de errores de [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs) y [LinkCreateValidatorTests.cs](../../tests/Linkubator.Tests/Application/LinkCreateValidatorTests.cs). |
| CA04 | T05–T07 | V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: identificadores inválidos, inexistentes y ajenos no crean datos ni revelan la colección ajena; sensibilidad comprobada. | 26-X-2026; mismo entorno | Casos de colección y comprobación negativa en [«Validación ejecutada»](#validación-ejecutada). |
| CA05 | T04–T07 | V01, V05 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio en alcance técnico: POST sin token rechazado, POST válido operativo y asociaciones de etiquetas/errores comprobadas en HTML. | 26-X-2026; mismo entorno | Casos de formulario y antiforgery de [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs); build satisfactorio. No acredita revisión visual o accesibilidad manual completa. |

## Validación ejecutada

Ejecutada el 26-X-2026 en Windows con .NET SDK 10.0.402, xUnit/VSTest y SQLite en memoria compartida por cada fixture HTTP.

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter '(FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkServiceTests|FullyQualifiedName~LinkCreateValidatorTests|FullyQualifiedName~ValidationCoverageTests|FullyQualifiedName~CollectionsListingIntegrationTests|FullyQualifiedName~Architecture|FullyQualifiedName~WebStartupTests)&FullyQualifiedName!~Startup_WhenDatabaseIsNew&FullyQualifiedName!~Startup_WhenProvisioningConfigurationIsInvalid&FullyQualifiedName!~Startup_WhenDevelopmentIdentityCannotBeResolved' --nologo
dotnet build Linkubator.sln --no-restore --nologo
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-build --no-restore --filter 'FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkCreateValidatorTests' --nologo
```

- Resultados: 143 pruebas satisfactorias en la selección de regresión; build con cero advertencias y errores; las dos clases nuevas suman 30 casos satisfactorios.
- Sensibilidad V04: se sustituyó temporalmente en el handler el identificador enviado por el de la primera colección propia. `PostNewLink_WithForeignCollection_DoesNotCreateOrDiscloseForeignData` falló al recibir redirección de éxito en vez del formulario de error. Se restauró el identificador original y se repitió satisfactoriamente la selección de 143 pruebas. La mutación demuestra detección de la aceptación indebida del envío ajeno; no simula una escritura en la colección ajena ni demuestra aisladamente todas las defensas de persistencia.
- CSS: regenerado desde [develop.css](../../src/Linkubator.Web/wwwroot/css/develop.css) con `@tailwindcss/cli@4.3.3`, versión del CSS existente; comprobados los selectores compartidos de selector, foco y errores en [styles.css](../../src/Linkubator.Web/wwwroot/css/styles.css). El ejecutable externo condicionado por el proyecto no existe en este entorno; no se cambió su configuración.
- Dependencias: tras el fallo de restauración contra un feed privado no autorizado, se restauró el proyecto de tests usando NuGet.org explícitamente, sin modificar configuración ni dependencias.
- Límites de regresión: la primera ejecución incluyó todos los casos de [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs). Tres fallan al esperar dos migraciones cuando existen tres scripts anteriores a S9.1. Se excluyeron explícitamente del comando final, sin corregir esos tests ajenos ni declarar verde la suite completa.
- Revisión de HTML y reutilización de estilos comprobadas técnicamente; la revisión visual y de accesibilidad manual completa no se ha realizado.
- Enlaces locales y anclas de los artefactos S9.1 comprobados. Git no reconoce el destino de metadatos del worktree; no se ejecutó satisfactoriamente `git diff --check`, no se modificó su configuración y no se crearon commits.

## Aceptación y cierre

- Estado del trabajo: implementado, validado técnicamente y aceptado por DLG.
- Criterios sin evidencia satisfactoria en el alcance técnico: ninguno.
- Bloqueos y riesgos restantes: regresiones preexistentes de arranque y metadatos Git no disponibles, detallados en [«Validación ejecutada»](#validación-ejecutada); no bloquean la aceptación de S9.1.
- Aceptación humana: T08 = DLG, 26-X-2026 («Comprobado»); T09 = DLG, 26-X-2026 («Completado»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso: cerrar la trazabilidad documental y dejar S9.1 en estado aceptado en los índices y roadmap; no se considera un trabajo pendiente.
