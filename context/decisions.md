# Decisiones acerca de Linkubator

## Confirmadas

### Producto

- Linkubator gestionará enlaces, colecciones y etiquetas.
- `Tag` y `LinkTag` se reservarán para nombres técnicos.
- El producto debe estar preparado para múltiples usuarios.
- El MVP0 se ejecutará únicamente en local.
- Las páginas públicas conservarán fundamentos SEO verificables localmente, pero no podrán ser rastreadas por buscadores hasta un despliegue público posterior.
- La interfaz inicial estará en castellano.

### Usuario

- El usuario tendrá nombre, alias, email, contraseña hasheada y fecha de creación.
- El email será único globalmente.
- El alias será visible, mutable y único globalmente.
- El alias tendrá entre 10 y 25 caracteres después de su generación.
- Las palabras reservadas serán:
  - `collection`
  - `collections`
  - `user`
  - `users`
  - `tag`
  - `tags`
- Si el alias cambia, su alias antiguo devolverá `404` en el MVP0.
- Las redirecciones de alias `301` quedan como propuesta futura fuera de éste MVP0.
- Deberá existir una página para editar la configuración del usuario (nombre, email, alias y contraseña).
- El cambio de contraseña se realizará desde esa pantalla mediante un botón que lleva a otra página con tres campos enmascarados (actual, nueva y confirmación de la nueva).
- La contraseña tendrá entre 10 y 50 caracteres, con texto libre (cualquier carácter, incluidos espacios, acentos y símbolos), sin exigir una combinación obligatoria de mayúsculas, minúsculas, números o símbolos. El usuario es responsable de la fortaleza de la contraseña elegida.
- Se deberá de permitir al usuario recuperar la contraseña, por si la ha olvidado. Para éste MVP0, se le enviará directamente a una pantalla dónde podrá introducir su email y su nueva contraseña dos veces (Para asegurar que la ha escrito correctamente).

### Generación de alias y slugs

`User.Alias`, `Collection.Slug` y `Tag.Slug` se generarán mediante la misma transformación:

1. Convertir el texto a minúsculas.
2. Eliminar los acentos de las vocales, conservando la letra base: `Generación` se convierte en `generacion`.
3. Sustituir `ç` por `c` y `ñ` por `n`: `caça` se convierte en `caca` y `España` en `espana`.
4. Sustituir los espacios por guiones medios.
5. Conservar letras, números y guiones medios.
6. Eliminar cualquier otro carácter.
7. Eliminar guiones medios duplicados.
8. Eliminar guiones medios al principio y al final.

Ejemplos:

```text
Saltó la raña al charço!!! I luego, croo
-> salto-la-rana-al-charco-i-luego-croo
```

Si el resultado de la transformación queda vacío, la operación se rechazará.

### Colecciones, etiquetas y enlaces

- Cada enlace pertenece a una única colección.
- Las colecciones serán públicas o privadas.
- El slug de la colección será único dentro del usuario, nunca global.
- El slug de la colección se autogenerará desde el nombre.
- El slug de la colección seguirá las reglas comunes de generación.
- El slug de la colección tendrá entre 1 y 50 caracteres después de su generación.
- El slug de la colección será invisible para el usuario.
- El slug de la colección será inmutable.
- Si el slug generado colisiona dentro del usuario, la operación se rechazará.
- No se añadirán sufijos ni prefijos automáticamente para resolver colisiones.
- El `Name` de una colección es único dentro del usuario, como consecuencia directa de la unicidad de su `Slug` generado.
- Una colección puede existir sin tener enlaces asociados.
- Una colección con enlaces no podrá eliminarse, sólo se podrán eliminar colecciones vacías.
- El slug técnico de las etiquetas seguirá las mismas reglas de generación que el slug de colección: automático desde el nombre, invisible, inmutable y con longitud entre 1 y 50 caracteres. También será único dentro del usuario y se rechazará cualquier slug de etiqueta vacío o duplicado.
- El `Name` de una etiqueta es único dentro del usuario, como consecuencia directa de la unicidad de su `Slug` generado.
- Una etiqueta puede existir sin tener enlaces asociados.
- Una etiqueta con enlaces no podrá eliminarse, sólo se podrán eliminar etiquetas vacías.
- La visibilidad de un enlace depende de su publicación individual y de la visibilidad de su colección.
- Un enlace no publicado no aparece en una colección pública.
- Un enlace de una colección privada nunca aparece públicamente.
- Un enlace dentro de una colección privada no podrá cambiar su estado a público.
- Al hacer privada una colección pública, todos los enlaces de la colección pasan a `IsPublic = false`.
- Una colección privada que vuelva a ser pública no publicará automáticamente sus enlaces.
- Todo enlace nuevo se creará con `IsPublic = false`, independientemente de la visibilidad de la colección.
- El usuario decidirá individualmente qué enlaces publica.
- Al mover un enlace entre colecciones, se establecerá `IsPublic = false`.
- Después de mover un enlace, será necesaria una nueva publicación explícita por parte del usuario.
- La URL del enlace será inmutable.
- Para cambiar la URL, el usuario deberá eliminar el enlace y crear uno nuevo.
- Solo se podrán editar los metadatos y el estado de publicación del enlace.
- Los enlaces individuales no tendrán páginas indexables.
- Solo la página de usuario y las páginas de colecciones públicas serán indexables.

