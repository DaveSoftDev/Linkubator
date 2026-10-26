# Plan técnico: S9.1 — Crear enlace

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»).
  - Plan: DLG, 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Añadir la página Razor canónica de creación bajo `Pages/App/Links`, con su `PageModel` e input model propios, y registrar la ruta en `AppRoutes`. La página privada mantiene la identidad y el layout existentes.
- Inyectar `ILinkService` para ejecutar `CreateLink` con un `LinkCreateRequest`; el propietario lo obtiene Application de la identidad de servidor. El formulario no recibe ni envía un `UserId`.
- Obtener las opciones del selector mediante `ICollectionService.ListAllCollections`, usando los identificadores y nombres de las colecciones del usuario para construir las opciones. La lista se presenta completa y con el orden de su fuente propietaria; el identificador del formulario no se toma del slug usado por la navegación del sidebar.
- `PrivatePageModel` comparte mediante `CollectionsResult` el resultado que ya carga para el sidebar. La página lo reutiliza para el selector, sin una consulta duplicada. Si la consulta falla, presenta una alerta, responde con error de disponibilidad y no ofrece controles de envío.
- Reutilizar los parciales y modelos de presentación existentes para errores, avisos y alertas. Traducir los resultados de `CreateLink` a errores asociados a campos o a avisos generales conforme a sus categorías, sin copiar mensajes ni exponer información de recursos ajenos. El resultado de duplicado ya incluye el nombre de la colección existente.
- `LinkCreateValidator` reutiliza las políticas de Domain para atribuir sus errores al campo de entrada antes de ejecutar la transacción. No se duplican algoritmos ni límites; los contratos de Application y las comprobaciones del agregado permanecen sin cambios.
- Reutilizar la estructura y las clases CSS de los formularios de colecciones. El selector amplía esas mismas reglas compartidas; no se introduce un diseño alternativo.
- Representar cada opción desde su `CollectionDto` con un atributo `data-public`. El estado inicial del checkbox se calcula desde las colecciones propias; el script de la página sincroniza el control y su ayuda en carga, cambios de selección y restauración de la página. Al volver de un POST rechazado, el modelo de presentación elimina la marca si el destino no permite publicación, sin alterar la validación del request enviado.
- Reutilizar la protección antiforgery de Razor Pages y el flujo de POST. Los campos del formulario se codifican contextualmente con Razor.
- `NewModel` reutiliza el binding de `LinkManagementPageModel`. El helper `LinkReturnRoute` concentra la validación local de destinos para el sidebar y los formularios conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas); solo la creación admite el listado de colecciones. El sidebar valida la ruta y query actuales antes de transmitirlas y usa el destino predeterminado cuando no son admisibles, sin trasladar identificadores de páginas de operaciones. El formulario conserva el destino validado en un campo oculto y en Cancelar, y el POST usa `LocalRedirect`.
- Tras crear, se guarda en TempData la URL devuelta por Application. El parcial compartido `_LinkCreationNotice` la consume al volver al dashboard o a los listados de enlaces o colecciones y representa la confirmación y su enlace externo con codificación Razor y `noopener noreferrer`. También consume el aviso de privacidad de la creación.

### Flujos internos

1. En GET, la página privada carga las colecciones propias, construye el selector sin paginar y presenta el formulario con el estado público inicial desmarcado. El GET no invoca operaciones de escritura.
   - El enlace «Avanzado...» del dashboard sincroniza su destino con el campo de URL mediante `URL.searchParams`. El handler GET recibe el parámetro `url` y lo asigna únicamente a `Input.Url`; Razor codifica su valor al representarlo. La validación y la creación continúan en el POST existente.
   - La creación rápida usa un formulario POST con antiforgery y el handler `QuickAdd` del dashboard. Reutiliza `CollectionsResult` y `ILinkService.CreateLink` conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), sin confiar en un destino ni en metadatos enviados por el cliente. Tras un error, recarga las tarjetas y mantiene el campo enviado; tras el éxito redirige al dashboard.
