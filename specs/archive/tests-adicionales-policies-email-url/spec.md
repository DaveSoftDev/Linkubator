# Especificación: Tests adicionales de `EmailPolicy`, `ImageUrlPolicy` y `LinkUrlPolicy`

## Estado y aprobación

- Estado: aceptada y cerrada por DLG el 13-X-2026; no archivada.
- Referencia de aprobación: «Adelante».
- Referencia de aceptación: «Aceptado».

## Objetivo

Completar los tests de las tres policies en los límites y ramas que no quedan demostrados por las suites actuales, sin modificar el comportamiento de producción ni duplicar sus matrices de ejemplos existentes.

## Fuentes

- [specifications.md → «Email»](../../../context/specifications.md#email)
- [specifications.md → «Longitudes máximas»](../../../context/specifications.md#longitudes-máximas)
- [specifications.md → «Ajuste de URL»](../../../context/specifications.md#ajuste-de-url)
- [specifications.md → «Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image)
- [S3.1 → «Criterios de aceptación»](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación)
- [S2.1 → «Aceptación y cierre»](../s2.1-ajuste-de-url/tasks.md#aceptación-y-cierre)
- [S2.2 → «Aceptación y cierre»](../s2.2-ajuste-de-image/tasks.md#aceptación-y-cierre)
- [S2.5 → «Aceptación y cierre»](../s2.5-tests-de-ejemplos-de-urls/tasks.md#aceptación-y-cierre)

## Alcance

- Incluye:
  - Tests unitarios adicionales para `EmailPolicy`, `ImageUrlPolicy` y `LinkUrlPolicy`.
  - Comprobación de las formas de email ya admitidas que no ejercitan los tests actuales, de los límites de longitud y de los tipos de rechazo correspondientes.
  - Comprobación de la aceptación en el límite de longitud de las URL ajustadas y de que los espacios exteriores se recortan.
- Excluye:
  - Cambios en código de producción, reglas funcionales, mensajes o excepciones.
  - Repetir los ejemplos de las tablas de URL que ya están cubiertos.
  - Cambios en normalización, validación posterior de URLs, entidades, persistencia o capas superiores.
- Trabajo vecino y responsable de sus entregables:
  - S3.1 es responsable de la creación inicial y los tests de `EmailPolicy`.
  - S2.1 y S2.2 son responsables de la implementación y los tests base de ajuste de `UrlOriginal` e `Image`; sus trabajos están aceptados y archivados.
  - S2.5 es responsable de los tests de ejemplos funcionales de URL; su trabajo está aceptado y archivado.

## Dependencias y prerrequisitos

- Las tres policies y sus suites existen en Domain y en `tests/Linkubator.Tests/Domain/Policies/`.
- Las reglas de email y ajuste de URL citadas están aprobadas.
- La aprobación de esta especificación es necesaria antes de preparar el plan técnico. No se modifica código hasta que el plan y las tareas posteriores estén aprobados.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Email»](../../../context/specifications.md#email) | Direcciones con las alternativas admitidas de parte local y dominio que aún no están ejercitadas. | Se normalizan con `EmailPolicy`. | Se aceptan y se normalizan según la fuente, sin alterar puntos ni el signo `+` de la parte local. |
| CA02 | [specifications.md → «Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) y [specifications.md → «Email»](../../../context/specifications.md#email) | Un email válido en el máximo permitido y otro que lo supera. | Se normalizan con `EmailPolicy`. | El primero se acepta y el segundo se rechaza por longitud, sin confundir el resultado con un error de sintaxis. |
| CA03 | [S3.1 → «Criterios de aceptación»](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación) | Una entrada ausente o vacía, una dirección con sintaxis no admitida y una dirección válida. | Se aplica `EmailPolicy.Normalize`. | Los tests distinguen la excepción de entrada ausente, la de longitud y la de sintaxis, y comprueban el resultado normalizado de la válida. |
| CA04 | [specifications.md → «Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image) y [specifications.md → «Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Una URL de imagen ajustada en el máximo permitido y otra por encima. | Se ajustan con `ImageUrlPolicy` contra una base pública válida. | La primera se acepta; la segunda se rechaza por longitud medida sobre la URL ajustada. |
| CA05 | [specifications.md → «Ajuste de `Image`»](../../../context/specifications.md#ajuste-de-image) | Una URL de imagen absoluta rodeada de espacios exteriores. | Se ajusta con `ImageUrlPolicy`. | El resultado coincide con el ajuste especificado y no conserva esos espacios. |
| CA06 | [specifications.md → «Ajuste de URL»](../../../context/specifications.md#ajuste-de-url) y [specifications.md → «Longitudes máximas»](../../../context/specifications.md#longitudes-máximas) | Una entrada cuya URL ajustada alcanza el máximo permitido y otra cuyo ajuste lo supera. | Se ajustan con `LinkUrlPolicy`. | La primera se acepta y la segunda se rechaza por longitud medida sobre el resultado ajustado, incluido el esquema que corresponda. |
| CA07 | [specifications.md → «Ajuste de URL»](../../../context/specifications.md#ajuste-de-url) | Una URL absoluta rodeada de espacios exteriores. | Se ajusta con `LinkUrlPolicy`. | El resultado no conserva los espacios exteriores y mantiene el resto de la URL según la fuente. |
| CA08 | CA01–CA07 | Las suites existentes y los tests añadidos. | Se ejecutan los tests focalizados y la suite completa. | Todos pasan; los cambios quedan limitados a archivos de tests y no alteran los ejemplos ya aceptados. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. | - | - | - |

## Artefactos relacionados

Plan técnico: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Adelante»). Tareas y evidencia de cierre: [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre), aceptadas por DLG el 13-X-2026 («Aceptado»).