# Plan técnico: S4.2 — Contratos de repositorios, Unit of Work e identidad identificada

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Planifica»).
	- Plan: aprobado por DLG el 15-X-2026 («Adelante»).

## Diseño de implementación

### Componentes y contratos

- **Contratos de repositorio.** Añadir a Application interfaces por responsabilidad de acceso a colecciones y enlaces que cubran las operaciones del catálogo S4: escrituras privadas, listados y lecturas públicas. Sus firmas usarán entidades de Domain y los DTOs de Application aprobados en S4.1; no expondrán tipos de SQLite, Dapper, Infrastructure ni Web. La matriz operación → repositorio → datos necesarios se cerrará en las tareas después de aprobar este plan y contrastarla con el catálogo S4.1 y con los criterios funcionales de S4.4 disponibles; no se agregarán operaciones de otras etapas.
- **Unit of Work.** Definir en Application un contrato de coordinación de persistencia para las escrituras que requieran atomicidad, con responsabilidades acotadas a iniciar/completar o revertir la unidad de trabajo y liberar sus recursos. La forma final de sus métodos y su compatibilidad con el ciclo de vida de los repositorios se concretará en las tareas sin filtrar objetos de transacción de la infraestructura.
- **Usuario identificado.** Definir una abstracción consumible por Application que exponga la identidad efectiva de la ejecución. Los casos privados la usarán como fuente de identidad; no la tomarán de parámetros de solicitud ni de DTOs. El contrato concreto que resuelve esa identidad de desarrollo queda en S4.3.
- **Lecturas públicas.** Los contratos destinados a consultas públicas se distinguirán de los accesos privados; las rutas y claves públicas solo localizarán datos públicos y no se tratarán como autorización para consultar datos privados.
- **Ubicación.** Organizar cada tipo público en su propio archivo y carpeta de responsabilidad bajo Application, siguiendo la estructura existente y [AGENTS.md → «Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código).
- Las firmas, granularidad y nombres descritos son propuestas técnicas. No alteran las reglas funcionales y requieren aprobación de este plan.

### Flujos internos

1. El punto de entrada de Web obtiene la identidad mediante el mecanismo vigente y la proporciona a la abstracción de usuario identificado, no como un campo autoritativo de la solicitud.
2. Un caso de uso obtiene la identidad de esa abstracción cuando es una operación privada; una lectura pública sigue el contrato público correspondiente.
3. El caso de uso coordina repositorios a través de los contratos de Application. Para una escritura compuesta, delimita la unidad de trabajo y termina confirmándola o revirtiéndola según el resultado.
4. La implementación de Infrastructure resuelve posteriormente las interfaces con SQLite y Dapper; Application no conoce esa implementación.
5. La resolución concreta de la identidad de desarrollo se incorpora por S4.3, sin acoplarla a los repositorios ni a los DTOs.

### Persistencia y dependencias

- No se implementan en este trabajo repositorios concretos, conexión, transacción, SQL, esquema ni scripts. La implementación correspondiente pertenece a Infrastructure y a los bloques posteriores del roadmap.
- Application conserva exclusivamente las referencias de proyecto permitidas por [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos); no se añaden paquetes.
- Los repositorios y el Unit of Work colaboran para permitir que las escrituras compuestas sean atómicas, siguiendo [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia). No se incorporan esperas, llamadas HTTP, scraping ni envío de correo dentro de una transacción.
- S4.3 es propietario del proveedor de identidad de desarrollo. La conexión del proveedor con la abstracción se configura en el composition root cuando se implemente esa etapa; S4.2 no define su mecanismo de provisión.

### Garantías técnicas

