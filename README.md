# Linkubator

Aplicación web para la gestión de enlaces en colecciones y etiquetas.

## Estado del proyecto

Actualmente el repositorio contiene únicamente la documentación inicial del producto. No se ha generado código de aplicación ni infraestructura técnica.

## Visión

Linkubator permitirá que cada usuario gestione sus propios enlaces, los organice en colecciones y etiquetas, y obtenga automáticamente los metadatos principales de sus enlaces:

- Título.
- Descripción.
- Imagen Open Graph.

Las colecciones podrán ser públicas o privadas. Dentro de una colección pública, el usuario decidirá individualmente qué enlaces aparecen en su página pública indexable por buscadores.

## Documentación

- [Requisitos de Linkubator](context/requirements.md)
- [Modelo de dominio de Linkubator](context/domain-model.md)
- [Especificaciones de Linkubator](context/specifications.md)
- [Arquitectura prevista para Linkubator](context/architecture.md)
- [Decisiones, riesgos, pendientes y fuera de alcance](context/decisions.md)
- [Roadmap de Linkubator](context/roadmap.md)
- [Guía para agentes y colaboradores](AGENTS.md)
- [Skill de revisión de requisitos](skills/requirements-review/SKILL.md)

## Alcance tecnológico previsto

- .NET 10 y C#.
- ASP.NET Core Razor Pages.
- Tailwind CSS.
- SQLite con FTS5.
- Dapper como micro ORM (Object-Relational Mapper).
- Argon2id para el hashing de contraseñas.
- MailKit y smtp4dev (local) para el correo.
- Clean Architecture.

La estrategia de despliegue se decidirá más adelante.

## Enlaces para Brais Moure
- 
-  