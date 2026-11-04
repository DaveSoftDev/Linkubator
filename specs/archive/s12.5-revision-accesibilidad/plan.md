# Plan técnico: S12.5 — Revisión manual de accesibilidad

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- La revisión es manual y cubre las páginas públicas y privadas y las comprobaciones A01–A11 definidas en la [matriz de checklist y tests](../../plans/mvp0-checklist-test-matrix.md).
- La evidencia se registra en las filas correspondientes de esa matriz. Incluirá el alcance revisado, el entorno utilizado, el resultado y, cuando proceda, los pasos para reproducir un hallazgo o el límite que impidió verificarlo.
- Las fuentes propietarias de las reglas son [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad) y [roadmap.md → «Checklist de calidad del MVP0»](../../context/roadmap.md#checklist-de-calidad-del-mvp0). Este plan describe cómo comprobarlas; no introduce criterios funcionales nuevos.

### Flujos internos

1. Identificar las páginas, formularios, estados de error y estados vacíos incluidos en la aplicación, usando la matriz como guía de cobertura.
2. Recorrer con teclado las rutas incluidas y revisar orden y visibilidad del foco, incluidos formularios y errores.
3. Revisar la asociación de labels y los anuncios con la tecnología de asistencia disponible, registrando cuál se utilizó y qué mensajes se comprobaron.
4. Inspeccionar landmarks, encabezados, el acceso para saltar al contenido, los nombres accesibles y los estados vacíos.
5. Comprobar contraste y dependencia del color en las combinaciones revisadas; registrar las combinaciones, el método y los resultados observados.
6. Activar la preferencia `prefers-reduced-motion` y revisar el comportamiento afectado.
7. Actualizar A01–A11 con la cobertura y evidencia reales. Registrar hallazgos y límites sin corregirlos dentro de S12.5.

### Persistencia y dependencias

- No hay cambios en persistencia de aplicación, transacciones ni contratos de código.
- Son prerrequisitos la aplicación disponible para revisión manual, la matriz de S12.1 y las fuentes de accesibilidad enlazadas en [spec.md](spec.md#fuentes).
- S12.6 realiza pruebas manuales de navegadores y flujos funcionales por separado. S12.5 no depende de su ejecución ni la sustituye.

### Garantías técnicas

- Las comprobaciones y la evidencia corresponden a páginas y estados efectivamente revisados; no se extrapolan resultados a páginas, mensajes o tecnologías no probados.
- Los hallazgos se registran con suficiente detalle para reconocer la página o control y reproducir el caso. Las combinaciones de contraste se identifican junto con el método y resultado observado.
- Un elemento no comprobado se registra como no verificable, no como satisfactorio.
- La revisión no modifica código, estilos, pruebas ni reglas, y no declara corregidos los hallazgos.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar las comprobaciones de accesibilidad manualmente y reportar evidencia en A01–A11. | confirmado | [spec.md → «Alcance»](spec.md#alcance), [matriz](../../plans/mvp0-checklist-test-matrix.md) | Mantiene la evidencia en la matriz existente y no crea un informe duplicado. |
| Registrar el entorno y las tecnologías de asistencia realmente usados, sin presuponer cobertura de herramientas no probadas. | confirmado | [spec.md → CA04 y CA09](spec.md#criterios-de-aceptación) | Hace explícitos los límites de la revisión. |
| La aplicación o un estado necesario podría no estar disponible para la revisión manual. | pendiente | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Registrar el bloqueo y las comprobaciones afectadas; no marcar criterios como satisfactorios por falta de acceso. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual: navegación por teclado de las páginas públicas y privadas incluidas | Aplicación accesible y páginas del alcance identificadas | Recorrer la navegación y operar los controles con teclado | Cobertura registrada; cualquier recorrido o control inoperable queda identificado | Filas A01 con páginas, controles, resultado y pasos de reproducción |
| V02 | CA02 | Manual: orden y visibilidad del foco, formularios y errores | Formularios y estados de error accesibles | Revisar secuencia de foco, foco inicial de formularios y foco tras errores | Se registra la cobertura y toda discrepancia respecto a los requisitos | Filas A02–A03 con páginas, campos, estado y resultado |
| V03 | CA03 | Manual: labels de formularios | Formularios incluidos accesibles | Inspeccionar la relación entre cada campo revisado y su label | Se identifican los campos comprobados y cualquier asociación ausente | Fila A04 con alcance y hallazgos |
| V04 | CA04 | Manual: anuncios para tecnologías de asistencia | Tecnología de asistencia disponible y mensajes de error/estado accesibles | Provocar o visitar los mensajes incluidos y comprobar su anuncio | Se registra la tecnología, los mensajes comprobados y los anuncios ausentes o no verificables | Fila A05 y, para mensajes de error, P27 con estado y evidencia |
| V05 | CA05 | Manual: semántica y salto al contenido | Páginas del alcance accesibles | Inspeccionar landmarks, jerarquía de encabezados y mecanismo de salto | Se registra la cobertura y toda estructura o acción ausente o inoperable | Filas A06–A07 con páginas y resultados |
| V06 | CA06 | Manual: contraste y uso del color | Combinaciones visuales relevantes presentes | Comprobar las combinaciones revisadas mediante un método adecuado y observar si el color es el único indicador | Se documentan combinaciones, método, resultados y cualquier límite o incumplimiento | Fila A08 con combinaciones, resultados y evidencia |
| V07 | CA07 | Manual: estados vacíos y nombres accesibles | Estados vacíos y enlaces o botones disponibles | Revisar la comunicación del estado vacío y el nombre accesible de los controles incluidos | Se registra cada estado y control revisado, incluidos los resultados no conformes | Filas A09–A10 con páginas, controles y resultados |
| V08 | CA08 | Manual: preferencia de movimiento reducido | Página o estado con movimiento disponible | Activar `prefers-reduced-motion` y observar el movimiento revisado | Se registra el comportamiento y cualquier caso no conforme o no verificable | Fila A11 con entorno, comportamiento y evidencia |
| V09 | CA09 | Revisión documental de cobertura y evidencia | V01–V08 completadas | Contrastar A01–A11 con las comprobaciones realizadas y sus evidencias | Cada fila refleja cobertura, resultado o límite real; los hallazgos son trazables y no se reportan como corregidos | Matriz actualizada y referencias a evidencias reproducibles |

Estas son comprobaciones manuales y no sustituyen tests automatizados ni las pruebas generales en navegadores de S12.6. Una limitación de entorno o de herramienta se registra como tal y no como resultado satisfactorio.

## Orden de ejecución y cierre

- Crear `tasks.md` después de que este plan obtenga aprobación humana explícita.
- Orden propuesto: inventariar páginas y estados; ejecutar V01–V08; actualizar las filas A01–A11 y revisar la trazabilidad con V09. Las comprobaciones son independientes cuando usan páginas distintas, pero deben consolidarse antes de V09.
- Cerrar cuando CA01–CA09 tengan evidencia satisfactoria de su comprobación, sin tratar un hallazgo observado como motivo para ocultar el resultado; las tareas propias estén completadas y la aceptación humana se registre en `tasks.md`.
- Si no se puede revisar un elemento, registrar el bloqueo y su impacto en los criterios afectados. La corrección de hallazgos queda fuera de S12.5.
