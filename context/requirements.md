# Requisitos de Linkubator

## 1. Propósito

Linkubator será una aplicación web para que cada usuario gestione sus enlaces favoritos, los organice mediante colecciones y etiquetas, y pueda publicar selectivamente parte de ese contenido.

## 2. Alcance del MVP0

El usuario podrá:

- Definir un usuario con nombre, email, alias y contraseña.
- Crear y editar colecciones.
- Eliminar colecciones únicamente cuando no tengan enlaces.
- Marcar una colección como pública o privada.
- Crear enlaces y editar únicamente sus metadatos y estado de publicación (Público o privado).
- Asignar cada enlace a una única colección.
- Asociar varias etiquetas a un enlace.
- Publicar o retirar individualmente un enlace dentro de una colección pública, cambiando la visibilidad de dicho enlace.
- Buscar sus propios enlaces.
- Ver sus enlaces por colección o por etiqueta.
- Obtener los meta `title`, `description` y `og:image` de la URL.
- Consultar páginas públicas de colecciones.

El MVP0 será una aplicación local. No tendrá registro, login ni autenticación. Se usará un usuario fijo configurable, pero el modelo quedará preparado para futuros usuarios.

Las páginas públicas y sus fundamentos SEO podrán verificarse localmente, pero no serán rastreables por buscadores mientras la aplicación no tenga un despliegue público. El despliegue queda fuera del MVP0.

## 3. Usuario y alias

Cada usuario tendrá un alias público único globalmente.

El alias se generará a partir del texto proporcionado por el usuario mediante las mismas reglas de generación que se utilizarán para los slugs de colecciones y etiquetas.

Reglas comunes de generación de alias y slugs:

1. Convertir el texto a minúsculas.
2. Eliminar los acentos de las letras, conservando la letra base: `Generación` se convierte en `generacion`.
3. Sustituir `ç` por `c` y `ñ` por `n`: `caça` se convierte en `caca` y `España` en `espana`.
4. Sustituir los espacios por guiones medios.
5. Mantener letras, números y guiones medios.
6. Eliminar cualquier otro carácter.
7. Evitar guiones medios duplicados o en los extremos del valor generado.

Por ejemplo:

```text
Saltó la raña al charço!!! I luego, croo
-> salto-la-rana-al-charco-i-luego-croo
```

Reglas específicas del alias:

- Longitud entre 10 y 25 caracteres.
- El valor almacenado solo contendrá letras minúsculas sin acentos, números y guiones medios.
- Podrá cambiarse posteriormente si el nuevo valor es válido y único.
- Palabras reservadas: `collection`, `collections`, `user`, `users`, `tag` y `tags`.

Si el alias cambia, las rutas públicas asociadas al alias anterior devolverán `404` en el MVP0. Las redirecciones de alias quedan como propuesta para una versión futura.

La contraseña se almacenará únicamente como hash seguro. Nunca se guardará en texto claro.

## 4. Colecciones

Una colección tendrá nombre, slug, descripción, propietario, visibilidad y fecha de creación.

El slug será único dentro del usuario, pero no será global.

El slug se generará automáticamente desde el nombre de la colección mediante las reglas comunes de generación. Tendrá una longitud mínima de 1 carácter y una máxima de 50 caracteres, será invisible para el usuario y no podrá cambiarse.

El slug será único dentro del usuario. Si el nombre genera una colisión, la operación se rechazará y el usuario deberá elegir otro nombre; no se añadirán sufijos automáticos.

Las etiquetas también tendrán un slug técnico generado mediante exactamente las mismas reglas de transformación. El slug de etiqueta será único dentro del usuario. Si el texto no produce ningún carácter válido, o si el resultado colisiona con otra etiqueta del usuario, la operación se rechazará.

Las colecciones públicas tendrán una URL con este formato:

```text
/user-alias/collections/collection-slug
```

Las colecciones privadas no serán accesibles desde fuera.

Una colección privada y una colección inexistente responderán de la misma forma desde el exterior: `404`.

## 5. Visibilidad de enlaces

La visibilidad pública efectiva de un enlace será:

```text
Collection.IsPublic && Link.IsPublic
```

Reglas:

- Un enlace de una colección privada nunca será público.
- En una colección pública, el usuario decidirá individualmente qué enlaces hace visibles.
- Al hacer privada una colección pública, ninguno de sus enlaces será accesible públicamente.
- Al volver pública una colección privada, sus enlaces permanecerán privados (No visibles).
- Un enlace movido a una colección privada quedará privado (No visible).
- Si después vuelve a una colección pública, seguirá sin publicarse hasta una decisión explícita por parte del usuario.

Al mover un enlace entre colecciones, `IsPublic` pasará a `false`. El usuario tendrá que publicarlo explícitamente en su nueva colección, aunque también sea pública.

Al crear un enlace, `IsPublic` siempre será `false`, independientemente de la visibilidad de su colección. El usuario tendrá que publicarlo explícitamente si desea hacerlo visible.

## 6. Enlaces

Se conservará la URL original introducida por el usuario.

Se calculará `UrlNormalized` únicamente para detectar duplicados. `UrlNormalized` será una clave plana, no una URL válida, y no se mostrará al usuario.

No se seguirán redirecciones para decidir si una URL está duplicada.

La misma `UrlNormalized` no podrá guardarse dos veces dentro del mismo usuario. El sistema debe avisar antes de crear un duplicado.

Antes de calcular `UrlNormalized`, la URL original deberá superar una validación sintáctica y se aplicará `UrlDecode` sobre la cadena completa.

Después se eliminarán el esquema `http`/`https` y el fragmento, se retirarán los parámetros de marketing conocidos, se ordenarán los parámetros restantes por nombre y valor y se conservarán los parámetros repetidos.

