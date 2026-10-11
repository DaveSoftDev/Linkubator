# Especificación: S2.6 — Unificación de las validaciones de entrada de Domain

## Estado y aprobación

- Estado: aprobada por DLG el 11-X-2026.
- Referencia de aprobación: «Aprobada».

## Objetivo

Que todos los puntos de entrada públicos de `Linkubator.Domain` que reciben texto traten igual una entrada nula, vacía o en blanco: una única comprobación de precondición al inicio de cada método, con el mismo tipo de error en todos, sin recortar la entrada y sin alterar el resto del comportamiento aceptado. El resultado es homogeneidad y ausencia de comprobaciones duplicadas, no un cambio de reglas de producto.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.6, Definition of Done del sprint y reparto con los trabajos vecinos.
- [S2.1 → spec](../archive/s2.1-ajuste-de-url/spec.md) (CA04) y [S2.1 → plan](../archive/s2.1-ajuste-de-url/plan.md): decisión confirmada por DLG de que la entrada nula, vacía o en blanco es una precondición del llamador, con la misma filosofía para alias y slugs.
- [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario): el recorte y el rechazo del texto vacío en campos obligatorios ocurren antes de llegar a Domain.
- [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii), [«Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../../context/specifications.md#generación-de-slugs): el texto llega ya recortado y un resultado vacío tras la transformación se rechaza.
- [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image), [«Validación»](../../context/specifications.md#validación) y [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados): comportamiento que debe conservarse.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): aislamiento de Domain.
- [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación): estado de la etapa.

## Alcance

- Incluye:
  - Inventariar los métodos públicos de `Linkubator.Domain/Policies` que reciben texto y clasificar su tratamiento actual de nulo, vacío y en blanco.
  - Unificar el criterio: nulo, vacío o en blanco lanza `NotValidInputException`; la comprobación se hace una sola vez al inicio del método público y no recorta la entrada.
  - Aplicar el criterio a los métodos cuyo texto debe contener algo: generación de alias y de slugs, extracción de host, detección de esquema URI, ajuste de `UrlOriginal`, validación y normalización, y la URL base del ajuste de `Image`.
  - Sustituir las comprobaciones manuales equivalentes (nulo seguido de vacío o blanco con excepción propia) por la comprobación única, y eliminar las que han quedado inalcanzables.
  - Actualizar las pruebas existentes cuyo caso de entrada vacía o en blanco cambia de error y homogeneizar las pruebas de precondición de cada método.
  - Registrar como bloqueo o propuesta cualquier otra inconsistencia de entrada que surja durante el trabajo («lo que surja durante el sprint» del plan), sin implementarla sin aprobación.
- Excluye:
  - Cambiar reglas, listas, límites o mensajes de las fuentes, ni el tipo de error de dominio de ningún rechazo que ya no sea una precondición.
  - El cliente (aplicación o scraper) que desee dejar vacía la imagen debe no llamar a `ImageUrlPolicy.Adjust`, según [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image). El rechazo es una precondición como en otros métodos de Domain.
  - El resultado vacío tras la transformación de alias y slugs (por ejemplo, un texto formado solo por emojis): sigue siendo un error de dominio de longitud.
  - `UserTextPolicy.TrimToNull` (acepta nulo por contrato), `UserTextPolicy.CountCodePoints` e `IsWithinMaximumLength` (el texto vacío tiene longitud cero) y los predicados `IsValidHost` y `TryExtractHost`, que devuelven falso en lugar de lanzar.
  - La precondición de URI absoluta de `UrlNormalizationPolicy.Normalize`, que pertenece al contrato aprobado de S2.4.
  - Refactorizar las políticas para mejorar rendimiento o legibilidad, que pertenece a S3a.
  - Integrar las políticas en las entidades, que pertenece a S3, y recortar o rechazar el texto vacío de los campos obligatorios, que pertenece a las capas que reciben la entrada del usuario.
- Trabajo vecino y responsable de sus entregables:
  - S1.3, S1.4 y S2.1 a S2.5 entregaron las políticas y las pruebas sobre las que se trabaja.
  - S2.7 y S2.8 (humano) revisan la normalización con URLs reales y deciden los casos dudosos sobre el código ya unificado.
  - S3a refactoriza las políticas y S3 las integra en las entidades.

## Dependencias y prerrequisitos

- Existen `Linkubator.Domain` y el proyecto de pruebas con las pruebas de las políticas, incluidas las de ejemplos de S1.5 y S2.5.
- S1.3, S1.4 y S2.1 a S2.5 están aceptadas por DLG según [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación).
- S2.6 no depende de S2.7 ni de S2.8; al contrario, la revisión humana con URLs reales se hace sobre el código ya unificado.
- Los archivos de S1.3, S1.4 y S2.1 a S2.5 que citan la unificación como S2.8 y la revisión humana como S2.6 y S2.7 conservan la numeración anterior del [mvp0-plan.md](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l); el plan es la fuente vigente de la numeración.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | Los métodos públicos de las políticas de Domain que reciben texto. | Se revisa el inventario del trabajo. | Cada método figura con su tratamiento anterior y posterior de nulo, vacío y en blanco, o con el motivo de su exclusión, y no queda ninguno sin clasificar. |
| CA02 | [S2.1 → spec](../archive/s2.1-ajuste-de-url/spec.md) (CA04) | Una entrada nula en cualquier método incluido. | Se llama al método. | Lanza `NotValidInputException`, con el mismo tipo en todos. |
| CA03 | [S2.1 → spec](../archive/s2.1-ajuste-de-url/spec.md) (CA04) y [«Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) | Una entrada vacía, en blanco o con espacios en los extremos en cualquier método incluido, y la URL base vacía o en blanco del ajuste de `Image`. | Se llama al método. | La vacía o en blanco lanza `NotValidInputException` y la que tiene espacios en los extremos no se recorta ni se rechaza por ello, con el mismo comportamiento en todos los métodos. |
| CA04 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada de `Image` vacía o en blanco con una URL base válida. | Se ajusta. | Lanza `NotValidInputException`, igual que otros métodos de Domain. |
| CA05 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Un texto no vacío cuya transformación queda vacía o fuera de rango. | Se genera el alias o el slug. | Se rechaza con el mismo error de dominio de longitud que antes, sin truncar. |
| CA06 | [S2.1 → spec](../archive/s2.1-ajuste-de-url/spec.md) (CA04) | Una precondición incumplida con un texto que contiene datos sensibles, como una URL con credenciales. | Se captura el error. | Su mensaje no incluye el texto introducido ni las credenciales. |
| CA07 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | El código de las políticas tras el cambio. | Se revisa cada método público incluido. | La comprobación de nulo, vacío o en blanco aparece una sola vez al inicio, mediante la comprobación única, y no quedan comprobaciones manuales equivalentes ni inalcanzables. |
| CA08 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image), [«Validación»](../../context/specifications.md#validación), [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados), [«Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Todas las pruebas de ejemplos de S1.5 y S2.5 y las de cada política. | Se ejecutan tras el cambio. | Pasan todas, salvo los casos de entrada vacía o en blanco que CA03 cambia de forma explícita y registrada; ningún ejemplo de las fuentes cambia de resultado. |
| CA09 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | Cada método incluido. | Se revisan sus pruebas. | Tiene pruebas de nulo, vacío y en blanco con el tipo de error exacto, y las de alias y slugs que antes esperaban un error de longitud para vacío o en blanco esperan ahora la precondición. |
| CA10 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) y [mvp0-plan.md → «Definition of Done común (aplica a todos los sprints)»](../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución con el cambio. | Se compila y se ejecutan todas las pruebas. | Compila sin advertencias, todas las pruebas pasan y no se añaden dependencias a Domain. |
| CA11 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | Las inconsistencias que surjan y las preguntas de [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes). | Se cierra el trabajo. | Cada una está resuelta por DLG o registrada como propuesta, y ninguna se ha implementado sin aprobación. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| `AsciiTransformationPolicy.Transform` también exige la precondición, como decidiría el usuario. | [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | CA02, CA03, CA05, CA09 | Se implementa con `NotValidInputException` si la entrada es nula, vacía o en blanco. |
| La detección de esquema de `LinkUrlPolicy` duplica la de `UriSchemePolicy`; [S2.3 → plan](../s2.3-validacion-de-urls/plan.md) la dejó pendiente para la unificación de Domain. Se deja para S3a. | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | — | — |

Nota de trazabilidad: el cambio de error para vacío o en blanco en alias y slugs altera pruebas aceptadas de S1.3 y S1.4 (`AliasPolicyTests`, `SlugPolicyTests`); lo autoriza la tarea S2.6 del plan y la decisión confirmada en S2.1, y debe quedar registrado en las tareas como cambio explícito, no como regresión.

## Artefactos relacionados

El plan técnico y las tareas se enlazan aquí solo después de que cada artefacto exista.

- [Plan técnico](plan.md), aprobado por DLG el 11-X-2026.
- [Tareas](tasks.md), aprobadas por DLG el 11-X-2026.
