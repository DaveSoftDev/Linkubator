# Plan técnico: S9.5 — Revisión manual de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Aprobar spec y generar plan.md como propuesta»).
  - Plan: aprobado por DLG el 26-X-2026 («Aprobar plan y preparar tasks.md como borrador»).

## Diseño de implementación

### Componentes y contratos

- Revisar en navegador los flujos Web ya implementados de creación de S9.1 y edición integral de S9.3, sin modificar código ni reglas. La navegación, duplicidad y privacidad se contrastan con los criterios de [spec.md](spec.md), [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace) y sus fuentes propietarias.
- Preparar datos de prueba únicamente en una cuenta y colecciones propias destinadas a la revisión. Para probar la información de la colección de un duplicado, el enlace existente y el intento duplicado se preparan en colecciones propias distintas.
- La evidencia por criterio registrará entorno, navegador y versión, URL probada sin datos sensibles, pasos, estado relevante antes y después, respuesta visible y resultado. Los hallazgos incluirán ubicación y pasos reproducibles. No se guardan credenciales, tokens ni otros secretos.
- La revisión se limita a los campos y estados visibles en los flujos previstos. No inspecciona ni evalúa la presentación de metadatos, scraping o páginas públicas, que pertenecen a otros trabajos.

### Flujos internos

1. Confirmar que la aplicación Web y el acceso autenticado están disponibles. Registrar navegador, versión, entorno y colecciones de prueba a utilizar.
2. Escoger una URL pública real que cumpla las reglas aprobadas, comprobar que está disponible desde el entorno y usarla en la creación. Registrar la URL introducida y la guardada conforme al ajuste aprobado; abrir la navegación del enlace y registrar el resultado, incluyendo redirecciones observadas sin tratarlas como una sustitución de la URL guardada.
3. Desde otra colección propia, intentar crear un enlace con una URL que resulte duplicada conforme a la regla propietaria. Comprobar que no se crea un segundo enlace y que se señala la colección que contiene el existente.
4. Editar un enlace propio en una colección pública. Guardar primero el estado privado y después el público, reabriendo el formulario tras cada operación para comprobar el estado persistido y que la URL permanece igual.
5. Intentar hacer público un enlace que conserva su colección privada. Comprobar el rechazo, el mensaje previsto y que los campos del enlace permanecen sin cambios.
6. Consolidar los resultados por CA01–CA04, distinguir observaciones de casos no comprobados y presentar la evidencia para decisión humana. No corregir hallazgos dentro de esta revisión.

### Persistencia y dependencias

No se modifican la infraestructura, dependencias ni contratos. Las operaciones de revisión sí crean y actualizan datos de prueba propios; se registran sus valores relevantes antes y después. Solo se eliminan datos de prueba cuando la persona responsable lo autorice expresamente y después de conservar la evidencia necesaria.

S9.1 y S9.3 aportan los flujos Web que se revisan y tienen implementación y aceptación registradas. S9.4 está completada, pero sus pruebas automatizadas no son prerrequisito para ejecutar o cerrar esta revisión. Se requiere un navegador y conectividad con la URL pública elegida; si el entorno o el destino no están disponibles, se registra el criterio afectado como no comprobado, sin simular su resultado.

### Garantías técnicas

