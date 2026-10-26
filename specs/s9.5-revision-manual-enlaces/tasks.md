# Tareas: S9.5 — Revisión manual de enlaces

## Referencias y autorización

- Estado: aprobadas por DLG el 26-X-2026 («Vamos»).
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Aprobar spec y generar plan.md como propuesta»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 26-X-2026 («Aprobar plan y preparar tasks.md como borrador»).
  - Tareas: aprobadas por DLG el 26-X-2026 («Vamos»).
- Implementación:
  - Estado: completada y aceptada por DLG.
  - Inicio: 26-X-2026; se inicia T01 con la comprobación del entorno y los datos de prueba propios.
  - Finalización: 26-X-2026; revisión manual CA01–CA04 y cierre aceptados por DLG («Aceptar y cerrar»).
  - Evidencia consolidada: registrada en este documento.
- La preparación de estas tareas no inicia la revisión ni inventa evidencia de ejecución; la ejecución real comienza con T01 y sigue el flujo comprobado en este documento.

## Descomposición

T01 confirma el entorno y los datos de prueba propios. T02 crea y prueba la navegación con una URL real; T03 usa ese enlace para comprobar el rechazo de un duplicado. T04 comprueba los estados de edición y el rechazo de publicación en una colección privada. T05 consolida la evidencia y actualiza la trazabilidad; T06 presenta el resultado para aceptación humana. Las comprobaciones no ejecutadas siguen pendientes y no se implementan arreglos dentro de S9.5.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar disponibilidad de la aplicación, usuario y navegador; identificar colecciones y enlaces propios de prueba para la creación, el duplicado y la edición de estados. Registrar el estado inicial sin secretos. | CA01–CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias). | Ninguna | La aplicación y la sesión privada están disponibles; se identifican colecciones propias adecuadas y datos de prueba sin modificar recursos no destinados a la revisión. Si falta una condición, se registra el bloqueo concreto. | completada | Entorno local en el equipo de DLG. Navegador: Chrome 155.0.8059.85, Build oficial, 64 bits. DLG confirma que el listado contiene únicamente los enlaces creados para las pruebas. |
| T02 | Crear un enlace en una colección pública propia con una URL real accesible y activar su navegación; registrar URL guardada y resultado observado. | CA01; [plan.md → V01](plan.md#estrategia-de-validación), [«Flujos internos»](plan.md#flujos-internos). | T01 | La creación y la acción de navegación se observan y contrastan con la regla propietaria; las redirecciones se registran sin tratarlas como sustitución de la URL guardada. | completada | DLG informa que la creación con URL válida se guardó, localizó el enlace en el dashboard y lo abrió al destino esperado. La URL concreta y el destino no se consignaron. |
| T03 | Intentar crear en otra colección propia un enlace duplicado del creado en T02; verificar la respuesta y la ausencia de un segundo enlace. | CA02; [plan.md → V02](plan.md#estrategia-de-validación). | T02 | El intento se rechaza según la fuente, se indica la colección del enlace existente y no aparece una segunda creación; cualquier diferencia queda documentada. | completada | DLG informa que la URL existente se rechaza en el servidor, se indica la colección donde está y en el listado solo aparecen los enlaces creados, sin un duplicado adicional. |
| T04 | Desde la edición, guardar como privado y como público un enlace propio de una colección pública; luego intentar hacer público un enlace propio de una colección privada y comprobar su estado/campos tras el rechazo. | CA03–CA04; [plan.md → V03 y V04](plan.md#estrategia-de-validación). | T01, T02 | Los dos estados permitidos persisten y la URL original no cambia; la publicación en colección privada se rechaza con el mensaje previsto y sin cambios parciales. | completada | DLG informa que edición, cancelar y cambios de privacidad permitidos funcionan; una colección privada deja sus enlaces privados, y al intentar marcar público se muestra el mensaje de que la colección debe ser pública. DLG confirma que no se guardó ningún cambio del intento rechazado y que la URL continúa siendo la original. |
| T05 | Consolidar los resultados por criterio, incluyendo entorno, fecha, pasos, evidencia, hallazgos y límites; actualizar la referencia de S9.5 en el roadmap con el estado real. | CA01–CA04; [plan.md → V05](plan.md#estrategia-de-validación), [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T03, T04 | Cada criterio tiene evidencia real satisfactoria o una limitación/bloqueo explícito; la documentación no presenta resultados esperados como ejecutados ni registra arreglos no realizados. | completada | Resultados aportados por DLG consolidados por criterio en esta tabla. Se explicita que no se registraron las URL concretas de prueba ni de destino; no se han implementado cambios de producto. |
| T06 | Presentar la evidencia consolidada a DLG y registrar la decisión explícita de aceptación o los cambios solicitados. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T05 | La decisión humana queda registrada con responsable, fecha y referencia real. Si se solicitan cambios, el trabajo permanece abierto y estos se tramitan por separado. | completada | DLG acepta y cierra S9.5 el 26-X-2026 («Aceptar y cerrar»). |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T02, T05 | V01 y V05 en [plan.md](plan.md#estrategia-de-validación) | Resultado funcional informado: creación válida, enlace localizado en el dashboard y navegación al destino esperado. Evidencia descriptiva incompleta: no consta la URL concreta ni el destino observado. | 26-X-2026; entorno local; Chrome 155.0.8059.85 (64 bits) | Informe de DLG en esta conversación |
| CA02 | T01–T03, T05 | V02 y V05 en [plan.md](plan.md#estrategia-de-validación) | Resultado funcional informado: el servidor rechaza el duplicado, indica la colección existente y no aparece un enlace duplicado en el listado. | 26-X-2026; entorno local; Chrome 155.0.8059.85 (64 bits) | Informe de DLG en esta conversación |
| CA03 | T01–T02, T04–T05 | V03 y V05 en [plan.md](plan.md#estrategia-de-validación) | Resultado funcional informado: los cambios permitidos de privacidad persisten, la URL conserva su valor original y hacer privada una colección deja privados sus enlaces. | 26-X-2026; entorno local; Chrome 155.0.8059.85 (64 bits) | Informe de DLG en esta conversación |
| CA04 | T01, T04–T05 | V04 y V05 en [plan.md](plan.md#estrategia-de-validación) | Resultado funcional informado: el intento de publicar un enlace de colección privada se rechaza con el mensaje de que la colección debe ser pública y no guarda ningún cambio. | 26-X-2026; entorno local; Chrome 155.0.8059.85 (64 bits) | Informe de DLG en esta conversación |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG el 26-X-2026 («Aceptar y cerrar»); T01–T06 completadas.
- Criterios sin evidencia satisfactoria: ninguno informado. CA01–CA04 tienen resultados funcionales reportados; no se capturaron las URL concretas usadas.
- Bloqueos y riesgos restantes: ninguno para las comprobaciones funcionales ejecutadas. No se implementan correcciones dentro de S9.5.
- Aceptación humana: DLG, 26-X-2026 («Aceptar y cerrar»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso: ninguno; revisión cerrada. El archivado no fue solicitado.
