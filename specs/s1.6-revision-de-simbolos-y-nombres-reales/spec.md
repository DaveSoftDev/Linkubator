# Especificación: S1.6 — Revisión de la tabla de símbolos y prueba con nombres reales

## Estado y aprobación

- Estado: aprobada por DLG el 10-X-2026.
- Referencia de aprobación: «Listo».

## Objetivo

Que DLG compruebe, con evidencia registrada, que la [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) y el código de Domain que la aplica se corresponden entrada por entrada, y que la generación de slugs produce resultados aceptables con 20 nombres reales de colecciones. El resultado es una revisión trazable con sus hallazgos, no un cambio de comportamiento.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.6, Definition of Done del sprint y reparto con los trabajos vecinos.
- [specifications.md → «Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos): entradas, bloques Unicode cubiertos y caracteres ausentes.
- [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii): pasos que preceden y siguen a la aplicación de la tabla.
- [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs): resultado, rango y colisión de los nombres de colección.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): rango del slug.
- [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados): riesgo aceptado de la conversión a ASCII del alias y de los slugs.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): aislamiento de Domain.
- [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación): estado de la etapa.

## Alcance

- Incluye:
  - Contrastar cada entrada de la tabla de símbolos con el código que la aplica, en ambos sentidos: toda entrada de la fuente existe en el código con su resultado, y todo símbolo, separador o letra que el código sustituye figura en la fuente.
  - Comprobar la cobertura declarada por la fuente para las letras latinas de los bloques Latin-1 y Latin Extended-A: cada una se reduce a ASCII, está en la tabla o se rechaza.
  - Probar la generación del slug con 20 nombres reales de colecciones aportados por DLG, y registrar para cada uno el resultado o el rechazo con su motivo.
  - Registrar el juicio de DLG sobre cada resultado y los hallazgos (divergencias con la fuente, resultados sorprendentes o ambigüedades) como entrada de S1.7.
  - Dejar la evidencia de la revisión en `tasks.md`, sin copiar la regla.

- Excluye:
  - Cambiar el código, la tabla, los mensajes o cualquier otra fuente. Una divergencia o un resultado no aceptado se registra como hallazgo y se propone el cambio en su documento propietario (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)).
  - Resolver ambigüedades y aprobar el comportamiento: pertenece a S1.7.
  - Probar nombres de alias, que no son nombres de colección.
  - Consultar o resolver colisiones, que requieren entidades, casos de uso y persistencia (S3–S5).
  - Los ejemplos de las fuentes y su cobertura por pruebas, entregados por S1.5.

- Trabajo vecino y responsable de sus entregables:
  - S1.4 entregó la implementación de la transformación y de los slugs; S1.5 (aceptada) entregó las pruebas de los ejemplos de las fuentes.
  - S1.6 es una tarea de DLG: aporta los 20 nombres y emite el juicio sobre los resultados. La preparación de los materiales de revisión y el registro de la evidencia los realiza quien se indique en el plan técnico, sin sustituir ese juicio.
  - S1.7 (humano) resuelve las ambigüedades que surjan y aprueba el comportamiento.
  - S3–S5 integran los valores en las entidades y detectan las colisiones.

## Dependencias y prerrequisitos

- Existen `Linkubator.Domain` y el proyecto de tests con [AsciiTransformationPolicy.cs](../../src/Linkubator.Domain/Policies/AsciiTransformationPolicy.cs) y [SlugPolicy.cs](../../src/Linkubator.Domain/Policies/SlugPolicy.cs).
- S1.4 y S1.5 están aceptadas según [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación).
- Los 20 nombres reales de colecciones los aporta DLG; no se inventan ni se completan con ejemplos de las fuentes. Hasta que existan, CA03–CA06 no pueden ejecutarse.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | Cada entrada de la tabla de símbolos y de letras latinas, y cada símbolo, separador o letra que el código sustituye. | Se contrastan la fuente y el código. | Cada entrada de la fuente tiene su sustitución en el código con el mismo resultado y no hay sustituciones en el código que no figuren en la fuente. |
| CA02 | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | Todas las letras latinas de los bloques Latin-1 y Latin Extended-A. | Se transforma cada una con el generador común. | Cada una se reduce a ASCII, está en la tabla o se rechaza como no ASCII, y las que la fuente declara ausentes se rechazan. Una letra que no cumpla ninguna de las tres se registra como hallazgo. |
| CA03 | [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | 20 nombres reales de colecciones distintos, aportados por DLG. | Se genera el slug de cada uno con la política de slugs. | Cada nombre tiene registrado su slug o el motivo de rechazo, tal como lo devuelve el código, y ninguno se omite ni se sustituye por otro. |
| CA04 | [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | El resultado de cada uno de los 20 nombres. | Se contrasta con lo que la fuente determina para ese nombre. | Cada resultado coincide con la fuente, o la diferencia queda registrada como hallazgo con la sección afectada, sin cambiar el código ni la fuente. |
| CA05 | [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) | Los 20 resultados y los hallazgos de CA01 y CA02. | DLG los revisa. | Cada resultado tiene el juicio de DLG (aceptable o no aceptable) con su referencia, y cada resultado no aceptable o ambiguo queda registrado como entrada de S1.7 sin resolverlo aquí. |
| CA06 | [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) | La revisión terminada. | Se revisa la evidencia registrada. | Constan los 20 nombres con su origen, el resultado, el juicio, la fecha y el estado del código revisado, y los hallazgos enlazan su sección fuente sin copiar la regla. |
| CA07 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) y [mvp0-plan.md → «Definition of Done común (aplica a todos los sprints)»](../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución tras la revisión. | Se compila y se ejecutan todas las pruebas. | Compila sin advertencias, todas las pruebas pasan y no hay cambios en el código de producción de Domain ni en sus dependencias. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| DLG debe aportar los 20 nombres reales de colecciones. No se conoce su procedencia ni si deben incluir casos que ejerciten acentos, símbolos o alfabetos no ASCII. | [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) | CA03–CA06 | DLG facilita los nombres y, si lo desea, indica de dónde proceden. No bloquea la aprobación de esta especificación. |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas](tasks.md).
