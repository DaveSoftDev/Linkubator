# Tareas: S0.5 — Configuración Web y base de Razor Pages

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 07-X-2026.
- [Plan técnico](plan.md): aprobado por DLG el 07-X-2026.
- Referencia: instrucción de DLG «Cumple requisitos» para todos los documentos S0.5.
- Enmienda: DLG, 07-X-2026, confirma que `AllowedHosts` corresponde a S0.5 según el plan principal; se añade CA-06/V-06 sin alterar el registro de aceptación original.
- Las aprobaciones documentales no prueban el estado actual. Las comprobaciones siguientes registran evidencia obtenida ahora y no afirman cuándo se crearon los artefactos.

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar la [especificación](spec.md) y el [plan](plan.md). | [CA-01 a CA-05](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación de DLG registrada. | completada | DLG, 07-X-2026: «Cumple requisitos» para ambos documentos. |
| T02 | Configurar el host para Razor Pages. | [CA-01](spec.md#criterios-de-aceptación); [V-01](plan.md#estrategia-de-validación) | T01 | Revisar registro y mapeo de endpoints Razor Pages. | completada | Revisado `src/Linkubator.Web/Program.cs`: `AddRazorPages()` y `MapRazorPages().WithStaticAssets()` presentes. 07-X-2026. |
| T03 | Conectar Tailwind al build del proyecto Web. | [CA-02](spec.md#criterios-de-aceptación); [V-02](plan.md#estrategia-de-validación) | T01 | Ejecutar build y comprobar ejecución del target y CSS de salida. | completada | Build normal de `Linkubator.sln` correcto; salida del target: Tailwind CSS v4.3.3, `Done in 58ms`; `src/Linkubator.Web/wwwroot/css/styles.css` existe (993 bytes). El timestamp no cambió respecto a la ejecución anterior, pero el target corrió en el build satisfactorio. 07-X-2026. |
| T04 | Comprobar redirección HTTP→HTTPS y respuesta HTTPS local. | [CA-03](spec.md#criterios-de-aceptación); [V-03](plan.md#estrategia-de-validación) | T02 | Verificar redirect y respuesta final con certificado/perfil HTTPS disponible. | completada | `curl.exe` contra `http://localhost:5228/` observó 307 a `https://localhost:7263/`; HTTPS devolvió 200, primero con `-k` y luego con validación normal de certificado. 07-X-2026. |
| T05 | Comprobar respuesta de la página mínima y límite arquitectónico. | [CA-04, CA-05](spec.md#criterios-de-aceptación); [V-04, V-05](plan.md#estrategia-de-validación) | T02 | Comprobar ruta raíz y revisar que no se añada uso de Infrastructure fuera de `Program`. | completada | `WebStartupTests.HomePageReturnsSuccess`: 1 passed con binarios existentes. Revisión de fuentes `src/Linkubator.Web` excluyendo `bin`, `obj` y `logs`: único uso de tipos de Infrastructure en `Program.cs`; la referencia de proyecto en el `.csproj` se conserva. Suite completa existente: 3 passed, 0 failed, 0 skipped. 07-X-2026. |
| T06 | Ejecutar build integrado y consolidar evidencias. | [CA-01 a CA-05](spec.md#criterios-de-aceptación); [V-01 a V-05](plan.md#estrategia-de-validación) | T02, T03, T04, T05 | Build sin errores/warnings y criterios con evidencia adecuada. | completada | Tras liberar DLG el proceso Web bloqueante, `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 advertencias y 0 errores, ejecutando Tailwind CSS v4.3.3 (`Done in 58ms`). Suite completa recién compilada: 3 superadas, 0 fallidas, 0 omitidas. CSS existente (993 bytes); timestamp sin cambio. 07-X-2026. |
| T07 | Revisar evidencia, aceptar resultado y archivar con autorización. | [Puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T06 | Aceptación expresa de DLG; movimiento y enlaces verificados antes del estado archivado. | completada | DLG aceptó «Cumple requisitos» el 07-X-2026. Carpeta trasladada a `specs/archive/s0.5-configuracion-web/`; enlaces entrantes/salientes y las 3 rutas del índice verificados. |
| T08 | Verificar `AllowedHosts` live tras la enmienda. | [CA-06](spec.md#criterios-de-aceptación); [V-06](plan.md#estrategia-de-validación) | T07 | Host local permitido responde y host ajeno es rechazado por el filtro de hosts. | completada | 07-X-2026, perfil HTTPS Development: `curl.exe` con `Host: localhost` devolvió HTTP 200; con `Host: disallowed.example` devolvió HTTP 400. El host de prueba se detuvo tras las solicitudes. |

Las tareas se redactan como trabajo de S0.5 en el orden del plan. Las comprobaciones sobre los archivos actuales no presuponen el momento en que se crearon. `AllowedHosts` pertenece a S0.5 según el plan principal; `PublicOrigin` pertenece a S0.6. La autoría del test `WebApplicationFactory` pertenece a S0.7.

### Enmienda de alcance posterior al archivado

El 07-X-2026, DLG confirmó que `AllowedHosts` se asigna a S0.5 conforme a la fila S0.5 de `mvp0-plan.md`. La aprobación y aceptación originales de S0.5 se conservan como registro histórico; esta enmienda añade CA-06/V-06 y registra su comprobación live.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA-01 | T02, T06 | [V-01](plan.md#estrategia-de-validación) | Conforme por revisión del host | SDK .NET 10.0.401; fuentes revisadas el 07-X-2026. | `Program.cs`: registro y mapeo Razor Pages. |
| CA-02 | T03, T06 | [V-02](plan.md#estrategia-de-validación) | Conforme | .NET SDK 10.0.401; Tailwind CSS v4.3.3; 07-X-2026. | Build normal satisfactorio con salida `Done in 58ms`; `styles.css` existe. Su mtime no cambió, sin afectar el resultado satisfactorio del target. |
| CA-03 | T04, T06 | [V-03](plan.md#estrategia-de-validación) | Conforme en ejecución local | Perfil `https`, localhost:5228/7263; 07-X-2026. | HTTP 307 a HTTPS; HTTPS 200 con validación de certificado normal. |
| CA-04 | T05, T06 | [V-04](plan.md#estrategia-de-validación) | Conforme por test y respuesta HTTPS | Test binario existente; endpoint localhost:7263; 07-X-2026. | `WebStartupTests.HomePageReturnsSuccess`: 1 passed; HTTPS local: 200. |
| CA-05 | T05, T06 | [V-05](plan.md#estrategia-de-validación) | Conforme por revisión de fuentes | Código Web actual revisado el 07-X-2026. | Uso de tipos de Infrastructure solo en `Program.cs`. |
| CA-06 (enmienda) | T08 | [V-06](plan.md#estrategia-de-validación) | Conforme por verificación live | 07-X-2026; perfil HTTPS Development, localhost:7263. | `Host: localhost` → HTTP 200; `Host: disallowed.example` → HTTP 400. |

## Aceptación y cierre

- Estado del trabajo: archivado; documentos aprobados, CA-01 a CA-05 verificados, T01 a T07 completadas y resultado aceptado por DLG.
- Enmienda posterior: CA-06/T08 añadidas el 07-X-2026 por aclaración expresa de DLG sobre la propiedad de `AllowedHosts`; V-06 pasó y no altera la aceptación original.
- Criterios pendientes de cierre técnico: ninguno.
- Bloqueos y riesgos: el timestamp de `styles.css` permaneció igual tras el build; el target Tailwind sí se ejecutó y el build completo fue satisfactorio. El cambio de timestamp no se toma como requisito adicional.
- Aprobación documental: DLG, 07-X-2026, «Cumple requisitos».
- Aceptación del resultado: DLG, 07-X-2026, «Cumple requisitos», respuesta expresa a la revisión de las evidencias registradas aquí.
- Archivado: completado el 07-X-2026 en `specs/archive/s0.5-configuracion-web/`; el índice apunta a los tres documentos y las rutas locales entrantes/salientes resuelven.
- Siguiente paso: ninguno para S0.5.
- Estado de la enmienda: V-06 conforme; `AllowedHosts` en `appsettings.Development.json` validado en vivo sin modificar valores de configuración.
