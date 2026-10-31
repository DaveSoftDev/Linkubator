# Auditoría individual del grupo 4: gestión privada de enlaces

## Alcance y evidencia

Revisión del 2026-10-30 de **188 casos de 103 métodos**, recogidos en el [inventario individual](test-audit-group-4.csv): **173 vigentes, 14 a reforzar y 1 duplicado demostrado**. Distribución: Application 48, Infrastructure 8 y HTTP 132. Cada variante descubierta conserva su identificador, método, fuente y justificación. La garantía es la efectivamente exigida por el test, no toda la que podría sugerir su nombre.

No se reasignan casos de los grupos 1–3. Los tests de propiedad y privacidad inventariados en el grupo 2 son evidencia complementaria, no nuevas filas de este grupo. Búsqueda, paginación, proyecciones Unicode/atomicidad, helpers de errores y presentación general del dashboard/sidebar permanecen en los grupos 5–9. La creación rápida y sus recorridos de retorno sí pertenecen al CRUD de enlaces aquí seleccionado. Los escenarios de URL/imágenes se revisan por su integración con la operación, sin repetir el inventario de políticas del grupo 6.

El checkout tiene cambios simultáneos en servicios, repositorios y formularios privados. Se recompiló Debug para descubrir los casos actuales y se ejecutó exactamente la selección del CSV: **188/188 superados**, sin omitidos. Este diagnóstico solo modifica documentación; no ejecuta otra suite global ni altera tests o producción.

```powershell
$cases = Import-Csv plans\test-audit-group-4.csv
$methods = $cases.Caso | ForEach-Object { ($_ -split '\(')[0] } | Sort-Object -Unique
$filter = ($methods | ForEach-Object { 'FullyQualifiedName=' + $_ }) -join '|'
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter $filter -v minimal
```

## Fuentes y trazabilidad

