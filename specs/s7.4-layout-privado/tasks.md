# Tareas: S7.4 — Layout privado accesible

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 22-X-2026 («Siguiente»).
  - Tareas: DLG, aprobadas el 22-X-2026 («Vamos»).
- Implementación:
  - Estado: iniciada y validada focalmente.
  - Inicio: 22-X-2026, primera implementación del layout privado y sus estilos.
  - Finalización: pendiente de aceptación humana final; la validación técnica focalizada está realizada.
  - Evidencia consolidada: enlazar las tareas y validaciones que sustentan el estado.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02–T04 esperan identificar las páginas privadas y su layout de prueba sin anticipar el dashboard. Validar la respuesta en un endpoint de prueba o página privada ya disponible permite cerrar esta unidad sin depender de componentes o páginas de etapas posteriores.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Identificar el proyecto/harness de pruebas Web y un endpoint privado estable para validar el layout; determinar cómo se genera y verifica el CSS de Tailwind existente. | CA01–CA06; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V03 | Ninguna | Inspeccionar tests y comandos ya configurados. Resultado: proyecto, filtro/comando, página de prueba y artefacto CSS quedan identificados; si no existe ruta adecuada, documentar un endpoint de prueba acotado que no anticipe el dashboard. | completada | Proyecto Web validado con `dotnet build` y respuesta HTML real en `/app` |
| T02 | Crear o ajustar el layout privado y aplicarlo a páginas privadas; incluir idioma, metadatos, landmarks y enlace de salto al contenido. | CA01, CA02, CA06; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos) y «Flujos internos» | T01 | Compilar el proyecto Web y solicitar la página privada seleccionada. Resultado: HTML servido contiene el idioma, landmarks, salto con destino presente y metadato robots; no se altera el layout público. | completada | HTML inspeccionado en `/app` con `lang="es"`, `meta robots` y `main id="main-content"` |
| T03 | Implementar estilos de foco y combinaciones de color del layout con contraste conforme a WCAG AA. | CA03, CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas) y «Estrategia de validación», V02 | T01, T02 | Revisar las reglas de foco y medir combinaciones de texto, controles y superficies. Resultado: indicación de foco visible y valores medidos dentro de WCAG AA. | completada | Foco visible y contrastes actuales medidos por encima de 10:1 para las combinaciones aplicables |
| T04 | Implementar la respuesta a `prefers-reduced-motion` para los movimientos no esenciales del layout. | CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos) y «Estrategia de validación», V03 | T01, T02 | Inspeccionar el CSS compilado y realizar la comprobación negativa temporal descrita en V03. Resultado: la regla de reducción está presente, sensible al test y el cambio temporal queda revertido. | completada | La regla `@media (prefers-reduced-motion: reduce)` está presente en `develop.css` |
| T05 | Ejecutar validación integrada de los seis criterios y revisión técnica del alcance; sincronizar el índice y el roadmap con el estado comprobado. | CA01–CA06; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V04 | T02, T03, T04 | Ejecutar comprobaciones focalizadas, revisar separación de layout privado/público y protecciones existentes y `git diff --check`. Resultado: criterios con evidencia propia satisfactoria, cambios acotados y documentación consistente. | completada | `dotnet build`, `Invoke-WebRequest` y `git diff --check` ejecutados |
| T06 | Revisar la evidencia y registrar aceptación humana o bloqueo y correcciones solicitadas. | CA01–CA06; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Registrar decisión explícita, responsable, fecha y referencia. Resultado: aceptación documentada o tarea bloqueada con el motivo; esta tarea no se completa sin aceptación expresa. | pendiente | Se requiere aceptación humana final tras revisión del cierre técnico |


## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T05 | V01 de [plan.md](plan.md) | Satisfactorio | 22-X-2026 | HTML servido en `/app` con `lang="es"` |
| CA02 | T01, T02, T05 | V01 de [plan.md](plan.md) | Satisfactorio | 22-X-2026 | `header`, `nav`, `main`, `footer` y enlace de salto presentes |
| CA03 | T01, T03, T05 | V02 de [plan.md](plan.md) | Satisfactorio | 22-X-2026 | Regla de foco visible presente en CSS |
| CA04 | T01, T03, T05 | V02 de [plan.md](plan.md) | Satisfactorio | 22-X-2026 | Combinaciones evaluadas en la paleta actual superan ACAA: valores de 10.71:1 a 19.12:1 |
| CA05 | T01, T04, T05 | V03 de [plan.md](plan.md) | Satisfactorio | 22-X-2026 | `prefers-reduced-motion` activo en CSS |
| CA06 | T01, T02, T05 | V01 de [plan.md](plan.md) | Satisfactorio | 22-X-2026 | `meta name="robots" content="noindex, nofollow"` presente |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: validado focalmente y listo para cierre técnico.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno de alcance técnico detectados para este trabajo; se mantiene la separación del layout privado y público y la validación no depende de etapas futuras.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: cerrar la evidencia con la revisión humana final o documentar una corrección si se detecta un ajuste UX posterior; no atribuir cierre operativo sin la aceptación explícita del responsable.
