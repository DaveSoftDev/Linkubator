# Plan técnico: S10.5 — Pruebas de integración del listado de enlaces

## Estado y aprobación del plan

- Estado: propuesta.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: pendiente; registrar responsable, fecha y referencia real cuando se apruebe.

## Diseño de implementación

### Componentes y contratos

- [LinkListingIntegrationTests](../../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs) es la suite HTTP focalizada existente del listado. Reutilizar su `WebApplicationFactory`, SQLite en memoria, usuarios/colecciones aislados y helpers de creación de datos; ampliar o ajustar casos de forma que cada comprobación sea independiente y estable.
- La página [LinksModel](../../../src/Linkubator.Web/Pages/App/Links.cshtml.cs) depende del analizador Web de parámetros y consulta `ILinkService.ListLinksPage`; las pruebas HTTP deben observar tanto la respuesta renderizada como los datos que aparecen, sin duplicar implementación de búsqueda.
- Application e Infrastructure son responsables de la búsqueda, el ranking, el filtrado por colección, el aislamiento por identidad, la paginación y la proyección FTS5. En esta spec se prueban a través del límite HTTP y, cuando sea necesario para demostrar identidad de entidad, mediante comprobaciones de datos de lectura controladas por los tests; no se modifica el contrato productivo por conveniencia de test.
- El criterio de aislamiento por tipo de entidad necesita un test HTTP que consulte también el listado de colecciones o use la cobertura HTTP ya existente de ese listado. Reutilizar las pruebas de listado de colecciones disponibles en lugar de introducir una prueba de unidad artificial que no atraviese el comportamiento real.
- Los tests solo pueden insertar datos de preparación en su base temporal aislada. No escribir fixtures, datos semilla ni cambios sobre bases compartidas o locales.

### Flujos internos

1. Crear por test un host Web con base SQLite en memoria aislada y usuarios/colecciones propios y ajenos.
2. Insertar enlaces de prueba usando el mismo mecanismo de proyección FTS5 que usan los tests existentes; seleccionar datos de manera que los resultados discriminen efectivamente el filtro de colección, el ranking, la página y el propietario.
3. Enviar peticiones GET a los listados privados con parámetros controlados y comprobar status, tarjetas/resultados, orden y controles de paginación o error que sean parte de la respuesta fuente.
4. Para el ranking, usar distintos grados de coincidencia y desempates con orden de inserción distinto del esperado. Para la combinación `q` y `c`, preparar coincidencias dentro y fuera de la colección y al menos un registro ajeno con coincidencia.
5. Para el aislamiento entre tipos, enviar el mismo término a listado de enlaces y de colecciones y comprobar que cada respuesta solo contiene su entidad propia.
6. Para parámetros, cubrir solamente los casos propietarios de enlaces definidos en CA06 y observar los resultados, no solo el mensaje de respuesta. Los parámetros no aplicables o repetidos no deben causar que se aplique un valor posterior o un filtro de capacidad aún no disponible.
7. Al terminar, validar el conjunto focalizado relacionado, revisar trazabilidad de CA01–CA06 y el diff documental/de tests. Si se detecta una regla fuente incoherente, detener únicamente la comprobación afectada y elevar la propuesta al documento propietario antes de cambiar comportamiento.

### Persistencia y dependencias

- No se prevén cambios de persistencia productiva, esquema, consultas, migraciones, transacciones ni dependencias de paquetes.
- Reutilizar `WebApplicationFactory`, la fábrica SQLite ya configurada para tests, Dapper y `SearchTextNormalizer` donde el patrón de la suite ya los utiliza. No introducir nuevas herramientas de automatización de navegador.
- Prerrequisitos disponibles: proyectos Web/Application/Infrastructure, pruebas de integración de listado y fuentes aprobadas. El SDD no requiere el trabajo de revisión manual de S10.6 para cerrarse.
- La estrategia debe evitar contaminar el índice de búsqueda: las filas de fixture para `Link` y `LinkSearch` se insertan de forma coherente en la base temporal y se comprueba el resultado por consulta de aplicación.

### Garantías técnicas