- Las conclusiones se limitan al comportamiento observado manualmente y a los criterios propios de S9.5; no sustituyen las pruebas automatizadas ni declaran cubiertas las páginas públicas.
- La navegación se inicia a la URL guardada; una redirección externa no implica que Linkubator haya reemplazado la URL original.
- Los cambios se hacen exclusivamente sobre enlaces propios preparados para la revisión. No se usan datos ajenos ni se registran secretos.
- Ante discrepancias, se documenta el resultado y se detiene el cambio afectado. No se implementan correcciones ni se modifica la regla propietaria dentro de esta revisión.
- El cierre requiere evidencia propia para CA01–CA04 y aceptación humana; no depende de S9.6 ni de etapas posteriores.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Inspeccionar los flujos Web reales en navegador, sin cambiar producto | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | La evidencia será del comportamiento observable; los defectos se registran para seguimiento independiente. |
| Usar colecciones propias distintas para verificar que el duplicado informa dónde existe | Propuesta | [CA02](spec.md#criterios-de-aceptación) y [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace) | Permite observar la colección informada sin introducir reglas nuevas. Confirmar que los datos disponibles permiten ejecutar el recorrido. |
| Seleccionar el navegador, entorno, destino público y formato de evidencia | Pendiente | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Concretar antes de ejecutar las tareas; si no hay destino público accesible, registrar el límite y no afirmar CA01 comprobado. |
| Los destinos externos pueden redirigir o dejar de estar disponibles | Riesgo | [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces) | Registrar la URL y el resultado observado; no inferir que Linkubator sustituyó la URL ni atribuir indisponibilidad externa al flujo de guardado. |
| No implementar arreglos durante S9.5 | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | Los hallazgos que requieran cambios se tramitan como trabajo separado. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual, creación y navegación a una URL real | Aplicación Web disponible, usuario identificado, colección propia y URL pública accesible | Crear el enlace por la interfaz y activar su navegación. Comparar la URL guardada con el resultado de la regla fuente y registrar el destino y las redirecciones observadas. | La creación respeta el criterio propietario y la acción navega usando la URL guardada; cualquier indisponibilidad o resultado inesperado queda registrado, sin atribuirlo a un cambio de URL que no se haya observado. | Entorno, navegador/versión, URL sin datos sensibles, pasos, URL guardada, destino observado y resultado. |
| V02 | CA02 | Manual, rechazo de duplicado | V01 completada o enlace propio ya existente con URL adecuada; segunda colección propia disponible | Intentar crear en la segunda colección un enlace cuya URL resulte duplicada según la regla aprobada; comprobar la respuesta y revisar que no exista una segunda creación. | Se rechaza el duplicado, se indica la colección del enlace existente y no se crea otro enlace. | Estado inicial, colecciones de prueba, URL usada, respuesta visible, comprobación de ausencia de segundo enlace y resultado. |
| V03 | CA03 | Manual, edición de ambos estados permitidos | Enlace propio en colección pública y edición Web disponible | Guardar el enlace como privado y después como público desde el checkbox. Reabrir el formulario después de cada guardado y comparar estado y URL. | El estado persistido coincide en cada paso con el seleccionado y la URL no cambia; cualquier discrepancia queda registrada. | Estado anterior y posterior, pasos, URL comparada, respuesta visible y resultado por cada guardado. |
| V04 | CA04 | Manual, comprobación negativa de privacidad y ausencia de cambios | Enlace propio en colección privada y estado/campos iniciales anotados | Intentar marcarlo público desde la edición; comprobar rechazo, aviso y estado/campos al reabrir el formulario. | Se observa el rechazo previsto, el mensaje aprobado y ningún cambio en los campos del enlace. | Estado y campos iniciales y posteriores, pasos, mensaje, entorno y resultado. |
| V05 | CA01–CA04 | Revisión técnica y aceptación humana | V01–V04 ejecutadas o límites documentados | Revisar que cada criterio dispone de evidencia real o de una limitación explícita, que los hallazgos son reproducibles y que no se atribuyen arreglos. Presentar el resultado para aceptación. | La decisión humana queda registrada explícitamente; los criterios no comprobados ni los hallazgos abiertos se mantienen visibles. | Referencias a V01–V04, fecha, responsable, decisión y referencia real. |

Estas validaciones son manuales y no amplían la cobertura de la batería HTTP de S9.4. Una redirección no demuestra sustitución de la URL persistida; la navegación solo comprueba el destino observado en esa ejecución. Si la URL externa no está disponible, V01 no demuestra que Linkubator haya fallado al guardar o navegar. Los casos negativos de duplicidad y privacidad deben registrarse como resultados observados, no deducirse de la ausencia de un error en la interfaz.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde su plantilla y someterlo a aprobación antes de iniciar la revisión.
- El cierre requiere evidencia satisfactoria o límites explícitos para CA01–CA04, ausencia de cambios productivos atribuibles a S9.5 y aceptación humana registrada en las tareas.
- Si una discrepancia exige cambiar una regla funcional, detener el recorrido afectado y tramitar la decisión en su documento propietario antes de continuar.
