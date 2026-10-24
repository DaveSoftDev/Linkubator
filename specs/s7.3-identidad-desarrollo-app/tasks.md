# Tareas: S7.3 — Identidad de desarrollo en `/app`

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 22-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 22-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 22-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 22-X-2026; validación focalizada `WebStartupTests` 7/7 y `git diff --check` limpio.
  - Evidencia consolidada: [T01–T04](#descomposición), [evidencia por criterio](#evidencia-por-criterio) y [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 identifica el host de pruebas y el consumidor de Application adecuados. T02 prepara el endpoint únicamente en la configuración de pruebas y las sustituciones controladas del proveedor. T03 añade la comprobación de identidad y la prueba negativa; T04 verifica que la ruta pública no invoque el proveedor y ejecuta la validación integrada. Ninguna tarea depende de S7.7 ni de S7.9.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar el host `WebApplicationFactory<Program>`, servicios de Application y rutas existentes; elegir el consumidor real de Application que se observará y el mecanismo de endpoint de prueba exclusivo del host de test. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01 | Ninguna | Revisar el harness y su resolución de servicios. Resultado esperado: consumidor y mecanismo identificados; la solicitud de test pasará por el middleware productivo antes de invocarlo y no se añadirá endpoint productivo. | completada | Encontrados `WebApplicationFactory<Program>` y `DevelopmentIdentityMiddlewareTests`. `DashboardService.GetDashboard` consume `ICurrentUser`; `IDashboardRepository` permite observar el ID usado. El host Web no tiene aún páginas `/app`; la factory debe añadir ruta de prueba y sustituir el repositorio/provider solo en el host de test. Revisión el 22-X-2026. |
| T02 | Configurar el host de pruebas con proveedor de identidad determinista y un endpoint de prueba que invoque el consumidor real seleccionado en T01. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01 | T01 | Ejecutar la prueba de integración de la ruta privada. Resultado esperado: el endpoint queda disponible solo en pruebas y la solicitud atraviesa el middleware; no se registran cambios de producción ni a contratos. | completada | En `WebStartupTests`, `WebApplicationFactory` añade `/app/test-dashboard` solo al host de test, reemplaza proveedor/repositorio y registra `DashboardService`. No se modificaron rutas productivas ni contratos. Test focalizado pasó el 22-X-2026. |
| T03 | Añadir la prueba de integración que comprueba el `UserId` recibido por Application y repetirla con datos de identidad manipulados por el cliente; verificar sensibilidad de la aserción mediante una perturbación temporal, restaurada al finalizar. | CA01–CA02; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V01–V02 | T02 | Ejecutar los tests nuevos. Resultado esperado: Application observa el ID del proveedor en ambos casos y la perturbación temporal produce un fallo de aserción, tras cuya reversión los tests pasan. | completada | `AppDashboardTestEndpoint_UsesResolvedIdentityEvenWhenRequestContainsUserId` invocó el `DashboardService` real mediante HTTP; el repositorio de captura observó `42` con `?userId=99`. El test falló al cambiar temporalmente el esperado a `99` (esperado 99, observado 42), y se restauró. Evidencia del 22-X-2026 en `WebStartupTests`. |
| T04 | Añadir la prueba de ruta pública con proveedor de fallo y ejecutar la validación focalizada integrada; revisar alcance y sincronizar la referencia de avance en el roadmap. | CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), [«Estrategia de validación»](plan.md#estrategia-de-validación), V03–V04 | T02 | Ejecutar el filtro focalizado que incluye V01–V04 y `git diff --check`. Resultado esperado: la ruta pública responde sin invocar el proveedor; los checks pasan y los cambios quedan limitados a tests/documentación necesarios. Actualizar roadmap solo al reflejar estado real, sin inferir aceptación. | completada | `HomePage_DoesNotResolvePrivateIdentity` confirmó respuesta exitosa y cero llamadas del proveedor posteriores al arranque. Los 7 tests de `WebStartupTests` pasaron el 22-X-2026 con `-p:BuildProjectReferences=false` debido a procesos Web activos que bloqueaban los ensamblados; `git diff --check` limpio. Se observó la advertencia del middleware HTTPS de tests («Failed to determine the https port for redirect»), sin fallos. |
| T05 | Revisar la evidencia de CA01–CA03 y registrar aceptación humana o el bloqueo y las correcciones solicitadas. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que cada criterio tiene evidencia válida y registrar decisión explícita, responsable, fecha y referencia. Resultado esperado: aceptación documentada o tarea bloqueada; no completar sin aceptación expresa. | completada | DLG aceptó el resultado el 22-X-2026 con «Fase Completada», instrucción explícita en esta conversación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | [V01](plan.md#estrategia-de-validación) | Satisfactoria: petición HTTP bajo `/app` invocó `DashboardService` y el repositorio de captura recibió el `UserId` del proveedor, 42. | 22-X-2026; .NET 10, `WebApplicationFactory<Program>` | `WebStartupTests.AppDashboardTestEndpoint_UsesResolvedIdentityEvenWhenRequestContainsUserId`; suite focalizada 7/7 |
| CA02 | T02–T03 | [V02](plan.md#estrategia-de-validación) | Satisfactoria: `?userId=99` no alteró la identidad observada (42); sensibilidad verificada con perturbación del esperado a 99, que falló (actual 42), restaurada después. | 22-X-2026; .NET 10, `WebApplicationFactory<Program>` | `WebStartupTests.AppDashboardTestEndpoint_UsesResolvedIdentityEvenWhenRequestContainsUserId`; fallo negativo esperado y suite restaurada 7/7 |
| CA03 | T04 | [V03](plan.md#estrategia-de-validación) | Satisfactoria: la landing respondió exitosamente sin llamada del proveedor después de reiniciar su contador tras el arranque del host. | 22-X-2026; .NET 10, `WebApplicationFactory<Program>` | `WebStartupTests.HomePage_DoesNotResolvePrivateIdentity`; suite focalizada 7/7 |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 tienen evidencia técnica satisfactoria.
- Bloqueos y riesgos restantes: ninguno identificado. La validación se ejecutó sin reconstruir referencias (`-p:BuildProjectReferences=false`) porque procesos Web activos bloqueaban ensamblados; la compilación del proyecto de tests y los tests focalizados pasaron. Se observó una advertencia HTTPS de `WebApplicationFactory`, sin afectar los resultados.
- Aceptación humana: DLG, 22-X-2026 («Fase Completada»), instrucción explícita en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: solicitar aceptación humana de S7.3; la validación técnica está completa, pero no constituye aceptación.
