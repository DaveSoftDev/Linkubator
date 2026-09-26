---
name: requirements-review
description: "Use when reviewing or synchronizing Linkubator requirements, domain model, architecture, decisions, or roadmap. Detect contradictions, stale decisions, missing cross-document updates, MVP0 scope drift, and undocumented assumptions before implementation."
---

# Requirements Review

## Purpose

Mantener coherente la documentación de Linkubator durante la fase de análisis de requisitos y antes de iniciar la implementación.

Esta skill revisa documentación. No genera código, proyectos .NET, SQL ejecutable, paquetes, migraciones ni configuración de despliegue.

## When to use

Usa esta skill cuando:

- Se añade o modifica un requisito.
- Se cambia una decisión de producto o arquitectura.
- Se pide revisar la documentación completa.
- Se incorporan entidades, propiedades, relaciones o reglas de negocio.
- Se necesita comprobar si una funcionalidad pertenece al MVP0.
- Se prepara el paso desde análisis hacia implementación.

No la uses para:

- Implementar funcionalidades.
- Resolver errores de código.
- Diseñar una API concreta.
- Optimizar consultas ya implementadas.
- Crear una solución .NET.

## Source of truth

Revisa estos documentos en este orden:

1. `context/requirements.md`
2. `context/domain-model.md`
3. `context/architecture.md`
4. `context/decisions.md`
5. `context/roadmap.md`
6. `AGENTS.md`

`context/decisions.md` distingue decisiones confirmadas, pendientes y fuera de alcance. `AGENTS.md` contiene las restricciones de trabajo del repositorio.

Si falta alguno de estos archivos, informa de ello como hallazgo. No inventes su contenido sin indicarlo.

## Review workflow

### 1. Gather context

Lee los documentos de la fuente de verdad y localiza:

- Alcance del MVP0.
- Entidades y propiedades.
- Relaciones e invariantes.
- Reglas de propiedad por usuario.
- Reglas de visibilidad y publicación.
- Decisiones confirmadas y pendientes.
- Tecnologías planificadas.
- Funcionalidades fuera de alcance.

No empieces editando. Primero formula una hipótesis concreta sobre la posible incoherencia y una comprobación que pueda confirmarla o descartarla.

### 2. Build a consistency matrix

Comprueba como mínimo:

| Área | Comprobación |
| --- | --- |
| Terminología | Los nombres de entidades, propiedades y estados son consistentes. |
| Alcance | Ningún documento introduce funcionalidades fuera del MVP0 como si estuvieran confirmadas. |
| Usuario | Toda lectura o escritura privada está limitada al usuario actual. |
| Alias | `Alias` tiene las mismas reglas de generación, longitud, mutabilidad, unicidad y palabras reservadas en todos los documentos. |
| Colecciones | El slug se genera con la regla común, es único dentro del usuario y no se trata como global. |
| URLs | La URL original se conserva y `UrlNormalized` se trata como clave plana de comparación, no como URL reconstruible. |
| Duplicados | La unicidad se aplica al usuario correcto. |
| Visibilidad | Una colección privada nunca expone enlaces. |
| Publicación | En una colección pública, cada enlace necesita publicación explícita. |
| Transiciones | Al volver pública una colección, sus enlaces siguen sin publicarse. |
| Búsqueda | La búsqueda nunca devuelve enlaces de otro usuario. |
| Etiquetas | La documentación usa “etiqueta” funcionalmente y `Tag`/`LinkTag` como nombres técnicos. |
| Persistencia | Dapper, SQL explícito, SQLite y FTS5 no se mezclan con EF Core. |
| Scraping | El scraping está dentro del MVP0, en fase posterior, con flujo provisional y reintentos a 5 minutos. |
| SEO | Solo las colecciones públicas tienen páginas indexables. |
| Accesibilidad | Los requisitos de teclado, foco, labels, errores y contraste no se contradicen. |
| Roadmap | Las fases reflejan las decisiones y pendientes actuales. |

### 3. Classify findings

Clasifica cada hallazgo como:

