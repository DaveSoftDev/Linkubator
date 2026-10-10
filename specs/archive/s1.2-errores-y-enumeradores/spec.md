# Especificación: S1.2 — Errores de dominio base y enumeradores

## Estado y aprobación

- Estado: aprobada por DLG el 09-X-2026.
- Referencia de aprobación: «Excepciones de dominio identificadas».
- Actualización del contrato: aprobada por DLG el 09-X-2026 («hazlo con la base y las específicas»).
- Este documento describe el resultado esperado; el diseño y la evidencia de implementación pertenecerán a los artefactos relacionados cuando existan.

## Objetivo

Disponer en Domain de una excepción base para capturar errores de dominio en conjunto y de excepciones específicas por regla, empezando por el rechazo de textos que superan su longitud máxima, junto con los enumeradores documentados en el modelo, sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.2 y reparto con las tareas vecinas.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): Domain contiene enumeraciones y excepciones de dominio, es independiente de frameworks externos; el patrón `Result` pertenece a Application.
- [domain-model.md → «UserToken»](../../../context/domain-model.md#usertoken): significado de `Purpose`.
- [domain-model.md → «Link»](../../../context/domain-model.md#link): propiedad `ScrapingStatus`.
- [specifications.md → «Tokens»](../../../context/specifications.md#tokens): propósitos de token existentes.
- [specifications.md → «Estados del scraping»](../../../context/specifications.md#estados-del-scraping): valores de `ScrapingStatus`.
- [specifications.md → «Contraseñas»](../../../context/specifications.md#contraseñas) y [«Registro de eventos»](../../../context/specifications.md#registro-de-eventos): datos sensibles que no pueden aparecer en claro.

## Alcance

- Incluye:
  - `DomainException`, el tipo base de error de dominio en Domain, distinguible de otras excepciones y de las excepciones específicas.
  - `TextExceedsMaximumLengthException`, excepción específica para indicar que un texto supera su longitud máxima; expone el código estable `TextExceedsMaximumLength`.
  - El enumerador de `Link.ScrapingStatus` con los valores de [«Estados del scraping»](../../../context/specifications.md#estados-del-scraping).
  - El enumerador de `UserToken.Purpose` con un valor por cada propósito de [«Tokens»](../../../context/specifications.md#tokens).
  - Tests automatizados de los enumeradores, la jerarquía de excepciones y `TextExceedsMaximumLengthException`.

- Excluye:
  - Los errores concretos de alias y slugs y el momento en que se lanzan; corresponden a S1.3 y S1.4 (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)).
  - Las entidades, sus constructores, valores iniciales y transiciones de estado; corresponden a S3.
  - El patrón `Result` y la traducción de errores a respuestas o mensajes de usuario; corresponden a Application y Web.
  - La caducidad de cada propósito de token y su persistencia; corresponden a S4 y a la etapa de persistencia.
  - Definir o cambiar reglas, estados, propósitos o mensajes de producto.

- Trabajo vecino y responsable de sus entregables:
  - S1.1 (aceptada) entrega el recorte y la comprobación de longitudes; no lanza errores de dominio.
  - S1.3 y S1.4 consumen el error base para sus rechazos.
  - S3 usa los enumeradores y el error base al implementar las entidades.

## Dependencias y prerrequisitos

- Existe el proyecto `Linkubator.Domain`, configurado para .NET 10, y un proyecto de tests con referencia a Domain.
- S1.1 está aceptada; S1.2 no depende de su código.
- `ScrapingStatus` y `Purpose` son los únicos enumeradores de este trabajo. No representan errores de dominio: sus valores solo los modifica código interno y cualquier valor corrupto se trata durante el mapeo de persistencia.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) | Un error de dominio lanzado desde Domain. | Un consumidor lo captura. | Puede distinguirlo de cualquier otra excepción por su tipo base, sin depender del texto del mensaje. |
| CA02 | [«Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Una entidad comprueba un texto con `UserTextPolicy.IsWithinMaximumLength` y el resultado es `false`. | Protege su invariante de longitud. | Lanza `TextExceedsMaximumLengthException`, que deriva de `DomainException` y expone el código `TextExceedsMaximumLength`. |
| CA03 | [«Contraseñas»](../../../context/specifications.md#contraseñas) y [«Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | El contrato de la jerarquía de excepciones. | Se revisa qué datos admite. | No incluye ni requiere valores introducidos por el usuario, contraseñas ni tokens. |
| CA04 | [«Estados del scraping»](../../../context/specifications.md#estados-del-scraping) | El enumerador de `ScrapingStatus`. | Se enumeran sus valores. | Contiene exactamente los estados de la fuente, con sus mismos nombres, ni más ni menos. |
| CA05 | [«Tokens»](../../../context/specifications.md#tokens) y [domain-model.md → «UserToken»](../../../context/domain-model.md#usertoken) | El enumerador de `Purpose`. | Se enumeran sus valores. | Contiene exactamente un valor por cada propósito de la fuente, ni más ni menos. |
| CA06 | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) | La implementación de S1.2. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para el alcance acordado. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).
