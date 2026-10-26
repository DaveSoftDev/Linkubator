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
- Mantener los enlaces de navegación y controles como HTML semántico y compatibles con navegación por teclado. Los enlaces del sidebar conservan su nombre accesible mediante `aria-label` al ocultarse el texto.
- `_AppSidebar` incorpora un enlace de acción independiente hacia `AppRoutes.LinkNew`, reutilizando `primary-button` y las reglas de `.sidebar-collapsed`. El texto se oculta en el rail compacto, mientras el SVG, `title` y `aria-label` permanecen disponibles conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas).
- Aplicar las reglas visuales de `.sidebar-collapsed` sin restringirlas al breakpoint de escritorio: el rail compacto oculta textos, contadores, búsquedas y colecciones en todos los tamaños. Las reglas móviles de apertura y cierre con la hamburguesa permanecen independientes. El rail contraído usa `--sidebar-width` con un ajuste móvil más estrecho y conserva áreas táctiles para los enlaces y el botón de expansión, sin cambiar el ancho expandido ni el de escritorio.
- Guardar la preferencia visual en la cookie de sesión `sidebar-collapsed`, limitada a `/app`, con `SameSite=Lax` y `Secure` bajo HTTPS. No contiene identidad ni permisos. El layout y el control de expansión leen únicamente el valor literal `true` para renderizar el estado antes del JavaScript, sin destello del sidebar expandido. El script actualiza la cookie al cambiar el estado y sincroniza las restauraciones desde la caché de navegación mediante `pageshow`; no guarda `.is-open`.
- Los enlaces y el botón de expansión del sidebar contraído usan superficies cuadradas centradas con esquinas discretamente redondeadas, sin alterar sus iconos ni el aspecto expandido.
- El sidebar ocupa toda la altura del viewport y sitúa el perfil en su extremo inferior, sin reservar espacio para el pie. En escritorio, el pie fijo queda por debajo del sidebar en el orden de apilado. Centrar el avatar cuando el sidebar está contraído. En móvil, el pie se oculta y el rail contraído conserva la altura del viewport sin estirarse con el contenido principal.

### Adaptación a pantallas pequeñas

- Acotar la tipografía y el espaciado del contenido privado al breakpoint de teléfono, sin modificar el aspecto de tablet y escritorio ni el comportamiento del sidebar.
- Usar una jerarquía de títulos compacta, formularios en una columna y campos con texto legible sin reducir el tamaño raíz del documento. Mantener las áreas táctiles de los botones y reservar en la cabecera el espacio de la hamburguesa. El buscador lateral usa `1rem` en el breakpoint móvil, sin deshabilitar el zoom del navegador.
- Comprobar el contenido con el sidebar oculto y contraído: campos utilizables, texto sin recortes y ausencia de desplazamiento horizontal.
- Aplicar también la presentación compacta a teléfonos en horizontal. Mantener el rail contraído con posicionamiento `sticky` durante el desplazamiento de páginas largas.
- Sincronizar `inert` con el menú móvil: sidebar oculto fuera de la interacción y contenido principal inactivo mientras el menú esté abierto. Restablecer el estado al cambiar de breakpoint.
- El desplegable de creación dispone de nombre accesible y se cierra con Escape; permite recorrer sus botones habilitados con flechas arriba/abajo de forma circular y saltar con Inicio/Fin, conservando Tab. Desde el disparador, flecha abajo abre y enfoca la primera opción y flecha arriba la última. El JavaScript usa el área de estado renderizada para los mensajes del prototipo, sin implementar persistencia ni navegación avanzada.
- En teléfonos de poca altura, reducir únicamente los márgenes y el relleno vertical para que el dashboard vacío quepa en pantalla; conservar tipografía, áreas táctiles y desplazamiento natural cuando haya más contenido o se amplíe el texto.

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
| La revisión manual del dashboard S7.10 y la validación global del estilo base S7.11 no bloquean CA01–CA06 | Confirmado | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) y regla de independencia del cierre SDD | El trabajo se cierra con evidencia propia; cada revisión humana tiene alcance y seguimiento independientes. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02, CA06 | Integración HTTP focalizada contra el host Web; inspección del HTML renderizado de una página privada | Una página privada accesible en el host de pruebas; coordinar la ruta del layout con S7.3/S7.7 sin depender de su implementación futura | Detectar primero el harness existente y elegir una respuesta renderizada estable; comprobar idioma, landmarks, destino del enlace de salto y meta robots en el HTML servido. Si las páginas `/app` aún no existen, crear una página de prueba acotada al host/harness de Web, sin anticipar el dashboard. | Cada requisito aparece en la respuesta real del host y el salto apunta a un identificador presente. | Nombre del test o petición, ruta usada, respuesta, comando, fecha y resultado; registrar qué no demuestra el comportamiento de la futura página final. |
| V02 | CA03, CA04 | Revisión técnica automatizada o comprobación focalizada de CSS | Estilos del layout disponibles | Identificar herramientas ya presentes; medir contrastes de las combinaciones de color seleccionadas y comprobar reglas de foco visible. Añadir test con dependencias existentes si hay un harness adecuado; no instalar herramientas solo para esta tarea. | Estados de foco están definidos y las combinaciones aplicables alcanzan WCAG AA. | Colores medidos, método/herramienta, cálculo de contraste y resultado por combinación; revisión del estilo de foco. |
| V03 | CA05 | Revisión automatizada focalizada del CSS generado y prueba negativa de sensibilidad | CSS del layout compilado | Confirmar regla CSS activa bajo `prefers-reduced-motion: reduce`; comprobar que el test/revisión detecta la ausencia temporal de esa regla y restaurarla. | Movimiento no esencial reducido bajo la preferencia y comprobación capaz de detectar su ausencia. | Selector/regla revisado, perturbación temporal y revertida, resultado y fecha. |
| V04 | CA01–CA06 | Revisión técnica integrada | V01–V03 | Revisar asignación del layout, scope privado/público, semántica del HTML, CSS, protecciones conservadas y `git diff --check`; ejecutar las comprobaciones focalizadas definidas arriba. | Todos los criterios tienen evidencia propia satisfactoria, sin cambios ajenos ni dependencia de etapas futuras. | Archivos revisados, resultados de las comprobaciones, fecha y entorno. |

Las comprobaciones automatizadas de estructura o CSS no sustituyen la revisión manual de teclado, foco, contraste y encabezados del dashboard asignada a S7.10 ni la validación del estilo base global asignada a S7.11. Esas tareas son independientes del cierre de esta spec.

## Orden de ejecución y cierre

- Tras aprobar este plan, preparar `tasks.md` con tareas para detectar el harness existente, implementar la plantilla y estilos, validar CA01–CA06 y sincronizar el roadmap. No iniciar cambios productivos antes de aprobar las tareas.
- CA01–CA06 se cierran con evidencia generada en este trabajo; no esperan la implementación futura del dashboard ni la revisión humana posterior.
- Cerrar solo cuando exista evidencia satisfactoria para todos los criterios, las tareas propias estén completadas y la aceptación humana esté registrada en `tasks.md`.
