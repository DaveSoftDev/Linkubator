# Especificación: S2.8 — Resolución de hallazgos de URLs reales

## Estado y aprobación

- Estado: aprobada por DLG el 12-X-2026.
- Referencia de aprobación: «Aprobada».

## Objetivo

Resolver los hallazgos H01 y H02 de la revisión S2.7 alineando la regla vigente, Domain, sus tests y la evidencia de la URL afectada. Cerrar H03 por referencia a la regla y al ejemplo existentes, sin tratarlo como una decisión o incoherencia nueva.

## Fuentes

- [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l): tarea S2.8 y Definition of Done del sprint.
- [specs/s2.7-revision-con-urls-reales/tasks.md → «Hallazgos para S2.8»](../s2.7-revision-con-urls-reales/tasks.md#hallazgos-para-s28): hallazgos H01, H02 y H03.
- [specs/s2.7-revision-con-urls-reales/tasks.md → «URLs reales»](../s2.7-revision-con-urls-reales/tasks.md#urls-reales): entradas concretas relacionadas con H01, H02 y H03.
- [specs/s2.7-revision-con-urls-reales/tasks.md → «Colisiones»](../s2.7-revision-con-urls-reales/tasks.md#colisiones): URLs de las filas 9 y 10.
- [specifications.md → «Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados): regla propietaria de la clave, tratamiento de caracteres Unicode, lista de parámetros y equivalencia de puertos por defecto.
- [specifications.md → «Duplicados»](../../../context/specifications.md#duplicados): uso de la clave y disponibilidad de la URL original.
- [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados): riesgos aceptados de la clave plana.
- [domain-model.md → «Link»](../../../context/domain-model.md#link): naturaleza de `UrlOriginal` y `UrlNormalized`.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.
- [roadmap.md → «Dominio y aplicación»](../../../context/roadmap.md#dominio-y-aplicación): trazabilidad de S2.

## Alcance

- Incluye:
  - Resolver H01 incorporando `gclsrc` a la lista propietaria de parámetros de marketing, adaptando Domain y cubriendo el resultado con un test de regresión basado en la URL de la fila 6 de S2.7.
  - Investigar H02 con la URL percent-encoded de la fila 25 de S2.7 y contrastar el resultado real de `UrlNormalizationPolicy.Normalize` con el resultado derivado de la fuente. La comprobación distingue una corrupción producida por Domain de una corrupción introducida al capturar, guardar o mostrar la salida.
  - Si Domain produce una clave que no corresponde a la fuente, corregir la implementación y añadir tests de regresión para la URL codificada y su representación Unicode equivalente. Si Domain produce el resultado definido por la fuente, registrar en este trabajo la evidencia fiel y la causa de la corrupción observada, sin modificar la regla ni filtrar la salida a ASCII.
  - Resolver H03 por referencia: el tratamiento de `:443` ya está definido por [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) y demostrado por su ejemplo de puertos por defecto. No requiere una decisión, una entrada nueva en `decisions.md` ni un cambio de comportamiento.
  - Registrar en los artefactos de este trabajo el resultado y la evidencia de cada hallazgo, sin reescribir el registro histórico de S2.7.

- Excluye:
  - Cambiar la regla propietaria sobre la conservación de letras y números Unicode, eliminar en general los caracteres fuera de ASCII o cambiar la normalización de kana. Cualquier cambio de esa regla requiere una decisión explícita en su documento propietario.
  - Añadir H01 a `decisions.md`: H01 es una ampliación de la lista que pertenece a `specifications.md`.
  - Añadir una decisión o cambiar la regla, el código o los tests para H03; la equivalencia de las URLs de las filas 9 y 10 ya está especificada.
  - Alterar reglas ajenas a `UrlNormalized`, revisar URLs nuevas o resolver duplicados almacenados, que corresponden a otros trabajos.

- Trabajo vecino y responsable de sus entregables:
  - S2.7 aporta las URLs reales, sus resultados registrados y los hallazgos.
  - S2.8 resuelve H01 y H02 en la regla, Domain, tests y evidencia según corresponda, y cierra H03 por trazabilidad.
  - S2.4 implementó la normalización y S2.5 sus tests de ejemplos; S2.8 modifica únicamente el comportamiento cubierto por H01 o un defecto comprobado para H02.
  - S3 integra los valores en `Link`; S4 y S5 aplican la detección de duplicados.

## Dependencias y prerrequisitos

- S2.7 fue aceptada por DLG el 12-X-2026 con la referencia «Con errores»; sus hallazgos H01, H02 y H03 constan en `tasks.md`.
- Existen `UrlNormalizationPolicy` y los tests [UrlNormalizationPolicyTests](../../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationPolicyTests.cs) y [UrlNormalizationExamplesTests](../../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationExamplesTests.cs). Estos ya cubren caracteres Unicode escritos directamente, pero no la URL percent-encoded de la fila 25.
- La fuente vigente especifica conservar letras y números Unicode. La descripción de H02 en S2.7 atribuye mojibake al código, pero esa atribución no está demostrada independientemente de la captura de salida; resolver cuál capa lo introduce forma parte de este trabajo.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) y [S2.7 → «URLs reales»](../s2.7-revision-con-urls-reales/tasks.md#urls-reales) | La URL de la fila 6, cuyo parámetro `gclsrc` no figura en la lista vigente. | Se actualizan la fuente, Domain y sus tests. | `gclsrc` queda incluido en la lista propietaria y deja de contribuir a la clave; un test de regresión fija el resultado de esa URL. |
| CA02 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) y [S2.7 → «URLs reales»](../s2.7-revision-con-urls-reales/tasks.md#urls-reales) | La URL percent-encoded de la fila 25 y la misma URL con la ruta representada como Unicode. | Se calcula la clave dentro de un test y se compara con el resultado derivado independientemente de la fuente, usando una comprobación que no dependa de la codificación de la consola. | Queda establecido si Domain devuelve el resultado de la fuente y ambas representaciones equivalentes producen la misma clave, o si Domain introduce una divergencia real. |
| CA03 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) y [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) | El resultado de CA02. | Se identifica la capa que introduce el mojibake. | Si la divergencia está en Domain, la implementación se corrige y los tests fijan la salida conforme a la fuente; si está en la captura o representación, se registra evidencia fiel y no se modifica la semántica Unicode ni se impone salida ASCII. |
| CA04 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) y [S2.7 → «Colisiones»](../s2.7-revision-con-urls-reales/tasks.md#colisiones) | H03 y las URLs de las filas 9 y 10. | Se contrasta el hallazgo con el paso y el ejemplo de puertos por defecto de la fuente. | H03 queda cerrado por referencia como comportamiento ya especificado; no se registra como decisión nueva ni se cambia la regla o el código. |
| CA05 | [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | Los cambios necesarios para CA01 y, si aplica, CA03. | Se ejecutan los tests de normalización y la suite integrada, y se compila la solución. | Los tests cubren el resultado decidido, la suite pasa, la compilación no produce advertencias y los cambios de producción se limitan a `UrlNormalizationPolicy`. |
| CA06 | [mvp0-plan.md → «S2: Dominio II: URLs (L)»](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l) | H01, H02 y H03. | Se revisa la evidencia final. | H01 queda resuelto en regla, Domain y test; H02 queda resuelto por corrección de Domain o por diagnóstico y evidencia fiel; H03 queda cerrado por referencia, sin decisión redundante. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| La salida registrada para H02 aparece como mojibake, pero no hay evidencia independiente que demuestre que esa secuencia la devuelve `UrlNormalizationPolicy.Normalize` en vez de producirse al capturar o mostrar la salida. | [S2.7 → «Hallazgos para S2.8»](../s2.7-revision-con-urls-reales/tasks.md#hallazgos-para-s28) y [«Normalización para duplicados»](../../../context/specifications.md#normalización-para-duplicados) | CA02, CA03 | Ninguna previa para diagnosticar. Si el diagnóstico sugiriera cambiar la regla de conservación Unicode, detener el trabajo y solicitar una decisión explícita antes de editar su fuente o implementar ese cambio. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 12-X-2026 («Listo»).
- [Tareas](tasks.md), aprobadas por DLG el 12-X-2026 («Vamos»); implementación y validación completadas el 12-X-2026 («Solucionado»), con aceptación humana pendiente.