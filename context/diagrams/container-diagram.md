# Diagrama de contenedores

Vista general de los contenedores prevista en [architecture.md](../architecture.md), derivada también de [requirements.md](../requirements.md). No representa una etapa de entrega concreta. Los contenedores describen unidades de ejecución; las capas internas de Clean Architecture no se dibujan como contenedores independientes.

```mermaid
flowchart LR
    user([Usuario autenticado])
    visitor([Visitante anónimo])
    browser["Navegador web<br/>Cliente de la aplicación"]

    subgraph scope["Sistema Linkubator"]
        web["Aplicación web<br/>ASP.NET Core Razor Pages, .NET 10 y C#<br/>Renderizado servidor, casos de uso e infraestructura"]
        database[("Base de datos local<br/>Archivo SQLite con FTS5")]
        web -->|SQL mediante Dapper| database
    end

    smtp["Servidor SMTP"]
    targetSites["Sitios web externos<br/>Destinos de los enlaces"]
    imageHosts["Servidores externos de imágenes"]

    user -->|Usa| browser
    visitor -->|Usa| browser
    browser -->|HTTPS: páginas públicas y zona privada| web
    web -.->|SMTP| smtp
    web -.->|HTTP para extraer metadatos| targetSites
    browser -->|Carga directa de imágenes públicas| imageHosts
```
