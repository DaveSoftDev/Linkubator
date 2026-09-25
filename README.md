# Linkubator

Aplicación web para la gestión de enlaces, colecciones y etiquetas.

## Estado del proyecto

Actualmente el repositorio contiene únicamente la documentación inicial del producto. No se ha generado código de aplicación ni infraestructura técnica.

## Visión

Linkubator permitirá que cada usuario gestione sus propios enlaces, los organice en colecciones y etiquetas, y obtenga automáticamente sus metadatos principales:

- Título.
- Descripción.
- Imagen Open Graph.

Las colecciones podrán ser públicas o privadas. Dentro de una colección pública, el usuario decidirá individualmente qué enlaces aparecen en su página pública indexable.

## Documentación

- [Requisitos del producto](context/requirements.md)
- [Modelo de dominio](context/domain-model.md)
- [Arquitectura prevista](context/architecture.md)
- [Decisiones confirmadas y pendientes](context/decisions.md)
- [Roadmap del MVP0](context/roadmap.md)
- [Guía para agentes y colaboradores](AGENTS.md)
- [Skill de revisión de requisitos](skills/requirements-review/SKILL.md)

## Alcance tecnológico previsto

- .NET 10 y C#.
- ASP.NET Core Razor Pages.
- Tailwind CSS.
- SQLite con FTS5.
- Dapper, sin Entity Framework Core.
- Clean Architecture.

La estrategia de despliegue se decidirá más adelante.
