# Arquitectura prevista

## Plataforma

- .NET 10.
- C#.
- ASP.NET Core Razor Pages.
- Renderizado del lado servidor.
- Tailwind CSS.
- SQLite.
- Dapper.

La estrategia de despliegue se decidirá más adelante.

## Capas

### Domain

Responsabilidades:

- Entidades.
- Value Objects.
- Invariantes.
- Reglas de negocio.
- Errores de dominio.

No conocerá SQLite, Dapper, HTTP, Razor Pages ni detalles de infraestructura.

### Application

Responsabilidades:

- Casos de uso.
- DTOs.
- Validadores.
- Interfaces de repositorios.
- Interfaces de Unit of Work.
- Patrón `Result`.
- Abstracción del usuario actual.
- Abstracción del scraper.

Casos de uso iniciales:

- Crear, editar y eliminar colecciones.
- Cambiar la visibilidad de una colección.
- Crear, editar y eliminar enlaces.
- Comprobar duplicados.
- Publicar y despublicar enlaces.
- Mover enlaces entre colecciones.
- Crear, editar y eliminar tags.
- Asociar y desasociar tags.
- Buscar enlaces.
- Procesar metadatos.
- Reintentar scraping.

### Infrastructure

Responsabilidades:

- Implementación de repositorios con Dapper.
- SQL explícito.
- Conexiones SQLite.
- Transacciones.
- Unit of Work ligero.
- Scripts SQL versionados.
- Índices y restricciones.
- SQLite FTS5.
- Scraper HTTP.
- Trabajos persistidos o estado equivalente para scraping.
- Implementación del usuario fijo del MVP0.

No se utilizará Entity Framework Core.

### Web

Responsabilidades:

- Razor Pages.
- Tailwind.
- Panel de gestión.
- Páginas públicas.
- Routing.
- SEO.
- Validación antiforgery.
- Resolución del usuario actual.
- Presentación de errores y estados accesibles.

## SEO por fases

El SEO no se pospondrá por completo hasta el final porque algunas decisiones afectan a la arquitectura y al modelo de publicación.

Desde el inicio deben quedar definidos:

- Rutas públicas con `UserAlias` y slug de colección.
- Renderizado SSR mediante Razor Pages.
- Separación entre colecciones públicas y privadas.
- Publicación individual de enlaces dentro de colecciones públicas.
- Exclusión de enlaces no publicados.
- HTML semántico con títulos, descripciones, encabezados y canonical básica.
- Respuestas `404` para recursos públicos inexistentes o no accesibles.
- Paginación estable de 15 enlaces.

En una fase posterior se completarán:

- Open Graph y Twitter Cards.
- `robots.txt`.
- `sitemap.xml`.
- Datos estructurados, si procede.
- Revisión de rastreabilidad, paginación indexable y metadatos reales.

## Persistencia

SQLite se utilizará mediante `Microsoft.Data.Sqlite` y Dapper.

El Unit of Work será ligero y gestionará una conexión y una transacción explícita. Las operaciones que afecten a varias tablas deberán ejecutarse de forma atómica.

Las migraciones se resolverán mediante scripts SQL versionados o una herramienta ligera equivalente. No se introducirá EF Core para resolver este problema.

Restricciones e índices previstos:

- Alias único global.
- Slug único por usuario.
- URL normalizada única por usuario.
- Índices por usuario, colección, publicación y fecha.
- Relación `LinkTag` con pareja única.

## FTS5

El índice de búsqueda será una proyección desnormalizada que podrá incluir:

- URL original.
- Título.
- Descripción.
- Nombre de colección.
- Nombres de tags.

La actualización del índice deberá ser transaccional con las operaciones de alta, edición, movimiento, etiquetado y borrado físico.

La consulta de búsqueda siempre aplicará el usuario actual. FTS5 no debe convertirse en una vía para devolver resultados de otro usuario.

La implementación deberá contemplar:

- Tokenización adecuada para castellano.
- Escape de términos de consulta.
- Ranking.
- Paginación estable.
- Reconstrucción o reparación del índice.

## Scraping

El scraping se ejecutará fuera de la petición principal mediante `BackgroundService` y trabajos persistidos o un mecanismo equivalente.

Plan de reintentos:

- Intento inicial.
- Hasta dos reintentos adicionales.
- Aproximadamente 20 segundos entre intentos.

Medidas obligatorias:

- Protección SSRF.
- Solo protocolos permitidos.
- Validación de destino antes y durante las redirecciones.
- Timeout.
- Límite de bytes descargados.
- Tipos de contenido permitidos.
- No sustituir la URL original por una redirección.
- Sanitizar metadatos extraídos.

## Rutas públicas

Formato:

```text
/user-alias/collections/collection-slug
```

La resolución debe comprobar:

1. Que el alias existe.
2. Que la colección pertenece a ese usuario.
3. Que el slug coincide dentro de ese usuario.
4. Que la colección es pública.
5. Que el enlace está publicado antes de mostrarlo.

## Calidad y accesibilidad

La interfaz deberá ser accesible desde el diseño:

- HTML semántico.
- Labels asociados.
- Orden de tabulación lógico.
- Foco visible.
- Errores asociados y anunciables.
- Contraste suficiente.
- No depender solo del color.
- Navegación por teclado.
- Soporte para lectores de pantalla.
- `prefers-reduced-motion`.
- Pruebas en navegadores actuales.

## Logs

Se registrarán logs para operaciones relevantes, errores de persistencia, scraping, reintentos, SSRF, validación, búsqueda y publicación.

No se añadirá todavía una plataforma como Grafana o Kibana.
