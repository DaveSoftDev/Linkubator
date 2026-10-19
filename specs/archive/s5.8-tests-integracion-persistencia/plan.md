# Plan técnico: S5.8 — Tests de integración de persistencia

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Fixture SQLite temporal.** Reutilizar `SqliteRepositoryTestDatabase` para crear una base aislada, aplicar las migraciones y preparar usuarios, colecciones y enlaces. Si hacen falta casos de seed adicionales, extender el helper sin cambiar el esquema ni crear una base persistente.
- **Pruebas de restricciones.** Usar SQL directo parametrizado sobre conexiones abiertas por `SqliteConnectionFactory` para intentar duplicados y una relación de enlace/colección manipulada. La prueba observa tanto la excepción de SQLite como el estado posterior; no toma una validación previa de Application como evidencia de las restricciones.
- **Pruebas integradas de Application e Infrastructure.** Componer `CollectionService` y `LinkService` con los repositorios Dapper y `SqliteUnitOfWorkFactory` reales, con un `ICurrentUser` fijo. Mantener los tests dentro de `Linkubator.Tests`, sin iniciar Web ni usar la base local.
- **Transacción de creación de enlaces.** Ajustar `LinkService.CreateLink` para iniciar `BEGIN IMMEDIATE` antes de leer la colección, comprobar su visibilidad, buscar duplicados y escribir. Las lecturas y escrituras que deciden la operación deben usar los repositorios obtenidos de esa unidad; preservar los resultados de validación y duplicidad de Application. Revertir y liberar la unidad en todos los retornos no exitosos.
- **Intercalación controlada.** Para CA07, usar un decorador de prueba de `IUnitOfWorkFactory`/`IUnitOfWork` que pueda detener la primera operación antes de delegar `Begin`. Mientras la creación espera, la prueba hace privada la colección con el servicio real y una unidad independiente; después libera la creación. No usar `Sleep` ni depender del azar del planificador.
- **Pruebas de concurrencia restantes.** Ejecutar las operaciones mediante servicios reales y escrituras independientes sobre la base temporal. Coordinar el inicio con primitivas de test para que las peticiones compitan por `BEGIN IMMEDIATE`; comprobar el estado completo confirmado, no solo los `Result` individuales.
- **Listados.** Consultar mediante los repositorios ya implementados, sin texto de búsqueda. Incluir pares lingüísticamente equivalentes y empates para comprobar la consulta completa y el desempate propietario por `Id`.

### Flujos internos

1. Crear la base temporal y aplicar las migraciones a través de los servicios de Infrastructure.
2. Ejecutar las pruebas de CA01, CA02 y CA05 mediante SQL parametrizado; después, leer las filas para confirmar que el rechazo no produjo cambios parciales.
3. Para CA03 y CA04, invocar los casos de uso desde dos operaciones coordinadas y verificar que SQLite serializa las escrituras, las invariantes siguen satisfechas y los rechazos no dejan escrituras parciales.
4. Para CA07, detener la creación antes de comenzar su unidad, confirmar la privatización desde otra operación y liberar la creación. Con el arreglo propuesto, la creación debe leer el estado privado dentro de su unidad y rechazar la petición.
5. Para CA06, consultar las vistas de listado cubiertas por la spec mediante sus repositorios y comparar la secuencia entera con los resultados esperados derivados de `specifications.md → «Listados»`.
6. Disponer servicios, conexiones y archivos temporales incluso si una aserción o una operación falla.

### Persistencia y dependencias

- Reutilizar `SqliteConnectionFactory`, `SqliteMigrationService`, `SqliteUnitOfWorkFactory` y los repositorios existentes de S5.1–S5.5. Las conexiones de prueba mantienen WAL, timeout, claves foráneas y colación registrados por la fábrica.
- Las operaciones integradas que prueban invariantes usan el mismo Unit of Work para lecturas sujetas a carrera y escrituras. En `CreateLink`, esto requiere trasladar al UoW la lectura de colección y la comprobación de URL duplicada, no cambiar contratos ni DDL.
- No añadir tablas, migraciones, proyecciones FTS5, búsqueda textual ni referencias productivas desde Application a Infrastructure.
- S5.9 y S5.10 conservan sus revisiones manuales y no son prerrequisitos para ejecutar ni cerrar los tests automatizados de esta spec.

### Garantías técnicas

