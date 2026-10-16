# Ficha derivada: Link

> Estado: vista de revisión derivada dentro de S3.5. No es una fuente de reglas ni sustituye la aprobación final de S3.6.

## Fuentes consultadas

- [domain-model.md → «Link»](../../../../context/domain-model.md#link)
- [domain-model.md → «Público y privado»](../../../../context/domain-model.md#público-y-privado)
- [domain-model.md → «Propiedad de los datos»](../../../../context/domain-model.md#propiedad-de-los-datos)
- [domain-model.md → «Restricciones de unicidad»](../../../../context/domain-model.md#restricciones-de-unicidad)
- [requirements.md → «Enlaces»](../../../../context/requirements.md#enlaces)
- [requirements.md → «Público y privado»](../../../../context/requirements.md#público-y-privado)
- [specifications.md → «Ajuste de URL»](../../../../context/specifications.md#ajuste-de-url)
- [specifications.md → «Normalización para duplicados»](../../../../context/specifications.md#normalización-para-duplicados)
- [specifications.md → «Ajuste de `Image`»](../../../../context/specifications.md#ajuste-de-image)
- [specifications.md → «Validación»](../../../../context/specifications.md#validación)
- [specifications.md → «Estados del scraping»](../../../../context/specifications.md#estados-del-scraping)
- [specifications.md → «Listados»](../../../../context/specifications.md#listados)

## Propiedades y comportamiento del enlace

`Link` representa el enlace guardado por el usuario. La entidad se define en [domain-model.md → «Link»](../../../../context/domain-model.md#link) con estas propiedades: `Id`, `UserId`, `CollectionId`, `UrlOriginal`, `UrlNormalized`, `Title`, `Description`, `Image`, `IsPublic`, `ScrapingStatus` y `CreatedAt`.

- `UrlOriginal` guarda la URL ajustada y validada, y es inmutable: para cambiarla hay que crear un enlace nuevo.
- `UrlNormalized` es una clave plana para detectar duplicados, no una URL pública válida. Se calcula a partir de `UrlOriginal` según [specifications.md → «Normalización para duplicados»](../../../../context/specifications.md#normalización-para-duplicados).
- `Title`, `Description` e `Image` son opcionales; la imagen debe cumplir los ajustes y validaciones de [specifications.md → «Ajuste de `Image`»](../../../../context/specifications.md#ajuste-de-image) y [specifications.md → «Validación»](../../../../context/specifications.md#validación).
- `IsPublic` es `false` al crearlo salvo que el usuario lo marque como público si la operación está habilitada.
- `ScrapingStatus` arranca en `NotRequested` y solo cambia según los estados del scraping; ver [specifications.md → «Estados del scraping»](../../../../context/specifications.md#estados-del-scraping).

Las propiedades y reglas del dominio se enumeran en [domain-model.md#L131-L160](../../../../context/domain-model.md#L131); el conjunto de propiedades editables y valores iniciales está en la misma fuente. El requisito funcional de crear, editar, mover, etiquetar, ejecutar scraping y eliminar enlaces figura en [requirements.md#L61-L79](../../../../context/requirements.md#L61).

## Reglas de unicidad, propiedad y movimiento

- El enlace pertenece a un único usuario y a una única colección, según [domain-model.md → «Link»](../../../../context/domain-model.md#link).
- La unicidad efectiva es por `UserId + Link.UrlNormalized`, según [domain-model.md → «Restricciones de unicidad»](../../../../context/domain-model.md#restricciones-de-unicidad).
- Al mover un enlace a otra colección, el enlace vuelve a privado, independientemente de la visibilidad de la colección origen o destino.
- El borrado del enlace es físico y elimina también sus relaciones `LinkTag` y su entrada en el índice de búsqueda.
- La regla de propiedad y de relaciones de usuario/colección/etiquetas queda en [domain-model.md → «Propiedad de los datos»](../../../../context/domain-model.md#propiedad-de-los-datos): la identidad del usuario y la relación del recurso deben comprobarse antes de cualquier operación privada.

Las restricciones de unicidad están enumeradas en [domain-model.md#L179-L187](../../../../context/domain-model.md#L179); la propiedad entre enlace, colección y usuario en [domain-model.md#L173-L177](../../../../context/domain-model.md#L173).

## Públicos, privados y páginas de colección

La visibilidad del enlace está ligada a la colección:

- Un enlace público aparece en la página pública de su colección; un enlace privado no.
- Para que un enlace aparezca en la parte pública, tanto la colección como el enlace deben ser públicos.
- Al hacer privada una colección, todos sus enlaces pasan a privado.
- Al hacer pública una colección, sus enlaces se mantienen privados hasta que el usuario los publica individualmente.

Estas condiciones están redactadas en [domain-model.md → «Público y privado»](../../../../context/domain-model.md#público-y-privado) y en [requirements.md → «Público y privado»](../../../../context/requirements.md#público-y-privado).

Las invariantes de transición se detallan en [domain-model.md#L193-L218](../../../../context/domain-model.md#L193); el resumen funcional está en [requirements.md#L81-L83](../../../../context/requirements.md#L81).

## Ajuste y validación de URL

La URL de un enlace se ajusta y valida antes de guardarse. El documento de producto lo expresa como parte del flujo de creación en [requirements.md → «Enlaces»](../../../../context/requirements.md#enlaces), y la regla exacta se acompaña en [specifications.md → «Ajuste de URL»](../../../../context/specifications.md#ajuste-de-url), [«Normalización para duplicados»](../../../../context/specifications.md#normalización-para-duplicados) y [«Validación»](../../../../context/specifications.md#validación).

El hecho de que `UrlOriginal` sea inmutable y que la detección de duplicados se haga por `UrlNormalized` es clave para la integridad del dominio; la comparación de equivalencia de URL forma parte de la especificación del producto, no de una regla ad hoc.

El flujo funcional, incluidos los metadatos opcionales y las etiquetas, está en [requirements.md#L61-L79](../../../../context/requirements.md#L61). Los contratos detallados de ajuste, validación y duplicados se encuentran en [specifications.md#L368-L588](../../../../context/specifications.md#L368).

## Scraping, listados y alcance

El scraping manual se menciona junto al campo de URL, mientras el scraping de metadatos está previsto para una etapa posterior y mantiene decisiones pendientes. Las fuentes son [requirements.md#L68-L75](../../../../context/requirements.md#L68), [specifications.md#L589-L621](../../../../context/specifications.md#L589), [decisions.md#L103-L116](../../../../context/decisions.md#L103) y [decisions.md#L126-L142](../../../../context/decisions.md#L126). Esta ficha no presenta esa capacidad como implementada.

El listado y la ordenación de enlaces se definen en [specifications.md#L622-L669](../../../../context/specifications.md#L622); las páginas públicas que pueden incluir enlaces se describen en [requirements.md#L98-L109](../../../../context/requirements.md#L98) y [specifications.md#L754-L789](../../../../context/specifications.md#L754).

## Observaciones finales

- No se detectaron contradicciones específicas de `Link` en las secciones revisadas. El scraping aparece en fuentes funcionales y técnicas con decisiones todavía pendientes; por ello se registra como alcance no implementado/resuelto, no como una regla contradictoria.
- La regla más crítica de revisión es la combinación de ajuste/normalización/duplicados con la visibilidad pública, porque un mismo enlace puede estar contenido en una colección pública pero no ser visible si su `IsPublic` no lo permite.
- La aprobación final con código y con las invariantes pertenece a S3.6, según [spec.md → «Alcance»](../spec.md#alcance).
