# Especificación: S2.2 — Ajuste de `Image`

## Estado y aprobación

- Estado: aprobada por DLG el 10-X-2026.
- Referencia de aprobación: «Revisada».

## Objetivo

Proporcionar en Domain el ajuste determinista de la URL de imagen de un enlace, de modo que produzca la URL ajustada que guardará `Link.Image`, indique que la imagen no está informada o la rechace con un error de dominio específico, sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.2 y reparto con los trabajos vecinos.
- [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image): pasos, rechazos, resolución de referencias relativas y ejemplos del ajuste.
- [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri): regla común de esquema explícito y de forma host y puerto.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): límite de `Link.Image`.
- [specifications.md → «Validación»](../../context/specifications.md#validación): definición de host, IP y fase posterior que queda fuera de este trabajo.
- [domain-model.md → «Link»](../../context/domain-model.md#link): `Image` guarda la URL ajustada y validada.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.
- [decisions.md → «Pendientes de scraping»](../../context/decisions.md#pendientes-de-scraping): base de resolución cuando la página del scraper ha redirigido, fuera de este trabajo.
- [Ajuste de Imagen (diagrama)](../../context/diagrams/process-adjustment-image.md): vista derivada del proceso.

## Alcance

- Incluye:
  - Implementar en Domain el ajuste de `Image` conforme a [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image): imagen no informada, rechazo de caracteres de control y saltos de línea, `//`, esquemas admitidos con cambio de `http` a `https`, forma host y puerto, resolución de referencias relativas contra `Link.UrlOriginal`, rechazo de `localhost` y de IP y longitud máxima.
  - Reutilizar la detección de esquema URI y el reconocimiento de host de S2.1, sin duplicarlos.
  - Reconocer si el host resultante es `localhost` o una IP (IPv4 o IPv6 entre corchetes, según [«Validación»](../../context/specifications.md#validación)), sin validar la URL completa.
  - Comunicar cada rechazo con un error de dominio específico, sin incluir el texto introducido, como en el ajuste de `UrlOriginal`.
  - Aplicar las mismas reglas con independencia de si la imagen la introdujo el usuario o la obtuvo el scraper.
  - Pruebas automatizadas del comportamiento que pertenece a este trabajo; S2.5 completa la cobertura de todos los ejemplos de las especificaciones.
- Excluye:
  - Recortar el texto: lo hace quien recibe lo introducido por el usuario, según [«Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario).
  - Ajustar `UrlOriginal` (S2.1) y validar la URL ajustada (S2.3).
  - Calcular `UrlNormalized` y detectar duplicados (S2.4).
  - Decidir qué ocurre al rechazar según el origen de la imagen (error en el formulario, o descarte por el scraper) y obtener `Link.UrlOriginal`; corresponde a las etapas de Application y Web, y al scraping.
  - Decidir la base de resolución cuando la página del scraper ha redirigido (pendiente en [decisions.md → «Pendientes de scraping»](../../context/decisions.md#pendientes-de-scraping)).
  - Integrar la imagen ajustada en la entidad `Link` y sus invariantes; corresponde a S3.
  - Realizar peticiones de red, resolver nombres de dominio o seguir redirecciones.
  - Definir o cambiar reglas, límites, esquemas o mensajes de producto.
- Trabajo vecino y responsable de sus entregables:
  - S2.1 (implementada y aceptada) proporciona la detección de esquema URI, el reconocimiento de host y el ajuste de `UrlOriginal`.
  - S2.3 implementa la validación de la URL ajustada y de `Image`.
  - S2.4 implementa `UrlNormalized` y la regla de duplicados.
  - S2.5 escribe los tests con todos los ejemplos de las especificaciones de URL, separando `UrlOriginal` e `Image`.
  - S2.6 y S2.7 aportan la revisión humana con URLs reales y la decisión de los casos dudosos.
  - S2.8 unifica las validaciones de entrada de Domain.

## Dependencias y prerrequisitos

- Existen los proyectos `Linkubator.Domain` y de tests, con la detección de esquema URI y el reconocimiento de host de S2.1, aceptada por DLG el 10-X-2026.
- `Link.UrlOriginal`, base de la resolución relativa, llega ya ajustada y validada; este trabajo no la ajusta ni la valida.
- S2.3 a S2.5 no están implementadas; este trabajo no depende de ellas.
- La especificación no presupone que exista la entidad `Link`.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada vacía o formada solo por espacios. | Domain ajusta la entrada. | Se trata como imagen no informada, sin error y distinguible de una URL ajustada, y no continúa a validación. |
| CA02 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada nula. | Domain ajusta la entrada. | Incumple la precondición y se rechaza como argumento no válido; es la única entrada que la incumple. |
| CA03 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada con caracteres de control o saltos de línea. | Domain ajusta la entrada. | Se rechaza con un error de dominio específico, sin incluir el texto introducido. |
| CA04 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada que empieza por `//`. | Domain ajusta la entrada. | Se le añade `https:` y continúa con las comprobaciones de host y longitud. |
| CA05 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada con esquema explícito. | Domain ajusta la entrada. | `http` se cambia a `https`, `https` se acepta sin distinguir mayúsculas y cualquier otro esquema se rechaza con un error de dominio específico. |
| CA06 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Un esquema explícito `http` o `https` sin `://` y una autoridad válida, o con otra sintaxis de autoridad. | Domain ajusta la entrada. | Se rechaza con un error de dominio específico, sin incluir el texto introducido. |
| CA07 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada con la forma de host y puerto. | Domain ajusta la entrada. | Se trata como autoridad, se le añade `https://` y continúa con las comprobaciones de host y longitud. |
| CA08 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada sin esquema ni forma de host y puerto, aunque su primer segmento parezca un nombre de dominio, y una URL base. | Domain ajusta la entrada. | Se trata como referencia relativa y se resuelve contra la base según las reglas estándar de resolución URI. |
| CA09 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Referencias relativas de ruta, de raíz, con `..`, con ruta y consulta, solo con consulta y solo con fragmento. | Domain ajusta cada referencia. | Cada una conserva o sustituye ruta, consulta y fragmento de la base según lo indicado en la fuente. |
| CA10 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una URL resultante que usa `http` tras la resolución relativa. | Domain ajusta la entrada. | Su esquema pasa a `https`. |
| CA11 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Validación»](../../context/specifications.md#validación) | Una entrada cuyo host resultante es `localhost`, una IPv4 o una IPv6 entre corchetes, por cualquiera de las vías de la fuente, incluida una base con ese host. | Domain ajusta la entrada. | Se rechaza con un error de dominio específico, sin incluir el texto introducido. |
| CA12 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Un resultado cuya longitud, con el esquema incluido, supera el límite de `Link.Image`, y otro que lo iguala. | Domain ajusta la entrada. | El primero se rechaza con el error de dominio de longitud máxima y el segundo se acepta. |
| CA13 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Cualquier entrada que el ajuste rechaza. | Domain procesa la entrada. | No devuelve una URL ajustada: lanza el error de dominio y el flujo no continúa a validación. Quien lo captura decide el mensaje y el tratamiento según el origen. |
| CA14 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada y una base iguales, introducidas por el usuario o obtenidas por el scraper. | Domain ajusta la entrada. | El resultado es idéntico en ambos orígenes. |
| CA15 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Validación»](../../context/specifications.md#validación) | Una entrada aceptada por el ajuste. | Domain devuelve la URL ajustada. | Solo contiene las transformaciones del ajuste, sin otras como cambiar las mayúsculas del host. |
| CA16 | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Las entradas de la tabla de ejemplos de «Ajuste de `Image`». | Domain ajusta cada entrada, con su base cuando se indica. | El resultado coincide con el de la tabla. |
| CA17 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | La implementación del ajuste de `Image`. | Se examina su uso de la detección de esquema y del reconocimiento de host de S2.1. | Reutiliza esas reglas comunes sin duplicarlas. |
| CA18 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | La implementación de S2.2. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain, no accede a red ni a resolución de nombres y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. Resuelta: una entrada de solo espacios cuenta como imagen no informada. Decidido por DLG el 10-X-2026 («Una fuente de solo espacios cuenta como no informada»). | [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).
