# Especificación: S7.6 — Análisis de parámetros de listados

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 23-X-2026 («Siguiente»).

## Objetivo

Definir el comportamiento comprobable de la frontera Web al interpretar los parámetros `p`, `c` y `q` de las páginas privadas incluidas en el alcance de S7.6, distinguiendo valores aplicables, ignorables y erróneos sin implementar la consulta ni el filtrado de resultados.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): alcance de S7.6 y Definition of Done.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas): parámetros aplicables, valores repetidos, vacíos, inválidos, filtros no disponibles y respuestas de error.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): límite del texto de búsqueda.
- [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros): semántica funcional de la búsqueda y filtros que consume el análisis.
- [S7.5 — Componentes de presentación accesible → CA01–CA06](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación): componentes de presentación ya disponibles.

## Alcance

- **Incluye:** análisis de `p`, `c` y `q` en las páginas privadas que forman parte del trabajo; selección del primer valor cuando un parámetro se repite; distinción entre parámetros aplicables, vacíos, válidos, inválidos, fuera de rango y no disponibles; entrega a la página de los valores interpretados o del estado de error.
- **Excluye:** análisis del parámetro `e` según la delimitación acordada para esta spec; consultas, búsquedas, filtrado, paginación de datos o autorización de recursos; presentación de rutas/dashboard y respuestas por método no permitido (S7.7); registro de peticiones (S7.8); suite HTTP transversal (S7.9); despliegue.
- **Trabajo vecino y responsable de sus entregables:** S7.7 integra las páginas privadas y sus respuestas HTTP; S7.8 gestiona el registro sin cadena de consulta; S7.9 verifica transversalmente los parámetros por HTTP. Estas tareas no son prerrequisitos para demostrar el análisis propio de S7.6.

## Dependencias y prerrequisitos

- La infraestructura Razor Pages y el host de pruebas Web de S0.5 están disponibles.
- S7.3 aporta la identidad privada y S7.4 el layout; S7.5 aporta los componentes de avisos y resumen de errores. S7.1 y S7.2 aportan la base de seguridad Web. Los trabajos S7.1–S7.5 constan como completados en el roadmap.
- La lógica de análisis puede comprobarse con pruebas focalizadas de la frontera Web, sin requerir que S7.7 implemente el dashboard o que S7.9 complete la suite amplia.
- Esta spec no redefine la regla fuente para `e`; su análisis y aplicación quedan fuera del alcance aquí acordado.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | Una petición a un listado privado dentro del alcance, con parámetros aplicables, desconocidos o de capacidades no disponibles. | La frontera Web interpreta la consulta. | Solo `p`, `c` y `q` se analizan cuando son aplicables a la ruta; los parámetros desconocidos, no aplicables o de capacidades no disponibles —incluido `e`, fuera del alcance MVP0 acordado— se ignoran sin validación, error ni consulta asociada. |
| CA02 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | Un parámetro aplicable aparece más de una vez. | Se analizan sus valores. | Se utiliza el primer valor y se descartan los sucesivos. |
| CA03 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | Un listado recibe `p` ausente o vacío, o recibe `p` con formato positivo válido, incluidos ceros iniciales. | Se interpreta el número de página. | El estado entregado corresponde a la página indicada por la fuente; si no se informa `p`, se aplica el valor por defecto definido allí. `p=1` no cambia el contenido respecto de la primera página sin parámetro. |
| CA04 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | Un listado de enlaces recibe `c` vacío o un identificador válido que no corresponde a una colección accesible del usuario identificado. | Se analiza el filtro de colección. | El filtro se ignora y los demás parámetros válidos se conservan. |
| CA05 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | `p` o `c` contiene un valor no válido o fuera del rango admitido por la fuente. | Se analiza la consulta del listado. | Se produce el estado de error accesible indicado por la fuente; no se usa el valor incorrecto para consultar ni filtrar resultados. |
| CA06 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) y [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Un listado recibe `q` vacío, con texto válido o con texto que, tras el recorte definido por la fuente, excede la longitud permitida. | Se analiza el texto de búsqueda. | El valor vacío se omite y se preservan los demás filtros; el texto válido se entrega para la búsqueda conforme a las reglas fuente; el exceso produce el error accesible establecido, sin ejecutar una búsqueda con ese valor. |
| CA07 | [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [S7.5 → CA01–CA02](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación) | Una combinación de parámetros contiene un error y también otros parámetros válidos. | La página consume el estado de error para presentar la respuesta prevista por su flujo. | Cuando el flujo mantiene parámetros válidos junto al error, estos no se pierden; la capa de presentación puede usar los componentes accesibles disponibles, sin cambiar la respuesta definida por la fuente. |

Los casos negativos quedan cubiertos por CA01 y CA05–CA07: parámetro desconocido/no aplicable, repetición, valor vacío, formato inválido, fuera de rango y longitud excedida. La validez de `c` se comprueba sin exponer recursos ajenos; la lectura y autorización de recursos no forman parte de esta unidad.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| La fuente de «Rutas privadas» también define el filtro `e`, mientras que el alcance de S7.6 pedido aquí lo excluye por quedar fuera del MVP0. Esta spec no cambia esa regla funcional ni declara completado su análisis. | [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas) | Ninguno de CA01–CA07; el análisis de `e` queda expresamente fuera del alcance de esta spec. | La delimitación de esta spec fue indicada por DLG en esta conversación: “Sólo P, c y q. e es para etiquetas y estas están fuera del mvp0”. La regla fuente no se modifica. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 23-X-2026 («Siguiente»).
- [Tareas y evidencia](tasks.md), aprobadas por DLG el 23-X-2026 («Vamos»). La base técnica fue aceptada por DLG el 23-X-2026 («parámetros validados»); integración funcional pendiente según su sección «Aceptación y cierre».