- La matriz de referencias de proyectos permanece conforme a [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos).
- Las operaciones privadas usan la identidad identificada y mantienen la propiedad de recursos y relaciones según [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos); ningún identificador público suministrado por el cliente concede acceso privado.
- Las lecturas públicas permanecen limitadas a datos públicos según [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado).
- Las transacciones se mantienen breves y limitadas al trabajo de persistencia según [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia).
- Los contratos se mantienen consumibles por los DTOs y resultados aprobados de S4.1, sin exponer detalles de presentación o persistencia.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ubicar interfaces de repositorio, Unit of Work e identidad identificada en Application | Confirmada por [architecture.md → «Application»](../../../context/architecture.md#application) | Arquitectura aprobada | Mantiene el flujo de dependencias; validar referencias con el test de arquitectura. |
| Separar los contratos de acceso privado de las consultas públicas | Propuesta | CA03–CA04 de [spec.md](spec.md#criterios-de-aceptación) | Evita usar identificadores de rutas públicas como autoridad; aprobar forma y granularidad en este plan. |
| Usar un Unit of Work explícito como frontera de las escrituras compuestas | Propuesta derivada de [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | Contrato de Application y persistencia futura deben compartir un ciclo de vida coherente; definir API concreta tras contrastar los consumidores. |
| Definir granularidad y operaciones de los repositorios a partir del catálogo completo S4 y de S4.1 | Pendiente de inventario de tareas | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y S4.1 | La matriz debe cubrir todas las operaciones previstas sin incorporar capacidades posteriores; cualquier regla ambigua queda para S4.4 o su fuente propietaria. |
| Application consume la abstracción de identidad, sin implementar el proveedor de desarrollo | Confirmada en alcance por CA06; la forma concreta es propuesta | [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo) y S4.3 | Mantiene separado el origen de identidad; la integración concreta requiere el trabajo S4.3. |
| Dependencia efectiva de contratos S4.1 | Pendiente de comprobación de aceptación e implementación | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Antes de cerrar el diseño debe comprobarse la aceptación real de S4.1 y contrastar sus DTOs/resultados disponibles; si hay divergencia se bloquea el diseño afectado, sin asumirla resuelta. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada y técnica: matriz de dependencias | Contratos implementados y solución restaurada | Ejecutar `ProjectReferenceMatrixTests` y revisar el `.csproj` de Application | Application conserva solo las referencias permitidas y los contratos no incorporan referencias de Infrastructure o Web | Filtro/comando ejecutado, SDK y resultado del test; referencias revisadas. |
| V02 | CA02, CA07 | Revisión técnica: inventario operación → contrato | Catálogo de S4 y contratos S4.1 aceptados; criterios S4.4 consultables cuando estén disponibles | Contrastar cada operación MVP0 de S4 con la matriz de repositorios y sus parámetros/resultados; revisar que solo use tipos de Domain y Application | Cada operación dentro del alcance tiene acceso a datos contractual suficiente, sin omisiones ni funciones fuera de alcance | Matriz final, fuentes contrastadas y excepciones justificadas. |
| V03 | CA03, CA06 | Automatizada y técnica: contratos de identidad y propiedad | Contratos de Application implementados | Inspeccionar interfaces públicas y DTOs; comprobar que la identidad efectiva se expone por `ICurrentUser`, que las firmas privadas reciben el ámbito de propietario y que ningún DTO de cliente porta `UserId` autoritativo | La abstracción de identidad está separada del transporte y las operaciones privadas ofrecen ámbito explícito de usuario | Firmas, DTOs revisados y tests de contratos disponibles. Los ataques mediante casos de uso quedan para la etapa consumidora. |
| V04 | CA04 | Técnica: separación contractual público/privado | Contratos de consulta implementados | Revisar que los métodos de acceso privado exigen `userId` y que las lecturas públicas se expresan por separado y devuelven DTOs limitados al alcance público | Las firmas distinguen acceso privado de lecturas públicas y estas últimas no exponen entidades de persistencia ni identidad privada | Firmas revisadas y matriz consulta → alcance. La prueba de datos privados en ejecución queda para Infrastructure/consumidores. |
| V05 | CA05 | Técnica: ciclo contractual de unidad de trabajo | Contrato de Unit of Work implementado | Revisar el contrato frente a Persistencia: `Begin`, `Commit`, `Rollback`, disposición síncrona/asíncrona y resolución de repositorios sin exponer conexión/transacción ni trabajo externo | El ciclo contractual es explícito y acotado; el comportamiento de confirmación/reversión se prueba cuando exista implementación/consumidor | Firmas revisadas; no requiere fakes de casos de uso en S4.2. |
| V06 | CA01–CA07 | Automatizada: sensibilidad de la protección arquitectónica | Tests arquitectónicos disponibles y cambios de contratos realizados | Confirmar que `ProjectReferenceMatrixTests` detecta una referencia no permitida en una copia temporal aislada o mediante el test arquitectónico existente, sin dejar cambios en el repositorio | La validación falla ante una referencia prohibida y pasa al restaurar el estado | Procedimiento, resultado negativo/positivo y confirmación de que no quedan cambios temporales. |
| V07 | CA01–CA07 | Integrada: compilación y suite | SDK y dependencias restauradas; tareas de implementación completadas | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal` y `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Compilación y suite pasan; warnings se registran, sin inferir aprobación o aceptación | Comandos, SDK, códigos de salida y resumen real. |

V02–V05 comprueban el diseño y las firmas contractuales que son entregable de S4.2; no afirman que futuros casos de uso ni repositorios concretos estén implementados. Las pruebas de ejecución de identidad, aislamiento público/privado y caminos de commit/rollback corresponden a las etapas que implementen esos consumidores y la persistencia. V06 es una comprobación negativa del test arquitectónico, no una sustitución de la matriz de dependencias ni una prueba funcional de propiedad.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde la plantilla. Las tareas inventariarán las operaciones y contratos concretos frente a S4.1 y a los criterios disponibles de S4.4, e incluirán sus validaciones y dependencias.
- Antes de implementar, confirmar la aceptación y el estado efectivo de S4.1; resolver la matriz de contratos sin cambiar reglas funcionales. Si la fuente no permite decidir una firma que altere comportamiento, dejarla bloqueada y escalarla a su documento propietario.
- La implementación de los casos de uso S4.6–S4.10 sigue bloqueada hasta que S4.5 esté aprobada; la aprobación de este plan o de las tareas de S4.2 no abre esa puerta.
- S4.2 se considerará validada cuando CA01–CA07 tengan evidencia satisfactoria y DLG acepte el resultado explícitamente en las tareas. Esa aceptación no aprueba S4.5.
- Si una validación revela un cambio de regla o contradicción, detener la tarea afectada y tramitar la aprobación en la fuente propietaria antes de actualizar los artefactos dependientes.
