# Plan técnico: S11.8 — Prueba manual de navegación pública

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 27-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- La persona revisora usará los formularios existentes para establecer y guardar los estados de las colecciones y los enlaces de prueba; después navegará las páginas públicas como visitante.
- Los datos de prueba deben permitir distinguir, sin ambigüedad, una colección pública, una privada, una colección pública sin enlaces públicos, y enlaces públicos y privados pertenecientes a una colección pública.
- La revisión observará la página pública del usuario, su página de colecciones, la página de una colección pública y las rutas de colección no accesibles públicamente. Contrasta cada observación con los criterios CA01–CA04 de [spec.md](spec.md) y sus fuentes.
- La revisión no modifica código ni define reglas nuevas. Si descubre una discrepancia, registra la observación y la remite al trabajo propietario; no la corrige dentro de S11.8.
- La evidencia se registrará en las tareas una vez aprobadas. Por criterio debe incluir fecha, entorno, navegador y versión, página o ruta, estado guardado, resultado esperado y observado, y hallazgos reproducibles. No incluirá credenciales, tokens ni datos personales innecesarios.

### Flujos internos

1. Confirmar que las páginas, rutas y formularios requeridos están disponibles. Identificar un entorno local o de pruebas y datos de prueba seguros bajo control de la persona revisora; registrar navegador y versión. Si los prerrequisitos no se cumplen, registrar el bloqueo sin alterar datos ajenos a la revisión.
2. Preparar o identificar una colección pública con al menos un enlace público y uno privado, una colección privada y una colección pública sin enlaces públicos. Usar valores distinguibles para reconocer las colecciones y enlaces durante la navegación.
3. En los formularios existentes, guardar los estados necesarios para CA01–CA02. Acceder como visitante a la página del usuario y a la página de colecciones; seguir el enlace de una colección que se publica y comprobar qué colecciones aparecen.
4. Abrir la colección pública desde la página pública y comprobar la presencia del enlace público y la ausencia del enlace privado de acuerdo con CA02.
5. Editar y guardar la colección pública como privada. Revisar en los formularios que los enlaces asociados hayan quedado privados y volver a la navegación pública para comprobar CA03.
6. Visitar las rutas de la colección privada y de la colección pública sin enlaces públicos. Comprobar el estado HTTP y que no se muestra contenido de esas colecciones, conforme a CA04.
7. Registrar el resultado de cada criterio, con observaciones diferenciadas de los resultados automatizados de S11.6 y de la inspección de HTML de S11.7. Comunicar las discrepancias al trabajo propietario, sin modificar comportamiento durante la prueba.

### Persistencia y dependencias

