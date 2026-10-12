# Especificación: S3.1 — Entidades `User`, `Collection` y `Link`

## Estado y aprobación

- Estado: cerrada como implementada y validada el 12-X-2026. P01, P02 y P03 resueltas (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)); no se archiva este trabajo.
- Referencia de cierre: «Implementada».

## Objetivo

Disponer en Domain de las entidades `User` (solo en estado completado), `Collection` y `Link`, con sus propiedades y las invariantes propias de cada entidad, reutilizando las políticas de texto, alias, slug y URL ya implementadas en S1 y S2. Las invariantes entre entidades y las transiciones de público y privado se implementan en S3.2 y S3.3.

## Fuentes

- [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): tarea S3.1 y tareas vecinas S3.2–S3.6.
- [domain-model.md → «User»](../../context/domain-model.md#user): propiedades e invariantes de la cuenta completada.
- [domain-model.md → «Collection»](../../context/domain-model.md#collection): propiedades e invariantes de la colección.
- [domain-model.md → «Link»](../../context/domain-model.md#link): propiedades e invariantes del enlace.
- [domain-model.md → «Relaciones»](../../context/domain-model.md#relaciones): pertenencia de colecciones y enlaces a un usuario.
- [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas): recorte, obligatoriedad y límites de cada propiedad.
- [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../../context/specifications.md#generación-de-slugs): generación de `User.Alias` y `Collection.Slug`.
- [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image), [«Validación»](../../context/specifications.md#validación) y [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados): valores de `UrlOriginal`, `Image` y `UrlNormalized`.
- [specifications.md → «Estados del scraping»](../../context/specifications.md#estados-del-scraping): estado inicial de `ScrapingStatus`.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): contenido y aislamiento de Domain y matriz de dependencias.
- [roadmap.md → «Now»](../../context/roadmap.md#now): etapa que abarca S3 y alcance de los tests de invariantes.

## Alcance

- Incluye:
  - `User` en estado completado, con todas las propiedades de [domain-model.md → «User»](../../context/domain-model.md#user), incluidas las de bloqueo y de límite de correo con su valor inicial y sin comportamiento (decisión de DLG, ver P02), y sus invariantes de identidad: `Name`, `Alias`, `Password` y `EmailConfirmedAt` informados a la vez, `Password` como hash ya calculado y `SecurityStamp` generado al crear el usuario.
  - Una política de email en Domain que aplique [specifications.md → «Email»](../../context/specifications.md#email) (recorte, minúsculas, sintaxis `addr-spec` y longitud máxima) y su uso por `User` (decisión de DLG, ver P01).
  - `Collection` con sus propiedades, la obligatoriedad y la longitud de `Name` y `Description`, la generación de `Slug` a partir de `Name` y su regeneración al renombrar.
  - `Link` con sus propiedades, el ajuste y la validación de `UrlOriginal`, el cálculo de `UrlNormalized`, la validez de `Image`, los límites de `Title` y `Description`, el estado inicial de `ScrapingStatus` y la inmutabilidad de `UrlOriginal` y `UrlNormalized`.
  - Edición de `Title`, `Description` e `Image` de un `Link` con las mismas reglas que la creación.
  - Rechazo de las entradas inválidas con las excepciones de dominio existentes, sin dejar la entidad parcialmente modificada.
  - Estado inicial privado (`IsPublic = false`) de `Collection` y `Link` al crearlos.
- Excluye:
  - Cualquier camino que deje `IsPublic = true` o que cambie `IsPublic` o `CollectionId`, y la igualdad de `UserId` entre enlace y colección: S3.2 y S3.3.
  - Los tests de invariantes de propiedad y de público/privado (S3.4), las fichas de entidad (S3.5) y la aprobación del modelo (S3.6). Los tests que S3.1 necesite para comprobar sus propios criterios no sustituyen a los de S3.4.
  - `User` sin completar, y el comportamiento de cuenta propio de etapas posteriores: tokens, bloqueo, límites de correo, cambio de email y de contraseña. Las propiedades correspondientes sí se incluyen (ver P02).
  - Las reglas de la contraseña en claro (longitud, lista de prohibidas, confirmación) y su hash: `User` recibe el hash ya calculado, según [architecture.md → «Casos de uso iniciales»](../../context/architecture.md#casos-de-uso-iniciales).
  - `UserToken`, `Tag` y `LinkTag`, que no pertenecen al alcance comprometido.
  - Unicidad de `Alias`, `Email`, `Collection.Slug` y `Link.UrlNormalized`, la regla de eliminar una colección con enlaces o la última colección y la creación de la «Bandeja de entrada»: requieren consultar otros datos y pertenecen a Application y a Infrastructure (S4 y S5).
  - Persistencia, mapeo a base de datos, DTOs, casos de uso e interfaz.
  - Refactor de las entidades y de las políticas, que es S3a.
- Trabajo vecino y responsable de sus entregables:
  - S1 y S2 entregaron las políticas de texto, alias, slug y URL que las entidades reutilizan.
  - S3.2 implementa las invariantes de [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y S3.3 las de [«Público y privado»](../../context/domain-model.md#público-y-privado).
  - S3.4 escribe los tests de invariantes, S3.5 genera las fichas y S3.6 las revisa.
  - S4 y S5 aplican la detección de colisiones y la persistencia.

## Dependencias y prerrequisitos

- S1 y S2 están completadas según [roadmap.md → «Hecho»](../../context/roadmap.md#hecho). En Domain existen `AliasPolicy`, `SlugPolicy`, `LinkUrlPolicy`, `ImageUrlPolicy`, `UrlValidationPolicy`, `UrlNormalizationPolicy`, `UserTextPolicy`, el enumerador `ScrapingStatus` y las excepciones de dominio de texto y URL.
- No existe todavía ninguna entidad en Domain ni la clase base `BaseEntity`, definida en [architecture.md → «Domain»](../../context/architecture.md#domain): `User`, `Collection` y `Link` heredan de ella (ver P03).
- Ningún sprint anterior creó reglas para `User`: [mvp0-plan.md](../../plans/mvp0-plan.md) solo planificó en S1 las transformaciones de alias y slugs. No existe en Domain una política de email ni ninguna regla de `User` aparte de `AliasPolicy` y `UserTextPolicy`. S3.1 es la primera tarea que las incorpora (ver P01).
- Existe el proyecto de tests con referencia a Domain y el test de arquitectura de S0.7.
- S3.1 no depende de S4 ni de la persistencia.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «User»](../../context/domain-model.md#user) y [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Los datos de un usuario con el registro completado: email, nombre, alias, hash de contraseña y fecha de confirmación. | Se crea el `User`. | Expone todas sus propiedades con los valores recibidos (el email, normalizado), `Name`, `Alias`, `Password` y `EmailConfirmedAt` informados, un `SecurityStamp` informado que difiere entre usuarios distintos, y sin intentos fallidos, bloqueo ni correos registrados. |
| CA02 | [domain-model.md → «User»](../../context/domain-model.md#user), [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Datos de usuario con `Name`, `Alias`, `Password` o `EmailConfirmedAt` ausentes, vacíos o fuera de límite. | Se intenta crear el `User`. | Se rechaza con la excepción de dominio que corresponde a cada caso y no se obtiene ningún `User`; el `Name` válido se guarda recortado. |
| CA03 | [domain-model.md → «User»](../../context/domain-model.md#user) y [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias) | Un candidato a alias con acentos, mayúsculas o espacios, y otro que la generación rechaza. | Se crea el `User` con cada candidato. | El primero queda con el alias generado según la fuente; el segundo se rechaza como en «Generación del alias», sin truncar ni añadir sufijos. |
| CA04 | [domain-model.md → «Collection»](../../context/domain-model.md#collection), [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Un nombre, una descripción opcional y el `UserId` propietario. | Se crea la `Collection`. | `Name` queda recortado y obligatorio, `Description` queda recortada y es `null` si queda vacía, ambas respetan su límite, la colección pertenece al `UserId` recibido y es privada. |
| CA05 | [domain-model.md → «Collection»](../../context/domain-model.md#collection) y [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Un nombre válido, y otro que no produce un slug válido (vacío tras la transformación, con caracteres no admitidos o fuera de rango). | Se crea la `Collection` con cada nombre. | `Slug` se genera con la política de slugs con el valor de la fuente; el segundo nombre se rechaza con la excepción de dominio de la política y no se crea ninguna colección. |
| CA06 | [domain-model.md → «Collection»](../../context/domain-model.md#collection) y [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Una `Collection` existente. | Se renombra con un nombre válido y con uno inválido, y se modifica su descripción. | El nombre válido regenera `Slug`. El inválido se rechaza y la colección conserva `Name` y `Slug` anteriores. La descripción sigue las reglas de CA04. `UserId`, `IsPublic` y `CreatedAt` no cambian. |
| CA07 | [domain-model.md → «Link»](../../context/domain-model.md#link), [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Validación»](../../context/specifications.md#validación) y [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Una URL introducida por el usuario, y otras con esquema no permitido, longitud excesiva, sin dominio o con credenciales. | Se crea el `Link`. | `UrlOriginal` guarda la URL ajustada y validada; `UrlNormalized` es el resultado de la normalización de esa URL; las URL inválidas se rechazan con la excepción que corresponde y no se crea ningún `Link`. |
| CA08 | [domain-model.md → «Link»](../../context/domain-model.md#link), [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | `Title`, `Description` e `Image` opcionales, informados, vacíos, con espacios o por encima de su límite. | Se crea el `Link`. | Los valores vacíos quedan como `null`, los informados quedan recortados, los que superan su límite se rechazan, y `Image` informada queda ajustada y validada según la fuente, resuelta contra `UrlOriginal`; si el ajuste o la validación de `Image` fallan, se rechaza y no se crea el `Link`. |
| CA09 | [domain-model.md → «Link»](../../context/domain-model.md#link) y [specifications.md → «Estados del scraping»](../../context/specifications.md#estados-del-scraping) | Un `Link` recién creado, con su `UserId` y `CollectionId` recibidos. | Se consultan sus propiedades. | `IsPublic` es `false`, `ScrapingStatus` es `NotRequested`, y `UserId` y `CollectionId` conservan los valores recibidos. |
| CA10 | [domain-model.md → «Link»](../../context/domain-model.md#link) | Un `Link` existente. | Se editan `Title`, `Description` e `Image` con valores válidos e inválidos. | Los valores válidos siguen las reglas de CA08. Los inválidos se rechazan y el `Link` conserva sus valores anteriores. |
| CA11 | [domain-model.md → «Link»](../../context/domain-model.md#link) | Un `Link` existente. | Se busca un camino público para cambiar `UrlOriginal` o `UrlNormalized` después de crearlo. | No existe: ni los consumidores de Domain pueden asignarlos, ni las operaciones de edición los modifican. |
| CA12 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | Las entidades implementadas. | Se compila la solución y se ejecuta su suite completa, incluidos los tests de arquitectura. | La compilación no produce advertencias, los tests existentes de S0 a S2 siguen pasando y Domain no incorpora referencias a otros proyectos ni a infraestructura. |
| CA13 | [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m) | El código de S3.1 terminado. | Se revisa el alcance entregado. | No hay camino para dejar `IsPublic = true`, ni para cambiar `IsPublic` o `CollectionId`, ni validación de igualdad de `UserId`, que quedan para S3.2 y S3.3. |
| CA14 | [domain-model.md → «User»](../../context/domain-model.md#user), [specifications.md → «Email»](../../context/specifications.md#email) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Emails con espacios y mayúsculas, con puntos y `+` en la parte local, sin `@`, con sintaxis inválida, vacíos y por encima del límite. | Se aplica la política de email y se crea el `User` con cada uno. | Los válidos quedan recortados y en minúsculas sin alterar puntos ni `+`; los demás se rechazan con la excepción de dominio que corresponde y no se crea ningún `User`. |

Los casos negativos y los límites se cubren mediante las fuentes enlazadas en cada criterio: CA02, CA05, CA07, CA08, CA10 y CA14 incluyen entradas inválidas y valores en su límite.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| P01. Resuelta: [domain-model.md → «User»](../../context/domain-model.md#user) exige un `Email` normalizado y validado según [specifications.md → «Email»](../../context/specifications.md#email), y ningún sprint anterior creó esa regla. S3.1 incorpora la política de email en Domain (opción A). | [domain-model.md → «User»](../../context/domain-model.md#user) y [specifications.md → «Email»](../../context/specifications.md#email) | Ninguno | Decidida por DLG en la conversación de preparación de S3.1 y aprobada con la spec. |
| P02. Resuelta: `User` incluye todas las propiedades de [domain-model.md → «User»](../../context/domain-model.md#user), también las de bloqueo y de límite de correo, con su valor inicial y sin comportamiento. | [domain-model.md → «User»](../../context/domain-model.md#user) | Ninguno | Decidida por DLG en la conversación de preparación de S3.1 y aprobada con la spec. |
| P03. Resuelta: `BaseEntity` queda definida en [architecture.md → «Domain»](../../context/architecture.md#domain) con `Id` y `CreatedAt`; la heredan las entidades con `Id` propio y `LinkTag` queda fuera ([decisions.md → «Tecnología»](../../context/decisions.md#tecnología)). Queda abierto para el plan, sin bloquear la spec, cómo se crean `Collection` y `Link` antes de persistir, porque [specifications.md → «Registro»](../../context/specifications.md#registro) indica que la base de datos asigna `Id` y `CreatedAt` al usuario. | [architecture.md → «Domain»](../../context/architecture.md#domain) y [specifications.md → «Registro»](../../context/specifications.md#registro) | Ninguno | Decidida por DLG el 12-X-2026; el valor de `Id` y `CreatedAt` antes de persistir se confirma en la revisión del plan. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 12-X-2026 («Consensuados los stoppers»).
- [Tareas](tasks.md), con la implementación y la validación completadas y la evidencia registrada.
