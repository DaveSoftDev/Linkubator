# Plan técnico: S7.4 — Layout privado accesible

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: DLG, aprobado el 22-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Ajustar la plantilla Razor compartida para páginas `/app`, manteniendo separada la presentación pública. Definir en Web un layout privado explícito y asignarlo desde `_ViewStart` del área privada cuando esta exista; no cambiar el layout público de forma incidental.
- El layout privado aporta el elemento raíz, metadatos, landmarks, navegación de salto y contenedor principal conforme a los criterios de [spec.md](spec.md).
- Centralizar el estilo base del layout en el pipeline CSS existente de Tailwind, usando clases utilitarias y CSS de aplicación solo donde sea necesario para foco, salto y preferencias de movimiento. No añadir dependencias ni crear componentes que correspondan a S7.5.
- Mantener los enlaces de navegación y controles como HTML semántico y compatibles con navegación por teclado.

### Flujos internos

1. Una página de la zona privada selecciona el layout privado.
2. Razor compone metadatos, landmarks, controles comunes y contenido de la página.
3. El navegador aplica el estilo base; al solicitar movimiento reducido, las reglas CSS eliminan o minimizan los movimientos no esenciales.
4. Las páginas públicas siguen usando su layout propio y conservan su metadato robots conforme a sus fuentes.

### Persistencia y dependencias

No se modifica persistencia ni se requieren dependencias de paquetes. Se reutilizan Razor Pages y el proceso CSS/Tailwind ya configurados. Los componentes comunes de formularios, avisos, estados vacíos, paginación y búsqueda permanecen a cargo de S7.5. El dashboard y los análisis HTTP de S7.6–S7.9 no son prerrequisitos para cerrar esta especificación.

### Garantías técnicas

- El layout se aplica solo a páginas privadas, con el idioma, la semántica, los metadatos y las características de accesibilidad fijadas por [spec.md](spec.md) y las fuentes enlazadas.
- Los colores y estados visuales definidos por el layout se revisan mediante valores de contraste medibles; no se considera suficiente una inspección visual para probar WCAG AA.
- Las animaciones no esenciales responden a `prefers-reduced-motion` sin alterar la navegación ni ocultar información.
- Las protecciones de HTTPS, antiforgery y cabeceras existentes de [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web) se conservan.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener una plantilla privada diferenciada de la pública para acotar estilos y metadatos de `/app` | Propuesta | Alcance de S7.4 y [spec.md](spec.md) | Confirmar encaje con la estructura actual de Razor Pages al iniciar; no aplicar `noindex` a páginas públicas. |
| Usar el CSS/Tailwind existente y no incorporar una biblioteca de interfaz | Propuesta | [architecture.md → «Tecnologías»](../../context/architecture.md#tecnologías) | Reduce cambios y permite comprobar CSS generado en el proyecto Web. |
| Las revisiones manuales posteriores S7.10–S7.11 no bloquean CA01–CA06 | Confirmado | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) y regla de independencia del cierre SDD | El trabajo se cierra con evidencia propia; registrar las revisiones humanas como seguimiento de esas tareas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02, CA06 | Integración HTTP focalizada contra el host Web; inspección del HTML renderizado de una página privada | Una página privada accesible en el host de pruebas; coordinar la ruta del layout con S7.3/S7.7 sin depender de su implementación futura | Detectar primero el harness existente y elegir una respuesta renderizada estable; comprobar idioma, landmarks, destino del enlace de salto y meta robots en el HTML servido. Si las páginas `/app` aún no existen, crear una página de prueba acotada al host/harness de Web, sin anticipar el dashboard. | Cada requisito aparece en la respuesta real del host y el salto apunta a un identificador presente. | Nombre del test o petición, ruta usada, respuesta, comando, fecha y resultado; registrar qué no demuestra el comportamiento de la futura página final. |
| V02 | CA03, CA04 | Revisión técnica automatizada o comprobación focalizada de CSS | Estilos del layout disponibles | Identificar herramientas ya presentes; medir contrastes de las combinaciones de color seleccionadas y comprobar reglas de foco visible. Añadir test con dependencias existentes si hay un harness adecuado; no instalar herramientas solo para esta tarea. | Estados de foco están definidos y las combinaciones aplicables alcanzan WCAG AA. | Colores medidos, método/herramienta, cálculo de contraste y resultado por combinación; revisión del estilo de foco. |
| V03 | CA05 | Revisión automatizada focalizada del CSS generado y prueba negativa de sensibilidad | CSS del layout compilado | Confirmar regla CSS activa bajo `prefers-reduced-motion: reduce`; comprobar que el test/revisión detecta la ausencia temporal de esa regla y restaurarla. | Movimiento no esencial reducido bajo la preferencia y comprobación capaz de detectar su ausencia. | Selector/regla revisado, perturbación temporal y revertida, resultado y fecha. |
| V04 | CA01–CA06 | Revisión técnica integrada | V01–V03 | Revisar asignación del layout, scope privado/público, semántica del HTML, CSS, protecciones conservadas y `git diff --check`; ejecutar las comprobaciones focalizadas definidas arriba. | Todos los criterios tienen evidencia propia satisfactoria, sin cambios ajenos ni dependencia de etapas futuras. | Archivos revisados, resultados de las comprobaciones, fecha y entorno. |

Las comprobaciones automatizadas de estructura o CSS no sustituyen la revisión manual de usabilidad del dashboard, la secuencia real de teclado ni la aceptación del estilo global asignadas a S7.10 y S7.11. Esas tareas son independientes de cierre de esta spec.

## Orden de ejecución y cierre

- Tras aprobar este plan, preparar `tasks.md` con tareas para detectar el harness existente, implementar la plantilla y estilos, validar CA01–CA06 y sincronizar el roadmap. No iniciar cambios productivos antes de aprobar las tareas.
- CA01–CA06 se cierran con evidencia generada en este trabajo; no esperan la implementación futura del dashboard ni la revisión humana posterior.
- Cerrar solo cuando exista evidencia satisfactoria para todos los criterios, las tareas propias estén completadas y la aceptación humana esté registrada en `tasks.md`.
