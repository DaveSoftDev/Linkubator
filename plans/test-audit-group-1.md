# Auditoría individual del grupo 1: área pública y SEO

## Alcance y evidencia

Revisión estática del 2026-10-30 de preparación, invocación y aserciones de los métodos inventariados, contrastada con las reglas fuente y la implementación actual. No se modificaron tests ni producción en esta auditoría. El [inventario por caso](test-audit-group-1.csv) clasifica los **118 identificadores descubiertos** asignados al grupo; no es una muestra. La clasificación de una teoría se aplica a cada variante solo cuando comparte la justificación registrada. «Vigente» significa que la prueba aporta la garantía descrita, no que cubra toda su regla ni que sea inmune a cualquier defecto.

Descubrimiento global recompilado: **1.694 casos**, frente a los recuentos históricos del plan. Referencia inicial del checkout: `0ffbf41`. Al finalizar la lectura se detectaron cambios externos en entidades y repositorio privado; no se sobrescribieron. Los resultados de ejecución corresponden al binario construido durante esta auditoría, no constituyen validación de cambios posteriores.

Distribución:

- 25 casos de `PublicReadServiceTests`.
- 22 de `SqlitePublicReadRepositoryTests`.
- 56 de páginas públicas, landing y privacidad de `WebStartupTests`.
- 2 de ordenación pública de `SqlitePersistenceIntegrationTests`.
- 3 de `AliasExtensionsTests`.
- 10 variantes públicas de las teorías mixtas de tema y pie de página. Sus otras 6 variantes se ejecutaron como regresión, pero corresponden a presentación privada del grupo 9.

Asignación transversal: formato de dominio compartido, imágenes y favicons estáticos como recursos, sidebar privado, identidad, cabeceras de seguridad y eventos de logging permanecen en sus grupos principales 7 o 9. Aquí se incluyen sus contratos de HTML público cuando los comprueba un test público. No se excluyen casos por fallar ni por necesitar refuerzo.

