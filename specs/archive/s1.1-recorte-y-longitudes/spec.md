# Especificación: S1.1 — Recorte y validación de longitudes

## Estado y aprobación

- Estado: aprobada por DLG el 07-X-2026.
- Referencia de aprobación: «Comprobada la lógica de las necesidades».
- Este documento describe el resultado esperado; el diseño y la evidencia de implementación pertenecen a los artefactos relacionados.

## Objetivo

Proporcionar en Domain el comportamiento común de recorte y validación de longitudes para los textos de dominio introducidos por el usuario, con conteo por puntos de código Unicode y conforme a las reglas fuente.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.1 y reparto con las tareas vecinas.
- [specifications.md → «Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario): recorte, tratamiento del texto vacío y rechazo de entradas demasiado largas.
- [specifications.md → «Longitudes máximas»](../../../context/specifications.md#longitudes-máximas): unidad de conteo y límites por propiedad.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.

## Alcance

- Incluye:
  - Implementar y probar en Domain el comportamiento común para recortar textos, tratar valores vacíos y contar/comprobar su longitud en puntos de código Unicode dado el límite aplicable.
  - Comprobar la longitud con el máximo recibido, sin truncar ni modificar la entrada. El rechazo efectivo y la política de obligatoriedad corresponden a la integración en las entidades, en S3.
  - Tests automatizados del comportamiento común, incluidos límites, caracteres fuera del BMP y marcas combinantes.
  - Mantener este comportamiento común independiente de las reglas particulares de las entidades que lo consuman.

- Excluye:
  - Contraseñas y parámetros de búsqueda, que no son textos de dominio.
  - Generar alias o slugs y decidir en qué momento validar sus resultados transformados; corresponde a S1.3 y S1.4.
  - Integrar la validación en propiedades concretas de `User`, `Collection` o `Link`, o hacer que esas entidades protejan sus invariantes; corresponde a S3, al implementar las entidades.
  - Validar URLs, ajustar imágenes o normalizarlas; corresponde a S2.
  - Definir o cambiar reglas, límites o mensajes de producto.

- Trabajo vecino y responsable de sus entregables:
  - S1.2 define los errores de dominio base y los enumeradores necesarios.
  - S1.3 y S1.4 implementan la generación de alias y slugs, respectivamente.
  - S1.5 cubre los ejemplos de alias y slugs; no sustituye las pruebas del comportamiento común de S1.1.
  - S1.6 y S1.7 corresponden a revisión y decisiones humanas del sprint.

## Dependencias y prerrequisitos

- Existe el proyecto `Linkubator.Domain`, configurado para .NET 10.
- La solución ya dispone de un proyecto de tests con referencia a Domain.
- Los nombres y firmas concretos de los componentes comunes se decidirán en el plan técnico; no forman parte de las reglas funcionales de esta especificación.
- Esta especificación está aprobada. El plan y las tareas mantienen sus propios estados de aprobación, que se registrarán en esos documentos.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario) | Una cadena con espacios iniciales y finales. | Se llama a `UserTextPolicy.TrimToNull`. | Devuelve la cadena recortada y conserva intacto el contenido interior. |
| CA02 | [«Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario) | Una entrada nula, vacía o que queda vacía tras el recorte. | Se llama a `UserTextPolicy.TrimToNull`. | Devuelve `null`; la obligatoriedad u opcionalidad del campo queda fuera de S1.1. |
| CA03 | [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Una cadena cuyo conteo por puntos de código no supera el máximo recibido. | Se llama a `UserTextPolicy.IsWithinMaximumLength`. | Devuelve `true`. |
| CA04 | [«Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Una cadena cuyo conteo por puntos de código supera el máximo recibido. | Se llama a `UserTextPolicy.IsWithinMaximumLength`. | Devuelve `false` y deja intacta la cadena; el rechazo por entidad queda fuera de S1.1 y se comprobará en S3. |
| CA05 | [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Texto que contiene una pareja sustituta UTF-16 que representa un carácter fuera del BMP. | Se calcula su longitud. | La pareja cuenta como un punto de código, no como dos unidades UTF-16. |
| CA06 | [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Texto que contiene una letra seguida de una o más marcas combinantes. | Se calcula su longitud. | Se cuenta cada punto de código; no se cuenta el grafema como una única unidad. |
| CA07 | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) | La implementación del comportamiento de S1.1. | Se comprueban las dependencias del proyecto Domain. | La implementación reside en Domain y no introduce dependencias de Infrastructure, Application ni Web. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para el alcance limitado a textos de dominio. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).
