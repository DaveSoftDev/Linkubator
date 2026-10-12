# EspecificaciÃ³n: S3.1 â€” Entidades `User`, `Collection` y `Link`

## Estado y aprobaciÃ³n

- Estado: aceptada, cerrada y archivada el 12-X-2026. P01, P02 y P03 resueltas (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)).
- Referencia de cierre: DLG, Â«CompletadasÂ».

## Objetivo

Disponer en Domain de las entidades `User` (solo en estado completado), `Collection` y `Link`, con sus propiedades y las invariantes propias de cada entidad, reutilizando las polÃ­ticas de texto, alias, slug y URL ya implementadas en S1 y S2. Las invariantes entre entidades y las transiciones de pÃºblico y privado se implementan en S3.2 y S3.3.

## Fuentes

- [mvp0-plan.md â†’ Â«S3: Dominio III: entidades e invariantes (M)Â»](../../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): tarea S3.1 y tareas vecinas S3.2â€“S3.6.
- [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user): propiedades e invariantes de la cuenta completada.
- [domain-model.md â†’ Â«CollectionÂ»](../../../context/domain-model.md#collection): propiedades e invariantes de la colecciÃ³n.
- [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link): propiedades e invariantes del enlace.
- [domain-model.md â†’ Â«RelacionesÂ»](../../../context/domain-model.md#relaciones): pertenencia de colecciones y enlaces a un usuario.
- [specifications.md â†’ Â«Textos introducidos por el usuarioÂ»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas): recorte, obligatoriedad y lÃ­mites de cada propiedad.
- [specifications.md â†’ Â«GeneraciÃ³n del aliasÂ»](../../../context/specifications.md#generaciÃ³n-del-alias) y [Â«GeneraciÃ³n de slugsÂ»](../../../context/specifications.md#generaciÃ³n-de-slugs): generaciÃ³n de `User.Alias` y `Collection.Slug`.
- [specifications.md â†’ Â«Ajuste de URLÂ»](../../../context/specifications.md#ajuste-de-url), [Â«Ajuste de `Image`Â»](../../../context/specifications.md#ajuste-de-image), [Â«ValidaciÃ³nÂ»](../../../context/specifications.md#validaciÃ³n) y [Â«NormalizaciÃ³n para duplicadosÂ»](../../../context/specifications.md#normalizaciÃ³n-para-duplicados): valores de `UrlOriginal`, `Image` y `UrlNormalized`.
- [specifications.md â†’ Â«Estados del scrapingÂ»](../../../context/specifications.md#estados-del-scraping): estado inicial de `ScrapingStatus`.
- [architecture.md â†’ Â«ArquitecturaÂ»](../../../context/architecture.md#arquitectura): contenido y aislamiento de Domain y matriz de dependencias.
- [roadmap.md â†’ Â«NowÂ»](../../../context/roadmap.md#now): etapa que abarca S3 y alcance de los tests de invariantes.

## Alcance

- Incluye:
  - `User` en estado completado, con todas las propiedades de [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user), incluidas las de bloqueo y de lÃ­mite de correo con su valor inicial y sin comportamiento (decisiÃ³n de DLG, ver P02), y sus invariantes de identidad: `Name`, `Alias`, `Password` y `EmailConfirmedAt` informados a la vez, `Password` como hash ya calculado y `SecurityStamp` generado al crear el usuario.
  - Una polÃ­tica de email en Domain que aplique [specifications.md â†’ Â«EmailÂ»](../../../context/specifications.md#email) (recorte, minÃºsculas, sintaxis `addr-spec` y longitud mÃ¡xima) y su uso por `User` (decisiÃ³n de DLG, ver P01).
  - `Collection` con sus propiedades, la obligatoriedad y la longitud de `Name` y `Description`, la generaciÃ³n de `Slug` a partir de `Name` y su regeneraciÃ³n al renombrar.
  - `Link` con sus propiedades, el ajuste y la validaciÃ³n de `UrlOriginal`, el cÃ¡lculo de `UrlNormalized`, la validez de `Image`, los lÃ­mites de `Title` y `Description`, el estado inicial de `ScrapingStatus` y la inmutabilidad de `UrlOriginal` y `UrlNormalized`.
  - EdiciÃ³n de `Title`, `Description` e `Image` de un `Link` con las mismas reglas que la creaciÃ³n.
  - Rechazo de las entradas invÃ¡lidas con las excepciones de dominio existentes, sin dejar la entidad parcialmente modificada.
  - Estado inicial privado (`IsPublic = false`) de `Collection` y `Link` al crearlos.
- Excluye:
  - Cualquier camino que deje `IsPublic = true` o que cambie `IsPublic` o `CollectionId`, y la igualdad de `UserId` entre enlace y colecciÃ³n: S3.2 y S3.3.
  - Los tests de invariantes de propiedad y de pÃºblico/privado (S3.4), las fichas de entidad (S3.5) y la aprobaciÃ³n del modelo (S3.6). Los tests que S3.1 necesite para comprobar sus propios criterios no sustituyen a los de S3.4.
  - `User` sin completar, y el comportamiento de cuenta propio de etapas posteriores: tokens, bloqueo, lÃ­mites de correo, cambio de email y de contraseÃ±a. Las propiedades correspondientes sÃ­ se incluyen (ver P02).
  - Las reglas de la contraseÃ±a en claro (longitud, lista de prohibidas, confirmaciÃ³n) y su hash: `User` recibe el hash ya calculado, segÃºn [architecture.md â†’ Â«Casos de uso inicialesÂ»](../../../context/architecture.md#casos-de-uso-iniciales).
  - `UserToken`, `Tag` y `LinkTag`, que no pertenecen al alcance comprometido.
  - Unicidad de `Alias`, `Email`, `Collection.Slug` y `Link.UrlNormalized`, la regla de eliminar una colecciÃ³n con enlaces o la Ãºltima colecciÃ³n y la creaciÃ³n de la Â«Bandeja de entradaÂ»: requieren consultar otros datos y pertenecen a Application y a Infrastructure (S4 y S5).
  - Persistencia, mapeo a base de datos, DTOs, casos de uso e interfaz.
  - Refactor de las entidades y de las polÃ­ticas, que es S3a.
- Trabajo vecino y responsable de sus entregables:
  - S1 y S2 entregaron las polÃ­ticas de texto, alias, slug y URL que las entidades reutilizan.
  - S3.2 implementa las invariantes de [Â«Propiedad de los datosÂ»](../../../context/domain-model.md#propiedad-de-los-datos) y S3.3 las de [Â«PÃºblico y privadoÂ»](../../../context/domain-model.md#pÃºblico-y-privado).
  - S3.4 escribe los tests de invariantes, S3.5 genera las fichas y S3.6 las revisa.
  - S4 y S5 aplican la detecciÃ³n de colisiones y la persistencia.

## Dependencias y prerrequisitos

- S1 y S2 estÃ¡n completadas segÃºn [roadmap.md â†’ Â«HechoÂ»](../../../context/roadmap.md#hecho). En Domain existen `AliasPolicy`, `SlugPolicy`, `LinkUrlPolicy`, `ImageUrlPolicy`, `UrlValidationPolicy`, `UrlNormalizationPolicy`, `UserTextPolicy`, el enumerador `ScrapingStatus` y las excepciones de dominio de texto y URL.
- No existe todavÃ­a ninguna entidad en Domain ni la clase base `BaseEntity`, definida en [architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain): `User`, `Collection` y `Link` heredan de ella (ver P03).
- NingÃºn sprint anterior creÃ³ reglas para `User`: [mvp0-plan.md](../../../plans/mvp0-plan.md) solo planificÃ³ en S1 las transformaciones de alias y slugs. No existe en Domain una polÃ­tica de email ni ninguna regla de `User` aparte de `AliasPolicy` y `UserTextPolicy`. S3.1 es la primera tarea que las incorpora (ver P01).
- Existe el proyecto de tests con referencia a Domain y el test de arquitectura de S0.7.
- S3.1 no depende de S4 ni de la persistencia.

## Criterios de aceptaciÃ³n

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) y [specifications.md â†’ Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) | Los datos de un usuario con el registro completado: email, nombre, alias, hash de contraseÃ±a y fecha de confirmaciÃ³n. | Se crea el `User`. | Expone todas sus propiedades con los valores recibidos (el email, normalizado), `Name`, `Alias`, `Password` y `EmailConfirmedAt` informados, un `SecurityStamp` informado que difiere entre usuarios distintos, y sin intentos fallidos, bloqueo ni correos registrados. |
| CA02 | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user), [specifications.md â†’ Â«Textos introducidos por el usuarioÂ»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) | Datos de usuario con `Name`, `Alias`, `Password` o `EmailConfirmedAt` ausentes, vacÃ­os o fuera de lÃ­mite. | Se intenta crear el `User`. | Se rechaza con la excepciÃ³n de dominio que corresponde a cada caso y no se obtiene ningÃºn `User`; el `Name` vÃ¡lido se guarda recortado. |
| CA03 | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) y [specifications.md â†’ Â«GeneraciÃ³n del aliasÂ»](../../../context/specifications.md#generaciÃ³n-del-alias) | Un candidato a alias con acentos, mayÃºsculas o espacios, y otro que la generaciÃ³n rechaza. | Se crea el `User` con cada candidato. | El primero queda con el alias generado segÃºn la fuente; el segundo se rechaza como en Â«GeneraciÃ³n del aliasÂ», sin truncar ni aÃ±adir sufijos. |
| CA04 | [domain-model.md â†’ Â«CollectionÂ»](../../../context/domain-model.md#collection), [specifications.md â†’ Â«Textos introducidos por el usuarioÂ»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) | Un nombre, una descripciÃ³n opcional y el `UserId` propietario. | Se crea la `Collection`. | `Name` queda recortado y obligatorio, `Description` queda recortada y es `null` si queda vacÃ­a, ambas respetan su lÃ­mite, la colecciÃ³n pertenece al `UserId` recibido y es privada. |
| CA05 | [domain-model.md â†’ Â«CollectionÂ»](../../../context/domain-model.md#collection) y [specifications.md â†’ Â«GeneraciÃ³n de slugsÂ»](../../../context/specifications.md#generaciÃ³n-de-slugs) | Un nombre vÃ¡lido, y otro que no produce un slug vÃ¡lido (vacÃ­o tras la transformaciÃ³n, con caracteres no admitidos o fuera de rango). | Se crea la `Collection` con cada nombre. | `Slug` se genera con la polÃ­tica de slugs con el valor de la fuente; el segundo nombre se rechaza con la excepciÃ³n de dominio de la polÃ­tica y no se crea ninguna colecciÃ³n. |
| CA06 | [domain-model.md â†’ Â«CollectionÂ»](../../../context/domain-model.md#collection) y [specifications.md â†’ Â«GeneraciÃ³n de slugsÂ»](../../../context/specifications.md#generaciÃ³n-de-slugs) | Una `Collection` existente. | Se renombra con un nombre vÃ¡lido y con uno invÃ¡lido, y se modifica su descripciÃ³n. | El nombre vÃ¡lido regenera `Slug`. El invÃ¡lido se rechaza y la colecciÃ³n conserva `Name` y `Slug` anteriores. La descripciÃ³n sigue las reglas de CA04. `UserId`, `IsPublic` y `CreatedAt` no cambian. |
| CA07 | [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link), [specifications.md â†’ Â«Ajuste de URLÂ»](../../../context/specifications.md#ajuste-de-url), [Â«ValidaciÃ³nÂ»](../../../context/specifications.md#validaciÃ³n) y [Â«NormalizaciÃ³n para duplicadosÂ»](../../../context/specifications.md#normalizaciÃ³n-para-duplicados) | Una URL introducida por el usuario, y otras con esquema no permitido, longitud excesiva, sin dominio o con credenciales. | Se crea el `Link`. | `UrlOriginal` guarda la URL ajustada y validada; `UrlNormalized` es el resultado de la normalizaciÃ³n de esa URL; las URL invÃ¡lidas se rechazan con la excepciÃ³n que corresponde y no se crea ningÃºn `Link`. |
| CA08 | [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link), [specifications.md â†’ Â«Textos introducidos por el usuarioÂ»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) | `Title`, `Description` e `Image` opcionales, informados, vacÃ­os, con espacios o por encima de su lÃ­mite. | Se crea el `Link`. | Los valores vacÃ­os quedan como `null`, los informados quedan recortados, los que superan su lÃ­mite se rechazan, y `Image` informada queda ajustada y validada segÃºn la fuente, resuelta contra `UrlOriginal`; si el ajuste o la validaciÃ³n de `Image` fallan, se rechaza y no se crea el `Link`. |
| CA09 | [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link) y [specifications.md â†’ Â«Estados del scrapingÂ»](../../../context/specifications.md#estados-del-scraping) | Un `Link` reciÃ©n creado, con su `UserId` y `CollectionId` recibidos. | Se consultan sus propiedades. | `IsPublic` es `false`, `ScrapingStatus` es `NotRequested`, y `UserId` y `CollectionId` conservan los valores recibidos. |
| CA10 | [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link) | Un `Link` existente. | Se editan `Title`, `Description` e `Image` con valores vÃ¡lidos e invÃ¡lidos. | Los valores vÃ¡lidos siguen las reglas de CA08. Los invÃ¡lidos se rechazan y el `Link` conserva sus valores anteriores. |
| CA11 | [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link) | Un `Link` existente. | Se busca un camino pÃºblico para cambiar `UrlOriginal` o `UrlNormalized` despuÃ©s de crearlo. | No existe: ni los consumidores de Domain pueden asignarlos, ni las operaciones de ediciÃ³n los modifican. |
| CA12 | [architecture.md â†’ Â«ArquitecturaÂ»](../../../context/architecture.md#arquitectura) | Las entidades implementadas. | Se compila la soluciÃ³n y se ejecuta su suite completa, incluidos los tests de arquitectura. | La compilaciÃ³n no produce advertencias, los tests existentes de S0 a S2 siguen pasando y Domain no incorpora referencias a otros proyectos ni a infraestructura. |
| CA13 | [mvp0-plan.md â†’ Â«S3: Dominio III: entidades e invariantes (M)Â»](../../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m) | El cÃ³digo de S3.1 terminado. | Se revisa el alcance entregado. | No hay camino para dejar `IsPublic = true`, ni para cambiar `IsPublic` o `CollectionId`, ni validaciÃ³n de igualdad de `UserId`, que quedan para S3.2 y S3.3. |
| CA14 | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user), [specifications.md â†’ Â«EmailÂ»](../../../context/specifications.md#email) y [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) | Emails con espacios y mayÃºsculas, con puntos y `+` en la parte local, sin `@`, con sintaxis invÃ¡lida, vacÃ­os y por encima del lÃ­mite. | Se aplica la polÃ­tica de email y se crea el `User` con cada uno. | Los vÃ¡lidos quedan recortados y en minÃºsculas sin alterar puntos ni `+`; los demÃ¡s se rechazan con la excepciÃ³n de dominio que corresponde y no se crea ningÃºn `User`. |

Los casos negativos y los lÃ­mites se cubren mediante las fuentes enlazadas en cada criterio: CA02, CA05, CA07, CA08, CA10 y CA14 incluyen entradas invÃ¡lidas y valores en su lÃ­mite.

## Bloqueos y preguntas pendientes

| Pregunta o contradicciÃ³n | Fuente afectada | Criterio bloqueado | DecisiÃ³n humana necesaria |
| --- | --- | --- | --- |
| P01. Resuelta: [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) exige un `Email` normalizado y validado segÃºn [specifications.md â†’ Â«EmailÂ»](../../../context/specifications.md#email), y ningÃºn sprint anterior creÃ³ esa regla. S3.1 incorpora la polÃ­tica de email en Domain (opciÃ³n A). | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) y [specifications.md â†’ Â«EmailÂ»](../../../context/specifications.md#email) | Ninguno | Decidida por DLG en la conversaciÃ³n de preparaciÃ³n de S3.1 y aprobada con la spec. |
| P02. Resuelta: `User` incluye todas las propiedades de [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user), tambiÃ©n las de bloqueo y de lÃ­mite de correo, con su valor inicial y sin comportamiento. | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) | Ninguno | Decidida por DLG en la conversaciÃ³n de preparaciÃ³n de S3.1 y aprobada con la spec. |
| P03. Resuelta: `BaseEntity` queda definida en [architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain) con `Id` y `CreatedAt`; la heredan las entidades con `Id` propio y `LinkTag` queda fuera ([decisions.md â†’ Â«TecnologÃ­aÂ»](../../../context/decisions.md#tecnologÃ­a)). Queda abierto para el plan, sin bloquear la spec, cÃ³mo se crean `Collection` y `Link` antes de persistir, porque [specifications.md â†’ Â«RegistroÂ»](../../../context/specifications.md#registro) indica que la base de datos asigna `Id` y `CreatedAt` al usuario. | [architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain) y [specifications.md â†’ Â«RegistroÂ»](../../../context/specifications.md#registro) | Ninguno | Decidida por DLG el 12-X-2026; el valor de `Id` y `CreatedAt` antes de persistir se confirma en la revisiÃ³n del plan. |

## Artefactos relacionados

- [Plan tÃ©cnico](plan.md), aprobado por DLG el 12-X-2026 (Â«Consensuados los stoppersÂ»).
- [Tareas](tasks.md), con la implementaciÃ³n y la validaciÃ³n completadas y la evidencia registrada.

