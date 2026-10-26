# Plan técnico: S8.1 — Listado de colecciones

## Estado y aprobación del plan

- Estado: aprobado, con actualizaciones de contrato y regla fuente aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»); sincronizada tras la resolución de `p` inválido.
  - Plan: aprobado por DLG el 25-X-2026 («Siguiente»).
  - Actualización de contrato: DLG autorizó el 25-X-2026 un resultado paginado con total, implementado por operación nueva en Application que reutiliza el repositorio paginado y su conteo y conserva `ListAllCollections` («Sí: aplica el resultado paginado nuevo y conserva ListAllCollections intacto»).
  - Regla fuente: DLG autorizó que `p` inválido/fuera de rango muestre la primera página sin error y la sincronización en `specifications.md` el 25-X-2026.

## Diseño de implementación

### Componentes y contratos

- Una Razor Page privada para `/app/colecciones` consume una operación de Application que devuelve los elementos de la página y el total de páginas. La ampliación autorizada añade un resultado paginado dedicado en `Dtos/Models`; no cambia la semántica de la operación de lista existente usada por consumidores previos.
- La identidad se obtiene del contexto de usuario identificado de servidor ya establecido. Los parámetros de query solo controlan búsqueda y navegación, no propiedad ni autorización.
- La PageModel traduce el resultado de Application a un modelo de presentación de listado, búsqueda, paginación o error. Reutilizar los componentes accesibles existentes cuando encajen sin cambiar su contrato ni introducir estado compartido innecesario.
- Los criterios funcionales y los parámetros se mantienen en las fuentes de [spec.md → «Fuentes»](spec.md#fuentes); el plan no replica sus valores.

### Flujos internos

1. Recibir GET en la ruta canónica de colecciones.
2. Interpretar `q` y `p` según las reglas de la página, reutilizando el analizador de S7.6 si su contrato disponible las cubre.
3. Invocar Application con la identidad de servidor y los parámetros de listado válidos.
4. Presentar la página de resultados o el estado vacío; representar errores según las respuestas ya definidas.
5. Construir enlaces de paginación que mantengan los parámetros aplicables y un formulario de búsqueda que descarte la página actual.

### Persistencia y dependencias

- Web no consulta repositorios directamente. `ICollectionService.ListCollections` proporciona la operación de listado con búsqueda y página; `CollectionService` obtiene el `UserId` desde `ICurrentUser` y delega la consulta al repositorio. Infrastructure ofrece la consulta paginada correspondiente.
- No se esperan cambios de esquema. Application añade un modelo de salida paginado y una operación de consulta que devuelve la página y el total. El repositorio expone ambos en una operación por usuario/búsqueda; la implementación reutiliza las consultas paginada y de conteo existentes, sin materializar todas las colecciones. El método preexistente `ListAllCollections` queda intacto para su consumidor del sidebar.
- La integración pendiente de consumidores de S7.6 se satisface para esta página en S8.1; no se exige completar ni reabrir otros consumidores o criterios de esa spec.
- No se esperan cambios de esquema ni nuevas transacciones. Las lecturas siguen la arquitectura de persistencia y las consultas paginadas no cargan en memoria resultados innecesarios.

### Garantías técnicas

- Mantener Clean Architecture: Web → Application; no añadir reglas de negocio en la PageModel.
- Preservar la propiedad por usuario y evitar confiar en identificadores de usuario, alias o slug enviados por el cliente.
- Codificar contextualmente los valores presentados mediante Razor y conservar los controles de navegación y estado vacío accesibles.
- Validar el filtrado, orden y paginación tanto en la integración Web como en el punto de consulta disponible, sin atribuir a una prueba más garantías de las que ejerce. La integración debe normalizar `p` inválido o fuera de rango a la primera página conforme a [specifications.md → «Rutas privadas»](../../../context/specifications.md), sin alterar el analizador común a rutas que no forman parte de S8.1.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener la página como frontera de presentación y consumir Application para obtener el listado. | confirmado | [architecture.md → «Arquitectura»](../../../context/architecture.md) y S8.1 aprobada. | Mantiene las reglas y consultas fuera de Web. |
| Añadir una operación Application que devuelva la página de colecciones junto con el total de páginas, reutilizando las lecturas paginadas y de conteo existentes. | confirmado | Aprobación explícita de DLG el 25-X-2026 («Aprueba la actualización y continúa con el resultado paginado con total»); [ICollectionRepository](../../../src/Linkubator.Application/Contracts/ICollectionRepository.cs). | La PageModel puede construir navegación completa sin cargar toda la colección ni alterar el contrato antiguo usado por el sidebar. |
| Reutilizar el analizador de query de S7.6 para extraer los demás parámetros; normalizar `p` inválido/fuera de rango a primera página en el consumidor de S8.1. | confirmado | Regla aprobada en [specifications.md → «Rutas privadas»](../../../context/specifications.md), decisión DLG del 25-X-2026 y [S7.6 — plan técnico](../../s7.6-analisis-parametros-listados/plan.md). | Mantiene el analizador compartido y aplica la regla de colecciones dentro de la ruta propia, sin alterar las demás rutas privadas. |
| La revisión de accesibilidad requiere inspección del HTML renderizado y operación por teclado; los tests HTTP por sí solos no prueban todas las necesidades de asistencia. | confirmado | [requirements.md → «Accesibilidad»](../../../context/requirements.md). | Registrar por separado evidencia automatizada y revisión manual. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; integración HTTP de GET al listado con datos de dos identidades/dos conjuntos de colecciones. Verifica el límite Web y el aislamiento en esta página. | Contrato de listado disponible; harness HTTP y fixtures existentes. | Detectar test class/nombre en el proyecto Tests y ejecutar filtro específico con `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter <filtro-detectado>`. | Solo aparecen los elementos del usuario de la identidad del servidor; cambios de parámetros enviados no cambian esa identidad. | Comando real, pruebas ejecutadas, resultado y alcance exacto de los datos preparados. |
| V02 | CA02 | Automatizada; casos HTTP/unitarios para búsqueda, parámetros vacíos, repetidos, inválidos, desconocidos/no aplicables, orden estable, paginación y conservación/limpieza de parámetros en controles. | V01 y mecanismo de consulta disponible. | Mismo filtro focalizado, ampliado con los casos detectados; comprobar posteriormente el comando exacto del runner. | Los valores visibles y errores coinciden con las fuentes y AC-COL-05; no se aplican filtros de otros listados. | Casos, resultados y cualquier límite no cubierto por el harness. |
| V03 | CA03 | Automatizada; respuesta renderizada de primera página sin datos y búsqueda sin coincidencias. | Página y componentes de estado vacío disponibles. | Añadir/ejecutar casos en el filtro HTTP focalizado identificado para V01. | Mensaje de estado vacío accesible; sin filas ni controles de paginación que apunten a páginas inexistentes. | HTML/respuesta verificada, resultado y comprobación de los selectores relevantes. |
| V04 | CA04 | Manual y revisión técnica; usar la página renderizada con teclado y revisar nombres accesibles, orden de tabulación, foco, encabezados, estado vacío y controles. | Implementación ejecutable y entorno local. | Seguir la revisión indicada por requirements.md; registrar pasos concretos en tasks.md al aprobarlo. | Búsqueda, resultados/paginación y estado vacío se pueden identificar y operar con teclado; observaciones y límites quedan registrados. | Navegador/entorno, pasos, observaciones, resultado y aceptación humana por separado. |
| V05 | CA01–CA04 | Integrada: build/tests focalizados, diff y revisión de trazabilidad de la página. | V01–V04. | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter <filtro-detectado>` y `git diff --check`; ejecutar build de Web si el test no lo reconstruye. | Validaciones satisfactorias; el diff respeta las capas y no cambia fuentes funcionales. | Salida real, comandos, warnings relevantes y límites de validación. |

`<filtro-detectado>` queda como tarea de detección, no como comando ejecutable final, hasta confirmar el nombre real de los tests existentes. Si no hay cobertura HTTP adecuada, se define en tareas tras inspeccionar el runner y la arquitectura disponible. Los tests automáticos no sustituyen la comprobación manual de accesibilidad. La comprobación de que un test detecta un fallo de aislamiento debe usar una fixture adversarial controlada o prueba negativa reversible, sin alterar datos persistentes compartidos.

## Orden de ejecución y cierre

- [tasks.md](tasks.md) fue creado tras la aprobación y aprobado por DLG el 25-X-2026 («Vamos»).
- La ampliación del contrato Application para devolver los elementos de página y el total está aprobada por DLG; T02 se implementó con esa solución acotada.
- La implementación y validación de esta spec finalizaron y fueron aceptadas por DLG con «Completado» el 25-X-2026. La evidencia automatizada y el alcance limitado de la revisión manual se registran en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio). No depende de completar otras rutas, el resto de S7.6 ni tareas posteriores de S8.
