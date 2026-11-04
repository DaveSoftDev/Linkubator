# Tareas: S12.5 — Revisión manual de accesibilidad

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada.
  - Inicio: 01-XI-2026, por solicitud de DLG con la referencia «Iniciamos implementación».
  - Finalización: 01-XI-2026; tareas T01–T06 completadas y resultado aceptado con límites.
  - Evidencia consolidada: [matriz A01–A11](../../plans/mvp0-checklist-test-matrix.md) y [evidencia por criterio](#evidencia-por-criterio).

## Descomposición

T02–T04 pueden ejecutarse independientemente una vez que T01 haya preparado el inventario y el entorno; su ejecución concurrente no es necesaria. T05 requiere que las comprobaciones estén registradas y T06 requiere la evidencia consolidada.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Registrar el inicio real, actualizar el avance de S12.5 en el roadmap, inventariar las páginas, formularios, errores y estados vacíos a partir de las rutas de la aplicación, y comprobar la disponibilidad del entorno de revisión. | CA01, CA09; [plan.md → «Flujos internos»](plan.md#flujos-internos), V01 y V09 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | Ninguna | Contrastar las rutas de la aplicación con el alcance público y privado y las filas A01–A11. Resultado: inventario registrado y acceso al área pública y privada confirmado, sin declarar revisiones de accesibilidad aún no realizadas. | completada | Inventario y entorno registrados en «Inventario de páginas y disponibilidad». DLG confirmó acceso a las áreas pública y privada con Chromium el 01-XI-2026. |
| T02 | Revisar la navegación con teclado, el orden y visibilidad del foco, el foco en formularios y errores, y la asociación de labels. | CA01–CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), V01–V03 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Recorrer las páginas y estados incluidos y contrastar el foco y las etiquetas con los requisitos. Resultado: páginas, controles y campos cubiertos, con discrepancias reproducibles o límites documentados. | completada | CA01 y CA03 completados; CA02 parcial por cobertura limitada del foco en formularios/errores. La navegación con teclado, visibilidad y orden del foco se comprobaron en las páginas públicas y privadas inventariadas. Las etiquetas de altas/ediciones de colección y enlace, mover enlace, alta rápida y búsqueda del dashboard y los listados enfocaron o activaron sus campos; Narrador anunció «Buscar enlaces» como nombre de la búsqueda lateral. En el formulario de colección, el envío vacío mostró validación nativa; un nombre de un espacio produjo error propio con mensaje junto al campo y resumen, campo rojo, enfocado y seleccionado. |
| T03 | Comprobar los anuncios para tecnologías de asistencia, los landmarks y encabezados, y el mecanismo para saltar al contenido. | CA04–CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos), V04–V05 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Usar y registrar la tecnología de asistencia disponible; revisar los mensajes y la estructura de las páginas incluidas. Resultado: anuncios, estructuras, mecanismo de salto y límites identificados. | completada | Evidencia y límites registrados: Narrador de Windows anunció automáticamente «El nombre es obligatorio, requerido» al enviar el formulario de colección con un espacio. Durante un intento de revisar `/app` se oyó «Punto de referencia principal», pero no se pudo determinar a qué comando correspondía ni se obtuvo la jerarquía de encabezados. DLG pidió omitir más comprobaciones con Narrador. En `/app` y `/`, el primer Tab enfocó «Saltar al contenido» y Enter llevó el foco al contenido principal. |
| T04 | Revisar contraste y dependencia del color, estados vacíos, nombres accesibles y `prefers-reduced-motion`. | CA06–CA08; [plan.md → «Flujos internos»](plan.md#flujos-internos), V06–V08 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Comprobar las combinaciones, controles, estados y comportamientos disponibles. Resultado: alcance, método, observaciones, incumplimientos y límites registrados. | completada | Evidencia y límites registrados: cálculo con los valores del CSS generado para `.public-intro` de `/Error` (`#b7b7b7` sobre `#070b0d`): 9,85:1 (AA para texto normal); DLG confirmó que `/Error` mantiene el mismo estilo que Linkubator; el error de colección se mostró con texto y campo rojo; se probaron estados vacíos de enlaces y colecciones privados, y DLG aclaró que la página ofrece controles de creación aunque el texto de orientación no sea un enlace; DevTools calculó nombres para tres controles; con `prefers-reduced-motion: reduce`, el hover de `/app` permaneció pero el cambio ocurrió instantáneamente. Otras combinaciones, estados vacíos y nombres no se revisaron; quedan reflejados como límites. |
| T05 | Consolidar los resultados de T02–T04 en A01–A11, completar la evidencia por criterio y actualizar el estado de S12.5 en el roadmap sin anticipar aceptación. | CA01–CA09; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V09 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T02, T03, T04 | Contrastar cada criterio con las comprobaciones realizadas y las filas correspondientes de la matriz. Resultado: ninguna comprobación pendiente figura como satisfactoria y ningún hallazgo se presenta como corregido. | completada | Matriz A01–A11 contrastada y actualizada con resultados reales y límites; CA02 y CA04–CA07 permanecen parciales, sin presentar comprobaciones pendientes como satisfactorias. Estado de S12.5 sincronizado en [roadmap.md](../../context/roadmap.md). |
| T06 | Revisar la evidencia consolidada y registrar la aceptación o el rechazo humano del resultado; si se rechaza, anotar las correcciones necesarias sin declarar cerrado el trabajo. | CA01–CA09; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Confirmar que CA01–CA09 tienen evidencia satisfactoria o límites explícitos y registrar la decisión humana. Resultado: aceptación o rechazo documentado, sin confundirlo con las pruebas de S12.6. | completada | DLG aceptó el resultado con los límites documentados el 01-XI-2026 (respuesta de T06: `acepto_con_limites`). Los criterios parciales siguen identificados como tales; no se declara que toda la accesibilidad esté verificada ni que los hallazgos estén corregidos. |

### Inventario de páginas y disponibilidad (T01)

Rutas de páginas públicas identificadas en el proyecto:

- `/` (inicio), `/Privacy`, `/Error` y `/404`.
- `/{alias}/` (página pública de usuario).
- `/{alias}/colecciones/` (colecciones públicas del usuario).
- `/{alias}/colecciones/{slug}` (colección pública).

Rutas privadas identificadas:

- `/app` (dashboard).
- `/app/colecciones`, `/app/colecciones/nueva`, `/app/colecciones/{id}/editar` y `/app/colecciones/{id}/eliminar`.
- `/app/enlaces`, `/app/enlaces/nuevo`, `/app/enlaces/{id}/editar`, `/app/enlaces/{id}/mover` y `/app/enlaces/{id}/eliminar`.
- `/app/404` (recurso privado no disponible).
- `/app/presentacion` (página de demostración de componentes).

El recorrido posterior incluye los formularios de alta, edición, movimiento y eliminación, los formularios de búsqueda y alta rápida, los errores de validación y los estados vacíos de páginas públicas y privadas. Su revisión y sus resultados pertenecen a T02–T04; este inventario no afirma que esos estados se hayan probado.

Casos de formularios y estados identificados para el recorrido:

- Formularios: alta rápida desde el dashboard; alta, edición y eliminación de colecciones; alta, edición, movimiento y eliminación de enlaces; búsqueda en listados y navegación lateral.
- Errores: validación de formularios y mensajes de error/estado asociados; recurso privado no disponible en `/app/404`; páginas generales `/404` y `/Error`.
- Estados vacíos: páginas públicas de usuario, listado de colecciones públicas y detalle de colección; dashboard, listados privados de colecciones y enlaces, y resultados de búsqueda sin coincidencias. `/app/presentacion` contiene estados de demostración, no es una página funcional de usuario.

Disponibilidad confirmada por DLG: acceso a las áreas pública y privada mediante Chromium y Narrador de Windows disponible para T03, 01-XI-2026. Tras una prueba de anuncio de error y un intento no concluyente de estructura, DLG pidió omitir más comprobaciones con Narrador.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02 | V01 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: todas las páginas públicas y privadas inventariadas, incluidas páginas de error, listados, formularios y confirmaciones, se recorrieron con Tab en Chromium; se alcanzaron los elementos interactivos revisados. El detalle público se abrió con Tab + Enter; los borrados no se ejecutaron. | 01-XI-2026; Chromium | [Matriz A01](../../plans/mvp0-checklist-test-matrix.md) |
| CA02 | T02 | V02 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: foco visible y orden de Tab coincidente con el visual en las páginas recorridas, incluidos formularios y barra lateral. Se observó el foco inicial en formularios de colección y enlace, y un error de aplicación con foco en el campo. | 01-XI-2026; Chromium | [Matriz A02–A03](../../plans/mvp0-checklist-test-matrix.md) |
| CA03 | T02 | V03 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: en altas/edición de colección y enlace, mover enlace, dashboard y listados privados, las etiquetas enfocaron o activaron sus campos. Narrador anunció «Buscar enlaces» como nombre accesible de la búsqueda lateral. | 01-XI-2026; Chromium y Narrador de Windows | [Matriz A04](../../plans/mvp0-checklist-test-matrix.md) |
| CA04 | T03 | V04 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Parcial: Narrador de Windows anunció automáticamente «El nombre es obligatorio, requerido» al enviar el formulario de colección con un espacio. Otros mensajes siguen pendientes. DLG pidió omitir más pruebas con Narrador. | 01-XI-2026; Chromium y Narrador de Windows | [Matriz A05 y P27](../../plans/mvp0-checklist-test-matrix.md) |
| CA05 | T03 | V05 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: en `/app` y `/`, el primer Tab enfocó «Saltar al contenido» y Enter llevó el foco al contenido principal.  | 01-XI-2026; Chromium y Narrador de Windows | [Matriz A06–A07](../../plans/mvp0-checklist-test-matrix.md) |
| CA06 | T04 | V06 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: texto `.public-intro` de `/Error`, `#b7b7b7` sobre fondo `#070b0d`, ratio calculado de 9,85:1 a partir del CSS generado. En el error de colección, el mensaje textual acompaña al campo rojo. | 01-XI-2026; CSS generado y Chromium; aclaración de DLG | [Matriz A08](../../plans/mvp0-checklist-test-matrix.md) |
| CA07 | T04 | V07 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: búsquedas sin coincidencias en `/app/enlaces` y `/app/colecciones` mostraron mensajes textuales de estado vacío. DevTools calculó «Colapsar menú lateral», «Crear enlace» y «Volver al inicio» para controles de `/app/enlaces` y `/Error`. | 01-XI-2026; Chromium y DevTools; aclaración de DLG | [Matriz A09–A10](../../plans/mvp0-checklist-test-matrix.md) |
| CA08 | T04 | V08 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: con `prefers-reduced-motion: reduce` emulado en DevTools, el hover de navegación de `/app` permaneció visible, pero su cambio de estilo ocurrió instantáneamente. | 01-XI-2026; Chromium, DevTools | [Matriz A11](../../plans/mvp0-checklist-test-matrix.md) |
| CA09 | T01, T05, T06 | V09 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado: matriz A01–A11 contrastada con las comprobaciones y límites documentados; los resultados parciales no se presentan como satisfactorios ni como corregidos. | 01-XI-2026; revisión documental | [Matriz de checklist](../../plans/mvp0-checklist-test-matrix.md) |

Un resultado no disponible o fallido seguirá pendiente o bloqueado; no se registrarán resultados esperados como reales.

## Hallazgos

- Incumplimientos confirmados: ninguno en las comprobaciones realizadas.
- Aclaraciones de DLG: el texto de orientación del estado vacío no es un enlace, pero la pantalla incluye las acciones para crear enlaces y colecciones; `/Error` mantiene el mismo estilo que el resto de Linkubator. Se corrigen esas dos observaciones previas: no son hallazgos.
- Los criterios parciales reflejan cobertura pendiente, no incumplimientos confirmados. Sus límites permanecen en [«Evidencia por criterio»](#evidencia-por-criterio).

## Aceptación y cierre

- Estado del trabajo: implementación completada y aceptada con límites por DLG el 01-XI-2026; T01–T06 completadas; CA01-CA03 y CA05–CA09 completados; CA04 parcial.
- Criterios sin evidencia satisfactoria: CA04, parcial según la evidencia por criterio.
- Bloqueos y riesgos restantes: la cobertura de anuncios y estructura con tecnología de asistencia es limitada; DLG pidió omitir más pruebas con Narrador.
- Aceptación humana: aceptada con límites por DLG el 01-XI-2026; referencia `acepto_con_limites` («Acepto el resultado con los límites documentados»).
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: S12.5 queda aceptada con los límites registrados; el archivado no se ha solicitado.
