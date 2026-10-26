# Especificación: S9.5 — Revisión manual de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 26-X-2026 a petición de DLG («Crea la spec S9.5»).
- Aprobación: DLG, 26-X-2026 («Aprobar spec y generar plan.md como propuesta»).

## Objetivo

Registrar una revisión manual reproducible de la creación y edición de enlaces, comprobando el uso de URLs reales, el rechazo de duplicados y el cambio de estado público o privado desde el formulario de edición.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): asignación y alcance de S9.5.
- [Criterios de aceptación del MVP0 → «AC-LINK-01 — Crear enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace) y [«AC-LINK-02 — Editar enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace): comportamiento aprobado de creación, duplicados y edición integral.
- [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) y [«Público y privado»](../../context/requirements.md#público-y-privado): capacidades visibles para la persona usuaria.
- [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces), [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) y [«Errores controlados»](../../context/specifications.md#errores-controlados): tratamiento de URL, duplicados y rechazos.
- [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado): transiciones de privacidad aplicables a la edición.
- [S9.1 — Crear enlace → «Criterios de aceptación»](../s9.1-crear-enlace/spec.md#criterios-de-aceptación) y [S9.3 — Gestionar enlaces → «Criterios de aceptación»](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación): flujos Web aceptados que se revisan manualmente.

## Alcance

- **Incluye:** revisión manual de la creación con URLs reales; intento de crear un enlace duplicado y comprobación de la respuesta; selección y guardado de los estados público y privado desde el checkbox de edición; registro de resultados y hallazgos reproducibles.
- **Excluye:** implementación o corrección de código; pruebas automatizadas; revisión visual de imagen, título y descripción (S9.6); auditoría de páginas públicas, navegación pública o metadatos obtenidos por scraping; redefinir las reglas de URL, duplicados, propiedad o privacidad.
- **Trabajo vecino y responsable de sus entregables:** S9.1 proporciona la creación de enlaces y S9.3 la edición integral; S9.4 cubre las pruebas automatizadas de integración HTTP y S9.6 la presentación de metadatos. Esta revisión no sustituye esos trabajos ni sus entregables.

## Dependencias y prerrequisitos

- S9.1 y S9.3 tienen implementación y aceptación registradas; sus flujos de creación y edición están disponibles para la revisión.
- Se requiere una instancia Web ejecutable, un navegador y un usuario identificado con colecciones propias. Las URLs concretas y el entorno de revisión se determinarán en el plan técnico.
- La revisión puede completarse con sus propias comprobaciones y no depende de S9.4, S9.6 ni de etapas posteriores.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [S9.1 CA02](../s9.1-crear-enlace/spec.md#criterios-de-aceptación) y [«URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces) | Un usuario identificado tiene una colección propia disponible y una URL real que cumple las reglas propietarias. | Crea un enlace y utiliza la acción de navegación del enlace guardado. | El enlace se crea conforme al comportamiento aprobado y la navegación utiliza la URL guardada, sin que esta sea sustituida por la de una redirección. Se registra la URL y el resultado observado. |
| CA02 | [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [«Enlaces»](../../context/requirements.md#enlaces) y [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados) | El usuario ya tiene un enlace propio cuya URL normalizada corresponde a una URL real comprobada. | Intenta crear otro enlace con una URL que resulta duplicada según la regla propietaria. | No se crea otro enlace y se indica la colección donde ya existe; el resultado se registra sin atribuirle una respuesta distinta de la definida por las fuentes. |
| CA03 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [S9.3 CA02](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación) y [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Un usuario identificado edita un enlace propio en una colección pública. | Usa el checkbox para guardar primero el estado privado y después el público. | Cada guardado establece el estado seleccionado conforme a las reglas propietarias y la URL del enlace permanece igual. |
| CA04 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [S9.3 CA03](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación) y [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Un usuario identificado edita un enlace propio de una colección privada. | Intenta seleccionarlo como público y guarda la edición. | La edición se rechaza según la regla aprobada, no se modifica ningún campo del enlace y se muestra el mensaje previsto; el resultado queda registrado. |

Los casos negativos y los límites de URL, duplicidad y privacidad se contrastan con las fuentes enlazadas. Esta revisión no afirma que una URL remota vaya a permanecer disponible ni cubre la presentación de metadatos o su recuperación mediante scraping.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar la especificación. Las URLs de prueba y el entorno se concretarán en el plan sin alterar las reglas aprobadas. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): propuesta; aprobación pendiente.
- [Tareas, evidencia y aceptación](tasks.md): pendientes de aprobación.
