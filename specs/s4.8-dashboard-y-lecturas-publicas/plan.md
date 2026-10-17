# Plan técnico: S4.8 — Dashboard y lecturas públicas

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan: aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Servicio del dashboard.** Propuesta: añadir `IDashboardService` y su implementación en el módulo de casos de uso de dashboard, siguiendo el patrón de Application existente. Consultará `ICurrentUser`; si no hay identidad autenticada, devolverá el error de Application apropiado, y si la hay, solicitará `DashboardDto` a `IDashboardRepository.GetByUser` con el identificador obtenido de esa abstracción. No recibirá un `UserId` de la petición.
- **Servicio de lectura pública.** Propuesta: añadir `IPublicReadService` y una implementación cohesiva para las tres lecturas, consumiendo `IPublicReadRepository`. Las entradas serán los localizadores públicos y la página que requiera la lectura paginada; no se exigirá autenticación para estos casos de uso.
- **Contratos y resultados.** `IDashboardRepository`, `IPublicReadRepository`, `ICurrentUser`, `DashboardDto` y los contratos comunes `Result`/`ApplicationError` coordinan los casos de uso. `GetUserPage`, `GetCollectionsPage` y `GetCollectionPage` representan ausencia/no acceso mediante resultados anulables; `GetCollectionPageCount` está separado de la recuperación de página. Los métodos públicos usan `PublicCollectionSummaryDto` y `PublicLinkDto`, que contienen solo campos admitidos por las vistas públicas; los resultados nulos se traducen en errores de Application.
- **DTOs públicos y privacidad.** No se reutilizan `CollectionDto` ni `LinkDto` para lecturas públicas: estos modelos privados incluyen identificadores internos, estado de privacidad, fechas y URL normalizada. `PublicCollectionSummaryDto` expone nombre, slug y descripción; `PublicLinkDto` expone URL original, título, descripción e imagen. Las consultas concretas siguen siendo responsables de filtrar recursos públicos, ordenar y paginar según las fuentes propietarias; S4.8 no reimplementa esas consultas.
- **Errores.** Para ausencia de usuario, colección o lectura pública no accesible, usar el contrato de resultado de Application y un código estable sin revelar si el recurso es privado o inexistente; Web determina la respuesta HTTP prescrita por sus reglas. No incluir mensajes de presentación ni detalles de persistencia. El tratamiento concreto deberá concordar con el mapeo ya disponible y las fuentes propietarias; si no, registrar el bloqueo antes de introducir un código nuevo.
- **Identidad y composición.** El dashboard es una operación privada que obtiene identidad de `ICurrentUser`; las lecturas públicas no usarán ese contexto como autorización. Application coordina el contrato y devuelve su resultado, sin implementar rutas, canonical, metadatos, HTML ni lógica de presentación.

### Flujos internos

1. Dashboard: comprobar que `ICurrentUser` está autenticado; obtener su `UserId`; llamar `IDashboardRepository.GetByUser` con ese identificador y el `CancellationToken`; devolver `DashboardDto` mediante `Result`. El caso no acepta un usuario indicado en la entrada.
2. Página pública de usuario: pasar el alias al contrato de lectura, convertir el resultado de ausencia al error de Application definido tras contrastar el mapeo y devolver el modelo de lectura sin ampliar los datos expuestos.
3. Página pública de colecciones: solicitar por alias las colecciones que se muestran en público; distinguir la ausencia del usuario del conjunto vacío según la forma de resultado que establece el contrato y las fuentes, sin inventar un estado.
4. Página pública de colección: pasar alias, slug y página al repositorio; representar ausencia/no acceso según el criterio; solicitar el recuento mediante `GetCollectionPageCount` cuando el consumidor necesite el total, comprobando antes que el resultado de página es accesible y sin tratar un `404` como una página vacía. Tanto la página como su recuento dependen de la misma localización pública.
5. Propagar cancelación y errores no funcionales conforme al patrón existente; Application traduce solo los rechazos que correspondan a sus contratos y no absorbe indiscriminadamente errores de infraestructura.

