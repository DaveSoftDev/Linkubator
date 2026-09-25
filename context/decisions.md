# Decisiones acerca de Linkubator

## Confirmadas

### Producto

- Linkubator gestionará enlaces, colecciones y tags.
- El producto está preparado conceptualmente para múltiples usuarios.
- El MVP0 no tendrá registro, login ni autenticación.
- Se utilizará un usuario fijo configurable.
- El MVP0 se ejecutará únicamente en local.
- Las páginas públicas conservarán fundamentos SEO verificables localmente, pero no podrán ser rastreadas por buscadores hasta un despliegue público posterior.
- La interfaz inicial estará en castellano.

### Usuario

- El usuario tendrá nombre, alias, email y contraseña hasheada.
- El alias será único globalmente.
- El alias tendrá entre 10 y 25 caracteres.
- Solo permitirá caracteres ASCII básicos, números y guiones.
- Se almacenará en minúsculas.
- No podrá cambiarse.
- `collections`, `users` y `tags` o sus singulares serán palabras reservadas.

### Colecciones y enlaces

- Cada enlace pertenece a una única colección.
- Las colecciones serán públicas o privadas.
- El slug de la colección será único dentro del usuario, nunca global.
- El slug de la colección será autogenerado por el sistema a partir del nombre de la colección y seguirá las reglas del alias de usuario, pero con una longitud máxima de 50 caracteres.
- El slug de la colección podrá cambiarse si el nuevo valor sigue siendo válido y único dentro del usuario.
- El slug de la colección será invisible al usuario
- Una colección con enlaces no podrá eliminarse; solo se eliminarán colecciones vacías.
- La visibilidad de un enlace depende de la colección y de su publicación individual.
- Un enlace no publicado no aparece en una colección pública.
- Un enlace de una colección privada nunca aparece públicamente.
- Una colección privada que vuelva a ser pública no publicará automáticamente sus enlaces.
- Por defecto, al crear el enlace, la marca de visible públicamente será establecida a como tenga establecida la colección esa misma propiedad.
- El usuario decidirá individualmente qué enlaces publica.
- Al mover un enlace entre colecciones, se establecerá `IsPublic = false` y será necesaria una nueva publicación explícita.
- La URL del enlace será inmutable; para cambiarla se eliminará el enlace y se creará de nuevo.
- Solo se podrán editar los metadatos y el estado de publicación del enlace.
- Los enlaces individuales no tendrán páginas indexables.
- Solo las páginas públicas de colección serán indexables.

### URLs

- Se conservará la URL original introducida por el usuario.
- Se almacenará una URL normalizada auxiliar para detectar duplicados, esta url no será visible por el usuario.
- No se seguirán redirecciones para detectar duplicados.
- No se reemplazará la URL original por la URL final.
- No se permitirá la misma URL normalizada dos veces para el mismo usuario.

### Datos y tecnología

- .NET 10.
- C#.
- ASP.NET Core Razor Pages.
- Tailwind CSS.
- SQLite con FTS5.
- Dapper.
- No se utilizará Entity Framework Core.
- Se aplicarán Clean Architecture, SOLID, DRY y YAGNI.
- Se utilizarán los patrones Repository, Unit of Work y Result.
- El borrado será físico.
- No habrá importación ni exportación en el MVP0.
- `LinkTag` y la proyección FTS5 se actualizarán dentro de la misma transacción que cada operación de escritura o borrado relacionada.
- FTS5 se utilizará para búsqueda textual; los filtros estructurados usarán SQL e índices convencionales.
- Las operaciones de SQLite seguirán un ciclo breve: abrir conexión, ejecutar lo imprescindible, confirmar o revertir y liberar recursos.
- No habrá llamadas HTTP, scraping, esperas de usuario ni reintentos externos dentro de transacciones.
- SQLite se configurará con WAL, `busy_timeout` y claves foráneas activadas.
- Las consultas y transacciones se optimizarán para minimizar la duración del bloqueo de escritura.

### Listados y operación

- Las colecciones públicas se ordenarán por `CreatedAt` descendente.
- Se mostrarán 15 enlaces por página.
- Se generarán logs desde el inicio.
- Grafana, Kibana y plataformas similares quedan fuera de ámbito por ahora.
- La aplicación deberá ser accesible y compatible con los navegadores actuales.
- Los logs serán agnósticos y no incluirán secretos, contraseñas, hashes innecesarios ni credenciales contenidas en URLs.

### SEO

- El SEO se abordará por fases.
- Las rutas, el alias de usuario, el slug de colección, la visibilidad, el SSR y el HTML semántico se definirán desde el inicio.
- Solo las colecciones públicas serán indexables.
- El refinamiento de Open Graph, Twitter Cards, sitemap, robots, datos estructurados y auditoría de rastreabilidad se completará en una fase posterior.

## Pendientes

- Reglas exactas de normalización de URL.
- Lista completa de parámetros de marketing que se ignorarán.
- Tratamiento de barra final y orden de parámetros.
- Diferencias entre HTTP y HTTPS en duplicados.
- Timeout exacto del scraper.
- Redirecciones permitidas.
- Tamaño máximo de respuesta.
- Tipos de contenido aceptados.
- Estrategia de despliegue.
- Diseño detallado del scraping y su contrato de ejecución en segundo plano.

## Fuera de alcance del MVP0

- Registro y autenticación.
- Login funcional.
- Recuperación de contraseña.
- Cambio de alias.
- Importación de marcadores.
- Exportación de marcadores.
- Extensión de navegador.
- Colaboración entre usuarios.
- Crawling completo.
- Sincronización con servicios externos.
- Analítica avanzada.
- Plataforma externa de observabilidad.
- SEO avanzado.
