# Roadmap del MVP0

## Fase 0: Documentación

Objetivo: conservar una definición coherente antes de implementar.

Entregables:

- Requisitos del producto.
- Modelo de dominio.
- Arquitectura prevista.
- Decisiones confirmadas y pendientes.
- Roadmap.

Estado: en progreso.

## Fase 1: Cierre de decisiones estructurales

Pendientes principales:

- Reglas exactas de normalización de URLs.
- Diseño detallado del scraper, aplazado hasta una revisión específica.
- Fundamentos SEO que afectan a rutas, visibilidad y HTML.
- Estrategia de despliegue, que queda fuera del MVP0 local.

Criterio de finalización:

- No quedan reglas ambiguas que afecten al modelo de datos o a los casos de uso principales.

## Fase 2: Dominio y aplicación

Objetivos:

- Definir entidades e invariantes.
- Definir reglas de alias, slug, propiedad y visibilidad.
- Definir casos de uso.
- Definir contratos `Result`.
- Definir contexto de usuario fijo.
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

- Las operaciones de enlaces, colecciones y tags son atómicas.
- La búsqueda respeta siempre el usuario actual.
- FTS5 se mantiene sincronizado con altas, cambios y borrados.
- Las transacciones son breves y no contienen operaciones externas.
- WAL, `busy_timeout` y claves foráneas están configurados.
- Las escrituras no mantienen conexiones ni transacciones abiertas más tiempo del necesario.

## Fase 4: Scraping (aplazada)

Objetivos futuros, no incluidos en la implementación actual:

- Validar URLs.
- Implementar protección SSRF.
- Obtener title, description y og:image.
- Implementar intento inicial y dos reintentos.
- Procesar trabajos en segundo plano.
- Registrar logs operativos.

Criterio de finalización:

- Un fallo de scraping no impide guardar el enlace.
- Los reintentos se ejecutan con el intervalo definido.
- No se puede acceder a destinos internos o no permitidos.

Esta fase permanecerá pendiente hasta cerrar una política específica de scraping y SSRF.

## Fase 5: Interfaz privada

Objetivos:

- Crear panel de gestión con Razor Pages.
- Gestionar colecciones.
- Gestionar enlaces.
- Gestionar tags.
- Publicar y despublicar enlaces.
- Buscar y filtrar enlaces.
- Mostrar estados vacíos y errores accesibles.

Criterio de finalización:

- El usuario fijo puede completar el flujo completo de gestión sin acceder a datos fuera de su contexto.

## Fase 6: Área pública y fundamentos SEO

Objetivos:

- Crear rutas con alias y slug.
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
- Tests de alias único.
- Tests de slug por usuario.
- Tests de visibilidad.
- Tests de transacciones.
- Tests de FTS5.
- Tests de propiedad entre usuarios.
- Tests de accesibilidad.
- Pruebas en navegadores actuales.

Criterio de finalización:

- Las reglas críticas tienen cobertura automatizada.
- La interfaz puede recorrerse con teclado.
- El orden de foco es lógico.
- Los formularios anuncian correctamente sus errores.
- Los logs relevantes están disponibles.

Las pruebas de scraping y SSRF se incorporarán cuando se reactive y cierre esa fase.

## Fuera de este roadmap

- Registro de usuarios.
- Autenticación funcional.
- Importación y exportación.
- Extensión de navegador.
- Colaboración.
- Recomendaciones.
- Crawling completo.
- Integraciones externas.
- Despliegue definitivo.
- SEO avanzado.
