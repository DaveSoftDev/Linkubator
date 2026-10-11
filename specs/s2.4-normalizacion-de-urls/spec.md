# Especificación: S2.4 — Normalización de URL (`UrlNormalized`)

## Estado y aprobación

- Estado: aprobada por DLG el 11-X-2026.
- Referencia de aprobación: «Comprendida».

## Objetivo

Proporcionar en Domain el cálculo determinista de la clave plana `UrlNormalized` a partir de una URL ya ajustada y validada, sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.4 y reparto con los trabajos vecinos.
- [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados): fases, parámetros de marketing eliminados, ejemplos y colisiones aceptadas.
- [specifications.md → «Duplicados»](../../context/specifications.md#duplicados): ausencia de seguimiento de redirecciones.
- [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces): la URL guardada no se transforma más allá del ajuste.
- [specifications.md → «Validación»](../../context/specifications.md#validación): la URL que se normaliza llega ajustada y validada, con esquema siempre presente.
- [domain-model.md → «Link»](../../context/domain-model.md#link) y [«Restricciones de unicidad»](../../context/domain-model.md#restricciones-de-unicidad): naturaleza de `UrlNormalized` y su unicidad por usuario.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.
- [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados): colisiones aceptadas de la clave plana.
- [Normalización de URL para detectar duplicados (diagrama)](../../context/diagrams/process-url-normalization.md): vista derivada del proceso.

## Alcance

- Incluye:
  - Implementar en Domain el cálculo de `UrlNormalized` conforme a [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados), en sus dos fases.
  - Exigir como precondición que la entrada no sea nula, vacía ni esté en blanco, sin recortarla, como en el ajuste y la validación. No se revalida que la URL esté ajustada y validada: el flujo garantiza que llega así.
  - Pruebas automatizadas del comportamiento que pertenece a este trabajo, incluidos los caracteres estructurales codificados (`%23`, `%26`), el texto codificado dos veces (`%2520`) y `+` en la ruta y en la consulta; S2.5 completa la cobertura de todos los ejemplos de las especificaciones de URL.
- Excluye:
  - Ajustar `UrlOriginal` o `Image` (S2.1 y S2.2) y validarlas (S2.3).
  - Modificar la URL guardada: `UrlOriginal` conserva lo ajustado y la clave solo sirve para detectar duplicados.
  - Consultar enlaces existentes, indicar en qué colección está el enlace duplicado, rechazar la creación y la restricción única de SQLite; corresponden a Application y a la persistencia. La comparación de dos claves queda fuera de Domain.
  - Integrar `UrlNormalized` en la entidad `Link` y sus invariantes; corresponde a S3.
  - Realizar peticiones de red, resolver nombres de dominio o seguir redirecciones.
  - Definir o cambiar reglas, límites, listas o ejemplos de producto, o la lista de parámetros de marketing.
- Trabajo vecino y responsable de sus entregables:
  - S2.1, S2.2 y S2.3 proporcionan el ajuste y la validación de la URL que este trabajo recibe ya ajustada y validada.
  - S2.5 escribe los tests con todos los ejemplos de las especificaciones de URL, separando `UrlOriginal` e `Image`.
  - S2.7 y S2.8 aportan la revisión humana de la normalización con URLs reales y la decisión de los casos dudosos.
  - S2.6 unifica las validaciones de entrada de Domain.
  - S3 integra la clave en la entidad `Link`; las etapas de Application y persistencia detectan el duplicado entre enlaces guardados.

## Dependencias y prerrequisitos

- Existen los proyectos `Linkubator.Domain` y de tests, con las políticas de URL de S2.1, S2.2 y S2.3. S2.3 está implementada y aceptada por DLG el 11-X-2026 según [roadmap.md](../../context/roadmap.md); este trabajo no depende de su aceptación, pues recibe la URL ya validada y no reutiliza su resultado.
- La URL que se normaliza llega ya ajustada y validada; este trabajo no la ajusta ni la valida.
- La especificación no presupone que exista la entidad `Link` ni la persistencia.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) y [«Validación»](../../context/specifications.md#validación) | Una entrada nula, vacía o solo con espacios. | Domain calcula la clave. | Incumple la precondición y se rechaza como argumento no válido, sin recortarla. |
| CA02 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Dos URL que solo difieren en el fragmento. | Domain calcula la clave de cada una. | Las claves son iguales. |
| CA03 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | URL con puerto `80` en HTTP, con puerto `443` en HTTPS, sin puerto y con otro puerto. | Domain calcula la clave de cada una. | Los puertos predeterminados de su esquema no figuran en la clave; cualquier otro puerto sí figura. |
| CA04 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Dos URL que solo difieren en `http` o `https`. | Domain calcula la clave de cada una. | Las claves son iguales. |
| CA05 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Un dominio internacionalizado escrito en Unicode y en punycode. | Domain calcula la clave de cada uno. | Las claves son iguales; las rutas en otros alfabetos se conservan. |
| CA06 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Una ruta con texto codificado y con `+`. | Domain calcula la clave. | La ruta se decodifica una sola vez y `+` se trata como carácter literal. |
| CA07 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Una consulta con nombres y valores codificados y con `+`. | Domain calcula la clave. | Cada nombre y valor se decodifica una sola vez con la regla de formularios, en la que `+` equivale a un espacio. |
| CA08 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | URL con cada parámetro de la lista de marketing, escrito en mayúsculas y en minúsculas, junto con parámetros no incluidos en ella. | Domain calcula la clave. | Los de la lista se eliminan sin distinguir mayúsculas y los demás se conservan. |
| CA09 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | URL con los mismos parámetros en distinto orden y con distintas mayúsculas, y con parámetros repetidos. | Domain calcula la clave. | Nombres y valores restantes pasan a minúsculas sin reglas de idioma, se ordenan por comparación ordinal y los repetidos se conservan; el orden original no altera la clave. |
| CA10 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | URL con `%23` y `%26` en la ruta y en la consulta. | Domain calcula la clave. | No cortan la URL como fragmento ni separan parámetros: se convierten en texto y desaparecen al aplanar. |
| CA11 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Una URL con texto codificado dos veces (`%2520`). | Domain calcula la clave. | Se decodifica una sola vez y el `%20` resultante se conserva como texto, que desaparece al aplanar. |
| CA12 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Una cadena unida con mayúsculas, acentos, marcas diacríticas, `ç`, `ñ`, espacios, caracteres de otros alfabetos, signos y guiones duplicados, iniciales y finales. | Domain aplica la segunda fase. | La clave queda en minúsculas, sin marcas diacríticas, con `ç` y `ñ` convertidas, con los espacios como guiones, solo con letras y números de cualquier alfabeto y guiones, sin guiones duplicados ni en los extremos, y sin volver a decodificar nada. |
| CA13 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Los ejemplos de «Normalización para duplicados», incluida la colisión aceptada. | Domain calcula la clave de cada entrada. | El resultado coincide con el de cada ejemplo. |
| CA14 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Una misma URL calculada con distintas culturas del entorno de ejecución. | Domain calcula la clave. | El resultado es idéntico: ninguna conversión depende de la cultura. |
| CA15 | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados), [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Validación»](../../context/specifications.md#validación) | Las pruebas existentes de ajuste, validación, alias y slugs. | Se ejecutan tras añadir este trabajo. | Siguen correctas sin modificar su comportamiento. |
| CA16 | [«URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces) y [«Duplicados»](../../context/specifications.md#duplicados) | La URL ajustada y validada de un enlace. | Domain calcula su clave. | La URL de entrada no se modifica y no se sigue ninguna redirección ni se accede a la red. |
| CA17 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | La implementación de S2.4. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain, no accede a red ni a resolución de nombres y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno pendiente. | [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | — | — |

Decisiones de DLG del 11-X-2026:

- La consulta de enlaces guardados, el aviso de la colección y la restricción única corresponden a Application y persistencia. S2.4 solo implementa la normalización en Domain. Referencia: «Me parece bien» y «Pues sí, elimina lo relacionado con los duplicados porque no tiene mucho sentido».
- La normalización recibe la URL del flujo de ajuste, validación y normalización, por lo que no revalida una URL ajustada y validada. Cada función sí comprueba que sus parámetros de entrada tengan valor (CA01). Referencia: «no me parece mal que todas las funciones comprueben que sus parámetros de entrada […] deberán tener valor».
## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 11-X-2026.
