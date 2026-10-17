# Plan técnico: S4.9 — Fakes en memoria y pruebas de casos de uso

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan: aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Inventario de cobertura.** Tomar como base `CollectionServiceTests` y `LinkServiceTests`, que ya contienen repositorios en memoria y dobles de Unit of Work privados anidados. Antes de cambiar tests, construir una matriz criterio aprobado → pruebas existentes → brecha; no contar una prueba solo por cubrir un método o por aparecer en la suite.
- **Fakes compartidos.** Propuesta: extraer a `tests/Linkubator.Tests/` únicamente los fakes repetidos o necesarios para nuevas pruebas, organizados por contratos/responsabilidad y accesibles desde las pruebas de Application. Preservar los comportamientos ya ejercitados y evitar un framework de testing general. Los repositorios falsos deben respetar los límites y argumentos de los contratos; no pretenden simular SQLite, FTS5, concurrencia física ni garantías de persistencia.
- **Pruebas de colecciones y enlaces.** Completar los huecos identificados por criterio en las clases existentes y/o sus módulos de tests. Cubrir al menos un test por criterio propietario aplicable, y escenarios positivos y negativos donde los defina; validar estado de las entidades y llamadas a repositorio/Unit of Work para distinguir efectos y rechazos.
- **Dashboard y lecturas públicas.** Añadir pruebas de Application para `DashboardService` y `PublicReadService`, con dobles de `ICurrentUser`, `IDashboardRepository` e `IPublicReadRepository`. Verificar las ramas y resultados definidos por AC-DASH-01, AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01 en el límite del servicio: identidad, validación/normalización de localizadores, ausencia, colección vacía, página y recuento. Las consultas y filtros reales son responsabilidad de repositorios concretos; los fakes solo verifican la delegación y la traducción del contrato.
- **Composición de las pruebas.** Mantener los tests bajo `tests/Linkubator.Tests/Application/`, con agrupación por caso de uso y contratos/fakes auxiliares en carpetas de responsabilidad. No añadir referencias ni paquetes; el proyecto ya usa xUnit y prueba los proyectos de producción.

### Flujos internos

1. Inventariar los métodos públicos de los servicios S4.6–S4.8, los tests actuales y los AC correspondientes; anotar explícitamente cobertura suficiente, faltante o no atribuible a Application.
2. Extraer los dobles compartidos de las pruebas existentes de forma incremental, asegurando que los tests previos continúan pasando en cada cambio. Si la extracción produce una abstracción genérica sin necesidad probada, conservar los dobles locales.
3. Completar las pruebas de colecciones y enlaces para las brechas de la matriz, siguiendo el contrato real y afirmando resultados, errores y ausencia de mutaciones indebidas.
4. Crear pruebas de dashboard y lecturas públicas para escenarios aprobados de identidad, entradas, datos presentes/ausentes y resultados vacíos. Comprobar argumentos de repositorio y `CancellationToken` cuando forme parte de la delegación bajo prueba.
5. Ejecutar la clase afectada tras cada cambio y después la suite completa; actualizar matriz y evidencia por criterio solo con resultados realmente observados.
6. Ante cualquier inconsistencia entre comportamiento actual y una regla aprobada, no ajustar el criterio ni ampliar producción desde los tests: detener el caso afectado y elevar la resolución a su documento propietario.

### Persistencia y dependencias

- El trabajo modifica exclusivamente el proyecto de tests; los fakes no se registran en producción. Se mantienen las dependencias autorizadas por [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).
- Los Unit of Work falsos permiten observar llamadas y resultados del contrato; no demuestran atomicidad de una transacción real. Las garantías SQL, concurrencia y proyecciones se validan en S5–S6.
- No se añaden paquetes ni se requieren servicios externos. El test runner es xUnit, ya configurado en `Linkubator.Tests.csproj`.
- AC-TRANS-03 y el registro de eventos se excluyen: pertenecen a S4.10. La respuesta HTTP, el renderizado y la presentación se excluyen también y se validan en Web.

### Garantías técnicas

