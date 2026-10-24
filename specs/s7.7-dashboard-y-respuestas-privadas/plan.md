# Plan técnico: S7.7 — Dashboard y respuestas privadas

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 23-X-2026 («Siguiente»).
  - Plan: DLG, aprobado el 23-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Mantener el dashboard y sus datos de presentación en Razor Pages Web. Invocar el caso de uso de dashboard existente y presentar sus resultados; no duplicar su lógica de Application ni agregar datos ficticios al flujo productivo.
- Completar la presentación del dashboard para que su búsqueda use el componente de formulario de S7.5 y sus enlaces de destino. Ofrecer accesos solo a operaciones disponibles dentro del alcance aprobado; no crear gestión de etiquetas ni anticipar las páginas de enlaces/colecciones de sprints posteriores.
- Conservar la resolución de identidad de `/app` proporcionada por S7.3. No aceptar `UserId` desde query, formulario o ruta.
- Centralizar en el límite Web la conversión del resultado de recurso no accesible a la presentación genérica descrita en [spec.md](spec.md), reutilizando el componente de aviso de S7.5. La presentación no debe exponer código interno, identificador, existencia ni propietario. La forma concreta de integración se confirmará contra los contratos existentes al implementar; no introducir una abstracción transversal si las páginas consumidoras actuales no la necesitan.
- Asegurar el tratamiento `404` genérico para métodos no permitidos en las páginas privadas existentes, manteniendo la diferencia entre una respuesta de método no admitido y los errores de validación funcionales ya definidos por las fuentes.

### Flujos internos

1. La petición privada entra al pipeline Web y la identidad es resuelta por el mecanismo existente de S7.3.
2. Razor Pages sirve el dashboard dentro del layout privado existente. Su PageModel consume el caso de uso de dashboard; la vista muestra los accesos y el formulario de búsqueda, sin realizar consultas por sí misma.
3. El formulario de búsqueda se representa con el componente de S7.5 y envía los datos al listado de enlaces. La interpretación de parámetros sigue siendo responsabilidad de S7.6 y no forma parte de este plan.
4. Cuando un PageModel recibe un resultado Application de recurso no accesible, la frontera Web selecciona el aviso común y el destino de navegación permitido por la fuente, sin emitir detalles del recurso. Como no hay aún páginas de gestión consumidoras, el comportamiento se observará en un punto de prueba añadido solo al host de tests; no se añadirán rutas productivas de gestión.
5. Una solicitud a una ruta existente con método no declarado debe producir la respuesta genérica `404` configurada para páginas privadas; comprobarlo mediante el host Web y comparar la respuesta pública, no el estado interno del recurso.

### Persistencia y dependencias

No se modifica persistencia, SQL ni transacciones. Se reutilizan el caso de uso y las dependencias Web ya registradas. S7.6, S7.8 y las páginas de gestión de etapas posteriores no son prerrequisitos de cierre. La validación de la salida por recurso no accesible usará dependencias controladas en el harness existente y no registrará ni alterará recursos reales.

### Garantías técnicas

