# Plan técnico: S10.6 — Revisión manual de búsqueda de enlaces

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: aprobado por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- La revisión observa en navegador el listado Web ya implementado en `/app/enlaces`; no modifica las páginas, los casos de uso, las consultas ni los datos de producto. Contrasta el resultado con los criterios de [spec.md](spec.md), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas).
- Utilizar el conjunto local preparado por [S6.8](../archive/s6.8-datos-local-busqueda/spec.md), asociado a la identidad de desarrollo. Confirmar antes de la sesión que la base local existe, es la prevista para esta revisión y conserva los datos/proyecciones requeridos; no ejecutar el seed, borrar datos ni reconstruir proyecciones como parte de S10.6.
- Ejecutar manualmente las consultas del listado privado y observar los enlaces mostrados, el texto de consulta, el filtro de colección y los controles de paginación. Para cada escenario registrar la URL de la aplicación sin credenciales ni tokens, la consulta funcional no sensible, la colección, el orden/contenido observado y el resultado.
- El registro estructurado de evidencia y sus límites constan en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio); no incluir credenciales, cookies, identificadores de sesión ni otros secretos.

### Flujos internos

1. Confirmar entorno Web local, identidad de desarrollo, base de ejemplo existente y navegador. Registrar sistema operativo, navegador/versión y fecha; validar que el contenido visible identifica los ejemplos esperados antes de ejecutar escenarios. Si falta un prerrequisito, detener la revisión afectada y registrar el bloqueo; no crear ni alterar datos para simularlo.
2. Para CA01, localizar en los datos existentes un ejemplo con acento y probar desde el listado la forma acentuada y la forma equivalente sin acento; cuando el seed exponga ambas formas Unicode, probarlas también. Registrar consultas y resultados comparables.
3. Para CA02, usar ejemplos del seed con más de un término y con palabras que parezcan operadores. Comprobar los resultados en el listado y contrastarlos con la semántica fuente, sin inferir reglas por el texto de la consulta solamente.
4. Para CA03, efectuar una consulta con resultados en más de una colección, primero sin filtro y después con una colección propia seleccionada. Comparar los enlaces visibles y comprobar que no aparezcan elementos de otra colección o de otro usuario.
5. Para CA04, usar una consulta con resultados que crucen una frontera de página; navegar entre páginas y comprobar visualmente los enlaces, el orden y la conservación de `q` y `c` en los controles de navegación.
6. Consolidar por criterio los pasos, resultado observado, límites y hallazgos. No corregir discrepancias durante la revisión; documentarlas y detener cualquier cambio de producto para tramitarlo por separado en la fuente o trabajo correspondiente.

### Persistencia y dependencias

- No se escriben ni modifican datos: las operaciones son consultas GET en el listado. No se ejecuta el seed local, el comando de reconstrucción de FTS5 ni cambios sobre la base compartida.
- Prerrequisitos: entorno Web local accesible y apuntando a la base preparada; identidad de desarrollo y datos/proyecciones de S6.8 presentes; navegador disponible. La persona ejecutora confirma las rutas y versiones efectivas antes de probar.
- S6.8 aporta la ayuda de datos local, S10.1–S10.4 los flujos ya aceptados y S10.5 la cobertura automatizada complementaria. Ninguno de los resultados de S10.5 ni de la etapa S10 completa es prerrequisito de cierre de esta revisión.
- Si la base está ausente, los datos están incompletos o la aplicación no los consulta, se registra como prerrequisito bloqueado; no se amplía S10.6 para reconstruir S6.8 ni se atribuye una causa sin evidencia.

### Garantías técnicas

