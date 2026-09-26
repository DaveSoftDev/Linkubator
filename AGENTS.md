# Guía de trabajo de Linkubator

## Estado actual

Este repositorio se encuentra en fase de definición de requisitos. Hasta que se indique expresamente lo contrario, el trabajo debe centrarse en documentación y análisis, sin generar código de aplicación.

## Fuente de verdad

La documentación funcional vive en `context/`:

- `requirements.md`: requisitos funcionales y no funcionales.
- `domain-model.md`: entidades, propiedades, relaciones e invariantes.
- `architecture.md`: arquitectura técnica prevista y límites entre capas.
- `decisions.md`: decisiones confirmadas, pendientes y fuera de alcance.
- `roadmap.md`: fases de entrega y criterios de finalización.

Si una conversación cambia una decisión, actualiza primero la documentación afectada y mantén consistencia entre estos documentos.

## Skills de Linkubator

Las skills específicas del proyecto viven en `skills/`. Cada una contiene un `SKILL.md` con su procedimiento y debe utilizarse cuando la tarea corresponda a su propósito.

- [requirements-review](skills/requirements-review/SKILL.md): revisar y sincronizar los documentos de requisitos, modelo de dominio, arquitectura, decisiones y roadmap. Detecta contradicciones, omisiones, deriva del alcance del MVP0 y supuestos no documentados. No genera código.

Cuando se cree una nueva skill de Linkubator, debe añadirse aquí con su nombre, enlace y propósito principal.

## Reglas de producto

- El producto está preparado conceptualmente para múltiples usuarios.
- Se debe implementar un registro de usuarios, login y autenticación.
- Toda lectura o modificación privada debe quedar limitada al usuario actual.
- Nunca se debe confiar en un `UserId` o `Alias` enviado por el cliente.
- Una colección privada nunca expone sus enlaces.
- Todo enlace nuevo comienza como privado (`IsPublic = false`) y requiere publicación explícita por parte del usuario.
- Al volver pública una colección privada, sus enlaces siguen sin publicarse.
- Los enlaces individuales no tendrán páginas indexables.
- Solo la página de usuario y las páginas de colecciones públicas serán indexables.
- El alias del usuario es visible, mutable y único globalmente.
- `User.Alias`, `Collection.Slug` y `Tag.Slug` se generan con la misma transformación explicada en decisions.md (Generación de alias y slugs).
- El slug de colección y el de etiqueta se genera desde el nombre, es invisible e inmutable y es único dentro del usuario.
- El scraping forma parte del MVP0, pero se implementa en una fase posterior con flujo provisional de intento inicial y reintentos a 5 minutos.
- En la documentación funcional se usa “etiqueta”; `Tag` y `LinkTag` son nombres técnicos.
- El MVP0 se ejecuta únicamente en local.
- Una colección con enlaces no se puede borrar.
- La URL de un enlace es inmutable después de su creación.
- Al mover un enlace de colección, queda despublicado y requiere publicación explícita por parte del usuario.
- `UrlNormalized` es una clave plana de comparación.
- `LinkTag` y FTS5 se actualizan dentro de la misma transacción que las operaciones de escritura o borrado relacionadas.
- Las transacciones deben ser breves: abrir, ejecutar lo imprescindible, confirmar o revertir y liberar.
- Nunca realizar scraping, llamadas HTTP ni esperas de usuario dentro de una transacción.

## Reglas de documentación

- Escribir en castellano.
- Separar decisiones confirmadas de propuestas y preguntas abiertas.
- No presentar una tecnología como implementada si solo está planificada.
- Conservar las razones de las decisiones que afecten al alcance.
- No añadir funcionalidades fuera del MVP0 sin registrarlas como propuesta.

## Restricciones técnicas previstas

- .NET 10 y C#.
- ASP.NET Core Razor Pages con renderizado del lado servidor.
- Tailwind CSS.
- SQLite con FTS5.
- Dapper y SQL explícito; no Entity Framework Core.
- Unit of Work ligero con transacciones.
- Clean Architecture, SOLID, DRY, YAGNI y patrón Result.
- FTS5 para búsqueda textual; SQL e índices convencionales para filtros estructurados.
- SQLite con WAL, `busy_timeout` y claves foráneas activadas.

## Restricciones de edición actuales

No crear todavía:

- Proyectos .NET.
- Código C#.
- SQL de aplicación o migraciones ejecutables.
- Paquetes o archivos de dependencias.
- Frontend funcional.
- Configuración de despliegue.

La siguiente etapa de implementación debe comenzar solo después de revisar y aceptar la documentación del MVP0.
