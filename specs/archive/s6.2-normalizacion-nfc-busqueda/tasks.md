# Tareas: S6.2 — Normalización NFC de búsqueda

## Referencias y autorización

- Estado: aprobadas; ejecución técnica completada y aceptación renovada por DLG («Sí, acepto S6.2», 20-X-2026), instrucción explícita en esta conversación.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Aprobar»), respuesta explícita al formulario de aprobación del plan.
  - Tareas: aprobadas por DLG el 19-X-2026 («Aprobar»), respuesta explícita al formulario de aprobación de las tareas.
- Implementación:
  - Estado: completada en el alcance aprobado (normalizador reutilizable y pruebas unitarias; sin integración con consumidores S6.3/S6.4).
  - Inicio: DLG solicitó iniciar la implementación el 19-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización técnica: 20-X-2026; `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SearchTextNormalizerTests` pasó 6/6 con SDK 10.0.401; `git diff --check` pasó.
  - Revisión de cierre: DLG pidió verificar tareas y evidencia el 19-X-2026 («ejecutar las tareas, evidencias... y asegurarte del estado»). Los tests se reejecutaron y la documentación se sincroniza con el alcance de normalizador aislado; la integración productiva se asigna a S6.3/S6.4 y las pruebas conjuntas a S6.7.
  - Evidencia consolidada: T01–T05 completadas; prueba focalizada 6/6 y revisión documental/diff limpios el 20-X-2026. La aceptación renovada sigue pendiente; no se declara cerrado ni archivado.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T03 depende de T02 porque cubre casos de borde del mismo normalizador. T04 integra validación focalizada y revisión documental. T05 actualiza evidencia y estado de navegación; la aceptación humana es una decisión separada.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar estructura de Infrastructure y Tests y confirmar ubicación y convenciones para el componente y sus pruebas, sin implementar la integración de etapas vecinas. | CA01–CA04; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | Ninguna | Confirmar el módulo adecuado para búsqueda y el patrón de tests unitarios; resultado documentado en esta tarea sin modificar fuentes funcionales. | completada | Infrastructure mantiene componentes por responsabilidad; normalizador en `Search/` y tests xUnit en la carpeta existente `Infrastructure/`. Verificado el 19-X-2026. |
| T02 | Implementar el normalizador NFC reutilizable para texto de búsqueda mediante la API de normalización Unicode de .NET, sin modificar las entidades. | CA01, CA04; [plan → «Flujos internos»](plan.md#flujos-internos), [V01](plan.md#estrategia-de-validación) | T01 | Prueba focalizada confirma que texto en forma descompuesta devuelve la representación NFC esperada, y la forma de entrada permanece intacta. | completada | `SearchTextNormalizer` implementa Form C en `src/Linkubator.Infrastructure/Search/SearchTextNormalizer.cs`; `SearchTextNormalizerTests` pasó el filtro focalizado 6/6 el 20-X-2026 con SDK 10.0.401. |
| T03 | Añadir pruebas de entradas NFC/precompuestas, ASCII, vacías, nulas y normalización repetida, preservando la cadena descompuesta de origen. | CA02, CA03; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), [V02](plan.md#estrategia-de-validación), [V03](plan.md#estrategia-de-validación) | T02 | Tests focalizados demuestran preservación del dato de origen, idempotencia y manejo sin error de entradas ya NFC y `null`. | completada | `SearchTextNormalizerTests` cubre descompuestas, NFC, texto mixto, ASCII, vacío, repetición y `null`; total focalizado 6/6 el 20-X-2026 con SDK 10.0.401. |
| T04 | Ejecutar y revisar la validación integrada del componente, los criterios y los límites respecto a S6.3–S6.4; corregir únicamente problemas dentro del alcance S6.2. | CA01–CA04; [plan → V01–V04](plan.md#estrategia-de-validación) | T02, T03 | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SearchTextNormalizerTests` pasa y `git diff --check` no reporta errores; no se exige la existencia de consumidores S6.3/S6.4. | completada | Reejecución el 20-X-2026: 6/6 pasan con .NET SDK 10.0.401; `git diff --check` limpio. Se confirmó en la spec y el plan que la integración desde consulta/escrituras pertenece a trabajos posteriores, no prerrequisito de S6.2. El blob CSS coincide con HEAD. |
| T05 | Sincronizar la evidencia y la navegación; registrar la aceptación humana. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Tareas, roadmap e índice reflejan cierre aceptado; no se inicia S6.3 ni se archiva automáticamente. | completada | Artefactos sincronizados el 20-X-2026; DLG aceptó S6.2 con «Completado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04 | V01, V04 | Satisfactoria: el helper devuelve NFC para entradas descompuestas y deja intacta la cadena fuente. | 20-X-2026; .NET SDK 10.0.401 | `SearchTextNormalizerTests`, ejecución focalizada 6/6. |
| CA02 | T01, T02, T03, T04 | V02, V04 | Satisfactoria: entradas ya NFC, ASCII y cadena vacía permanecen equivalentes y repetir normalización no altera la salida. | 20-X-2026; .NET SDK 10.0.401 | `SearchTextNormalizerTests`, ejecución focalizada 6/6. |
| CA03 | T01, T02, T03, T04 | V03, V04 | Satisfactoria: la entrada null produce null según el contrato del helper. | 20-X-2026; .NET SDK 10.0.401 | `SearchTextNormalizerTests`, ejecución focalizada 6/6. |
| CA04 | T01, T02, T04 | V01, V04 | Satisfactoria: la prueba confirma que normalizar el texto no modifica la cadena fuente. No pretende probar persistencia ni integración posterior. | 20-X-2026; .NET SDK 10.0.401 | `SearchTextNormalizerTests`, ejecución focalizada 6/6. |

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado por DLG («Completado», 20-X-2026).
- Criterios sin evidencia satisfactoria dentro del alcance aprobado de S6.2: ninguno; CA01–CA04 tienen pruebas focalizadas satisfactorias.
- Bloqueos y riesgos restantes: ninguno en S6.2. La respuesta «No aceptar» del 20-X-2026 rechazó iniciar S6.3 junto con el cierre; no señaló defectos en S6.2.
- Aceptación humana: DLG respondió «Completado» el 20-X-2026 para cerrar S6.2 por separado. No incluye el inicio de S6.3 ni el archivado.
- Archivado: completado el 21-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s6.2-normalizacion-nfc-busqueda/`.
- Fecha efectiva de archivado y comprobación de enlaces: 21-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado: ninguno para S6.2; el trabajo está aceptado, cerrado y archivado. S6.3 se gestionó como trabajo independiente.
