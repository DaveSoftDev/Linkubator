# Tareas: S7.11 — Validación del estilo base

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 24-X-2026 («Siguiente»).
  - Tareas: DLG, aprobadas el 24-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: 24-X-2026; DLG («Inicia la implementación»).
  - Finalización: 24-X-2026; revalidación de `/app` y `/app/presentacion`, comprobación de foco, contraste representativo, viewport estrecho emulado, decisión humana sobre el solapamiento residual, aceptación del cierre de la revisión y actualización del roadmap.
  - Evidencia consolidada: [T01–T05 y V01–V04](#descomposición); resultado de revisión y decisión humana incluidos en «Evidencia por criterio» y «Aceptación y cierre».

## Descomposición

La inspección manual de las vistas/estados seleccionados requiere primero un inventario de cobertura; la consolidación y decisión dependen de que se registren tanto ese inventario como los resultados de la revisión. No se incluyen cambios productivos ni se bloquea el cierre por la implementación de ajustes solicitados.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Identificar el host Web, el entorno manual y las páginas, componentes y estados existentes que se podrán revisar; registrar los ausentes como límites. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V01](plan.md#estrategia-de-validación) | Ninguna | Confirmar disponibilidad de las vistas y dejar una matriz de cobertura que distinga lo inspeccionable de lo ausente. | completada | 24-X-2026: host local HTTPS disponible en `https://localhost:7263`; `/app` y `/app/presentacion` respondieron y se inspeccionaron visualmente. La segunda es una página de demostración de componentes, no un flujo funcional. En ella están disponibles el resumen de errores, aviso, estado vacío, paginación y búsqueda; `/app` muestra el dashboard y el estado vacío real. No hay formularios funcionales ni listados de producto para revisar en el estado actual. |
| T02 | Inspeccionar la presentación visual de las vistas y estados disponibles frente a las expectativas aplicables; registrar hallazgos reproducibles o su ausencia. | CA02; [plan.md → «Flujos internos»](plan.md#flujos-internos), [V02](plan.md#estrategia-de-validación) | T01 | Revisar cada elemento seleccionado y dejar observaciones con ubicación, estado y método; no registrar como revisado lo que no esté disponible. | completada | Revalidación 24-X-2026, host local HTTPS, Development, navegador integrado VS Code/Chromium 150.0.7871.250, viewport amplio efectivo 1067×667 CSS px y estrecho limitado a 250×375. En viewport estrecho los formularios de alta y búsqueda se apilan y quedan dentro de la tarjeta; el footer se oculta bajo 900px. En ancho amplio, al final del scroll de `/app/presentacion`, la búsqueda termina en y=594 y el footer empieza en y=598 (sin solapamiento observado); en `/app` el estado vacío termina en y=611 y el footer empieza en y=598: queda solapamiento de 13px en el dashboard. El viewport estrecho permanece limitado por la escala del host, aunque el formulario se ve contenido a 250px efectivos. Comprobación de foco: enlace del resumen de errores mostró outline sólido de 2.67px, blanco al 80%, offset 2px. Contraste representativo recalculado sobre fondos opacos: resumen de error 16.54:1, enlace de error 14.76:1, aviso 15.35:1, texto de estado vacío 15.38:1, botón 15.57:1, página actual 8.81:1 y pie 16.20:1; los valores medidos superan 4.5:1. No se midieron todos los estados ni bordes no textuales. |
| T03 | Consolidar la evaluación y registrar la decisión de solicitar o no ajustes, con cada solicitud descrita por separado. | CA03; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [V03](plan.md#estrategia-de-validación) | T02 | Verificar que la decisión y las solicitudes, si las hay, son identificables y que no se presentan como implementadas. | completada | 24-X-2026: consolidada la nueva revisión. Los formularios se apilan y caben en el viewport estrecho emulado; el footer se oculta en el breakpoint móvil. En `/app` a 1067×667 CSS px el estado vacío termina 13px por debajo del inicio del footer fijo al final del scroll. DLG indica que considera aceptable ese solapamiento («Lo considero aceptable»); no se solicita ajuste por este hallazgo. La emulación estrecha sigue limitada por el host. |
| T04 | Presentar evidencia y registrar decisión humana explícita: aceptar el estilo como está o aceptar la revisión solicitando ajustes. | CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [V04](plan.md#estrategia-de-validación) | T03 | Confirmar que hay una decisión explícita sobre el resultado de revisión, con responsable, fecha y referencia. | completada | DLG, 24-X-2026: acepta dar la revisión por finalizada a falta de pulir flecos de diseño («Pues por mi lo daría por finalizado a falta de pulir flecos de diseño»). La frase se registra como aceptación del cierre de S7.11 con los ajustes visuales pendientes como seguimiento separado; no equivale a afirmar que se implementaron todos los ajustes solicitados ni a cerrar la etapa S7. |
| T05 | Actualizar el roadmap con el estado real de la revisión y enlazar su evidencia y decisión, sin declarar aceptación del estilo base. | CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Confirmar que el roadmap refleja fielmente la decisión y el seguimiento pendiente, sin declarar cerrado S7.11 ni la etapa S7. | completada | 24-X-2026: roadmap actualizado con la revisión realizada, ajustes solicitados, aceptación del estilo pendiente y enlace a esta evidencia. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T04 | V01 y V04 de [plan.md](plan.md#estrategia-de-validación) | Inventario de páginas, componentes y estados disponibles registrado; las limitaciones de cobertura están documentadas. | 24-X-2026; navegador integrado de VS Code, Windows, Development | T01 y decisión registrada en T04 |
| CA02 | T02, T04 | V02 y V04 de [plan.md](plan.md#estrategia-de-validación) | Revalidación manual y contraste representativo realizados; las combinaciones de texto medidas superan 4.5:1. Los formularios se apilan dentro del ancho estrecho emulado; en el dashboard a viewport amplio se observó un solapamiento de 13px entre el estado vacío y el footer fijo al final del scroll, que DLG considera aceptable. | 24-X-2026; Chromium 150.0.7871.250, Windows, Development; viewport amplio 1067×667 CSS px, estrecho efectivo 250×375 | T02 y decisión DLG registrada en T04 |
| CA03 | T03–T04 | V03 y V04 de [plan.md](plan.md#estrategia-de-validación) | Formularios apilados dentro del viewport estrecho emulado; el footer se oculta bajo el breakpoint observado. El solapamiento residual de 13px fue considerado aceptable por DLG y no genera una solicitud de ajuste adicional. La emulación estrecha real no pudo confirmarse por limitación del host. | 24-X-2026; entorno de T02 | T03–T04 |
| CA04 | T04–T05 | V04 de [plan.md](plan.md#estrategia-de-validación) | DLG aceptó dar la revisión por finalizada con los flecos de diseño pendientes («Pues por mi lo daría por finalizado a falta de pulir flecos de diseño»). La decisión sobre el solapamiento puntual también está registrada como aceptable («Lo considero aceptable»). | 24-X-2026; revisión documentada en T01–T04 | T04 |

## Aceptación y cierre

- Estado del trabajo: aceptado y completado como revisión; los flecos de diseño solicitados siguen como seguimiento independiente.
- Criterios sin evidencia satisfactoria: ninguno dentro del alcance de esta revisión. El viewport móvil real no se comprobó; la emulación estrecha queda registrada como limitación, no como cobertura real.
- Bloqueos y riesgos restantes: la confirmación en dispositivo/viewport móvil real y la implementación de flecos visuales pertenecen a seguimiento separado y no reabren esta revisión aceptada.
- Aceptación humana: DLG, 24-X-2026, «Pues por mi lo daría por finalizado a falta de pulir flecos de diseño». El solapamiento concreto se considera aceptable por DLG («Lo considero aceptable»); los flecos restantes no se consideran implementados por esta aceptación.
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: dar seguimiento separado a los flecos de diseño pendientes y, si se realizan cambios, revisarlos en su trabajo propietario. Esta aceptación no cierra la etapa S7.
