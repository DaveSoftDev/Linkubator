# Capas y dependencias

Vista derivada de [architecture.md → «Capas»](../architecture.md#capas). Las flechas indican dependencias de código: las capas externas pueden depender del núcleo, pero el dominio no depende de infraestructura ni de la interfaz web.

```mermaid
flowchart LR
    subgraph outer[Capas externas]
        Web["Web<br/>Razor Pages, Tailwind, identidad inyectada en MVP0, autenticación en MVP1 y presentación"]
        Infrastructure["Infrastructure<br/>Dapper, SQLite, logs; correo y hashing en MVP1"]
    end

    subgraph core[Núcleo de la aplicación]
        Application["Application<br/>Casos de uso, DTO, validadores y puertos"]
        Domain["Domain<br/>Entidades, value objects e invariantes"]
    end

    Web -->|Invoca casos de uso| Application
    Application -->|Usa entidades y reglas| Domain
    Infrastructure -->|Implementa repositorios y puertos| Application
    Infrastructure -->|Persiste y traduce el modelo| Domain
```

`Application` define las interfaces de repositorios, Unit of Work, hashing y correo; la interfaz del scraper se incorpora en MVP1. `Infrastructure` proporciona sus implementaciones. `Web` presenta y enruta las operaciones mediante los casos de uso, sin que `Domain` conozca HTTP, Razor Pages, SQLite ni Dapper.