# Guía de trabajo de Linkubator

## Estado actual

La documentación funcional fue aceptada el 2026-10-06. Está autorizada su implementación según el alcance y el orden de [roadmap.md](context/roadmap.md); las reglas de producto y las invariantes de esta guía siguen siendo obligatorias.

## Dónde vive cada cosa

La documentación funcional vive en `context/`. Cada documento responde a una sola pregunta:

| Documento | Responde a | No contiene |
|---|---|---|
| [`requirements.md`](context/requirements.md) | Qué hace el producto y para quién | Valores exactos, SQL, cabeceras, nombres de clases |
| [`specifications.md`](context/specifications.md) | Reglas exactas: algoritmos, límites, valores, mensajes y respuestas | Justificaciones |
| [`decisions.md`](context/decisions.md) | Qué se decidió y por qué, riesgos aceptados, pendientes y fuera de alcance | Especificaciones ni copias de reglas |
| [`architecture.md`](context/architecture.md) | Cómo se construye: plataforma, capas, persistencia, seguridad técnica | Reglas de negocio |
| [`domain-model.md`](context/domain-model.md) | Entidades, propiedades, relaciones e invariantes | Capas, formatos de almacenamiento, SEO |
| [`roadmap.md`](context/roadmap.md) | En qué orden se construye y cuándo termina cada etapa, checklist de calidad y backlog de MVP1 | Reglas ni historial de decisiones |

Principio de fuente única: **cada regla se escribe en un único documento, y los demás la enlazan sin repetirla.** Antes de añadir una regla, busca si ya existe. Si cambia una decisión, actualiza su documento propietario y revisa que los enlaces sigan siendo válidos.

La documentación es genérica para todo el producto, las únicas referencias a MVPs concretos se deben hacer en [decisions.md](/context/decisions.md) y en [roadmap.md](/context/roadmap.md). Fuera de él, no deben aparecer ninguna mención a MVPs específicos.


### Dónde va cada texto

Antes de escribir cualquier texto, clasifícalo. **Si no encaja en el documento que estás editando, no se escribe ahí:** se escribe en su documento propietario y, si hace falta, se enlaza desde el actual.

| Si el texto es… | Va en |
|---|---|
| Algo que el usuario puede hacer, ver o esperar del producto | [`requirements.md`](context/requirements.md) |
| Un valor, un límite, un algoritmo, un formato, una lista cerrada, un mensaje, un código de respuesta o el paso a paso de un flujo | [`specifications.md`](context/specifications.md) |
| El porqué de algo, una alternativa descartada, un riesgo aceptado, algo pendiente o algo fuera de alcance | [`decisions.md`](context/decisions.md) |
| Una tecnología, una capa, una transacción, una configuración o una medida técnica | [`architecture.md`](context/architecture.md) |
| Una entidad, una propiedad, una relación o una regla que los datos cumplen siempre | [`domain-model.md`](context/domain-model.md) |
| Una etapa, su orden, su criterio de finalización, un test de la checklist o un candidato para MVP1 | [`roadmap.md`](context/roadmap.md) |

Señales de que un texto está en el documento equivocado:

- Una capa, una tabla, una transacción o una librería en [requirements.md](context/requirements.md) o en [domain-model.md](context/domain-model.md).
- Un número, una lista cerrada o un código de respuesta fuera de [specifications.md](context/specifications.md).
- Un «porque», un «así se evita» o un «para que» que justifica una decisión, fuera de [decisions.md](context/decisions.md). Describir qué casos cubre una regla o qué consigue un mecanismo técnico sí está permitido.
- Qué puede hacer el usuario o cuándo algo es público escrito como regla en [architecture.md](context/architecture.md).
- Una lista de propiedades de una entidad fuera de [domain-model.md](context/domain-model.md). Nombrar una entidad o una propiedad para enlazarla sí está permitido.
- Una regla completa en [decisions.md](context/decisions.md) o en [roadmap.md](context/roadmap.md), en lugar de un enlace a ella.

Este mismo criterio se aplica a [AGENTS.md](AGENTS.md) y a las skills: no copian reglas del producto, salvo las invariantes críticas de la sección siguiente.

