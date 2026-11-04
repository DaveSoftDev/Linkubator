# Plan técnico: S12.3 — Auditoría de invariantes críticas en código y SQL

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- Se trazará la propiedad de los datos desde la identidad actual de Web y los modelos de entrada de Application hasta los casos de uso y repositorios SQLite. La revisión abarcará [ICurrentUser.cs](../../src/Linkubator.Application/Contracts/ICurrentUser.cs), los servicios de colecciones y enlaces, los modelos de página privados y [SqliteCollectionRepository.cs](../../src/Linkubator.Infrastructure/Repositories/SqliteCollectionRepository.cs) y [SqliteLinkRepository.cs](../../src/Linkubator.Infrastructure/Repositories/SqliteLinkRepository.cs), además del SQL que ejecutan.
- Para el estado público o privado se seguirán las lecturas públicas y privadas y las transiciones desde Application hasta los repositorios. Se incluirán [PublicReadService.cs](../../src/Linkubator.Application/UseCases/PublicReads/PublicReadService.cs), [SqlitePublicReadRepository.cs](../../src/Linkubator.Infrastructure/Repositories/SqlitePublicReadRepository.cs), los repositorios de escritura y las páginas públicas de Web.
- Las instrucciones SQL persistidas se revisarán en las migraciones de SQLite y en los scripts de aprovisionamiento. También se buscará SQL embebido en los repositorios, constructores de consultas y servicios de persistencia; la auditoría no se limitará a los archivos `.sql`.
- Para respuestas de cuenta y manejo de secretos se inventariarán rutas y operaciones presentes en Web, Application e Infrastructure. Se contrastarán las operaciones existentes con sus respuestas, hashing, persistencia y superficies de logging. Las operaciones de cuenta pendientes de definición se clasificarán según [decisions.md → «Pendiente de rutas de cuenta»](../../context/decisions.md#pendiente-de-rutas-de-cuenta), sin asumir que están implementadas.
- Para los límites transaccionales se seguirán los usos de [SqliteUnitOfWork.cs](../../src/Linkubator.Infrastructure/Sqlite/SqliteUnitOfWork.cs) desde los casos de uso y repositorios, comprobando el trabajo síncrono y asíncrono que queda dentro de cada transacción.
- Las pruebas existentes de persistencia, Application, Web y logging se usarán como evidencia auxiliar. Su existencia o resultado no sustituirá la revisión de los caminos de ejecución ni demostrará cobertura exhaustiva.

### Flujos internos

1. Inventariar las superficies ejecutables relevantes: código de Domain, Application, Web e Infrastructure; migraciones y scripts SQL; consultas SQL embebidas; y operaciones de cuenta presentes.
2. Construir una matriz de revisión para CA01–CA05 que asocie cada invariante con sus entradas, capas, persistencia, superficies de respuesta o logging y comprobaciones existentes.
3. Seguir las rutas de extremo a extremo desde las entradas Web o los casos de uso hasta la consulta o escritura persistente. Para CA05, delimitar las transacciones y revisar las llamadas y esperas efectuadas dentro de ellas.
4. Registrar evidencia con referencias concretas a archivos y miembros. Distinguir cumplimiento observado, hallazgo, operación no aplicable por alcance y zona que no se pudo verificar.
5. Revisar la relación entre criterios, evidencia y conclusiones; consolidar el informe de CA01–CA06 en la evidencia de las tareas una vez que `tasks.md` esté autorizado.

### Persistencia y dependencias

- La auditoría es de solo lectura: no cambia repositorios, SQL, esquemas, migraciones, configuración ni dependencias.
- Las superficies SQL iniciales son [V004_CreateUserCollectionLinkSchema.sql](../../src/Linkubator.Infrastructure/Sqlite/Migrations/V004_CreateUserCollectionLinkSchema.sql), las migraciones posteriores de la misma carpeta y [ProvisionDevelopmentIdentity.sql](../../src/Linkubator.Infrastructure/DevelopmentIdentity/Sql/ProvisionDevelopmentIdentity.sql). El inventario incluirá además las instrucciones embebidas en C#.
- Las rutas de cuenta que aún no estén implementadas no bloquean la auditoría: se documentan como no aplicables a la implementación actual, conforme a la fuente de decisiones enlazada en [spec.md → «Alcance»](spec.md#alcance).
- No se depende de S12.2 ni de S12.4. Las pruebas existentes pueden aportar contexto, pero no son una dependencia de implementación ni sustituyen el informe.

### Garantías técnicas

- Cada conclusión indicará qué superficie fue inspeccionada y qué evidencia la respalda; no se inferirá cobertura de una capa a partir de otra.
- La revisión de SQL incluye consultas de lectura y escritura, restricciones y migraciones pertinentes, sin asumir que una garantía de aplicación está impuesta por la base de datos.
- La revisión de respuestas y secretos se limita a las operaciones presentes y a las reglas fuente; el trabajo no adelanta rutas ni requisitos de etapas posteriores.
- No se copiarán contraseñas, tokens ni otros valores sensibles a la evidencia. Los hallazgos describirán la ubicación y el flujo afectado sin divulgar el valor.
- Una ejecución de tests puede corroborar comportamientos concretos, pero no demostrar por sí sola que todas las rutas o consultas estén cubiertas.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener la auditoría en modo de solo lectura y no corregir los hallazgos dentro de S12.3. | Confirmado | Alcance de [spec.md](spec.md#alcance) | Los hallazgos se entregarán para revisión y resolución posterior; no se amplía esta tarea a cambios productivos. |
| Consolidar el informe y sus referencias como evidencia de CA01–CA06 en el futuro `tasks.md`, sin crear un artefacto adicional por ahora. | Propuesta | CA06 de [spec.md](spec.md#criterios-de-aceptación) y convención de [sdd-writer](../../skills/sdd-writer/SKILL.md) | Mantiene los resultados y el cierre en el artefacto propietario de la ejecución. Revisar esta decisión al aprobar las tareas si el volumen de evidencia requiere un informe separado. |
| Las operaciones de cuenta ausentes se clasificarán como no aplicables al alcance actual, no como hallazgos por ausencia. | Confirmado | [decisions.md → «Pendiente de rutas de cuenta»](../../context/decisions.md#pendiente-de-rutas-de-cuenta) y [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Evita adelantar trabajo de autenticación; cualquier operación parcial existente sí se revisa. |
| Los tests existentes pueden no cubrir todas las rutas de datos, SQL o transacciones que se inspeccionan. | Confirmado | Organización actual de `tests/Linkubator.Tests/` y CA01–CA06 | Los resultados de tests se informan como evidencia complementaria, nunca como sustituto de la trazabilidad manual. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica de identidad, operaciones privadas y consultas SQL | Inventario de rutas privadas, servicios, repositorios e instrucciones SQL | Seguir lecturas y escrituras desde Web/Application hasta SQLite; contrastar identidad y propiedad en consultas y relaciones persistidas; consultar como apoyo `CollectionServiceTests`, `LinkServiceTests`, `SqliteCollectionRepositoryTests` y `SqliteLinkRepositoryTests` | Cada ruta relevante tiene referencias a código y SQL, o queda explícitamente registrada como no verificable; los tests no se toman como prueba de cobertura completa | Matriz CA01, rutas examinadas, consultas/restricciones, tests consultados y hallazgos o límites |
| V02 | CA02 | Revisión técnica de lecturas y transiciones públicas/privadas | Inventario de páginas públicas, servicios y persistencia de privacidad | Seguir consultas de lectura pública y cambios de estado desde Web/Application hasta SQLite; consultar como apoyo `PublicReadServiceTests`, `SqlitePublicReadRepositoryTests` y `WebStartupTests` | Se documentan los caminos revisados y las restricciones o comprobaciones de privacidad relevantes, con cualquier diferencia o límite identificado | Matriz CA02, rutas y consultas inspeccionadas, tests consultados y hallazgos o límites |
| V03 | CA03 | Revisión técnica de operaciones y respuestas observables | Inventario de rutas y casos de uso realmente presentes | Contrastar cada operación de cuenta existente con la regla de respuestas; comprobar también las consultas que condicionen su contenido o tipo. Clasificar las operaciones ausentes conforme a la decisión de alcance | Todas las operaciones presentes quedan revisadas; las ausentes se identifican como no aplicables y no como cumplimiento demostrado | Inventario de operaciones, respuestas y consultas revisadas, referencias y clasificación de ausencias |
| V04 | CA04 | Revisión técnica de tratamiento, persistencia y logging de contraseñas y tokens | Inventario de productores, verificadores, persistencia y sinks de logging actuales | Rastrear los valores desde sus entradas hasta el almacenamiento y los logs; contrastar código, SQL y pruebas de logging disponibles con las fuentes propietarias | Las rutas existentes quedan documentadas sin exponer valores sensibles; operaciones ausentes y superficies no observables se distinguen expresamente | Matriz CA04, flujo y superficies inspeccionadas, referencias a código/SQL/tests y hallazgos o límites |
| V05 | CA05 | Revisión técnica de límites transaccionales y llamadas | Inventario de aperturas, confirmaciones, reversiones y usos de `IUnitOfWork` | Seguir cada operación transaccional desde Application hasta `SqliteUnitOfWork` y revisar esperas y dependencias externas dentro del límite; consultar `SqliteUnitOfWorkTests` como apoyo | Cada límite transaccional relevante queda identificado y cualquier operación incompatible o límite no verificado queda registrado | Matriz CA05, operaciones, límites y llamadas revisados, tests consultados y hallazgos o límites |
| V06 | CA01–CA06 | Revisión técnica de trazabilidad y regresión complementaria | V01–V05 completadas | Verificar que cada invariante tiene evidencia y conclusión; ejecutar `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --filter "FullyQualifiedName~CollectionServiceTests|FullyQualifiedName~LinkServiceTests|FullyQualifiedName~PublicReadServiceTests|FullyQualifiedName~SqliteCollectionRepositoryTests|FullyQualifiedName~SqliteLinkRepositoryTests|FullyQualifiedName~SqlitePublicReadRepositoryTests|FullyQualifiedName~SqliteUnitOfWorkTests|FullyQualifiedName~WebSecurityIntegrationTests|FullyQualifiedName~LoggingTests"` | La evidencia permite rastrear cada criterio; el resultado de tests se registra como complementario y no sustituye el análisis técnico | Tabla de evidencia CA01–CA06, comando, resultado, entorno y fecha |

## Orden de ejecución y cierre

- Crear `tasks.md` a partir de su plantilla solo después de la aprobación explícita de este plan.
- Ejecutar la revisión de inventario y los criterios CA01–CA05; resolver cada discrepancia de alcance antes de concluirla, sin corregir el código ni el SQL.
- Consolidar la evidencia y el resultado por criterio según V01–V06 en las tareas.
- El cierre técnico requiere evidencia para CA01–CA06 y tareas propias completadas; la aceptación humana se registra por separado y no se presume por esta propuesta ni por tests satisfactorios.
- Ante una contradicción con una regla fuente, detener la conclusión afectada y solicitar la resolución en su documento propietario; no redefinir la regla desde el informe.
