# Especificación: S2.5 — Tests de los ejemplos de URL, separando `UrlOriginal` e `Image`

## Estado y aprobación

- Estado: aprobada por DLG el 11-X-2026.
- Referencia de aprobación: «Ready».

## Objetivo

Que todos los ejemplos y casos concretos de las fuentes de ajuste, validación y normalización de URL estén cubiertos por pruebas automatizadas de Domain que los ejecuten contra la implementación de S2.1 a S2.4, con `UrlOriginal` e `Image` comprobadas por separado y con una trazabilidad comprobable entre cada ejemplo de la fuente y su prueba. El resultado es la cobertura completa y verificable, no un cambio de comportamiento.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.5, Definition of Done del sprint y reparto con los trabajos vecinos.
- [specifications.md → «Detección de esquema URI»](../../../context/specifications.md#detección-de-esquema-uri): reconocimiento de host y puerto frente a esquema explícito, que determinan varios ejemplos.
- [specifications.md → «Ajuste de URL»](../../../context/specifications.md#ajuste-de-url): ajuste de `UrlOriginal`, esquemas rechazados y tabla de ejemplos.
- [specifications.md → «Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image): ajuste de `Image`, resolución de referencias relativas y tabla de ejemplos.
- [specifications.md → «Validación»](../../../context/specifications.md#validación): validación común a ambas y tabla de ejemplos.
- [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados): ejemplos de `UrlNormalized`, derivaciones citadas y colisión aceptada.
- [domain-model.md → «Link»](../../../context/domain-model.md#link): naturaleza de `UrlOriginal`, `Image` y `UrlNormalized`.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): aislamiento de Domain.
- [roadmap.md → «Hecho»](../../../context/roadmap.md#hecho): estado de la etapa.

## Alcance

- Incluye:
  - Auditar cada ejemplo de las tablas de [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image) y [«Validación»](../../../context/specifications.md#validación), y cada ejemplo de [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados), contra las pruebas existentes de S2.1 a S2.4, e identificar los que no tienen una prueba que los ejecute tal como los escribe la fuente.
  - Añadir las pruebas que falten y, cuando un ejemplo esté cubierto de forma indirecta, una comprobación que lo ejecute con su texto exacto.
  - Comprobar `UrlOriginal` con el ajuste de `UrlOriginal` y `Image` con el ajuste de `Image`, en pruebas diferenciadas, y los textos que valen para ambas con el resultado propio de cada una.
  - Comprobar los rechazos citados por las fuentes (esquemas no permitidos, `http:` sin `://`, host sin dominio, credenciales, `localhost` e IP en `Image`) con el tipo de error de dominio que corresponde.
  - Comprobar la colisión aceptada de la normalización partiendo de las entradas tal como las escribe la fuente, a través del ajuste y la validación de `UrlOriginal`.
  - Dejar la trazabilidad ejemplo de la fuente → prueba en `tasks.md`, sin copiar la regla.
- Excluye:
  - Cambiar el comportamiento, las fuentes, las listas o los mensajes. Si una prueba revela una divergencia con la fuente, se registra como bloqueo y se propone el cambio en su documento propietario (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)).
  - Reescribir las pruebas aceptadas de S2.1 a S2.4 salvo lo necesario para completar la cobertura de un ejemplo.
  - Calcular `UrlNormalized` a partir de `Image`: la clave pertenece a `UrlOriginal`.
  - Consultar o resolver duplicados entre enlaces guardados, que requieren entidades, casos de uso y persistencia.
  - Revisar la normalización con URLs reales (S2.7), decidir los casos dudosos (S2.8) y unificar las validaciones de entrada de Domain (S2.6).
- Trabajo vecino y responsable de sus entregables:
  - S2.1, S2.2, S2.3 y S2.4 entregaron la implementación y las pruebas sobre las que se audita.
  - S2.7 y S2.8 (humano) revisan la normalización con datos reales y deciden los casos dudosos.
  - S2.6 unifica las validaciones de entrada de Domain.
  - S3 integra los valores en la entidad `Link`.

## Dependencias y prerrequisitos

- Existen `Linkubator.Domain` y el proyecto de tests con las pruebas de [UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs), [ImageUrlPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/ImageUrlPolicyTests.cs), [UrlValidationPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlValidationPolicyTests.cs), [UrlHostPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlHostPolicyTests.cs) y [UrlNormalizationPolicyTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationPolicyTests.cs).
- S2.1, S2.2, S2.3 y S2.4 están aceptadas por DLG según [roadmap.md → «Hecho»](../../../context/roadmap.md#hecho).
- La existencia de pruebas no demuestra que cada ejemplo esté cubierto: la cobertura se determina en la auditoría de este trabajo.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url) y [«Detección de esquema URI»](../../../context/specifications.md#detección-de-esquema-uri) | Cada entrada de la tabla de ejemplos de la fuente, incluidas las rechazadas, y cada esquema que la fuente lista como no permitido. | Se ejecuta el ajuste de `UrlOriginal` con el texto de entrada. | El resultado, o el rechazo con su motivo, coincide con el de la fuente, y cada ejemplo tiene al menos una prueba que lo ejecuta con ese texto. |
| CA02 | [«Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image) | Cada entrada de la tabla de ejemplos de la fuente, con la URL base que indica cada una, incluidas las rechazadas, y cada esquema que la fuente lista como no permitido. | Se ejecuta el ajuste de `Image` con el texto de entrada y su base. | El resultado, o el rechazo con su motivo, coincide con el de la fuente, y cada ejemplo tiene al menos una prueba que lo ejecuta con ese texto y esa base. |
| CA03 | [«Validación»](../../../context/specifications.md#validación) | Cada entrada de la tabla de ejemplos de la fuente, tratada como `UrlOriginal`. | Se ajusta con el ajuste de `UrlOriginal` y se valida el resultado cuando el ajuste no la rechaza. | El resultado final coincide con el de la tabla, y los rechazos lo son con el error de dominio que corresponde, sin incluir el texto validado ni las credenciales. |
| CA04 | [«Validación»](../../../context/specifications.md#validación) y [«Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image) | Las entradas de la tabla de «Validación» que son una URL absoluta, tratadas como `Image` ya ajustada. | Se valida el texto como `Image`. | La validación da el mismo resultado que para `UrlOriginal`; el rechazo de `localhost` y de IP en `Image` se comprueba en el ajuste de CA02, no en la validación. |
| CA05 | [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image) y [«Validación»](../../../context/specifications.md#validación) | Un mismo texto de las fuentes que ambos ajustes tratan de forma distinta, como una dirección IP, `localhost` o un texto sin esquema. | Se ajusta como `UrlOriginal` y como `Image`. | Cada campo produce su resultado propio, y las pruebas de `UrlOriginal` y de `Image` están diferenciadas, de modo que ningún ejemplo de un campo se comprueba solo con la política del otro. |
| CA06 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | Cada ejemplo de la fuente (los bloques de ejemplos, los textos de `c++` y de dominio internacionalizado, y las URL de entrada de cada grupo), con su clave resultante. | Se calcula `UrlNormalized` con cada entrada. | El resultado coincide con el de la fuente; las entradas que la fuente agrupa producen la misma clave, y cada ejemplo tiene al menos una prueba que lo ejecuta con ese texto. |
| CA07 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados), [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url) y [«Validación»](../../../context/specifications.md#validación) | Las dos entradas de la colisión aceptada, escritas tal como las da la fuente, sin esquema. | Se ajustan, se validan y se calcula su `UrlNormalized`. | Las dos producen la clave que indica la fuente, aunque su ruta y su consulta difieran. |
| CA08 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | Las derivaciones que la fuente cita en texto (`?q=c++ 2` y `/c++2`). | Se calcula `UrlNormalized` con cada una. | Producen las claves que la fuente indica, de modo que la diferencia entre `+` en la ruta y `+` en la consulta queda comprobada. |
| CA09 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | La lista de ejemplos de las fuentes y las pruebas resultantes. | Se revisa la trazabilidad. | Cada ejemplo identifica su sección fuente, el campo (`UrlOriginal`, `Image` o clave), su prueba y el resultado de su ejecución, sin copiar la regla, y no queda ninguno sin prueba ni con un bloqueo sin registrar. |
| CA10 | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) y [mvp0-plan.md → «Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución con las pruebas nuevas. | Se compila y se ejecutan todas las pruebas. | Compila sin advertencias, todas las pruebas pasan y no se modifican las dependencias de Domain ni su código de producción. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. Una divergencia entre un ejemplo y la implementación se registrará aquí como bloqueo al auditar. | [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image), [«Validación»](../../../context/specifications.md#validación) y [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | — | — |

Nota de trazabilidad: el ejemplo `example.com:8080/x` figura en las tablas de ajuste de `UrlOriginal` y de validación, y `https://192.168.1.1/...` se acepta como `UrlOriginal` y se rechaza como `Image`; CA05 cubre esa diferencia entre campos.

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 11-X-2026.
- [Tareas](tasks.md), aprobadas por DLG el 11-X-2026 («Aprobado»); implementación aceptada el 11-X-2026 («Implementado»).
