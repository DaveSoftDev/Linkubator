# Auditoría individual del grupo 5: búsqueda, filtros y listados

## Alcance y evidencia

Diagnóstico individual del 2026-10-30: **180 casos ejecutables de 111 métodos**, clasificados en el [inventario por caso](test-audit-group-5.csv): **151 vigentes, 28 a reforzar y 1 duplicado demostrado**. Distribución: Infrastructure 85, HTTP/integración 59, Web 18 y Application 18. Se revisaron preparación, invocación y aserciones de los métodos seleccionados, no solo sus nombres.

La selección excluye identificadores ya inventariados en los grupos 1–4. Incluye construcción y normalización de consulta, analizador privado, FTS5, reconstrucción/migraciones de proyecciones, listados de aplicación, métodos de búsqueda/paginación de repositorios, listado HTTP de enlaces y variantes de búsqueda de colecciones. El orden privado con desempate de nombres de `SqlitePersistenceIntegrationTests` se asigna aquí. Presentación general, dashboard reciente y acciones CRUD ya asignadas o reservadas a otros grupos no se consideran revisadas por esta selección.

«Vigente» acredita la garantía observada en ese caso, no cobertura completa de toda su regla. Los errores de atomicidad incluidos prueban transacciones preparadas por el test; no implican que cada repositorio abra su propia transacción. Las funciones de etiquetas reservadas a etapas futuras no se consideran huecos del alcance actual.

Se recompiló Debug para descubrir la suite y se ejecutó el filtro exacto generado del CSV: **180/180 superados**, sin omitidos. Este paso solo modifica documentación; no se alteraron tests ni producción ni se repitió la suite global. El checkout contiene cambios previos de los grupos anteriores y refactors concurrentes; los recuentos históricos no describen necesariamente ese estado.

```powershell
$cases = Import-Csv plans\test-audit-group-5.csv
$methods = $cases.Caso | ForEach-Object { ($_ -split '\(')[0] } | Sort-Object -Unique
$filter = ($methods | ForEach-Object { 'FullyQualifiedName=' + $_ }) -join '|'
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter $filter -v minimal
```

## Fuentes y trazabilidad

