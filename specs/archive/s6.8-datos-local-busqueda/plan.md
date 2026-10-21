# Plan técnico: S6.8 — Datos locales de ejemplo para búsqueda

## Estado y aprobación del plan

- Estado: aprobado; trabajo cerrado para su alcance técnico.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Apruebo la spec y preparar el plan»), decisión explícita mediante formulario en esta conversación.
  - Plan: aprobado por DLG el 20-X-2026 («Apruebo el plan y preparar tasks.md»), decisión explícita mediante formulario en esta conversación.
  - Resultado: aceptado por DLG el 20-X-2026 («Tenemos búsqueda»); [evidencia y cierre](tasks.md#aceptación-y-cierre).

## Diseño de implementación

### Componentes y contratos

- El entregable principal es un script SQL local no versionado, invocado explícitamente mediante un modo S6.8 de la utilidad local existente `tools/s510-manual-review`. El modo existente `prepare` solo modifica marcadores de revisión; el modo nuevo queda restringido al script de S6.8, recibe una ruta de base existente y el email configurado de desarrollo.
- El archivo local de datos vive en `data/local/s6.8-seed.sql`, bajo una regla específica de `.gitignore`. El ejecutable y su modo de invocación se versionan; el script de datos queda ignorado.
- La aplicación configura la base por `ConnectionStrings:Linkubator`, aplica migraciones y provisiona/resuelve la identidad de desarrollo durante el arranque. La validación técnica inicial se realizó con una base temporal aislada; a petición de DLG, la base final validada se conservó en `data/database/Linkubator-S6.8-20261020.db`, ignorada por Git mediante la regla general `*.db`.
- El script resuelve al usuario mediante el email de desarrollo configurado (`user@linkubator.test` como valor por defecto de la configuración versionada); el modo genérico del harness recibe ese email explícitamente para acotar la ejecución, pero no acepta `UserId`, alias ni slug de propietario como parámetros.
- El modo genérico del harness local utiliza `SqliteConnectionFactory` con la colación `LINKUBATOR_ES_ES`, configura `NormalizeSearchText` con el normalizador compartido y ejecuta el script bajo transacción. No añade cambios al código productivo, esquema, migraciones, endpoint, tests ni cableado al arranque Web.
- El fixture de prueba borra bases temporales al terminar, por lo que no se reutiliza como almacenamiento de demostración persistente; se ejecutará la validación sobre una copia temporal retenida y eliminable al final.

### Flujos internos

1. Arrancar Web sobre una ruta temporal explícita para aplicar las migraciones y aprovisionar la identidad de desarrollo; detener el host una vez que la aplicación responde.
2. Cargar el script local contra esa base y validar las búsquedas/reconstrucción en ella; al concluir se conserva una copia en `data/database/Linkubator-S6.8-20261020.db` para el recorrido manual S6.9/S10.6. El modo de carga exige un archivo de base existente y no lo inicializa ni sobrescribe automáticamente.
3. El runner valida la existencia de la identidad completa antes de ejecutar SQL. En una transacción `BEGIN IMMEDIATE`, el script local localiza o crea las colecciones de ejemplo e inserta los enlaces mediante claves estables; si una operación falla, el runner revierte la transacción.
4. El script inserta las filas asociadas de `CollectionSearch` y `LinkSearch` en esa misma transacción, con textos normalizados NFC por `NormalizeSearchText`. Esta proyección espejo es necesaria porque la carga es SQL de mantenimiento y no usa repositorios productivos; S6.6 reconstruirá las mismas filas desde las entidades si se altera una proyección.
5. Informar el estado de la base y recuentos por el usuario seleccionado, sin volcar contraseñas ni secretos.

### Persistencia y dependencias

- Usar el esquema existente de `User`, `Collection`, `Link` y las proyecciones `CollectionSearch`/`LinkSearch`; no añadir tablas ni modificar migraciones.
- No extender repositorios ni el código productivo para un trabajo de carga manual. Añadir al harness de revisión manual S5.10 un modo genérico y acotado de ejecutar el script SQL, dentro de una transacción explícita; mantener las filas concretas de datos en un archivo SQL local ignorado.
- Las altas de colecciones y enlaces deben incluir las filas de proyección necesarias en la misma transacción, respetando la normalización de S6.2/S6.4; los textos originales de dominio permanecen intactos.
- La carga no ejecuta reconstrucción; esta se invoca separadamente con la herramienta existente de S6.6.
- La base destino para la carga debe existir, estar migrada y contener la identidad de desarrollo. El script rechaza una base sin esquema o sin esa identidad y no crea archivos de base accidentalmente.

### Garantías técnicas

- Idempotencia verificable: repetir la carga no añade copias del mismo conjunto de datos.
- Aislamiento por propietario: el runner verifica una identidad completa por email y el SQL restringe altas y búsqueda de existentes al usuario obtenido; ningún identificador de usuario entrado determina el propietario.
- Compatibilidad FTS5: el SQL local actualiza las dos proyecciones dentro de la transacción con la normalización compartida; la reconstrucción S6.6 las regenera desde las entidades originales.
- La validación técnica en la base aislada conservada demuestra que el script puede cargar y sincronizar los datos contra el esquema SQLite actual; no demuestra la comprobación de la interfaz Web ni la búsqueda manual asignada a S6.9/S10.6, y no sustituye las pruebas automatizadas de S6.7.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener los datos de ejemplo y el script fuera del control de versiones, como herramienta local y no parte del producto. | confirmado | [mvp0-plan.md](../../../plans/mvp0-plan.md), [spec.md → «Alcance»](spec.md#alcance) | Evita que el conjunto de ejemplo se introduzca en despliegues o scripts del producto. T01 verificará la exclusión concreta. |
| Reutilizar `tools/s510-manual-review` con un modo S6.8 explícito para ejecutar únicamente su SQL local contra una base existente, sin cableado al host Web. | confirmado | Inspección T01; CA01, CA03 | El modo se restringe a `data/local/s6.8-seed.sql`, valida base, esquema e identidad; el SQL específico permanece ignorado por Git. |
| Identificar el conjunto de ejemplo con claves deterministas para que una segunda ejecución no duplique datos. | confirmado | Elección explícita de DLG en aclaración del borrador, 20-X-2026; CA03 | La clave debe respetar las restricciones únicas existentes y no sobrescribir datos ajenos o preexistentes sin autorización. |
| Transacción SQLite única para el conjunto completo, incluyendo las proyecciones de ambas entidades. | confirmado | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), implementación de repositorios confirmada durante T01 | Una violación o fallo revierte toda la carga y evita datos o índices parciales. |
| Crear solo colecciones y enlaces, sin etiquetas. | confirmado | [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance), [spec.md → «Alcance»](spec.md#alcance) | Se limita al esquema y funcionalidades existentes de la etapa actual. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada, verificación SQL de integración sobre una base aislada migrada. Comprobar datos con consultas FTS5 y consultas directas limitadas al archivo de validación. | Base aislada, esquema S6.1 y CLI S5.10 disponibles. | Ejecutar el modo restringido `script` con el SQL local; revisar recuentos por identidad y consultas de búsqueda por ejemplos. | Se crean las colecciones/enlaces del usuario configurado, con ejemplos discriminantes para CA02; los resultados aparecen solo en la entidad/listado que corresponde. | Comando, versión de .NET/SQLite, ruta de la base final, resultados/recuentos observados. |
| V02 | CA03 | Automatizada y revisión de control de versiones: ejecutar dos veces contra la misma copia e inspeccionar el estado del repositorio para confirmar que el artefacto local está excluido. | V01; regla de `.gitignore` añadida para el SQL de ejemplo. | Repetir el modo `script`; revisar `git check-ignore -v data/local/s6.8-seed.sql` y `git status --short`. | Los recuentos no aumentan en la segunda ejecución y el script queda ignorado, no staged ni versionado. | Salida de ambas ejecuciones, recuentos comparativos y regla de exclusión aplicada. |
| V03 | CA04 | Automatizada, control técnico en una copia temporal: cargar ejemplos, borrar las filas de proyección de esa copia dentro de una transacción confirmada, ejecutar la utilidad existente de reconstrucción y consultar de nuevo. | V01; herramienta de reconstrucción S6.6 disponible. | Ejecutar carga con `tools/s510-manual-review script` y reconstrucción con `tools/s66-rebuild-search-projections`; la base validada se conserva en `data/database/Linkubator-S6.8-20261020.db`. | Las entidades de ejemplo permanecen y vuelven a ser consultables tras reconstruir; la herramienta de carga no modifica proyecciones en una segunda ejecución ni invoca reconstrucción. | Comandos, base final, recuentos y búsquedas antes/después. |
| V04 | CA01–CA04 | Revisión técnica de seguridad/localidad y cobertura fuente→criterio→validación. | V01–V03. | Revisar rutas en `.gitignore`, referencias desde Web/startup, uso de identidad configurada y ausencia de parámetros de propietario suministrados por usuario. | No hay cableado de producto ni datos versionados; el script delimita la identidad local y sus escrituras. | Archivos revisados, comandos de inspección y resultado por criterio. |
| V05 | CA02, CA04 | Comprobación manual de búsqueda y reconstrucción: fuera del cierre técnico de S6.8. | Datos locales preparados y aplicación/CLI disponibles. | Asignada a S6.9 y S10.6 según [mvp0-plan.md → S6 y S10](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l). | La persona responsable verifica los escenarios en la Web y la reconstrucción según el encargo de esas tareas. | Evidencia registrada únicamente en las tareas propietarias S6.9/S10.6; no bloquea el cierre de S6.8. |

## Orden de ejecución y cierre

- Las tareas están aprobadas por DLG y la implementación técnica se completó; la evidencia por criterio y el estado de aceptación se mantienen en [tasks.md](tasks.md#evidencia-por-criterio) y [«Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- Se usó una base temporal con override de conexión efímero, sin crear configuración local ni tocar la base habitual.
- La aceptación humana de S6.8 sigue pendiente; las pruebas manuales Web de S6.9/S10.6 son posteriores y no bloquean el cierre de la parte técnica de S6.8.
