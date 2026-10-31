# Auditoría individual del grupo 6: reglas de entrada

## Alcance y diagnóstico inicial

**Estado actual:** revisión individual automatizada cerrada. Las correcciones aprobadas permanecen verificadas: **651/651 casos del grupo** y **1.784/1.784 de la suite global** superados con código recompilado. El inventario conserva 651 identificadores del runner, comprobados nuevamente. Los bloqueos y fallos descritos a continuación son históricos.

Diagnóstico estático inicial del 2026-10-30 de políticas de texto, ASCII, alias, slug, email, autoridad, URL de enlace, imagen y normalización; validadores de aplicación y sus pruebas de configuración. Se leyeron preparación, invocación y aserciones de los 172 métodos inventariados. Las invariantes y pruebas de campos de entidades ya asignadas al grupo 2 y los recorridos HTTP de grupos 3–4 no vuelven a asignarse aquí.

El inventario inicial contenía **624 identificadores provisionales**: **588 vigentes, 14 a reforzar y 22 variantes duplicadas demostradas**. Las 22 variantes proceden de dos teorías de ejemplos de imagen, no de 22 métodos. No hay solapamientos con inventarios 1–5. La captura textual provisional pierde distinciones de mayúsculas y contiene saltos de línea en algunos argumentos: tiene 624 registros pero solo 606 etiquetas `Caso` distintas. La columna `Registro` distingue filas de trabajo, no sustituye identificadores del runner. Debe regenerarse el inventario con argumentos Unicode/escapes preservados antes de afirmar un recuento único definitivo. «Vigente» significa garantía estática observada, no validación del binario contra los últimos cambios.

**Durante el diagnóstico inicial la compilación estaba bloqueada por CS0266** en `PublicReadService` y `DashboardService`, por conversiones de `Result` a `Result<T>` en cambios concurrentes ajenos a este diagnóstico. Dos intentos de compilación no alcanzaron la ejecución. El descubrimiento provisional procede del último binario Debug disponible, de **2026-10-30 14:40:20**, mediante `--no-build --list-tests`; todos sus métodos seleccionados existen en el código leído, pero no se puede acreditar su vigencia ejecutable hasta recompilar. No se ejecutaron tests de un binario anterior como validación del código actual.

En el diagnóstico inicial solo se modificó documentación. No se restauraron conversiones ni se interfirió en el refactor concurrente. El redescubrimiento y la ejecución posterior se documentan al final; las tablas siguientes conservan los hallazgos aprobados.

## Trazabilidad