- La revisión se limita al comportamiento observado de la búsqueda de enlaces; no afirma que haya ejecutado pruebas automatizadas, validado la implementación interna o cubierto listados de otras entidades.
- Los escenarios se realizan bajo la identidad de desarrollo y con datos locales preparados, sin usar ni exponer recursos ajenos ([domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos)).
- Las expectativas de normalización, operadores literales, filtros, orden y páginas son las fuentes de [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../context/specifications.md#listados); no se copian ni redefinen aquí.
- Una observación satisfactoria no sustituye las pruebas HTTP/de datos de S10.5 ni prueba por sí sola la ausencia de defectos no cubiertos por los escenarios manuales.
- Ante resultados inesperados, se conserva el estado, se documenta evidencia reproducible y no se modifica producto, seed ni fuente funcional dentro de S10.6.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar el navegador sobre el listado privado real con la base local de ejemplo existente. | Confirmado | [spec.md → «Alcance»](spec.md#alcance), [S6.8](../archive/s6.8-datos-local-busqueda/spec.md) | Mantiene el trabajo en revisión manual y no cambia el producto. Confirmar en T01 que el entorno apunta a la base preparada. |
| Registrar solo entorno, navegador/versión, consultas y resultados necesarios para reproducir los escenarios. | Propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | La evidencia no incluirá credenciales, cookies, tokens ni datos personales. Concretar el formato al crear las tareas tras aprobar el plan. |
| La disponibilidad y consistencia de la base de S6.8 debe confirmarse antes de cada revisión; la base observada inicialmente no contenía enlaces. | Comprobada inicialmente; prerrequisito contextual | [S6.8](../archive/s6.8-datos-local-busqueda/spec.md), [tasks.md → T01](tasks.md#descomposición) | T01 registró la ausencia inicial de enlaces; se identifican resultados posteriores obtenidos con el seed cargado, pero no consta el procedimiento ni entorno de carga en la evidencia original. No interpretar ese hecho como una recarga autorizada por S10.6. |
| La indisponibilidad externa, de la Web local o del navegador puede impedir una comprobación manual. | Riesgo | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Registrar el criterio afectado como no comprobado; no simular su resultado ni cerrar con evidencia incompleta. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual, búsqueda en listado Web: acentos y formas Unicode previstas en los datos | Web local y base de S6.8 disponibles; ejemplo de texto con acento localizado | En `/app/enlaces`, buscar las formas previstas y comparar los enlaces visibles. Elegir consultas a partir del contenido realmente cargado; no alterar el seed. | Los resultados observados concuerdan con la equivalencia de búsqueda establecida en la fuente. | Entorno, navegador/versión, consultas, enlaces/resultados observados y resultado por forma probada. |
| V02 | CA02 | Manual, búsquedas con varios términos y operadores literales | Ejemplos del seed identificados que discriminen términos requeridos y palabras que parecen operadores | Ejecutar cada consulta en el listado, contrastando los enlaces mostrados con los ejemplos y con la regla propietaria. | Coinciden solo los resultados que satisfacen los términos; las palabras de operador actúan según la semántica fuente y no como instrucciones de búsqueda. | Consulta, casos de contraste, resultados visibles, pasos y resultado observado. |
| V03 | CA03 | Manual, búsqueda combinada con filtro de colección | Consulta con enlaces propios coincidentes en más de una colección; colección objetivo identificada | Buscar sin filtro y luego aplicar la colección objetivo; comparar tarjetas/enlaces de ambas respuestas. | El filtro restringe el resultado a los enlaces de la colección seleccionada y no muestra datos ajenos. | Consulta, colección seleccionada, resultados comparados, URL sin datos sensibles y resultado. |
| V04 | CA04 | Manual, navegación paginada con consulta y filtro | Consulta con resultados suficientes para más de una página y filtros aplicables | Navegar entre páginas desde los controles del listado y revisar los resultados y parámetros conservados. | Los resultados siguen el orden y paginación de la fuente; los controles mantienen los filtros aplicables. | Consulta, filtro, páginas recorridas, elementos visibles, parámetros observados y resultado. |
| V05 | CA01–CA04 | Revisión técnica de cierre y evidencia | V01–V04 ejecutadas o bloqueo concreto documentado | Revisar que cada CA tenga observaciones reales, los hallazgos sean reproducibles y la evidencia no contenga secretos; verificar referencias y cambios limitados a la documentación de S10.6. | Trazabilidad completa a la fuente, sin presentar expectativas como resultados ni modificaciones de producto como parte de la revisión. | Matriz final por CA, fecha, entorno/versión, límites y referencia de cada evidencia. |

La validación es manual y no demuestra el comportamiento de otras combinaciones de datos, pruebas automatizadas, rendimiento, seguridad global ni compatibilidad con otros navegadores. No se requiere una comprobación negativa mediante mutación, ya que esta especificación no añade tests automatizados ni cambios técnicos; los escenarios de contraste de V02 y V03 sirven para hacer observables los resultados dentro del alcance manual.

## Orden de ejecución y cierre

- Las tareas fueron aprobadas por DLG el 27-X-2026 («Vamos»); la evidencia, sus límites y la aceptación final constan en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- El cierre de S10.6 registra los resultados observados: CA01–CA03 satisfactorios y CA04 parcialmente comprobado con límite documentado para la combinación `q + c` en varias páginas. La aceptación humana no eleva esa evidencia parcial a validación satisfactoria.
- No condicionar el cierre a la aprobación, ejecución o resultado de S10.5 ni a la finalización de S10; estos son trabajos independientes.
- Si aparece una discrepancia con reglas aprobadas, no redefinirla ni corregir el producto en este trabajo: documentar el caso y solicitar resolución en el documento propietario.