| Fuente única | Evidencia y límite |
|---|---|
| [specifications.md → «Búsqueda y filtros»](../context/specifications.md#búsqueda-y-filtros) | Términos obligatorios, prefijos, ausencia de términos, NFC, campos y entidades aislados. La salida exacta del builder no acredita toda la ejecución FTS5: H1–H2. |
| [specifications.md → «Listados»](../context/specifications.md#listados) | Secuencias y tamaños de página, relevancia y desempates, orden alfabético. Recuentos pequeños pueden ocultar filtros omitidos: A2. |
| [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas) | Parámetros repetidos, ignorados, inválidos, fuera de rango y conservación de contexto. Fronteras Unicode y callback: H3. |
| [architecture.md → «FTS5»](../context/architecture.md#fts5) | Esquema, tokenizador, proyecciones separadas, escritura y reconstrucción. SQL manual con filtro no equivale a probar filtrado del repositorio: A6. |
| [architecture.md → «Persistencia»](../context/architecture.md#persistencia) | Fallos inyectados y conservación de proyecciones en rollback; actualización de nombre de colección en proyección de enlaces. |
| [roadmap.md → «Checklist de calidad del MVP0»](../context/roadmap.md#checklist-de-calidad-del-mvp0) | Cobertura automatizada contrastada con checklist; no acredita interacción visual/accesible humana ni búsqueda de capacidades futuras. |

## Refuerzos y duplicado

| ID | Prioridad | Hallazgo observado | Acción propuesta |
|---|---|---|---|
| A1 | Media | Los tests `GetPageByUser_WhenPageIsIntMaxValue_ReturnsEmptyPageWithoutOverflow` de ambos repositorios usan bases sin filas. Un offset que desbordara y devolviera la primera página también encontraría cero filas. | Preparar filas reales y exigir vacío para el número extremo; conservar el caso de página normal como control positivo. |
| A2 | Media | Los recuentos `CountsOnlyItsLinks`, `CountsOnlyMatchesAndCollectionFilter` y `CountsOnlyOwnedNameMatches` preparan pocos elementos: incluso contando indebidamente todas las filas el resultado seguiría siendo una página. | Cruzar la frontera de página con filas no coincidentes, ajenas o de otra colección y exigir el recuento filtrado; cubrir cada filtro de forma discriminante. |
| A3 | Media | `GetLinks_WithOversizedSearch_ShowsErrorWithoutResults` no prepara enlaces y no exige estado HTTP. La ausencia de tarjetas es inevitable incluso sin suprimir resultados. | Añadir datos que se mostrarían sin error, comprobar estado, mensaje y ausencia de resultados/selector. |
| A4 | Baja | `Analyze_WhenPageIsMissingOrEmpty_UsesDefaultPage` solo prepara clave vacía; `Analyze_WhenSearchHasText_TrimsExtremesAndPreservesInternalSpaces` solo usa un espacio interior. Sus nombres prometen variantes que no discriminan. | Parametrizar ausencia y vacío; preparar varios espacios internos y exigir conservación exacta. |
| A5 | Media | `Rebuild_WhenProjectedTextIsUnicodeDecomposed_NormalizesEveryFieldButPreservesEntityValues` usa el identificador de colección en el UPDATE de Link, funcionando por coincidencia con el identificador del enlace. Tampoco modifica/comprueba la descripción de colección pese a prometer todos los campos. | Separar identificadores, hacerlos distintos como precondición y actualizar/comprobar cada campo propio y proyectado, incluida descripción de colección. |
| A6 | Baja | `ApplyPendingMigrations_WhenProjectionRowsHaveDifferentUsers_FiltersByUser` solo inserta una fila y ejecuta consultas manuales con `WHERE UserId`. Comprueba semántica SQL, no que el código de búsqueda filtre usuarios. | Renombrar como contrato de esquema/columna y, si se conserva, preparar dos usuarios; mantener la garantía real de aislamiento en repositorios e integración, no atribuirla al SQL manual. |
| A7 | Media | `ListCollectionsPage_ReturnsItemsAndPageCountForCurrentUser` y `ListLinks_WhenCollectionIdBelongsToUser_ReturnsLinks` solo comparan cantidad y algunos campos. Sustituir DTOs por otros del mismo tamaño/colección podría pasar inadvertido. | Comparar salida y secuencia completas contra los objetos preparados; preservar comprobación de parámetros y ámbito. |
| A8 | Media | Las variantes HTTP de páginas inválidas/fuera de rango comparan contra otra respuesta del mismo endpoint y un tamaño esperado. Un error común en el contenido de ambas respuestas puede pasar. | Añadir secuencia esperada independiente para cada página efectiva, manteniendo la equivalencia de respuestas y la conservación de filtros. |
| D1 | Baja | `ListLinks_WhenCollectionIdDoesNotBelongToUser_ReturnsResourceNotAccessible` y `ListLinks_WhenCollectionDoesNotBelongToUser_ReturnsResourceNotAccessible` ejercitan la misma operación con colección ajena; los identificadores distintos no añaden un escenario funcional. | Consolidar en el caso que conserve categoría, código y ausencia de resultados/consulta de listado. |

Los 28 casos a reforzar incluyen las variantes de teorías que comparten las aserciones débiles; no son fallos actuales de producción. No se eliminan casos de normalización, proyección y consulta por compartir el mismo texto: prueban puntos de entrada distintos.

## Huecos de escenarios

| ID | Prioridad | Escenario sin prueba discriminante en este grupo | Acción propuesta |
|---|---|---|---|
| H1 | Media | El builder comprueba `AND OR NOT NEAR` y símbolos como texto escapado, pero no hay ejecución real FTS5 que demuestre que esos términos no actúan como operadores ni alteran obligatoriedad. | Datos coincidentes y parciales, consulta con palabras de operador y puntuación, y secuencias exactas en enlaces y colecciones sin errores de sintaxis. |
| H2 | Media | Los tests integrados comprueban NFC de campos almacenados con consulta precompuesta y equivalencia latina sin acentos, pero no una matriz completa de consulta descompuesta/precompuesta frente a ambas formas almacenadas ni el reparto de términos entre campos propios de enlace. | Variantes NFC en consulta/campos y términos distribuidos entre URL, título, descripción y colección, con señuelos parciales. No exigir equivalencias de diacríticos o mayúsculas fuera de cobertura del tokenizador. |
| H3 | Media | Se prueba rechazo de búsqueda de máximo + 1 puntos de código, pero no aceptación exacta con caracteres fuera del BMP y espacios exteriores. Los parámetros no aplicables no demuestran que se evite invocar el callback de accesibilidad. | Límite exacto Unicode y recorte, más callback que falle si se consulta para parámetro no aplicable o vacío. |
| H4 | Media | La prueba de ranking usa más repeticiones del término y orden de fecha que favorece al otro enlace, pero no una fecha fijada explícitamente para demostrar precedencia de relevancia; depende del momento de siembra. | Fijar fechas opuestas al ranking y comparar secuencia exacta, separando esta garantía de desempates por fecha/Id ya presentes. |
| H5 | Baja | No se ha localizado un recorrido HTTP de consulta sin términos con filtro de colección y otra colección con enlaces, que demuestre equivalencia con no buscar y conservación del filtro. El repositorio sí lo prueba. | Comparar contenido y navegación HTTP para `q` solo símbolos, blanco y ausente, con datos que discriminen el filtro. |

## Estado

**Diagnóstico inicial conservado como historial; acciones aprobadas y resueltas según el cierre siguiente.** El CSV contiene 180 identificadores únicos sin solapamientos con los inventarios de grupos 1–4. Los cinco huecos se registran aparte, no como casos ejecutables existentes. No se declara cierre: antes deben resolverse los refuerzos y el duplicado, cubrir o decidir los huecos, redescubrir el inventario y ejecutar los tests afectados y la suite global tras los cambios.


## Cierre automatizado

El 2026-10-30 se completaron A1-A8, D1 y H1-H5:

- Bases con datos/control positivo para offsets extremos; filas excluidas suficientes para cruzar pagina si se omite cualquiera de los filtros de recuento.
- Busqueda excesiva con resultados reales disponibles; ausencia/vacio de pagina y varios espacios internos diferenciados.
- Reconstruccion con identificadores distintos y descripcion de coleccion original/proyectada comprobada; prueba de UserId de esquema renombrada sin atribuirle aislamiento de aplicacion.
- Secuencias/DTOs completos de Application y paginas HTTP esperadas independientes del propio endpoint; duplicado de coleccion ajena eliminado.
- FTS5 real con palabras de operadores y puntuacion, matriz NFC de consulta/almacenamiento y terminos distribuidos entre URL, titulo, descripcion y coleccion.
- Limite Unicode exacto con recorte; callback de acceso no invocado cuando no aplica; fechas fijas opuestas a relevancia; consultas HTTP ausentes, blancas o simbolicas conservan coleccion y paginacion.

Inventario final: **193 casos vigentes unicos**, manteniendo `EstadoInicial`: 180 iniciales menos un duplicado mas catorce casos/variantes nuevos. Verificacion Debug: **193/193** en el filtro exacto del CSV y **1.757/1.757** en suite global sin omitidos, con `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore -v minimal`.

Solo se modificaron tests y documentos de este lote, sin cambios de produccion. Los fallos durante la preparacion de nuevos datos (URL duplicada y selector HTML) y la incidencia de codificacion de un archivo editado se corrigieron antes de la validacion final, sin atribuirlos al producto.

**Cerrada la revision individual automatizada del grupo 5.** No acredita interaccion visual/accesible humana ni capacidades futuras. Las modificaciones posteriores exigen reevaluar los casos afectados.
