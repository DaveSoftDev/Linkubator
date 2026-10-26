# Plan técnico: S8.6 — Revisión manual de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 25-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Inspeccionar en navegador las páginas Web existentes de listado, creación, edición y eliminación de colecciones, sin modificar código ni datos fuera de los recorridos preparados para la revisión. Aplicar los resultados esperados de [spec.md](spec.md), [S8 del plan](../../../plans/mvp0-plan.md), [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md), [«Listados»](../../../context/specifications.md) y [«Rutas privadas»](../../../context/specifications.md).
- Las operaciones manuales cubren crear y renombrar una colección propia, buscar y cambiar de página en el listado, abrir la confirmación de eliminación y confirmar una eliminación permitida, y probar los rechazos por colección con enlaces y última colección. Las invariantes se contrastan con [domain-model.md → «Collection»](../../../context/domain-model.md) y [«Propiedad de los datos»](../../../context/domain-model.md); los resultados propietarios se enlazan desde cada criterio de [spec.md](spec.md).
- Para la búsqueda y paginación, preparar datos visibles suficientes para distinguir coincidencias, orden y resultados en varias páginas; el plan no introduce una cantidad funcional nueva. Registrar cualquier caso que no pueda observarse con los datos disponibles como no comprobado.
- Mantener la evidencia en el registro de tareas una vez que estas se aprueben. Por criterio anotar entorno y fecha, página y acción, estado relevante antes/después, resultado observado y hallazgos reproducibles. No guardar credenciales, tokens ni otros secretos. No crear una conclusión de accesibilidad o de revisión de textos: esos resultados corresponden a S8.7.

### Flujos internos

1. Confirmar que el entorno Web de revisión está disponible y que se puede acceder a la zona privada; registrar navegador y versión, entorno y configuración pertinente.
2. Preparar o identificar datos de prueba que permitan realizar las operaciones sin tocar datos no destinados a la revisión. Registrar el estado inicial relevante.
3. Crear una colección y editar el nombre de una colección propia; comprobar el resultado en la interfaz y en el listado.
4. Buscar colecciones y recorrer las páginas; comparar los resultados visibles con los datos preparados y las reglas fuente de búsqueda, orden y paginación.
5. Abrir la confirmación de eliminación y comprobar que la visita no elimina; confirmar después la eliminación de una colección elegible.
6. Intentar borrar una colección con enlaces y la última colección. Si alguna condición no puede prepararse de forma segura en el entorno, registrar el bloqueo y no simular evidencia.
7. Consolidar la evidencia por CA01–CA05. Documentar hallazgos con ruta, pasos y resultado observado; detener el recorrido afectado si revela pérdida de datos o una discrepancia funcional que exija intervención, y escalarla sin corregirla dentro de esta spec.

### Persistencia y dependencias

No se modifican la persistencia, las dependencias ni la infraestructura. Los recorridos escriben solo los datos de prueba expresamente preparados en el entorno de revisión; se registra su estado previo y el efecto de cada operación. La zona Web privada y las rutas implementadas son prerrequisitos de ejecución. S8.2 tiene implementación presente, pero conserva pendiente su cierre técnico según el roadmap, por lo que no se asume que sus operaciones ya estén aceptadas. S8.1–S8.5 aportan los criterios y flujos propietarios, pero su aceptación o cierre formal no es una dependencia de la evidencia propia de esta revisión. S8.7 y la revisión de textos, mensajes y accesibilidad tampoco bloquean su cierre.

### Garantías técnicas

