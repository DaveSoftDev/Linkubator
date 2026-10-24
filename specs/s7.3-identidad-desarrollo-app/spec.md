# Especificación: S7.3 — Identidad de desarrollo en `/app`

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 22-X-2026 («Siguiente»).

## Objetivo

Comprobar que las peticiones privadas de `/app` proporcionan a los casos de uso la identidad de desarrollo resuelta por el proveedor existente, antes de ejecutar el endpoint, y que los datos enviados por el cliente no pueden elegir esa identidad.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): tarea S7.3 y Definition of Done de S7.
- [specifications.md → «Identidad de desarrollo»](../../context/specifications.md#identidad-de-desarrollo): provisión y uso de la identidad.
- [architecture.md → «Autenticación»](../../context/architecture.md#autenticación) y [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos): composición, contexto del usuario y límites entre capas.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas): identidad autoritativa y propiedad de datos.
- [S4.2 — Contratos de repositorios, Unit of Work e identidad identificada](../archive/s4.2-contratos-repositorios-identidad/spec.md) y [S4.3 — Contrato del proveedor de identidad de desarrollo](../archive/s4.3-proveedor-identidad-desarrollo/spec.md): contratos de Application existentes.
- [S5.7 — Proveedor de identidad de desarrollo](../archive/s5.7-proveedor-identidad-desarrollo/spec.md): proveedor concreto, resolución y adaptación al contexto privado, aceptados para su alcance.
- [S7.1 — Seguridad web básica](../s7.1-seguridad-web/spec.md) y [S7.2 — Cabeceras de seguridad web](../s7.2-cabeceras-seguridad/spec.md): protecciones web previas aceptadas.

## Alcance

- Incluye:
  - Verificar e integrar el límite Web → Application para que una petición bajo `/app` use el `ICurrentUser` configurado a partir del proveedor de identidad de desarrollo antes de invocar un caso de uso.
  - Comprobar mediante integración HTTP el valor de identidad recibido por el componente de Application invocado por la petición.
  - Comprobar que valores de identidad manipulados por el cliente no sustituyen al valor resuelto por el proveedor.
- Excluye:
  - Cambiar el contrato o la implementación del proveedor de identidad, el aprovisionamiento de la cuenta o el orden de inicialización ya entregados por S5.7.
  - Implementar autenticación real, cookies, LoginV0, nuevas reglas de negocio, páginas de gestión, dashboard funcional o análisis de parámetros privados.
  - La suite amplia de seguridad HTTP de S7.9, la accesibilidad y el despliegue.
- Trabajo vecino y responsable de sus entregables: S7.6 implementa el análisis de parámetros privados, S7.7 las rutas y el dashboard, y S7.9 las pruebas de integración HTTP amplias; esta especificación valida de manera focalizada el paso de identidad hacia Application sin depender de esos trabajos.

## Dependencias y prerrequisitos

- S4.2 y S4.3 están completadas y aceptadas; Application define `ICurrentUser` y `IDevelopmentIdentityProvider`.
- S5.7 está completada, aceptada y archivada; el proveedor concreto y la resolución del usuario para `/app` están disponibles. Esta especificación no vuelve a implementar ni a aceptar esos entregables.
- S7.1 y S7.2 están completadas y aceptadas; se reutiliza el host Web existente.
- La comprobación debe poder ejecutarse con el host de tests disponible y una identidad de desarrollo controlada. Si no puede observarse el paso de identidad a Application sin una ruta funcional futura, el plan deberá determinar un punto de observación de pruebas que no añada rutas productivas ni espere a S7.7.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Autenticación»](../../context/architecture.md#autenticación), [S4.2 → CA03](../archive/s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación), [S4.3 → CA02–CA04](../archive/s4.3-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) y [S5.7 → CA02 y CA04](../archive/s5.7-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) | Una petición HTTP dirigida a una ruta bajo `/app`, un proveedor controlado y un consumidor de Application invocado dentro del pipeline Web. | La petición llega al consumidor después de la resolución de identidad. | El consumidor de Application recibe el `UserId` proporcionado por el proveedor mediante el `ICurrentUser` scoped; la comprobación no depende de que S7.7 implemente una página funcional. |
| CA02 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [S5.7 → CA04](../archive/s5.7-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) | La misma petición bajo `/app` incluye datos de identidad manipulados por el cliente, y el proveedor devuelve una identidad de control distinta. | Se ejecuta el consumidor de Application. | El `UserId` recibido por Application sigue siendo el resuelto por el proveedor; ningún valor de la petición altera la identidad usada. |
| CA03 | [architecture.md → «Autenticación»](../../context/architecture.md#autenticación) y [«Enrutamiento»](../../context/architecture.md#enrutamiento) | Una petición dirigida a una ruta ajena a `/app` y un proveedor de prueba que permite detectar si se invoca. | La petición atraviesa el pipeline Web. | La integración de identidad privada no invoca el proveedor ni establece identidad privada para esa ruta. |

Los criterios verifican el enlace Web–Application. No vuelven a demostrar la búsqueda de cuenta, el fallo de arranque ni las garantías del proveedor ya aceptadas en S5.7.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar esta especificación. El plan debe concretar el punto de observación en el host de tests sin añadir rutas productivas ni depender de páginas futuras. | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) y [S5.7 → CA04](../archive/s5.7-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) | CA01–CA03 | Revisar y aprobar la estrategia técnica en `plan.md`. |

## Artefactos relacionados

- [Plan técnico](plan.md), propuesta pendiente de aprobación.
