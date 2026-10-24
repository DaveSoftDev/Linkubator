# Especificación: S7.7 — Dashboard y respuestas privadas

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 23-X-2026 («Siguiente»).

## Objetivo

Completar la entrada al área privada y el dashboard, así como la respuesta común de Web para métodos no permitidos y recursos válidos no accesibles, sin revelar datos ni propiedad de recursos.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): entregable S7.7 y su límite respecto a la suite integrada y las revisiones manuales vecinas.
- [requirements.md → «Páginas privadas»](../../context/requirements.md#páginas-privadas): acceso al dashboard y gestión privada.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas): rutas, métodos, contenido del dashboard, respuestas genéricas y comportamiento de páginas privadas.
- [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados): presentación de errores de acceso.
- [architecture.md → «Web»](../../context/architecture.md#web): responsabilidad de enrutamiento y presentación de errores accesibles.
- [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento): organización y protección de las páginas privadas.
- [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance): límite de MVP0 respecto a gestión de etiquetas.

## Alcance

- Incluye: completar el dashboard privado para el alcance disponible en MVP0; asegurar que entrar por `/app` conduce al dashboard; respuestas `404` genéricas para métodos no permitidos; presentación del mensaje común de recurso no accesible donde ya exista una ruta consumidora.
- Excluye: análisis de parámetros de listado (S7.6); creación de rutas o formularios de gestión de colecciones y enlaces que pertenecen a etapas posteriores; gestión de etiquetas, reservada a MVP1; configuración del registro de peticiones (S7.8); suite integrada HTTP transversal (S7.9); revisión manual amplia de teclado, foco y estilo (S7.10–S7.11); despliegue.
- Trabajo vecino y responsable de sus entregables: S7.6 implementa la interpretación de parámetros; S7.8 configura el registro de peticiones; S7.9 añade las pruebas HTTP transversales; S7.10 y S7.11 realizan las revisiones manuales. Las rutas funcionales futuras corresponden a sus etapas propietarias y no son prerrequisito del cierre de esta spec.

## Dependencias y prerrequisitos

- La infraestructura Razor Pages y el host Web existentes están disponibles.
- S7.3 aporta la resolución de identidad de desarrollo y S7.4 el layout privado accesible; S7.5 aporta los componentes comunes de presentación. Sus resultados se reutilizan sin alterar sus contratos.
- Los casos de uso de dashboard y sus contratos de aplicación ya están disponibles; S7.7 no redefine su lógica ni introduce datos de ejemplo como contenido funcional.
- Las rutas consumidoras de las respuestas de recurso no accesible aún no disponibles son trabajo de etapas posteriores. Esta spec valida el tratamiento común con un resultado controlado o un punto de prueba aislado, sin depender de que aquellas rutas se implementen.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Requisitos → «Páginas privadas»](../../context/requirements.md#páginas-privadas), [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [Arquitectura → «Enrutamiento»](../../context/architecture.md#enrutamiento) | Una petición al punto de entrada privado `/app` con la identidad de desarrollo resuelta. | Se carga el área privada. | Se muestra el dashboard, sin solicitar al cliente un `UserId`. La comprobación valida el destino funcional sin depender de si la navegación se resuelve en la ruta de entrada o mediante redirección. |
| CA02 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [Decisiones → «Producto y alcance»](../../context/decisions.md#producto-y-alcance) y [S7.5 → CA05](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación) | Se renderiza el dashboard dentro del alcance funcional disponible. | Se inspecciona su contenido y se usa el formulario de búsqueda. | Presenta accesos a los listados y acciones disponibles para colecciones y enlaces, y envía la búsqueda por GET al listado de enlaces con `q`; no presenta acciones de gestión de etiquetas reservadas a MVP1 ni implementa resultados de búsqueda en el dashboard. |
| CA03 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | Una ruta privada existente recibe un método distinto de los declarados para esa ruta. | Se solicita la ruta con dicho método. | La respuesta es el `404` genérico establecido, sin exponer si existe un recurso ni si pertenece al usuario. El criterio se valida con una ruta disponible o un punto de prueba aislado, sin depender de páginas futuras. |
| CA04 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Errores controlados»](../../context/specifications.md#errores-controlados) y [Arquitectura → «Web»](../../context/architecture.md#web) | El límite Web recibe el resultado de recurso no accesible para un recurso cuyo identificador tiene formato válido. | Se construye la respuesta privada GET correspondiente. | Se presenta el aviso genérico «No se ha podido acceder al elemento solicitado.» de forma accesible, sin revelar si el recurso no existe o pertenece a otra persona. La validación usa un resultado controlado y no depende de rutas consumidoras futuras. |
| CA05 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [S7.5 → CA02](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación) | Se comprueban las respuestas de CA03 y CA04. | Se comparan los datos de respuesta observables y el HTML renderizado en los casos equivalentes. | No se exponen identificadores, contenido privado ni la causa interna de falta de acceso; los avisos destinados al usuario se representan mediante los componentes accesibles existentes. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado tras la aclaración de alcance: la gestión de etiquetas queda fuera de MVP0. La navegación desde `/app` se verifica por su resultado visible y su ruta canónica, sin hacer depender el criterio de si el cliente muestra el salto de redirección. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 23-X-2026 («Siguiente»).
- [Tareas](tasks.md): propuestas; aprobación pendiente.