Los diagramas de `context/diagrams/` son vistas derivadas: muestran estructura y nombres (entidades, propiedades, estados, pasos y decisiones de un flujo) y pueden incluir valores de estándares o convenciones ampliamente adoptadas cuando ayuden a interpretarlos, como tipos de datos o códigos de estado HTTP. No deben introducir ni duplicar reglas propias de Linkubator; estas se definen en su documento propietario y se enlazan desde el diagrama. Que un valor sea estándar no convierte en estándar la decisión de Linkubator de usarlo en un caso concreto.

## Skills de Linkubator

Las skills del proyecto viven en `skills/`. Cada una contiene un archivo [`SKILL.md`](skills/requirements-review/SKILL.md) con su procedimiento y debe usarse cuando la tarea corresponda a su propósito.

- [requirements-review](skills/requirements-review/SKILL.md): revisa y sincroniza la documentación. Detecta contradicciones, duplicados, textos en el documento equivocado, omisiones, deriva del alcance del MVP0 y supuestos no documentados, y genera bajo demanda la ficha de una entidad. No genera código.

Cuando se cree una nueva skill de Linkubator, añádela aquí con su nombre, enlace y propósito.

## Invariantes críticas

Estas reglas no se pueden romper nunca, ni en la documentación ni en el código. Su detalle está en los documentos enlazados.

- Toda lectura o modificación privada se limita al usuario identificado, y nunca se confía en un `UserId`, `Alias` o `Slug` enviado por el cliente ([domain-model.md → «Propiedad de los datos»](context/domain-model.md#propiedad-de-los-datos)).
- Una colección privada nunca expone sus enlaces, y nada se hace público sin una acción explícita del usuario ([domain-model.md → «Público y privado»](context/domain-model.md#público-y-privado)).
- Las respuestas de login, registro, recuperación y cambio de email no revelan si una cuenta existe, ni por su contenido ni por su tipo de respuesta ([specifications.md → «Respuestas que no revelan si una cuenta existe»](context/specifications.md#respuestas-que-no-revelan-si-una-cuenta-existe)).
- Contraseñas y tokens nunca se guardan ni se registran en claro ([specifications.md → «Contraseñas»](context/specifications.md#contraseñas) y [«Tokens»](context/specifications.md#tokens)).
- Dentro de una transacción de base de datos nunca se espera al usuario, ni se hacen llamadas HTTP, scraping o envíos de correo ([architecture.md → «Persistencia»](context/architecture.md#persistencia)).

## Reglas de documentación

- Escribir en castellano.
- Respetar el principio de fuente única.
- Separar decisiones confirmadas de propuestas y preguntas abiertas.
- No presentar una tecnología como implementada si solo está planificada.
- Conservar las razones de las decisiones que afecten al alcance.
- No añadir funcionalidades fuera del MVP0 sin registrarlas como propuesta.
- Vocabulario de lo público: colecciones y enlaces solo son «públicos» o «privados», y las acciones son «hacer público» y «hacer privado». «Visible» e «invisible» solo se usan para la interfaz de usuario.
- En la documentación funcional se nombra «colección»; `Collection` es un nombre técnico.
- En la documentación funcional se nombra «etiqueta»; `Tag` y `LinkTag` son nombres técnicos.
- Referencias a secciones: usar enlaces Markdown con el nombre exacto del título y un solo nivel. A otro documento: `[specifications.md → «Sesión»](context/specifications.md#sesión)`. En el mismo documento: `[«Reglas de documentación»](#reglas-de-documentación)`. Varias secciones: enlazar cada sección por separado. La ruta relativa parte del documento que contiene la referencia. Por eso los títulos no se repiten dentro de un documento.

## Restricción de acceso de Copilot

Copilot no debe listar, buscar, leer ni modificar archivos dentro de ningún directorio cuyo nombre empiece por `.` en cualquier nivel del workspace. En búsquedas y operaciones recursivas, debe excluir siempre esos directorios. Si una tarea requiere acceder a uno de ellos, debe detenerse y explicárselo al usuario.

## Implementación autorizada

Se permite crear los proyectos, el código, el SQL de aplicación y migraciones, las dependencias y el frontend necesarios para implementar la etapa activa del [roadmap.md](context/roadmap.md), siguiendo la pila prevista en [architecture.md → «Plataforma»](context/architecture.md#plataforma). No se amplía por ello el alcance comprometido: las funciones reservadas a etapas posteriores y las propuestas siguen sujetas a [decisions.md](context/decisions.md) y [roadmap.md](context/roadmap.md). La configuración de despliegue continúa fuera del alcance.