### Visibilidad efectiva

La visibilidad pública efectiva de un enlace será:

```text
Collection.IsPublic && Link.IsPublic
```

### URLs

- Se conservará la URL original introducida por el usuario.
- La URL original nunca será sustituida por una URL redireccionada.
- No se seguirán redirecciones para detectar duplicados.
- Se almacenará una representación auxiliar llamada `UrlNormalized`.
- `UrlNormalized` no será visible para el usuario.
- `UrlNormalized` no será una URL válida.
- `UrlNormalized` será una clave plana utilizada exclusivamente para detectar duplicados.
- No se permitirá la misma `UrlNormalized` dos veces dentro del mismo usuario.
- Se aceptan conscientemente posibles colisiones derivadas de la eliminación de separadores.
- La URL original estará disponible para consultar las diferencias entre enlaces que hayan producido la misma clave.

### Normalización de URLs para duplicados

Proceso de generación:

1. Comprobar que la URL original tenga una sintaxis válida.
2. Aplicar `UrlDecode` sobre la cadena completa para obtener el valor real de secuencias como `%20` o `+`.
3. Eliminar el esquema `http` o `https`, ya que ambos se consideran equivalentes.
4. Tratar el puerto: eliminarlo si es `80` u `8080` en HTTP, o `443` en HTTPS; conservar cualquier otro puerto. Los dos puntos del puerto desaparecerán más adelante, al aplicar la transformación a minúsculas y aplanado.
5. Ignorar el fragmento de la URL.
6. Eliminar los parámetros de marketing conocidos.
7. Ordenar alfabéticamente los parámetros restantes por nombre y valor.
8. Conservar los parámetros repetidos.
9. Convertir todo el contenido a minúsculas.
10. Eliminar los acentos conservando la letra base.
11. Sustituir `ç` por `c`.
12. Sustituir `ñ` por `n`.
13. Convertir los espacios en guiones medios.
14. Conservar únicamente letras, números y guiones medios.
15. Eliminar cualquier otro carácter, incluidos:
    - `.`
    - `:`
    - `/`
    - `?`
    - `&`
    - `=`
16. Eliminar guiones medios duplicados.
17. Eliminar guiones medios al principio y al final.

La aplicación de `UrlDecode` sobre la cadena completa es intencionada. Por tanto, valores codificados que representen caracteres estructurales podrán participar posteriormente en la transformación plana.

#### Parámetros de marketing eliminados

Se eliminarán estos parámetros, sin distinguir mayúsculas y minúsculas:

- `utm_source`
- `utm_medium`
- `utm_campaign`
- `utm_term`
- `utm_content`
- `gclid`
- `wbraid`
- `gbraid`
- `fbclid`
- `msclkid`
- `ttclid`
- `epik`
- `gad_source`
- `gad_campaignid`
- `srsltid`
- `gcs`
- `gcd`
- `at_medium`
- `at_campaign`
- `at_platform`
- `at_creation`
- `at_term`

#### Ejemplos

```text
HTTPS://Example.COM/Generación de datos/?utm_source=google&id=10
-> examplecomgeneracion-de-datosid10
```

```text
http://example.com:4587/Artículo número 2?b=2&a=1
-> examplecom4587articulo-numero-2a1b2
```

Como la barra `/` se elimina junto con el resto de separadores, estas URLs producirán la misma clave:

```text
example.com/articulo
example.com/articulo/
-> examplecomarticulo
```

También se aceptan colisiones como esta:

