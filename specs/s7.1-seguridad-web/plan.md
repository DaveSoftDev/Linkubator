# Plan técnico: S7.1 — Seguridad web básica

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: DLG, aprobado el 22-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

El host del proyecto Web configura el middleware y las convenciones de Razor Pages necesarias para cumplir [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web). Se reutiliza `AllowedHosts` ya incorporado en S0.5, sin crear otra política de validación de `Host`. El pipeline aplica la redirección HTTPS y conserva la protección antiforgery predeterminada de Razor Pages para los formularios que modifican datos.

El cambio se limita a la capa Web y a la configuración de host que sea necesaria. No añade reglas de negocio, autenticación, páginas funcionales ni cabeceras adicionales, y no altera el límite de referencias entre proyectos establecido en [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).

### Flujos internos

1. El host recibe una petición y valida el `Host` mediante la configuración admitida.
2. Las peticiones HTTP se redirigen a HTTPS.
3. Razor Pages procesa formularios que modifican datos mediante POST y exige validación antiforgery antes de ejecutar su handler.
4. Las solicitudes HTTPS con `Host` permitido continúan por el pipeline existente.

### Persistencia y dependencias

No se modifica persistencia ni se añaden dependencias de paquetes. La comprobación local de HTTPS depende del certificado de desarrollo disponible. `AllowedHosts` existente es el prerrequisito para validar hosts; su configuración inicial pertenece a [S0.5 — Configuración Web](../archive/s0.5-configuracion-web/spec.md).

La suite de integración HTTP amplia de HTTPS, `Host` y antiforgery está asignada a S7.9 en [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l). Esa asignación no impide ejecutar ni completar en S7.1 las comprobaciones focalizadas de sus propios criterios; S7.9 sigue siendo trabajo independiente.

### Garantías técnicas

- Las solicitudes con un `Host` ajeno no llegan a ejecutar las páginas.
- El tráfico HTTP se conduce a HTTPS conforme al host y al entorno configurados.
- Las operaciones que cambian datos no se ejecutan cuando falta una prueba antiforgery válida.
- La implementación no relaja la protección predeterminada de Razor Pages ni confía en valores de `Host` enviados por la petición para construir destinos.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar `AllowedHosts` configurado en S0.5 para el rechazo de hosts ajenos | Confirmado por el alcance previo | [S0.5 — Configuración Web](../archive/s0.5-configuracion-web/spec.md) | Verificar la configuración efectiva; ajustar solo si la validación revela que el comportamiento previsto no se cumple. |
| Mantener antiforgery de Razor Pages para POST que modifican datos | Confirmado | [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web) | No deshabilitar ni omitir la validación en handlers de modificación. |
| La prueba HTTPS local puede depender de certificado no disponible | Riesgo de entorno | [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web) | Registrar el criterio como bloqueado para validación live si falta el certificado; no atribuir a TestServer evidencia de HTTPS real. |
| Las pruebas HTTP automatizadas se entregan en S7.9 | Asignación confirmada en el plan de implementación | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | Coordinar que la suite S7.9 verifique los tres criterios; este plan no reasigna su autoría. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Integración HTTP local; distingue redirección de transporte HTTPS real | Host Web y certificado de desarrollo disponibles; perfil HTTP/HTTPS identificado | Solicitar la ruta por HTTP y registrar el estado y `Location`; solicitarla directamente por HTTPS y registrar el resultado. Esta comprobación puede ejecutarse en S7.1; la suite amplia y automatizada también cubre el tema en S7.9 como trabajo independiente. | HTTP redirige al destino HTTPS y el host atiende la solicitud HTTPS. TestServer por sí solo no demuestra HTTPS real. | Fecha, perfil y esquema usados, estado de redirección y estado final; anotar si la validación quedó limitada por el certificado. |
| V02 | CA02 | Integración HTTP y control negativo | Host Web ejecutándose y un `Host` permitido identificado | Solicitar la misma ruta con un `Host` ajeno y con el `Host` permitido. Esta comprobación puede ejecutarse en S7.1; la suite amplia de S7.9 es independiente. | La solicitud ajena se rechaza; la petición de control sigue atendiendo la ruta. | Hosts usados (sin datos sensibles), códigos HTTP, resultado de ambos casos y fecha. |
| V03 | CA03 | Integración HTTP; control negativo y positivo | Endpoint POST existente que modifica datos, estado inicial conocido y protección antiforgery activa | Obtener el formulario y enviar el POST sin prueba antiforgery válida y luego con la prueba emitida. Esta comprobación se realiza en cuanto exista un endpoint de modificación en el alcance, sin depender de S7.9. | La solicitud sin prueba válida se rechaza antes de ejecutar la modificación; el control positivo supera antiforgery y alcanza el resultado funcional previsto. | Endpoint probado, método, presencia/ausencia de prueba, resultado HTTP y comprobación de que el caso negativo no modificó datos. No registrar el valor del token. |

Las comprobaciones automatizadas asignadas a S7.9 pueden corroborar el comportamiento HTTP, pero no demuestran por sí solas que el certificado local esté instalado ni que un navegador externo confíe en él. No se automatiza la UI en esta spec.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` con las tareas y dependencias explícitas; la implementación no comienza antes de esa aprobación.
- Registrar evidencia por criterio y mantener cualquier limitación del certificado local como pendiente, no como resultado satisfactorio.
- Cerrar solo con evidencia satisfactoria de CA01–CA03, tareas propias completadas y aceptación humana explícita. La cobertura más amplia de S7.9 permanece como trabajo vecino y no se atribuye a S7.1.
