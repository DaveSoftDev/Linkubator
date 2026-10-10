# Especificación: S1.4 — Transformación común a ASCII y generación de slugs

## Estado y aprobación

- Estado: aprobada por DLG el 09-X-2026.
- Referencia de aprobación: «Se han estudidado todos los casos».
- Historial: reabierta el 09-X-2026 para ampliar el alcance. La aprobación anterior de DLG («Especificación confirmada») cubría solo la generación de slugs y quedó sustituida por esta.

## Objetivo

Proporcionar en Domain una única transformación a ASCII, compartida por `User.Alias`, `Collection.Slug` y `Tag.Slug`, junto con la política de slugs que la usa y la adaptación de la política de alias de S1.3 a ella, para que las entidades y los casos de uso puedan rechazar los textos no admisibles sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.4 y reparto con los trabajos vecinos.
- [specifications.md → «Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii): pasos, rechazos y ejemplos de la transformación.
- [specifications.md → «Generación del alias»](../../../context/specifications.md#generación-del-alias): validaciones propias del alias.
- [specifications.md → «Generación de slugs»](../../../context/specifications.md#generación-de-slugs): validaciones propias de los slugs y tratamiento de la colisión.
- [specifications.md → «Tabla de conversión de símbolos»](../../../context/specifications.md#tabla-de-conversión-de-símbolos): sustituciones de símbolos y de letras latinas, y ejemplos.
- [specifications.md → «Longitudes máximas»](../../../context/specifications.md#longitudes-máximas): rangos aplicables y unidad de conteo.
- [specifications.md → «Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario): recorte previo a la transformación.
- [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection) y [«Tag»](../../../context/domain-model.md#tag): origen del valor, regeneración y unicidad.
- [decisions.md → «Datos, URLs y búsqueda»](../../../context/decisions.md#datos-urls-y-búsqueda): transformación única y criterio de rechazo y eliminación.
- [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados): conversión a ASCII del alias y de los slugs.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.

## Alcance

- Incluye:
  - Un único generador en Domain que aplica la [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) y sirve a `User.Alias`, `Collection.Slug` y `Tag.Slug`. Se invoca igual en los tres casos y no recibe ningún parámetro que indique el origen de la llamada.
  - Identificar los textos con controles o separadores no admitidos y con letras o números no ASCII, para que se rechacen indicando el motivo mediante un error de dominio específico, sin incluir contenido introducido por el usuario.
  - Una política de slugs que usa el generador y valida el resultado vacío o fuera de rango con los errores de longitud de S1.2.
  - Adaptar la política de alias de S1.3 al generador común: deja de recortar el texto, conserva su validación propia (rango y palabras reservadas) y pasa a rechazar los caracteres que el generador rechaza.
  - Pruebas automatizadas del generador, de la política de slugs y de la política de alias adaptada; S1.5 completa la cobertura exhaustiva de los ejemplos de alias y slugs.
  - Comprobación de que las pruebas de alias de S1.3, actualizadas a la fuente vigente, siguen pasando.

- Excluye:
  - Recortar el texto: lo hace quien recibe lo introducido por el usuario, según [«Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y S1.1.
  - Consultar, persistir o resolver la colisión de alias o de slugs, incluida la excepción de asociar la etiqueta existente al crearla desde el formulario de un enlace; requieren `User`, `Collection`, `Tag` y los casos de uso, y su garantía persistente corresponde a S5.
  - Crear las entidades, regenerar el slug al renombrarlas, asignar el slug de la colección «Bandeja de entrada» al provisionarla o decidir si las palabras reservadas del alias se comprueban en la entidad.
  - Mostrar la vista previa o los mensajes de validación en la interfaz, o decidir la traducción de errores de dominio a resultados de aplicación.
  - Definir o cambiar reglas, límites, tabla de símbolos o mensajes de producto fuera de las fuentes enlazadas.

- Trabajo vecino y responsable de sus entregables:
  - S1.1 (aceptada) proporciona el recorte y la comprobación por puntos de código que este trabajo reutiliza cuando proceda.
  - S1.2 (aceptada) proporciona la excepción base de dominio y las excepciones de longitud que reutiliza este trabajo.
  - S1.3 (aceptada) entregó la política de alias que este trabajo adapta.
  - S1.5 completa los tests de todos los ejemplos de alias y slug de la fuente.
  - S1.6 revisa la tabla de símbolos contra el código y prueba nombres reales de colecciones.
  - S3 integra los valores en `User`, `Collection` y `Tag` y sus invariantes, y gestiona los errores de dominio; S4 y S5 realizan, respectivamente, el rechazo de colisiones en los casos de uso y la garantía de unicidad persistente.

## Dependencias y prerrequisitos

- Existen el proyecto `Linkubator.Domain` y el proyecto de tests con referencia a Domain.
- S1.1, S1.2 y S1.3 están aceptadas e implementadas; sus componentes se pueden consumir sin introducir dependencias de otras capas.
- La evidencia de S1.3 corresponde al comportamiento anterior del alias. Los cambios de este trabajo sobre el alias provienen de las fuentes vigentes y no reescriben su aceptación.
- La especificación no presupone que existan las entidades ni almacenamiento para consultar colisiones.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) | Un texto admisible, ya recortado. | Domain aplica el generador. | Produce el resultado ASCII de la fuente, igual para el mismo texto en cada ejecución. Es el único generador de alias, slug de colección y slug de etiqueta, sin parámetro que distinga su origen. |
| CA02 | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) | Textos con mayúsculas, diacríticos, espacios normales, puntuación, emojis, guiones duplicados o guiones en los extremos. | Domain aplica el generador. | El resultado coincide con los ejemplos de la fuente; la puntuación, los emojis y los símbolos ausentes de la tabla se eliminan sin conversión. |
| CA03 | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) | Textos con letras y números de ancho completo, superíndices, `℃`, `™`, fracciones o espacios de compatibilidad. | Domain aplica el generador. | La normalización los reduce a ASCII como indica la fuente y el resultado coincide con sus ejemplos. |
| CA04 | [«Tabla de conversión de símbolos»](../../../context/specifications.md#tabla-de-conversión-de-símbolos) | Textos con símbolos, separadores y letras latinas de las filas de la tabla. | Domain aplica el generador. | Cada sustitución de la tabla se aplica y el resultado coincide con los pares texto/resultado de la fuente. |
| CA05 | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) y [decisions.md → «Datos, URLs y búsqueda»](../../../context/decisions.md#datos-urls-y-búsqueda) | Un texto con un carácter de control o un separador distinto del espacio normal, o que tras las conversiones aún contiene alguna letra o número no ASCII, solo o junto a texto ASCII. | Domain aplica el generador. | Se rechaza identificando el motivo mediante un error de dominio específico, sin eliminar esos caracteres ni producir un resultado parcial ni conservar el texto de entrada en el error. |
| CA06 | [«Generación de slugs»](../../../context/specifications.md#generación-de-slugs) y [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Un nombre cuyo resultado queda vacío o fuera del rango del slug. | La política de slugs lo valida. | Se rechaza con el error de longitud correspondiente, sin truncarlo ni conservar el texto de entrada. |
| CA07 | [«Generación de slugs»](../../../context/specifications.md#generación-de-slugs) y [domain-model.md → «Collection»](../../../context/domain-model.md#collection) | El nombre de la colección por defecto, el mismo nombre usado como colección y como etiqueta, y dos nombres que solo se distinguen por mayúsculas, acentos o símbolos de la tabla. | La política de slugs genera sus slugs. | El primero produce el slug fijado por la fuente; el mismo nombre produce el mismo slug en ambos usos; los otros dos producen el mismo slug, sin sufijos ni prefijos, de modo que las capas posteriores puedan detectar la colisión. La consulta de la colisión no se realiza en este trabajo. |
| CA08 | [«Generación del alias»](../../../context/specifications.md#generación-del-alias) y [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) | Textos de alias válidos, vacíos, fuera de rango, reservados y con caracteres no admitidos, incluidos los que tienen espacios en los extremos. | La política de alias los procesa. | Usa el generador común sin recortar el texto, devuelve el resultado de la fuente (por ejemplo, `Anna_López!` produce `anna-lopez`), rechaza el vacío, el rango y las palabras reservadas con los errores de S1.3 y rechaza los caracteres no admitidos con el error del generador. |
| CA09 | [«Generación del alias»](../../../context/specifications.md#generación-del-alias) y [domain-model.md → «User»](../../../context/domain-model.md#user) | Las pruebas de alias de S1.3 actualizadas a la fuente vigente. | Se ejecutan tras adaptar la política. | Pasan; los casos cuyo resultado cambia por la fuente se actualizan y quedan identificados. Domain no consulta la ocupación del alias. |
| CA10 | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) | La implementación de S1.4. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. Las decisiones de este alcance se tomaron con DLG el 09-X-2026 y están recogidas en las fuentes enlazadas. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas](tasks.md).
