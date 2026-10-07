# Tareas: S0.5 — Configuración Web y base de Razor Pages

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 7-X-2026.
- [Plan técnico](plan.md): aprobado por DLG el 7-X-2026.
- Las aprobaciones documentales no sustituyen evidencias de ejecución; las comprobaciones de hoy describen el estado actual y no la fecha histórica de implementación.

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar la [especificación](spec.md) y el [plan](plan.md). | [CA01 a CA05](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación de DLG registrada. | completada | DLG, 7-X-2026: «Cumple requisitos» para ambos documentos. |
| T02 | Configurar el host para Razor Pages. | [CA01](spec.md#criterios-de-aceptación); [V01](plan.md#estrategia-de-validación) | T01 | Revisar registro y mapeo de endpoints Razor Pages. | completada | Revisado `src/Linkubator.Web/Program.cs`: `AddRazorPages()` y `MapRazorPages().WithStaticAssets()` presentes. 7-X-2026. |
| T03 | Conectar Tailwind al build del proyecto Web. | [CA02](spec.md#criterios-de-aceptación); [V02](plan.md#estrategia-de-validación) | T01 | Ejecutar build y comprobar ejecución del target y CSS de salida. | completada | Build normal de `Linkubator.sln` correcto; salida del target: Tailwind CSS v4.3.3, `Done in 58ms`; `src/Linkubator.Web/wwwroot/css/styles.css` existe (993 bytes). El timestamp no cambió respecto a la ejecución anterior, pero el target corrió en el build satisfactorio. 7-X-2026. |
| T04 | Comprobar redirección HTTP→HTTPS y respuesta HTTPS local. | [CA03](spec.md#criterios-de-aceptación); [V03](plan.md#estrategia-de-validación) | T02 | Verificar redirect y respuesta final con certificado/perfil HTTPS disponible. | completada | `curl.exe` contra `http://localhost:5228/` observó 307 a `https://localhost:7263/`; HTTPS devolvió 200, primero con `-k` y luego con validación normal de certificado. 7-X-2026. |
| T05 | Comprobar respuesta de la página mínima y límite arquitectónico. | [CA04, CA05](spec.md#criterios-de-aceptación); [V04, V05](plan.md#estrategia-de-validación) | T02 | Comprobar ruta raíz y revisar que no se añada uso de Infrastructure fuera de `Program`. | completada | `WebStartupTests.HomePageReturnsSuccess`: 1 passed con binarios existentes. Revisión de fuentes `src/Linkubator.Web` excluyendo `bin`, `obj` y `logs`: único uso de tipos de Infrastructure en `Program.cs`; la referencia de proyecto en el `.csproj` se conserva. Suite completa existente: 3 passed, 0 failed, 0 skipped. 7-X-2026. |
| T06 | Ejecutar build integrado y consolidar evidencias. | [CA01 a CA05](spec.md#criterios-de-aceptación); [V01 a V05](plan.md#estrategia-de-validación) | T02, T03, T04, T05 | Build sin errores/warnings y criterios con evidencia adecuada. | completada | Tras liberar DLG el proceso Web bloqueante, `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 advertencias y 0 errores, ejecutando Tailwind CSS v4.3.3 (`Done in 58ms`). Suite completa recién compilada: 3 superadas, 0 fallidas, 0 omitidas. CSS existente (993 bytes); timestamp sin cambio. 7-X-2026. |
| T07 | Revisar evidencia, aceptar resultado y archivar con autorización. | [Puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T06 | Aceptación expresa de DLG; movimiento y enlaces verificados antes del estado archivado. | completada | DLG aceptó «Cumple requisitos» el 7-X-2026. Carpeta trasladada a `specs/archive/s0.5-configuracion-web/`; enlaces entrantes/salientes y las 3 rutas del índice verificados. |
| T08 | Verificar `AllowedHosts` live tras la enmienda. | [CA06](spec.md#criterios-de-aceptación); [V06](plan.md#estrategia-de-validación) | T07 | Host local permitido responde y host ajeno es rechazado por el filtro de hosts. | completada | 7-X-2026, perfil HTTPS Development: `curl.exe` con `Host: localhost` devolvió HTTP 200; con `Host: disallowed.example` devolvió HTTP 400. El host de prueba se detuvo tras las solicitudes. |

Las tareas se redactan como trabajo de S0.5 en el orden del plan. Las comprobaciones sobre los archivos actuales no presuponen el momento en que se crearon. `AllowedHosts` pertenece a S0.5 según el plan principal; `PublicOrigin` pertenece a S0.6. La autoría del test `WebApplicationFactory` pertenece a S0.7.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T06 | [V01](plan.md#estrategia-de-validación) | Conforme por revisión del host | SDK .NET 10.0.401; fuentes revisadas el 7-X-2026. | `Program.cs`: registro y mapeo Razor Pages. |
| CA02 | T03, T06 | [V02](plan.md#estrategia-de-validación) | Conforme | .NET SDK 10.0.401; Tailwind CSS v4.3.3; 7-X-2026. | Build normal satisfactorio con salida `Done in 58ms`; `styles.css` existe. Su mtime no cambió, sin afectar el resultado satisfactorio del target. |
| CA03 | T04, T06 | [V03](plan.md#estrategia-de-validación) | Conforme en ejecución local | Perfil `https`, localhost:5228/7263; 7-X-2026. | HTTP 307 a HTTPS; HTTPS 200 con validación de certificado normal. |
| CA04 | T05, T06 | [V04](plan.md#estrategia-de-validación) | Conforme por test y respuesta HTTPS | Test binario existente; endpoint localhost:7263; 7-X-2026. | `WebStartupTests.HomePageReturnsSuccess`: 1 passed; HTTPS local: 200. |
| CA05 | T05, T06 | [V05](plan.md#estrategia-de-validación) | Conforme por revisión de fuentes | Código Web actual revisado el 7-X-2026. | Uso de tipos de Infrastructure solo en `Program.cs`. |
| CA06 (enmienda) | T08 | [V06](plan.md#estrategia-de-validación) | Conforme por verificación live | 7-X-2026; perfil HTTPS Development, localhost:7263. | `Host: localhost` → HTTP 200; `Host: disallowed.example` → HTTP 400. |

## Aceptación y cierre

- Estado del trabajo: archivado; documentos aprobados, CA01 a CA05 verificados, T01 a T07 completadas y resultado aceptado por DLG.
- Criterios pendientes de cierre técnico: ninguno.
- Bloqueos y riesgos: el timestamp de `styles.css` permaneció igual tras el build; el target Tailwind sí se ejecutó y el build completo fue satisfactorio. El cambio de timestamp no se toma como requisito adicional.
- Aprobación documental: DLG, 7-X-2026, «Cumple requisitos».
- Aceptación del resultado: DLG, 7-X-2026, «Cumple requisitos», respuesta expresa a la revisión de las evidencias registradas aquí.
- Archivado: completado el 7-X-2026 en `specs/archive/s0.5-configuracion-web/`; el índice apunta a los tres documentos y las rutas locales entrantes/salientes resuelven.
- Siguiente paso: ninguno para S0.5.
- Estado de la enmienda: V06 conforme; `AllowedHosts` en `appsettings.Development.json` validado en vivo sin modificar valores de configuración.
