# Auditoría individual del grupo 3: gestión privada de colecciones

## Alcance y evidencia

Revisión del 2026-10-30 de **53 casos ejecutables de 48 métodos** del [inventario individual](test-audit-group-3.csv). Clasificación: **37 vigentes, 15 a reforzar y 1 duplicado demostrado**. Se revisaron preparación, invocación y aserciones, no solo los nombres. Las variantes de una teoría comparten clasificación cuando comparten la debilidad o garantía indicada.

Asignación principal sin duplicados con los inventarios de grupos 1 y 2: Application 30 casos, Infrastructure 6 y HTTP 17. Los tests de privacidad, aislamiento y borrado de última colección ya asignados al grupo 2 siguen allí; los recorridos públicos de renombrado permanecen en el grupo 1. La búsqueda, ordenación y paginación de colecciones de las clases mixtas se reservan al grupo 5; actualización de proyecciones y rollback explícito se revisarán allí o en el grupo 8. La fecha de creación y fábricas de errores genéricas de la clase de aplicación se reservan a entidades/base de aplicación. Esos métodos no se consideran auditados por esta selección ni desaparecen del alcance global.

Se consultaron la implementación actual de `CollectionService`, el repositorio y los handlers de edición. El checkout contiene cambios simultáneos en estos archivos y tests; no se alteraron ni se atribuyen a este diagnóstico. Se recompiló Debug para descubrir casos, y luego se ejecutó la selección exacta del CSV: **53/53 superados**, sin omitidos. No se modificaron tests ni producción. No se ejecutó de nuevo la suite global porque este paso solo genera documentación.

```powershell
$cases = Import-Csv plans\test-audit-group-3.csv
$methods = $cases.Caso | ForEach-Object { ($_ -split '\(')[0] } | Sort-Object -Unique
$filter = ($methods | ForEach-Object { 'FullyQualifiedName=' + $_ }) -join '|'
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter $filter -v minimal
```

## Trazabilidad