- La identidad usada para listados privados procede de la identidad de desarrollo configurada en el host de prueba; variar `userId` en query string no autoriza cambiar de propietario ([AC-TRANS-01](../../../plans/mvp0-acceptance-criteria.md#ac-trans-01-identidad-propiedad-y-límites-de-acceso), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)).
- La búsqueda de enlaces y colecciones no mezcla entidades ni usuarios; los tests incluyen coincidencias deliberadas en ambas entidades y con otro usuario ([AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros)).
- Ranking, desempates y paginación se comparan contra los resultados observables definidos por la fuente funcional, no contra el orden incidental de almacenamiento ([«Listados»](../../../context/specifications.md#listados)).
- El comportamiento negativo de parámetros sigue exclusivamente las reglas del listado privado de enlaces y las especificaciones aprobadas S10.2 y S10.3; esta suite no añade soporte para etiquetas ni redefine errores o límites.
- Una respuesta HTTP exitosa sola no demuestra el contenido correcto; las aserciones comprueban elementos y orden. Que no aparezcan tarjetas solas tampoco demuestra que no haya existido una lectura errónea; las pruebas negativas incluyen datos de contraste para hacer observable el defecto.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Extender `LinkListingIntegrationTests` y reutilizar el host/base SQLite en memoria existentes. | propuesta | [spec.md → «Alcance»](spec.md#alcance), [LinkListingIntegrationTests.cs](../../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs) | Mantiene las comprobaciones cercanas a su ruta y evita infraestructura de tests paralela. Revisar en T01 si una prueba de entidad debe ubicarse en la suite de listados de colecciones existente. |
| Sembrar en la base aislada las proyecciones FTS5 necesarias para que la prueba mida el resultado del listado y no un índice incompleto. | confirmado | [architecture.md → «FTS5»](../../../context/architecture.md#fts5), patrón de fixtures en [LinkListingIntegrationTests.cs](../../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs) | Los datos semilla deben ser coherentes con las tablas base y la proyección; utilizar el helper existente o ajustarlo solo con causa concreta. |
| CA03 pide un caso fuera de rango, pero la regla vigente de rutas privadas prescribe error de parámetros para enlaces; no tratarlo como el comportamiento distinto del listado de colecciones. | confirmado | [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), decisión de alcance de DLG en conversación del 27-X-2026 («follow_current_specs»). | La prueba del listado de enlaces debe verificar su error fuente, sin extrapolar la excepción de `p` que aplica a colecciones. |
| Los tests actuales ofrecen cobertura parcial de límites, filtros no accesibles, parámetros inválidos, páginas fuera de rango, consulta combinada, conservación, dashboard y datos de usuarios distintos. | confirmado por inspección, no como evidencia de ejecución | [LinkListingIntegrationTests.cs](../../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs), [S10.1](../s10.1-listado-enlaces/spec.md), [S10.2](../s10.2-parametros-listado-enlaces/spec.md), [S10.3](../s10.3-conservar-parametros-listado-enlaces/spec.md), [S10.4](../s10.4-buscador-dashboard/spec.md) | CA01–CA06 requieren matriz de cobertura de las pruebas existentes frente a cada criterio; no atribuir como nuevas las comprobaciones ya implementadas ni reescribirlas sin necesidad. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración HTTP: búsqueda combinada con filtro `c`; comparar resultados dentro/fuera del filtro, otro usuario y término que coincide en más de una colección. | SQLite en memoria aislada, usuario/colecciones/enlaces preparados en `LinkListingIntegrationTests`. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkListingIntegrationTests --no-restore --nologo` | Solo enlaces del usuario y de la colección filtrada que satisfacen `q`; sin enlaces no coincidentes ni de usuario ajeno. | Tests/casos efectivamente ejecutados, conjunto de datos y resultados; fecha, SDK y salida real. |
| V02 | CA02 | Automatizada, integración HTTP: construir resultados con distintos grados de coincidencia y empates, insertados en orden deliberadamente distinto del ranking esperado. | Fixture de enlaces en SQLite en memoria y mecanismo de indexado consistente. | Mismo comando focal de V01. | Secuencia de resultados respeta relevancia y desempates definidos en la fuente, de manera reproducible. | Casos y ranking observado frente al esperado, salida real y fecha/SDK. |
| V03 | CA03 | Automatizada, integración HTTP de límites: cero/uno/última página y más de una página; página fuera de rango y frontera exacta. | Datos de test suficientes para cruzar la frontera de página. | Mismo comando focal de V01. | Cantidad y pertenencia de resultados por página, navegación y respuesta fuera de rango coinciden con [«Listados»](../../../context/specifications.md#listados) y [«Rutas privadas»](../../../context/specifications.md#rutas-privadas). | Valores usados, resultados observados y test; fecha/SDK. |
| V04 | CA04 | Automatizada, integración HTTP entre listado de enlaces y listado de colecciones: mismo término coincide con ambas entidades. | Fixtures de enlaces/colecciones para identidad de test; reutilizar test host de colecciones existente. | Comando dirigido a los tests de integración de enlaces y colecciones; seleccionar los nombres concretos tras localizar el patrón de suite en T01. | Cada endpoint devuelve solo su entidad, sin mezclar la coincidencia de la otra lista. | Nombres exactos, proyectos/filtros ejecutados y contenido observado por endpoint. |
| V05 | CA05 | Automatizada, integración HTTP y comprobación sensible negativa: usuarios con datos coincidentes, colecciones/enlaces propios y ajenos, y parámetro `userId` manipulado. | Fixture aislado, identity provider del host de test y coincidencias cruzadas. | Mismo comando focal de V01; incluir los tests existentes que cubran filtro ajeno y query de identidad. | Ningún dato ajeno se devuelve; el test detecta la regresión si el filtro por `UserId` o la comprobación de propiedad se elimina o se usa identidad del cliente. Para comprobar sensibilidad, introducir temporalmente la mutación solo en el worktree de test, confirmar que la aserción falla y restaurar el archivo sin conservar la mutación. | Test y mutación temporal aplicada/revertida, aserción que falla, restauración confirmada y reejecución correcta; fecha/SDK. No manipular datos reales ni un checkout compartido. |
| V06 | CA06 | Automatizada, integración HTTP: valores inválidos/fuera de rango, filtros inexistentes/ajenos, repetidos, desconocidos/no aplicables, `q` vacío o excedido, según la fuente de enlaces y los casos implementados en S10.2/S10.3. | Tests y analizador del listado existentes, fixture con datos discriminantes. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests|FullyQualifiedName~PrivateListingQueryAnalyzerTests" --no-restore --nologo` | Resultados y mensajes de la respuesta se ajustan a cada regla fuente; se selecciona solo el primer valor repetido, y `e`/desconocidos no se validan como filtros disponibles. | Casos ejecutados, URL consultada, status/contenido/elementos y salida real con fecha/SDK. |
| V07 | CA01–CA06 | Validación integrada: ejecutar tests focalizados de páginas de enlaces y colecciones, revisar diff, cobertura y enlaces de documentación. | V01–V06 completadas o bloqueos explícitos documentados. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests|FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~CollectionListingIntegrationTests|FullyQualifiedName~PrivateListingQueryAnalyzerTests" --no-restore --nologo`; `git diff --check` | Tests focalizados pasan; cada criterio tiene evidencia ligada, sin atribuir cobertura a S10.6 ni a la suite completa. Diff sin errores de whitespace. | Salida de ambos comandos, casos ejecutados, SDK/fecha, cambios y límites de la cobertura. |

La comprobación negativa de V05 debe realizarse de manera controlada en un worktree aislado. Si no se puede hacer sin riesgo de interferir con cambios del usuario o de otros procesos, se deja explícitamente pendiente y no se afirma como pasada; no se elimina permanentemente un control de seguridad. Los tests HTTP no sustituyen la revisión manual con datos de ejemplo de S10.6.

## Orden de ejecución y cierre

- El artefacto de tareas se creará solo después de la aprobación humana de este plan; hasta entonces no se generan tareas ni se inicia implementación.
- Cerrar esta spec cuando CA01–CA06 tengan evidencia automatizada/técnica satisfactoria dentro de este trabajo, sus tareas estén completas y exista aceptación humana registrada, sin esperar S10.6 ni el cierre de la etapa S10.
- Si la inspección de cobertura revela que un test ya acredita un criterio, registrarlo como cobertura existente y ejecutarlo en la validación de este trabajo; no atribuirle ejecución previa como evidencia nueva.
- Ante una regla ambigua o una discrepancia nueva, detener la validación afectada y tramitarla en el documento propietario antes de cambiar el comportamiento.
