# Especificación: S3a.1 — Refactor de las políticas de URL

## Estado y aprobación

- Estado: borrador. Se redacta después de la implementación, ejecutada por autorización directa de DLG sobre el plan de [.kilo/plans/1791934991905-s3a-refactor-policies-url.md](../../.kilo/plans/1791934991905-s3a-refactor-policies-url.md). La aprobación formal de esta especificación queda pendiente.
- Aprobación: pendiente; registrar fecha, responsable y referencia cuando exista.

## Objetivo

Dejar el clúster de políticas de URL de Domain en la mínima estructura que aplica las reglas ya aprobadas: un único punto de análisis de la sintaxis de autoridad, una única comprobación de esquemas permitidos, una única higiene de entrada y ningún método sin consumidor, sin cambiar el resultado de ninguna entrada.

## Fuentes

- [context/specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri)
- [context/specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url)
- [context/specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image)
- [context/specifications.md → «Validación»](../../context/specifications.md#validación)
- [context/specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas)
- [context/decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados)
- [plans/mvp0-plan.md → «S3a: Dominio IV: refactor (M)»](../../plans/mvp0-plan.md#s3a-dominio-iv-refactor-M)

## Alcance

- Incluye:
  - `UriAuthorityPolicy`, `UrlValidationPolicy`, `UrlNormalizationPolicy`, `UserTextPolicy`, `LinkUrlPolicy` e `ImageUrlPolicy`.
  - La fusión de `UrlHostPolicy` dentro de `UriAuthorityPolicy` y la eliminación de `UriSchemePolicy` con su record `UriSchemeDetection`.
  - La nueva `UrlInputPolicy` con la higiene de entrada común.
  - La división interna de `UriAuthorityPolicy` en fachada pública y colaboradores internos (`UriAuthoritySyntax`, `UriHostFormat`, `UriPortFormat`), añadida en la revisión de S3a.1: la fachada conserva el contrato público y las reglas no cambian.
  - La adecuación de la nomenclatura de los rechazos del ajuste de URL a su causa (DLG, «adecuar nomenclaturas»): host presente con formato no admitido, puerto no válido y autoridad ausente pasan a nombrarse cada uno con su excepción. Ninguna entrada cambia de aceptada a rechazada; solo cambia el código devuelto en las ya rechazadas.
  - La alineación de las dos excepciones genéricas que quedaban en Domain con la regla de [architecture.md → «Domain»](../../context/architecture.md#domain): `NotValidInputException` para la base en blanco del ajuste de `Image` y `UrlNotAbsoluteException` para la entrada no absoluta de la normalización. Ninguna entrada cambia de aceptada a rechazada.
  - Los tests de `tests/Linkubator.Tests/Domain/Policies` afectados y los nuevos casos de espacios interiores, bordes de puerto, preparación de entrada y el test estructural que fija que los colaboradores de `UriAuthorityPolicy` no se hagan públicos.
- Excluye:
  - S3a.2 (refactor de las entidades) y las políticas ajenas a las URLs (`EmailPolicy`, `AliasPolicy`, `SlugPolicy`, `AsciiTransformationPolicy`).
  - Cualquier cambio de regla en `context/`: este trabajo no modifica ninguna regla funcional.
  - El commit del WIP previo y el marcado de la Definition of Done de S3a.1, que corresponde al humano.
- Trabajo vecino y responsable de sus entregables: S3a.2 (entidades), pendiente; S4 y posteriores consumen estas políticas sin cambios de contrato.

## Dependencias y prerrequisitos

- El WIP de S3a sin commitear (con `UriAuthorityPolicy` ya creado) se toma como base, por decisión de DLG.
- Línea base verde comprobada el 14-X-2026: 582 tests correctos antes del primer cambio.
- Sin dependencias nuevas: las políticas usan solo tipos de la BCL.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Validación»](../../context/specifications.md#validación) | Una entrada cualquiera de ajuste o validación | Se ejecuta el refactor y la suite completa | El resultado y la excepción de dominio son los mismos que antes del refactor para cada entrada |
| CA02 | [specifications.md → «Detección de esquema URI»](../../context/specifications.md#detección-de-esquema-uri) | La clasificación de esquema, host y puerto de una entrada | La consultan `LinkUrlPolicy` e `ImageUrlPolicy` | Ambos obtienen la clasificación del mismo análisis de `UriAuthorityPolicy`, sin copias propias |
| CA03 | [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url) y [«Validación»](../../context/specifications.md#validación) | Un esquema escrito en la entrada | Se comprueba si está admitido | La comprobación se resuelve en un único punto compartido por los dos ajustes y la validación |
| CA04 | [specifications.md → «Ajuste de URL»](../../context/specifications.md#ajuste-de-url) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Una entrada con espacios interiores, caracteres de control o texto no válido | Se ajusta como `UrlOriginal` o como `Image` | Ambos ajustes rechazan con la misma excepción y la misma comprobación común |
| CA05 | [plans/mvp0-plan.md → «S3a: Dominio IV: refactor (M)»](../../plans/mvp0-plan.md#s3a-dominio-iv-refactor-M) | Los miembros públicos de las políticas de URL | Se revisa el árbol de llamadas | Ninguno queda sin consumidor productivo ni sin test que lo fije |
| CA06 | [context/roadmap.md → «Checklist de calidad del MVP0»](../../context/roadmap.md#checklist-de-calidad-del-mvp0) | La solución completa | Se compila con warnings como errores y se ejecuta la suite | La compilación no produce errores ni warnings y todas las pruebas pasan |

Casos negativos y de límite: los cubren las teorías existentes de cada política y las añadidas para espacios interiores (espacio normal, `\u00A0`), puertos (`+80`, ` 80`, `080`, `0`, `65536`, vacío, ausente) y longitud en puntos de código.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna comprobada durante la implementación | — | — | Queda pendiente la aceptación humana del resultado (DoD de S3a.1) |

## Artefactos relacionados

- [plan.md](plan.md)
- [tasks.md](tasks.md)