| Fuente única | Evidencia y límite |
|---|---|
| [domain-model.md → «Link»](../context/domain-model.md#link) | Creación, edición, URL inmutable, campos opcionales, movimiento y borrado físico. La privacidad y la propiedad ya inventariadas conservan su grupo principal 2. |
| [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas) | Formularios canónicos, origen `ru`, estados inválidos, confirmación, creación rápida y redirecciones. No se atribuye interacción real a simples coincidencias de HTML. |
| [specifications.md → «Normalización para duplicados»](../context/specifications.md#normalización-para-duplicados) | Duplicados equivalentes rechazados por creación completa y rápida, con nombre de colección existente. La matriz del normalizador corresponde al grupo 6. |
| [specifications.md → «Longitudes máximas»](../context/specifications.md#longitudes-máximas) | Excesos de creación enlazan al campo; refuerzo A5 y límites HTTP de edición H1 pendientes. |
| [specifications.md → «Ajuste de `Image`»](../context/specifications.md#ajuste-de-image) | Imagen relativa aceptada por servicio y HTTP; salida exacta del servicio pendiente en A2. |
| [architecture.md → «Application»](../context/architecture.md#application) | Solicitudes inválidas, excepciones de dominio, propagación inesperada y validadores personalizados. Fallos antes de escritura no acreditan fallo de escritura. |
| [architecture.md → «Persistencia»](../context/architecture.md#persistencia) | Round-trip y persistencia editable; dispose sin commit observado en fake no demuestra por sí solo rollback físico. |
| [architecture.md → «Seguridad web»](../context/architecture.md#seguridad-web) | Antiforgery comprobado para creación, edición, movimiento y eliminación. No es auditoría de vulnerabilidades. |

## Refuerzos y duplicado

| ID | Prioridad | Hallazgo observado | Acción propuesta |
|---|---|---|---|
| A1 | Media | `CreateLink_WhenRequestIsValid_ReturnsCreatedLink` solo exige URL original, colección, privacidad y cantidad; no correspondencia completa DTO/persistido. | Comparar identificador, URL normalizada, campos opcionales, fecha y contenido de la salida con el enlace creado; preparar valores discriminantes. |
| A2 | Media | `CreateLink_WhenImageUsesRelativeReference_IsAccepted` solo exige prefijo y sufijo. Una ruta intermedia errónea podría pasar. | Parametrizar resultado exacto para referencia relativa y autoridad sin esquema, sin ramas dentro del test. |
| A3 | Baja | `GetNewLink_WithValidIdentity_RendersAccessibleFormUsingCollectionStyles` no exige estado HTTP correcto. | Añadir `200` antes de atribuir garantías al formulario renderizado. |
| A4 | Media | `GetEditLink_LoadsEditableFieldsButDoesNotOfferUrlAsInput` busca nombres de campos y título persistido, pero no sus valores cargados ni snapshot del enlace y proyección tras GET. | Exigir valores de título, descripción, imagen y colección en sus controles; comparar snapshot íntegro antes/después. |
| A5 | Media | Las dos variantes de texto demasiado largo en creación solo buscan enlace al campo y cantidad cero, sin estado ni mensaje concreto de validación. | Exigir respuesta prevista, mensaje asociado al campo y valores enviados conservados, además de ausencia de escritura. |
| A6 | Media | El recorrido GET/POST de borrado exige existencia antes y ausencia después; no verifica no modificación completa por GET ni conservación de otro enlace. | Snapshot del enlace/proyección antes del GET y enlace independiente superviviente tras confirmar. |
| A7 | Media | El test de método PUT comprueba solo privacidad final. Otro campo o la proyección podrían cambiar sin detectarse. | Comparar estado completo y proyección tras cada ruta no admitida; evitar atribuir garantía global solo al checkbox. |
| A8 | Media | `Delete_WhenUserOwnsLink_RemovesSearchProjection` solo comprueba la proyección; dejar la fila de enlace pasaría este test. | Exigir ausencia de fila y proyección del identificador, preservando un enlace independiente. |
| A9 | Media | El reintento después de eliminar un destino solo exige nueva colección. No verifica privacidad, metadatos conservados ni proyección tras el movimiento exitoso. | Exigir estado y proyección completos al finalizar, conservando las comprobaciones de rechazo inicial. |
| A10 | Baja | `UpdateLink_WhenLinkIdIsInvalid_ReturnsValidationError` envía tanto enlace como colección inválidos. | Mantener colección válida para aislar el identificador de enlace, con campo/código exactos y sin escrituras. |
| A11 | Media | Los rechazos por URL equivalente en creación completa y rápida solo conservan cantidad, no identidad/valores originales ni proyección. | Comparar snapshots del enlace original y proyección antes/después de ambos rechazos. |
| D1 | Baja | `DeleteLink_WhenIdIsInvalid_ReturnsValidationError` repite el caso `ReturnsValidationWithoutDeleting`, que prueba el mismo código y categoría y además preserva un enlace existente. | Retirar el más débil sin perder el caso de validación no mutante. |

No se consideran duplicados los tests de esquema/mapeo y el round-trip: preparan datos distintos y ejercitan puntos de entrada distintos. Tampoco se eliminan los fallos de escritura tras await por parecerse a excepciones de lectura: esas fases son diferentes.

## Huecos de escenarios

| ID | Prioridad | Hueco de cobertura | Acción propuesta |
|---|---|---|---|
| H1 | Media | Creación HTTP cubre excesos, entidades cubren límites, pero no hay tabla HTTP de edición con máximo y máximo + 1 de título/descripción, conservando el estado previo al rechazo. | Añadir aceptación y rechazo independientes con token válido, campo exacto, estado y proyección. |
| H2 | Media | El movimiento a la misma colección está probado en entidad, pero no hay recorrido HTTP explícito que conserve privacidad y metadatos sin tratarlo como traslado real. | Enviar la colección actual al formulario de movimiento y exigir resultado conforme al contrato del formulario y estado intacto; si el selector la excluye, comprobar el rechazo/no modificación en vez de inventar éxito. |
| H3 | Media | Tras la discrepancia detectada en edición de colecciones, no hay prueba equivalente de precedencia ruta/campo oculto en editar, mover o eliminar enlaces con dos enlaces propios. | Contrastar los bindings de cada handler; enviar identificadores discrepantes y exigir que nunca cambie un recurso distinto del canónico de la ruta. |
| H4 | Baja | Los métodos no admitidos se prueban en gestión existente, no explícitamente en creación completa y creación rápida. | Probar métodos no admitidos según cada ruta y demostrar ausencia de enlace/proyección insertados; no rechazar GET legítimo del dashboard ni de creación. |

## Estado

**Acciones aprobadas; implementación en curso y validación bloqueada.** Se aplicaron refuerzos de salida, imágenes exactas, snapshots HTTP, borrado y reintento; se retiró el duplicado y se añadieron escenarios de límites, destino actual, ids discrepantes y métodos de creación. Aún no se declaran resueltos ni se actualiza la clasificación final: falta ejecutar y corregir las regresiones, incluido comprobar los bindings canónicos de los handlers. Dos intentos de compilación Debug quedaron bloqueados por un cambio concurrente ajeno al lote: `IPublicReadService` ya no declara `GetCollectionPageCount` mientras `Pages/Public/Collection.cshtml.cs` todavía lo invoca (CS1061). No se restauró ni alteró ese refactor, ni se ejecutaron binarios anteriores como evidencia de estos cambios. El inventario tiene 188 casos únicos sin solapamiento con grupos 1–3. Los 14 casos a reforzar incluyen variantes de teorías, no catorce defectos demostrados de producto. Los cuatro huecos son escenarios sin caso ejecutable y no se suman al recuento.

Antes de cerrar: resolver acciones y duplicado, redescubrir casos añadidos/retirados, validar selección exacta y suite global. Las revisiones visuales, foco y accesibilidad real permanecen en S12.


## Cierre automatizado

El bloqueo CS1061 documentado arriba quedo resuelto por el refactor concurrente, sin restaurar la interfaz desde este lote. Se completo la validacion y se resolvieron A1-A11, D1 y H1-H4:

- Salida completa y valores discriminantes de creacion, imagen relativa exacta, estado HTTP y metadatos cargados comprobados.
- Snapshots completos de enlace/proyeccion tras GET, metodos no admitidos y duplicados; borrado de fila/proyeccion con superviviente independiente; movimiento reintentado conserva datos y actualiza proyeccion.
- Identificador de enlace invalido aislado con coleccion valida; retirado el duplicado mas debil.
- Limites HTTP de titulo/descripcion, movimiento a coleccion actual sin mutacion, y metodos PUT de creacion completa/rapida cubiertos.
- Las tres variantes de id oculto discrepante reprodujeron una desviacion real: se editaba, movia o eliminaba el otro enlace propio. Los handlers de `Edit`, `Move` y `Delete` resuelven ahora el id desde `RouteData`, no desde el valor enlazado del formulario. Las regresiones exigen el recurso canonico y preservan el otro enlace y su proyeccion.

Inventario final **197 casos vigentes unicos**: 188 iniciales menos un duplicado mas diez casos nuevos. `EstadoInicial` conserva la clasificacion previa. La salida exacta del filtro del CSV pasa **197/197** en Debug. La suite global pasa **1.743/1.743**, sin omitidos, mediante `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore -v minimal`.

Este lote modifica tests, estos documentos y los tres handlers de gestion de enlaces. No se atribuyen a el los cambios concurrentes del area publica ni sus variaciones en el recuento global.

**Cerrada la revision individual automatizada del grupo 4.** Las comprobaciones humanas visuales, foco y accesibilidad real permanecen en S12. Los resultados no garantizan ausencia absoluta de defectos y deben reevaluarse tras cambios de reglas o implementacion.
