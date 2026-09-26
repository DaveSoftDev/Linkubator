# Requisitos de Linkubator

## 1. Propósito

Linkubator será una aplicación web para que cada usuario gestione sus enlaces favoritos, los organice mediante colecciones y etiquetas, y pueda publicar selectivamente parte de ese contenido.

## 2. Alcance del MVP0

El usuario podrá:

- Registrar un usuario con nombre, email, alias y contraseña.
- Identificarse en el sistema.
- Editar su configuración de usuario (nombre, email, alias).
- Cambiar su contraseña.
- Recuperar su contraseña si la ha olvidado.
- Crear y editar colecciones.
- Eliminar colecciones únicamente cuando no tengan enlaces.
- Marcar una colección como pública o privada.
- Crear enlaces y editar únicamente sus metadatos y estado de publicación (Público o privado).
- Asignar cada enlace a una única colección.
- Mover enlaces entre colecciones. Al mover un enlace, `IsPublic` pasa a `false` y el usuario deberá publicarlo explícitamente en su nueva colección.
- Crear, editar y eliminar etiquetas. Las etiquetas vacías (sin enlaces asociados) se pueden eliminar.
- Asociar varias etiquetas a un enlace.
- Publicar o retirar individualmente un enlace dentro de una colección pública, cambiando la visibilidad de dicho enlace.
- Buscar sus propios enlaces.
- Ver sus enlaces por colección o por etiqueta.
- Obtener los meta `title`, `description` y `og:image` de la URL.
- Consultar páginas públicas de colecciones.

El MVP0 será una aplicación local con registro de usuario, login y autenticación. Las credenciales se gestionarán localmente; no se requiere conexión a servidores externos para la autenticación.

Las páginas públicas y sus fundamentos SEO podrán verificarse localmente, pero no serán rastreables por buscadores mientras la aplicación no tenga un despliegue público. El despliegue queda fuera del MVP0.

## 3. Usuario y alias

Cada usuario tendrá un nombre, un email único globalmente y un alias público único también globalmente. El nombre es obligatorio y no necesita ser único.

Reglas específicas del alias:

- Se generará aplicando las mismas reglas comunes de generación especificadas en decisions.md (Generación de alias y slugs).
- Longitud entre 10 y 25 caracteres.
- Podrá cambiarse posteriormente si el nuevo valor es válido y único globalmente.
- Palabras reservadas: `collection`, `collections`, `user`, `users`, `tag` y `tags`.

Si el alias cambia, las rutas públicas asociadas al anterior alias devolverán `404` en el MVP0.

La contraseña se almacenará únicamente como hash seguro. Nunca se guardará en texto claro.

La contraseña tendrá entre 10 y 50 caracteres, con texto libre: se acepta cualquier carácter (letras con o sin acentos, mayúsculas, minúsculas, números, símbolos y espacio), sin exigir una combinación obligatoria de tipos de carácters; ya que la normativa NIST SP 800-63B recomienda no forzar al usuario a incluirlos, debido a que estos siempre los incluian al final. El usuario es el único responsable de la fortaleza de la contraseña que elija.

La página de usuario será también indexable por los buscadores, pero obviamente no mostrará datos del usurio

## 4. Colecciones

Una colección tendrá nombre, slug, descripción, propietario, visibilidad y fecha de creación.

El slug será único dentro del usuario, pero no será global.

El slug se generará automáticamente desde el nombre de la colección mediante las reglas comunes de generación. Tendrá una longitud mínima de 1 carácter y una máxima de 50 caracteres, será invisible para el usuario y no podrá cambiarse.

Si el nombre no produce ningún carácter, o si genera una colisión, la operación se rechazará y el usuario deberá elegir otro nombre; no se añadirán sufijos ni prefijos automáticamente.

Las colecciones públicas tendrán una URL con este formato:

```text
/user-alias/collections/collection-slug
```

Las colecciones privadas no serán accesibles desde fuera.

Una colección privada y una colección inexistente responderán de la misma forma desde el exterior: `404`.

## 5. Etiquetas

Una etiqueta tendrá nombre, slug y fecha de creación.

El slug será único dentro del usuario, pero no será global.

El slug se generará automáticamente desde el nombre de la etiqueta mediante las reglas comunes de generación. Tendrá una longitud mínima de 1 carácter y una máxima de 50 caracteres, será invisible para el usuario y no podrá cambiarse.

Si el nombre no produce ningún carácter, o si genera una colisión, la operación se rechazará y el usuario deberá elegir otro nombre; no se añadirán sufijos ni prefijos automáticamente.

## 6. Visibilidad de enlaces

La visibilidad pública efectiva de un enlace será:

```text
Collection.IsPublic && Link.IsPublic
```

Al crear un enlace, `IsPublic` será siempre `false`, independientemente de la visibilidad de su colección. El usuario tendrá que publicarlo explícitamente si desea hacerlo visible.

Al mover un enlace entre colecciones, `IsPublic` pasará a `false`; independientemente de si la colección es pública o privada. El usuario tendrá que publicarlo explícitamente en su nueva colección, aunque también sea pública.

Reglas:

