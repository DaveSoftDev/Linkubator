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

## Reglas de producto

- El producto está preparado conceptualmente para múltiples usuarios.
- El MVP0 usa un usuario fijo configurable y no implementa registro, login ni autenticación.
- Toda lectura o modificación privada debe quedar limitada al usuario actual.
- Nunca se debe confiar en un `UserId` enviado por el cliente.
- Una colección privada nunca expone sus enlaces.
- En una colección pública, cada enlace requiere publicación explícita.
- Al volver pública una colección privada, sus enlaces siguen sin publicarse.
- Los enlaces individuales no tendrán páginas indexables.
- Solo las colecciones públicas tendrán páginas indexables.

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

## Restricciones de edición actuales

No crear todavía:

- Proyectos .NET.
- Código C#.
- SQL de aplicación o migraciones ejecutables.
- Paquetes o archivos de dependencias.
- Frontend funcional.
- Configuración de despliegue.

La siguiente etapa de implementación debe comenzar solo después de revisar y aceptar la documentación del MVP0.
