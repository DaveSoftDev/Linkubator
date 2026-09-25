# Decisiones acerca de Linkubator

## Confirmadas

### Producto

- Linkubator gestionará enlaces, colecciones y etiquetas.
- `Tag` y `LinkTag` se reservarán para nombres técnicos.
- El producto está preparado conceptualmente para múltiples usuarios.
- El MVP0 no tendrá registro, login ni autenticación.
- Se utilizará un usuario fijo configurable.
- El MVP0 se ejecutará únicamente en local.
- Las páginas públicas conservarán fundamentos SEO verificables localmente, pero no podrán ser rastreadas por buscadores hasta un despliegue público posterior.
- La interfaz inicial estará en castellano.

### Usuario

- El usuario tendrá nombre, alias, email, contraseña hasheada y fecha de creación.
- El alias será visible, mutable y único globalmente.
- El alias tendrá entre 10 y 25 caracteres después de su generación.
- El alias se almacenará en minúsculas.
- Las palabras reservadas serán:
  - `collection`
  - `collections`
  - `user`
  - `users`
  - `tag`
  - `tags`
- Si el alias cambia, las rutas antiguas devolverán `404` en el MVP0.
- Las redirecciones de alias quedan como propuesta futura.

### Generación de alias y slugs

`User.Alias`, `Collection.Slug` y `Tag.Slug` se generarán mediante la misma transformación:

1. Convertir el texto a minúsculas.
2. Eliminar los acentos conservando la letra base.
3. Sustituir `ç` por `c`.
4. Sustituir `ñ` por `n`.
5. Sustituir los espacios por guiones medios.
6. Conservar letras, números y guiones medios.
7. Eliminar cualquier otro carácter.
8. Eliminar guiones medios duplicados.
9. Eliminar guiones medios al principio y al final.

Ejemplos:

```text
Generación -> generacion
caça -> caca
España -> espana
```

```text
Saltó la raña al charço!!! I luego, croo
-> salto-la-rana-al-charco-i-luego-croo
```

Si el resultado de la transformación queda vacío, la operación se rechazará.

### Colecciones y enlaces

- Cada enlace pertenece a una única colección.
- Las colecciones serán públicas o privadas.
- El slug de la colección será único dentro del usuario, nunca global.
- El slug de la colección se autogenerará desde el nombre.
- El slug de la colección seguirá las reglas comunes de generación.
- El slug de la colección tendrá entre 1 y 50 caracteres después de su generación.
- El slug de la colección será invisible para el usuario.
- El slug de la colección será inmutable.
- Si el slug generado colisiona dentro del usuario, la operación se rechazará.
- No se añadirán sufijos automáticos para resolver colisiones.
- El slug técnico de las etiquetas seguirá las mismas reglas de generación.
- El slug de etiqueta será único dentro del usuario.
- Se rechazará cualquier slug de etiqueta vacío o duplicado.
- Una colección con enlaces no podrá eliminarse.
- Solo se podrán eliminar colecciones vacías.
- La visibilidad de un enlace depende de su publicación individual y de la visibilidad de su colección.
- Un enlace no publicado no aparece en una colección pública.
- Un enlace de una colección privada nunca aparece públicamente.
- Un enlace dentro de una colección privada no podrá cambiar su estado a público.
- Una colección privada que vuelva a ser pública no publicará automáticamente sus enlaces.
- Todo enlace nuevo se creará con `IsPublic = false`, independientemente de la visibilidad de la colección.
- El usuario decidirá individualmente qué enlaces publica.
- Al mover un enlace entre colecciones, se establecerá `IsPublic = false`.
- Después de mover un enlace, será necesaria una nueva publicación explícita.
- La URL del enlace será inmutable.
- Para cambiar la URL, el usuario deberá eliminar el enlace y crear uno nuevo.
- Solo se podrán editar los metadatos y el estado de publicación del enlace.
- Los enlaces individuales no tendrán páginas indexables.
- Solo las páginas públicas de colección serán indexables.

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

