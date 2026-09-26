# Roadmap del MVP0 de Linkubator

## Fase 0: Documentación

Objetivo: conservar una definición coherente antes de implementar.

Entregables:

- Requisitos del producto.
- Modelo de dominio.
- Arquitectura prevista.
- Decisiones confirmadas y pendientes.
- Roadmap.

Estado: Cerrada.

## Fase 1: Cierre de decisiones estructurales

Estado: Cerrada.

Diferido a fases posteriores (no bloquea el cierre, no son ambigüedades del modelo ni de los casos de uso):

- Diseño detallado del scraper, aplazado hasta una revisión específica (timeout, redirecciones, límite de bytes, tipos de contenido, contrato de background). Se retoma en la Fase 5.
- Estrategia de despliegue, que queda fuera del MVP0 local.

Resueltos:

- Reglas de visibilidad al alternar colección privada↔pública (todos los enlaces pasan a `IsPublic = false`).
- Orden de resultados de búsqueda (rank FTS5 → `CreatedAt` DESC → Id DESC).
- Orden de listados privados (igual que públicos).
- Comportamiento del scraper frente a metadatos ya informados (no sobrescribe).
- `Tag.CreatedAt` añadido al modelo.
- Pantalla de edición de usuario incluida en el ámbito del MVP0.
- `Name` de colecciones y etiquetas es único por usuario (consecuencia de la unicidad de `Slug`).
- Una colección o un tag pueden existir sin enlaces asociados.
- `Title`, `Description` e `Image` del enlace son nullable.
- `User.CreatedAt` es solo informativo/auditoría.
- Semántica de `Link.Retries` (intentos realizados, máx. 3) y `Link.NextTry` (fecha del siguiente intento, `null` al agotarse).

Criterio de finalización:

- No quedan reglas ambiguas que afecten al modelo de datos o a los casos de uso principales.

## Fase 2: Dominio y aplicación

Objetivos:

- Definir entidades e invariantes.
- Definir la regla común de generación de alias y slugs, además de propiedad y visibilidad.
- Definir casos de uso.
- Definir contratos `Result`.
- Definir contexto de usuario autenticado.
- Definir interfaces de repositorios y Unit of Work.

Criterio de finalización:

- Los casos de uso principales tienen contratos y criterios de aceptación claros.

## Fase 3: Persistencia

Objetivos:

- Preparar SQLite.
- Incorporar Dapper.
- Crear scripts SQL versionados.
- Añadir restricciones únicas e índices.
- Implementar transacciones.
- Implementar `LinkTag`.
- Implementar SQLite FTS5.

Criterio de finalización:

- Las operaciones de enlaces, colecciones y etiquetas son atómicas.
- La búsqueda respeta siempre el usuario actual.
- FTS5 se mantiene sincronizado con altas, cambios y borrados.
- Las transacciones son breves y no contienen operaciones externas.
- WAL, `busy_timeout` y claves foráneas están configurados.
- Las escrituras no mantienen conexiones ni transacciones abiertas más tiempo del necesario.

## Fase 4: Interfaz privada y gestión básica

Objetivos:

- Registrar e identificar usuarios (login).
- Editar la configuración del usuario (nombre, email, alias, contraseña).
- Completar la interfaz privada de gestión.
- Validar estados vacíos y errores accesibles.

Criterio de finalización:

- El usuario autenticado puede gestionar colecciones, enlaces y etiquetas.
- Todo enlace nuevo aparece inicialmente como privado.

## Fase 5: Scraping del MVP0

El scraping forma parte del MVP0, pero se implementará después de la gestión básica. El flujo descrito es provisional y podrá cambiar tras la revisión técnica específica.

Objetivos:

- Obtener `meta.title`, `meta.description` y `og.image` de la URL introducida por el usuario.
- Ejecutar el intento inicial al abandonar el campo URL.
- Programar un segundo intento silencioso a los 5 minutos.
- Programar un tercer y último intento silencioso 5 minutos después.
- Procesar los trabajos fuera de las transacciones de SQLite.
- Definir e implementar la protección SSRF y los límites operativos.

Criterio de finalización:

- El enlace se conserva aunque no haya metadatos.
- Los reintentos no bloquean las operaciones de base de datos.
- Los detalles provisionales quedan revisados y documentados.

## Fase 6: Área pública y fundamentos SEO

Objetivos:

- Crear rutas con alias para la página de usuario.
- Crear rutas con alias y slug para la página de colecciones.
- Mostrar colecciones públicas.
- Mostrar solo enlaces publicados.
- Ordenar por fecha descendente.
- Paginar a 15 enlaces.
- Añadir title, description y canonical básica.
- Mantener HTML semántico y SSR.
- Definir correctamente respuestas 404 y exclusiones de contenido privado.

Criterio de finalización:

- Las colecciones privadas no son accesibles.
- Los enlaces no publicados no aparecen.
- El HTML público se renderiza desde servidor y es rastreable.

### SEO avanzado posterior al MVP0

Objetivos:

- Añadir Open Graph.
- Añadir Twitter Cards, si procede.
- Generar `sitemap.xml`.
- Generar `robots.txt`.
- Revisar datos estructurados, si aportan valor.
- Auditar canonical, títulos, descripciones, paginación y rastreabilidad.

Este trabajo queda fuera del MVP0. Si se retoma, tendrá como criterio de finalización:

- Las páginas públicas tienen metadatos coherentes y no se ofrecen rutas indexables para contenido privado o no publicado.
- La estrategia de sitemap y paginación está documentada.

## Fase 7: Calidad

Objetivos:

- Tests de dominio.
- Tests de duplicados.
- Tests de `UrlNormalized`.
- Tests de alias único y mutable.
- Tests de generación común para alias, slug de colección y slug de etiqueta.
- Tests de slug autogenerado, invisible e inmutable.
- Tests de visibilidad.
- Tests de transacciones.
- Tests de FTS5.
- Tests de propiedad entre usuarios.
- Tests de visibilidad privada inicial de nuevos enlaces.
- Tests del flujo de scraping, sus reintentos y SSRF.
- Tests de accesibilidad.
- Pruebas en navegadores actuales.

Criterio de finalización:

- Las reglas críticas tienen cobertura automatizada.
- La interfaz puede recorrerse con teclado.
- El orden de foco es lógico.
- Los formularios anuncian correctamente sus errores.
- Los logs relevantes están disponibles.

## Fuera de este roadmap

- Importación y exportación de enlaces.
- Extensión de navegador.
- Colaboración.
- Recomendaciones.
- Crawling completo.
- Integraciones externas.
- Despliegue definitivo.
- SEO avanzado.
