# Plan técnico: S4.7 — Casos de uso de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan: aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Casos de uso de Application.** Añadir servicios de aplicación por operación o responsabilidad cohesiva para crear, editar, mover, cambiar visibilidad, eliminar y listar enlaces. Seguir el patrón `I*Service` → `*Service` de [architecture.md → «Application»](../../context/architecture.md#application) y ubicar cada tipo público en su archivo y carpeta de responsabilidad según [AGENTS.md → «Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código). La granularidad final se propone en tareas tras revisar el precedente S4.6 y el inventario actual.
- **Contratos disponibles.** El inventario inicial encuentra `ICurrentUser`, `ILinkRepository`, `ICollectionRepository`, `IUnitOfWorkFactory`, `IUnitOfWork`, `CreateLinkRequest`, `LinkCreateValidator`, `LinkDto` y los contratos comunes `Result`/`ApplicationError`. `ILinkRepository` ofrece lecturas por Id y URL normalizada, listados, página/recuento, y operaciones `Insert`, `Update` y `Delete`; `ICollectionRepository` permite recuperar una colección por Id y usuario. Verificar firmas completas y uso en las tareas antes de codificar.
- **Carencias por contrastar, no decisiones tomadas.** En la inspección inicial no se encontraron DTOs/validadores de actualización, movimiento, visibilidad o listado equivalentes a `CreateLinkRequest`/`LinkCreateValidator`. Tampoco aparece en `ILinkRepository` una operación explícita para obtener el enlace duplicado junto a su colección. T02 debe comprobar si los contratos existentes permiten cumplir los criterios; cualquier ampliación se somete como propuesta justificada y no se da por aprobada por este plan.
- **Dominio.** Reutilizar `Link.Create`, las operaciones de dominio disponibles en `Link` y las policies existentes para ajuste, validación y normalización de URL e imagen. Capturar las excepciones de dominio en la frontera de Application y mapear sus códigos mediante los mecanismos compartidos. No duplicar transformaciones ni invariantes en el servicio.
- **Duplicados.** Al crear, calcular/obtener el valor normalizado usando Domain y buscar coincidencia limitada al usuario. Para informar de la colección coincidente, inspeccionar qué resultado y datos permiten los contratos; proponer solo la ampliación mínima que sea necesaria para cumplir AC-LINK-01. La restricción única de persistencia sigue siendo la garantía final frente a concurrencia; el tratamiento técnico de un conflicto concurrente se validará en la etapa propietaria de persistencia, sin inventar aquí un resultado funcional.
- **Errores.** Respetar categorías y códigos de los contratos existentes y [architecture.md → «Application»](../../context/architecture.md#application). Validar primero el formato del identificador; un ID inválido produce `Validation`. En eliminación, un ID válido inexistente o ajeno usa `ResourceNotAccessible` con el código estable de la operación, común a ambas causas. Los resultados de Application no transportan mensajes HTTP, detalles SQL ni explicación que revele la causa de un recurso válido no accesible.
- **Listado.** Delegar al repositorio la consulta paginada de enlaces con búsqueda y filtro de colección, con `LinkDto` y total de páginas. Revisar qué validación y normalización de parámetros corresponde a Web y cuál recibe Application a partir de S4.6 y de las firmas actuales; no trasladar al servicio el parseo de la query string.

### Flujos internos

1. Cada operación privada obtiene `UserId` de `ICurrentUser`; no acepta un identificador de usuario enviado por el cliente como autoridad.
2. Crear: validar la solicitud con los contratos disponibles, obtener la colección dentro del ámbito del usuario, crear el enlace mediante Domain, comprobar duplicados y persistirlo; si se solicita visibilidad pública, respetar las condiciones del criterio aprobado. Si el resultado funcional es rechazo, no dejar cambios parciales.
3. Editar: cargar el enlace por Id y usuario, validar campos permitidos, comprobar la colección necesaria para la regla de visibilidad, aplicar las operaciones de Domain y persistir solo cuando todas las comprobaciones hayan pasado. La URL base se conserva inmutable.
4. Mover: cargar el enlace y la colección destino dentro del ámbito del usuario, hacer que Domain aplique la transición de movimiento y guardar el enlace. Un destino no accesible o una regla infringida produce un resultado de error sin persistir cambios.
5. Cambiar visibilidad: cargar el enlace y la colección actual con comprobación de propiedad, aplicar la transición de Domain y persistir solo cuando la regla de publicación se cumpla.
6. Eliminar: validar el identificador; si es válido, recuperar el enlace bajo el usuario identificado y eliminarlo mediante el repositorio. Un identificador inválido produce `Validation`; un ID válido inexistente o ajeno comparte la categoría `ResourceNotAccessible` y el código estable aprobados.
7. Listar: solicitar al repositorio página y recuento usando `UserId`, búsqueda y filtro aplicables; devolver el modelo de listado existente o propuesto tras verificar contratos. El orden, relevancia, paginación y tratamiento de parámetros obedecen a las fuentes enlazadas en spec.md.
8. Las escrituras que necesitan coordinación usan la unidad de trabajo existente: begin, operaciones mínimas, commit en éxito, rollback en rechazo o excepción y liberación de recursos en todos los caminos. No realizar llamadas externas ni esperas dentro de la transacción.

Las tareas fijarán la secuencia según firmas reales y pruebas existentes. Una insuficiencia o contradicción funcional bloquea el criterio afectado y se resuelve primero en su documento propietario.

### Persistencia y dependencias

- Application depende solo de Domain conforme a [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos). No añadir paquetes ni referencias a Infrastructure, Web, SQLite o Dapper.
- Usar los contratos de repositorios y Unit of Work; no implementar repositorios concretos ni SQL. S5/S6 son responsables de persistencia, restricciones y proyecciones FTS5.
- La atomicidad física de escrituras y la resolución de colisiones concurrentes dependen del Unit of Work y de las restricciones únicas en SQLite; se comprobarán en la etapa de persistencia. Application debe dejar sus operaciones preparadas para utilizar esos contratos sin afirmar que ya existe persistencia concreta.
- No se requieren servicios externos. Fakes y tests de comportamiento de consumidores se asignan a S4.9 y el registro de eventos a S4.10.

### Garantías técnicas

- Cada lectura y escritura se limita al usuario de `ICurrentUser`; se verifica la propiedad del enlace, de la colección y de las relaciones aplicables según [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- Domain conserva las reglas de URL, duplicados representados por `UrlNormalized`, campos editables y privacidad según [domain-model.md → «Link»](../../context/domain-model.md#link), [«Público y privado»](../../context/domain-model.md#público-y-privado) y [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces).
- La creación, edición, movimiento, cambio de visibilidad y eliminación no dejan mutaciones parciales cuando la operación falla; se usa el Unit of Work disponible para coordinar escrituras.
- Los casos de uso no acoplan Application a Infrastructure o Web, ni incorporan mensajes de presentación o detalles persistentes a sus resultados.
- La consulta de listado devuelve solo enlaces del usuario y usa la proyección/consulta de enlaces que implemente el repositorio propietario; Application no consulta directamente una base de datos ni otra entidad de búsqueda.
- Revisión estructural y compilación no demuestran comportamiento; la evidencia conductual de consumidores se atribuye a S4.9.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener los casos de uso en Application y depender de Domain y contratos de Application | Confirmado por arquitectura y S4.1–S4.3 | [architecture.md → «Application»](../../context/architecture.md#application), specs S4.1–S4.3 | No añadir referencias a Infrastructure, Web o persistencia concreta. |
| Organizar servicios por operación o grupo cohesivo y tipos públicos en archivos independientes | Propuesta técnica | [AGENTS.md → «Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código), precedente S4.6 | Concretar granularidad al desglosar tareas y revisar el patrón existente. |
| Ampliar contratos para datos de duplicado/colección o solicitudes de operaciones que aún no aparecen en el inventario inicial | Pendiente de inspección | Contratos actuales de Application; AC-LINK-01–AC-LINK-06 | Confirmar si hay una carencia real. Si se requiere, registrar alcance mínimo y compatibilidad en tareas y esperar decisión antes de editar contratos. |
| La restricción única de persistencia es la garantía final para duplicados concurrentes | Confirmado por arquitectura | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia), [domain-model.md → «Restricciones de unicidad»](../../context/domain-model.md#restricciones-de-unicidad) | La conversión técnica del conflicto a resultado de Application requiere comprobar los contratos y la implementación propietaria de persistencia; no inventar un error nuevo. |
| Fakes y pruebas de consumidores corresponden a S4.9; eventos a S4.10 | Confirmado por plan MVP0 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | Los criterios funcionales que dependan de esos tests mantienen su evidencia pendiente hasta el trabajo correspondiente. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA09 | Técnica: inventario de contratos, estructura y límites de capa | Tareas aprobadas; cambios de Application implementados | Revisar los contratos usados, los archivos/tipos públicos y las referencias de `Linkubator.Application.csproj`; ejecutar `ProjectReferenceMatrixTests` existente | Compatibilidad de contratos confirmada o carencias documentadas; no se incorporan dependencias de capa prohibidas | Contratos y archivos revisados, test/resultado, SDK y fecha. No acredita comportamiento funcional. |
| V02 | CA01–CA08 | Técnica: revisión de correspondencia con criterios y fuentes | Operaciones implementadas; catálogo aprobado | Revisar crear, editar, mover, visibilidad, borrar y listar contra AC-LINK-01, AC-LINK-01b y AC-LINK-02–AC-LINK-06; verificar identidad, propiedad, Unit of Work, errores y límites de capa | Trazabilidad de cada caso y sus rechazos; hallazgos funcionales o de contrato quedan bloqueados y no se improvisan | Matriz criterio → servicio/tipo/flujo revisado y hallazgos con resolución. No reemplaza tests. |
| V03 | CA01–CA08 | Automatizada: pruebas de comportamiento del consumidor | Fakes y tests implementados por S4.9, además de S4.7 disponible | Ejecutar los tests de Application S4.9 asociados a enlaces una vez que existan; el filtro/comando concreto se descubrirá en tareas al inspeccionar el runner y nombres de tests | Escenarios felices y negativos de los criterios cubiertos pasan; un criterio sin test/evidencia permanece pendiente | Comando/filtro real, nombres de tests, SDK, resultado, fecha y enlace a evidencia S4.9. No se atribuye esta prueba a S4.7. |
| V04 | CA01–CA08 | Automatizada: compilación integrada | SDK .NET 10 y dependencias restauradas | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Solución compila; errores y warnings se registran y corrigen según alcance | Comando, SDK, exit code y resumen. No acredita criterios funcionales. |
| V05 | CA01–CA08 | Automatizada: suite disponible y revisión de diff | Validaciones anteriores; cambios integrados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal`; `git diff --check` | Suite disponible pasa y no hay errores de whitespace; pruebas S4.9 aún no disponibles se informan como pendientes, no como éxito | Comandos, SDK, exit code, resumen y alcance real de tests ejecutados. La suite verde no equivale a aceptación humana. |

V03 es evidencia conductual de S4.9, no un entregable adelantado de este trabajo. Si S4.9 no está disponible, no se declara satisfecho por inferencia ningún criterio que exija comportamiento comprobado. Persistencia concreta, concurrencia y restricciones de base de datos no quedan probadas por tests con fakes ni por el build y se validan en sus etapas propietarias.

## Orden de ejecución y cierre

- Tras la aprobación de este plan, crear `tasks.md` desde la plantilla, con tareas para inventario de contratos, cada operación, checks inmediatos, integración y evidencia pendiente de S4.9.
- Antes de modificar contratos, confirmar en tareas la carencia que impide cumplir un criterio, proponer el cambio mínimo y resolver la decisión según el proceso aprobado.
- Todos los criterios deben tener tarea y validación vinculadas; la aprobación de las tareas será necesaria antes de cualquier cambio productivo.
- La aceptación de implementación requiere evidencia real por criterio y revisión humana; plan aprobado, compilación o tests verdes no equivalen a aceptación.
- Ante una contradicción de fuentes, detener el criterio afectado y actualizar primero el documento propietario con aprobación humana.