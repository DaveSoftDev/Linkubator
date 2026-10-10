# Especificación: S1.7 — Resolución de ambigüedades y aprobación del comportamiento

## Estado y aprobación

- Estado: aprobada por DLG el 10-X-2026.
- Referencia de aprobación: «Nada que modificar».

## Objetivo

Cerrar las decisiones pendientes de la revisión de la [tabla de conversión de símbolos](../../context/specifications.md#tabla-de-conversión-de-símbolos) y de la generación de slugs, dejando registrada la decisión final sobre cada caso ambiguo, cada desviación real y cada resultado no aceptado. La finalidad es aprobar el comportamiento que quedará como base de la etapa S1 y de la integridad de la definición del MVP0, sin cambiar el código de producción sin una decisión explícita.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.7 y criterio de cierre del sprint.
- [specifications.md → «Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos): entradas, bloques Unicode cubiertos y casos ausentes.
- [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii): pasos previos y posteriores a la tabla.
- [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs): criterios de resultado, rango y colisión.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): límites de los slugs y del texto introducido por el usuario.
- [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados): riesgo aceptado para las transformaciones de alias y slug.
- [specs/s1.6-revision-de-simbolos-y-nombres-reales/tasks.md → «Hallazgos para S1.7»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17): lista de resultados no aceptados o ambiguos que requiere decisión humana.
- [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación): estado del trabajo activo y la revisión de la etapa.

## Alcance

- Incluye:
  - Revisar cada hallazgo notificado en [S1.6](../s1.6-revision-de-simbolos-y-nombres-reales/spec.md), distinguiendo casos aceptables, no aceptables y ambigüedades.
  - Decidir si un caso debe mantenerse como comportamiento válido, reformularse en una regla de la fuente o dejarse como propuesta fuera del alcance del MVP0.
  - Registrar la decisión por cada nombre real, por cada entrada problemática de la tabla y por cada caso de revisión con su fecha, origen y responsable.
  - Concretar si un resultado que no coincide con la fuente debe cambiar la fuente, la implementación o ambas, y dejarlo trazado antes de crear o modificar el código.
  - Dejar la evidencia documental de la decisión en la carpeta de la spec y, si hace falta, documentar la fuente revisada para la regla que se confirma o se corrige.

- Excluye:
  - Implementar cambios de comportamiento en Domain ni modificar la tabla o los tests sin aprobación explícita de DLG.
  - Resolver casos que no estén registrados en la evidencia de S1.6 o que dependan de persistencia, entidades o colisiones de S3–S5.
  - Repetir la revisión de nombres reales que ya estén documentados con juicio y fecha en la evidencia de S1.6.
  - Encubrir decisiones con un consenso implícito; la aprobación debe quedar explícita y enlazada a la evidencia.

- Trabajo vecino y responsable de sus entregables:
  - S1.6 aporta la revisión y los hallazgos registrados con nombres reales, la comparación contra la fuente y el estado del código revisado.
  - S1.7 decide el comportamiento correcto, aprueba la regla y dispara el cambio documental o de implementación si procede.
  - S3–S5 resuelven colisiones, persistencia y la integración en entidades, pero no reemplazan la decisión de la regla de dominio.

## Dependencias y prerrequisitos

- S1.6 está cerrada en su parte técnica y la evidencia de revisión existe en [specs/s1.6-revision-de-simbolos-y-nombres-reales/tasks.md](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md).
- DLG ha revisado los nombres reales y ha dejado los resultados y hallazgos como entrada de esta etapa.
- La fuente propietaria de la regla sigue siendo [context/specifications.md](../../context/specifications.md), y cualquier cambio de regla debe realizarse en ese documento antes de adaptar código o pruebas.
- Si una discrepancia requiere ampliar la especificación, debe procederse con la regla del documento propietario y con la trazabilidad documental; no se actúa sobre el comportamiento sin decisión habilitada.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) | Cada hallazgo registrado en S1.6. | Se revisa cada caso. | Cada hallazgo queda clasificado como aceptado, rechazado o con decisión pendiente, con su motivo evidente y su criterio de origen. |
| CA02 | [specifications.md → «Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) y [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | La tabla y las reglas de pre/postproceso. | Se compara la decisión humana con la fuente. | Cada decisión de comportamiento queda alineada con la fuente o, si no lo está, se registra como cambio de regla y no se interpreta como comportamiento aceptado por defecto. |
| CA03 | [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs) y [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Los nombres reales y los resultados no aceptados de la revisión. | DLG aprueba o rechaza un resultado. | Cada caso resuelto tiene una decisión final con su justificación y la referencia del responsable. |
| CA04 | [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Los casos de nombres reales con resultados sorprendentes o ambiguos. | Se refuerza la decisión del comportamiento con prueba de ejemplo. | Queda registrado si el caso se acepta como comportamiento válido, exige un ajuste de la regla o se deja como caso fuera del alcance del MVP0. |
| CA05 | [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados) | Los riesgos aceptados y los casos de ambigüedad. | Se evalúan los resultados no previstos con la fuente. | La decisión de DLG confirma si el riesgo se acepta o si requiere cambiar la regla antes de continuar. |
| CA06 | [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación) | La etapa S1 y el cierre de su definición. | Se revisa la evidencia de la decisión. | Quedan resueltas todas las ambigüedades de S1.6, sin casos abiertos pendientes de trámite, y la etapa puede dejarse cerrada en la documentación de recorrido del sprint. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Los hallazgos de S1.6 pueden mostrar casos que no encajan exactamente con la fuente o con un comportamiento previo. | [specs/s1.6-revision-de-simbolos-y-nombres-reales/tasks.md → «Hallazgos para S1.7»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17) | CA01–CA06 | DLG decide si el caso se acepta como riesgo, requiere un ajuste de la regla o queda fuera del alcance del MVP0 y se registra como decisión. |
| El comportamiento puede ser válido para un nombre real y no para otro, sin que la regla fuente lo indique con suficiente precisión. | [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) y [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs) | CA02–CA05 | DLG aprueba la excepción o requiere cambiar la regla en su documento propietario antes de implementar cualquier cambio. |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Especificación de S1.6](../s1.6-revision-de-simbolos-y-nombres-reales/spec.md).
- [Hallazgos para S1.7](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17).
- [Roadmap del sprint S1](../../context/roadmap.md#dominio-y-aplicación).
