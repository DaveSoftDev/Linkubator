# Plan técnico: S8.7 — Revisión de textos, mensajes y accesibilidad de formularios

## Estado y aprobación del plan

- Estado: aprobado; el trabajo asociado está completado, aceptado y archivado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 26-X-2026 («Siguiente»).
- Cierre del trabajo: tareas completadas y resultado aceptado por DLG el 26-X-2026 («Completado»); evidencia y límites en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).

## Diseño de implementación

### Componentes y contratos

- El trabajo consiste en una revisión manual de las páginas Web privadas de crear, editar y confirmar la eliminación de colecciones; no añade ni cambia componentes, rutas o contratos productivos.
- Se contrastan los textos y mensajes renderizados con las fuentes propietarias y con los resultados observables de S8.2–S8.4. La revisión no cambia mensajes ni los convierte en una nueva fuente de reglas.
- El resultado se registra como evidencia de revisión para cada criterio en las tareas, indicando página/estado, contexto de ejecución, comprobación y resultado. Cualquier discrepancia se describe como hallazgo, sin corregir código como parte de esta spec.

### Flujos internos

1. Confirmar que el entorno Web de revisión está disponible y que la identidad de desarrollo permite abrir las páginas privadas.
2. Preparar o localizar colecciones de prueba que permitan abrir creación, edición, confirmación de eliminación y al menos una respuesta de rechazo de dominio sin alterar datos ajenos.
3. Recorrer los formularios en estado normal y en los estados de error disponibles; leer las etiquetas, instrucciones, acciones y mensajes y compararlos con sus fuentes.
4. Repetir la interacción relevante usando teclado: orden de tabulación, activación, foco visible, foco tras errores y posibilidad de volver/cancelar sin confirmar una operación accidentalmente.
5. Inspeccionar la estructura semántica y los nombres accesibles de los controles y mensajes mediante las herramientas disponibles del navegador. No se afirmará cobertura de lector de pantalla, contraste medido ni compatibilidad entre navegadores si no se ejecutaron esas comprobaciones.
6. Registrar el resultado por criterio y los límites de la revisión. Si aparece una discrepancia funcional o una barrera de accesibilidad, dejar el criterio como no satisfactorio y proponer su seguimiento en el trabajo propietario; no cambiar código ni fuentes funcionales bajo este plan.

### Persistencia y dependencias

- No se requieren cambios de persistencia ni transacciones. La interacción se limita a la identidad y datos de prueba disponibles.
- Prerrequisitos: entorno Web utilizable; datos que permitan inspeccionar los flujos; y estados de error relevantes accesibles sin alterar datos ajenos. Si no es seguro o posible generar un estado, se registra como no comprobado y se detiene la comprobación afectada.
- S8.2, S8.3 y S8.4 son propietarios de los flujos y componentes observados; su cierre técnico no se presupone ni es requisito para registrar la revisión S8.7.

### Garantías técnicas