Validación realizada en Debug, sin filtros de resultados ni omitidos:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --list-tests -v quiet
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter 'FullyQualifiedName~PublicReadServiceTests|FullyQualifiedName~SqlitePublicReadRepositoryTests|FullyQualifiedName~WebStartupTests.Public|FullyQualifiedName~WebStartupTests.UserPage|FullyQualifiedName~WebStartupTests.HomePage|FullyQualifiedName~WebStartupTests.PrivacyPage|FullyQualifiedName~SqlitePersistenceIntegrationTests.GetPublic|FullyQualifiedName~AliasExtensionsTests' -v minimal
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter 'FullyQualifiedName~WebStartupTests.PrivateTheme_IsLoadedOnlyByPrivatePages|FullyQualifiedName~WebStartupTests.Pages_WithSharedPresentation_RenderBrandFaviconsAndPrivacyLink' -v minimal
```

Resultados: **108/108** del núcleo y **16/16** de presentación mixta, de los que 10 pertenecen al grupo. No se ejecutó otra suite global: no hubo cambios de código. No se hicieron mutaciones del código ni se automatizó el navegador.

## Fuentes y trazabilidad

El inventario enlaza para cada caso su archivo, método, estado, garantía y fuente. Las fuentes siguientes se usan sin repetir sus reglas:

| Referencia | Fuente | Evidencia y límite |
|---|---|---|
| Aplicación | [architecture.md → «Application»](../context/architecture.md#application) | Validación de entrada, propagación de resultados, traducción de excepciones y no accesibilidad en `PublicReadServiceTests`. Ver A1 y A2. |
| Público/privado | [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado) | Combinaciones de privacidad e inconsistencias persistidas en repositorio y HTTP; vacíos y ausencia de datos privados. Ver A4 y H3. |
| Usuario | [specifications.md → «Página de usuario»](../context/specifications.md#página-de-usuario) | Metadatos, vacíos, contenido y límite reciente en HTTP; selección de colecciones y enlaces en SQLite. Ver A5 y H1. |
| Colecciones | [specifications.md → «Página de colecciones»](../context/specifications.md#página-de-colecciones) | Listado, metadatos, vacíos y alias antiguos en HTTP; aislamiento y orden en SQLite. Ver H2. |
| Colección | [specifications.md → «Página de colección»](../context/specifications.md#página-de-colección) | Enlaces, campos opcionales, metadatos y `404`; fallback y orden en SQLite. Ver A6 y H4. |
| Orden | [specifications.md → «Listados»](../context/specifications.md#listados) | Orden reciente, colación y desempate por identificador con fechas iguales; paginación SQLite. Falta separar fecha de identificador en recientes: H1. |
| URLs | [specifications.md → «Rutas públicas»](../context/specifications.md#rutas-públicas) | Redirecciones y canonical, y ausencia de datos personales en páginas con contenido. Ver A3 y H4. |
| Paginación | [specifications.md → «Paginación de las páginas públicas»](../context/specifications.md#paginación-de-las-páginas-públicas) | Diez variantes HTTP y límites SQLite. Ver A6 y H5. |
| Presentación | [specifications.md → «Páginas públicas»](../context/specifications.md#páginas-públicas) | Cabeceras, alias, navegación, footer, idioma y encabezado único. Ver A3. |
| Landing | [specifications.md → «Landing»](../context/specifications.md#landing) | Metadatos, acciones y preview; el smoke test aislado es redundante: D1. |
| Revisión manual | [roadmap.md → «Checklist de calidad del MVP0»](../context/roadmap.md#checklist-de-calidad-del-mvp0) | Alturas, orden visual, contraste, foco e interacción siguen siendo revisión humana; no se consideran acreditados por coincidencias de HTML ni por esta auditoría. |

## Hallazgos de revisión individual

Las referencias A agrupan refuerzos de pruebas existentes; H identifica escenarios sin prueba discriminante en este grupo. Se cruzan con los casos del CSV; varios casos pueden compartir una misma acción.

| ID | Prioridad | Hallazgo y evidencia | Acción propuesta |
|---|---|---|---|
| A1 | Media | Los éxitos `GetUserPage_WhenUserExists_ReturnsPublicPage`, `GetCollectionsPage_WhenCollectionsExist_ReturnsSummaries` y `GetCollectionPage_WhenCollectionExists_ReturnsKnownPage` verifican cardinalidad y algunos campos, no todos los valores y el orden devueltos por el fake. | Comparar el DTO o la secuencia esperada completa, conservando las comprobaciones de argumentos normalizados. |
| A2 | Media | Los dos tests de excepciones de `GetCollectionPageCount` provocan el error en la consulta preliminar `GetCollectionPage`; el fake de recuento nunca lanza. Son válidos para esa fase, no para el fallo del recuento. | Aclarar nombres y añadir fallos específicos de la segunda llamada, con éxito de la consulta preliminar; verificar traducción o propagación según la excepción. |
| A3 | Media | `PublicHeaders_RenderSameLoginAndRegistrationActionsAsHome` compara acciones con la landing, pero no exige estado final ni destinos exactos de las dos acciones. Las pruebas de metadatos buscan encabezados y su texto por separado. | Exigir estado esperado de cada ruta y destinos en el bloque de acciones; vincular el texto exacto a `h1`/`h2` y no a cualquier parte del HTML. |
| A4 | Media | `GetCollectionPageCount_WhenCollectionHasPublicAndPrivateLinks_CountsOnlyPublicLinks` prepara un enlace público y uno privado: contarlos ambos sigue dando una sola página. Las variantes de límites 0, 16 y 32 sí detectan esa regresión en otra prueba. | Reforzar o consolidar con el caso de frontera ya discriminante; no afirmar que este caso aislado demuestra el filtro. |
| A5 | Media | `UserPage_WithManyPublicLinks_RendersSixMostRecentCards` añade todos los recientes a una sola colección; el caso SQLite de seis recientes también usa solo una. Demuestran límite y orden local, no agregación entre colecciones ni independencia del límite de colecciones del perfil. | Añadir escenario con varias colecciones y un reciente en una colección que queda fuera de las cinco mostradas. |
| A6 | Media | Las diez variantes HTTP de paginación exigen cantidad y un título presente/otro ausente; no toda la secuencia. No verifican los destinos y estados de Anterior/Siguiente. | Comparar secuencia exacta y navegación; conservar los parámetros repetidos y canonical. |
| A7 | Baja | `UserPage_ReturnsNotFoundForUnknownAndFormerAliases` no comprueba que el perfil existiera antes del cambio. El test equivalente de colecciones sí lo hace. | Comprobar acceso previo y acceso por el alias nuevo para distinguir renombrado de indisponibilidad general. |
| A8 | Baja | `PublicCollectionsPage_ReturnsNotFoundForUnknownAlias` permite redirecciones automáticas: solo exige el estado final. | Desactivar seguimiento y exigir ausencia de `Location`, como ya hacen los demás casos `404`. |
| D1 | Baja | `HomePageReturnsSuccess` solo exige éxito en la misma ruta y con la misma factoría que `HomePage_Renders_PublicLandingMetadataAndActions`, que exige `200` y además contenido. Es un duplicado demostrado de esa garantía. | Eliminar el smoke test si no se le asigna un propósito distinto; no eliminar pruebas de otras capas por simple parecido. |
| H1 | Media | En los recientes SQLite, las fechas crecientes están alineadas con los identificadores crecientes, salvo empates que sí prueban `Id`. Ordenar solo por `Id DESC` produciría el resultado actual. | Crear fecha e identificador en orden contrario, tanto para enlaces como para colecciones recientes. |
| H2 | Media | No hay un listado público con más de una página privada de tamaño que demuestre que devuelve todas las colecciones, sin límite accidental. El test de orden público existente usa tres filas. | Preparar más de 16 colecciones publicables y comparar la secuencia completa; mezclar vacías/privadas para comprobar filtrado antes de límites. |
| H3 | Media | El aislamiento del recuento público no se comprueba con dos usuarios que compartan slug y tengan cantidades de páginas distintas. Los casos de perfil/listado/contenido sí separan usuarios. | Añadir recuentos discriminantes por alias y slug compartido. |
| H4 | Media | El `404` de slug antiguo se simula cambiando SQL. No hay recorrido que renombre a un nombre con slug equivalente y compruebe que la URL pública sigue disponible con el nombre nuevo. | Añadir integración de renombrado y lectura pública para ambos casos de slug, sin repetir las pruebas unitarias del generador. |
| H5 | Baja | No hay casos públicos explícitos para página negativa, espacios, dígitos no ASCII y página interior de una colección con tres páginas. El analizador privado no sustituye el parser público. | Completar la tabla de entradas públicas y la navegación de una página intermedia. |

## Decisión y cierre

Estado inicial: **inventario individual y diagnóstico realizados**. Las acciones se aprobaron y ejecutaron posteriormente; el cierre actualizado se registra a continuación. Clasificación: **87 casos vigentes, 30 a reforzar y 1 duplicado demostrado**; total **118**. Los cinco huecos H1–H5 se registran aparte porque no son casos ejecutables existentes. El recuento se obtiene del CSV y debe conservar la igualdad con los 118 casos inventariados. No se declara cierre del grupo: deben decidirse y resolverse los refuerzos, el duplicado y los huecos, y volver a descubrir/ejecutar los casos añadidos o eliminados. No se encontraron expectativas necesariamente obsoletas que deban cambiar una regla del producto.

Las pruebas de página vacía de Application siguen siendo vigentes como contrato interno: el `404` pertenece al HTTP y no se debe imponer ciegamente a todas las capas. Las pruebas con estado persistido inconsistente son defensas explícitas del filtrado público, no ejemplos de estado válido de dominio.


## Resolución y cierre automatizado

Acciones aprobadas y completadas el 2026-10-30, sin cambios de producci?n de este lote:

| Hallazgos | Resolución verificada |
|---|---|
| A1 | Comparaci?n completa de DTOs y secuencia de res?menes, conservando argumentos normalizados. |
| A2 | Nombres expl?citos para fallos de consulta preliminar; dos casos nuevos fuerzan el fallo de recuento y verifican c?digo o identidad de excepci?n, adem?s de llamada efectiva al recuento. |
| A3 | Estados por ruta y destinos de Login/Registro exigidos en el bloque de acciones; textos vinculados a los elementos h1/h2. |
| A4 | Frontera de 16 enlaces p?blicos m?s uno privado: contarlo indebidamente cambiar?a el n?mero de páginas. |
| A5 | Perfil HTTP con seis colecciones: el enlace m?s reciente pertenece a la colecci?n excluida del resumen de cinco y sigue apareciendo en la secuencia exacta de recientes. |
| A6 | Secuencia completa en las variantes de paginaci?n; destino habilitado y extremo deshabilitado comprobados. |
| A7?A8 | Perfil comprobado antes/despu?s del cambio de alias y rechazo sin redirecciones en listado de alias inexistente. |
| D1 | Eliminado ?nicamente `HomePageReturnsSuccess`; permanece la prueba de landing que exige 200 y metadatos. |
| H1 | Fechas de inserciones anteriores elevadas por encima de las posteriores en ambos tests de recientes; se conservan empates por identificador y expectativas exactas. |
| H2 | Listado completo de 17 colecciones publicables con colecci?n vac?a y privada adicionales excluidas. |
| H3 | Mismo slug en dos usuarios con 1 y 17 enlaces; recuentos de páginas diferenciados por alias. |
| H4 | Renombrado por POST real con antiforgery: nombre con slug equivalente mantiene URL; nombre con slug distinto retira URL anterior y sirve el nombre/canonical nuevos. |
| H5 | Entradas negativas, espacios y d?gitos no ASCII; página intermedia de tres con secuencia exacta, canonical y ambos destinos de navegaci?n. |

El CSV actual contiene **129 casos ?nicos vigentes**, incluidos los 10 casos p?blicos de presentaci?n mixta. Conserva `EstadoInicial` para diferenciar refuerzos y altas. Los 118 originales menos el duplicado retirado m?s 12 variantes/casos nuevos producen los 129 actuales. El duplicado eliminado permanece documentado en D1, no se inventa un identificador ejecutable para ?l.

Validaci?n Debug: **119/119** casos del n?cleo p?blico; la suite global incluye adem?s las teor?as mixtas y pasa **1.708/1.708**, sin fallidos ni omitidos. Comando global:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore -v minimal
```

Se repiti? el descubrimiento despu?s de compilar y se verific? el inventario. Los fallos iniciales de los tests añadidos debidos a literales Unicode de edición se corrigieron antes de esta validaci?n; no se atribuyen a producci?n. Otros cambios externos del checkout no se sobrescribieron ni se atribuyen a esta tarea.

**Cerrada la revisión individual automatizada del alcance del grupo 1** y sus acciones A1?A8, D1 y H1?H5. No equivale a demostrar ausencia universal de defectos ni a completar las revisiones humanas visuales y accesibles de la checklist: estas permanecen en S12. Cualquier cambio posterior de reglas o implementaci?n exige reevaluar los casos afectados.