- La matriz relaciona cada criterio aplicable con pruebas ejecutables; AC-COL, AC-LINK, AC-DASH, AC-USER, AC-COLLS, AC-COLLECTION y las reglas transversales de identidad/errores se consideran de acuerdo con sus fuentes propietarias. Los criterios sobre persistencia, Web y eventos quedan identificados como límites externos, no como checks de fakes.
- Las pruebas de propiedad incluyen recursos de usuarios distintos y verifican el resultado observable del servicio, no solo que una llamada ocurrió.
- Los fakes permanecen fuera de los proyectos productivos y no debilitan ni sustituyen sus contratos.
- La batería de lecturas públicas no atribuye al servicio el filtrado, orden, búsqueda o paginación que pertenece a la consulta concreta.
- Una suite verde acredita únicamente las pruebas ejecutadas y no constituye aceptación humana ni evidencia para otras etapas.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar xUnit y el proyecto de tests existente | Confirmado por la configuración actual | `tests/Linkubator.Tests/Linkubator.Tests.csproj` | No requiere paquetes ni runner nuevo. |
| Consolidar fakes duplicados de colección/enlace | Propuesta, sujeta al inventario de tareas | `CollectionServiceTests` y `LinkServiceTests` actuales; [AGENTS.md → «Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código) | Extraer solo lo que reduzca duplicación real sin aumentar acoplamiento entre suites. |
| Añadir cobertura de Dashboard y PublicRead | Confirmado como brecha de inventario inicial; alcance exacto pendiente de matriz | S4.8 y ausencia de tests específicos en `tests/Linkubator.Tests/Application/` | Los criterios conductuales de S4.8 no deben inferirse de build/revisión. |
| Los fakes no validan SQL, atomicidad física ni consultas reales | Confirmado por arquitectura | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Mantener esas comprobaciones en S5–S6 y no hacerlas condiciones de cierre de S4.9. |
| AC-TRANS-03/eventos fuera de esta spec | Confirmado | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | Corresponde a S4.10; no añadir capturas de logs como alcance implícito. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA06 | Técnica: inventario y trazabilidad | Tareas aprobadas | Revisar tests/fakes actuales de colecciones y enlaces; inventariar servicios S4.6–S4.8 y criterios AC aplicables; construir matriz criterio → tests concretos | Cada criterio aplicable tiene una prueba propuesta o una brecha explícita; límites de S4.10, persistencia y Web no se clasifican como falta de test de Application | Matriz, rutas/nombres de pruebas, brechas y fecha. |
| V02 | CA01, CA04–CA06 | Automatizada: colecciones y propiedad | Fakes y tests de colecciones actualizados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~CollectionServiceTests"` | Pruebas seleccionadas pasan e incluyen todos los AC de colecciones que la matriz atribuye a Application, incluida propiedad multiusuario | Comando exacto, SDK, resultado, tests y vínculos de evidencia. |
| V03 | CA02, CA04–CA06 | Automatizada: enlaces y propiedad | Fakes y tests de enlaces actualizados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~LinkServiceTests"` | Pruebas seleccionadas pasan e incluyen los AC de enlaces atribuibles a Application, incluidas operaciones rechazadas y propiedad | Comando exacto, SDK, resultado, tests y vínculos de evidencia. |
| V04 | CA03–CA06 | Automatizada: dashboard y lecturas públicas | Nuevos tests con repositorios falsos | Ejecutar con filtro de los nombres reales de las clases de test añadidas; fijar el filtro en tasks tras decidir la organización | Cada AC de S4.8 aplicable tiene casos automatizados para su resultado de Application, sin afirmar consulta SQL ni respuesta HTTP | Nombres reales de test, comando/filtro, SDK, resultado y matriz actualizada. |
| V05 | CA01–CA06 | Automatizada e integrada | Cambios integrados; tests de solución disponibles | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal`; `dotnet build Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` | Suite y build pasan; el diff está limpio; las exclusiones y brechas externas quedan declaradas | Comandos, SDK, exit codes, resumen de tests y rutas revisadas. |
| V06 | CA06 | Revisión técnica: independencia de producción y cobertura | Tests completados | Revisar `ProjectReferenceMatrixTests`, referencias productivas, ubicación de los fakes y matriz criterio → test; ejecutar `ProjectReferenceMatrixTests` con filtro si está disponible | No se introducen dependencias productivas ni tests sin criterio trazable; la matriz no acredita cobertura por mera compilación | Test y resultado, archivos revisados, matriz final y fecha. |

Las pruebas no prueban Infrastructure, SQL, transacciones reales, consultas FTS5, contrato HTTP, HTML, accesibilidad visual ni registro de eventos. La cobertura debe contrastarse con los criterios individuales del catálogo, incluidos los subcriterios AC-COL-01b y AC-LINK-01b; que todos los tests pasen no compensa un criterio sin test asociado.

## Orden de ejecución y cierre

- Tras la aprobación de este plan, crear `tasks.md` desde la plantilla. Las tareas detallarán primero la matriz de cobertura, y luego los cambios de fakes y pruebas con sus comprobaciones inmediatas; no se presume su aprobación ni inicio.
- Mantener cambios de prueba y fakes en Tests. Cualquier defecto de producción revelado se registra como bloqueo/propuesta con el criterio afectado; su corrección no se incorpora a esta spec por inferencia.
- Cada criterio aplicable debe tener evidencia automatizada satisfactoria dentro del alcance de S4.9 y las tareas propias completadas. Aceptación humana separada en tasks; no depende de las pruebas de S5–S6, Web o S4.10.
- Si la matriz descubre ambigüedad o criterio no comprobable con los contratos actuales, detener el criterio afectado y resolver primero la fuente propietaria.
