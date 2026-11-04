# Especificación: S12.5 — Revisión manual de accesibilidad

## Estado y aprobación

- Estado: aprobada.
- Preparada por IA el 31-X-2026.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Revisar manualmente la accesibilidad de las páginas públicas y privadas y dejar evidencia trazable de las comprobaciones, los hallazgos y los límites de la revisión, sin implementar correcciones ni cambiar reglas del producto.

## Fuentes

- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [roadmap.md → «Checklist de calidad del MVP0»](../../../context/roadmap.md#checklist-de-calidad-del-mvp0)
- [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad)
- [Matriz de checklist y tests → A01–A11 y P27](../../../plans/mvp0-checklist-test-matrix.md)

## Alcance

- Incluye:
  - Revisar con teclado la navegación completa por las páginas públicas y privadas.
  - Comprobar el orden y la visibilidad del foco, el foco en formularios y errores, y la asociación de labels.
  - Revisar los anuncios para tecnologías de asistencia, los landmarks y encabezados, y el acceso para saltar al contenido principal.
  - Comprobar contraste WCAG AA, mensajes de estados vacíos, nombres accesibles y el comportamiento con `prefers-reduced-motion`.
  - Registrar en las filas A01–A11 de la matriz el alcance realmente comprobado, el entorno y la evidencia; documentar los hallazgos y los elementos no verificables.
- Excluye:
  - Corregir hallazgos o cambiar código, estilos, pruebas o reglas funcionales.
  - Automatizar pruebas de UI o sustituir las pruebas manuales de páginas, navegadores, formularios y listados asignadas a S12.6.
  - Dar por cubiertos los criterios por revisiones parciales anteriores; estas pueden aportar contexto, pero no sustituyen la revisión actual.
- Trabajo vecino y responsable de sus entregables: S12.1 mantiene la matriz de trazabilidad; S12.6 realiza las pruebas manuales en navegadores, páginas, formularios, listados, errores y estados vacíos. S12.5 registra la evidencia de accesibilidad en A01–A11 y no asume ni sustituye las comprobaciones de S12.6.

## Dependencias y prerrequisitos

- La matriz de S12.1 está disponible y clasifica A01–A11 como revisiones manuales pendientes.
- Los requisitos de accesibilidad están definidos en su documento fuente. Las revisiones parciales anteriores indicadas en la matriz no cubren la revisión completa.
- No se requiere que S12.6 haya finalizado. El entorno empleado durante la revisión se registrará con la evidencia, sin dar por comprobados navegadores o tecnologías de asistencia que no se hayan usado.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A01 | La aplicación ofrece páginas y controles públicos y privados | Se recorre su navegación usando teclado | Se registra la cobertura de la navegación y cualquier control o recorrido que no pueda operarse por teclado. |
| CA02 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A02–A03 | Se recorren páginas y formularios, incluidos los que presentan errores | Se observa el orden, la visibilidad y el movimiento del foco | Se registra si el foco sigue el orden esperado, permanece visible y llega al campo lógico y al primer campo con error, así como cualquier discrepancia. |
| CA03 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A04 | Se revisan los campos de los formularios incluidos en el alcance | Se comprueba su asociación con labels | Se registra la cobertura y cada campo cuya etiqueta no quede asociada. |
| CA04 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A05 y P27 | Se presentan errores u otros mensajes incluidos en la revisión | Se comprueba su comunicación mediante tecnologías de asistencia | Se registra qué mensajes se anunciaron y cuáles no, junto con la tecnología utilizada o el límite de verificación. |
| CA05 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A06–A07 | Se recorren las páginas incluidas en la revisión | Se inspeccionan landmarks, encabezados y el acceso para saltar al contenido | Se registra la cobertura y cualquier estructura semántica o mecanismo de salto ausente o inoperable. |
| CA06 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A08 | Se revisan las combinaciones de texto, fondo, controles e indicadores presentes en las páginas | Se comprueba el contraste y si la información depende únicamente del color | Se documentan las combinaciones y resultados comprobados, y cualquier incumplimiento o límite de medición. |
| CA07 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A09–A10 | Se muestran los estados vacíos y los enlaces y botones incluidos en la revisión | Se comprueba su comunicación y sus nombres accesibles | Se registra si los estados vacíos se perciben y si los nombres identifican las acciones, incluidos los casos que no cumplan estos criterios. |
| CA08 | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad); matriz A11 | La interfaz puede presentar movimiento | Se activa la preferencia `prefers-reduced-motion` y se revisa el comportamiento afectado | Se registra el comportamiento observado y cualquier movimiento que no respete la preferencia, o que no haya podido verificarse. |
| CA09 | [Checklist de calidad del MVP0](../../../context/roadmap.md#checklist-de-calidad-del-mvp0); matriz A01–A11 | Se han realizado las comprobaciones de accesibilidad dentro del alcance | Se consolida la evidencia de S12.5 | Cada fila A01–A11 refleja una comprobación realizada, su resultado o su límite, y los hallazgos quedan descritos de forma trazable sin presentar las correcciones como parte de S12.5. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna identificada al preparar la especificación. Los elementos que no puedan comprobarse durante la revisión se registrarán como límites, sin atribuirles un resultado. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — propuesta, pendiente de aprobación.
- Las tareas se crearán tras la aprobación del plan técnico.
