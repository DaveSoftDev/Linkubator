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
| [`architecture.md`](context/architecture.md) | Cómo se construye: tecnologías, arquitectura , persistencia, seguridad técnica | Reglas de negocio |
| [`domain-model.md`](context/domain-model.md) | Entidades, propiedades, relaciones e invariantes | Capas, formatos de almacenamiento, SEO |
| [`roadmap.md`](context/roadmap.md) | En qué orden se construye y cuándo termina cada etapa, checklist de calidad y backlog de MVP1 | Reglas ni historial de decisiones |
| [`specs/`](specs/) | Cómo se implementa cada subsistema: diseño técnico, estructura interna, ejecución de flujos y trazabilidad | Reglas del producto, límites funcionales, mensajes exactos, requisitos de usuario ni decisiones de negocio en sí mismas |

Principio de fuente única: **cada regla se escribe en un único documento, y los demás la enlazan sin repetirla.** Antes de añadir una regla, busca si ya existe. Si cambia una decisión, actualiza su documento propietario y revisa que los enlaces sigan siendo válidos.

La carpeta [`specs/`](specs/) es documentación técnica derivada: explica cómo se implementa cada subsistema, pero siempre enlaza a la regla fuente en `context/` y nunca repite sus requisitos ni sus decisiones de negocio.

La documentación es genérica para todo el producto, las únicas referencias a MVPs concretos se deben hacer en [README.md](README.md), [decisions.md](context/decisions.md) y en [roadmap.md](context/roadmap.md), las especificaciones (Carpeta [`specs/`](specs/)) y el [Plan para afrontar el MVP0](plans/mvp0-plan.md). Fuera de él, no deben aparecer ninguna mención a MVPs específicos.


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
| Un diseño de implementación: componentes, dependencias, flujos internos, errores de infraestructura, SQL y trazabilidad técnica | [`specs/`](specs/) |

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

Las skills del proyecto viven en `skills/`. Cada una contiene un archivo [`SKILL.md`](skills/requirements-review/SKILL.md) con su procedimiento y debe usarse cuando la tarea corresponda a su propósito. Al crear o modificar una skill del proyecto, usa [skill-authoring](skills/skill-authoring/SKILL.md).

- [skill-authoring](skills/skill-authoring/SKILL.md): define cómo crear, actualizar y validar skills de Linkubator, adaptando patrones externos a las convenciones del repositorio y de Copilot.
- [linkubator-guidelines](skills/linkubator-guidelines/SKILL.md): aplica guías conductuales al escribir, revisar o refactorizar código, con foco en simplicidad, cambios quirúrgicos, supuestos explícitos y criterios de éxito verificables.
- [project-structure](skills/project-structure/SKILL.md): organiza nuevas o existentes capas del proyecto por responsabilidad y evita que tipos se queden en la raíz o mezclados por conveniencia temporal. Aplica la convención reutilizable de `Contracts`, `Results`, `Enums`, `Requests`, `Models`, `Constants` y `UseCases`.
- [requirements-review](skills/requirements-review/SKILL.md): revisa y sincroniza la documentación. Detecta contradicciones, duplicados, textos en el documento equivocado, omisiones, deriva del alcance del MVP0 y supuestos no documentados, y genera bajo demanda la ficha de una entidad. No genera código.
- [sdd-writer](skills/sdd-writer/SKILL.md): prepara especificación, plan técnico y tareas bajo Spec-Driven Development; verifica cierre y archiva trabajos aceptados con evidencia y enlaces trazables. No introduce reglas de producto ni implementa código.

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

## Implementación autorizada

Se permite crear los proyectos, el código, el SQL de aplicación y migraciones, las dependencias y el frontend necesarios para implementar la etapa activa del [roadmap.md](context/roadmap.md), siguiendo la pila prevista en [architecture.md → Arquitectura](context/architecture.md#arquitectura). No se amplía por ello el alcance comprometido: las funciones reservadas a etapas posteriores y las propuestas siguen sujetas a [decisions.md](context/decisions.md) y [roadmap.md](context/roadmap.md). La configuración de despliegue continúa fuera del alcance.

## Referencia de diseño Web

- El diseño existente de gestión de colecciones es la referencia obligatoria para los nuevos desarrollos Web: [formularios CRUD](src/Linkubator.Web/Pages/App/Collections/) y [tabla de listado](src/Linkubator.Web/Pages/App/Collections.cshtml).
- Antes de preparar un plan técnico o implementar formularios y listados, revisar esas referencias y reutilizar sus patrones de estructura, estilos, campos, acciones, errores, búsqueda, paginación y estados vacíos, cuando sean aplicables.
- Adaptar únicamente los campos y comportamientos propios de cada funcionalidad. La referencia es visual y de interacción: no copiar indiscriminadamente lógica, validaciones ni reglas de negocio.
- Reutilizar los componentes y las clases CSS existentes; no duplicar estilos ni extraer nuevas abstracciones salvo que sean necesarias.
- No proponer ni implementar un diseño alternativo, ni modificar el diseño de referencia, sin consultarlo previamente con el usuario.

## Convención de estructura del código

- Cada tipo público debe vivir en su propio archivo.
- Los tipos no deben acumularse en archivos raíz dentro de una capa; se organizan por carpeta y responsabilidad: `Contracts/`, `Results/`, `Enums/`, `Models/`, `Dtos/Requests/`, `Dtos/Models/`, `Validation/`, `UseCases/`, `Services/` y así sucesivamente.
- Las carpetas deben reflejar el dominio y no dejar tipos sueltos que luego se reubiquen por refactor.
- Cuando una capa ya tiene una estructura clara, se mantiene ese patrón y no se crean archivos generales de “todo en uno” salvo el punto de entrada del proyecto.
- El patrón de Domain es referencia para Application: módulos por responsabilidad y un archivo por objeto o servicio.
- `Contracts/` debe contener solo interfaces y contratos de capa; no se guardan aquí `Result`, `ApplicationError`, enums ni DTOs concretos.
- `Results/` contiene tipos de salida de la aplicación: `Result`, `Result<T>`, `ApplicationError` y cualquier wrapper compartido de resultado/errores.
- `Enums/` contiene únicamente enums de la capa, como `ApplicationErrorCategory`.
- `Dtos/Requests/` contiene solo DTOs de entrada: peticiones que llegan al caso de uso.
- `Dtos/Models/` o `Models/` contiene representaciones de salida o modelos de dominio/aplicación, no datos de entrada. Se usa para DTOs que devuelven un valor al caso de uso o a otra capa.
- `Responses/` solo debe usarse cuando el proyecto está modelando explícitamente la respuesta HTTP o API, no como contenedor genérico para todo lo que devuelve un servicio. En esta capa, si no existe una respuesta HTTP con ese formato, preferimos `Models/`.
- `Constants/` se usa para literales, mensajes, códigos, constantes de error y fábricas de error concretas; no para tipos de lógica ni contratos.
- La regla de clasificación debe ser siempre: si el tipo describe una operación o dependencia, va a `Contracts`; si describe un valor de resultado, va a `Results` o `Models`; si describe un conjunto cerrado de valores, va a `Enums`; si describe un dato de entrada, va a `Requests`.
- En `Constants/`, los errores genéricos y repetitivos se centralizan en un `CommonErrors` o equivalente; los errores específicos del dominio se quedan en su clase de dominio. Si un error se repite en dos o más casos de uso y su semántica es la misma, se extrae; si el código o el mensaje dependen del dominio concreto, permanecen en esa clase.
