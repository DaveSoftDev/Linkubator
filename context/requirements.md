# Requisitos del producto

## 1. Propósito

Linkubator será una aplicación web para que cada usuario gestione sus enlaces favoritos, los organice mediante colecciones y etiquetas, y pueda publicar selectivamente parte de ese contenido.

## 2. Alcance del MVP0

El usuario podrá:

- Definir un usuario con nombre, email, alias y contraseña hasheada.
- Crear y editar colecciones.
- Eliminar colecciones únicamente cuando no tengan enlaces.
- Marcar una colección como pública o privada.
- Crear enlaces y editar únicamente sus metadatos y estado de publicación.
- Asignar cada enlace a una única colección.
- Asociar varios tags a un enlace.
- Publicar o retirar individualmente un enlace dentro de una colección pública.
- Buscar sus propios enlaces.
- Ver enlaces por colección o tag.
- Obtener `title`, `description` y `og:image` de la URL.
- Consultar páginas públicas de colecciones.

El MVP0 será una aplicación local. No tendrá registro, login ni autenticación. Se usará un usuario fijo configurable, pero el modelo quedará preparado para futuros usuarios.

Las páginas públicas y sus fundamentos SEO podrán verificarse localmente, pero no serán rastreables por buscadores mientras la aplicación no tenga un despliegue público. El despliegue queda fuera del MVP0.

## 3. Usuario y alias

Cada usuario tendrá un alias público único globalmente.

Reglas del alias:

- Longitud entre 10 y 25 caracteres.
- Solo caracteres ASCII básicos: letras minúsculas, números y guiones.
- Se almacenará en minúsculas.
- No se permiten acentos, `ñ`, `ç`, espacios, guiones bajos ni otros caracteres especiales.
- No podrá cambiarse posteriormente.
- Palabras reservadas: `collections`, `users` y `tags`.

La contraseña se almacenará únicamente como hash seguro. Nunca se guardará en texto claro.

## 4. Colecciones

Una colección tendrá nombre, slug, descripción, propietario, visibilidad y fecha de creación.

El slug será único dentro del usuario, pero no será global.

Sus reglas serán las mismas que las del alias, excepto la longitud máxima, que será de 50 caracteres. El slug podrá cambiarse, siempre que el nuevo valor siga siendo válido y continúe siendo único dentro del usuario.

Las colecciones públicas tendrán una URL con este formato:

```text
/user-alias/collections/collection-slug
```

Las colecciones privadas no serán accesibles desde fuera.

Una colección privada y una colección inexistente responderán de la misma forma desde el exterior: `404`.

## 5. Visibilidad de enlaces

La visibilidad pública efectiva de un enlace será:

```text
Collection.IsPublic && Link.IsPublished
```

Reglas:

- Un enlace de una colección privada nunca será público.
- En una colección pública, el usuario decidirá individualmente qué enlaces publica.
- Al hacer privada una colección pública, ninguno de sus enlaces será accesible públicamente.
- Al volver pública una colección privada, sus enlaces permanecerán privados/no publicados.
- Un enlace movido a una colección privada quedará privado/no publicado.
- Si después vuelve a una colección pública, seguirá sin publicarse hasta una decisión explícita.

Al mover un enlace entre colecciones, `IsPublished` pasará a `false`. El usuario tendrá que publicarlo explícitamente en su nueva colección, aunque también sea pública.

## 6. Enlaces

Se conservará la URL original introducida por el usuario.

Se calculará una URL normalizada únicamente para detectar duplicados. La URL normalizada no sustituirá ni se mostrará en lugar de la original.

No se seguirán redirecciones para decidir si una URL está duplicada.

La misma URL normalizada no podrá guardarse dos veces dentro del mismo usuario. El sistema debe avisar antes de crear un duplicado.

La URL del enlace será inmutable después de su creación. Si el usuario quiere guardar otra URL, deberá eliminar el enlace y crear uno nuevo.

## 7. Metadatos

La aplicación podrá obtener posteriormente los siguientes metadatos de la URL:

- HTML `title`.
- Meta description.
- Meta property `og:image`.

El comportamiento del intento inicial, los reintentos y su ejecución en segundo plano queda aplazado hasta cerrar el diseño específico del scraping.

Si no se encuentran metadatos, el enlace se conservará y esos campos quedarán vacíos.

El diseño detallado del scraping queda aplazado. Antes de implementarlo deberán definirse protección SSRF, validación de protocolos, timeout, límites de respuesta, control de redirecciones, validación del tipo de contenido y el contrato exacto del procesamiento en segundo plano.

## 8. Búsqueda

La búsqueda usará preferiblemente SQLite FTS5.

Podrá buscar sobre:

- URL original.
- Título.
- Descripción.
- Nombre de colección.
- Nombre de los tags.

La búsqueda siempre estará limitada al usuario actual. Nunca podrá devolver enlaces de otro usuario.

FTS5 se utilizará para la búsqueda textual. Los filtros estructurados por colección, tag, publicación o fechas se resolverán mediante SQL e índices convencionales.

## 9. Listados públicos

Las colecciones públicas mostrarán solo enlaces publicados.

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
- `title`.
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
- Mensajes de error junto al campo y anunciables.
- Navegación para saltar al contenido principal.
- Landmarks y encabezados semánticos.
- Contraste suficiente.
- No depender únicamente del color.
- Nombres accesibles para enlaces y botones.
- Soporte de `prefers-reduced-motion`.

La interfaz inicial estará en castellano y deberá soportar los navegadores actuales.

## 12. Operación

Se generarán logs agnósticos desde el inicio para operaciones relevantes, errores de persistencia, fallos y reintentos del scraper, validaciones, SSRF y operaciones de publicación. No deberán incluir secretos, contraseñas, hashes innecesarios ni credenciales contenidas en URLs.

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