| Fuente única | Casos y límites |
|---|---|
| [domain-model.md → «Collection»](../context/domain-model.md#collection) | Creación, colisión, lectura, edición, borrado físico. Las invariantes de privacidad y última colección conservan su inventario de grupo 2. |
| [specifications.md → «Generación de slugs»](../context/specifications.md#generación-de-slugs) | Colisión al crear y editar; falta demostrar nombres distintos con slug igual en la ruta HTTP: A7/H1. |
| [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas) | Formularios, identificadores inválidos, redirección tras POST y confirmación no mutante; métodos no permitidos: H3. |
| [specifications.md → «Textos introducidos por el usuario»](../context/specifications.md#textos-introducidos-por-el-usuario) | Recorte y rechazo de texto; las políticas unitarias ya tienen grupo propio. La asociación HTTP y ausencia de escritura en los extremos no se deducen de ellas: H2. |
| [architecture.md → «Application»](../context/architecture.md#application) | Resultados y validadores; excepciones inesperadas propagadas, de dominio traducidas. No se atribuye a un fake rollback físico. |
| [architecture.md → «Persistencia»](../context/architecture.md#persistencia) | Repositorio transaccional de colisiones y commit explícito al borrar. Pruebas de SQL/proyecciones en sus grupos complementarios. |
| [architecture.md → «Seguridad web»](../context/architecture.md#seguridad-web) | Antiforgery real en operaciones CRUD y conservación de datos. Esta revisión no es una auditoría de vulnerabilidades. |

## Refuerzos y limpieza

| ID | Prioridad | Evidencia | Acción propuesta |
|---|---|---|---|
| A1 | Media | `Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException` repite dos veces el mismo caso de factoría nula y no comprueba el usuario actual nulo. | Sustituir la repetición por dependencia de usuario nula y exigir el parámetro correspondiente cuando el contrato lo permita. |
| A2 | Baja | `Constructor_WhenValidatorIsNull_UsesDefaultValidator` solo exige que el constructor devuelva una instancia. Pasaría con un validador por defecto que aceptase cualquier cosa. | Invocar una operación con entrada inválida y exigir el error del validador por defecto; mantener separado el centinela del validador personalizado. |
| A3 | Media | El éxito de creación de Application no afirma descripción recortada ni correspondencia completa DTO/persistido. Las cinco variantes de creación HTTP tampoco verifican descripción, slug ni proyección. | Comparar valores completos de la colección creada y su salida; por HTTP exigir descripción, slug y proyección del identificador creado, además de redirección y privacidad existentes. |
| A4 | Media | `CreateCollection_WhenNameCannotProduceSlug_DoesNotInsertCollection` acepta cualquier fallo con repositorio vacío. Un rechazo ajeno a la generación del slug pasaría inadvertido. | Exigir categoría y código de causa, conservando la ausencia de inserción. |
| A5 | Media | `PostCreateCollection_WhenNameIsRequired_RendersFieldErrorAndFirstInvalidFocus` busca mensaje global, cualquier campo marcado y `querySelectorAll`. No identifica inequívocamente el campo ni demuestra foco real; tampoco comprueba conservación del recuento. | Renombrar para no prometer interacción de navegador, vincular mensaje/aria al campo de nombre y comprobar ausencia de escritura. El foco real sigue siendo revisión humana. |
| A6 | Media | El POST de edición sin token solo comprueba nombre; descripción y privacidad enviadas coinciden con sus estados previos. El POST de borrado inválido conserva solo cantidad. El GET de confirmación conserva cantidad y nombre, no estado completo/proyección. | Preparar valores que cambiarían todos los campos, comparar estado y proyección originales y conjunto de identificadores; no sustituir las comprobaciones del recurso por un recuento. |
| A7 | Media | El rechazo HTTP por nombre duplicado usa exactamente el texto existente. No distingue una comprobación incorrecta por `Name` de la comparación por slug generado. | Usar un nombre distinto que colisione por su slug y exigir el nombre del conflicto en validación, preservando datos y enlaces. |
| D1 | Baja | `DeleteCollection_WhenCollectionIdIsInvalid_ReturnsValidationError` repite la garantía de `DeleteCollection_WhenIdIsInvalid_ReturnsValidationWithoutDeleting`, que también prepara una colección y exige que no se borre. | Retirar el caso más débil conservando categoría, código y no modificación del más fuerte. |
| D2 | Baja | Los dos tests de nombre blanco reparten error de validación y ausencia de inserción con la misma ruta y preparación equivalente. Ninguno por separado reúne ambas garantías. | Consolidar en un caso con código, categoría y ausencia de inserción; el CSV marca ambos como refuerzo hasta decidir cuál se conserva. |

No se propone eliminar pruebas de lectura y mapeo por parecerse al round-trip: sus puntos de entrada y precondiciones son distintos. Los tests de excepciones siguen siendo válidos para la llamada que falla; no acreditan errores tardíos de `Insert`, `Update` o `Delete` si la primera lectura ya lanza.

## Huecos de escenarios

| ID | Prioridad | Hueco | Acción propuesta |
|---|---|---|---|
| H1 | Media | Hay colisiones con mismo nombre y pruebas unitarias de transformación, pero no un recorrido CRUD privado que reúna nombres distintos equivalentes ni una colección ajena con el mismo slug permitido. La actualización de una colección manteniendo su propio slug se ejercita indirectamente en privacidad del grupo 2 y no debe rechazarse por colisión consigo misma. | Crear/editar con equivalencia de slug y comprobar rechazo dentro del usuario; permitir coincidencia con otro usuario y permitir editar conservando el slug propio, con datos persistidos comprobados. |
| H2 | Media | Las políticas/entidades cubren longitudes, pero los tests HTTP inventariados de colecciones no cubren límites de nombre y descripción ni su rechazo independiente en creación y edición. | Casos máximos y máximo + 1 con token válido; exigir campo de error, valores conservados y ausencia de escritura ante rechazo. No duplicar toda la tabla de políticas URL o texto. |
| H3 | Media | No hay test HTTP de métodos no admitidos para las rutas CRUD de colección en esta clase. Las pruebas equivalentes de enlaces no demuestran el enrutamiento de colecciones. | Enviar método no admitido a creación, edición y eliminación, exigir `404` previsto y estado completo sin cambios. |
| H4 | Baja | El formulario de edición recibe identificador canónico en la ruta y un campo oculto `id`, pero no hay caso que haga discrepar ambos y pruebe qué colección puede cambiar. | Preparar dos colecciones propias, enviar `id` oculto distinto del de ruta y comprobar que solo se respeta el recurso canónico según el contrato de ruta; si la regla no resuelve la precedencia, aclararla antes de imponerla. |

## Estado

**Diagnóstico inicial conservado como historial; acciones aprobadas y completadas según el cierre siguiente.** Los 53 identificadores del CSV son únicos y no están asignados a los grupos 1 o 2. Los hallazgos no prueban defectos actuales de producción: son garantías ausentes o débiles en los tests. Antes del cierre hay que aprobar y resolver refuerzos, duplicados y huecos, recompilar/descubrir el inventario y validar la suite global tras los cambios.


## Cierre automatizado

El 2026-10-30 se completaron A1-A7, D1-D2 y H1-H4:

- Dependencias nulas discriminadas, validador por defecto invocado y valores completos de salida/persistencia exigidos al crear.
- Causa exacta de slug vacio; mensaje asociado al campo de nombre sin prometer foco de navegador; estado/proyecciones conservados tras rechazo o GET de confirmacion.
- Colision por nombres distintos con mismo slug, coincidencia permitida entre usuarios y edicion que conserva slug propio.
- Eliminado el duplicado de identificador invalido y consolidado nombre blanco con codigo y ausencia de escritura.
- Ocho variantes de limites HTTP para nombre/descripcion y crear/editar; tres rutas con metodo no admitido y estado intacto.
- POST con id oculto discrepante: la regresion detecto que se actualizaba la otra coleccion. Corregido el handler de edicion para resolver primero el identificador de `RouteData`, sin recortarlo; se conserva el fallback previo para peticiones sin segmento de ruta. No se cambiaron reglas de producto: la ruta canonica identifica el recurso.

Inventario final: **65 casos vigentes unicos** (53 iniciales menos dos consolidados/eliminados mas 14 casos nuevos), conservando `EstadoInicial`. Verificacion Debug: **65/65** exactos del CSV; regresion de clases y renombrado publico **119/119**; suite global **1.740/1.740**, sin omitidos, con `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore -v minimal`.

Este lote modifica tests, documentacion y el handler de edicion de colecciones. Otros cambios simultaneos del checkout no se sobrescribieron ni se atribuyen a esta tarea. La diferencia del total global respecto de ejecuciones anteriores puede incluir esos cambios externos.

**Cerrada la revision individual automatizada del grupo 3.** El foco, la presentacion visual y la accesibilidad de interaccion siguen siendo revision humana de S12; no se acreditan con coincidencias de HTML.
