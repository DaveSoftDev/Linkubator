# Tareas: S6.3 — Constructor de consultas FTS5

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Tareas: DLG, 20-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
  - Estado: completada técnicamente en el alcance aprobado.
  - Inicio: DLG solicitó iniciar la implementación el 20-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 20-X-2026; `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SearchQueryBuilderTests` pasó 12/12 con .NET SDK 10.0.401; `git diff --check` quedó limpio.
  - Evidencia consolidada: T01–T05 completadas; resultado focalizado 12/12 y revisión documental/diff limpios el 20-X-2026. DLG aceptó y cerró S6.3 con «Completado» el 20-X-2026.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 depende de T01 para confirmar el punto de extensión en Search y el patrón actual de tests. T03 añade pruebas directas al componente implementado en T02. T04 ejecuta la validación focalizada y revisa la trazabilidad; T05 mantiene navegación y estado una vez existan evidencias. No se requiere el consumidor de consultas S6.5 para completar estas tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar el módulo Search de Infrastructure, sus namespaces y el patrón de tests para confirmar ubicación y convenciones del constructor y sus pruebas. | CA01–CA05; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | Ninguna | Confirmar el punto de extensión y el proyecto/carpeta de tests sin cambiar reglas funcionales; registrar la ubicación antes de implementar. | completada | Confirmado el 20-X-2026: el módulo está en `src/Linkubator.Infrastructure/Search/`; el normalizador existente usa el namespace `Linkubator.Infrastructure.Search`; el proyecto xUnit es `tests/Linkubator.Tests` y los tests de infraestructura se ubican bajo `Infrastructure/`. |
| T02 | Implementar el constructor reutilizable de consultas FTS5 en Infrastructure: segmentar entrada normalizada, producir consulta citada con AND implícito o indicar ausencia de consulta. | CA01–CA05; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), [V01–V04](plan.md#estrategia-de-validación) | T01 | Inspección y prueba focalizada confirman que se construye desde el texto normalizado, sin mutar la entrada ni ejecutar SQL; el caso sin términos retorna ausencia de consulta. | completada | `SearchQueryBuilder.Build` añadido en `src/Linkubator.Infrastructure/Search/SearchQueryBuilder.cs`; segmenta por categorías Unicode `L*`/`N*`, cita cada término y devuelve `null` cuando no hay términos. Cubierto por tests focalizados 12/12. |
| T03 | Añadir pruebas unitarias directas para separadores, Unicode, puntuación, términos operadores, combinación AND implícita, ausencia de prefijos y entrada sin términos. | CA01–CA05; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), [V01–V04](plan.md#estrategia-de-validación) | T02 | Los tests demuestran el formato exacto esperado y fallan ante operadores sin citar, ausencia de combinación implícita, prefijo añadido o consulta vacía. | completada | `SearchQueryBuilderTests` cubre términos en varios alfabetos, operadores, separadores, puntuación, casos sin términos, `null` y preservación de entrada. Ejecución focalizada 12/12 el 20-X-2026 con .NET SDK 10.0.401, incluyendo entradas de término único y categorías Unicode numéricas y marcas combinantes. |
| T04 | Ejecutar tests focalizados y revisión técnica/documental de criterios y límites de S6.3. Corregir solo problemas de este alcance. | CA01–CA05; [plan → V01–V05](plan.md#estrategia-de-validación) | T02, T03 | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SearchQueryBuilderTests` pasa; `git diff --check` está limpio y la revisión confirma que el cierre no depende de S6.5/S6.7. | completada | 20-X-2026: tests focalizados pasaron 12/12 con .NET SDK 10.0.401 tras ampliar los casos; `git diff --check` limpio. No se ejecuta FTS5 integrado, conforme al alcance. |
| T05 | Sincronizar la evidencia obtenida en tareas, actualizar referencias de navegación en el roadmap e índice y preparar el trabajo para revisión/aceptación humana. | CA01–CA05; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Evidencia por criterio apunta a comprobaciones ejecutadas y el roadmap/índice reflejan solo el estado real; la aceptación humana permanece pendiente hasta recibirla. | completada | Actualizados el resumen de implementación, la evidencia por criterio, el roadmap y el índice el 20-X-2026; aceptación humana pendiente. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T04 | V01, V05 | Satisfactoria: secuencias de letras y números se convierten en términos separados y citados; los separadores no se incorporan como sintaxis. | 20-X-2026; .NET SDK 10.0.401 | `SearchQueryBuilderTests`, ejecución focalizada 12/12, reejecutada tras ampliar los casos límite. |
| CA02 | T01–T04 | V02, V05 | Satisfactoria: términos `AND`, `OR`, `NOT` y `NEAR` se citan individualmente y la salida los combina mediante AND implícito. | 20-X-2026; .NET SDK 10.0.401 | `SearchQueryBuilderTests`, ejecución focalizada 12/12. |
| CA03 | T01–T04 | V03, V05 | Satisfactoria: la salida no añade prefijos y conserva como términos completos las secuencias recibidas. | 20-X-2026; .NET SDK 10.0.401 | `SearchQueryBuilderTests` 12/12, salida exacta de un término y varios términos; revisión técnica. |
| CA04 | T01–T04 | V04, V05 | Satisfactoria: cadena vacía, separadores sin términos y `null` devuelven ausencia de expresión. | 20-X-2026; .NET SDK 10.0.401 | `SearchQueryBuilderTests`, ejecución focalizada 12/12. |
| CA05 | T01–T04 | V01, V05 | Satisfactoria: se prueban letras de varios alfabetos, categorías Unicode numéricas y una marca combinante; la puntuación/separadores delimitan términos y la entrada permanece intacta. | 20-X-2026; .NET SDK 10.0.401 | `SearchQueryBuilderTests`, ejecución focalizada 12/12. |

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado por DLG («Completado», 20-X-2026).
- Criterios sin evidencia satisfactoria dentro del alcance: ninguno; CA01–CA05 tienen evidencia focalizada satisfactoria.
- Bloqueos y riesgos restantes: ninguno comprobado dentro de S6.3. La ejecución integrada contra SQLite FTS5 y las consultas de entidad pertenecen a trabajos posteriores y no bloquean este cierre.
- Aceptación humana: DLG aceptó explícitamente el resultado con «Completado» el 20-X-2026, en esta conversación.
- Archivado: completado el 21-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s6.3-constructor-consulta-fts5/`.
- Fecha efectiva de archivado y comprobación de enlaces: 21-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: ninguno para S6.3; el trabajo está cerrado y archivado para su alcance.
