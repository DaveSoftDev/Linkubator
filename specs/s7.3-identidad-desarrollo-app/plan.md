# Plan técnico: S7.3 — Identidad de desarrollo en `/app`

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 22-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Reutilizar el middleware Web de identidad de desarrollo, `IDevelopmentIdentityProvider` y el `ICurrentUser` scoped ya entregados y aceptados en S5.7. No modificar contratos de Application, el proveedor de Infrastructure, la inicialización ni el aprovisionamiento.
- Añadir pruebas al proyecto de integración existente usando `WebApplicationFactory<Program>` y su sustitución de servicios de test. La cobertura debe observar la identidad en un servicio/caso de uso de Application invocado por una petición bajo `/app`, no inferirla solo inspeccionando el middleware o su objeto de contexto.
- Mantener la prueba de extremo a extremo dentro del host de test. Si no existe una página productiva apropiada, introducir un endpoint de prueba únicamente en la configuración del host de pruebas que invoque un consumidor real de Application; no añadir rutas productivas ni esperar a S7.7.
- Para el caso no privado, observar la ruta pública existente y sustituir el proveedor por uno que falle si se invoca. Los endpoints productivos públicos no deben depender de identidad privada.

### Flujos internos

1. `WebApplicationFactory` configura una identidad determinista sustituyendo el servicio de proveedor, conservando el middleware y el resto del pipeline de producción.
2. Una petición de test a una ruta bajo `/app` pasa por el middleware; este espera la resolución del proveedor y fija la identidad scoped antes de que el endpoint de prueba llame al consumidor real de Application.
3. El consumidor informa su resultado de manera observable por la respuesta HTTP o por un doble de frontera de prueba que reciba el `ICurrentUser` de Application; se comprueba que el valor concuerda con el proveedor.
4. Se repite la petición incluyendo un `UserId` manipulado en query y/o formulario según el mecanismo que el endpoint de test permita; la identidad observada en Application debe permanecer sin cambios.
5. Se realiza una petición a una ruta pública estable con el proveedor de fallo y se comprueba que la ruta funciona sin invocarlo.

### Persistencia y dependencias

No se cambian persistencia, contratos ni paquetes. Se reutilizan `WebApplicationFactory<Program>` y el proyecto de tests existentes. La validación se limita al paso de identidad a Application; no vuelve a probar las consultas SQLite, el fallo de arranque o las garantías del proveedor ya cerradas en S5.7. S7.9 mantiene la responsabilidad de consolidar la suite de integración HTTP de la etapa, sin ser prerrequisito para cerrar esta spec.

### Garantías técnicas

- La identidad de la petición privada es establecida por el proveedor en el contexto scoped antes de ejecutar Application, conforme a [architecture.md → «Autenticación»](../../context/architecture.md#autenticación).
- Los valores controlados por el cliente no eligen el `UserId`, según [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas).
- La ruta pública no requiere ni invoca la resolución de identidad privada.
- La comprobación negativa demuestra sensibilidad: falla si se elimina la inicialización de identidad antes de la llamada a Application o si el consumidor recibe el valor del cliente en lugar del proveedor.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el middleware y contratos aceptados en S5.7, sin reabrir su implementación | Confirmada por fuentes y trabajo aceptado | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos), [S5.7 → CA02–CA05](../archive/s5.7-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) | Mantiene el trabajo acotado a integrar y verificar la frontera de la capa Web. |
| Observar Application mediante configuración del host de pruebas, sin rutas nuevas productivas | Propuesta | [spec.md → CA01–CA03](spec.md#criterios-de-aceptación) | El plan de tests debe demostrar una llamada al componente de Application y permanecer independiente de las rutas de S7.7. |
| S7.9 aporta cobertura amplia HTTP, no un prerrequisito de cierre de S7.3 | Confirmada en el plan de implementación | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | S7.3 conserva sus pruebas focalizadas y puede cerrarse por su evidencia propia. |
| El endpoint de pruebas debe invocar un consumidor real de Application sin alterar producción | Propuesta | [spec.md → CA01](spec.md#criterios-de-aceptación) | Durante la aprobación de tareas se identificará el servicio de Application disponible y el modo seguro de observar su identidad. Si el test no atraviesa efectivamente Application, CA01 no se considerará demostrado. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Integración HTTP focalizada: una petición a `/app` llega a Application a través del host Web y observa el `UserId` del proveedor controlado | `WebApplicationFactory<Program>`; servicio/endpoint de prueba incorporado solo al host de tests | Ejecutar los tests de integración nuevos de identidad; detectar en T01 cuál es el filtro focalizado exacto. Verificar que el componente real de Application recibió el `ICurrentUser` antes de continuar | La respuesta o captura de frontera de prueba acredita la ejecución de Application con el ID del proveedor, y no solo una mutación del contexto Web | Nombre del test, comando y filtro, status/respuesta observada, proveedor configurado y valor recibido por Application, fecha/entorno |
| V02 | CA02 | Integración HTTP negativa de identidad manipulada | V01 implementada; endpoint de prueba permite adjuntar datos cliente no autoritativos | Enviar una segunda petición equivalente con el valor de identidad manipulado; comprobar que el consumidor de Application sigue observando el valor del proveedor. Como prueba de sensibilidad temporal, cambiar en el test el valor esperado al valor manipulado y confirmar que la aserción falla; restaurar el test | La identidad observada coincide solo con el proveedor; la prueba falla frente a la variante deliberadamente vulnerable y vuelve a pasar tras restaurarla | Casos de manipulación comprobados y salida de la perturbación temporal revertida |
| V03 | CA03 | Integración HTTP negativa de alcance | `WebApplicationFactory<Program>` y ruta pública existente | Sustituir proveedor por implementación de prueba que lanza al invocarse; solicitar la ruta pública y verificar respuesta funcional y cero invocaciones | La ruta pública sigue funcionando sin resolver identidad privada | Nombre de prueba, proveedor instalado, contador de invocaciones y respuesta HTTP |
| V04 | CA01–CA03 | Revisión técnica y validación integrada | Cambios de test completados | Ejecutar el filtro focalizado identificado en V01; revisar los cambios y `git diff --check` | Las comprobaciones cubren criterios sin alterar producción ni depender de S7.7/S7.9; diff sin errores | Comandos, resultados, archivos revisados, fecha y alcance de la evidencia |

V01 demuestra la integración con el componente de Application que se invoque, no el comportamiento funcional de páginas privadas futuras. V02 demuestra resistencia frente a los valores de identidad manipulados probados, no reemplaza la validación de propiedad de cada recurso. V03 solo comprueba que la ruta pública usada no requiere el proveedor.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` con tareas para localizar el harness y consumidor de Application, añadir las pruebas del host, ejecutar V01–V04 y registrar la evidencia. No iniciar cambios productivos antes de la aprobación de tareas.
- Los tres criterios se validan dentro de esta spec. La implementación futura de las rutas privadas y las pruebas amplias de S7.9 no bloquean su cierre.
- Cerrar cuando CA01–CA03 tengan evidencia satisfactoria, las tareas propias estén completadas y la aceptación humana esté registrada en las tareas.
