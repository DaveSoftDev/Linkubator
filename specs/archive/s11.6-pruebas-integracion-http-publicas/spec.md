# Especificación: S11.6 — Pruebas de integración HTTP del área pública

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

La suite de pruebas HTTP comprueba que las páginas públicas mantienen el contenido y orden públicos, las respuestas `404`, las redirecciones, la paginación y las URL canonical establecidos por sus fuentes.

## Fuentes

- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), en particular [«Rutas públicas»](../../../context/specifications.md#rutas-públicas), [«Paginación de las páginas públicas»](../../../context/specifications.md#paginación-de-las-páginas-públicas), [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones) y [«Página de colección»](../../../context/specifications.md#página-de-colección)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)
- [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento)
- [mvp0-acceptance-criteria.md → «AC-USER-01 — Página pública de usuario»](../../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario)
- [mvp0-acceptance-criteria.md → «AC-COLLS-01 — Página pública de colecciones»](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones)
- [mvp0-acceptance-criteria.md → «AC-COLLECTION-01 — Página pública de una colección»](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)
- [roadmap.md → «Área pública y SEO»](../../../context/roadmap.md#área-pública-y-seo)

## Alcance

- Incluye:
  - Pruebas HTTP del contenido público, su orden y la ausencia de datos privados en las páginas de usuario, colecciones y colección.
  - Pruebas de las respuestas `404` públicas para usuarios, colecciones y rutas que ya no representan recursos accesibles.
  - Pruebas de redirección de variantes de rutas públicas y de conservación del resultado de paginación.
  - Pruebas de las páginas de colección paginadas y de la canonical normalizada que corresponde a la página servida.
  - Pruebas de que las URL canonical usan el origen público configurado y no el `Host` de la petición.
- Excluye:
  - La implementación o modificación funcional de las páginas y del tratamiento de rutas, asignada a S11.1–S11.5.
  - La revisión manual del HTML renderizado, los textos finales y el aspecto visual, asignada a S11.7.
  - La prueba manual de navegación pública con datos preparados, asignada a S11.8.
  - La definición de reglas nuevas, el SEO avanzado y las verificaciones manuales de navegador.
- Trabajo vecino y responsable de sus entregables: S11.1 implementa la landing; S11.2, la página pública de usuario; S11.3, las páginas públicas de colecciones y colección; S11.4, los atributos transversales; S11.5, las variantes de URL y respuestas `404`; S11.7 y S11.8, las revisiones manuales. Esta spec cubre sus pruebas HTTP generales sin asumir sus entregables.

## Dependencias y prerrequisitos

- Los criterios aprobados AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01, junto con sus fuentes funcionales, definen el contenido, el orden y las respuestas que deben comprobarse.
- Para ejecutar la suite integrada deben estar disponibles las páginas de S11.1–S11.3 y el tratamiento de URL de S11.5. Según el [roadmap](../../../context/roadmap.md#área-pública-y-seo), S11.1 y S11.3 constan completadas, S11.2 está en curso y la validación integrada de S11.5 espera su cierre.
- La preparación de esta spec no requiere que esos trabajos estén terminados.
- No se identifican dependencias adicionales necesarias para definir el alcance.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-USER-01](../../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario), [AC-COLLS-01](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones), [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Hay datos públicos y privados distinguibles, con los órdenes y estados vacíos descritos por las fuentes | Se solicitan las páginas públicas correspondientes | Las respuestas incluyen solo el contenido público que corresponde a cada página y respetan el orden y los estados definidos; no contienen nombres, emails ni contenido privado. |
| CA02 | [AC-USER-01](../../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario), [AC-COLLS-01](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones), [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [S11.5 → CA03–CA04](../s11.5-variantes-url-publicas/spec.md#criterios-de-aceptación) | La ruta solicitada corresponde a un alias o recurso inexistente, antiguo, privado o no disponible públicamente | Se solicita su página pública | La respuesta es `404` para los casos definidos por la fuente y no expone el contenido del recurso. |
| CA03 | [specifications.md → «Rutas públicas»](../../../context/specifications.md#rutas-públicas), [S11.5 → CA01–CA02](../s11.5-variantes-url-publicas/spec.md#criterios-de-aceptación) | Una ruta pública existente se solicita mediante una variante equivalente, incluida una ruta paginada cuando corresponda | Se envía la petición y se sigue la redirección | La respuesta conduce a la ruta canónica en minúsculas y con la forma de barra definida; la variante paginada conserva su página y su canonical corresponde al resultado servido. |
| CA04 | [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [specifications.md → «Paginación de las páginas públicas»](../../../context/specifications.md#paginación-de-las-páginas-públicas) | Se solicita una página de una colección pública con enlaces y distintos valores de `p` | Se recibe la respuesta HTTP | El contenido corresponde a la página resultante de interpretar `p` según la fuente y su canonical corresponde a esa página normalizada, incluida la primera página. |
| CA05 | [specifications.md → «Rutas públicas»](../../../context/specifications.md#rutas-públicas), [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento), [S11.1 → CA03](../s11.1-landing-publica/spec.md#criterios-de-aceptación) | Se solicita una página pública existente incluyendo la landing y una petición con un `Host` distinto del origen configurado | Se inspecciona el enlace canonical de la respuesta | La canonical usa el origen público configurado y la ruta canónica de la página; no se construye a partir del `Host` recibido. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para definir el alcance de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas](tasks.md) — propuesta; aprobación pendiente.
