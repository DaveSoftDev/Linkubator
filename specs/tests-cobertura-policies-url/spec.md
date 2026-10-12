# Especificación: Tests adicionales de policies URL

## Estado y aprobación

- Estado: aprobada por DLG el 12-X-2026.
- Referencia de aprobación: «Aprobada».

## Objetivo

Ampliar los tests de `ImageUrlPolicy`, `LinkUrlPolicy`, `UriSchemePolicy`, `UrlHostPolicy`, `UrlNormalizationPolicy` y `UrlValidationPolicy` para cubrir decisiones de clasificación, fallos de extracción, delimitadores de autoridad, resolución de referencias y normalización de consultas que no quedan suficientemente demostradas por las suites actuales. El trabajo no modifica código de producción ni reglas funcionales.

## Fuentes

- [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri)
- [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url)
- [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image)
- [specifications.md → «Validación»](../../context/specifications.md#validación)
- [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados)
- [S2.1 → «Aceptación y cierre»](../archive/s2.1-ajuste-de-url/tasks.md#aceptación-y-cierre)
- [S2.2 → «Aceptación y cierre»](../archive/s2.2-ajuste-de-image/tasks.md#aceptación-y-cierre)
- [S2.3 → «Aceptación y cierre»](../archive/s2.3-validacion-de-urls/tasks.md#aceptación-y-cierre)
- [S2.4 → «Aceptación y cierre»](../archive/s2.4-normalizacion-de-urls/tasks.md#aceptación-y-cierre)
- [Tests adicionales de `EmailPolicy`, `ImageUrlPolicy` y `LinkUrlPolicy`](../tests-adicionales-policies-email-url/spec.md)

## Alcance

- Incluye:
  - Tests adicionales en las suites existentes de las seis policies URL.
  - Clasificación de entradas con esquema, host y puerto, delimitadores de consulta/fragmento y entradas no clasificables.
  - Casos de éxito y rechazo de extracción de host y validación de autoridad.
  - Resolución de referencias de imagen y cambio de esquema según las reglas fuente.
  - Conservación y ordenación de parámetros repetidos, eliminación de marketing sin distinguir mayúsculas y tratamiento de `+` en consulta durante la normalización.
- Excluye:
  - Cambios en código de producción, reglas funcionales, mensajes o excepciones.
  - Repetir las tablas de ejemplos y matrices ya cubiertas salvo que un caso sea necesario para verificar una rama distinta.
  - Persistencia, entidades, scraping, red, resolución DNS y capas superiores.
- Trabajo vecino y responsable de sus entregables:
  - S2.1–S2.4 implementaron y probaron el ajuste, la validación y la normalización; están aceptados y archivados.
  - La propuesta [tests-adicionales-policies-email-url](../tests-adicionales-policies-email-url/spec.md) cubre límites y recorte en `ImageUrlPolicy` y `LinkUrlPolicy`; este trabajo amplía otros comportamientos y añade las cuatro policies restantes.

## Dependencias y prerrequisitos

- Las seis policies y sus suites están presentes en Domain y en `tests/Linkubator.Tests/Domain/Policies/`.
- Las fuentes de detección de esquema, ajuste, validación y normalización están aprobadas.
- La spec, el plan y las tareas de este trabajo requieren aprobaciones secuenciales. No se modifica código hasta que las tareas estén aprobadas.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | Entradas con esquema explícito, host y puerto, delimitadores permitidos y texto que no clasifica como esquema ni como host válido. | Se detectan con `UriSchemePolicy`. | El resultado identifica el esquema, el host y el tipo de entrada de acuerdo con la fuente; los casos no clasificables no se confunden con un esquema explícito. |
| CA02 | [specifications.md → «Validación»](../../context/specifications.md#validación) | Autoridades válidas e inválidas, incluidos hosts IPv6 entre corchetes y variantes de puerto malformadas. | Se extrae o valida el host con `UrlHostPolicy`. | Los métodos distinguen extracción exitosa de fallida, devuelven el host esperado solo en éxito y rechazan los formatos no válidos. |
| CA03 | [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) y [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url) | Un host y puerto válido seguido por `/`, `?` o `#`. | Se ajusta con `LinkUrlPolicy`. | Se reconoce como autoridad y se aplica el esquema previsto, conservando la ruta, consulta o fragmento. |
| CA04 | [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una referencia relativa formada solo por fragmento y una base válida con esquema `http`. | Se ajusta con `ImageUrlPolicy`. | Se conservan la ruta y consulta de la base, se sustituye el fragmento y el resultado usa `https`. |
| CA05 | [specifications.md → «Validación»](../../context/specifications.md#validación) | URLs aceptables e inválidas por esquema, host, credenciales o puerto. | Se validan con `UrlValidationPolicy`. | Los casos válidos se aceptan y cada caso inválido produce la excepción de dominio correspondiente, sin exponer la entrada. |
| CA06 | [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | Consultas con parámetros repetidos, parámetros de marketing con variaciones de mayúsculas, `+` literal o decodificado en consulta y fragmentos diferentes. | Se normalizan con `UrlNormalizationPolicy`. | Se conservan y ordenan los parámetros repetidos, se eliminan los parámetros de marketing sin distinguir mayúsculas, se aplica la decodificación definida y el fragmento no altera la clave. |
| CA07 | CA01–CA06 | Las suites existentes y los tests añadidos. | Se ejecutan los filtros de las seis suites y la solución completa. | Todos pasan y los cambios de código quedan limitados a archivos de tests. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. Si un test derivado de una fuente falla por una discrepancia de producción, detener esa tarea y elevar el caso; esta propuesta no autoriza corregir producción. | [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | CA01–CA06 | DLG decidirá cualquier cambio de comportamiento en su documento propietario. |

## Artefactos relacionados

Plan técnico: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Aprobada»). Tareas y cierre: [tasks.md](tasks.md), trabajo completado por DLG el 12-X-2026 («Completada»).