- No se prevén cambios de código, persistencia, consultas, transacciones ni dependencias de software.
- La ejecución depende de que estén disponibles las páginas públicas y las rutas de colección de S11.1–S11.5, además de los formularios que permiten guardar los estados cubiertos por los criterios. El [roadmap](../../context/roadmap.md#área-pública-y-seo) indica que S11.2 y S11.4–S11.5 siguen en curso; la revisión no se iniciará hasta que los prerrequisitos necesarios estén disponibles.
- La preparación de la revisión requiere datos seguros que cubran todos los estados observados. No se usarán datos de producción ni se inferirá el resultado de un escenario que no pueda reproducirse.
- S11.6 y S11.7 son validaciones separadas. Sus resultados no reemplazan la observación manual de S11.8 ni son prerrequisitos para cerrar sus criterios.

### Garantías técnicas

- La revisión cubre únicamente los estados, páginas y rutas que se observen en el entorno registrado; no demuestra la indexación efectiva por buscadores ni sustituye las pruebas HTTP automatizadas.
- En las rutas de colección no accesibles, comprobar tanto la respuesta HTTP como el contenido devuelto; una página visual con mensaje de error no basta para demostrar el estado `404` definido por la fuente.
- Los datos de prueba deben permitir detectar una filtración: un elemento privado debe poder reconocerse si aparece indebidamente y se inspeccionarán las páginas y respuestas pertinentes para confirmar que no aparece.
- Si no está disponible un escenario requerido, el criterio afectado queda sin evidencia satisfactoria. Toda discrepancia o resultado no comprobado se registra explícitamente.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Realizar la validación mediante navegación manual en un entorno local o de pruebas, sin automatizar navegador. | Propuesta | [spec.md → «Alcance»](spec.md#alcance), [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | Mantiene la validación en el trabajo humano previsto; confirmar entorno y navegador antes de ejecutar. |
| La ausencia de contenido privado debe comprobarse con datos que distingan los elementos públicos de los privados. | Confirmado | [spec.md → CA01–CA04](spec.md#criterios-de-aceptación), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Datos ambiguos impedirían concluir sobre filtraciones; preparar datos de prueba seguros antes del recorrido. |
| Una diferencia respecto de las fuentes no se corrige como parte de esta revisión. | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | Se comunica al trabajo propietario y se repite la comprobación afectada cuando la corrección autorizada esté disponible. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual: página pública del usuario y página de colecciones; visibilidad de colecciones públicas, privadas y sin enlaces públicos | Páginas disponibles y datos de prueba seguros con los estados requeridos | Recorrer ambas páginas como visitante e inspeccionar los nombres, enlaces y contenido de colecciones presentes | Solo aparecen las colecciones que cumplen las condiciones de publicación de sus fuentes; las colecciones privadas y las que no cumplen esas condiciones no se identifican ni exponen su contenido | Fecha, entorno, navegador/versión, rutas, estados de las colecciones, valores esperados y observados, resultado y hallazgos |
| V02 | CA02 | Manual: formulario de edición de enlace y navegación a una colección pública | Formulario disponible y colección pública con enlaces guardados en ambos estados | En edición, comprobar que el control empieza desmarcado y habilitado para una colección pública; establecer y guardar explícitamente el estado público o privado. Confirmar los estados guardados en el listado privado y abrir la colección desde la navegación pública para inspeccionar sus enlaces | El control desmarcado es el estado inicial, no una indicación del valor guardado. Tras guardar, el listado y la página pública reflejan los estados establecidos: aparece el enlace público y no el privado | Fecha, entorno, navegador/versión, colección, estado inicial y disponibilidad del control, estado guardado de cada enlace, contenido público esperado/observado, resultado y hallazgos |
| V03 | CA03 | Manual: edición de colección, comprobación posterior de enlaces y navegación pública | Formulario disponible y colección pública con enlaces asociados | Guardar la colección como privada; revisar el estado de cada enlace en edición y visitar su ruta pública | Todos los enlaces quedan privados; la colección deja de aparecer en la navegación pública y no se expone contenido privado | Fecha, entorno, navegador/versión, colección y enlaces de prueba, estados antes/después, rutas consultadas, resultado y hallazgos |
| V04 | CA04 | Manual negativa: rutas de colección privada y colección pública sin enlaces públicos | Rutas y datos de prueba disponibles; navegador permite inspeccionar el estado HTTP | Solicitar directamente ambas rutas y revisar el estado HTTP y la respuesta, además de recorrer las páginas públicas disponibles | Cada ruta responde `404`, no expone contenido privado y no distingue esos casos de una colección inexistente o no accesible públicamente | Fecha, entorno, navegador/versión, rutas, estados HTTP, contenido observado, resultado y hallazgos |

La validación es manual: no se atribuirá a una compilación ni a S11.6 la comprobación de estados, visibilidad o respuestas que requieren estos recorridos. V04 es la comprobación negativa de ausencia de contenido; una respuesta `404` sin inspeccionar el contenido devuelto no basta para concluir que no hubo filtración.

## Orden de ejecución y cierre

- [Tareas](tasks.md) — aprobadas por DLG el 27-X-2026 con la referencia «Vamos». Ordenan la confirmación de prerrequisitos, los recorridos V01–V04, la consolidación de evidencias y la revisión humana final.
- Registrar por separado el resultado de cada criterio. Un entorno o escenario que no permita comprobar un criterio se registrará como bloqueo o evidencia pendiente, nunca como éxito supuesto.
- El cierre exige evidencia satisfactoria para CA01–CA04, tareas propias completadas y aceptación humana registrada en las tareas. La aceptación de esta especificación o del plan no constituye aceptación del resultado.
- Si la revisión identifica una regla contradictoria o ausente, detener el criterio afectado y tramitar primero el cambio en su documento propietario.