| Fuente única | Evidencia y límite |
|---|---|
| [specifications.md → «Textos introducidos por el usuario»](../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../context/specifications.md#longitudes-máximas) | Recorte, vacío, Unicode y límites de entrada. No se exige implementar truncado de scraper futuro. Helpers públicos aún necesitan matriz directa: H1. |
| [specifications.md → «Transformación común a ASCII»](../context/specifications.md#transformación-común-a-ascii) | Conversión, rechazo, símbolos, Unicode y cultura mediante valores exactos. La política no consulta disponibilidad de cuentas. |
| [specifications.md → «Generación del alias»](../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../context/specifications.md#generación-de-slugs) | Rangos y palabras reservadas, expansión y equivalencia. Las colisiones persistidas tienen evidencia en grupos 1–3; repetir Generate no prueba integración con etiqueta. |
| [specifications.md → «Email»](../context/specifications.md#email) | Valores normalizados, patrón, longitudes y rechazo. No equivale a comprobar titularidad ni implementar correo. |
| [specifications.md → «Ajuste de URL»](../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../context/specifications.md#ajuste-de-image) y [«Validación»](../context/specifications.md#validación) | Esquema, autoridad, host, puerto, credenciales, base relativa y conservación del resultado. Se mantienen separadas validación y ajuste: un caso repetido entre APIs distintas no es duplicado por sí mismo. |
| [specifications.md → «Normalización para duplicados»](../context/specifications.md#normalización-para-duplicados) | Claves exactas, decodificación única, consulta repetida, Unicode y cultura. La normalización recibe entrada ajustada/validada; casos de `mailto:` o sustitutos sueltos son pruebas técnicas fuera de esa precondición, no URLs aceptadas por el producto. |
| [architecture.md → «Application»](../context/architecture.md#application) | Código y campo de validación y validadores configurables. No se atribuye a LinkUpdateValidator la validación de metadatos que el dominio aplica en otra fase. |

## Refuerzos y limpieza

| ID | Prioridad | Hallazgo | Acción propuesta |
|---|---|---|---|
| A1 | Baja | `GenerateProducesTheSameSlugForTheSameNameUsedAsCollectionOrTag` ejecuta Generate dos veces sin origen; `GenerateDoesNotConsultTheAvailabilityOfTheAlias` repite una llamada sin dependencia de disponibilidad. Demuestran determinismo, no los flujos que sugieren los nombres. | Renombrar a la garantía real o consolidar conservando resultado exacto; no introducir entidades de etiquetas futuras para justificar el nombre. |
| A2 | Baja | `ValidateDoesNotTransformTheAcceptedUrl` compara una variable string local que nunca se reasigna tras llamar a una API void. Esa igualdad no demuestra almacenamiento sin transformación. | Limitar el test a aceptación y apoyarse en los round-trips exactos ya inventariados para conservación; no atribuir garantía imposible a una string inmutable pasada por valor. |
| A3 | Baja | `ValidateRejectsNullAsArgumentNull` exige realmente `NotValidInputException`; `NormalizeDoesNotRevalidateAnAlreadyAdjustedUrl` solo exige rechazo de texto que no es URL. | Renombrar según excepción/escenario reales sin cambiar comportamiento ni reglas del producto. |
| A4 | Baja | `NormalizeDoesNotLeakAProviderExceptionForALoneSurrogate` exige solo ausencia de excepción sobre entrada que no superaría el ajuste previo. | Documentar precondición artificial y fijar resultado si se mantiene como contrato técnico; no presentar como aceptación de entrada del usuario. |
| A5 | Media | La prueba de longitud Unicode de Image solo comprueba tamaño, y la expansión de símbolos del slug solo exige longitud 149. Resultados incorrectos del mismo tamaño pasarían. | Exigir valores completos esperados y conservar la frontera de exceso; distinguir longitud final codificada de longitud del input. |
| A6 | Baja | Las teorías de exceso de metadatos verifican campo y código pero no categoría; el conjunto de errores únicos se comprueba parcialmente. | Completar categoría de dominio y consistencia de error único para ambas entradas, sin duplicar límites HTTP de otros grupos. |
| A7 | Media | `CultureScope` de normalización guarda CurrentCulture y restaura también CurrentUICulture desde ese mismo valor. Si ambas culturas eran distintas, la prueba altera el contexto de quien la ejecuta. | Guardar y restaurar ambas independientemente; añadir control con culturas iniciales diferentes sin modificar estado global de otros tests. |
| D1 | Baja | Las 13 variantes de `ImageUrlExamplesMatchTheSpecificationTable` repiten input/base efectiva/resultado de `AdjustResolvesUrlsUsingTheConfiguredBase`. Las 9 de rechazo de ejemplos están cubiertas por las teorías de hosts y bases relativas de ImageUrlPolicyTests con la misma API y excepción. | Retirar la clase duplicada tras comprobar equivalencia por variante y conservar trazabilidad de tabla en la clase de políticas. No eliminar pruebas entre LinkUrlPolicy y UrlValidationPolicy solo por compartir entrada. |

## Huecos

| ID | Prioridad | Escenario | Acción propuesta |
|---|---|---|---|
| H1 | Media | UserTextPolicyTests no ejercita directamente EnsureRequiredText, EnsureOptionalMaximumLength y EnsureWithinRange; entidades cubren parte de ello, no toda combinación de límite, recorte y Unicode. | Matriz pequeña de requeridos/opcionales, máximo y mínimo exactos y fuera de rango, suplementarios y marcas combinantes; exigir valor o excepción concreta. |
| H2 | Media | URL de enlace acepta un suplementario en máximo, pero falta su exceso inmediato con el mismo prefijo válido; imagen necesita contrastar valor exacto tras codificar referencia relativa y cruzar el límite por expansión. | Casos máximo/máximo + 1 con autoridad válida y entrada restante igual, y resolución relativa que crece al codificarse; no atribuir el rechazo a host inválido. |
| H3 | Media | Las claves cubren un parámetro UTM y ejemplos reales con varios marcadores, pero no discriminan individualmente toda la lista cerrada de marketing frente a nombres parecidos legítimos. | Teoría por parámetro de la fuente y mayúsculas, con control que conserve un nombre parecido y clave exacta esperada. |
| H4 | Baja | ValidationCoverageTests no tiene caso propio de CollectionUpdateValidator con solicitud nula, identificador inválido, nombre inválido y petición válida; la configuración sí lo instancia. | Casos de validación directa con campos válidos en el resto de la petición y metadatos de error exactos, sin presumir qué validaciones pertenecen al dominio. |

## Correcciones aplicadas

| Hallazgo | Evidencia de resolución |
|---|---|
| A1–A3 | Nombres ajustados a determinismo, aceptación y rechazo realmente comprobados; retirada la igualdad tautológica de la variable local. |
| A4 | Claves exactas para sustitutos sueltos y precondición artificial explícita; no se atribuye aceptación de producto. |
| A5 | Resultado completo para expansión del slug e imagen Unicode, además de longitud. |
| A6 | Categoría de dominio en ambas teorías de exceso de metadatos. |
| A7 | Restauración independiente de ambas culturas y control con culturas originales distintas. |
| D1 | Retirada la clase de ejemplos de imagen: 22 variantes equivalentes permanecen cubiertas en la clase de políticas. |
| H1 | Helpers de texto: requerido, opcional, vacío, recorte, fronteras, suplementarios y marcas combinantes. |
| H2 | Exceso inmediato de URL con suplementario; imagen relativa con expansión codificada y frontera final exacta. |
| H3 | 23 variantes individuales de parámetros de marketing en mayúsculas, conservando un nombre parecido y un parámetro de control con clave exacta. |
| H4 | Actualización de colección: petición nula, id inválido, nombre vacío y petición válida; código, campo y categoría exactos. |

Este lote modifica tests y documentación, no código de producción. El [inventario definitivo](test-audit-group-6.csv) contiene **651 casos de 184 métodos**: 588 conservados, 14 reforzados y 49 nuevos; se retiraron 22 variantes duplicadas. Cada caso conserva `EstadoInicial` y añade `IdRunner`, resultado y descubrimiento actual. Los argumentos Unicode, mayúsculas y escapes proceden del XML TRX, no de la captura textual provisional. No hay solapamientos por archivo/método con grupos 1–5.

## Verificación y estado

Ejecución Debug recompilada con `dotnet test`, sin restaurar dependencias. Selección focalizada:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter 'FullyQualifiedName~Domain.Policies|FullyQualifiedName~LinkCreateValidatorTests|FullyQualifiedName~ValidationCoverageTests' -v minimal
```

Resultado: **651/651 superados**, ninguno omitido. La selección coincide con los 651 identificadores únicos del inventario; todos tienen resultado `Passed`.

Regresión global, recompilando el estado final de los tests:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore -v quiet
```

Resultado histórico previo a los cambios concurrentes: **1.665/1.784 superados; 119 fallidos**, ninguno omitido. Se repitió tras una primera ejecución de 1.781 casos con los mismos 119 fallos. Todos los fallos estaban fuera del grupo 6:

| Clase | Fallos |
|---|---:|
| WebStartupTests | 69 |
| LinkCreationIntegrationTests | 15 |
| PublicReadServiceTests | 12 |
| LinkManagementIntegrationTests | 11 |
| LinkListingIntegrationTests | 8 |
| LoggingTests | 2 |
| DashboardServiceTests | 2 |

En aquella ejecución las lecturas públicas y dashboard fallaban con `InvalidOperationException: This service does not have a unit-of-work factory`; varios recorridos HTTP devolvían `InternalServerError`. Los cambios concurrentes en servicios y su base quedaron fuera de este lote.

### Revalidación y cierre

Tras los cambios concurrentes, se recompiló y ejecutó la suite completa con `--configuration Debug --no-restore -v minimal`: **1.784/1.784 superados**, ninguno omitido. La selección focalizada se repitió sobre ese binario: **651/651 superados**; sus 651 identificadores coinciden exactamente con el inventario, sin altas ni bajas.

**Revisión individual automatizada del grupo cerrada.** Las capacidades futuras y revisiones humanas siguen fuera de esta evidencia.
