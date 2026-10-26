# Plan técnico: S9.6 — Revisión de la presentación de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 26-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Revisar manualmente los formularios Web ya disponibles en [New.cshtml](../../src/Linkubator.Web/Pages/App/Links/New.cshtml) y [Edit.cshtml](../../src/Linkubator.Web/Pages/App/Links/Edit.cshtml). S9.6 no modifica código, contratos ni reglas.
- La revisión se limita a los campos de título, descripción e imagen, sus etiquetas e indicaciones, los valores presentados en edición y los errores asociados cuando sea seguro provocarlos.
- Los flujos de presentación y los mensajes se contrastan con [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) y sus fuentes propietarias. Las pruebas HTTP existentes no sustituyen la inspección visual.
- La evidencia textual registrará el entorno, navegador y versión, ruta inspeccionada, estado de los datos de prueba y observaciones por criterio. No incluirá credenciales, tokens ni datos privados ajenos a la revisión.

### Flujos internos

1. Confirmar que la aplicación Web y la sesión privada están disponibles. Anotar navegador, versión y entorno; preparar o identificar enlaces propios con campos opcionales informados y vacíos.
2. Abrir el formulario de creación y revisar la presentación de título, descripción e imagen, sus etiquetas, indicaciones de opcionalidad y asociaciones con los controles. Registrar observaciones para CA01.
3. Abrir la edición de enlaces propios preparados y revisar cómo aparecen los valores informados y vacíos. Si se puede hacer de forma segura con datos propios, provocar un error de imagen y observar su presentación sin guardar cambios. Registrar los casos no ejecutados y su motivo para CA02.
4. Consolidar la evidencia y presentar a DLG la decisión sobre el resultado, registrando aceptación explícita o cambios solicitados para CA03. Los cambios de producto se tramitan aparte.

### Persistencia y dependencias

No se modifican persistencia, infraestructura, dependencias ni contratos. La revisión requiere una instancia Web ejecutable, navegador y sesión privada. Los enlaces y colecciones usados deben ser propios y estar destinados a la revisión; antes y después se anotará el estado relevante. No se eliminarán datos de prueba sin autorización expresa.

S9.1 y S9.3 aportan los formularios que se revisan y tienen implementación y aceptación registradas. S9.4, S9.5, el scraping y las páginas públicas no son prerrequisitos. Si no hay datos propios adecuados o no se puede provocar un error de imagen sin riesgo, se deja constancia de ese límite; no se simula una observación ni se bloquea la revisión de los estados normales.

### Garantías técnicas

- La evidencia se limita a los formularios privados disponibles y a los criterios de esta especificación; no afirma que la imagen se represente en páginas públicas ni que se haya ejecutado scraping.
- La inspección visual no sustituye las pruebas automatizadas de contratos o validación.
- Solo se usan datos propios preparados para esta revisión. No se hacen cambios persistentes durante la inspección de errores y no se registran datos sensibles.
- Un hallazgo no autoriza cambios de código dentro de S9.6 ni altera las reglas fuente; cualquier corrección se tramita como trabajo separado.
- El cierre depende únicamente de CA01–CA03 y de las tareas propias de S9.6, no de trabajos futuros.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Inspeccionar en navegador los formularios existentes sin implementar arreglos | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | Los hallazgos se documentan para seguimiento separado. |
| Utilizar enlaces propios preparados con valores opcionales informados y vacíos | Propuesta | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Confirmar que los datos están destinados a la revisión y registrar su estado sin incluir datos sensibles. |
| Concretar navegador, versión, entorno y formato de evidencia antes de iniciar | Pendiente | [spec.md → CA03](spec.md#criterios-de-aceptación) | Registrarlos en la tarea de preparación, sin fijar valores que aún no se conocen. |
| Provocar un error de imagen durante la revisión | Propuesta, condicionado a seguridad | [spec.md → CA02](spec.md#criterios-de-aceptación) y [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) | Hacerlo solo con un enlace propio preparado y si se puede confirmar que ningún cambio queda guardado; en otro caso registrar que no se probó. |
| La evaluación de presentación es cualitativa y no cubre las páginas públicas | Confirmado | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) y [«Alcance»](spec.md#alcance) | Delimita las conclusiones a los formularios privados y requiere registrar observaciones y hallazgos, no una conformidad visual global. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual; formulario privado de creación | Aplicación Web disponible, sesión privada y navegador | Abrir la ruta de creación e inspeccionar título, descripción e imagen, etiquetas, indicaciones de opcionalidad y asociación de controles. | Los tres campos y su estado opcional pueden identificarse y usarse según sus fuentes; registrar cualquier hallazgo con contexto reproducible. | Entorno, navegador/versión, ruta, observaciones por campo y pasos de reproducción de hallazgos. |
| V02 | CA02 | Manual; formulario privado de edición y, si resulta seguro, estado de error de imagen | Enlace propio preparado con valores opcionales informados o vacíos; para error, datos propios que no puedan quedar modificados | Abrir la edición e inspeccionar los valores mostrados y los errores asociados a los campos cuando puedan provocarse de forma segura. Si se envía una entrada inválida, confirmar que la operación no guardó cambios antes de cerrar la comprobación. | Se registra el estado observado de cada campo y del error probado; si el caso negativo no se ejecuta, queda anotado el motivo sin presentarlo como validado. | Identificador de prueba no sensible o referencia local, estado previo y posterior, pasos, mensajes y resultado por campo. |
| V03 | CA01–CA03 | Revisión manual y decisión humana | V01 y V02 ejecutadas o con sus límites documentados | Consolidar la evidencia por criterio y presentar el resultado a DLG para aceptación o solicitud de ajustes. | Entorno, alcance, resultados, límites y hallazgos quedan registrados; la decisión humana explícita se anota sin afirmar cambios o comprobaciones no realizados. | Registro consolidado vinculado a las tareas y referencia real de la decisión de DLG. |

Estas comprobaciones no acreditan accesibilidad completa, conformidad visual global, scraping ni representación de imagen en páginas públicas. No se añade una suite automatizada: el entregable S9.6 es una revisión manual y las comprobaciones funcionales automatizadas conservan sus trabajos propietarios.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá preparación del entorno, revisión de creación y edición, consolidación y decisión humana.
- Cada criterio requiere evidencia satisfactoria o un límite explícito antes de la aceptación; ningún check depende de páginas públicas ni de etapas futuras.
- Ante un cambio de regla o una corrección solicitada, detener ese cambio y tramitarlo fuera de S9.6 antes de modificar fuentes o producto.