- Toda operación privada conserva la identidad resuelta por el servidor y no confía en identificadores de propietario enviados por el cliente ([AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas), [architecture.md → «Autenticación»](../../context/architecture.md#autenticación)).
- El dashboard y los avisos mantienen el layout privado y la semántica accesible ya proporcionados por S7.4 y S7.5 ([requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad)).
- Los errores de recurso válido no accesible no permiten inferir existencia o propiedad; se muestra únicamente el resultado genérico de la fuente ([specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos)).
- No se crean enlaces funcionales a páginas futuras ni se implementa lógica de búsqueda o gestión fuera del alcance de S7.7.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el dashboard, caso de uso y layout existentes en lugar de reconstruirlos | Confirmado | [spec.md](spec.md), [architecture.md → «Web»](../../context/architecture.md#web) | Cambios acotados a la integración y presentación; verificar que la vista refleja datos del caso de uso y no datos estáticos de demostración. |
| Mantener la ruta de entrada actual de `/app` si su resultado visible cumple el criterio, sin reabrir en este trabajo el desacuerdo entre ruta y redirección indicado en la fuente | Propuesta | CA01 de [spec.md](spec.md) y estado actual de la página Web | Testificar el acceso observable al dashboard; cualquier cambio de URL que afecte a rutas canónicas debe detenerse y elevarse como decisión funcional, no decidirse desde el plan técnico. |
| Usar un endpoint exclusivo del host de tests para inyectar resultado de recurso no accesible mientras no exista una ruta productiva consumidora | Propuesta | CA04–CA05 de [spec.md](spec.md) y guía de [sdd-writer](../../skills/sdd-writer/SKILL.md) | Permite cerrar la evidencia de S7.7 sin crear páginas futuras ni exponer una ruta de prueba en producción; revisar que el punto de prueba se registra solo en el host de tests. |
| Las páginas actuales aún no contienen gestión funcional completa ni listados de enlaces/colecciones | Confirmado | [roadmap.md → «Now»](../../context/roadmap.md#now) y etapas posteriores de [mvp0-plan.md](../../plans/mvp0-plan.md) | Los accesos a funciones no disponibles deben omitirse o limitarse a elementos ya funcionales; no usar placeholders que aparenten una operación ya implementada. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Test automatizado de integración HTTP del host Web y revisión de HTML del dashboard | Factory Web con base temporal; dependencias de Application configuradas; identity provider controlado | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "Category=Web"`; añadir/ajustar tests focalizados que soliciten `/app`, comprueben dashboard renderizado, búsqueda GET y destinos disponibles, sin enviar `UserId` | El dashboard se sirve con la identidad resuelta por servidor; búsqueda apunta al listado de enlaces y no se muestran acciones de etiquetas ni flujos inexistentes | Nombre/filtro del test, petición, estado y fragmentos HTML relevantes, resultado y entorno. Comprobar también que añadir `?userId=...` no cambia la identidad consumida. |
| V02 | CA03 | Test automatizado de integración HTTP, incluido caso negativo | Ruta privada existente en el host y cliente configurado para no seguir redirecciones automáticamente | Enviar a una ruta existente un método no permitido, compararlo con una ruta inexistente y revisar estado y cuerpo; ejecutar la prueba focalizada mediante `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "Category=Web"` | Ambas respuestas corresponden al `404` genérico de la fuente y no revelan información de recurso; el test debe fallar ante método permitido por error o respuesta diferenciadora | Ruta y método probados, estado/cuerpo de ambas respuestas, test/filtro, resultado y evidencia de sensibilidad cuando se haga perturbación controlada y restaurada. |
| V03 | CA04, CA05 | Test automatizado de integración HTTP y revisión de respuesta/HTML | Punto de prueba disponible únicamente en el host de tests; resultado Application de recurso no accesible controlado | Invocar el punto de prueba con el resultado controlado; contrastar ausencia/presencia del recurso de prueba si el fixture lo permite; revisar texto, HTML y que no aparezcan ID, código interno o detalle de propiedad | Se presenta el aviso común accesible y la respuesta observada no revela la causa interna; no se crea endpoint de producción | Nombre del test, configuración del punto de prueba, respuesta/HTML inspeccionado, assertions, comando y resultado. Registrar cualquier diferencia entre respuestas. |
| V04 | CA01–CA05 | Revisión técnica integrada y build | Cambios de implementación completados y V01–V03 ejecutados | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "Category=Web"`; `git diff --check`; revisar diff de rutas y que el endpoint de prueba no esté en producción | Build satisfactoria; tests focalizados verdes; diff limpio; cobertura de todos los criterios sin alterar fuentes funcionales ni depender de tareas futuras | Comandos, resultados, archivos y rutas revisadas, fecha, límites de la evidencia y cualquier validación pendiente. |

Los tests del HTML no prueban por sí mismos una experiencia completa con lector de pantalla; la revisión manual amplia de teclado, contraste y encabezados pertenece a S7.10–S7.11 y no bloquea el cierre de S7.7. Los tests de respuestas solo acreditan los casos cubiertos por el host de pruebas, no todas las futuras páginas de gestión. La suite integrada transversal de S7.9 queda como trabajo independiente.

## Orden de ejecución y cierre

- [Tareas](tasks.md): propuestas; aprobación pendiente. Las tareas cubren el dashboard y la entrada privada, los métodos no permitidos, el aviso genérico mediante una ruta de prueba exclusiva de tests y la validación integrada.
- Cada tarea asigna un check inmediato a los criterios correspondientes. La inclusión de páginas de gestión, filtros, logs o suite transversal de etapas vecinas no será condición de cierre.
- El cierre exige evidencia satisfactoria para CA01–CA05 y aceptación humana registrada en `tasks.md`; este plan no registra implementación ni aceptación del resultado.