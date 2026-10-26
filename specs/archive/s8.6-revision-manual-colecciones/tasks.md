# Tareas: S8.6 — Revisión manual de colecciones

## Referencias y autorización

- Estado: completada y aceptada por DLG el 26-X-2026 («Sí, acepto y cierro S8.6»).
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 25-X-2026 («Vamos»).
- Implementación:
  - Estado: completada el 26-X-2026; sin cambios de código atribuibles a S8.6.
  - Entorno: aplicación privada en `https://localhost:7263`, instancia local ya activa; base de desarrollo SQLite `data/database/linkubator-dev.db`.
  - Evidencia: creación y renombrado comprobados en interfaz; búsqueda básica y paginación observadas; confirmación, cancelación y borrado permitido comprobados. La prueba de eliminación de colección con enlaces no es factible en el entorno actual y queda enlazada como seguimiento de S9.
  - Límite: no se registraron navegador/versión ni capturas. La sesión de la aplicación se detuvo el 26-X-2026; no se ha limpiado el registro de prueba de la base local.
  - Consolidación documental: 26-X-2026.
- La aceptación de criterios y la aceptación formal del trabajo se registran por separado.

## Descomposición

La preparación del entorno y los datos antecede a los recorridos. Las comprobaciones funcionales se mantienen separadas por criterio para poder registrar límites concretos; la consolidación y aceptación depende de la evidencia anterior. No se implementarán arreglos dentro de este trabajo.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar disponibilidad del entorno privado, navegador y acceso; identificar datos de prueba seguros para creación, búsqueda/paginación y límites de borrado. Registrar estado previo sin secretos. | CA01–CA04; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | Ninguna | Abrir el listado y confirmar las rutas necesarias; registrar las condiciones que sí pueden prepararse y las limitaciones. Resultado: entorno y datos de prueba determinados sin afectar datos ajenos al alcance. | completada | El listado privado estuvo disponible en `https://localhost:7263` mediante una instancia existente. El puerto ocupado impidió iniciar otra instancia. La preparación segura de escenarios de borrado y la versión del navegador no quedaron registradas. |
| T02 | Ejecutar el recorrido manual de creación y cambio de nombre, y registrar el estado observado antes/después. | CA01; [plan → V01](plan.md#estrategia-de-validación) | T01 | Completar los formularios canónicos con datos preparados y comprobar el listado. Resultado: operación observada frente a AC-COL-01/AC-COL-02 o hallazgo reproducible. | completada | DLG confirma que se editó y guardó el nombre, y que el nuevo nombre aparece en el listado. La creación observada y persistida previamente también se mantiene como evidencia. |
| T03 | Ejecutar búsqueda y navegación entre páginas usando datos preparados; registrar coincidencias, orden y límites observables. | CA02; [plan → V02](plan.md#estrategia-de-validación) | T01 | Buscar y avanzar/retroceder en la interfaz. Resultado: comportamiento contrastado con fuentes o condiciones no comprobadas identificadas. | completada con aceptación de cobertura | DLG confirma que las pruebas básicas funcionan y acepta el comportamiento de paginación dinámica observado actualmente. No se preparó un conjunto amplio para paginar resultados filtrados; esta limitación queda registrada y no bloquea CA02 por decisión de DLG. |
| T04 | Revisar el flujo de confirmación y eliminar una colección que cumpla las condiciones, limitándose a datos preparados. | CA03; [plan → V03](plan.md#estrategia-de-validación) | T01 | Consultar la confirmación y comprobar que no cambia datos; confirmar luego el borrado permitido y volver al listado. Resultado: efecto antes/después documentado conforme a AC-COL-04 o hallazgo reproducible. | completada | DLG confirma que abrir el formulario no elimina la colección, cancelar conserva y muestra la colección en el listado, y confirmar el borrado hace que deje de aparecer. |
| T05 | Probar los rechazos de borrado para una colección con enlaces y para la última colección cuando puedan prepararse con seguridad. | CA04; [plan → V04](plan.md#estrategia-de-validación) | T01 | Intentar la operación canónica y comprobar estado/datos persistidos. Resultado: rechazo y conservación observados para cada caso preparado; lo no preparado queda registrado como no comprobado. | completada con límite documentado | La evidencia de [S8.7, T05 y CA04](../s8.7-revision-textos-accesibilidad-formularios/tasks.md#evidencia-por-criterio) confirma el rechazo al borrar la última colección. La eliminación de una colección con enlaces no es actualmente factible; queda como seguimiento de [S9: Gestión de enlaces](../../../plans/mvp0-plan.md), sin atribuirle una comprobación satisfactoria. |
| T06 | Consolidar evidencia por criterio, registrar hallazgos y límites de cobertura y actualizar el roadmap con estado y trazabilidad de la revisión. | CA05; [plan → V05](plan.md#estrategia-de-validación), [roadmap.md → «Next»](../../../context/roadmap.md) | T02, T03, T04, T05 | Verificar que cada CA tiene evidencia o límite explícito, entorno/fecha y pasos reproducibles; sincronizar el roadmap sin declarar aceptación. Resultado: resumen coherente y enlaces válidos, sin afirmar casos no ejecutados. | completada | Evidencia, cobertura aceptada y límite de CA04 consolidados en esta tabla, en «Evidencia por criterio» y en el roadmap, 26-X-2026. La aceptación formal del trabajo queda pendiente. |
| T07 | Revisar la evidencia y solicitar/registrar decisión humana explícita sobre el resultado. | CA05; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Presentar resultados y límites para aceptación o solicitud de ajustes. Resultado: decisión humana registrada; si se solicitan cambios, registrar bloqueo/seguimiento separado y no cerrar. | pendiente | Aceptación humana pendiente. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T02 | V01 en [plan.md](plan.md#estrategia-de-validación) | Completado: creación y edición guardada; el listado muestra el nuevo nombre tras guardar. | 25–26-X-2026; entorno local `https://localhost:7263`, base SQLite de desarrollo. | DLG confirma el recorrido de edición y listado; la creación anterior se observó en la interfaz y persistió en SQLite. Navegador/versión y capturas no registrados. |
| CA02 | T01, T03 | V02 en [plan.md](plan.md#estrategia-de-validación) | Aceptado por DLG: las pruebas básicas de búsqueda funcionan y la paginación dinámica funciona actualmente. No se generó un conjunto amplio para probar en detalle la paginación de resultados filtrados; DLG acepta el límite de cobertura. | 25–26-X-2026; entorno local `https://localhost:7263`; navegador y versión no registrados. | Observación inicial del listado y confirmación de DLG en esta revisión; sin registro de consultas, resultados por página ni capturas. |
| CA03 | T01, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Completado: el formulario de confirmación no borra; cancelar conserva la colección y confirmar la elimina del listado. | 26-X-2026; revisión manual de DLG en entorno local; navegador/versión no registrados. | Confirmación de DLG en esta revisión; no hay capturas. |
| CA04 | T01, T05 | V04 en [plan.md](plan.md#estrategia-de-validación) | Parcial con límite aceptado: el rechazo de la última colección está respaldado por la evidencia de S8.7; no se ha podido probar el rechazo de una colección con enlaces. | 26-X-2026; evidencia S8.7 reportada por DLG; navegador/versión no registrados. | [S8.7, CA04](../s8.7-revision-textos-accesibilidad-formularios/tasks.md#evidencia-por-criterio) cubre el rechazo de la última colección. El escenario con enlaces se enlaza a [S9: Gestión de enlaces](../../../plans/mvp0-plan.md) como seguimiento no ejecutable actualmente. |
| CA05 | T06–T07 | V05 en [plan.md](plan.md#estrategia-de-validación) | Consolidación completada; la aceptación humana formal de S8.6 sigue pendiente. | Consolidación documental: 26-X-2026. | Esta matriz, la descomposición y la referencia de S8.6 en [roadmap.md](../../../context/roadmap.md); falta decisión explícita de cierre. |

## Aceptación y cierre

- Estado de implementación: completada el 26-X-2026; T01–T06 completadas.
- Criterios: CA01 y CA03 completados; CA02 completado con la cobertura aceptada por DLG; CA04 cubierto para el rechazo de última colección, con el caso de colección con enlaces no ejecutable y enlazado como seguimiento de S9; CA05 consolidado.
- Limitaciones aceptadas/registradas: no se preparó una cantidad amplia de colecciones para verificar la paginación de resultados filtrados en detalle; no se probaron enlaces porque la funcionalidad necesaria aún no está disponible. No se registraron navegador/versión ni capturas.
- Aceptación humana: DLG, 26-X-2026 («Sí, acepto y cierro S8.6»).
- Archivado: completado el 26-X-2026.
- Ubicación archivada: `specs/archive/s8.6-revision-manual-colecciones/`.
- Fecha efectiva de archivado y comprobación de enlaces: 26-X-2026; carpeta activa ausente y enlaces entrantes/salientes comprobados.
- Siguiente paso: ninguno para S8.6; el caso de colección con enlaces permanece como seguimiento independiente de S9.