- No se presentan credenciales, datos personales reales ni información de otros usuarios en la evidencia; se usan datos de desarrollo apropiados.
- La prueba de eliminación se limita a abrir la confirmación y a observar los controles. Solo se ejecuta una eliminación si se dispone de una colección de prueba prescindible y se necesita observar su respuesta; nunca se elimina la última colección ni una colección con datos que deban conservarse.
- La navegación por teclado y la lectura del HTML/árbol de accesibilidad no demuestran por sí solas compatibilidad integral con lectores de pantalla, mediciones WCAG ni pruebas en varios navegadores; esos límites se consignan en la evidencia.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar la revisión como comprobación manual en el host Web existente, sin modificar código. | confirmado | [spec.md → «Alcance»](spec.md#alcance) | Mantiene el trabajo dentro de S8.7; registrar el navegador y entorno realmente usados. |
| No considerar una observación no ejecutada como evidencia satisfactoria, en particular para tecnología asistiva, contraste medido o navegadores adicionales. | confirmado | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) y [requirements.md → «Accesibilidad»](../../../context/requirements.md) | Los límites deben quedar explícitos; ampliar el alcance requiere aprobación y no se presume. |
| Requiere un método seguro de obtener estados de validación y rechazo de dominio en el entorno disponible. | pendiente | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Confirmar cómo preparar los datos de prueba antes de empezar; no sustituir la falta de estado por cambios de reglas o datos ajenos. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual: texto, propósito de campos/acciones y relación label-control en creación y edición, en estado normal. | Host Web accesible e identidad de desarrollo. | Abrir los flujos canónicos y revisar visualmente cada campo, etiqueta, ayuda y acción frente a AC-COL-01 y AC-COL-02 y a las rutas propietarias. | Cada control tiene propósito comprensible y los textos no contradicen las fuentes; cualquier texto confuso, pendiente o discrepancia se registra como hallazgo. | Navegador/versión si está disponible, página y estado revisados, campos y textos discrepantes, resultado y fecha real. |
| V02 | CA01, CA02 | Manual: presentación de errores de campo y de dominio en creación y edición. | Estados de error de validación y rechazo de dominio preparados o reproducibles sin cambios ajenos. | Enviar casos de prueba que produzcan errores ya previstos; observar asociación campo-mensaje, conservación de valores no afectados, distinción visual, anuncio/semántica expuesta y foco tras el rechazo. | Mensajes y foco coinciden con las fuentes S8.3 y requisitos de accesibilidad; toda limitación de lectura asistida se declara, no se infiere. | Caso observado, mensaje y campo, resultado de foco/semántica, entorno, fecha y límites. |
| V03 | CA03 | Manual: navegación por teclado de creación, edición y confirmación. | Páginas cargables y controles activos. | Recorrer con Tab/Shift+Tab y activar controles por teclado; observar orden y foco en estados normal y error cuando estén disponibles. | Orden lógico y foco visible en el recorrido revisado; controles operables y con nombres comprensibles, sin barreras observadas. | Secuencia/página revisada, controles no operables o fallos de foco, navegador/versión y fecha. |
| V04 | CA04 | Manual: textos y acciones de confirmación y rechazo de eliminación. | Colección de prueba prescindible o posibilidad de inspeccionar el rechazo sin ejecutar una eliminación destructiva. | Abrir la confirmación sin confirmar; revisar acciones. Inspeccionar rechazo por restricción solo si se puede reproducir sin riesgo para datos. | La acción y la alternativa son distinguibles; cualquier rechazo explica el resultado acorde a la fuente y no comunica éxito. La eliminación no se ejecuta salvo necesidad y con datos descartables. | Página/estado, acciones y mensaje observados, si se ejecutó una eliminación de prueba y qué datos, resultado y fecha. |
| V05 | CA05 | Manual: encabezados, landmarks, estructura y percepción de mensajes. | Las páginas y estados de V01–V04 están disponibles. | Inspeccionar la jerarquía de encabezados, navegación, estructura semántica y árbol de accesibilidad del navegador; registrar por separado pruebas asistivas o mediciones no ejecutadas. | La estructura permite orientarse y los mensajes son perceptibles en el alcance observado; no se atribuye cobertura a tecnología o mediciones no usadas. | Páginas/estados y elementos inspeccionados, herramientas realmente usadas, resultado, fecha y límites. |

Las validaciones son manuales y no sustituyen la suite HTTP ni acreditan por sí solas una conformidad completa de accesibilidad. No se propone comprobación negativa automatizada, porque esta spec no crea ni modifica tests; las discrepancias se registran y se escalan a la spec propietaria.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan; debe descomponer V01–V05 en revisión, registro integrado y aceptación humana.
- Cerrar la revisión técnica solo con evidencia real para todos los criterios incluidos, sin tratar un estado no comprobado como satisfactorio.
- La aceptación humana del plan y, posteriormente, de las tareas y del resultado son decisiones separadas.
- Ante una discrepancia con una regla o texto fuente, no improvisar una corrección: documentar el hallazgo y tramitar el cambio en el documento propietario.
