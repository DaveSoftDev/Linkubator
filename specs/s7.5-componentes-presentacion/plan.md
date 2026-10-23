# Plan técnico: S7.5 — Componentes de presentación accesible

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 23-X-2026 («Siguiente»).
  - Plan: DLG, aprobado el 23-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Implementar en Web componentes Razor reutilizables para resumen de errores, avisos, estado vacío, paginación y formulario de búsqueda. Mantener los tipos de presentación en la capa Web; no introducir lógica de aplicación ni ampliar contratos entre capas.
- Los componentes reciben los datos y enlaces preparados por cada página: errores con su destino opcional, mensaje de aviso, contenido del estado vacío, estado y destinos de paginación, y ruta/campo/filtros de búsqueda. El componente representa esos valores y no los recalcula.
- Usar el mecanismo de validación de Razor Pages para mostrar el resumen común junto con los mensajes asociados a cada campo, respetando su estructura y los atributos accesibles existentes.
- El componente de aviso presenta el texto que la página le entrega. La página consumidora decide el mensaje y el momento de mostrarlo.
- El componente de estado vacío representa el caso sin elementos, separado del tratamiento de error.
- Los componentes de búsqueda y paginación generan enlaces y formularios a partir de la ruta y de los parámetros aplicables proporcionados por la página. No conocen ni analizan parámetros no suministrados.
- Reutilizar el layout y los estilos CSS/Tailwind existentes en S7.4; no agregar dependencias o patrones de interfaz globales sin necesidad.
- La página `PresentationDemo` y sus datos estáticos sirven solo como host de demostración y prueba de renderizado; no constituyen una página funcional de producto ni sustituyen la integración con páginas consumidoras.

### Flujos internos

1. Una página construye el modelo de presentación desde sus datos y resultados ya procesados.
2. Razor renderiza los componentes comunes con los datos recibidos, codificando valores de texto y atributos contextualmente.
3. El navegador expone la estructura semántica, los estados accesibles y los controles de formulario/enlace.
4. Las páginas consumidoras mantienen la responsabilidad de interpretar respuestas, ejecutar casos de uso, decidir mensajes y determinar el conjunto de parámetros aplicables.

### Persistencia y dependencias

No se modifica persistencia ni se añaden dependencias externas. Se reutilizan Razor Pages, el pipeline CSS existente y los recursos de accesibilidad del layout de S7.4. Los datos de listados, los parámetros interpretados y las respuestas HTTP siguen siendo responsabilidad de las etapas propietarias enlazadas en [spec.md](spec.md).

### Garantías técnicas

- Los mensajes se mantienen como texto codificado por Razor, no como HTML confiable aportado por la página.
- Los mensajes de error de campo continúan asociados a sus campos; el resumen complementa esa presentación y sus enlaces solo apuntan a destinos presentes.
- Los avisos, estados vacíos y controles de búsqueda/paginación conservan semántica HTML y nombres accesibles, y pueden operarse con teclado.
- La navegación de paginación y el formulario solo incluyen los parámetros que la página identifica como aplicables, conforme a [spec.md](spec.md) y las fuentes funcionales enlazadas allí.
- No se atribuye al componente la validación funcional de los parámetros, la búsqueda de datos, la autorización ni la decisión del código HTTP.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener los componentes de presentación como Razor reutilizable dentro de Web | Propuesta | [architecture.md → «Web»](../../context/architecture.md#web) y alcance de [spec.md](spec.md) | Reutiliza la tecnología existente; comprobar al iniciar el encaje con la estructura actual de páginas y parciales. |
| Entregar parámetros aplicables ya seleccionados por la página, sin que los componentes los analicen | Confirmado | Límites de esta spec y tareas S7.6–S7.7 de [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | Evita solapar componentes con el análisis y las decisiones de respuesta de las etapas vecinas. |
| Verificar el HTML renderizado con el harness existente, usando una página de demostración aislada sin presentarla como flujo funcional | Confirmado | [architecture.md → «Tecnologías»](../../context/architecture.md#tecnologías) y estrategia de S7.4 | La prueba integrada confirma renderizado y atributos seleccionados; no acredita una búsqueda, paginación de datos reales ni recorrido manual de teclado. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02, CA03, CA06 | Test automatizado o revisión focalizada del HTML renderizado con modelos representativos | Identificar harness Web y mecanismo de renderizado de Razor | Inspeccionar resumen con y sin destinos de campo, aviso y estado vacío; comprobar semántica y presencia de texto y destinos. Hacer una perturbación temporal de una aserción central y verificar que la validación detecta la regresión; restaurarla. | Estructura accesible y destinos presentes solo donde corresponden; test sensible al defecto introducido. | Test/comando, HTML o aserciones relevantes, perturbación revertida, resultado y entorno. |
| V02 | CA04, CA06 | Test automatizado de renderizado o revisión del HTML emitido | Datos representativos de página actual, páginas vecinas, búsqueda y filtros aplicables | Comprobar la navegación con teclado, indicación semántica de página actual y hrefs producidos; comprobar que los parámetros recibidos por el componente se preservan y los no suministrados no aparecen. | Navegación identificable y enlaces compuestos solo con valores preparados por la página. | Casos y hrefs inspeccionados, comando/test, fecha y resultado. |
| V03 | CA05, CA06 | Test automatizado de renderizado o revisión del formulario | Ruta de listado y filtros aplicables representativos | Inspeccionar método, destino, label asociado al campo, filtros conservados y ausencia de `p` en el formulario. Comprobar la operabilidad por teclado mediante el orden de controles expuesto. | Formulario GET con destino correcto, control etiquetado y parámetros esperados, sin estado de página. | HTML/aserciones, comando/test, resultado y alcance que no cubre la búsqueda real. |
| V04 | CA01–CA06 | Revisión técnica integrada | V01–V03 | Revisar codificación contextual, alcance Web, accesibilidad, dependencias y separación frente a S7.6–S7.9; ejecutar build Web, validación focalizada disponible y `git diff --check`. | Criterios con evidencia propia, componentes compilables y sin cambios de reglas ni dependencia de trabajo futuro. | Comandos, resultados, archivos revisados, fecha y límites. |

Los tests o revisiones del marcado no prueban la accesibilidad completa con lectores de pantalla ni la conducta de una búsqueda real; esas garantías no se atribuyen a esta spec. Las pruebas HTTP amplias de S7.9 y la revisión manual global de S7.10–S7.11 son seguimiento independiente y no bloquean el cierre de S7.5.

## Orden de ejecución y cierre

- [Tareas](tasks.md) aprobadas y ejecutadas para implementar y validar la base técnica de los componentes.
- El cierre de S7.5 acredita los componentes de presentación reutilizables y el alcance de evidencia consignado en `tasks.md`; no declara una página de producto completa ni una búsqueda o paginación funcional.
- Los límites de validación pendientes —incluidos recorrido manual de teclado y prueba negativa de sensibilidad— quedan declarados en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre), sin atribuirles resultados no ejecutados.
