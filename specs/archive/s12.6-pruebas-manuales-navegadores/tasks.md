# Tareas: S12.6 — Pruebas manuales en navegadores actuales

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada y aceptada con límites.
  - Inicio: 02-XI-2026, por solicitud de DLG en esta sesión.
  - Finalización: 02-XI-2026; T01–T07 completadas.
  - Evidencia consolidada: [registro de ejecución manual](#registro-de-ejecución-manual) y [evidencia por criterio](#evidencia-por-criterio).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 confirma el entorno seguro y los navegadores concretos; T02 prepara los escenarios aplicables. T03 y T04 ejecutan por separado la revisión de ambas áreas y sus categorías de superficie. T05 consolida la trazabilidad; T06 sincroniza la matriz y el roadmap con los resultados reales; T07 presenta la evidencia para aceptación humana. Ninguna tarea de ejecución empieza antes de aprobar estas tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar que la aplicación y las páginas necesarias están disponibles en un entorno local o de pruebas; identificar los datos aislados y los navegadores actuales seleccionados, registrando nombre y versión. | CA01–CA02; [plan.md → «Persistencia y dependencias»](plan.md#persistencia-y-dependencias), [«Decisiones técnicas y riesgos»](plan.md#decisiones-técnicas-y-riesgos) | Ninguna | Comprobar el acceso, la seguridad de los datos preparados y la identificación de cada navegador. Resultado: prerrequisitos listos o bloqueo concreto registrado, sin modificar datos ajenos a la revisión. | completada | Entorno local `https://localhost:7263`, datos aislados y acceso a `/` y `/app` confirmados en los tres navegadores; versiones en «Registro de ejecución manual». |
| T02 | Preparar el recorrido de escenarios aplicables para páginas públicas y privadas, formularios, listados, mensajes de error y estados vacíos, vinculando cada escenario a su fuente funcional. | CA01–CA02; [plan.md → «Flujos internos»](plan.md#flujos-internos), [V01](plan.md#estrategia-de-validación), [V02](plan.md#estrategia-de-validación) | T01 | Revisar que cada superficie aplicable esté representada o marcada como no disponible/no verificable, y que los datos permitan ejecutar el escenario. Resultado: recorrido trazable, sin escenarios supuestos como ejecutados. | completada | Recorrido y resultados por escenario registrados en «Registro de ejecución manual»; etiquetas excluidas por corresponder a MVP1. |
| T03 | Ejecutar manualmente en cada navegador seleccionado los escenarios públicos y privados preparados, incluyendo la comprobación de que el recorrido público no expone contenido privado. | CA01; [plan.md → V01](plan.md#estrategia-de-validación) | T02 | Comparar el comportamiento observado con las fuentes. Resultado: observaciones por área y navegador, con todo caso fallido o no verificable identificado. | completada | Páginas públicas y privadas recorridas en Edge, Brave y Chrome; los datos privados de prueba no aparecieron en páginas públicas. Detalle y límites en «Registro de ejecución manual». |
| T04 | Ejecutar manualmente en cada navegador seleccionado los escenarios aplicables de formularios, listados, errores y estados vacíos. | CA02; [plan.md → V02](plan.md#estrategia-de-validación) | T03 | Contrastar cada escenario con su fuente funcional. Resultado: todas las categorías aplicables tienen resultado registrado o su limitación queda explícita. | completada | Formularios, búsquedas, filtros, presentaciones, errores y estados vacíos representativos comprobados en los tres navegadores; límites registrados abajo. |
| T05 | Revisar la evidencia de T03 y T04 y consolidar resultados, hallazgos y límites por criterio, navegador y escenario. | CA01–CA03; [plan.md → V03](plan.md#estrategia-de-validación) | T03, T04 | Comprobar la trazabilidad y que no se declaran satisfactorias comprobaciones sin evidencia. Resultado: CA01–CA03 tienen evidencia localizable o un bloqueo explícito. | completada | Resultados identificados por escenario y navegador en «Registro de ejecución manual»; no se registran discrepancias en las observaciones reportadas. |
| T06 | Actualizar la entrada A12 de la matriz de checklist con el estado y la evidencia real; sincronizar el estado y los enlaces de S12.6 en el roadmap y el índice, sin registrar aceptación humana antes de obtenerla. | CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Validar que la matriz distingue resultados y limitaciones, y que roadmap e índice apuntan a los artefactos existentes con estado correcto. Resultado: referencias coherentes y sin afirmaciones de aceptación no registradas. | completada | A12–A14, roadmap e índice sincronizados con la evidencia de esta ejecución; T07 sigue pendiente de decisión humana. |
| T07 | Presentar la evidencia consolidada y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Revisar la evidencia y sus límites. Resultado: aceptación explícita registrada o bloqueo y acciones requeridas documentados; no cerrar por el mero hecho de ejecutar las pruebas. | completada | DLG aceptó la evidencia con los límites registrados el 02-XI-2026: «Acepto con los límites registrados». |

## Registro de ejecución manual

- Fecha: 02-XI-2026.
- Entorno: `https://localhost:7263`, con datos aislados de prueba.
- Navegadores: Microsoft Edge 156.0.431458; Brave 1.98.63; Google Chrome 156.0.8078.80.
- Resultado común: todas las observaciones siguientes fueron confirmadas en Edge, Brave y Chrome, salvo que se indique explícitamente un límite.

| Escenario | Observación y resultado |
| --- | --- |
| Inicio y privacidad públicos | `/` y `/Privacy` cargaron; la landing mostró las acciones «Login» y «Registro». |
| Dashboard y zona privada | `/app`, `/app/colecciones` y `/app/enlaces` cargaron. El dashboard mostró su bloque de enlaces recientes; el enlace rápido de prueba apareció como privado y con acciones disponibles. |
| Perfil público | Cargó sin nombre personal ni email; un elemento privado de prueba no apareció. |
| Colecciones públicas | El listado mostró al menos una colección pública y omitió la privada. En el detalle se vieron enlaces públicos y se ocultaron los privados. Las colecciones temporales públicas sin enlaces públicos tampoco aparecieron. |
| Búsqueda y filtros de colecciones | El listado reflejó los estados público/privado; una búsqueda distintiva devolvió la colección coincidente y al limpiar el filtro se restauró el listado. |
| Búsqueda, filtro y presentación de enlaces | La búsqueda por texto y el filtro de colección combinados devolvieron los resultados esperados; se conservaron los estados de privacidad. Alternar «Tarjetas»/«Lista» mantuvo resultados, orden y estados. |
| Alta y edición de colecciones | Se crearon colecciones privadas temporales, se editaron descripción y privacidad, y el listado reflejó los cambios. La colección vacía hecha pública no se publicó. |
| Error de colección duplicada | El nombre equivalente fue rechazado, se identificó la colección existente y no se creó un duplicado. |
| Alta, edición y movimiento de enlaces | Los enlaces temporales se crearon privados, conservaron URL y privacidad al editar y siguieron privados al moverlos a otras colecciones públicas. |
| Error de URL duplicada | El formulario rechazó cada URL duplicada, indicó la colección donde ya estaba y no creó copias. |
| Privacidad en formularios de enlace | Con colección privada, «Hacer público» estuvo desmarcado, deshabilitado y explicó el motivo. Al editar un enlace público apareció inicialmente desmarcado; los formularios se cancelaron sin guardar. |
| Alta rápida y búsqueda desde dashboard | La alta rápida creó enlaces privados y volvió al dashboard; la búsqueda llevó a `/app/enlaces` y encontró solo el enlace correspondiente. «Avanzado...» abrió el formulario completo con la URL precargada y sin crear el enlace. |
| Estados vacíos privados | Las búsquedas sin coincidencias en enlaces y colecciones mostraron sus mensajes de estado vacío, sin errores de página. |
| Errores | `/404`, `/Error` y `/app/404` mostraron sus páginas y enlaces correspondientes; el recurso privado no disponible no reveló su causa. |
| No exposición de pruebas privadas | Las colecciones/enlaces privados temporales y las altas rápidas privadas no aparecieron en el perfil ni en las colecciones públicas; el contenido público previo permaneció visible. |
| Limpieza | Se eliminaron los enlaces y colecciones temporales. Las búsquedas posteriores de los enlaces rápidos quedaron vacías y las demás colecciones permanecieron intactas. |

### Límites de la ejecución

- Las etiquetas se excluyeron a petición de DLG por estar reservadas a MVP1.
- No se comprobó el estado vacío del dashboard ni un perfil público sin contenido público, porque el entorno tenía datos para esas superficies; sí se comprobó el estado vacío de las búsquedas privadas y la omisión pública de colecciones sin enlaces públicos.
- No se comprobó el límite de paginación con más de 16 elementos ni navegadores distintos de los tres registrados. La revisión de accesibilidad continúa siendo la evidencia separada de S12.5.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T03, T05, T06 | V01 en [plan.md](plan.md#estrategia-de-validación) | Completado en Edge 156.0.431458, Brave 1.98.63 y Chrome 156.0.8078.80; páginas públicas y privadas recorridas, sin exposición observada de datos privados. | 02-XI-2026; entorno local `https://localhost:7263` | [Registro de ejecución manual](#registro-de-ejecución-manual) |
| CA02 | T01, T02, T04, T05, T06 | V02 en [plan.md](plan.md#estrategia-de-validación) | Completado con límites: formularios, listados, errores y estados vacíos representativos comprobados en los tres navegadores; límites en «Límites de la ejecución». | 02-XI-2026; entorno local `https://localhost:7263` | [Registro de ejecución manual](#registro-de-ejecución-manual) |
| CA03 | T05–T07 | V03 en [plan.md](plan.md#estrategia-de-validación) | Completado: cada resultado identifica escenario, observación y navegadores; hallazgos y límites no se presentan como satisfactorios. Aceptado con límites por DLG. | 02-XI-2026; Edge 156.0.431458, Brave 1.98.63 y Chrome 156.0.8078.80 | [Registro de ejecución manual](#registro-de-ejecución-manual) |

## Aceptación y cierre

- Estado del trabajo: implementación completada y aceptada con límites por DLG el 02-XI-2026; T01–T07 completadas.
- Criterios sin evidencia satisfactoria: ninguno; CA02 conserva los límites explícitos de la ejecución.
- Bloqueos y riesgos restantes: no hay bloqueos técnicos identificados; CA02 conserva los límites documentados en la ejecución.
- Aceptación humana: DLG aceptó con los límites registrados el 02-XI-2026 (referencia: «Acepto con los límites registrados»).
- Archivado: completado por @DaveSoftDev el 04-XI-2026 («Archiva las specs activas»).
- Ubicación archivada: `specs/archive/s12.6-pruebas-manuales-navegadores/`.
- Fecha efectiva de archivado y comprobación de enlaces: 04-XI-2026; carpeta, artefactos y enlaces entrantes/salientes comprobados.
- Siguiente paso: ninguno para S12.6; las limitaciones aceptadas no reabren la revisión.
