# Especificación: S1.5 — Tests de los ejemplos de alias y slugs

## Estado y aprobación

- Estado: aprobada por DLG el 09-X-2026.
- Referencia de aprobación: «Ok». Las decisiones sobre el alcance y sobre el ejemplo `Anna_López!` se tomaron con DLG el 09-X-2026.

## Objetivo

Que todos los ejemplos y casos concretos de las fuentes de generación de alias y de slugs estén cubiertos por pruebas automatizadas de Domain que los ejecuten contra la implementación de S1.3 y S1.4, con una trazabilidad comprobable entre cada ejemplo de la fuente y su prueba. El resultado es la cobertura completa y verificable, no un cambio de comportamiento.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.5, Definition of Done del sprint y reparto con los trabajos vecinos.
- [specifications.md → «Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii): pasos, rechazos y ejemplos de la transformación compartida.
- [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias): ejemplo, rango y palabras reservadas del alias.
- [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs): ejemplos, rango y colisión por nombres equivalentes.
- [specifications.md → «Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos): sustituciones y ejemplos de la tabla.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): rangos y unidad de conteo.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): aislamiento de Domain.
- [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación): estado de la etapa.

## Alcance

- Incluye:
  - Auditar los ejemplos de [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii), [«Generación del alias»](../../context/specifications.md#generación-del-alias), [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) y [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) contra las pruebas existentes de S1.3 y S1.4, e identificar los que no tienen una prueba que los ejecute.
  - Añadir las pruebas que falten y, cuando un ejemplo esté cubierto de forma indirecta, una comprobación que lo ejecute tal como lo escribe la fuente.
  - Comprobar que los ejemplos que valen para varios usos de la transformación producen el mismo resultado con el generador común, con la política de alias y con la política de slugs, cuando el resultado entra en el rango de cada una.
  - Comprobar los rechazos de la fuente (caracteres no admitidos, vacío, fuera de rango y palabras reservadas) con el tipo de error de dominio que corresponde.
  - Dejar la trazabilidad ejemplo de la fuente → prueba en `tasks.md`, sin copiar la regla.

- Excluye:
  - Cambiar el comportamiento, las fuentes, la tabla de símbolos o los mensajes. Si una prueba revela una divergencia con la fuente, se registra como bloqueo y se propone el cambio en su documento propietario (ver [«Bloqueos y preguntas pendientes»](#bloqueos-y-preguntas-pendientes)).
  - Reescribir las pruebas aceptadas de S1.1, S1.2, S1.3 y S1.4 salvo lo necesario para completar la cobertura de un ejemplo.
  - Recorte y longitudes en puntos de código fuera del BMP y marcas combinantes: pertenecen a S1.1.
  - Consultar o resolver colisiones de alias o de slugs, que requieren entidades, casos de uso y persistencia (S3–S5).
  - Revisar la tabla contra nombres reales de colecciones (S1.6) y resolver ambigüedades o aprobar el comportamiento (S1.7).

- Trabajo vecino y responsable de sus entregables:
  - S1.1, S1.2, S1.3 y S1.4 (aceptadas) entregaron la implementación y las pruebas sobre las que se audita.
  - S1.6 y S1.7 (humano) revisan la tabla con datos reales y aprueban el comportamiento.
  - S3–S5 integran los valores en las entidades y detectan las colisiones.

## Dependencias y prerrequisitos

- Existen `Linkubator.Domain` y el proyecto de tests con las pruebas de [AsciiTransformationPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AsciiTransformationPolicyTests.cs), [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs) y [SlugPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/SlugPolicyTests.cs).
- S1.1, S1.2, S1.3 y S1.4 están aceptadas según [roadmap.md → «Dominio y aplicación»](../../context/roadmap.md#dominio-y-aplicación).
- La existencia de pruebas no demuestra que cada ejemplo esté cubierto: la cobertura se determina en la auditoría de este trabajo.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | Cada ejemplo de la lista de la fuente, incluidos los dos rechazados. | Se ejecuta el generador común con el texto de entrada del ejemplo. | El resultado, o el rechazo con su motivo, coincide con el de la fuente y cada ejemplo tiene al menos una prueba que lo ejecuta con ese texto. |
| CA02 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) | El ejemplo del alias y cada palabra reservada de la fuente, en el estado exacto que indica la fuente y con variantes de mayúsculas, acentos y espacios en los extremos. | Se ejecuta la política de alias. | El ejemplo produce el alias de la fuente; cada palabra reservada se rechaza como reservada; un texto que solo contiene una palabra reservada no se rechaza por ello. |
| CA03 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Resultados vacíos, justo en el mínimo y en el máximo del alias, y uno por debajo y por encima de cada límite. | Se ejecuta la política de alias. | Los valores dentro del rango se aceptan sin cambios y los de fuera se rechazan con el error de longitud que corresponde, sin truncarlos. |
| CA04 | [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Cada ejemplo de la fuente y el slug fijado para la colección por defecto. | Se ejecuta la política de slugs. | El resultado coincide con el de la fuente, igual para el mismo nombre usado como colección o como etiqueta. |
| CA05 | [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Resultados vacíos, justo en el mínimo y en el máximo del slug, uno por encima del máximo, y nombres cuyo resultado crece por la tabla. | Se ejecuta la política de slugs. | Los valores dentro del rango se aceptan sin cambios y los de fuera se rechazan con el error de longitud que corresponde, sin truncarlos. |
| CA06 | [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Los pares de nombres que la fuente considera colisión, aunque se escriban distinto. | Se ejecuta la política de slugs con cada nombre. | Los dos nombres del par producen el mismo slug, sin sufijos ni prefijos. La consulta de la colisión no se comprueba aquí. |
| CA07 | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | Cada entrada de la tabla, cada par texto/resultado de la fuente y los dos caracteres que la fuente dice que no están en la tabla. | Se ejecuta el generador común. | Cada entrada produce su resultado, cada par coincide con la fuente y los caracteres ausentes se rechazan indicando el motivo. |
| CA08 | [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii), [«Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | Un mismo texto de las fuentes cuyo resultado entra en el rango de ambas políticas. | Se obtiene su valor con el generador común, la política de alias y la política de slugs. | Los tres devuelven el mismo valor; los textos rechazados por el generador se rechazan igual con las dos políticas, sin conservar el texto de entrada. |
| CA09 | [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) | La lista de ejemplos de las fuentes y las pruebas resultantes. | Se revisa la trazabilidad. | Cada ejemplo identifica su sección fuente, su prueba y el resultado de su ejecución, sin copiar la regla, y no queda ninguno sin prueba ni con un bloqueo sin registrar. |
| CA10 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) y [mvp0-plan.md → «Definition of Done común (aplica a todos los sprints)»](../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución con las pruebas nuevas. | Se compila y se ejecutan todas las pruebas. | Compila sin advertencias, todas las pruebas pasan y no se modifican las dependencias de Domain ni su código de producción. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno pendiente. DLG confirmó el 09-X-2026 que S1.5 cubre también los ejemplos de la transformación común y de la tabla de símbolos, además de los de las dos secciones que nombra el plan. | — | — | — |

Nota de trazabilidad: los ejemplos de la transformación de [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) se tratan como ejemplos del generador. Los que dan un resultado fuera del rango del alias, como `Ana_López!` (`ana-lopez`), se comprueban con el generador y con el rechazo por longitud en la política de alias, no como alias válidos. El ejemplo de alias de S1.4 pasó a `Anna_López!` (`anna-lopez`, 10 caracteres) por indicación de DLG el 09-X-2026.

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas](tasks.md).
