# Plan técnico: S7.9 — Pruebas de integración HTTP

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 24-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Mantener las pruebas HTTP en `tests/Linkubator.Tests/Integration`, usando `WebApplicationFactory<Program>` y fábricas de tests derivadas que configuran servicios mediante `ConfigureTestServices`.
- Añadir `WebSecurityIntegrationTests` como clase independiente y reutilizar los tests de `WebStartupTests` ya existentes para cabeceras, dashboard y método no permitido; cada test usa su factory y base SQLite temporal sin compartir estado.
- Para el análisis de parámetros y antiforgery, añadir endpoints exclusivamente en el host de pruebas mediante `IStartupFilter` o el punto de composición equivalente. Estos endpoints deben llamar a la configuración/pipeline o al analizador productivo y nunca incorporarse a `src/Linkubator.Web`.
- No añadir paquetes ni modificar código productivo. Usar únicamente `Microsoft.AspNetCore.Mvc.Testing`, xUnit y dependencias ya declaradas por el proyecto de pruebas.

### Flujos internos

1. Cada test prepara su fábrica Web y recursos temporales aislados; el cliente desactiva `AllowAutoRedirect` cuando deba inspeccionar la respuesta de redirección original.
2. El test hace solicitudes HTTP al `TestServer`, afirma status, cabeceras, contenido y efectos laterales observables según el criterio; los controles positivos y negativos comparten la misma configuración.
3. Los puntos de prueba de antiforgery obtienen la cookie y el token del propio host y envían el POST de control, además del caso sin token. La operación solo registra en memoria si el handler llegó a ejecutarse.
4. El punto de prueba de parámetros entrega `Request.Query` al analizador de producción y expone un resultado mínimo tipado/serializado necesario para la aserción, sin ejecutar casos de uso ni persistencia.
5. Los casos de `/app` y métodos no admitidos consultan las rutas reales existentes. El test de método compara la respuesta privada con la genérica/control disponible.

### Persistencia y dependencias

- Las comprobaciones de comportamiento HTTP no requieren cambios de esquema. Si el factory existente arranca el host y aplica migraciones, se conserva su patrón de base temporal y limpieza de archivos.
- Prerrequisitos: las configuraciones de seguridad de S7.1/S7.2, el host y dashboard de S7.7, y el analizador de S7.6 ya presentes.
- No se espera a que se implementen listados, formularios funcionales o el registro S7.8. El antiforgery se valida con una operación de test, no con una operación de producto que aún no existe.
- `TestServer` solo proporciona evidencia HTTP dentro del host de pruebas: no verifica transporte TLS real. El test de HTTPS se limita a la respuesta de redirección configurada.

### Garantías técnicas