- Cada conclusión se limita a las acciones observadas en el navegador y se enlaza a su criterio propietario; no se presenta la revisión manual como sustituto de pruebas automatizadas ni como auditoría global.
- Se distingue el resultado ejecutado de los casos no preparados o no comprobados, en especial los datos necesarios para observar paginación y restricciones de borrado.
- La confirmación de eliminación se comprueba antes de enviar la operación de escritura; toda operación que modifique datos se realiza solo sobre datos de prueba preparados y autorizados para el entorno.
- No se hacen cambios productivos. Los defectos se registran para el propietario correspondiente, sin considerarlos corregidos por el mero hecho de reportarlos.
- El cierre requiere evidencia por los criterios de la spec y decisión humana explícita; no depende de S8.7 ni del cierre global de S8.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar los recorridos sobre páginas Web reales en navegador y no sobre maquetas | Confirmado | [spec.md](spec.md) y S8.6 en [mvp0-plan.md](../../../plans/mvp0-plan.md) | Permite registrar el comportamiento observable de los flujos implementados; si no hay entorno disponible, registrar bloqueo sin sustituir la revisión por una inspección de código. |
| Navegador, versión, datos y procedimiento seguro para resetear el entorno | Pendiente parcial | Prerrequisitos de [spec.md](spec.md); T01 en [tasks.md](tasks.md) | Se observó la aplicación local en `https://localhost:7263` y la base de desarrollo; no se registraron navegador/versión ni un procedimiento de reset. Los recorridos de CA01–CA03 y las pruebas básicas de listado se han completado con evidencia reportada por DLG; el reset de datos no es necesario para la evidencia registrada. El caso CA04 de colección con enlaces queda como seguimiento no ejecutable hasta S9. |
| S8.2 mantiene pendiente el cierre técnico | Confirmado | [roadmap.md → Next](../../../context/roadmap.md) | Las discrepancias encontradas se documentan y escalan al trabajo propietario; no se consideran aceptación previa ni se implementan arreglos aquí. |
| La cantidad de datos para observar más de una página no está fijada por esta spec | Confirmado | [spec.md](spec.md), CA02 | Preparar el mínimo conjunto que permita comprobar las páginas del resultado sin convertirlo en una regla de producto; indicar cualquier límite de cobertura. |
| No incluir la revisión de textos, mensajes o accesibilidad | Confirmado | S8.7 en [mvp0-plan.md](../../../plans/mvp0-plan.md) y [spec.md](spec.md) | Evita solapar el entregable manual vecino; sus hallazgos quedan fuera de esta evidencia. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual, creación y cambio de nombre | Entorno Web y cuenta con colecciones de prueba; páginas canónicas disponibles | Crear una colección propia con datos de prueba y editar el nombre de una colección propia. Comprobar en el listado el estado resultante y contrastar con AC-COL-01 y AC-COL-02. | Las dos operaciones y sus resultados observables concuerdan con los criterios propietarios, o queda registrado un hallazgo reproducible. | Navegador/versión, entorno, datos previos no sensibles, pasos, resultado y referencia a criterios. |
| V02 | CA02 | Manual, búsqueda, orden y paginación | Datos de prueba suficientes para mostrar resultados de más de una página y una búsqueda distinguible | Buscar nombres preparados y recorrer páginas desde la interfaz; contrastar los elementos y su orden con las fuentes de listado. | Coincidencias, orden y páginas observadas son coherentes con las fuentes; se registran explícitamente los aspectos imposibles de comprobar. | Consulta realizada, conjunto de resultados esperado/observado, navegación, entorno y desviaciones. |
| V03 | CA03 | Manual, confirmación y borrado permitido | Colección propia preparada que cumpla las condiciones propietarias para eliminación | Abrir la ruta/página de confirmación por GET y verificar que el estado persiste; después confirmar la eliminación por el flujo de interfaz y volver al listado. | El GET no modifica la colección y la confirmación ejecuta el resultado previsto; cualquier diferencia se documenta. | Estado antes, pasos GET/confirmación, resultado posterior, entorno y hallazgos. |
| V04 | CA04 | Manual, límites de eliminación | Colección propia con enlaces y última colección disponibles, o bloqueo para preparar alguno de esos casos | Desde el flujo canónico, intentar eliminar cada caso restringido. Verificar la respuesta presentada y que las colecciones y relaciones permanecen sin cambios; no ejecutar una acción que pueda borrar datos reales. | Cada caso preparado se rechaza y conserva datos según sus criterios propietarios; los casos no preparados quedan como no comprobados, no como satisfactorios. | Caso preparado, estado antes/después, pasos, resultado, entorno y limitaciones. |
| V05 | CA05 | Revisión técnica de evidencia y aceptación humana | V01–V04 ejecutadas o con límites documentados | Comprobar trazabilidad para CA01–CA04, pasos reproducibles, registro de entorno y ausencia de afirmaciones sobre cambios o casos no ejecutados; someter el resultado a decisión humana explícita. | Evidencia consolidada y decisión de aceptación o solicitud de ajustes registrada. No demuestra condiciones que no se observaron. | Referencias a las evidencias, fecha, responsable, decisión explícita y referencia real. |

Las validaciones son exclusivamente manuales. No se requiere comando ni test automatizado para este trabajo. V02 depende de los datos disponibles; V04 no es satisfactorio para un caso que no pueda prepararse. La sensibilidad se preserva documentando como hallazgo cualquier diferencia observada frente a la fuente, y como pendiente cualquier comprobación no ejecutada; no se interpreta la ausencia de hallazgos como prueba de un caso omitido.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde su plantilla y someterlo a aprobación antes de ejecutar la revisión.
- La ejecución se limita a los datos de prueba preparados; si una acción amenaza datos ajenos al alcance, detenerla y registrar el bloqueo.
- S8.6 puede cerrarse cuando CA01–CA05 tengan evidencia suficiente o limitaciones explícitas aceptadas según el criterio, no queden bloqueos abiertos que invaliden la revisión y la aceptación humana esté registrada en `tasks.md`. Los hallazgos que requieran cambios productivos se mantienen como seguimiento independiente.