Finalmente se aplicará la transformación común de identificadores: minúsculas, eliminación de acentos, sustitución de `ç`/`ñ`, espacios a guiones medios y eliminación de cualquier carácter distinto de letras, números y guiones medios. También se eliminarán los guiones duplicados y extremos.

En HTTP se eliminarán los puertos `80` y `8080`; en HTTPS se eliminará el puerto `443`. Los demás puertos se conservarán antes de aplanar la clave.

Se aceptan conscientemente posibles colisiones derivadas de eliminar separadores como `.`, `:`, `/`, `?`, `&` y `=`. La URL original siempre se conservará.

La URL del enlace será inmutable después de su creación. Si el usuario quiere guardar otra vez la misma URL, deberá eliminar el enlace y crear uno nuevo.

## 7. Metadatos

La aplicación podrá obtener posteriormente los siguientes metadatos de la URL. Esta capacidad forma parte del MVP0, pero se implementará en una fase posterior:

- Meta title.
- Meta description.
- Meta property `og:image`.

Una vez el usuario haya introducido la URL y abandone ese campo, se lanzará el proceso de scraping para obtener los metadatos. Si se encuentran, se rellenarán los campos y no habrá más intentos. Si no se encuentran, se programará un segundo intento silencioso cinco minutos después. Si tampoco se encuentran, se realizará un tercer y último intento cinco minutos después del segundo.

Si no se encuentran metadatos en cualquiera de los intentos, el enlace se conservará y esos campos quedarán vacíos.

El diseño detallado del scraping queda aplazado. Antes de implementarlo deberán definirse protección SSRF, validación de protocolos, timeout, límites de respuesta, control de redirecciones, validación del tipo de contenido y el contrato exacto del procesamiento en segundo plano.

## 8. Búsqueda

La búsqueda usará SQLite FTS5.

Podrá buscar sobre:

- URL original.
- Título.
- Descripción.
- Nombre de colección.
- Nombre de las etiquetas.

La búsqueda siempre estará limitada al usuario actual. Nunca podrá devolver enlaces de otro usuario.

FTS5 se utilizará para la búsqueda textual. Los filtros estructurados por colección, etiqueta y publicación se resolverán mediante SQL e índices convencionales.

## 9. Listados públicos

Las colecciones públicas mostrarán solo enlaces con `IsPublic = true`.

El orden será:

1. `CreatedAt` descendente.
2. Identificador del enlace como desempate estable.

Se mostrarán 15 enlaces por página.

## 10. SEO

El SEO se abordará por fases. Las decisiones que afectan al dominio, las rutas, la visibilidad y el HTML se definirán desde el principio. El refinamiento y la auditoría SEO avanzada quedan fuera del MVP0.

### Fundamentos desde el inicio

Las páginas públicas de colección deberán tener:

- URL amigable con el formato `/user-alias/collections/collection-slug`.
- HTML renderizado en servidor.
- `meta title`.
- `meta description`.
- Un único `h1`.
- Encabezados semánticos.
- URL canonical básica.
- Exclusión de colecciones privadas.
- Exclusión de enlaces no publicados.
- Respuestas `404` para recursos públicos inexistentes o no accesibles.
- Paginación estable.

Los enlaces individuales no tendrán páginas indexables propias.

### SEO avanzado posterior al MVP0

Queda fuera del MVP0 y podrá abordarse posteriormente:

- Open Graph.
- Twitter Cards, si procede.
- `robots.txt`.
- `sitemap.xml`.
- Datos estructurados, si aportan valor.
- Política definitiva de paginación indexable.
- Auditoría de títulos, descripciones, canonical y rastreabilidad.

## 11. Accesibilidad

La aplicación deberá ser usable con teclado y tecnologías de asistencia.

Requisitos iniciales:

- Orden de tabulación lógico y coincidente con el orden visual.
- Foco visible.
- Primer campo lógico enfocable al entrar en formularios.
- Labels asociados a los campos.
- Mensajes de error junto al campo y anunciables. Si por falta de espacio puede ser debajo del campo, no hay problema. Pero el campo debe también quedar marcado de forma visual con error.
- Navegación para saltar al contenido principal.
- Landmarks y encabezados semánticos.
- Contraste suficiente.
- No depender únicamente del color.
- Nombres accesibles para enlaces y botones.
- Soporte de `prefers-reduced-motion`.

La interfaz inicial estará en castellano y deberá soportar los navegadores actuales.

## 12. Operación

Se generarán logs agnósticos desde el inicio para operaciones relevantes, errores de persistencia, fallos y reintentos del scraper, validaciones, SSRF y operaciones de publicación. No deberán incluir secretos, contraseñas, hashes innecesarios ni credenciales contenidas en URLs.

Las operaciones de base de datos deberán ser breves, explícitas y quirurgicas: abrir conexión, ejecutar la unidad mínima de trabajo, confirmar o revertir y liberar recursos. No se mantendrán transacciones abiertas mientras se espera al usuario o se realizan operaciones externas.

SQLite se configurará para permitir lecturas concurrentes durante escrituras y para tolerar bloqueos transitorios breves, sin ocultar errores persistentes.

Grafana, Kibana y otras plataformas de observabilidad quedan fuera del MVP0.

## 13. Fuera de alcance

- Registro de usuarios.
- Login y autenticación.
- Recuperación de contraseña.
- Cambio de alias.
- Importación y exportación.
- Extensión de navegador.
- Colaboración entre usuarios.
- Recomendaciones.
- Crawling completo.
- Sincronización con servicios externos.
- Analítica avanzada.
- SEO avanzado.