- Los tests se comportan como clientes HTTP, y no invocan directamente middleware, PageModels o el analizador para afirmar los criterios de integración.
- Cada garantía de seguridad relevante tiene control negativo y/o positivo. En particular: esquema HTTP contra destino HTTPS; `Host` permitido contra no permitido; POST antiforgery sin prueba contra prueba emitida; método no permitido contra recurso privado existente/control genérico.
- Los puntos de prueba se registran exclusivamente en el host de pruebas y no crean rutas productivas.
- Los resultados deben ser deterministas y no depender del orden de tests ni de procesos Web externos.
- Ningún token antiforgery se imprime, registra o conserva en evidencia.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar xUnit y `WebApplicationFactory` existentes, sin nuevos paquetes. | Confirmado por el entorno del proyecto | [Linkubator.Tests.csproj](../../tests/Linkubator.Tests/Linkubator.Tests.csproj) | Mantiene el stack existente y evita infraestructura adicional. |
| Exponer antiforgery y análisis de query mediante endpoints efímeros del factory de pruebas. | Confirmado y aplicado | CA04 y CA07 de [spec.md](spec.md); evidencia en [tasks.md → T03 y T05](tasks.md#descomposición) | Los endpoints existen solo en el startup filter de la fábrica de pruebas y no se registraron en producción. |
| La petición HTTPS en TestServer no establece TLS real. | Confirmado, limitación del harness | [spec.md → CA01](spec.md#criterios-de-aceptación) | El resultado solo demuestra la redirección. La comprobación live de certificado queda fuera del alcance de esta spec. |
| `WebStartupTests.cs` y `Program.cs` contenían cambios concurrentes de S7.8. | Riesgo resuelto | [tasks.md → T01 y T07](tasks.md#descomposición) | Se preservaron los cambios preexistentes; las pruebas S7.9 se añadieron en `WebSecurityIntegrationTests.cs`, sin cambios productivos. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; integración HTTP de redirección del host de pruebas. | `WebApplicationFactory`; cliente con redirección automática desactivada. | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | La petición por HTTP devuelve redirect con ubicación HTTPS. No se atribuye conexión TLS. | Test, status, `Location`, resultado y límites del `TestServer`. |
| V02 | CA02 | Automatizada; integración HTTP de cabeceras. | Endpoint real accesible en TestServer. | Mismo filtro focalizado `WebStartupTests`. | Se observan las cabeceras y valores propietarios aprobados en la respuesta. | Test, cabeceras observadas, status y resultado. |
| V03 | CA03 | Automatizada; integración HTTP con control positivo y negativo. | Fábrica con `AllowedHosts` conocido y cliente que puede establecer `Host`. | Mismo filtro focalizado `WebStartupTests`. | Host permitido llega al endpoint; host no permitido se rechaza. Una perturbación temporal de la expectativa o de la configuración de prueba hace fallar la aserción y se restaura. | Tests y resultados de ambos casos; resultado de sensibilidad si se ejecuta; no conservar configuración alterada. |
| V04 | CA04 | Automatizada; integración HTTP de antiforgery aislada en el host de tests. | Endpoint POST de prueba; token/cookie obtenidos mediante GET al mismo host. | Mismo filtro focalizado `WebStartupTests`. | POST sin token falla y no ejecuta la acción; control con token pasa antiforgery y alcanza handler. La aserción negativa deja de pasar si temporalmente se deshabilita antiforgery en el punto de test, tras restaurarlo. | Tests, códigos observados y contador de invocación; nunca el valor del token. |
| V05 | CA05–CA06 | Automatizada; integración HTTP contra `/app` y ruta privada real. | Factory existente; endpoints/rutas actuales disponibles. | Mismo filtro focalizado `WebStartupTests`. | `/app` termina en dashboard; método no permitido responde `404` genérico sin datos internos. Un test de control compara la respuesta de referencia cuando proceda. | Ruta, método, status y aserciones de contenido/cabeceras pertinentes. |
| V06 | CA07 | Automatizada; integración HTTP desde query hasta el analizador de producción. | Analizador S7.6 y endpoint de test conectado a él. | Mismo filtro focalizado `WebStartupTests`. | Casos de query se reflejan según el contrato del analizador, incluidos negativos y controles; no se consulta ni autoriza información de recursos. Una perturbación temporal controlada de la respuesta/assertion demuestra sensibilidad y se restaura. | Nombres de casos, respuesta sintetizada y resultado. |
| V07 | CA01–CA07 | Revisión integrada; diff y regresión focalizada. | V01–V06. | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` y `git diff --check` | Pruebas focalizadas pasan, diff limpio y sin cambios productivos ni rutas de test registradas en producción. | Resultado real, fecha/entorno, lista de tests y diff revisado. |

Los tests automatizados verifican solo el comportamiento observado por sus solicitudes. No demuestran TLS real, compatibilidad con navegadores ni presentación visual accesible. La revisión de sensibilidades debe usar cambios temporales limitados al test y revertirlos antes de finalizar; no se cambia configuración productiva para fabricar un resultado.

## Orden de ejecución y cierre

- Tras aprobar este plan, preparar `tasks.md` con dependencias explícitas y checks inmediatos por criterio; no comenzar cambios de tests hasta la aprobación de tareas.
- Cerrar cuando CA01–CA07 tengan evidencia satisfactoria dentro de esta spec, todas las tareas propias estén completadas y exista aceptación humana registrada. La ausencia de listados productivos no bloquea CA07, que utiliza exclusivamente un endpoint del host de pruebas conectado al analizador disponible.