- `Contradicción`: dos documentos establecen reglas incompatibles.
- `Omisión`: una decisión necesaria no aparece en un documento que debería recogerla.
- `Deriva de alcance`: se introduce una capacidad fuera del MVP0.
- `Ambigüedad`: una regla admite interpretaciones relevantes.
- `Terminología`: se usan nombres distintos para el mismo concepto.
- `Riesgo`: la documentación es compatible, pero puede producir un problema durante la implementación.

Ordena los hallazgos por impacto:

1. Seguridad o privacidad.
2. Integridad de datos.
3. Alcance del producto.
4. Arquitectura.
5. SEO y accesibilidad.
6. Claridad editorial.

### 4. Report before editing

Devuelve primero un informe con:

- Estado general: `coherente`, `coherente con observaciones` o `requiere decisiones`.
- Hallazgos ordenados por prioridad.
- Documentos afectados.
- Decisión o aclaración necesaria.
- Recomendación concreta.
- Preguntas abiertas que bloquean la implementación.

No presentes una propuesta como decisión confirmada.

### 5. Apply synchronized documentation changes

Solo edita documentos si el usuario lo pide explícitamente o si la tarea consiste en actualizar la documentación tras una decisión ya confirmada.

Cuando edites:

- Actualiza todos los documentos afectados en la misma tarea.
- Mantén el castellano.
- Conserva la separación entre confirmado, pendiente y fuera de alcance.
- No añadas código ni instrucciones de implementación ejecutables.
- No borres decisiones históricas relevantes; resume la decisión actual y su razón.
- Evita reformatear secciones no relacionadas.

## Linkubator invariants

Estas invariantes deben verificarse siempre:

- Un usuario tiene un `Alias` único globalmente.
- El alias es visible, mutable, único globalmente, usa solo caracteres ASCII básicos, minúsculas, números y guiones, mide entre 10 y 25 caracteres y no puede ser `collection`, `collections`, `user`, `users`, `tag` ni `tags`.
- Un enlace pertenece a un único usuario y una única colección.
- Una colección pertenece a un único usuario.
- Una etiqueta técnica `Tag` pertenece a un único usuario.
- El slug de colección y de etiqueta es único dentro de su usuario.
- El slug de colección y de etiqueta se genera desde el nombre, tiene las mismas reglas de generación, longitud, mutabilidad, unicidad y palabras reservadas en todos los documentos. Admite desde 1 hasta 50 caracteres y es único por usuario. El algoritmo para su generación se explica en decisions.md (Generación de alias y slugs).
- `User.Alias`, `Collection.Slug` y `Tag.Slug` comparten el mismo método de generación, especificado en decisions.md (Generación de alias y slugs).
- La generación rechaza resultados vacíos y respeta la unicidad correspondiente de alias, slug de colección y slug de etiqueta.
- Una URL normalizada es única dentro de su usuario.
- `UrlNormalized` se auto calcula según las reglas especificadas en decisions.md (Normalización de URLs para duplicados).
- Se aceptan conscientemente colisiones de `UrlNormalized` derivadas de eliminar separadores.
- En HTTP se eliminan los puertos `80` y `8080`; en HTTPS se elimina el puerto `443`; los demás se conservan.
- Todo enlace nuevo se crea con `IsPublic = false`.
- Una colección con enlaces no puede eliminarse.
- La URL original de un enlace es inmutable.
- Al mover un enlace, queda despublicado y requiere publicación explícita.
- La visibilidad efectiva de un enlace es `Collection.IsPublic && Link.IsPublic`.
- Una colección privada no es accesible públicamente.
- Volver pública una colección no publica automáticamente sus enlaces.
- La búsqueda está limitada al usuario actual.
- La URL original siempre se conserva aunque exista una URL normalizada auxiliar.
- Un fallo de scraping no impide conservar el enlace.
- `LinkTag` y FTS5 se mantienen en la misma transacción que las operaciones de escritura o borrado relacionadas.

## Expected output

Usa este formato breve:

```text
Estado: [coherente | coherente con observaciones | requiere decisiones]

Hallazgos:
- [Prioridad] [Tipo] Descripción. Documentos afectados: ...

Decisiones necesarias:
- ...

Cambios documentales aplicados:
- Ninguno, si solo se solicitó revisión.
```

Si no encuentras problemas, indícalo claramente y menciona los riesgos residuales o decisiones pendientes.
