# Tareas: S7.1 — Seguridad web básica

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 22-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 22-X-2026 («Siguiente»).
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 22-X-2026 («Inicia la implementación»).
  - Finalización: DLG, 22-X-2026 («Seguridad ready»); comprobaciones live de HTTP, HTTPS y `Host` ajeno satisfactorias. CA03 no es aplicable a la Web actual porque no hay operaciones que modifiquen datos mediante formularios.
  - Evidencia consolidada: T01–T04 y tabla «Evidencia por criterio».
- Aceptación humana: DLG, 22-X-2026 («Seguridad ready»). Trabajo completado para el alcance actual; no archivado.

## Descomposición

La configuración y validación inmediata de T01–T02 se completan dentro de S7.1. La prueba HTTP amplia se mantiene como trabajo independiente de S7.9 y no condiciona ni bloquea S7.1. La revisión manual del certificado se limita al entorno local disponible.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar la configuración actual del host y aplicar los ajustes acotados de redirección HTTPS, `AllowedHosts` y antiforgery que sean necesarios para el alcance aprobado. | CA01–CA03; [componentes y contratos](plan.md#componentes-y-contratos) | Ninguna | Revisar diff y configuración del pipeline; solo quedan los cambios necesarios y no se relajan `AllowedHosts` ni la protección antiforgery de Razor Pages. | completada | `Program.cs`: se configura `HostFilteringOptions` desde `AllowedHosts` y se activa `UseHostFiltering()` antes de `UseHttpsRedirection()`. `appsettings.json`: `AllowedHosts` se restringe a localhost, IPv4 loopback e IPv6 loopback. `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter WebStartupTests`: 4/4 aprobadas. No se encontraron formularios ni handlers POST de modificación; antiforgery predeterminado de Razor Pages se mantiene. Revisión del diff sin cambios en middleware antiforgery. Fecha: 22-X-2026. |
| T02 | Validar inmediatamente las garantías del cambio de host mediante el check disponible, revisión de configuración y comprobación de los handlers/formularios existentes. | CA01–CA03; [garantías técnicas](plan.md#garantías-técnicas), [V01–V03](plan.md#estrategia-de-validación) | T01 | Ejecutar los tests focalizados y revisar si el código actual ofrece endpoints que permitan pruebas específicas; registrar lo que esos checks demuestran y los límites restantes sin depender de trabajo futuro. | completada | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter WebStartupTests`: 4/4 aprobadas. Los tests verifican arranque/provisionamiento, no los casos HTTP de CA01–CA03. Revisión de `src/Linkubator.Web`: no existen formularios ni handlers POST de modificación en el estado actual; antiforgery de Razor Pages sigue habilitado por defecto. La configuración y el pipeline están aplicados. Esta tarea queda terminada con sus checks disponibles; las comprobaciones live y la suite HTTP amplia siguen como pendientes propias, no bloqueantes para T02. Fecha: 22-X-2026. |
| T03 | Comprobar HTTPS local con el certificado de desarrollo y registrar el resultado separado de cualquier prueba con TestServer. | CA01–CA02; [V01–V02](plan.md#estrategia-de-validación) | T01, T02 | Arrancar la Web con el perfil `https`; comprobar HTTP sin seguir redirecciones, HTTPS con validación normal del certificado y petición con `Host` ajeno. | completada | Arranque local del perfil `https` desde `src/Linkubator.Web` (22-X-2026). `GET http://localhost:5228/`: 307 Temporary Redirect, `Location: https://localhost:7263/`. `GET https://localhost:7263/`: 200 OK; validación TLS normal satisfactoria. `GET http://localhost:5228/` con `Host: attacker.invalid`: 400 Bad Request y sin `Location`. Se detuvo únicamente el proceso iniciado para la validación y se limpiaron sus archivos temporales. |
| T04 | Revisar evidencias por criterio, resolver la aplicabilidad actual de CA03, actualizar la trazabilidad y registrar aceptación humana del alcance S7.1. | CA01–CA03; [Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T02, T03 | Verificar que cada criterio aplicable tenga evidencia; dejar explícita la no aplicabilidad actual de CA03 si no existe operación de modificación; registrar aceptación y actualizar el roadmap e índice sin archivar automáticamente. | completada | CA01 y CA02 comprobados con peticiones live. Inspección de `src/Linkubator.Web`: solo páginas `Index`, `Error` y `Privacy`; no se encontraron formularios ni handlers/rutas de modificación. Razor Pages mantiene antiforgery predeterminado. CA03 no tiene operación aplicable en el estado actual. DLG aceptó el resultado con «Seguridad ready» el 22-X-2026. Roadmap e índice actualizados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | [V01](plan.md#estrategia-de-validación) | Satisfactorio: HTTP redirigió a HTTPS y la ruta HTTPS respondió correctamente con validación TLS normal. | Validación live, 22-X-2026; perfil `https`, puertos 5228/7263 | [T03](#descomposición): HTTP `307` con `Location: https://localhost:7263/`; HTTPS `200 OK`, TLS válido. |
| CA02 | T01–T03 | [V02](plan.md#estrategia-de-validación) | Satisfactorio: el host permitido responde y un `Host` ajeno se rechaza. | Validación live, 22-X-2026; perfil `https` | [T03](#descomposición): solicitud permitida llega a HTTPS con `200 OK`; petición HTTP con `Host: attacker.invalid` responde `400 Bad Request`, sin redirección. |
| CA03 | T01–T04 | [V03](plan.md#estrategia-de-validación) | No aplicable a las rutas Web existentes: la especificación condiciona el criterio a que exista una operación que modifique datos mediante formulario; la inspección no encontró formularios ni handlers/rutas de modificación. Razor Pages conserva antiforgery predeterminado. | Inspección de `src/Linkubator.Web`, 22-X-2026 | [T04](#descomposición): CA03 queda sin endpoint aplicable; no se inventó una operación ni se atribuye un test antiforgery inexistente. |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado para el alcance actual.
- CA01 y CA02: evidencia live satisfactoria registrada en T03.
- CA03: no aplicable a las rutas Web existentes porque no hay operación de modificación mediante formulario; la protección antiforgery predeterminada de Razor Pages permanece habilitada. Si se incorpora una operación aplicable, su comportamiento se valida dentro del trabajo que la introduzca.
- Seguimiento independiente, no bloqueante: la suite HTTP amplia corresponde a S7.9 y no es requisito para completar S7.1.
- Aceptación humana: DLG, 22-X-2026 («Seguridad ready»).
- Archivado: no solicitado; el trabajo permanece en la carpeta activa.
