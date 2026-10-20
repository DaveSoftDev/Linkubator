# Plan técnico: S6.9 — Validación manual de búsqueda y reconstrucción FTS5

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Apruebo la spec y preparar el plan»), decisión explícita mediante formulario en esta conversación.
  - Plan: aprobado por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- La validación se realiza sobre una copia de la base local `data/database/Linkubator-S6.8-20261020.db`; el archivo original se mantiene intacto y ambos quedan fuera del control de versiones por las reglas existentes.
- Las búsquedas se ejecutan directamente mediante conexiones SQLite de solo lectura, con los parámetros de la identidad de desarrollo y expresiones FTS5 equivalentes a las consultas ya implementadas en S6.5. No se requiere un host Web ni cambios a la solución.
- La reconstrucción se invoca con la utilidad existente `tools/s66-rebuild-search-projections` y la ruta de la copia de validación. La herramienta exige una base existente; no debe apuntarse a la base original ni a una ruta sin comprobar.
- El escenario reproducible de consulta y daño de proyección se hará con SQLite/PowerShell o el cliente SQLite disponible. Antes de planificar los comandos finales de tarea se verificará qué herramienta SQLite está instalada y los nombres/rutas reales del esquema.

### Flujos internos

1. Verificar que la base local existe, está ignorada por Git y contiene las tablas/proyecciones de enlaces y colecciones. Crear una copia de trabajo fuera del archivo de origen y confirmar su ruta.
2. Consultar los datos semilla y confirmar la identidad de desarrollo; ejecutar búsquedas FTS5 acotadas por usuario y cada tipo de proyección antes de alterar filas. Registrar términos, criterios y resultados reales para los escenarios de CA01–CA03.
3. En la copia, eliminar una fila identificable de cada proyección, manteniendo intactas las entidades; confirmar de manera discriminante que las búsquedas afectadas dejan de devolver la coincidencia.
4. Ejecutar la utilidad S6.6 con la ruta de la copia, nunca con una ruta inexistente o con la base original; comprobar el código de salida.
5. Repetir las búsquedas y comparar las filas reconstruidas y las entidades. Registrar comandos, versiones de SQLite/.NET si se consultan, resultados y límites observados.
6. Eliminar la copia solo después de guardar la evidencia necesaria, o conservarla únicamente si se confirma que está excluida por Git y se documenta su propósito temporal.

### Persistencia y dependencias

- Solo se modifica una copia local de la base de S6.8; la operación no escribe en los datos originales ni en bases de otros entornos.
- S6.5 provee consultas de enlaces y colecciones; S6.6 provee la utilidad de reconstrucción; S6.8 provee el seed local. No se agregan SQL versionados, migraciones, datos nuevos ni invocaciones al arranque.
- El borrado deliberado de filas de proyección es una preparación manual de la copia para validar recuperación, no un cambio de entidad ni una tarea de implementación.
- Se emplea la CLI `sqlite3` si existe en el entorno. Si no está disponible, se debe elegir antes de ejecutar la validación una alternativa local ya instalada (por ejemplo, el cliente de base de datos disponible en el IDE) y documentar la alternativa; no instalar dependencias por defecto.

### Garantías técnicas

- La copia protege la base local que otras pruebas manuales puedan necesitar y garantiza que el escenario destructivo de reconstrucción no altera el archivo original.
- Consultas separadas, `UserId` acotado y unión con entidades permiten verificar aislamiento y separación por tipo conforme a [architecture.md → «FTS5»](../../context/architecture.md#fts5) y [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- La alteración y posterior reconstrucción verifican el recorrido operativo de S6.6 sobre datos reales locales; no sustituyen sus pruebas de atomicidad ni la matriz de integración de S6.7.
- La evidencia de esta spec se limita a las comprobaciones ejecutadas manualmente; no se atribuyen resultados a la interfaz Web ni a S10.6.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar comprobaciones destructivas solo sobre una copia del archivo de base de S6.8. | confirmado | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos), CA04 | Evita perder el fixture local. Confirmar ruta y exclusión antes de ejecutar cambios. |
| No depender de SQLite CLI sin verificar su disponibilidad. | confirmado | Inspección del entorno al preparar el plan: `sqlite3` no está en PATH. | T01 detectará alternativa local ya disponible o bloqueará esa validación concreta para solicitar una decisión; no instalar herramientas como parte de la spec. |
| Comprobar S6.9 en capa SQLite, sin Web ni S10.6. | confirmado | [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [spec.md → «Alcance»](spec.md#alcance) | Mantiene el cierre independiente de interfaz futura; no demostrar el criterio de S10.6. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA03 | Manual/SQLite: consultas previas, diferenciadas por tipo y acotadas por identidad, en la copia de base. | Base S6.8 disponible, copia de trabajo verificada y una herramienta SQLite local. | T02 registrará los comandos ejecutables exactos según herramienta detectada; contrastar resultados FTS5 con filas semilla y sus entidades. | Resultados solo del tipo y usuario consultados; el recuento y los ejemplos coinciden con los datos semilla pertinentes. | Transcripción acotada de consultas, ruta de copia, resultados por consulta y fecha. |
| V02 | CA02 | Manual/SQLite: ejecutar casos discriminantes preparados por S6.8 para acentos/NFC, términos obligatorios, operadores literales, filtro y paginación. | V01; identificar previamente los ejemplos y expresiones FTS5 compatibles con S6.5. | T02 concretará comandos y parámetros tras inspección del script S6.8 y la consulta SQL real. | Cada observación corresponde a las reglas funcionales citadas; se registra cualquier límite de la comprobación directa frente a la interfaz. | Matriz de entrada y filas devueltas observadas, sin declarar comprobaciones que no se ejecutaron. |
| V03 | CA04 | Manual: alterar las proyecciones solo en la copia, comprobar pérdida controlada de coincidencias, ejecutar CLI S6.6 y repetir las consultas. | V01; CLI S6.6 construida/disponible; copia con ruta confirmada. | `dotnet run --project .\tools\s66-rebuild-search-projections\S66RebuildSearchProjections.csproj -- <ruta-de-copia>`; las instrucciones de manipulación SQLite se concretarán tras detección T01. | CLI exitosa; las proyecciones vuelven a reflejar las entidades y las coincidencias previas se recuperan, sin alterar el archivo de origen. | Comandos exactos, salida de CLI, comparación antes/después y verificación de origen intacto. |
| V04 | CA01–CA04 | Revisión técnica de seguridad de datos locales y trazabilidad de criterios. | V01–V03. | Verificar `git check-ignore -v` de la base fuente y que la copia temporal está fuera del repositorio; revisar estado Git, enlaces y matriz evidencia-criterio. | Ningún archivo local de base queda versionado; cada criterio tiene evidencia real o bloqueo explícito y S10.6 queda fuera del cierre. | Salida de inspecciones y matriz de resultados, fecha y entorno. |

## Orden de ejecución y cierre

- Crear `tasks.md` solo tras aprobación explícita de este plan; incluir una tarea inicial para confirmar la herramienta de SQLite y comandos exactos antes de modificar la copia.
- Si no hay una alternativa local utilizable para la manipulación de la base, detener la tarea afectada y registrar la pregunta concreta; no instalar tooling ni tocar el archivo fuente como atajo.
- El cierre requiere evidencia satisfactoria propia para CA01–CA04 y aceptación humana registrada en las tareas; no espera a la implementación ni a la ejecución de S10.6.
