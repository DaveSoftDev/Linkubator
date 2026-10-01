# Diagrama de contenedores

Vista derivada de la arquitectura prevista. Los contenedores describen unidades de ejecución; las capas internas de Clean Architecture no se dibujan como contenedores independientes.

```mermaid
flowchart LR
    registered([Usuario registrado])
    visitor([Visitante anónimo])
    browser["Navegador web<br/>Cliente de la aplicación"]

    subgraph scope["Sistema Linkubator (MVP0 local)"]
        web["Aplicación web<br/>ASP.NET Core Razor Pages, .NET 10 y C#<br/>Renderizado servidor, casos de uso e infraestructura<br/>Incluye BackgroundService de correo"]
        database[("Base de datos local<br/>Archivo SQLite con FTS5")]
        web -->|SQL mediante Dapper| database
    end

    smtp["smtp4dev<br/>Servidor SMTP y visor local de correo"]
    targetSites["Sitios web externos<br/>Destinos de los enlaces"]
    imageHosts["Servidores externos de imágenes"]

    registered -->|Usa| browser
    visitor -->|Usa| browser
    browser -->|HTTPS local: páginas públicas y zona privada| web
    web -->|SMTP: envía correos en segundo plano| smtp
    browser -->|HTTP local: consulta los correos capturados| smtp
    web -.->|HTTP para extraer metadatos; fase posterior| targetSites
    browser -->|Carga directa de imágenes públicas| imageHosts
```

## Notas

- La aplicación web, el almacenamiento SQLite y la integración SMTP son elementos previstos; este diagrama no afirma que estén implementados.
- El `BackgroundService` de correo se aloja en el proceso de la aplicación web. La cola de correo es en memoria, no un contenedor ni un servicio de colas separado.
- FTS5 forma parte del almacenamiento SQLite. Las capas Domain, Application, Infrastructure y Web son divisiones internas del código, no contenedores ejecutables separados.
- `smtp4dev` captura los mensajes en local y los presenta en su interfaz web; no entrega correo real.
- El scraper HTTP se implementará en una fase posterior. Su diseño sigue teniendo decisiones pendientes, por lo que la relación discontinua indica una integración futura.
- El navegador descarga directamente las imágenes de sus servidores de origen; Linkubator no las sirve mediante un proxy.