La secuencia final se concretará al revisar los servicios aceptados de S4.6/S4.7 y los contratos completos. Si la nulabilidad o granularidad de un contrato no permite distinguir los casos requeridos por los criterios, se detiene el criterio afectado y se eleva una propuesta a decisión; no se cambia la semántica desde el servicio.

### Persistencia y dependencias

- Application usa únicamente sus contratos y Domain; no se añaden dependencias de Infrastructure, Web, SQLite, Dapper ni paquetes externos.
- No se implementan repositorios concretos, SQL, consultas FTS5 o transacciones: son responsabilidad de S5/S6.
- Las lecturas no mantienen transacciones abiertas innecesariamente, según [architecture.md → «Persistencia»](../../context/architecture.md#persistencia). No hay operaciones externas en este alcance.
- Los fakes y pruebas de comportamiento de los consumidores pertenecen a S4.9; esta propuesta no los añade ni los declara ejecutados. La presentación y el contrato HTTP pertenecen a las etapas Web indicadas en la especificación.

### Garantías técnicas

- El dashboard consulta exclusivamente con la identidad de `ICurrentUser` y el contrato de repositorio limitado a ese usuario.
- Las lecturas públicas no permiten acceso a recursos privados, y los alias/slugs actúan solo como localizadores públicos conforme a [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado).
- La forma, el orden y la paginación de cada resultado se mantienen conforme a los criterios aprobados y a las fuentes propietarias; Application no reimplementa las consultas que pertenecen al repositorio.
- Los errores de Application no revelan la causa privada/inexistente ni contienen textos de interfaz o detalles de Infrastructure.
- Los tipos públicos nuevos se ubican en archivos y módulos por responsabilidad y el proyecto mantiene la dirección de dependencias definida en [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) y [AGENTS.md → «Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener los casos de uso en Application y usar contratos de repositorio existentes | Confirmado por la arquitectura y los contratos de S4.2 | [architecture.md → «Application»](../../context/architecture.md#application), [S4.2](../s4.2-contratos-repositorios-identidad/spec.md) | Sin acceso a infraestructura concreta. |
| Un servicio privado de dashboard y un servicio cohesivo de lecturas públicas | Propuesta técnica | S4.8 y patrón `I*Service` → `*Service` de Application | Confirmar nombres y granularidad en las tareas tras revisar S4.6/S4.7; evitar un servicio general de “todo”. |
| Usar `Result<T>` para errores de identidad/recursos públicos no accesibles | Propuesta a contrastar con convenciones de servicios y mapeo Web | [architecture.md → «Application»](../../context/architecture.md#application), contratos `Result`/`ApplicationError` | Determinar los códigos y mapeos existentes; no crear una respuesta nueva si una fuente propietaria requiere otra. |
| Las lecturas públicas son anónimas y no consultan identidad autenticada | Confirmado por el carácter público de las páginas | [requirements.md → «Páginas públicas»](../../context/requirements.md#páginas-públicas), [specifications.md → «Rutas públicas»](../../context/specifications.md#rutas-públicas) | No usar `ICurrentUser` como requisito de esos casos. |
| Los DTOs de lectura existentes no limitan los campos a exponer en una página pública | Resuelto en la implementación mediante modelos públicos específicos | Contratos públicos de S4.8 y AC-USER-01/AC-COLLS-01/AC-COLLECTION-01 | Las interfaces públicas exigen tipos reducidos; selección, orden y paginación permanecen responsabilidad del repositorio y requieren su evidencia propietaria. |
| La ausencia y el total de páginas de colección se expresan mediante métodos de repositorio separados | Confirmado por la firma actual de `IPublicReadRepository`; coherencia semántica pendiente de comprobar | `IPublicReadRepository` y AC-COLLECTION-01 | Verificar cuándo invocar el recuento y qué ocurre si cambian datos entre lecturas; no introducir transacciones de lectura sin respaldo arquitectónico. |
| Las pruebas de comportamiento de consumidores y el registro de eventos pertenecen a S4.9 y S4.10 | Confirmado por el plan MVP0 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | S4.8 no adelantará esos entregables ni afirmará evidencia que aún no exista. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA06 | Técnica: inventario de contratos, DTOs, servicios precedentes y referencias | Tareas aprobadas; cambios de Application disponibles | Inspeccionar `IDashboardRepository`, `IPublicReadRepository`, modelos, errores y servicios de S4.6/S4.7; ejecutar `ProjectReferenceMatrixTests` existente | Cada criterio se asigna a un servicio/contrato; suficiencia o carencia contractual queda explícita; no se añaden dependencias prohibidas | Tipos revisados, matriz criterio → servicio/contrato, resultado de test, SDK y fecha. No prueba comportamiento funcional. |
| V02 | CA01–CA06 | Técnica: revisión de correspondencia con fuentes, privacidad y límites de capa | Casos de uso implementados; fuentes y catálogo aprobados | Revisar rutas de ejecución y resultados contra AC-DASH-01, AC-USER-01, AC-COLLS-01, AC-COLLECTION-01 y AC-TRANS-01/02; revisar que no se incluya información privada | Trazabilidad completa por criterio; cualquier hueco de contrato o regla detiene el criterio, sin inventar respuesta | Matriz criterio → tipo/flujo, hallazgos, decisión y evidencia revisada. Revisión no sustituye tests. |
| V03 | CA01–CA06 (seguimiento fuera de puerta S4.8) | Automatizada: comportamiento de consumidores con fakes, perteneciente a S4.9 | S4.8 implementada; fakes y tests de S4.9 disponibles | S4.9 ejecutará sus tests de dashboard y lecturas públicas con el filtro y runner definidos por sus tareas | Evidencia de consumidor para S4.9. Su ausencia no bloquea el cierre de S4.8, que se valida sobre los contratos, tipos, flujos y dependencias propios de Application | S4.9 registra comando/filtro, tests, SDK, fecha, resultado y referencia en sus artefactos. No se atribuye esta prueba a S4.8. |
| V04 | CA01–CA06 | Automatizada: compilación integrada | SDK .NET 10 y dependencias restauradas | `dotnet build Linkubator.sln --no-restore --verbosity minimal` | La solución compila; los warnings y errores se registran y resuelven dentro del alcance | Comando, SDK, exit code y resumen. Build correcto no acredita comportamiento. |
| V05 | CA01–CA06 | Automatizada e integrada: suite disponible y diff | Cambios integrados y validaciones anteriores | `dotnet test Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` | La suite disponible pasa y el diff no presenta errores de whitespace; si S4.9 no está disponible, se declara pendiente la evidencia conductual | Comandos, SDK, exit code, resumen y alcance real de las pruebas. Suite verde no equivale a aceptación humana. |

V03 queda bajo responsabilidad de S4.9 y es seguimiento independiente, no prerrequisito de cierre de S4.8. S5/S6 y las etapas Web validan por separado persistencia concreta, consultas SQL, canonical, contenido HTML y respuestas HTTP. S4.8 cierra sus propios entregables mediante revisión de Application, compilación, suite disponible y matriz arquitectónica; no declara esas validaciones externas como ejecutadas ni las condiciona.

## Orden de ejecución y cierre

- Tras la aprobación de este plan, crear `tasks.md` desde su plantilla; desglosará la inspección de contratos, casos de uso, comprobaciones por criterio, validación integrada y coordinación de evidencia de S4.9.
- Antes de cambiar un contrato o añadir un tipo de datos público, las tareas deben señalar el criterio bloqueado, la insuficiencia comprobada y la decisión necesaria. No ampliar el alcance por inferencia.
- Todo criterio debe tener tarea y validación vinculadas. La aprobación del plan no autoriza tareas ni cambios productivos; `tasks.md` necesita aprobación explícita propia.
- La aceptación del resultado de S4.8 requiere evidencia satisfactoria de los entregables de Application descritos en sus criterios y aceptación humana registrada en las tareas. Las evidencias de S4.9, persistencia y Web son seguimiento de etapas propietarias, no prerrequisitos ni puertas de cierre de S4.8. Build o tests verdes por sí solos no equivalen a aceptación.
- Ante una contradicción de fuentes, detener la tarea afectada y resolver primero el documento propietario.