```text
example.com:4587/articulo-numero-2/?a=1&b=2
example.com:4587/articulo-numero-2/a/?$=1&b=2
-> examplecom4587articulo-numero-2a1b2
```

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
- El scraping estará incluido en el MVP0, pero se implementará en una fase posterior.
- El flujo provisional de scraping será:
  - Intento al abandonar el campo perteneciente a la URL que acaba de introducir el usuario.
  - Segundo intento silencioso a los 5 minutos.
  - Tercer y último intento silencioso 5 minutos después.
  - `Link.Retries` cuenta los intentos de scraping ya realizados (máximo 3). `Link.NextTry` es la fecha y hora del siguiente intento programado; será `null` cuando los reintentos se agoten.
  - El scraper solo rellena campos vacíos; nunca sobrescribe metadatos ya informados por scraping o edición manual. El scraping se considera completado cuando `Title`, `Description` e `Image` están todos informados.
- `LinkTag` y la proyección FTS5 se actualizarán dentro de la misma transacción que cada operación de escritura o borrado relacionada.
- Los filtros estructurados usarán SQL e índices convencionales.
- Los resultados de búsqueda se ordenan por relevancia (rank de FTS5, si existe), con `CreatedAt` de enlace descendente e `Id` de enlace descendente como desempates estables.
- Las operaciones de SQLite seguirán un ciclo breve:
  1. Abrir conexión.
  2. Ejecutar lo imprescindible.
  3. Confirmar o revertir.
  4. Liberar recursos.
- No habrá llamadas HTTP, scraping, esperas de usuario ni reintentos externos dentro de transacciones.
- SQLite se configurará con WAL, `busy_timeout` y claves foráneas activadas.
- Las consultas y transacciones se optimizarán para minimizar la duración del bloqueo de escritura.

#### SQLite FTS5 frente a SQLite, MySql o Postgress

para un proyecto pequeño, las ventajas de **FTS5** se entienden en dos frentes:

| | `LIKE` | FTS5 |
|---|---|---|
| Índice | Escanea **todas** las filas | Índice invertido, acceso directo |
| Ranking | No hay | **BM25** nativo (relevancia) |
| Consultas | Solo subcadena | Booleanas (`AND`, `OR`, `NOT`), frases, prefijos (`word*`), proximidad |
| 10k filas | ~50-100 ms | ~1-5 ms |

Es la diferencia entre "buscar en un texto plano" y "buscar en un motor de búsqueda". Y esto nos vendrá como anillo al dedo para la búsqueda abierta.

**Frente a MySQL / PostgreSQL**

Aquí no es una cuestión de *capacidad* (Postgres FTS es más rico: stemming, parson, `pg_trgm`, etc.), sino de **infraestructura**:

- **Cero servidor.** No hay proceso que arrancar, no hay conexión TCP, no hay pool de conexiones, no hay `pg_hba.conf`. El "motor de búsqueda" es la misma librería que ya tienes en memoria.
- **Un solo archivo.** Tu BD y tu índice FTS viven en el mismo `.db`. Backup = copiar un archivo. Deploy = copiar un archivo.
- **Sin red.** La query de búsqueda no hace un hop de red. En benchmarks con datasets pequeños (< 1M filas), FTS5 es **competitivo o más rápido** que Postgres FTS (4 ms vs 32 ms en un caso reportado en Stack Overflow, aunque con connection pool la diferencia se reduce a ~10%).
- **WASM / cliente.** Puedes correr FTS5 en el navegador, en una app móvil, en un edge device. Postgres no va a ningún sitio.
- **Sin ops.** No hay que actualizar el servidor, no hay que monitorizar un proceso, no hay que gestionar permisos de red.

**Cuándo FTS5 **no** es suficiente**

- **Stemming / lematización** → FTS5 no lo hace por defecto (Postgres sí con `to_tsvector`). Puedes añadir un tokenizer custom, pero es trabajo extra.
- **Volumen muy alto + escritura concurrente** → SQLite es single-writer. Si tienes miles de escrituras/segundo, necesitas Postgres.
- **Búsquedas complejas multi-idioma, fuzzy, vectorial** → Postgres con `pg_trgm` + `pgvector` va mucho más allá.

**FTS5 te da un motor de búsqueda real sin añadir un solo componente más al stack**. No tienes que "graduar" a Postgres, no tienes que montar Elasticsearch, no tienes que gestionar un servicio aparte. Es la opción de menor fricción posible para búsqueda a escala de miles o decenas de miles de documentos.

#### Dapper frente a Entity Framework

Se decide usar Dapper por el tamaño de Linkubator. Y por:

**1. Rendimiento sin overhead**
Dapper es un *micro-ORM*: una capa finísima sobre ADO.NET. Sin change tracking, sin lazy loading, sin traducción LINQ→SQL. En benchmarks suele ser **5–10× más rápido** que EF Core en materialización de resultados. En un proyecto pequeño con pocas tablas y queries simples, esa diferencia es casi imperceptible, pero el *techo* de rendimiento es mucho más alto.

