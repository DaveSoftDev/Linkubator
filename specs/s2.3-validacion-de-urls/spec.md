# Especificación: S2.3 — Validación de `UrlOriginal` e `Image`

## Estado y aprobación

- Estado: aprobada por DLG el 10-X-2026.
- Referencia de aprobación: «Listo».

## Objetivo

Proporcionar en Domain la validación determinista de una URL ya ajustada, común a `Link.UrlOriginal` y `Link.Image`, de modo que acepte la URL tal cual o la rechace con un error de dominio específico, y completar el reconocimiento de host de S2.1 hasta las condiciones de host de la validación, sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.3 y reparto con los trabajos vecinos.
- [specifications.md → «Validación»](../../context/specifications.md#validación): condiciones de validación, tratamiento de la URL aceptada y ejemplos.
- [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri): reconocimiento de host válido que usa la detección.
- [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image): fase previa que produce la URL que se valida y rechazos que quedan fuera de este trabajo.
- [domain-model.md → «Link»](../../context/domain-model.md#link): `UrlOriginal` e `Image` guardan la URL ajustada y validada.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.
- [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados): aceptación del riesgo asociado a direcciones privadas.
- [Validación de URL (diagrama)](../../context/diagrams/process-url-validation.md): vista derivada del proceso.

## Alcance

- Incluye:
  - Implementar en Domain la validación de una URL ajustada conforme a [«Validación»](../../context/specifications.md#validación): URI absoluta válida para `System.Uri`, esquema `http` o `https`, formato de host permitido, ausencia de credenciales y puerto válido.
  - Aplicar el mismo proceso a `UrlOriginal` e `Image`, sin reglas propias de cada campo.
  - Comunicar cada rechazo con un error de dominio específico, sin incluir el texto validado, como en el ajuste.
  - Exigir como precondición que la entrada no sea nula, vacía ni esté en blanco, sin recortarla, como en el ajuste de `UrlOriginal`.
  - Ampliar el reconocimiento de host de S2.1 hasta las condiciones completas de host de [«Validación»](../../context/specifications.md#validación), reutilizándolo en la validación y en la detección de esquema sin duplicarlo.
  - Pruebas automatizadas del comportamiento que pertenece a este trabajo; S2.5 completa la cobertura de todos los ejemplos de las especificaciones.
- Excluye:
  - Recortar el texto: lo hace quien recibe lo introducido por el usuario, según [«Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario).
  - Ajustar `UrlOriginal` (S2.1) o `Image` (S2.2), incluido el rechazo de `localhost` y de IP en `Image`.
  - Calcular `UrlNormalized` y detectar duplicados (S2.4).
  - Decidir qué ocurre al rechazar según el origen de `Image` (error en el formulario o enlace sin imagen) y mostrar errores; corresponde a las etapas de Application y Web, y al scraping.
  - Orquestar ajuste y validación en una operación única o integrarlos en la entidad `Link` y sus invariantes; corresponde a S3 y posteriores.
  - Realizar peticiones de red, resolver nombres de dominio o seguir redirecciones.
  - Definir o cambiar reglas, límites, esquemas o mensajes de producto.
- Trabajo vecino y responsable de sus entregables:
  - S2.1 y S2.2 (implementadas y aceptadas) proporcionan el ajuste de `UrlOriginal` e `Image`, la detección de esquema URI y el reconocimiento de host que este trabajo amplía.
  - S2.4 implementa `UrlNormalized` y la regla de duplicados.
  - S2.5 escribe los tests con todos los ejemplos de las especificaciones de URL, separando `UrlOriginal` e `Image`.
  - S2.7 y S2.8 aportan la revisión humana con URLs reales y la decisión de los casos dudosos.
  - S2.6 unifica las validaciones de entrada de Domain.

## Dependencias y prerrequisitos

- Existen los proyectos `Linkubator.Domain` y de tests, con la detección de esquema URI, el reconocimiento de host y los ajustes de S2.1 y S2.2, aceptadas por DLG el 10-X-2026.
- La URL que se valida llega ya ajustada; este trabajo no la ajusta.
- S2.4 y S2.5 no están implementadas; este trabajo no depende de ellas.
- La especificación no presupone que exista la entidad `Link`.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Validación»](../../context/specifications.md#validación) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada nula, vacía o solo con espacios. | Domain valida la entrada. | Incumple la precondición y se rechaza como argumento no válido, sin recortarla; una imagen no informada no llega a validación. |
| CA02 | [«Validación»](../../context/specifications.md#validación) | Una entrada que no es una URI absoluta válida para `System.Uri`, como una referencia relativa. | Domain valida la entrada. | Se rechaza con un error de dominio específico, sin incluir el texto validado. |
| CA03 | [«Validación»](../../context/specifications.md#validación) | Una URI absoluta con un esquema distinto de `http` y `https`. | Domain valida la entrada. | Se rechaza con un error de dominio específico. |
| CA04 | [«Validación»](../../context/specifications.md#validación) | Una URL cuyo host es `localhost`, una IPv4, una IPv6 entre corchetes, un dominio con al menos un punto y TLD de 2 o más letras, o un dominio en formato `xn--`, en Unicode o en punycode. | Domain valida la entrada. | Se acepta. |
| CA05 | [«Validación»](../../context/specifications.md#validación) | Una URL cuyo host no cumple ninguno de esos formatos, como un nombre sin punto o con TLD de menos de 2 letras. | Domain valida la entrada. | Se rechaza con un error de dominio específico. |
| CA06 | [«Validación»](../../context/specifications.md#validación) | Una URL con credenciales (`usuario:clave@`). | Domain valida la entrada. | Se rechaza con un error de dominio específico, sin incluir las credenciales. |
| CA07 | [«Validación»](../../context/specifications.md#validación) | Una URL sin puerto, una con puerto válido y una con puerto no válido. | Domain valida la entrada. | Las dos primeras se aceptan y la tercera se rechaza con un error de dominio específico. |
| CA08 | [«Validación»](../../context/specifications.md#validación) | Cualquier entrada que la validación rechaza. | Domain procesa la entrada. | Lanza el error de dominio y no se considera válida para guardarse. Quien lo captura decide el mensaje y el tratamiento según el origen. |
| CA09 | [«Validación»](../../context/specifications.md#validación) | La misma URL ajustada, destinada a `UrlOriginal` o a `Image`. | Domain valida la entrada. | El resultado es idéntico en ambos casos. |
| CA10 | [«Validación»](../../context/specifications.md#validación) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una URL con host `localhost` o IP. | Domain valida la entrada. | La validación la acepta; el rechazo de esos hosts en `Image` pertenece al ajuste de S2.2 y no se repite aquí. |
| CA11 | [«Validación»](../../context/specifications.md#validación) | Una URL aceptada por la validación. | Domain termina la validación. | La URL conserva exactamente lo ajustado, sin ninguna otra transformación, como cambiar las mayúsculas del host. |
| CA12 | [«Validación»](../../context/specifications.md#validación) | Las entradas de la tabla de ejemplos de «Validación». | Domain ajusta cada entrada con S2.1 y valida el resultado cuando no se rechaza en el ajuste. | El resultado final coincide con el de la tabla. |
| CA13 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) y [«Validación»](../../context/specifications.md#validación) | Una entrada con `:` cuyo host no cumple las condiciones de host de la validación. | Domain detecta si tiene esquema. | Se evalúa por la regla de esquema explícito, no como host y puerto. |
| CA14 | [«Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | La implementación de la validación y de la detección de esquema. | Se examina su uso del reconocimiento de host. | Comparten una única implementación de las condiciones de host, sin duplicarlas. |
| CA15 | [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Las entradas de las tablas de ejemplos de «Ajuste de URL» y «Ajuste de `Image`». | Domain ajusta cada entrada tras ampliar el reconocimiento de host. | El resultado sigue coincidiendo con el de cada tabla. |
| CA16 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | La implementación de S2.3. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain, no accede a red ni a resolución de nombres y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. | [«Validación»](../../context/specifications.md#validación) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).