- Al crear / editar un enlace de una colección privada, no deberemos permitir cambiar la visibilidad `IsPublic`, manteniendo el `false`.
- Un enlace de una colección privada nunca será público.
- En una colección pública, el usuario decidirá individualmente qué enlaces hace visibles. Lo puede hacer al editar o directamente al crear el enlace.
- Al hacer privada una colección pública, todos los enlaces de la colección pasan a `IsPublic = false` (No visibles).
- Al hacer pública una colección privada, sus enlaces permanecerán privados (No visibles), el sistema no alterará la visibilidad de los enlaces de forma automática como si hace a la inversa.
- Un enlace movido a una colección privada o pública quedará privado (No visible).
- Si después, ese mismo enlace, vuelve a una colección pública, seguirá sin publicarse hasta que el usuario así lo indique.

## 7. Enlaces

Se conservará intacta la URL original introducida por el usuario. Ya que esta es la que utilizaremos para navegar.

Se calculará `UrlNormalized` únicamente para detectar duplicados. `UrlNormalized` será una clave plana, no una URL válida, y no se mostrará al usuario.

No se seguirán redirecciones para decidir si una URL está duplicada.

La misma `UrlNormalized` no podrá guardarse dos veces dentro del mismo usuario. El sistema debe avisar antes de crear un duplicado.

La URL del enlace será inmutable después de su creación. Si el usuario quiere modificar la URL, deberá eliminar el enlace y crearlo de nuevo.

El algoritmo exacto, paso a paso, para la generación de `UrlNormalized` está definido en decisions.md ("Normalización de URLs para duplicados"); esta sección solo describe el criterio funcional.

Se aceptan conscientemente posibles colisiones derivadas de eliminar separadores como `.`, `:`, `/`, `?`, `&` y `=`.

## 8. Metadatos de los enlaces de usuario

La aplicación podrá obtener posteriormente los siguientes metadatos de la URL original mediante un sistema de scraping. El mapeo correcto es el siguiente:

- Meta title, si la URL original no tiene el meta title, entonces recuperemos el tag HTML title y lo almacenaremos en nuestra propiedad `Title`.
- Meta description, almacenado en la propiedad `Description`.
- Meta property `og:image`, almacenado en la propiedad `Image`.

Si no se encuentran metadatos, el enlace se conservará y los campos que permanezcan vacíos quedarán sin valor.

Una vez el usuario haya introducido la URL y abandone ese campo, se lanzará el proceso de scraping para obtener los metadatos. El scraper solo rellenará los campos que estén vacíos; **nunca sobrescribirá** valores ya informados, ya sea por un intento anterior de scraping o por edición manual del usuario. Una vez que los tres campos (`Title`, `Description` e `Image`) están informados, el scraping se considera completado y no se reintenta.

Si en un intento no se encuentran metadatos, se programará un segundo intento silencioso 5 minutos después. Si tampoco se encuentran, se realizará un tercer y último intento 5 minutos después del segundo. En este caso, el scraper completará solo los campos que sigan vacíos.

El diseño detallado del scraping queda aplazado hasta que se aborde su fase de uso. Antes de implementarlo deberán definirse protección SSRF, validación de protocolos, timeout, límites de respuesta, control de redirecciones, validación del tipo de contenido y el contrato exacto del procesamiento en segundo plano.

## 9. Filtros y búsqueda

Una búsqueda siempre estará limitada al usuario actual. Nunca podrá devolver enlaces de otro usuario.

La búsqueda textual usará SQLite FTS5.

Podrá buscar sobre:

- URL original.
- Título.
- Descripción.
- Nombre de colección.
- Nombre de las etiquetas.

Los filtros estructurados por colección y etiqueta se resolverán mediante SQL e índices convencionales.

El orden de resultados (tanto para los filtros estructurados como la búequeda textual) será:

1. Relevancia (rank de FTS5) descendente. Sólo aplicable a la búsqueda textual.
2. `CreatedAt` descendente como desempate.
3. Identificador del enlace descendente como desempate final.

## 10. Listados públicos

Las colecciones públicas mostrarán solo enlaces con `IsPublic = true`.

El orden será:

1. `CreatedAt` descendente.
2. Identificador del enlace descendente como desempate estable.

Los listados se mostrarán paginado y limitados a 15 enlaces por página.

Los listados privados seguirán el mismo orden que los públicos: `CreatedAt` descendente, con identificador del enlace descendente como desempate estable.

## 11. SEO

El SEO se abordará por fases. Las decisiones que afectan al dominio, las rutas, la visibilidad y el HTML se definirán desde el principio. El refinamiento y la auditoría SEO avanzada quedan fuera del MVP0.

La página pública de usuario deberán tener:

- URL amigable con el formato `/user-alias/`.
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

## 12. Accesibilidad

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

## 13. Operación

Se generarán logs agnósticos desde el inicio para operaciones relevantes, errores de persistencia, fallos y reintentos del scraper, validaciones, SSRF y operaciones de publicación. No deberán incluir secretos, contraseñas, hashes innecesarios ni credenciales contenidas en URLs.

Las operaciones de base de datos deberán ser breves, explícitas y quirurgicas: abrir conexión, ejecutar la unidad mínima de trabajo, confirmar o revertir y liberar recursos. No se mantendrán transacciones abiertas mientras se espera al usuario o se realizan operaciones externas.

SQLite se configurará para permitir lecturas concurrentes durante escrituras y para tolerar bloqueos transitorios breves, sin ocultar errores persistentes.

Grafana, Kibana y otras plataformas de observabilidad quedan fuera del MVP0.

## 14. Fuera de alcance

- Validación de contraseñas comprometidas.
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

