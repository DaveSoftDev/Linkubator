# Especificación: S7.10 — Revisión manual del dashboard

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 24-X-2026 («Siguiente»).

## Objetivo

Registrar una revisión manual reproducible del dashboard privado y sus controles disponibles, verificando la navegación mediante teclado, el foco, el contraste y la estructura de encabezados dentro del alcance actual.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): asignación de la revisión manual a S7.10 y lista de comprobaciones.
- [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad): expectativas de accesibilidad que sustentan la revisión.
- [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas): contenido y comportamiento previstos del dashboard.
- [S7.4 — Layout privado accesible → CA01–CA06](../s7.4-layout-privado/spec.md#criterios-de-aceptación): base del layout privado que se revisa en contexto.
- [S7.7 — Dashboard y respuestas privadas → CA01–CA05](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación): resultado funcional y presentación actual del dashboard.

## Alcance

- **Incluye:** inspección humana del dashboard privado actual mediante teclado; secuencia de tabulación y foco visible en controles alcanzados; contraste de las combinaciones visuales presentes en el dashboard; jerarquía y orden de encabezados; registro de hallazgos con contexto suficiente para reproducirlos y decisión humana sobre el resultado de la revisión.
- **Excluye:** implementación de arreglos, refactor o cambios de estilo (se tramitan como trabajo separado si se solicitan); auditoría completa de todas las páginas privadas o públicas; validación global del estilo base y solicitud de ajustes, asignadas a S7.11; pruebas automatizadas de integración HTTP de S7.9; soporte de lector de pantalla como revisión completa, pruebas con tecnologías de asistencia, rutas funcionales de etapas posteriores, despliegue.
- **Trabajo vecino y responsable de sus entregables:** S7.4 entrega el layout privado; S7.7 entrega el dashboard; S7.9 verifica contratos HTTP mediante pruebas automatizadas; S7.11 revisa el estilo base global y solicita ajustes. S7.10 evalúa manualmente el dashboard disponible y registra defectos, pero no implementa correcciones ni sustituye esas entregas.

## Dependencias y prerrequisitos

- El dashboard de S7.7 y el layout de S7.4 están implementados y aceptados; ambos pueden inspeccionarse en el entorno de desarrollo.
- Se necesita una instancia Web ejecutable y un navegador con soporte de teclado y herramientas para inspeccionar colores/contraste y estructura del documento. El procedimiento y entorno concretos se confirmarán en el plan técnico.
- La revisión se limita al contenido disponible al iniciar S7.10. No depende de que etapas posteriores añadan listados, formularios o controles; la ausencia de rutas funcionales futuras no bloquea el cierre.
- S7.11 y sus ajustes globales no son prerrequisito del cierre de S7.10. Si la revisión identifica un problema que requiere cambio de producto o fuente funcional, se registra como bloqueo concreto y no se inventa una regla.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad), [S7.4 → CA02–CA03](../s7.4-layout-privado/spec.md#criterios-de-aceptación) y [mvp0-plan.md → S7.10](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | El dashboard actual se sirve en el navegador y se inicia la revisión sin usar el ratón. | Se recorre mediante teclado el contenido interactivo disponible, siguiendo su orden de tabulación. | La secuencia es lógica y corresponde al orden de presentación; cada control alcanzado conserva un indicador visual de foco perceptible. Los defectos observados quedan registrados con pasos de reproducción o se deja constancia de que no se encontraron. |
| CA02 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad), [S7.4 → CA04](../s7.4-layout-privado/spec.md#criterios-de-aceptación) y [mvp0-plan.md → S7.10](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | Se identifican las combinaciones de texto, controles y superficies que aparecen en el dashboard durante la revisión. | Se mide el contraste de las combinaciones aplicables con el método y herramienta registrados. | El resultado se compara con el nivel de conformidad definido por la fuente y cualquier combinación que no lo alcance queda documentada de forma reproducible. |
| CA03 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad), [S7.7 → CA02](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación) y [mvp0-plan.md → S7.10](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | Se inspecciona el dashboard renderizado, incluidos sus encabezados y regiones de contenido. | Se revisa el orden y la jerarquía semántica de los encabezados frente a la estructura visual. | La estructura de encabezados representa una jerarquía coherente y los hallazgos, si los hay, identifican el encabezado y contexto afectados. |
| CA04 | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l), [S7.4 → CA01–CA06](../s7.4-layout-privado/spec.md#criterios-de-aceptación) y [S7.7 → CA01–CA05](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación) | Se han ejecutado las comprobaciones manuales CA01–CA03. | Se consolidan los resultados y se presenta la revisión para decisión humana. | Quedan registrados entorno y fecha, recorridos realizados, resultados positivos y hallazgos; la decisión explícita de aceptación o solicitud de ajustes se registra sin atribuir a esta revisión cambios productivos ni validaciones no realizadas. |

Los casos negativos y límites aplicables se evalúan mediante CA01–CA03: incluyen controles que no pueden alcanzarse o cuyo foco no es perceptible, contrastes insuficientes y jerarquías de encabezados incoherentes. No se afirma cobertura de lectores de pantalla ni conformidad completa de toda la zona privada.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar la especificación. El navegador, el perfil de contenido y el registro de evidencia se concretarán en el plan sin ampliar la revisión más allá del dashboard disponible. | — | — | — |

## Artefactos relacionados

Todavía no existen otros artefactos de esta spec. El plan técnico se preparará después de la aprobación explícita de esta especificación.
