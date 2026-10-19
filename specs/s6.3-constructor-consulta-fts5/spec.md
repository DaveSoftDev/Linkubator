# Especificación: S6.3 — Constructor de consultas FTS5

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Proporcionar y verificar un componente reutilizable que convierta el texto de búsqueda ya normalizado en una expresión segura para FTS5, preservando los términos resultantes y señalando cuándo no hay texto que buscar.

## Fuentes

- [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros): segmentación de `q`, términos y su semántica.
- [architecture.md → «FTS5»](../../context/architecture.md#fts5): expresión de consulta para FTS5 y cuándo no ejecutarla.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación de S6.3.
- [S6.1 — «Proyecciones FTS5 de enlaces y colecciones»](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md): proyecciones consumidoras de la expresión.
- [S6.2 — «Normalización NFC de búsqueda»](../s6.2-normalizacion-nfc-busqueda/spec.md): normalizador previo del texto.

## Alcance

- Incluye:
  - Construir una expresión de consulta FTS5 a partir del texto de búsqueda normalizado, extrayendo sus secuencias de letras o números según las reglas propietarias.
  - Encerrar cada término en una expresión literal de FTS5 y combinar los términos mediante AND implícito, sin prefijos.
  - Representar el caso sin términos para que el consumidor omita la consulta FTS5.
  - Verificar mediante pruebas directas del componente las entradas con símbolos, operadores aparentes, varios términos, Unicode, y el caso sin términos.
- Excluye:
  - Normalizar NFC: corresponde a S6.2; este componente recibe el texto después de ese paso.
  - Ejecutar consultas, elegir una proyección o aplicar usuario, filtros, orden y paginación: corresponde a trabajos posteriores de persistencia.
  - Sincronizar o reconstruir las proyecciones, ni modificar datos de entidades.
  - Cambiar las reglas funcionales de búsqueda o la semántica de FTS5 establecida en las fuentes.
- Trabajo vecino y responsable de sus entregables:
  - S6.1 proporciona las proyecciones FTS5.
  - S6.2 proporciona la normalización NFC previa a la segmentación.
  - S6.5 ejecuta consultas sobre las proyecciones y decide el uso del componente en el flujo de búsqueda. La cobertura integrada de sincronización, aislamiento y búsqueda corresponde a S6.7; no es necesaria para cerrar esta unidad.

## Dependencias y prerrequisitos

- S6.1 está implementada y aceptada; las definiciones de FTS5 están disponibles.
- S6.2 está implementada y aceptada; el normalizador NFC existe como prerrequisito del procesamiento de entrada.
- Las reglas funcionales y el contrato de construcción están establecidos por las secciones enlazadas. No se requiere definir reglas nuevas.
- Los criterios de esta spec se comprueban directamente sobre el constructor y no dependen de las consultas integradas de S6.5 ni de la sincronización de S6.4.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [mvp0-plan.md → S6.3](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Texto que contiene secuencias de letras o números separadas por espacios y otros símbolos. | Se invoca directamente el constructor con el texto ya normalizado. | Produce un término por cada secuencia según la regla fuente, sin conservar los separadores como sintaxis de consulta. |
| CA02 | [architecture.md → «FTS5»](../../context/architecture.md#fts5), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Uno o más términos, incluidos términos iguales a operadores FTS5 como `AND`, `OR`, `NOT` o `NEAR`. | Se construye la expresión para FTS5. | Cada término se representa literalmente entre comillas y los términos se combinan con AND implícito; los operadores aparentes no alteran la lógica de búsqueda. |
| CA03 | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [mvp0-plan.md → S6.3](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Un término o una secuencia de términos. | Se construye la expresión de búsqueda. | La expresión no introduce búsqueda por prefijo ni operadores que amplíen los resultados respecto a los términos recibidos. |
| CA04 | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [architecture.md → «FTS5»](../../context/architecture.md#fts5) | Entrada vacía, formada solo por separadores o que no produce términos. | Se invoca el constructor. | Devuelve la representación de ausencia de consulta, permitiendo al consumidor omitir FTS5 en vez de ejecutar una consulta vacía. |
| CA05 | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Texto con letras y números Unicode y puntuación, una vez aplicado el paso previo de normalización NFC. | Se invoca el constructor. | Segmenta conforme a las categorías de caracteres establecidas, sin reinterpretar puntuación como operadores y sin alterar el texto de origen. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. La invocación desde consultas ejecutables y las pruebas de integración permanecen asignadas a las etapas S6.5 y S6.7, sin bloquear el cierre de S6.3. | [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | — | — |

## Artefactos relacionados

El plan técnico se creará tras la aprobación explícita de esta especificación.
