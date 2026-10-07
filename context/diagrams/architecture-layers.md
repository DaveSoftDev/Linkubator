# Capas y dependencias

Vista derivada de [architecture.md → «Dependencias entre proyectos»](../architecture.md#dependencias-entre-proyectos). Los cuatro proyectos de producción forman el grafo productivo; las flechas continuas muestran sus referencias ordinarias y las discontinuas, una referencia restringida de composición o de verificación.

```mermaid
flowchart LR
    subgraph outer[Capas externas]
        Web["Web<br/>Razor Pages, Tailwind, autenticación y presentación"]
        Infrastructure["Infrastructure<br/>Dapper, SQLite; correo y hashing"]
    end

    subgraph core[Núcleo de la aplicación]
        Application["Application<br/>Casos de uso, DTO, validadores y puertos"]
        Domain["Domain<br/>Entidades, value objects e invariantes"]
    end

    subgraph verification[Verificación]
        Tests["Tests<br/>Pruebas unitarias, de integración y de dependencias"]
    end

    Web -->|Invoca casos de uso| Application
    Web -.->|Registro al arrancar| Infrastructure
    Application -->|Usa entidades y reglas| Domain
    Infrastructure -->|Implementa repositorios y puertos| Application
    Infrastructure -->|Persiste y traduce el modelo| Domain
    Tests -.-> Domain
    Tests -.-> Application
    Tests -.-> Infrastructure
    Tests -.-> Web
```