- CA01 y CA02 ejercitan las restricciones reales de SQLite en la base migrada, incluidas la unicidad y la relación compuesta, no dobles en memoria.
- CA03–CA04 y CA07 ejercitan los flujos de Application con transacciones SQLite reales; el estado confirmado cumple las invariantes aun cuando las operaciones compitan.
- En CA07, la comprobación de elegibilidad y la inserción de `CreateLink` ocurren dentro de la misma unidad `BEGIN IMMEDIATE`; el test sensible reproduce el orden que hacía obsoleta la lectura previa.
- CA05 verifica tanto el rechazo en Application como la defensa final de la FK ante SQL directo manipulado.
- CA06 verifica los órdenes en las consultas de repositorio completas, sin afirmar cobertura de búsqueda FTS5.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar SQLite temporal migrada y las implementaciones reales de repositorios y Unit of Work para la cobertura integrada. | Propuesta | [spec.md → CA01–CA07](spec.md#criterios-de-aceptación), `SqliteRepositoryTestDatabase` y servicios existentes | Comprueba las garantías en los límites relevantes sin tocar datos locales; confirmar el montaje durante la aprobación del plan. |
| Mover las lecturas de colección y duplicidad de `LinkService.CreateLink` dentro del `BEGIN IMMEDIATE` que protege la inserción. | Propuesta | [spec.md → CA07](spec.md#criterios-de-aceptación), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [S5.5 → riesgo registrado](../s5.5-unit-of-work-y-repositorios-dapper/plan.md#decisiones-técnicas-y-riesgos) | Cierra la ventana de carrera observada sin cambiar reglas ni contratos; preservar errores actuales y garantizar rollback en los retornos no exitosos. |
| Forzar la intercalación de CA07 con una unidad de prueba que pausa antes de abrir la transacción subyacente. | Propuesta | [spec.md → CA07](spec.md#criterios-de-aceptación) y [S5.5 → riesgo registrado](../s5.5-unit-of-work-y-repositorios-dapper/plan.md#decisiones-técnicas-y-riesgos) | Hace reproducible el fallo anterior y evita tests probabilísticos; validar que el decorador solo afecta la prueba y no requiere cambios en contratos productivos. |
| Comprobar concurrencia mediante operaciones independientes coordinadas y estado final en SQLite. | Propuesta | [spec.md → CA03–CA04](spec.md#criterios-de-aceptación), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | Comprueba serialización y atomicidad; el diseño de barreras debe evitar esperas arbitrarias y falsos positivos. |
| Cambiar reglas de producto, contratos o esquema para hacer pasar una prueba. | Fuera de alcance | [spec.md → «Alcance»](spec.md#alcance) y fuentes propietarias | Detenerse y tramitar cualquier discrepancia en el documento propietario antes de continuar. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA02 | Automatizada: imposición de restricciones SQLite | Base temporal migrada y fixtures válidos | Tests de integración con intentos directos de duplicidad y de relación compuesta inconsistente; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqlitePersistenceIntegration --verbosity minimal` | SQLite rechaza cada fila inválida, permite los alias nulos que admite el esquema y no quedan cambios parciales | Nombre de test, excepción observada, filas consultadas antes/después y salida del filtro. |
| V02 | CA03–CA04 | Automatizada: concurrencia de operaciones Application | Servicios y repositorios reales; base temporal con estado inicial controlado | Tests coordinados de eliminación de colecciones y de privatización frente a publicación de enlace; filtro `FullyQualifiedName~SqlitePersistenceIntegration` | Sobrevive al menos una colección; nunca queda enlace público en colección privada y no hay estado parcial | Operaciones iniciadas, resultados, lectura final de tablas y salida del filtro. |
| V03 | CA05 | Automatizada: propiedad en Application y garantía final SQL | Base temporal con dos usuarios y colecciones de ambos | Invocar `MoveLink` con una colección ajena y ejecutar una actualización SQL manipulada; filtro `FullyQualifiedName~SqlitePersistenceIntegration` | Application rechaza sin mutación, SQLite rechaza la relación compuesta y el enlace original permanece igual | Resultados, excepción SQL y estado persistido posterior. |
| V04 | CA06 | Automatizada: orden de consultas integradas | Esquema migrado, colación de la fábrica y datos con acentos, mayúsculas y equivalencias | Tests de listados privados de colecciones, colecciones públicas y enlaces públicos de colección; filtro `FullyQualifiedName~SqlitePersistenceIntegration` | Secuencias completas coinciden con el orden y los desempates propietarios por `Id` | Fixtures, secuencias obtenidas/esperadas y salida del filtro. |
| V05 | CA07 | Automatizada: carrera reproducible de creación pública | `LinkService`, `CollectionService`, repositorios y UoW reales; decorador de prueba con intercalación | Test que pausa la unidad de `CreateLink` antes de delegar `Begin`, privatiza la colección y reanuda; primero comprobar que el caso falla con el flujo actual y luego que pasa con el ajuste | Con el ajuste, la creación observa la colección ya privada y se rechaza; no queda enlace público ni escritura parcial. El test inicial debe fallar contra la implementación anterior | Intercalación aplicada, resultado de creación, estado final y evidencia de sensibilidad antes/después del arreglo. |
| V06 | CA01–CA07 | Integrada: regresión y compilación | Cambios y pruebas focalizadas completados; SDK y dependencias restaurados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqlitePersistenceIntegration --verbosity minimal`; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal`; `dotnet build Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` | Suite focalizada, suite completa, build y chequeo del diff terminan correctamente | Comandos, versión del SDK, códigos de salida, recuentos y enlace a evidencias V01–V05. |

V05 debe demostrar sensibilidad observando el fallo antes de modificar `CreateLink`; un test que solo pasa tras el cambio no demuestra que detecte la ventana de carrera. V02 confirma los invariantes de las operaciones concretas y no constituye una prueba general de toda posible intercalación. Las validaciones usan búsquedas sin términos; no prueban FTS5 ni sustituyen las revisiones manuales de S5.9/S5.10.

## Orden de ejecución y cierre

- [tasks.md](tasks.md) se creó después de la aprobación explícita de este plan y fue aprobado por DLG el 19-X-2026 («Vamos»). Las tareas asignan un check inmediato a cada criterio y no dependen de S5.9, S5.10 ni S6.
- El cierre requiere evidencia satisfactoria de CA01–CA07, tareas propias completadas y aceptación humana registrada; los tests verdes no conceden por sí solos aceptación ni cierran la etapa Persistencia.
- Si un test revela una discrepancia de reglas, contratos o esquema, detener la tarea afectada y tramitar el cambio en su documento propietario antes de implementar.