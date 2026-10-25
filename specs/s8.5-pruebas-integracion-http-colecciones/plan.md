# Plan técnico: S8.5 — Pruebas de integración HTTP de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: DLG, 25-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- El proyecto `Linkubator.Tests` será el único componente productivo de este trabajo: ejercitará el host real de Web mediante `WebApplicationFactory<Program>`, `HttpClient` y una base SQLite temporal e independiente por escenario, siguiendo el patrón ya presente en [CollectionsListingIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/CollectionsListingIntegrationTests.cs).
- Las pruebas observarán tanto respuestas HTTP/HTML como el estado persistido usando la conexión de pruebas. La identidad procede del proveedor de identidad de desarrollo configurado en el host; los valores de cliente como `userId` no se tratarán como autoridad.
- Se ampliará o reorganizará el conjunto de pruebas de integración existente en función del inventario que se confirme al iniciar el trabajo. Se evitarán duplicar assertions y fixtures de S8.1–S8.4 que ya cubran los mismos contratos; cada CA tendrá trazabilidad explícita a tests concretos.
- No se cambian código de producción, contratos de Application, reglas, esquema de base de datos ni la suite transversal S7.9. Los endpoints y sus resultados permanecen bajo responsabilidad de las páginas Web y casos de uso existentes.

### Flujos internos

1. Preparar un host con almacenamiento temporal, identidad configurada y cliente HTTP con redirecciones controladas cuando el criterio necesite inspeccionar la respuesta POST.
2. Sembrar los datos mínimos del usuario identificado y, cuando aplique, de un usuario distinto; registrar los valores iniciales relevantes para comparar.
3. Obtener tokens antiforgery desde la página GET canónica antes de cada operación protegida y enviar las peticiones a rutas/métodos definidos por la fuente.
4. Afirmar la respuesta HTTP y el contenido observable sin basar la comprobación únicamente en markup; verificar las filas persistidas y las invariantes de usuario antes/después.
5. Ejecutar los tests focalizados, probar sensibilidad mediante una perturbación controlada de una aserción representativa y restaurar inmediatamente su forma aprobada.

### Persistencia y dependencias

- No se añaden migraciones, SQL productivo ni cambios al almacenamiento. SQL de preparación se limita a los fixtures de Tests y respeta el esquema vigente.
- El host de pruebas ejecuta migraciones y aprovisiona la identidad según la configuración de `WebApplicationFactory`; cada caso elimina los archivos temporales al terminar.
- Prerrequisitos disponibles: proyectos Web y Tests, `WebApplicationFactory`, SQLite y páginas de colecciones. S8.1–S8.4 aportan los comportamientos a integrar; ninguna aceptación técnica vecina ni S8.6–S8.7 es dependencia de cierre de S8.5.
- Cuando un fallo del host o de una operación revele discrepancia con un criterio aprobado, se bloquea la comprobación implicada y se remite al artefacto propietario. No se corrige un contrato de producto dentro de una spec de pruebas.

### Garantías técnicas

