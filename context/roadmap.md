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

## Fase 1: Cierre de decisiones

Pendientes principales:

- Reglas exactas de normalización de URLs.
- Límites del scraper.
- Detalles de SEO y sitemap.
- Estrategia de despliegue, sin abordarla todavía en profundidad.

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

## Fase 4: Scraping

Objetivos:

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

## Fase 6: Área pública y SEO

Objetivos:

- Crear rutas con alias y slug.
- Mostrar colecciones públicas.
- Mostrar solo enlaces publicados.
- Ordenar por fecha descendente.
- Paginar a 15 enlaces.
- Añadir title, description, canonical y Open Graph.
- Añadir sitemap y robots.

Criterio de finalización:

- Las colecciones privadas no son accesibles.
- Los enlaces no publicados no aparecen.
- El HTML público se renderiza desde servidor y es rastreable.

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
- Tests de scraping y SSRF.
- Tests de accesibilidad.
- Pruebas en navegadores actuales.

Criterio de finalización:

- Las reglas críticas tienen cobertura automatizada.
- La interfaz puede recorrerse con teclado.
- El orden de foco es lógico.
- Los formularios anuncian correctamente sus errores.
- Los logs relevantes están disponibles.

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
