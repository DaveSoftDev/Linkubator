# Arquitectura prevista

## Plataforma

- .NET 10.
- C#.
- ASP.NET Core Razor Pages.
- Renderizado del lado servidor.
- Tailwind CSS.
- SQLite.
- Dapper.

El MVP0 se ejecutará únicamente en local. La protección del panel privado se basa en el límite de confianza del entorno local y en el contexto de usuario fijo; no se diseñará todavía una solución de autenticación para exposición pública.

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

- Crear y editar colecciones.
- Eliminar una colección solo cuando no tenga enlaces.
- Cambiar la visibilidad de una colección.
- Crear enlaces y editar únicamente sus metadatos y publicación.
- Comprobar duplicados.
- Publicar y despublicar enlaces.
- Mover enlaces entre colecciones.
- Crear, editar y eliminar tags.
- Asociar y desasociar tags.
- Buscar enlaces.
- Procesar metadatos.
- Reintentar scraping.

Los casos de uso de scraping quedan documentados como capacidad futura y no forman parte de la primera implementación mientras el diseño permanezca aplazado.

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

El diseño detallado del scraping queda aplazado y no debe considerarse cerrado hasta definir su contrato, límites y protección SSRF.

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

### Reglas operativas de acceso a datos

La interacción con SQLite seguirá siempre este ciclo:

1. Abrir la conexión.
2. Iniciar la transacción solo cuando sea necesaria.
3. Ejecutar las operaciones mínimas de la unidad de trabajo.
4. Confirmar o revertir inmediatamente.
5. Liberar la transacción, la conexión y los recursos asociados.

Reglas obligatorias:

- Las transacciones serán lo más cortas posible.
- No se esperará al usuario dentro de una transacción.
- No se harán llamadas HTTP, scraping ni operaciones externas dentro de una transacción.
- No se esperarán reintentos ni intervalos de tiempo dentro de una transacción.
- `LinkTag` y FTS5 se actualizarán dentro de la misma transacción que el cambio de datos relacionado.
- Las operaciones de lectura no mantendrán transacciones abiertas innecesariamente.
- Las consultas paginadas no cargarán en memoria más datos de los necesarios.
- Las restricciones únicas de SQLite serán la garantía final frente a duplicados y colisiones.

SQLite se configurará para el escenario local mediante WAL, `busy_timeout` y claves foráneas activadas. Los reintentos por bloqueo serán limitados y solo se aplicarán a errores transitorios identificados.

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

Cada operación de escritura o borrado de base de datos que afecte a enlaces, colecciones o tags deberá actualizar también la proyección FTS5 y las relaciones `LinkTag` que correspondan, dentro de la misma transacción.

La consulta de búsqueda siempre aplicará el usuario actual. FTS5 no debe convertirse en una vía para devolver resultados de otro usuario.

La proyección contendrá `UserId` asociado a cada entrada y la consulta combinará `MATCH` con el filtro obligatorio del usuario actual. FTS5 se reservará para texto libre; los filtros por colección, tag, publicación y fechas usarán SQL e índices.

La implementación deberá contemplar:

- Tokenización adecuada para castellano.
- Escape de términos de consulta.
- Ranking.
- Paginación estable.
- Reconstrucción o reparación del índice.

## Scraping

El diseño detallado de esta parte queda aplazado. Los requisitos funcionales conservan la intención de obtener los metadatos del enlace, pero no se implementará ni se cerrará su contrato hasta una revisión específica.

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

Una colección privada y una colección inexistente producirán la misma respuesta pública `404`, sin revelar si la colección privada existe.

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

Se registrarán logs agnósticos para operaciones relevantes, errores de persistencia, scraping, reintentos, SSRF, validación, búsqueda y publicación. Se evitará registrar secretos, contraseñas, hashes innecesarios y credenciales incluidas en URLs.

No se añadirá todavía una plataforma como Grafana o Kibana.
