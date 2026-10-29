# Plan técnico: S7.8 — Registro de peticiones Web

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: DLG, 24-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

Configurar el middleware de registro de peticiones Serilog en el pipeline existente de `Linkubator.Web`, limitando los cambios al host y al harness de pruebas. El registro debe capturar las peticiones con la misma infraestructura y destinos de logging ya registrados. Aplicar la exclusión de la cadena de consulta a las peticiones privadas para no registrar parámetros de búsqueda; para las rutas que transportan tokens, evitar el registro de la URL completa. No modificar logging de eventos funcionales ni alterar la configuración de sus destinos.

La política de datos que se deben excluir es propiedad de [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos); el objetivo técnico y el pipeline estructurado se apoyan en [architecture.md → «Logs»](../../../context/architecture.md#logs).

### Flujos internos

1. El host recibe la petición y ejecuta el middleware de request logging existente en el orden adecuado para capturar datos de petición y respuesta sin modificar el enrutamiento.
2. La configuración del evento de petición selecciona datos operativos no sensibles y omite la cadena de consulta en las rutas privadas.
3. Cuando una petición incluye un token en la URL, el evento se configura para no emitir la URL completa.
4. Los tests envían marcadores sintéticos sensibles y comprueban el texto y las propiedades capturadas por un sink de logging controlado, además de confirmar la presencia de un evento de request logging.

### Persistencia y dependencias

No se modifica persistencia, SQL ni transacciones, y no se añaden dependencias: el host ya usa Serilog y `UseSerilogRequestLogging`. Se reutilizarán el `WebApplicationFactory` y los tests de integración del proyecto `Linkubator.Tests`. S7.9 cubre los casos HTTP transversales y queda fuera del cierre de esta spec.

### Garantías técnicas

- Los datos inspeccionados por los tests no contienen cadena de consulta de listados privados, texto de búsqueda, token ni URL completa de las solicitudes con token, conforme a [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos).
- El pipeline conserva un evento de registro de petición con datos operativos no sensibles, conforme a [architecture.md → «Logs»](../../../context/architecture.md#logs).
- Las pruebas focalizadas deben observar eventos de logging reales del host de pruebas; la ausencia de un marcador en el HTML de respuesta no demuestra que el logger lo haya excluido.
- La prueba negativa temporal debe demostrar que las aserciones detectan una configuración insegura; cualquier perturbación se restaura y no queda en el árbol de trabajo.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar `UseSerilogRequestLogging` ya presente en el pipeline de `Program.cs` | Confirmado | Implementación actual de `Linkubator.Web`; [architecture.md → «Logs»](../../../context/architecture.md#logs) | Acota el cambio a la configuración de captura y mantiene el proveedor actual. |
| Capturar eventos mediante un sink controlado en el host de pruebas | Propuesta | Criterios CA01–CA04 de [spec.md](spec.md) | Debe comprobarse que el sink recibe los eventos emitidos desde la factory sin afectar el logging global del proceso de tests. |
| Aplicar exclusiones con una política basada en la ruta, no registrando la query privada ni URL en rutas con tokens | Propuesta | [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | Determinar en las tareas cómo reconocer las rutas sin duplicar reglas de producto ni exponer valores sensibles en filtros o diagnósticos. |
| La suite transversal de integración HTTP queda en S7.9 | Confirmado | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | No bloquea las pruebas focalizadas propias de esta spec. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Integración HTTP focalizada y captura de logs del host | `WebApplicationFactory` disponible y sink controlado conectado al host de prueba | Añadir o reutilizar test en `WebStartupTests`; enviar petición a ruta estable y revisar evento capturado de request logging. Comando previsto: `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Se captura al menos un evento de petición asociado a la solicitud, con información operativa no sensible. | Test ejecutado, evento capturado con valores sensibles redactados, comando, fecha y resultado. |
| V02 | CA02 | Integración HTTP automatizada con marcador de búsqueda sintético | V01 y ruta privada estable del host | Solicitar una ruta de listado privado con un valor único en `q`; inspeccionar el evento capturado y sus propiedades de URL/query. | El evento no contiene la cadena de consulta ni el marcador de búsqueda; el evento de petición se conserva. | Petición y aserción identificadas; resultado sin reproducir el marcador sensible en la evidencia. |
| V03 | CA03–CA04 | Integración HTTP automatizada con token centinela sintético | V01 y endpoint de prueba que no valida ni persiste credenciales reales | Enviar una petición controlada a una ruta de prueba con token sintético en la URL; buscar el token, la ruta completa y credenciales en las propiedades y mensaje del evento. | No aparece el token ni la URL completa; la inspección de los eventos cubre los datos sensibles enumerados por la fuente para estos casos. | Test/endpoint de harness, tipos de propiedades inspeccionadas y resultado; no guardar el token centinela en el informe. |
| V04 | CA01–CA04 | Comprobación negativa de sensibilidad y revisión técnica | V01–V03 implementadas | En un cambio temporal del test, permitir la captura de query o URL completa y comprobar que la aserción correspondiente falla; restaurar inmediatamente. Revisar además `git diff --check` y que no se alteraron destinos de logging. | Las aserciones fallan ante la regresión temporal, pasan tras restaurar la configuración y el diff no contiene perturbaciones residuales. | Resultado de perturbación/restauración, resultado de diff-check y revisión; no incluir valores sensibles. |

Los tests anteriores verifican las propiedades y contenido de los eventos observados en el sink del host; no prueban la política de retención de destinos externos ni sustituyen la suite HTTP transversal asignada a S7.9.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` a partir de la plantilla. Las tareas cubrirán el sink de pruebas, configuración focalizada, casos con marcadores sintéticos, comprobación negativa y revisión integrada.
- No iniciar cambios productivos antes de la aprobación de las tareas.
- CA01–CA04 se cierran con evidencia de las comprobaciones V01–V04 propias de esta spec; no se espera trabajo de S7.9.
- El cierre requiere evidencia satisfactoria de todos los criterios, tareas propias completadas y aceptación humana registrada en `tasks.md`.
