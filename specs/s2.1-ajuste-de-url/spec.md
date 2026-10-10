# Especificación: S2.1 — Detección de esquema URI y ajuste de `UrlOriginal`

## Estado y aprobación

- Estado: aprobada por DLG el 10-X-2026.
- Referencia de aprobación: «Aceptada revisión».
- Historial: reabierta el 10-X-2026 para unificar el tratamiento de errores y precondiciones con alias y slugs. La aprobación anterior de DLG («Validada») cubría la versión previa y quedó sustituida por esta.

## Objetivo

Proporcionar en Domain la detección de esquema URI y el ajuste determinista de la URL introducida para un enlace, de modo que produzca la URL ajustada que guardará `Link.UrlOriginal` o la rechace con un error de dominio específico, sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.1 y reparto con los trabajos vecinos.
- [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri): regla común de reconocimiento de host y puerto y de esquema explícito.
- [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url): pasos, rechazos y ejemplos del ajuste.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): límite de `Link.UrlOriginal`.
- [specifications.md → «Validación»](../../context/specifications.md#validación): definición de host válido que usa la detección y fase posterior que queda fuera de este trabajo.
- [domain-model.md → «Link»](../../context/domain-model.md#link): `UrlOriginal` guarda la URL ajustada y validada, y es inmutable.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.
- [Ajuste de URL (diagrama)](../../context/diagrams/process-adjustment-url.md): vista derivada del proceso.

## Alcance

- Incluye:
  - Implementar en Domain la detección de esquema URI conforme a [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri), como regla común reutilizable por el ajuste de `Image`.
  - Implementar el reconocimiento de host válido que esa detección necesita, con las condiciones de host de [«Validación»](../../context/specifications.md#validación), sin validar la URL completa. Se ofrece en dos formas: una que extrae el host de la entrada completa y lanza un error de dominio si no es válido, y otra que no lanza y usa la detección.
  - Implementar el ajuste de la URL del enlace conforme a [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url): rechazo de caracteres de control y saltos de línea, `//`, esquemas admitidos, esquema por defecto y longitud máxima. Cada rechazo se comunica con un error de dominio específico, sin incluir el texto introducido, como en alias y slugs.
  - Exigir como precondición, igual que una entrada nula, que la entrada no esté vacía ni en blanco; no se recorta el texto.
  - Pruebas automatizadas del comportamiento de detección y ajuste que pertenece a este trabajo; S2.5 completa la cobertura de todos los ejemplos de las especificaciones.
- Excluye:
  - Recortar el texto: lo hace quien recibe lo introducido por el usuario, según [«Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario).
  - Modificar las políticas de alias y slugs; la unificación de precondiciones con ellas corresponde a S2.8.
  - Ajustar `Image` (S2.2) y validar la URL ya ajustada (S2.3).
  - Calcular `UrlNormalized` y detectar duplicados (S2.4).
  - Mostrar errores en el formulario, conservar la URL introducida y decidir la traducción del rechazo a resultados de aplicación; corresponde a las etapas de Application y Web.
  - Integrar la URL ajustada en la entidad `Link` y sus invariantes; corresponde a S3.
  - Realizar peticiones de red, resolver nombres de dominio o seguir redirecciones.
  - Definir o cambiar reglas, límites, esquemas o mensajes de producto.
- Trabajo vecino y responsable de sus entregables:
  - S1.1 a S1.7 (aceptadas) proporcionan los textos, errores de dominio y políticas de Domain que este trabajo puede consumir.
  - S2.2 implementa el ajuste de `Image` y reutiliza la detección de esquema de este trabajo.
  - S2.3 implementa la validación de la URL ajustada y de `Image`, y amplía el reconocimiento de host de este trabajo hasta la validación completa del host.
  - S2.4 implementa `UrlNormalized` y la regla de duplicados.
  - S2.5 escribe los tests con todos los ejemplos de las especificaciones de URL, separando `UrlOriginal` e `Image`.
  - S2.6 y S2.7 aportan la revisión humana con URLs reales y la decisión de los casos dudosos.
  - S2.8 unifica las validaciones de entrada, incluida la precondición de entrada no vacía en alias y slugs.

## Dependencias y prerrequisitos

- Existen el proyecto `Linkubator.Domain` con las políticas y errores de dominio de S1 y el proyecto de tests con referencia a Domain.
- S2.2 a S2.5 no están implementadas; este trabajo no depende de ellas.
- La especificación no presupone que exista la entidad `Link`.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | Una entrada que comienza por un host válido seguido de `:`, un puerto de solo dígitos y, a continuación, el fin de la entrada o `/`, `?` o `#` (incluido IPv6 entre corchetes). | Domain detecta si tiene esquema. | No se interpreta como esquema. |
| CA02 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | Una entrada que no tiene la forma de host y puerto y cuyo prefijo anterior al primer `/`, `?` o `#` cumple la forma de esquema. | Domain detecta si tiene esquema. | Se reconoce el esquema explícito. Una entrada con `:` cuyo host no es válido según [«Validación»](../../context/specifications.md#validación) se evalúa por esta regla. |
| CA03 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) y [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Un esquema explícito `http` o `https` sin `://` y una autoridad válida, o con otra sintaxis de autoridad. | Domain ajusta la entrada. | Se rechaza con un error de dominio específico, sin incluir el texto introducido. |
| CA04 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Una entrada nula, vacía o solo con espacios, y otra con caracteres de control o saltos de línea. | Domain ajusta la entrada. | La entrada nula, vacía o en blanco incumple la precondición y se rechaza como argumento no válido, sin recortarla. La que tiene caracteres de control o saltos de línea se rechaza con un error de dominio específico, sin incluir el texto introducido. |
| CA05 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Una entrada que empieza por `//`. | Domain ajusta la entrada. | Se le añade `https:` y continúa con la comprobación de longitud. |
| CA06 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Una entrada con esquema explícito. | Domain ajusta la entrada. | Se acepta solo `http` o `https`, sin distinguir mayúsculas; cualquier otro esquema se rechaza con un error de dominio específico. |
| CA07 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Una entrada sin esquema, incluida la forma de host y puerto. | Domain ajusta la entrada. | Se le añade `http://` si el host es `localhost` y `https://` en cualquier otro caso. |
| CA08 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Un resultado cuya longitud, con el esquema incluido, supera el límite de `Link.UrlOriginal`, y otro que lo iguala. | Domain ajusta la entrada. | El primero se rechaza con el error de dominio de longitud máxima y el segundo se acepta. |
| CA09 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Cualquier entrada que el ajuste rechaza. | Domain procesa la entrada. | No devuelve una URL ajustada: lanza el error de dominio y el flujo no continúa a validación. Quien lo captura decide el mensaje que muestra. |
| CA10 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) y [«Validación»](../../context/specifications.md#validación) | Una entrada aceptada por el ajuste. | Domain devuelve la URL ajustada. | Solo contiene las transformaciones del ajuste: conserva las mayúsculas del host, la ruta, la consulta y el fragmento introducidos. |
| CA11 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Las entradas de la tabla de ejemplos de «Ajuste de URL». | Domain ajusta cada entrada. | El resultado coincide con el de la tabla. |
| CA12 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | La implementación de la detección. | Se examina su uso por el ajuste de `Image` (S2.2). | La detección es una regla común independiente del ajuste de `UrlOriginal`, utilizable por S2.2 sin duplicarla. |
| CA13 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | La implementación de S2.1. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain, no accede a red ni a resolución de nombres y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. Resuelta: como [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) exige reconocer un host válido, S2.1 implementa el reconocimiento de host necesario para la detección y S2.3 lo amplía. Decidido por DLG el 10-X-2026 («Vale, hagamos eso y que las siguientes features la amplien»). | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).