Antes de calcular `UrlNormalized`, se validará que la URL original tenga una sintaxis aceptable.

Proceso de generación:

1. Aplicar `UrlDecode` sobre la cadena completa como primer paso.
2. Eliminar el esquema `http` o `https`, ya que ambos se consideran equivalentes.
3. Ignorar el fragmento de la URL.
4. Eliminar los parámetros de marketing conocidos.
5. Ordenar alfabéticamente los parámetros restantes por nombre y valor.
6. Conservar los parámetros repetidos.
7. Convertir todo el contenido a minúsculas.
8. Eliminar los acentos conservando la letra base.
9. Sustituir `ç` por `c`.
10. Sustituir `ñ` por `n`.
11. Convertir los espacios en guiones medios.
12. Conservar únicamente letras, números y guiones medios.
13. Eliminar cualquier otro carácter, incluidos:
    - `.`
    - `:`
    - `/`
    - `?`
    - `&`
    - `=`
14. Eliminar guiones medios duplicados.
15. Eliminar guiones medios al principio y al final.

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

#### Tratamiento de puertos

Los puertos se tratarán antes de generar la clave plana:

- En HTTP se eliminará el puerto `80`.
- En HTTP se eliminará el puerto `8080`.
- En HTTPS se eliminará el puerto `443`.
- Cualquier otro puerto se conservará.
- Los dos puntos del puerto desaparecerán posteriormente al aplicar la transformación plana.

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
example.com:4587/articulo-numero-2/a/?!=1&b=2
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
  - Intento al abandonar la URL.
  - Segundo intento silencioso a los 5 minutos.
  - Tercer y último intento silencioso 5 minutos después.
- `LinkTag` y la proyección FTS5 se actualizarán dentro de la misma transacción que cada operación de escritura o borrado relacionada.
- FTS5 se utilizará para búsqueda textual.
- Los filtros estructurados usarán SQL e índices convencionales.
- Las operaciones de SQLite seguirán un ciclo breve:
  1. Abrir conexión.
  2. Ejecutar lo imprescindible.
  3. Confirmar o revertir.
  4. Liberar recursos.
- No habrá llamadas HTTP, scraping, esperas de usuario ni reintentos externos dentro de transacciones.
- SQLite se configurará con WAL, `busy_timeout` y claves foráneas activadas.
- Las consultas y transacciones se optimizarán para minimizar la duración del bloqueo de escritura.

### Listados y operación

- Las colecciones públicas se ordenarán por `CreatedAt` descendente.
- Se mostrarán 15 enlaces por página.
- La fecha se utilizará únicamente para ordenación.
- Se generarán logs desde el inicio.
- Los logs serán agnósticos.
- Los logs no incluirán secretos, contraseñas, hashes innecesarios ni credenciales contenidas en URLs.
- Grafana, Kibana y plataformas similares quedan fuera de ámbito por ahora.
- La aplicación deberá ser accesible y compatible con los navegadores actuales.

### SEO

- El SEO se abordará por fases.
- Las rutas, el alias de usuario, el slug de colección, la visibilidad, el SSR y el HTML semántico se definirán desde el inicio.
- Solo las colecciones públicas serán indexables.
- El refinamiento de Open Graph, Twitter Cards, sitemap, robots, datos estructurados y auditoría de rastreabilidad queda fuera del MVP0.

## Pendientes

- Timeout exacto del scraper.
- Redirecciones permitidas durante el scraping.
- Tamaño máximo de respuesta del scraper.
- Tipos de contenido aceptados por el scraper.
- Diseño detallado del scraping y su contrato de ejecución en segundo plano.
- Estrategia de despliegue.

## Fuera de alcance del MVP0

- Registro y autenticación.
- Login funcional.
- Recuperación de contraseña.
- Importación de marcadores.
- Exportación de marcadores.
- Extensión de navegador.
- Colaboración entre usuarios.
- Crawling completo.
- Sincronización con servicios externos.
- Analítica avanzada.
- Plataforma externa de observabilidad.
- SEO avanzado.
- Redirecciones de alias.
