# Plan técnico: S6.6 — Reconstrucción manual de proyecciones FTS5

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: aprobado por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Implementar la reconstrucción en `SqliteSearchProjectionRebuilder` como servicio de Infrastructure y añadir una utilidad de consola local independiente en `tools/s66-rebuild-search-projections`; no es un endpoint ni se ejecuta durante el arranque.
- Reutilizar la factoría de conexión SQLite. El servicio abre una conexión propia y mantiene la transacción en su ámbito; no amplía los contratos de casos de uso del usuario ni acepta un `UserId` como parámetro de alcance.
- Usar `SearchTextNormalizer` en cada texto reconstruido, para que la ruta de mantenimiento deje las proyecciones en la misma forma NFC que la sincronización ordinaria.
- Mantener separada la lógica de reconstrucción de los repositorios de escrituras ordinarias; el trabajo no cambia el comportamiento de S6.4.
- Las dos proyecciones cubiertas son las de enlaces y colecciones. No incorporar etiquetas en implementación, pruebas ni mecanismo de entrada; su gestión está reservada a MVP1 según [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance).

### Flujos internos

1. La herramienta de consola recibe una única ruta explícita a una base existente, crea la factoría SQLite y llama al servicio `ISqliteSearchProjectionRebuilder`.
2. El servicio abre la conexión SQLite configurada y comienza una transacción; dentro de ella vacía las proyecciones de enlaces y colecciones y las repuebla desde las tablas de entidades y campos asignados en [architecture.md → «FTS5»](../../context/architecture.md#fts5).
3. Confirma la transacción solo cuando ambas proyecciones hayan terminado de regenerarse; ante un error propaga el fallo y revierte ambas.
4. Libera la transacción y la conexión al terminar. No invoca servicios externos ni espera entrada interactiva mientras mantiene la transacción.

### Persistencia y dependencias

- Reutilizar las tablas de proyección creadas por S6.1 y las tablas persistidas del dominio. No introducir tablas, migraciones, triggers ni paquetes nuevos sin necesidad comprobada.
- La lectura desde las tablas de entidades y la escritura a las proyecciones se realiza mediante SQLite/Dapper sobre la misma conexión y transacción.
- La reconstrucción usa una transacción SQLite explícita en el servicio dedicado; no requiere añadir el caso de mantenimiento al contrato genérico `IUnitOfWork` ni a los casos de uso.
- No añadir ni ejecutar el comando desde inicialización de la aplicación, migraciones, escrituras ordinarias o peticiones de usuario.
- Prerrequisitos implementados: proyecciones FTS5 de enlaces y colecciones de S6.1, sincronización ordinaria de S6.4, SQLite/Dapper y mecanismo de unidad de trabajo de Persistencia.

### Garantías técnicas

- Atomicidad de la reconstrucción completa, de acuerdo con [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y la descripción de mantenimiento en [architecture.md → «FTS5»](../../context/architecture.md#fts5).
- Regeneración determinista de ambas proyecciones desde sus entidades actuales, normalizando los campos proyectados con el `SearchTextNormalizer` existente, dentro del alcance de S6.6 en [spec.md → «Alcance»](spec.md#alcance).
- La ejecución requiere invocación manual; no cambia el ciclo de arranque ni las escrituras ordinarias.
- La prueba automatizada comprueba reparación y rollback con SQLite; no demuestra por sí sola el procedimiento operativo local con datos de ejemplo, que corresponde a S6.9.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Limitar la reconstrucción a las proyecciones de enlaces y colecciones. | confirmado | [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance), [spec.md → «Alcance»](spec.md#alcance), y exclusión de etiquetas en [S6.1](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md) | Mantiene el trabajo dentro de MVP0; no implementar ni probar proyecciones de etiquetas. |
| Implementar la reconstrucción en un servicio dedicado de Infrastructure y exponerla mediante herramienta local de consola separada del host Web. | confirmado | Inspección T01: existe el patrón de herramienta .NET independiente en `tools/s510-manual-review`; CA03 | No se conecta al inicio Web. La herramienta exige una ruta explícita a una base existente y no crea una base vacía por error tipográfico. |
| Usar una sola transacción para vaciar y regenerar ambas proyecciones. | confirmado | CA02, [architecture.md → «FTS5»](../../context/architecture.md#fts5) y [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Un fallo en cualquier fase debe dejar intacto el estado anterior de ambas proyecciones. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración SQLite: preparar entidades con proyecciones correctas, alterar/eliminar filas de las dos proyecciones y ejecutar el comando. | Fixture `SqliteRepositoryTestDatabase` y migraciones de test; servicio rebuilder disponible. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionRebuilderTests` | Tras reconstruir, ambas proyecciones reflejan los datos de entidad, los textos indexados quedan NFC y la prueba discrimina entre índice previamente alterado y resultado reparado. | Salida de test, escenarios, versión de .NET y SQLite. |
| V02 | CA02 | Automatizada, integración SQLite: capturar filas iniciales, hacer que falle una lectura fuente después de vaciar las proyecciones, y comprobar el estado tras rollback. | V01 y fixture SQLite existente. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionRebuilderTests` | Las modificaciones de ambas proyecciones se revierten; no queda aplicada una reconstrucción parcial. Las aserciones sobre los valores centinela demuestran que un estado parcial sería detectado. | Salida, causa del error inducido y comparación del estado antes/después. |
| V03 | CA03 | Revisión técnica automatizable/manual del cableado y punto de entrada. | Inspección de composition root, procesos de arranque y herramientas locales existentes. | Revisar referencias del comando y confirmar que solo la invocación de mantenimiento lo ejecuta. | No hay llamada desde startup, migraciones, operaciones de repositorio ni peticiones ordinarias. | Archivos inspeccionados y resultado de búsqueda/revisión en tareas. |
| V04 | CA01–CA03 | Validación manual de uso operativo; fuera del cierre técnico de S6.6. | Datos de ejemplo locales y herramienta disponible. | Asignada a S6.9 en [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l). | El usuario ejecuta la reconstrucción manual siguiendo la entrada local confirmada y verifica la búsqueda con los datos de ejemplo. | Evidencia registrada por S6.9; no atribuirla a las pruebas automatizadas de S6.6. |

## Orden de ejecución y cierre

- Las tareas de implementación y validación están aprobadas y su evidencia de cierre consta en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- CA01–CA03 tienen evidencia satisfactoria; S6.9 es validación manual vecina y no condiciona el cierre de S6.6.
- Si la inspección muestra que no existe un punto de entrada manual viable dentro de la solución actual o que no puede probarse el rollback sin alterar contratos productivos, detener la tarea afectada y plantear una decisión antes de introducir infraestructura nueva.