2. En POST, Web construye el request con los campos del formulario y delega en `ILinkService.CreateLink`. No valida ni reproduce en Web las reglas de ajuste de URL, imagen, duplicados, propiedad o privacidad que corresponden a Application y Domain.
3. Si Application crea el enlace, Web redirige al origen validado o al destino predeterminado conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas).
4. Si Application devuelve validación o regla de dominio, Web vuelve a presentar el formulario con los valores y los errores asociados a los campos correspondientes. El error de URL duplicada conserva la información de la colección existente que suministra Application.
5. Un identificador de colección inválido, inexistente o ajeno no crea datos. Web presenta el error en el formulario sin revelar la existencia o propiedad de la colección. El control de publicación refleja el destino seleccionado conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas); Application sigue rechazando las peticiones manipuladas.

### Persistencia y dependencias

- No se añade SQL, migración ni acceso directo a repositorios desde Web. La creación y sus comprobaciones se delegan en el caso de uso existente de Application y en su transacción.
- La escritura y la actualización de las proyecciones de búsqueda permanecen bajo la responsabilidad de la ruta existente Application → Infrastructure, conforme a [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [«FTS5»](../../context/architecture.md#fts5).
- Prerrequisitos disponibles: identidad y páginas privadas de S7; contratos y caso de uso de enlaces de S4.7; persistencia existente; listado de colecciones de S8.1. No se espera a S9.2, S9.5, S9.6, S9.7 ni S10 para cerrar esta entrega.

### Garantías técnicas

- Mantener el flujo Web → Application → Domain y no inyectar repositorios en la página.
- Construir todas las opciones desde la colección obtenida bajo el usuario identificado; no aceptar identidad desde formulario, query o ruta.
- Asegurar que una colección inexistente o ajena produce un resultado sin creación ni filtración de datos.
- Mantener las escrituras exclusivamente en POST con antiforgery; el GET no modifica datos.
- No introducir una segunda implementación de validación de URL, imagen o duplicados, ni alterar contratos existentes.
- Si falla la carga de opciones, comunicar el fallo mediante el tratamiento Web existente; no ocultarlo como una lista legítimamente vacía.
- S9.1 usa la página canónica conforme a la aprobación de DLG. «Rutas privadas» define los formularios como páginas canónicas y permite diálogos como presentación alternativa; la etapa de uso de diálogos se mantiene en [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance) y [roadmap.md → «MVP1 confirmado»](../../context/roadmap.md#mvp1-confirmado).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Delegar la creación en `ILinkService.CreateLink` y las opciones en `ICollectionService.ListAllCollections`, sin persistencia desde Web. | confirmado | Contratos existentes; [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura). | Conserva las reglas y garantías existentes en las capas propietarias. |
| Compartir con la página el resultado de carga de colecciones que `PrivatePageModel` ya realiza para el sidebar, evitando una segunda consulta. | confirmado | Implementación verificada en T03 y T07; [specifications.md → «Listados»](../../context/specifications.md#listados). | Se expone el resultado protegido, conservando el tratamiento del sidebar y el comportamiento de las páginas existentes. |
| Atribuir en el validador de creación los errores de las políticas de Domain al campo correspondiente. | confirmado | Compatibilidad técnica comprobada en T05–T07. | Los errores de Domain no identificaban el campo; Application conserva código y categoría y añade su asociación, sin trasladar reglas a Web. |
| Traducir el resultado de colección inexistente o ajena a un error de formulario sin filtrar propiedad. | confirmado | [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos). | La categoría de Application no debe convertirse en una respuesta que revele la causa de inaccesibilidad. |
| Mantener la página canónica para S9.1 y tratar el diálogo como presentación alternativa, sin cambiar ruta, método ni comportamiento. | confirmado | DLG, confirmación de alcance previa a la aprobación «Siguiente» del 26-X-2026; [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance), [roadmap.md → «MVP1 confirmado»](../../context/roadmap.md#mvp1-confirmado) y [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas). | La especificación funcional ya no exige diálogos en la ruta canónica. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; GET de creación con base SQLite de prueba, datos de varias colecciones propias y otra identidad. Comprueba opciones completas, ausencia de colecciones ajenas, estado inicial y estructura accesible de controles. | Página, factory Web de prueba y migraciones existentes. | Añadir casos a una clase de integración dedicada siguiendo el patrón de `CollectionsListingIntegrationTests`; ejecutar `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkCreationIntegrationTests --nologo`. | La respuesta contiene todas las opciones propias sin paginar, no contiene opciones ajenas, el estado público está desmarcado y los controles tienen labels asociados. | Resultado del test focalizado, contenido HTML comprobado y conjunto de datos de prueba; anotar qué no demuestra una revisión manual de navegador. |
| V02 | CA02 | Automatizada; POST válido con antiforgery y comprobación de redirección y estado persistido. Incluye creación privada por defecto y solicitud pública válida en una colección pública. | V01 y acceso a la base temporal de integración. | El mismo filtro `LinkCreationIntegrationTests`; comprobar el enlace persistido por el usuario y la colección esperados. | Se crea exactamente un enlace con los datos enviados y la visibilidad solicitada permitida; la respuesta redirige al listado y las proyecciones de búsqueda permanecen consistentes. | Resultado de los escenarios positivos, respuesta HTTP y lectura de los datos persistidos. |
| V03 | CA03 | Automatizada; POST con URL o imagen inválida y con URL duplicada. Comprobar estado persistido y errores renderizados. | V01 y datos de enlace preexistentes para el caso duplicado. | El mismo filtro `LinkCreationIntegrationTests`; complementar con `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~LinkServiceTests --nologo`. | Los errores no crean registros; los valores del formulario se conservan, los errores se asocian a los campos y el duplicado identifica la colección existente. | Resultados de tests Web y Application; indicar que los tests de Application no prueban la presentación HTTP. |
| V04 | CA04 | Automatizada y comprobación negativa de seguridad; enviar identificadores de colección inválidos y de otra identidad. Verificar que no hay alta ni filtración en la respuesta. | Fixture con al menos dos identidades y sus colecciones. | Escenarios negativos en `LinkCreationIntegrationTests`; comprobación de sensibilidad controlada que confirme que la aserción falla si el aislamiento se elude, seguida de restauración y repetición satisfactoria. | Todos los casos son rechazados sin crear enlace ni revelar los datos de la colección ajena; el test detecta una infracción de propiedad. | Resultado de casos y comprobación negativa/restauración, con fecha y referencia del test ejecutado. |
| V05 | CA05 | Automatizada y revisión técnica de handlers, HTML y protección antiforgery. | Implementación Razor Pages y suite de integración. | El mismo filtro `LinkCreationIntegrationTests`; `dotnet build Linkubator.sln --nologo`. | GET no escribe; POST sin token es rechazado y POST con token válido puede completar el caso válido; campos y errores conservan asociaciones accesibles; build sin errores. | Resultado de tests/build y revisión del markup; no atribuir a la revisión de HTML una comprobación visual completa. |

Los tests de S9.5 pueden ampliar posteriormente la cobertura HTTP del sprint, pero no sustituyen ni bloquean la evidencia focalizada de S9.1. La aceptación de los tests tampoco sustituye la aceptación humana de la especificación ni del resultado implementado.

## Orden de ejecución y cierre

- Tras la aprobación explícita de este plan, crear `tasks.md` con checks inmediatos y dependencias de cada tarea.
- Ejecutar las pruebas de Application y las pruebas Web focalizadas dentro de S9.1; registrar cobertura, limitaciones y cualquier comprobación negativa real.
- Resolver en la implementación cualquier incompatibilidad técnica de los contratos existentes sin alterar reglas funcionales; una contradicción de reglas se detiene y se tramita en su documento propietario.
- No iniciar cambios productivos hasta que `tasks.md` esté aprobado.
- Cerrar S9.1 solo con evidencia satisfactoria de CA01–CA05 y aceptación humana registrada en las tareas; no declarar cerradas las tareas vecinas ni S9 completa.
