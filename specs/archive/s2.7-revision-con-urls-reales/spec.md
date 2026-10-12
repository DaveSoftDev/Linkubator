# Especificación: S2.7 — Revisión de la normalización con 31 URLs revisadas

## Estado y aprobación
- Estado: aprobada por DLG el 11-X-2026.
- Referencia de aprobación: «Aprobada».
- Enmienda CA01 aprobada por DLG el 12-X-2026 («Sí, apruebo la modificación»); conjunto aceptado de 31 URLs según la tabla de revisión.
- Ajuste de CA01: DLG acepta el conjunto de 31 URLs registrado y su diferencia respecto a las 30 inicialmente previstas al ordenar «Archiva también la S2.7», 12-X-2026.

## Objetivo

Que DLG compruebe, con evidencia registrada, que el ajuste, la validación y la normalización de URL producen resultados aceptables con las 31 URLs registradas en la tabla de revisión. DLG acepta ese conjunto y la diferencia respecto a las 30 URLs originalmente previstas al ordenar «Archiva también la S2.7» el 12-X-2026. El resultado es una revisión trazable con el resultado de cada URL, el juicio de DLG y los hallazgos como entrada de S2.8, no un cambio de comportamiento.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.7, Definition of Done del sprint y reparto con los trabajos vecinos.
- [specifications.md → «Ajuste de URL»](../../../context/specifications.md#ajuste-de-url): ajuste de `UrlOriginal` aplicable a cada URL.
- [specifications.md → «Validación»](../../../context/specifications.md#validación): validación aplicable a la URL ajustada.
- [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados): generación de `UrlNormalized`, lista de parámetros de marketing y colisiones aceptadas.
- [specifications.md → «Duplicados»](../../../context/specifications.md#duplicados): uso de la clave y disponibilidad de la URL original para ver las diferencias.
- [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados): colisiones y límites aceptados de la clave plana.
- [domain-model.md → «Link»](../../../context/domain-model.md#link): naturaleza de `UrlOriginal` y `UrlNormalized`.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): aislamiento de Domain.
- [roadmap.md → «Dominio y aplicación»](../../../context/roadmap.md#dominio-y-aplicación): estado de la etapa.

## Alcance

- Incluye:
  - Registrar las 31 URLs de origen «script S2.7» ya incluidas en la tabla, sin modificar sus textos; DLG acepta ese conjunto para esta revisión mediante su instrucción de archivo del 12-X-2026.
  - Ejecutar con cada URL el ajuste y la validación de `UrlOriginal` y, si las supera, el cálculo de `UrlNormalized`, y registrar para cada una el resultado o el rechazo con su error de dominio.
  - Derivar a mano el resultado esperado según la fuente y contrastarlo con el obtenido, sin reutilizar el código revisado.
  - Contrastar las claves entre sí para detectar colisiones entre las 31 URLs revisadas y registrar la URL original de las que coincidan.
  - Registrar el juicio de DLG sobre cada resultado y los hallazgos como entrada de S2.8.
  - Dejar la evidencia de la revisión en `tasks.md`, sin copiar la regla.

- Excluye:
  - Cambiar el código, las fuentes, las listas o los mensajes. Una divergencia o un resultado no aceptado se registra como hallazgo y se propone el cambio en su documento propietario (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)).
  - Decidir los casos dudosos y registrarlos como decisiones: pertenece a S2.8.
  - Ajustar o validar `Image`: no se aportan imágenes reales y sus reglas están cubiertas por S2.2, S2.3 y S2.5.
  - Descargar, abrir o comprobar las URLs en la red, seguir redirecciones o hacer scraping.
  - Consultar o resolver duplicados contra enlaces guardados, que requieren entidades, casos de uso y persistencia.
  - Los ejemplos de las fuentes y su cobertura por pruebas, entregados por S2.5.

- Trabajo vecino y responsable de sus entregables:
  - S2.1 a S2.4 entregaron la implementación; S2.5 entregó las pruebas de los ejemplos de las fuentes; S2.6 unificó las validaciones de entrada de Domain.
  - S2.7 es una tarea de DLG: acepta el conjunto de 31 URLs registrado y emite el juicio sobre los resultados. La preparación de los materiales de revisión y el registro de la evidencia los realiza quien se indique en el plan técnico, sin sustituir ese juicio.
  - S2.8 (humano) decide los casos dudosos y los registra.
  - S3 integra los valores en la entidad `Link`; S4 y S5 aplican la detección de duplicados.

## Dependencias y prerrequisitos

- Existen `Linkubator.Domain` y el proyecto de tests con [LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs), [UrlValidationPolicy.cs](../../../src/Linkubator.Domain/Policies/UrlValidationPolicy.cs) y [UrlNormalizationPolicy.cs](../../../src/Linkubator.Domain/Policies/UrlNormalizationPolicy.cs).
- S2.3, S2.4, S2.5 y S2.6 están aceptadas por DLG según [roadmap.md → «Dominio y aplicación»](../../../context/roadmap.md#dominio-y-aplicación).
- Las 31 URLs evaluadas constan en la tabla de `tasks.md`; DLG acepta el conjunto registrado mediante la instrucción de archivo del 12-X-2026.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | Las 31 URLs registradas con origen «script S2.7» y aceptadas por DLG para esta revisión. | Se registran y contrastan para la revisión. | Constan las 31 con su texto y origen, sin modificar, omitir ni sustituir ninguna; DLG acepta la muestra aportada aunque difiera de las 30 previstas originalmente. |
| CA02 | [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url) y [«Validación»](../../../context/specifications.md#validación) | Cada una de las 31 URLs. | Se ajusta como `UrlOriginal` y se valida el resultado cuando el ajuste no la rechaza. | Cada URL tiene registrada su URL ajustada, o el rechazo con el error de dominio, tal como lo devuelve el código. |
| CA03 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | Cada URL que supera el ajuste y la validación. | Se calcula `UrlNormalized`. | Cada una tiene registrada su clave, tal como la devuelve el código, y ninguna se omite. |
| CA04 | [«Ajuste de URL»](../../../context/specifications.md#ajuste-de-url), [«Validación»](../../../context/specifications.md#validación) y [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | El resultado de cada URL en CA02 y CA03. | Se contrasta con lo que la fuente determina para esa URL, derivado a mano. | Cada resultado coincide con la fuente, o la diferencia queda registrada como hallazgo con la sección afectada, sin cambiar el código ni la fuente. |
| CA05 | [«Duplicados»](../../../context/specifications.md#duplicados) y [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) | Las claves obtenidas para las URLs aceptadas. | Se comparan entre sí. | Constan los grupos de URLs con la misma clave, con su URL original, o la comprobación de que no hay colisiones, y cada colisión queda registrada como hallazgo, sin resolverla aquí. |
| CA06 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | Las 31 URLs con su resultado, los hallazgos de CA04 y las colisiones de CA05. | DLG los revisa. | Cada URL tiene el juicio de DLG (aceptable o no aceptable) con su referencia, y cada resultado no aceptable o dudoso queda registrado como entrada de S2.8 sin resolverlo aquí. |
| CA07 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | La revisión terminada. | Se revisa la evidencia registrada. | Constan las 31 URLs con su origen, resultado, juicio, fecha y estado del código revisado, y los hallazgos enlazan su sección fuente sin copiar la regla. |
| CA08 | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) y [mvp0-plan.md → «Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución tras la revisión. | Se compila y se ejecutan todas las pruebas. | Compila sin advertencias, todas las pruebas pasan y no hay cambios en el código de producción de Domain ni en sus dependencias. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno pendiente tras la aceptación de DLG del conjunto revisado y sus resultados el 12-X-2026 («Archiva también la S2.7»). | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 11-X-2026.
- [Tareas](tasks.md), aprobadas por DLG el 11-X-2026.
