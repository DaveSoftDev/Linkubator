# Plan técnico: S6.1 — Proyecciones FTS5 de enlaces y colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Infrastructure añadirá dos proyecciones FTS5 independientes al esquema gestionado por los scripts versionados: una de enlaces y otra de colecciones.
- La proyección de enlaces contendrá los campos asignados a esa entidad por [architecture.md → «FTS5»](../../../context/architecture.md#fts5) y [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros). En este trabajo no incluye nombres de etiquetas.
- La proyección de colecciones contendrá el campo indicado por la fuente técnica. Cada proyección mantendrá la asociación con el identificador y usuario de su entidad, sin mezclarlas.
- No se incorpora un contrato público de Application para consultar búsquedas; las consultas de lectura son trabajo posterior dentro de S6.

### Flujos internos

1. El ejecutor de scripts versionados aplica la evolución que crea las dos proyecciones.
2. La validación prepara datos controlados en la base de pruebas e inspecciona estructura, configuración de tokenización y asignación de entidades.
3. La implementación no realiza la sincronización con escrituras de dominio ni habilita búsquedas de aplicación; esas responsabilidades se entregarán en los trabajos posteriores definidos en [spec.md → «Alcance»](spec.md#alcance).

### Persistencia y dependencias

- Integrar el cambio al catálogo de scripts SQL versionados ya usado por Infrastructure; los datos auxiliares de tests deben permanecer separados según [S5.2.1](../s5.2.1-separar-scripts-prueba-catalogo/spec.md).
- Las proyecciones se crean mediante la extensión FTS5 de SQLite con las columnas e identificadores definidos por la arquitectura. No se incluye SQL ejecutable en este plan.
- No se añaden triggers: la arquitectura asigna a la aplicación las actualizaciones de datos relacionados. La sincronización atómica corresponde a S6.4.
- Prerrequisitos de base de datos, scripts, conexión y esquema se proveen por los trabajos S5 archivados y listados en [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos).

### Garantías técnicas

- Mantener separadas las proyecciones de enlaces y colecciones, de modo que identificadores coincidentes entre entidades no colisionen.
- Asociar cada fila con la identidad de usuario y entidad correctas; las consultas futuras deberán combinar la búsqueda con el usuario identificado y verificar la pertenencia de la entidad, conforme a [architecture.md → «FTS5»](../../../context/architecture.md#fts5) y [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos).
- El tokenizador debe cumplir las reglas técnicas y funcionales existentes de [architecture.md → «FTS5»](../../../context/architecture.md#fts5) y [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros).
- La incorporación de las proyecciones no modifica los valores originales ni implementa una regla de producto nueva.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener una proyección separada por tipo de entidad y excluir etiquetas de esta unidad. | confirmado | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), [mvp0-plan.md → S6.1](../../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) y decisión de alcance registrada en [spec.md](spec.md) | Evita mezclar identificadores y respeta la asignación S6.1; etiquetas se incorporan en su etapa propia. |
| Usar el catálogo versionado vigente, sin triggers ni sincronización de escrituras en este bloque. | confirmado | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [S5.2.1](../s5.2.1-separar-scripts-prueba-catalogo/spec.md) y [spec.md → «Alcance»](spec.md#alcance) | Mantiene la separación de responsabilidades; altas y cambios quedan pendientes de S6.4. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada: integración SQLite con la base de pruebas y el catálogo versionado. | Proyecto Tests, fábrica de base temporal y scripts existentes. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionMigrationTests` | Ambas proyecciones existen con columnas, asociación de identificadores y opciones de tokenización conformes a fuentes. | Resultado del test y versión SQLite. |
| V02 | CA03 | Automatizada: prueba de integración con datos de varios usuarios y entidades. | V01 y esquema S5. | El mismo filtro de V01; pruebas `ApplyPendingMigrations_WhenProjectionRowsHaveSameRowId_RemainsSeparatedByEntity` y `ApplyPendingMigrations_WhenProjectionRowsHaveDifferentUsers_FiltersByUser`. | Las pruebas detectan coincidencias en la proyección equivocada y para el usuario equivocado; la separación esperada pasa. | Tests, fixtures no sensibles, resultado y versión SQLite. |
| V03 | CA04 | Automatizada: aplicación del catálogo sobre base vacía y base con esquema previo. | Ejecutor de migraciones S5.2 y esquema S5.3. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteMigrationServiceTests|FullyQualifiedName~SqliteSearchProjectionMigrationTests|FullyQualifiedName~SqlitePersistenceIntegrationTests"` | Aplicación inicial y repetición admitida por el ejecutor; la migración V005 se aplica sobre V004 conservando datos, y los scripts de prueba no aparecen en el catálogo. | Tests, salida del historial de migraciones y resultado de ambos escenarios. |
| V04 | CA01–CA04 | Técnica: revisión de cambios y consistencia documental. | V01–V03. | `git diff --check` y revisión del script versionado junto a sus pruebas. | Sin errores de whitespace, SQL ejecutable fuera de scripts versionados ni referencias a artefactos futuros como evidencia cumplida. | Salida del comando, archivos revisados y fecha al completar tareas. |

Las validaciones V01–V03 comprueban el artefacto de esquema y su configuración, pero no demuestran la sincronización con altas, cambios o borrados de entidades ni la corrección de consultas de búsqueda; esas garantías pertenecen a S6.2–S6.6 y sus pruebas. No se deben completar comandos ni nombres de tests hasta identificarlos en la implementación.

## Orden de ejecución y cierre

- [Tareas](tasks.md) aprobadas por DLG el 19-X-2026 («Vamos»); implementación completada y validada el 19-X-2026.
- Criterios CA01–CA04 con evidencia satisfactoria; DLG aceptó y cerró el trabajo el 19-X-2026 con «Tenemos proyeciones». El cierre no depende de S6.2–S6.6.
- Ante una discrepancia de alcance o regla, detener la tarea afectada y tramitar el cambio en su documento propietario antes de continuar.
