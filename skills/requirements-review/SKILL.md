---
name: requirements-review
description: "Use when reviewing, synchronizing or writing Linkubator requirements, domain model, specifications, architecture, decisions, or roadmap. Detect contradictions, duplicated rules, content written in the wrong document, missing cross-references, MVP0 scope drift, and undocumented assumptions before implementation. Also builds on-demand entity sheets that gather every rule about one entity across documents."
---

# Requirements Review

## Purpose

Mantener coherente la documentación de Linkubator durante la fase de análisis de requisitos y antes de iniciar la implementación.

Esta skill revisa documentación. No genera código, proyectos .NET, SQL ejecutable, paquetes, migraciones ni configuración de despliegue.

## When to use

Usa esta skill cuando:

- Se añade o modifica un requisito, una regla o una decisión. Úsala antes de escribir, para decidir en qué documento va el texto.
- Se pide revisar la documentación completa.
- Se incorporan entidades, propiedades, relaciones o reglas de negocio.
- Se necesita comprobar si una funcionalidad pertenece al MVP0.
- Se prepara el paso desde análisis hacia implementación.
- Se pide la ficha de una entidad (por ejemplo, «ficha de `User`»). Ver «Entity sheet».

No la uses para implementar funcionalidades, resolver errores de código, diseñar una API concreta ni crear una solución .NET.

## Source of truth

Documentos y su responsabilidad (la tabla completa está en AGENTS.md → «Dónde vive cada cosa»):

1. `context/requirements.md`: qué hace el producto y para quién.
2. `context/domain-model.md`: entidades, propiedades, relaciones e invariantes.
3. `context/specifications.md`: reglas exactas (algoritmos, límites, valores, mensajes y respuestas).
4. `context/architecture.md`: cómo se construye.
5. `context/decisions.md`: qué se decidió y por qué, riesgos aceptados, pendientes y fuera de alcance.
6. `context/roadmap.md`: orden de construcción, checklist de calidad y backlog.
7. `AGENTS.md`: reglas de trabajo e invariantes críticas.

Si falta alguno de estos archivos, informa de ello como hallazgo. No inventes su contenido sin indicarlo.

## Review workflow

### 1. Gather context

Lee completos los documentos de la fuente de verdad. No te fíes de fragmentos de búsqueda para dar una regla por comprobada.

No empieces editando. Primero formula una hipótesis concreta sobre la posible incoherencia y una comprobación que pueda confirmarla o descartarla.

### 2. Check placement

Recorre cada documento párrafo a párrafo y viñeta a viñeta:

- Clasifica cada texto con la tabla de AGENTS.md → «Dónde va cada texto».
- Si su tipo no corresponde al documento en el que está, es un hallazgo de tipo `Ubicación incorrecta`, aunque el texto sea correcto y no esté repetido. Indica el documento de destino.
- Aplica las señales de AGENTS.md → «Dónde va cada texto». Las más habituales:
  - `requirements.md`: valores exactos, nombres de clases o propiedades usados como regla, SQL, cabeceras HTTP, capas o librerías.
  - `domain-model.md`: capas, transacciones, índices, formatos de almacenamiento, rutas, SEO o comportamiento de la interfaz.
  - `specifications.md`: justificaciones de por qué se decidió algo, tecnologías o capas.
  - `architecture.md`: listas de propiedades de entidades, reglas de negocio (qué puede hacer el usuario, cuándo algo es público) o valores del producto.
  - `decisions.md`: reglas completas, algoritmos o tablas de valores, en lugar de un enlace.
  - `roadmap.md`: reglas, valores o historial de decisiones.
  - `AGENTS.md` y skills: reglas del producto, salvo las invariantes críticas de `AGENTS.md`.
- No son hallazgo: nombrar una entidad o propiedad para enlazarla, un resumen sin valores que remite a su fuente, ni describir qué casos cubre una regla o qué consigue un mecanismo técnico.

### 3. Check single source

Para cada regla, valor o algoritmo:

- Identifica su documento propietario según la tabla de responsabilidades.
- Comprueba que solo está escrito ahí. En los demás documentos solo puede aparecer un enlace o un resumen sin valores.
- Si aparece un valor (un número, una lista, un código de respuesta, un nombre de parámetro) fuera de su documento propietario, es un hallazgo de tipo `Duplicado`, aunque hoy coincida.
- Comprueba que cada referencia apunta a una sección que existe y sigue el formato de AGENTS.md → «Reglas de documentación» (`documento.md → «Sección»`, un solo nivel). Si no, es un hallazgo de tipo `Terminología`.
- Recorre también los diagramas de `context/diagrams/`: un número, un límite o una lista cerrada de valores dentro de un diagrama es un `Duplicado` (ver AGENTS.md → «Dónde va cada texto»). Comprueba que sus enlaces relativos (`../`) resuelven.

### 4. Build a consistency matrix

Comprueba como mínimo:

