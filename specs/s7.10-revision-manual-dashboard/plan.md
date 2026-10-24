# Plan técnico: S7.10 — Revisión manual del dashboard

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 24-X-2026 («Vamos»).

## Diseño de implementación

### Componentes y contratos

- Reutilizar el dashboard Web y el layout privado existentes, sin modificar el producto durante esta spec. La página objetivo es la servida por `/app/dashboard`, con el contenido y controles actuales descritos en [spec.md](spec.md), [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [S7.7 → CA01–CA05](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación).
- La evidencia manual se organizará por criterio y registrará navegador/versión, viewport o configuración pertinente, pasos realizados, elementos examinados, mediciones y resultado. Si se detectan defectos, cada hallazgo incluirá ubicación/contexto y pasos suficientes para reproducirlo; no se atribuirá al plan un cambio no ejecutado. La inspección se hace con el entorno autenticado de desarrollo ya disponible; no se introducen credenciales ni se incluyen secretos en evidencia.
- Los contrastes se evaluarán sobre combinaciones realmente presentes en el dashboard, incluyendo estados interactivos pertinentes; se usará un método de medición identificable. La inspección de encabezados se hará sobre el documento renderizado y su jerarquía semántica, no solo por apariencia visual.
- Mantener separadas la evaluación del dashboard y la revisión global del estilo de S7.11. No incluir tareas de arreglo, cambios de código ni ajustes de reglas funcionales.

### Flujos internos

1. Identificar el host y la configuración local disponibles y abrir el dashboard en un navegador.
2. Registrar entorno inicial y confirmar la página/contenido examinado.
3. Ejecutar el recorrido de teclado, registrar orden y foco; inspeccionar contrastes de los elementos relevantes y revisar la jerarquía de encabezados.
4. Consolidar la evidencia por CA01–CA03, enumerar hallazgos y límites de cobertura, y someter el resultado a aceptación humana.
5. Si un hallazgo exige cambio de código, regla funcional o decisión de diseño fuera del alcance, describirlo y detener cualquier cambio: debe tratarse mediante trabajo separado. La revisión puede documentar el hallazgo sin esperar a la implementación de la corrección.

### Persistencia y dependencias

No se modifican datos de aplicación, persistencia, dependencias ni infraestructura. Se requiere una instancia de desarrollo ejecutable con el dashboard privado disponible y el acceso necesario a la pantalla. S7.4 y S7.7 aportan el layout y el dashboard ya implementados; S7.9, S7.11 y etapas posteriores no son prerrequisitos de cierre.

### Garantías técnicas

- La revisión informa solo de lo observado en el dashboard disponible; no declara conformidad de toda la aplicación ni cobertura completa con lectores de pantalla.
- Los criterios y resultados se vinculan a los requisitos aprobados de accesibilidad y al alcance de S7.10, sin redefinir umbrales ni añadir reglas de producto.
- No se realizan cambios productivos ni se considera un hallazgo corregido sin evidencia separada y autorizada.
- El cierre se sostiene en evidencia propia para CA01–CA04 y aceptación humana; no depende de S7.11.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Inspeccionar la página real del dashboard en el navegador y no una maqueta | Confirmado | [S7.10 spec](spec.md) y [S7.7 → CA01–CA02](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación) | Permite observar el orden, foco, color y estructura del contenido actual; registrar cualquier límite de acceso al entorno. |
| Herramienta/navegador y formato concreto para guardar evidencia | Pendiente | [S7.10 spec](spec.md), prerrequisitos | Confirmar herramientas ya disponibles al preparar tasks; evitar instalar dependencias para una revisión manual. |
| No implementar arreglos en el alcance de S7.10 | Confirmado | [S7.10 spec](spec.md) | Hallazgos que precisen cambios se registran para seguimiento independiente; la ausencia de esos cambios no se disfraza como corrección. |
| La revisión global del estilo base pertenece a S7.11 y no bloquea CA01–CA04 | Confirmado | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) y [S7.10 spec](spec.md) | Mantener el alcance limitado al dashboard y cerrar con evidencia propia. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual, teclado y foco en dashboard | Aplicación Web disponible, ruta `/app/dashboard` accesible, navegador elegido | Con el puntero apartado, iniciar desde el comienzo de la página y recorrer por teclado todos los controles disponibles en el flujo; registrar secuencia, orden visual y estado de foco en cada control. Incluir navegación inversa cuando sea necesaria para documentar una desviación. | Recorrido observable y conclusión respecto al orden lógico y foco visible; cualquier control omitido o foco imperceptible se reporta con pasos. | Navegador/versión, entorno y contenido, pasos y controles inspeccionados, resultado y hallazgos. No demuestra usabilidad con lectores de pantalla. |
| V02 | CA02 | Manual/instrumental, medición de contraste | La página y sus colores/estados visuales pueden inspeccionarse; herramienta disponible identificada | Enumerar combinaciones aplicables de texto, superficies y controles visibles del dashboard, capturar los colores efectivos y medirlos con un método reproducible; considerar estados visuales pertinentes observados. | Cada combinación queda medida y contrastada con la exigencia de la fuente; las combinaciones insuficientes se reportan claramente. | Elemento y estado, colores medidos, herramienta/método, resultado, entorno y fecha. La medición no valida el contraste en páginas no revisadas. |
| V03 | CA03 | Manual, semántica de encabezados | Dashboard renderizado | Inspeccionar el árbol DOM/outline accesible del dashboard y comparar el orden semántico con la organización visual de secciones. Registrar títulos, niveles y contexto de inconsistencias. | Jerarquía comprensible y coherente o hallazgos reproducibles documentados. | Ruta/contenido, orden y niveles observados, herramienta de inspección, fecha y resultado. No es una prueba completa con lector de pantalla. |
| V04 | CA01–CA04 | Revisión técnica y aceptación humana | V01–V03 completadas y evidencia consolidada | Verificar que cada criterio tiene evidencia o limitación explícita, que los hallazgos se pueden reproducir, que no hay cambios productivos asociados y que la decisión humana está registrada. | Evidencia completa y decisión explícita de aceptación o ajustes solicitados; cualquier cambio solicitado queda fuera del alcance y se registra como seguimiento. | Referencias a registros de V01–V03, fecha, responsable, decisión y referencia explícita. |

Las comprobaciones son revisión manual, no tests automatizados. La secuencia de teclado no demuestra por sí sola que una tecnología de asistencia anuncie correctamente el contenido; el análisis de contraste solo cubre las combinaciones identificadas en el dashboard; la jerarquía inspeccionada no acredita conformidad global de todo el sitio. La prueba negativa correspondiente consiste en registrar como hallazgo cualquier control inaccesible, foco imperceptible, combinación insuficiente o estructura incoherente, en vez de cerrar el criterio sin documentar el resultado.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde su plantilla y someterlo a aprobación antes de ejecutar la revisión. Las tareas incluirán preparación del entorno, las revisiones manuales CA01–CA03, consolidación y decisión humana CA04.
- Las tareas de inspección son independientes entre sí una vez disponible el entorno; la consolidación depende de completar la evidencia de todas ellas. El cierre no espera arreglos, S7.11 ni etapas posteriores.
- Cerrar solo cuando CA01–CA04 tengan evidencia satisfactoria, las tareas propias estén completadas y la aceptación humana esté registrada en `tasks.md`.
