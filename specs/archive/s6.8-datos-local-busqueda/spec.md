# Especificación: S6.8 — Datos locales de ejemplo para búsqueda

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Apruebo la spec y preparar el plan»), decisión explícita mediante formulario en esta conversación.
- Revisión: 20-X-2026.

## Objetivo

Preparar una ayuda local, repetible y no versionada que cargue en la base SQLite de desarrollo datos sintéticos suficientes para probar manualmente las búsquedas y la reconstrucción de proyecciones, sin formar parte del producto ni sustituir las pruebas automatizadas.

## Fuentes

- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): encargo de S6.8 y comprobaciones manuales posteriores S6.9.
- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): escenarios manuales de búsqueda S10.6.
- [mvp0-plan.md](../../../plans/mvp0-plan.md): carácter local y no versionado de los datos.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros): términos, acentos, operadores literales, filtros y resultados por entidad.
- [specifications.md → «Listados»](../../../context/specifications.md#listados): relevancia y paginación.
- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): proyecciones de búsqueda y reconstrucción manual.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): escrituras relacionadas y transacciones.
- [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance): alcance actual limitado a enlaces y colecciones.
- [S6.5 — Consultas de búsqueda FTS5](../s6.5-consultas-busqueda-fts5/spec.md): consultas de enlaces y colecciones ya implementadas.
- [S6.6 — Reconstrucción manual de proyecciones FTS5](../s6.6-reconstruccion-proyecciones/spec.md): herramienta de reconstrucción ya disponible.
- [S6.7 — Pruebas integradas de búsqueda FTS5](../s6.7-pruebas-integradas-fts5/spec.md): cobertura automatizada que no sustituye esta ayuda manual.

## Alcance

- Incluye:
  - Preparar un script local, excluido del control de versiones, que cargue datos sintéticos en la base de desarrollo ya inicializada y asociados a la identidad de desarrollo configurada.
  - Facilitar ejemplos para que la comprobación manual pueda recorrer acentos y formas Unicode, varios términos, operadores tratados literalmente, filtro por colección y paginación, además de búsquedas separadas de enlaces y colecciones.
  - Permitir volver a ejecutar la preparación sin duplicar los datos de ejemplo.
  - Mantener separados los datos de ejemplo de las pruebas automatizadas y de las entidades de producción.
- Excluye:
  - Cambiar consultas, proyecciones, el esquema, la reconstrucción o cualquier comportamiento productivo.
  - Ejecutar la validación manual de búsquedas o reconstrucción; corresponde a S6.9.
  - Validar la interfaz Web o los parámetros HTTP; la cobertura de interfaz pertenece a sus etapas propietarias.
  - Incorporar datos de ejemplo versionados al producto, migraciones, scripts SQL versionados o pruebas automatizadas.
- Trabajo vecino y responsable de sus entregables:
  - S6.9 realiza la búsqueda manual y ejecuta el comando de reconstrucción con los datos preparados.
  - S10.6 usa los datos de ejemplo para su comprobación manual de búsqueda de enlaces cuando esa etapa esté disponible.
  - S6.1–S6.7 aportan las proyecciones, el procesamiento, la sincronización, las consultas, la reconstrucción y las pruebas automatizadas; sus entregables no se vuelven a implementar aquí.

## Dependencias y prerrequisitos

- S5.6 y S5.7 están implementadas y aceptadas; la aplicación prepara una identidad de desarrollo.
- S6.1–S6.7 están implementadas y aceptadas; existen las proyecciones y consultas para enlaces y colecciones, y la cobertura integrada está cerrada.
- S6.6 está implementada y aceptada; existe una herramienta de invocación manual de reconstrucción.
- La base SQLite local debe existir, tener aplicado el esquema y contener la identidad de desarrollo. La especificación no requiere crear o inicializar una base por su cuenta.
- S6.9 y S10.6 consumen los datos, pero su ejecución no es prerrequisito para validar ni cerrar S6.8.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → S6.8](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance) | Una base SQLite local inicializada con la identidad de desarrollo configurada. | Se ejecuta el script de datos de ejemplo. | Se crean datos sintéticos para enlaces y colecciones asociados a esa identidad y aptos para probar manualmente la búsqueda; no se crean etiquetas ni se modifican datos de otros usuarios. |
| CA02 | [mvp0-plan.md → S6.8 y S10.6](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l), [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [specifications.md → «Listados»](../../../context/specifications.md#listados) | El conjunto de ejemplo está cargado. | Se preparan los escenarios de búsqueda manual definidos por las fuentes. | Hay datos suficientes para distinguir búsquedas con acentos y formas Unicode equivalentes, varios términos obligatorios, operadores literales, coincidencias separadas por entidad, filtro por colección y un resultado paginado con más de una página. |
| CA03 | [mvp0-plan.md → S6.8](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [mvp0-plan.md → «Decisiones y alcance del plan»](../../../plans/mvp0-plan.md#5-decisiones-y-alcance-del-plan) | El script local y el repositorio. | Se inspecciona el control de versiones y se ejecuta el script por segunda vez sobre la misma base. | El script no está versionado y la repetición no duplica los datos de ejemplo. |
| CA04 | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [S6.6 — Reconstrucción manual de proyecciones FTS5](../s6.6-reconstruccion-proyecciones/spec.md) | Se prepara el conjunto local de ejemplo para búsqueda y reconstrucción posterior. | Se comprueba la carga y después se ejecuta la reconstrucción existente. | La carga deja las proyecciones sincronizadas con los ejemplos y, tras reconstruirlas desde las entidades, los ejemplos siguen disponibles; el script de carga no invoca la reconstrucción. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. La herramienta y la base local existentes permiten desarrollar el script dentro del alcance asignado. | [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [S6.6](../s6.6-reconstruccion-proyecciones/spec.md) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 20-X-2026 («Apruebo el plan y preparar tasks.md»).
- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre): aceptadas y cerradas por DLG el 20-X-2026 («Tenemos búsqueda»).
