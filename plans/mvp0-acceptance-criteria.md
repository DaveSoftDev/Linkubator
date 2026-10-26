# Criterios de aceptación de S4 — casos de uso

## Estado

- Estado del artefacto: aprobado por DLG el 16-X-2026 («Leído y comprendido»).
- La aprobación de este catálogo no inicia la implementación; S4.6–S4.10 requieren sus propios artefactos SDD aprobados.

## Alcance y fuente

Este documento reúne los criterios GIVEN/WHEN/THEN para los casos de uso S4, con trazabilidad a las reglas propietarias de [requirements.md](../context/requirements.md), [specifications.md](../context/specifications.md), [domain-model.md](../context/domain-model.md), [architecture.md](../context/architecture.md) y la puerta de S4 en [mvp0-plan.md](mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4).

No sustituye a las fuentes ni repite sus reglas: cada criterio enlaza el documento y la sección que lo fundamentan. DLG aprobó el catálogo en S4.5 el 16-X-2026 («Leído y comprendido»); la decisión y el cierre documental constan en [S4.5 → «Aceptación y cierre»](../specs/archive/s4.5-aprobacion-criterios-aceptacion/tasks.md#aceptación-y-cierre).

## Criterios de aceptación

### Colecciones

#### AC-COL-01 — Crear colección

- Fuente: [requirements.md → «Colecciones»](../context/requirements.md#colecciones), [specifications.md → «Generación de slugs»](../context/specifications.md#generación-de-slugs), [domain-model.md → «Collection»](../context/domain-model.md#collection)
- Dado un usuario identificado.
- Cuando crea una colección con nombre y descripción permitidos.
- Entonces se crea exactamente una colección privada para el usuario identificado y se resuelve su slug según la regla de unicidad.
- Si el nombre es inválido, la operación se rechaza y no se guarda ninguna entidad. Si equivale al nombre de otra colección del mismo usuario, la operación se rechaza, indica con qué colección coincide y no guarda ninguna entidad.

#### AC-COL-01b — Hacer pública una colección vacía

- Fuente: [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado)
- Dado un usuario identificado y los datos válidos para crear una colección.
- Cuando solicita crearla pública sin enlaces.
- Entonces se crea pública, pero todavía no aparece en ninguna página pública mientras no tenga enlaces públicos.

#### AC-COL-02 — Editar colección

- Fuente: [requirements.md → «Colecciones»](../context/requirements.md#colecciones), [requirements.md → «Público y privado»](../context/requirements.md#público-y-privado), [specifications.md → «Generación de slugs»](../context/specifications.md#generación-de-slugs), [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas), [domain-model.md → «Collection»](../context/domain-model.md#collection), [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado), [architecture.md → «Persistencia»](../context/architecture.md#persistencia)
- Dada una colección del usuario identificado.
- Cuando edita nombre, descripción y visibilidad desde su único formulario de edición, con un nombre válido cuyo slug no colisiona con otra colección suya y un estado destino válido.
- Entonces se actualizan sus campos; al cambiar el nombre también se regenera el slug y, si la colección era pública, la dirección anterior deja de existir. Al hacerla privada, todos sus enlaces pasan a ser privados; al hacerla pública, sus enlaces permanecen privados. El cambio de la colección y sus enlaces es atómico.
- Si el nombre no genera un slug válido, colisiona con otra colección suya o el estado destino falta, se repite o es inválido, se rechaza la edición y no se modifican la colección ni sus enlaces.
- El formulario Web y su ruta canónica están definidos en [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas); la implementación técnica se desglosa en [S8.2](../specs/archive/s8.2-operaciones-colecciones/spec.md).

#### AC-COL-04 — Eliminar colección

- Fuente: [requirements.md → «Colecciones»](../context/requirements.md#colecciones), [specifications.md → «Errores controlados»](../context/specifications.md#errores-controlados), [domain-model.md → «Collection»](../context/domain-model.md#collection)
- Dado un usuario identificado y una colección de su propiedad que no tiene enlaces y no es la última del usuario.
- Cuando solicita eliminar esa colección.
- Entonces se elimina la colección.
- Dado un usuario identificado y una solicitud de eliminación con un identificador inválido, o con un identificador válido que no existe o corresponde a una colección ajena.
- Cuando solicita eliminar la colección identificada.
- Entonces un identificador inválido produce un error de validación y no modifica datos. Si el identificador es válido pero la colección no existe o es ajena, Application devuelve un error de categoría `ResourceNotAccessible` con un código estable específico de eliminar colecciones, común a ambos casos y sin revelar cuál ocurrió. La operación no modifica datos. Web traduce cada resultado según [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas).

#### AC-COL-04b — Eliminar una colección con enlaces o la última colección

- Fuente: [requirements.md → «Colecciones»](../context/requirements.md#colecciones), [domain-model.md → «Collection»](../context/domain-model.md#collection) y [specifications.md → «Errores controlados»](../context/specifications.md#errores-controlados)
- Dado un usuario identificado y una colección de su propiedad que tiene enlaces o es la última colección del usuario.
- Cuando solicita eliminar esa colección.
- Entonces la operación se rechaza como regla de dominio, no modifica los datos y comunica el motivo según el tratamiento de errores controlados.

#### AC-COL-05 — Listar colecciones del usuario

- Fuente: [requirements.md → «Búsqueda y filtros»](../context/requirements.md#búsqueda-y-filtros), [specifications.md → «Búsqueda y filtros»](../context/specifications.md#búsqueda-y-filtros), [specifications.md → «Listados»](../context/specifications.md#listados)
- Dado un usuario identificado.
- Cuando entra al listado de colecciones con o sin texto de búsqueda y filtros aplicables.
- Entonces solo se muestran las colecciones del usuario identificado, ordenadas por nombre ascendente con desempate por id.
- `q` y `p` siguen las reglas del listado: vacío en `q` equivale a no buscar; si `q`, tras recortar los espacios extremos, supera su longitud máxima, se muestra el error previsto; un valor inválido de `p` produce el error previsto; los parámetros repetidos usan el primero y los parámetros no aplicables se ignoran.

### Enlaces

#### AC-LINK-01 — Crear enlace

- Fuente: [requirements.md → «Enlaces»](../context/requirements.md#enlaces), [specifications.md → «URLs de los enlaces»](../context/specifications.md#urls-de-los-enlaces), [specifications.md → «Errores controlados»](../context/specifications.md#errores-controlados)
- Dado un usuario identificado y una colección válida de su propiedad.
- Cuando crea un enlace con URL, título, descripción e imagen opcional.
- Entonces la URL se ajusta y valida antes de guardar, el enlace queda asociado a una sola colección y, por defecto, es privado; solo se crea público si se solicita y la colección permite esa transición.
- Si la URL normalizada ya existe para ese usuario o si su imagen o URL son inválidas, la operación se rechaza sin crear el enlace.
- Si el enlace ya existe normalizado, la operación indica en qué colección está y no duplica el registro.

#### AC-LINK-01b — Crear enlace público

- Fuente: [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado), [specifications.md → «Errores controlados»](../context/specifications.md#errores-controlados)
- Dado un usuario identificado y una colección pública de su propiedad.
- Cuando crea un enlace y solicita que sea público.
- Entonces el enlace se crea público y puede mostrarse en las páginas públicas conforme a las reglas de publicación.
- Si la colección es privada, la creación pública se rechaza sin crear el enlace.

#### AC-LINK-02 — Editar enlace

- Fuente: [requirements.md → «Enlaces»](../context/requirements.md#enlaces), [specifications.md → «URLs de los enlaces»](../context/specifications.md#urls-de-los-enlaces), [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas), [«Errores controlados»](../context/specifications.md#errores-controlados) y [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado)
- Dado un enlace del usuario identificado.
- Cuando edita su título, descripción, imagen, colección y estado público o privado en el mismo formulario, sin cambiar la URL base.
- Entonces solo se actualizan los campos válidos; si cambia la colección, se aplica la regla de privacidad del movimiento; si conserva la colección actual, el checkbox establece el estado del enlace conforme a las reglas de privacidad. Un valor del checkbox que no se pueda interpretar como booleano rechaza la edición completa y conserva el estado original. La opción de hacerlo público solo está disponible cuando conserva su colección actual pública.
- Si la colección seleccionada es inválida o ajena, o la imagen no es válida, la operación se rechaza sin cambiar la entidad. El formulario independiente de movimiento y su acción se mantienen conforme a AC-LINK-03.
- Dado un enlace del usuario identificado en una colección privada.
- Cuando intenta editarlo para hacerlo público, incluso si la petición incluye otros cambios válidos.
- Entonces se rechaza la edición completa, no se modifica ningún campo del enlace y se muestra el mensaje «No se puede hacer público un enlace de una colección privada».

#### AC-LINK-03 — Mover enlace

- Fuente: [requirements.md → «Enlaces»](../context/requirements.md#enlaces), [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas) y [architecture.md → «Application»](../context/architecture.md#application)
- Dado un enlace del usuario identificado y una colección destino válida.
- Cuando ejecuta la acción de mover.
- Entonces el enlace pasa a la colección destino, conserva su identidad y sus datos editables, queda privado y la operación solo tiene éxito si el destino pertenece al usuario y la regla de propiedad se cumple.
- Si la colección de destino es inexistente, ajena o la operación es manipulada, el cambio se rechaza sin aplicar cambios.

#### AC-LINK-05 — Eliminar enlace

- Fuente: [requirements.md → «Enlaces»](../context/requirements.md#enlaces), [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas)
- Dado un usuario identificado y un enlace de su propiedad.
- Cuando confirma la eliminación de ese enlace.
- Entonces el enlace se elimina.
- Dado un usuario identificado y una solicitud de eliminación con un identificador inválido, o con un identificador válido que no existe o corresponde a un enlace ajeno.
- Cuando solicita eliminar el enlace identificado.
- Entonces un identificador inválido produce un error de validación y no modifica datos. Si el identificador es válido pero el enlace no existe o es ajeno, Application devuelve un error de categoría `ResourceNotAccessible` con un código estable específico de eliminar enlaces, común a ambos casos y sin revelar cuál ocurrió. La operación no modifica datos. Web traduce cada resultado según [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas).

#### AC-LINK-06 — Listar enlaces del usuario

- Fuente: [requirements.md → «Búsqueda y filtros»](../context/requirements.md#búsqueda-y-filtros), [specifications.md → «Búsqueda y filtros»](../context/specifications.md#búsqueda-y-filtros), [specifications.md → «Listados»](../context/specifications.md#listados)
- Dado un usuario identificado.
- Cuando entra al listado de enlaces con `q`, filtro de colección y paginación previstos para este caso de uso.
- Entonces solo aparecen sus enlaces, filtrados por texto y colección, ordenados por relevancia cuando hay búsqueda —con los desempates especificados— y paginados según la regla propietaria.
- Los valores inválidos de `p` o `c` producen el error previsto; una colección inexistente o ajena en `c` se ignora. Los parámetros no aplicables al listado no se validan ni alteran el resultado.
- `q` vacío equivale a no buscar; si supera el límite especificado se informa del error. Si un parámetro se repite, se aplica solo el primer valor.

### Dashboard y páginas públicas

#### AC-DASH-01 — Dashboard

- Fuente: [requirements.md → «Páginas privadas»](../context/requirements.md#páginas-privadas), [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas)
- Dado un usuario identificado.
- Cuando accede a `/app/dashboard`.
- Entonces ve accesos para crear enlaces y colecciones y puede navegar a sus listados privados, conforme al alcance de estos casos de uso. El formulario de búsqueda envía `q` por GET al listado de enlaces; el dashboard no presenta resultados ni permite editar o eliminar elementos. La gestión de etiquetas queda fuera del alcance de este catálogo.
- Al solicitar `/app`, se redirige a `/app/dashboard`; el dashboard solo se entrega al usuario identificado y no acepta identidad enviada por el cliente.

#### AC-USER-01 — Página pública de usuario

- Fuente: [requirements.md → «Páginas públicas»](../context/requirements.md#páginas-públicas), [specifications.md → «Página de usuario»](../context/specifications.md#página-de-usuario)
- Dado un alias público existente.
- Cuando visita la ruta `/{alias}/`.
- Entonces ve hasta los cinco enlaces públicos más recientes de las colecciones que se muestran en público y, después, hasta las cinco colecciones públicas más recientes; cada conjunto respeta sus reglas de orden. Los elementos muestran el contenido público previsto, sin nombre/email del usuario ni contenido privado. El enlace al listado completo de colecciones aparece cuando corresponda.
- Si no hay enlaces o colecciones que mostrar, aparece el estado vacío definido para cada sección. Si el alias no existe, la ruta responde `404`, incluidos los alias antiguos.
- La página incluye los metadatos, canonical, encabezados y enlaces externos definidos por la fuente propietaria.

#### AC-COLLS-01 — Página pública de colecciones

- Fuente: [requirements.md → «Páginas públicas»](../context/requirements.md#páginas-públicas), [specifications.md → «Página de colecciones»](../context/specifications.md#página-de-colecciones)
- Dado un alias público existente.
- Cuando visita `/{alias}/colecciones/`.
- Entonces se muestran todas las colecciones que se muestran en público, con su nombre, descripción y enlace, ordenadas según la especificación. La página responde `404` si el usuario no existe. Si el usuario no tiene ninguna colección que se muestre en público se mostrará el estado vacío definido.
- Si una colección es privada o no tiene enlaces públicos, no se muestra ni se identifica por la vista pública.
- La página incluye los metadatos, canonical y encabezados definidos por la fuente propietaria.

#### AC-COLLECTION-01 — Página pública de una colección

- Fuente: [requirements.md → «Páginas públicas»](../context/requirements.md#páginas-públicas), [specifications.md → «Página de colección»](../context/specifications.md#página-de-colección)
- Dada una colección pública con al menos un enlace público y un alias existente.
- Cuando visita `/{alias}/colecciones/{slug}`.
- Entonces se muestran solo enlaces públicos de la colección, con el contenido, orden y paginación definidos, y la ruta responde `404` si la colección es privada, inexistente, renombrada o no tiene enlaces públicos.
- Si `p` falta, está vacío, es inválido o fuera de rango, se muestra la primera página sin error; si se repite, se usa el primer valor. La canonical sigue la regla propietaria para primera página, páginas interiores y parámetros desconocidos.
- La página incluye los metadatos, canonical, encabezados e hipervínculos externos definidos por la fuente propietaria.

### Reglas transversales

#### AC-TRANS-01 — Identidad, propiedad y límites de acceso

- Fuente: [domain-model.md → «Propiedad de los datos»](../context/domain-model.md#propiedad-de-los-datos), [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado), [AGENTS.md → «Invariantes críticas»](../AGENTS.md#invariantes-críticas)
- Dado un caso de uso privado de S4 y recursos de uno o más usuarios.
- Cuando se intenta acceder o modificar un recurso ajeno o se envía `UserId` o `Alias` como dato de autorización desde el cliente.
- Entonces la identidad se obtiene de la abstracción fiable y se comprueba la propiedad del recurso y sus relaciones, sin revelar datos privados ni aplicar cambios no autorizados. En lecturas públicas, `alias` y `slug` son localizadores de ruta, no autoridad para leer contenido privado.

#### AC-TRANS-02 — Errores y mensajes accesibles

- Fuente: [specifications.md → «Errores controlados»](../context/specifications.md#errores-controlados), [specifications.md → «Rutas privadas»](../context/specifications.md#rutas-privadas), [requirements.md → «Accesibilidad»](../context/requirements.md#accesibilidad) y [architecture.md → «Application»](../context/architecture.md#application)
- Dado cualquier rechazo funcional o validación.
- Cuando Application comunica el resultado al consumidor.
- Entonces expresa el rechazo mediante el tipo y código de error de Application, sin incluir datos privados ni textos de presentación. Web mapea ese resultado al mensaje accesible y tratamiento HTTP prescritos por la fuente propietaria; esta presentación no se valida como comportamiento de Application en S4.

#### AC-TRANS-03 — Registro de eventos

- Fuente: [requirements.md → «Operación»](../context/requirements.md#operación), [specifications.md → «Registro de eventos»](../context/specifications.md#registro-de-eventos), [architecture.md → «Logs»](../context/architecture.md#logs)
- Dado un caso de uso S4 cuya operación debe producir un evento según la fuente propietaria.
- Cuando se implementa el registro de eventos en S4.10.
- Entonces los eventos cumplen la fuente propietaria y la prohibición de registrar datos sensibles; este criterio no declara implementado el registro ni evidencia de logging antes de S4.10.

## Criterio de cierre de S4.5

- La aprobación humana de este artefacto es la puerta antes de implementar S4.6–S4.10; DLG la registró el 16-X-2026 («Leído y comprendido»).
- Si aparece una omisión o contradicción en una fuente propietaria, se detiene el criterio afectado y se resuelve en el documento propietario antes de aprobarlo.
- Antes de esa aprobación, la implementación de casos de uso y la validación por fakes no podían comenzar.
- La cobertura documental y la validación de enlaces e identificadores están registradas en [S4.4 → «Descomposición»](../specs/archive/s4.4-criterios-aceptacion-casos-uso/tasks.md#descomposición) y [«Evidencia por criterio»](../specs/archive/s4.4-criterios-aceptacion-casos-uso/tasks.md#evidencia-por-criterio). La aprobación posterior de S4.5 consta en [sus tareas → «Aceptación y cierre»](../specs/archive/s4.5-aprobacion-criterios-aceptacion/tasks.md#aceptación-y-cierre); no implica que S4.6–S4.10 hayan comenzado.