- Cada escenario usa una base aislada y datos identificables para evitar interferencia entre pruebas.
- Los tests no usan parámetros de petición para seleccionar el usuario autorizado ni exponen en la respuesta colecciones de otros usuarios.
- Las acciones de escritura se realizan por los métodos canónicos con antiforgery; el GET de confirmación se comprueba como no mutante.
- Para operaciones sobre recursos válidos no accesibles se comparan respuestas y estado para asegurar el tratamiento no discriminante definido por las fuentes.
- La comprobación negativa se limitará a una aserción de protección de usuario/no divulgación o antiforgery, se ejecutará de forma aislada, y el archivo se restaurará antes de la ejecución final satisfactoria. Su resultado no se presentará como prueba de una explotación real.
- No se atribuyen a estos tests garantías de UI, accesibilidad manual, seguridad transversal ajena a S8 ni comportamiento que no haya sido ejercitado por HTTP.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el `WebApplicationFactory` y el patrón de SQLite temporal ya utilizados por la prueba de integración de colecciones. | propuesta | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos), [CollectionsListingIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/CollectionsListingIntegrationTests.cs). | Aprovecha el harness existente; el inventario y los helpers compartidos se confirman en la primera tarea, evitando refactor amplio. |
| Organizar las pruebas por contratos observables y mantener una matriz test → CA, extendiendo la fixture existente o creando fixtures separadas cuando reduzca acoplamiento. | propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación). | Facilita detectar brechas y evita concentrar responsabilidades innecesarias; decidir estructura al inspeccionar cobertura actual. |
| Verificar estado persistido además de respuestas HTTP en operaciones exitosas y rechazadas. | confirmado | [AC-COL-01–AC-COL-05](../../plans/mvp0-acceptance-criteria.md#ac-col-01-crear-colección), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos). | El markup por sí solo no demuestra que no haya cambios parciales ni modificaciones ajenas. |
| Hacer una perturbación reversible representativa para comprobar sensibilidad de tests, además de ejecutar la suite normal. | confirmado | [spec.md → CA06](spec.md#criterios-de-aceptación). | Requiere restaurar y comprobar el estado final de los tests; un build fallido no cuenta como sensibilidad. |
| Los tests o la validación pueden revelar una regresión preexistente de listado o un bloqueo por procesos que mantengan ensamblados. | pendiente | Estado de cierre registrado para S8.2/S8.4 en [roadmap.md → «Next»](../../context/roadmap.md#next). | Distinguir el fallo del criterio de la suite y el bloqueo de entorno; registrar el resultado exacto y no cambiar alcance para ocultarlo. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada HTTP; listado con y sin búsqueda, paginación, estado vacío, parámetros no aplicables y datos de usuario ajeno, según fuente. | Harness Web y semillas aisladas; comportamiento de S8.1 disponible. | Ejecutar los tests focalizados de `CollectionsListingIntegrationTests`; filtro exacto confirmado al descubrir el runner/proyecto si cambia el nombre o fixture. | Respuestas reflejan la búsqueda/listado definidos, no incluyen datos ajenos, y parámetros no aplicables no alteran la consulta. | Nombres de tests, comando, conteo, fecha y contenido/respuesta relevante sin valores sensibles. |
| V02 | CA02 | Automatizada HTTP y persistencia; creación y edición válidas, junto con rechazos de formulario cubiertos por criterios propietarios. | Páginas de alta/edición, colección identificada, token antiforgery. | Ejecutar los tests focalizados de creación/edición en el proyecto Tests. | Respuesta y estado persistido coinciden con los criterios; rechazo deja los valores originales intactos. | Test/criterio, petición resumida, status/redirect, snapshot antes/después y resultado. |
| V03 | CA03 | Automatizada HTTP y persistencia; GET confirmatorio, POST aceptado y rechazos de las reglas de eliminación propietarias. | Página canónica de eliminación, estado de colección propio y token antiforgery para POST. | Ejecutar los tests focalizados de eliminación en el proyecto Tests con redirecciones según el caso. | GET conserva estado; POST elimina solo cuando corresponde, en otro caso comunica el rechazo y mantiene los datos. | Test/criterio, status, ubicación si procede y estado antes/después. |
| V04 | CA04 | Automatizada HTTP y persistencia; recurso ajeno/inexistente e identificador inválido en sus rutas aplicables. | Dos usuarios y datos diferenciables; pruebas de S8.4 integradas o nuevas. | Ejecutar los tests focalizados de identificadores/respuestas y la suite de colecciones. | Respuestas de inaccesibilidad no distinguen inexistente de ajeno; ningún caso revela o modifica datos de otro usuario. | Pares comparados, respuestas normalizadas cuando haya antiforgery, estado de ambos usuarios y resultado. |
| V05 | CA05 | Automatizada HTTP; rechazo sin token y control con token en operaciones de escritura de colecciones. | Cliente que mantiene cookies antiforgery y endpoint productivo protegido disponible. | Ejecutar pruebas focalizadas antiforgery en la fixture de colecciones. | Sin token válido la operación no se aplica; con token válido se alcanza el comportamiento previsto por el formulario. | Status y estado antes/después; no registrar el valor del token. |
| V06 | CA06 y CA01–CA05 | Automatizada, prueba de sensibilidad; alterar temporalmente una aserción de antiforgery o aislamiento para que contradiga el comportamiento observado, ejecutar solo el test correspondiente, restaurar y reejecutar suite focalizada. | Tests finales implementados y entorno sin bloqueos. | Ejecutar el test objetivo en la perturbación y después el comando focalizado restaurado. | El caso perturbado falla por la aserción del comportamiento, no por build/harness; el estado restaurado pasa. | Comandos y salidas de ambas ejecuciones, test afectado, fecha y confirmación de restauración. |
| V07 | CA01–CA06 | Integrada automatizada; comprobar el conjunto cubierto y cambios documentales mínimos. | V01–V06 satisfactorios. | Ejecutar la suite focalizada `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~CollectionsListingIntegrationTests" --no-restore -m:1 -p:BuildInParallel=false` si el proyecto/filtro se confirma vigente; además `git diff --check`. | Todos los tests relacionados pasan; diff check limpio; no se introducen cambios productivos. | Salida resumida, conteos, versión/entorno pertinente, resultado de diff check y fecha. |

La suite focalizada no demuestra recorrido visual completo, lector de pantalla, contraste, navegación manual ni cobertura exhaustiva de todas las garantías generales de S7. Esas comprobaciones siguen sus documentos propietarios.

## Orden de ejecución y cierre

- Tras aprobar este plan se creará `tasks.md` desde su plantilla; incluirá primero el inventario de cobertura/harness, luego las pruebas faltantes por criterio, la sensibilidad negativa y la validación integrada final.
- CA01–CA05 deben quedar vinculados a pruebas concretas; CA06 verifica sensibilidad y restauración. Toda tarea propia tendrá un check inmediato y evidencia pendiente hasta ejecutarse.
- El cierre de S8.5 requiere criterios probados con evidencia satisfactoria, sin bloqueos, tareas propias completadas y aceptación humana registrada. No depende del cierre global de S8 ni de evidencia manual de S8.6–S8.7.
- Si se descubre una diferencia entre el contrato implementado y las fuentes aprobadas, se detiene el criterio afectado y se registra la pregunta al propietario; no se cambia comportamiento ni documentación fuente por inferencia.