| Área | Comprobación |
| --- | --- |
| Responsabilidades | Cada documento contiene solo lo que le corresponde según `AGENTS.md`. |
| Terminología | Nombres de entidades, propiedades y estados consistentes. Vocabulario de lo público según AGENTS.md → «Reglas de documentación». |
| Alcance | Ningún documento presenta como confirmada una funcionalidad fuera del MVP0. |
| Propiedad | Toda lectura o escritura privada está limitada al usuario identificado, y la invariante de `UserId` entre enlace, colección y etiquetas se respeta en casos de uso y rutas. |
| Público y privado | Las invariantes de domain-model.md → «Público y privado» se respetan en requisitos, especificaciones de páginas públicas y casos de uso. |
| Cuenta y seguridad | Los flujos de specifications.md → «Cuenta», «Contraseñas», «Correo», «Tokens» y «Sesión» son coherentes entre sí: qué correo sale en cada flujo, qué tokens se invalidan, qué sesiones se cierran y qué respuestas son genéricas. |
| URLs | El ajuste, la validación y la normalización se aplican donde corresponde (enlaces e `Image`) y `UrlNormalized` se trata como clave plana. |
| Listados y búsqueda | Orden, paginación y alcance por usuario coinciden entre specifications.md y architecture.md → «FTS5». |
| Páginas públicas | Rutas, indexabilidad y respuestas coinciden entre specifications.md, architecture.md → «Enrutamiento» y requirements.md. |
| Persistencia | Las operaciones que deben ser atómicas están en architecture.md → «Persistencia», y no hay operaciones externas dentro de transacciones. |
| Scraping | El flujo funcional (specifications.md), las medidas técnicas (architecture.md) y los pendientes (decisions.md) no se contradicen. |
| Decisiones | Cada decisión de decisions.md enlaza a su regla y tiene motivo cuando no es obvio. |
| Roadmap | Las etapas y la checklist cubren todas las reglas y reflejan los pendientes actuales. |

### 5. Classify findings

- `Contradicción`: dos documentos establecen reglas incompatibles.
- `Duplicado`: una regla o un valor aparece fuera de su documento propietario.
- `Ubicación incorrecta`: un texto está en un documento que no le corresponde, aunque no esté repetido.
- `Omisión`: falta una regla necesaria, o un enlace a ella donde se usa.
- `Deriva de alcance`: se introduce una capacidad fuera del MVP0.
- `Ambigüedad`: una regla admite interpretaciones relevantes.
- `Terminología`: se usan nombres distintos para el mismo concepto.
- `Riesgo`: la documentación es coherente, pero puede producir un problema al implementar.

Ordena los hallazgos por impacto:

1. Seguridad o privacidad.
2. Integridad de datos.
3. Alcance del producto.
4. Arquitectura.
5. SEO y accesibilidad.
6. Claridad editorial.

### 6. Report before editing

Devuelve primero un informe con:

- Estado general: `coherente`, `coherente con observaciones` o `requiere decisiones`.
- Hallazgos ordenados por prioridad, con archivo y línea de cada lugar implicado.
- Decisión o aclaración necesaria.
- Recomendación concreta.
- Preguntas abiertas que bloquean la implementación.

No presentes una propuesta como decisión confirmada.

### 7. Apply synchronized documentation changes

Solo edita documentos si el usuario lo pide explícitamente o si la tarea consiste en actualizar la documentación tras una decisión ya confirmada.

Cuando edites:

- Antes de escribir, clasifica cada texto con AGENTS.md → «Dónde va cada texto». Si no encaja en el documento que estás editando, escríbelo en su propietario; nunca lo dejes «de momento» donde no corresponde.
- Escribe la regla solo en su documento propietario y enlázala desde los demás.
- Si la decisión tiene un motivo, añádelo en decisions.md.
- Si la regla necesita un test, añádelo a la checklist de roadmap.md.
- Mantén el castellano y la separación entre confirmado, pendiente y fuera de alcance.
- No añadas código ni instrucciones de implementación ejecutables.
- Evita reformatear secciones no relacionadas.

## Entity sheet

Cuando se pida la ficha de una entidad (`User`, `Collection`, `Link`, `Tag`, `LinkTag`, `UserToken`):

1. Busca en todos los documentos de la fuente de verdad cada regla, propiedad, caso de uso, ruta, límite, test de la checklist y elemento fuera de alcance que afecte a esa entidad, incluidas sus menciones funcionales («usuario», «colección», «enlace», «etiqueta»).
2. Agrúpalas por tema: propiedades, generación y validación, público y privado, ciclo de vida (alta, cambios, borrado), rutas y páginas, seguridad, pendientes y fuera de alcance.
3. Enlaza cada regla con su fuente (`archivo#Lnn`).
4. Señala al final las contradicciones, duplicados u omisiones que detectes.

La ficha es una vista generada bajo demanda: devuélvela en la conversación y no la guardes en ningún archivo, para que no se desincronice de la documentación.

## Expected output

```text
Estado: [coherente | coherente con observaciones | requiere decisiones]

Hallazgos:
- [Prioridad] [Tipo] Descripción. Lugares: archivo#Lnn, archivo#Lnn. Destino (si es Ubicación incorrecta o Duplicado): documento.md → «Sección». Recomendación: ...

Decisiones necesarias:
- ...

Cambios documentales aplicados:
- Ninguno, si solo se solicitó revisión.
```

Si no encuentras problemas, indícalo claramente y menciona los riesgos residuales y las decisiones pendientes.