**2. Menos "maquinaria" que gestionar**
EF Core arrastra consigo: `DbContext`, `IQueryable`, change tracking, migrations, `SaveChanges()`, navigation properties, identity map… En un proyecto pequeño, gran parte de eso es **complejidad que no vas a usar**. Dapper es un solo NuGet package y unas extension methods sobre `IDbConnection`. No hay nada que configurar, no hay "modo de usarlo" que puedas hacer mal.

**3. Control total del SQL**
Escribes el SQL exacto que se ejecuta. No hay sorpresas de N+1, no hay queries generadas que no entiendes. En un proyecto pequeño con 3-4 tablas, escribir `SELECT` directamente es más transparente que componer una expresión LINQ.

**4. Curva de aprendizaje mínima**
Si sabes SQL, ya sabes Dapper. No necesitas entender el concepto de `DbContext`, ni el ciclo de vida de tracking, ni cómo funciona `migrations`. Para un proyecto pequeño donde el equipo es reducido (o eres tú solo), esto se traduce en **velocidad de desarrollo real**.

**5. Footprint mínimo**
Sin `DbContext` que mantener, sin `IQueryable` que resolver, sin identity map en memoria. Menos objetos, menos GC, menos memoria.

**Lo que pierdes (y en un proyecto pequeño suele dar igual):**

| Lo que no tienes | Por qué da igual en un proyecto pequeño |
|---|---|
| Migrations automáticas | Con 3 tablas, un script SQL basta |
| Change tracking / `SaveChanges()` | Haces `INSERT`/`UPDATE` directo |
| Navigation properties / lazy loading | Con pocas entidades, un `JOIN` resuelve |
| LINQ sobre la BD | SQL directo es más simple para queries simples |
| Abstracción de proveedor | Si usas SQL Server o PostgreSQL, no necesitas abstraer |

**En resumen:** en un proyecto pequeño, EF Core te da 80% de features que no vas a usar y 20% de productividad en CRUD básico. Dapper te da el 100% de lo que necesitas (ejecutar SQL y mapear a objetos) con la mínima fricción.

### Listados y operación

- Las colecciones públicas se ordenarán por `CreatedAt` descendente e `Id` de enlace descendente como desempates estables.
- Se mostrarán 15 enlaces por página.
- Los listados privados seguirán el mismo orden que los públicos: `CreatedAt` descendente e `Id` de enlace descendente como desempates estables.
- La fecha se utilizará únicamente para ordenación.
- Se generarán logs desde el inicio.
- Los logs serán agnósticos.
- Los logs no incluirán secretos, contraseñas, hashes innecesarios ni credenciales contenidas en URLs.
- Grafana, Kibana y plataformas similares quedan fuera de ámbito por ahora.
- La aplicación deberá ser accesible y compatible con los navegadores actuales.

### SEO

- El SEO se abordará por fases.
- Las rutas, el alias de usuario, el slug de colección, la visibilidad, el SSR y el HTML semántico se definirán e implementarán desde el inicio.
- Solo las páginas públicas de colección y la página de usuario serán indexables por buscadores.
- El refinamiento de Open Graph, Twitter Cards, sitemap, robots, datos estructurados y auditoría de rastreabilidad queda fuera del MVP0.

## Pendientes

- Diseño detallado del scraping y su contrato de ejecución en segundo plano. Se abordarán cuando se tenga que interactuar con el scraping; mientras no entremos en esa fase no es bloqueante.
  1. Timeout exacto del scraper.
  2. Redirecciones permitidas durante el scraping.
  3. Tamaño máximo de respuesta del scraper.
  4. Tipos de contenido aceptados por el scraper.

## Fuera de alcance del MVP0

- Validación de contraseñas comprometidas: **NIST SP 800-63B** recomienda comparar la contraseña elegida contra una lista de contraseñas filtradas/comunes conocidas, en vez de (o además de) exigir reglas de composición. Servicio sugerido: *Pwned Passwords, de Have I Been Pwned* (https://haveibeenpwned.com/API/v3#PwnedPasswords), vía su endpoint de k-anonimato (`https://api.pwnedpasswords.com/range/{5 primeros caracteres del hash SHA-1}`), gratuito y sin necesidad de enviar la contraseña en claro. No forma parte del MVP0; queda como mejora futura para cuando exista exposición pública real.
- Redirecciones de alias.
- Importación de marcadores.
- Exportación de marcadores.
- Extensión de navegador.
- Colaboración entre usuarios.
- Sincronización con servicios externos.
- Analítica avanzada.
- Plataforma externa de observabilidad.
- SEO avanzado.
- Estrategia de despliegue.
