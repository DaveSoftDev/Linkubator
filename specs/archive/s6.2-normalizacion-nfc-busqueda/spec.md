# Especificación: S6.2 — Normalización NFC de búsqueda

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Proporcionar y verificar el componente reutilizable que normaliza a Unicode NFC una cadena destinada al procesamiento de búsqueda, sin modificar el texto recibido.

## Fuentes

- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros): normalización Unicode exigida para `q` y el texto indexado.
- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): ubicación de la normalización respecto de tokenización y proyecciones.
- [domain-model.md → «Link»](../../../context/domain-model.md#link): propiedades originales del enlace y su inmutabilidad aplicable.
- [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia): orden y alcance de S6.2.
- [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l): asignación de esta unidad.
- [S6.1 — «Proyecciones FTS5 de enlaces y colecciones»](../s6.1-proyecciones-fts5-enlaces-colecciones/spec.md): especificación aprobada de las proyecciones base.

## Alcance

- Incluye:
  - Proporcionar el normalizador reutilizable que convierte texto de búsqueda a Unicode NFC.
  - Comprobar mediante tests directos del componente las formas descompuestas y precompuestas, entradas ya normalizadas, repetición, valores vacíos y nulos, y que el texto fuente no se modifica.
  - Mantener intactos los valores originales de las entidades; la normalización se limita a la representación procesada por el componente.
- Excluye:
  - Conectar el normalizador al procesamiento de `q`, construir consultas FTS o ejecutar búsquedas; corresponden a S6.3 y S6.5.
  - Incorporar o sincronizar proyecciones en las escrituras de las entidades; corresponde a S6.4.
  - Implementar la reconstrucción manual; corresponde a S6.6.
  - Cambiar otras normalizaciones de dominio o las reglas funcionales de búsqueda.
- Trabajo vecino y responsable de sus entregables:
  - S6.1 proporciona las proyecciones FTS5.
  - S6.3 usa la normalización en el procesamiento de `q` y construye la consulta; S6.4 la aplica al texto incorporado a las proyecciones y sincroniza las escrituras.
  - S6.5 y S6.6 implementan consultas y reconstrucción, respectivamente. Su ejecución e integración no son prerrequisitos para cerrar S6.2.

## Dependencias y prerrequisitos

- S6.1 está implementada y aceptada; sus proyecciones de enlaces y colecciones están disponibles en [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia).
- Las reglas propietarias para la normalización de búsqueda están documentadas en [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) y [architecture.md → «FTS5»](../../../context/architecture.md#fts5).
- Las comprobaciones de esta spec se limitan al componente normalizador y no dependen de que estén implementados S6.3–S6.6.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [mvp0-plan.md → S6.2](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | Una cadena con caracteres Unicode descompuestos, equivalente a una forma NFC precompuesta. | Se invoca directamente el normalizador. | Devuelve la forma equivalente en NFC; la cadena fuente permanece intacta. |
| CA02 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) | Una cadena ya normalizada en NFC, texto ASCII o cadena vacía. | Se invoca el normalizador una o más veces. | Devuelve contenido equivalente en NFC y la repetición no cambia el resultado ni produce error. |
| CA03 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) | Una entrada `null`. | Se invoca el normalizador. | Devuelve `null`, sin error ni sustitución por texto por defecto. |
| CA04 | [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) | Una cadena Unicode que contiene marcas combinantes. | Se invoca el normalizador sin reasignar ni mutar la cadena de entrada. | El helper devuelve una representación NFC y la cadena de entrada conserva su valor original. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. La integración del normalizador en los flujos de consulta e indexación y sus pruebas conjuntas corresponden a las tareas posteriores de S6. | [mvp0-plan.md → S6.2–S6.7](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [architecture.md → «FTS5»](../../../context/architecture.md#fts5) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 19-X-2026 («Aprobar»).
- [Tareas y cierre](tasks.md#aceptación-y-cierre): T01–T05 completadas; aceptada por DLG («Completado», 20-X-2026).
