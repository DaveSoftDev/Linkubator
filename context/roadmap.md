# Roadmap del MVP0 de Linkubator

Este documento indica en qué orden se construye el MVP0 y cuándo se da por terminada cada etapa. No repite reglas: cada objetivo remite a los documentos donde se especifica.

## Hecho

### Documentación del MVP0 (aceptada el 2026-10-06)

Entregables aceptados:

- [requirements.md](requirements.md), [domain-model.md](domain-model.md), [specifications.md](specifications.md), [architecture.md](architecture.md), [decisions.md](decisions.md) y este roadmap.

La aceptación cierra la fase de definición y permite iniciar la implementación según [AGENTS.md → «Implementación autorizada»](../AGENTS.md#implementación-autorizada).

### Fundaciones

La Definition of Done de S0 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S0: Fundaciones (M)»](../plans/mvp0-plan.md#s0-fundaciones-m).

### Dominio I

La Definition of Done de S1 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S1: Dominio I (L)»](../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l).

### Dominio II

La Definition of Done de S2 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S2: Dominio II (L)»](../plans/mvp0-plan.md#s2-dominio-ii-urls-l).

### Dominio III

La Definition of Done de S3 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S3: Dominio III (M)»](../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m).

### Dominio - Refactor

La Definition of Done de S3a está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S3a: Dominio IV (M)»](../plans/mvp0-plan.md#s3a-dominio-iv-refactor-m).

### Aplicación

La Definition of Done de S4 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l).

### Aplicación - Refactor

La Definition of Done de S4a está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S4a: Aplicación: refactor (M)»](../plans/mvp0-plan.md#s4a-aplicación-refactor-m).

### Persistencia I

La Definition of Done de S5 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l).

### Persistencia II

La Definition of Done de S6 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S6: Persistencia II: FTS5 (L)»](../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l).

### Persistencia - Refactor

La Definition of Done de S6a está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S6a: Persistencia: refactor (M)»](../plans/mvp0-plan.md#s6a-persistencia-refactor-m).

## Now

Esta es la etapa activa del roadmap: S7, Web base, seguridad y accesibilidad. El trabajo de aplicación y persistencia ya está completado y consta en [Hecho](#hecho).

### Web base, seguridad y accesibilidad

Objetivos:

- Completar el dashboard, las rutas privadas, los parámetros comunes y los errores accesibles que forman parte de S7, según [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas) y [requirements.md → «Accesibilidad»](requirements.md#accesibilidad).

- Completar la base Web privada y su seguridad según [architecture.md → «Seguridad web»](architecture.md#seguridad-web).
- [S7.1 — Seguridad web básica](../specs/s7.1-seguridad-web/spec.md), su [plan técnico](../specs/s7.1-seguridad-web/plan.md) y sus [tareas](../specs/s7.1-seguridad-web/tasks.md) están completados y aceptados por DLG el 22-X-2026 («Seguridad ready»): redirección HTTP a HTTPS, respuesta HTTPS válida y rechazo de un `Host` ajeno. CA03 no era aplicable porque aún no había operaciones de modificación mediante formulario; la suite amplia S7.9 es trabajo independiente.
- [S7.2 — Cabeceras de seguridad web](../specs/s7.2-cabeceras-seguridad/spec.md), su [plan técnico](../specs/s7.2-cabeceras-seguridad/plan.md) y sus [tareas](../specs/s7.2-cabeceras-seguridad/tasks.md) están completados y aceptados por DLG el 22-X-2026 («Completado»): tests focalizados 5/5, comprobaciones negativas, revisión integrada y `git diff --check` satisfactorios.
- [S7.3 — Identidad de desarrollo en `/app`](../specs/s7.3-identidad-desarrollo-app/spec.md), su [plan técnico](../specs/s7.3-identidad-desarrollo-app/plan.md) y sus [tareas → «Aceptación y cierre»](../specs/s7.3-identidad-desarrollo-app/tasks.md#aceptación-y-cierre) están completados y aceptados por DLG el 22-X-2026 («Fase Completada»). La implementación y validación técnica se completaron ese día: tests focalizados `WebStartupTests` 7/7, incluido el caso HTTP que confirma el `UserId` del proveedor frente a `?userId=99` y la prueba de ruta pública sin resolución de identidad. La prueba negativa de sensibilidad falló al esperar el ID manipulado 99 (observado 42); el valor original se restauró y la suite pasó. `git diff --check` quedó limpio. La ejecución de tests no reconstruyó referencias porque había procesos Web activos que bloqueaban ensamblados; se observó una advertencia HTTPS no bloqueante de `WebApplicationFactory`. Los detalles y límites de validación están registrados en las tareas. Concreta el paso de la identidad de desarrollo hacia Application sin volver a implementar S5.7.
- [S7.4 — Layout privado accesible](../specs/s7.4-layout-privado/spec.md), su [plan técnico](../specs/s7.4-layout-privado/plan.md) y sus [tareas](../specs/s7.4-layout-privado/tasks.md#aceptación-y-cierre) están completados y aceptados por DLG el 23-X-2026 («Dashboard ready»). La build Web de cierre fue satisfactoria (0 warnings, 0 errors). Esto cierra S7.4, no la etapa S7 completa ni sus tareas vecinas.
- [S7.5 — Componentes de presentación accesible](../specs/s7.5-componentes-presentacion/spec.md), su [plan técnico](../specs/s7.5-componentes-presentacion/plan.md) y sus [tareas → «Aceptación y cierre»](../specs/s7.5-componentes-presentacion/tasks.md#aceptación-y-cierre): completados y aceptados por DLG el 23-X-2026 («Componentes realizados») como base técnica de componentes Razor reutilizables. La prueba focalizada de renderizado con `--no-build` pasó y `git diff --check` no reportó errores; el intento de build normal fue bloqueado porque procesos Web activos mantenían ensamblados abiertos. Los límites de validación constan en las tareas. No se declara integración funcional con listados, búsqueda o paginación reales; esos flujos corresponden a sus etapas propietarias.
- [S7.6 — Análisis de parámetros de listados](../specs/s7.6-analisis-parametros-listados/spec.md), su [plan técnico](../specs/s7.6-analisis-parametros-listados/plan.md) y sus [tareas](../specs/s7.6-analisis-parametros-listados/tasks.md#aceptación-y-cierre): base técnica implementada y aceptada por DLG el 23-X-2026 («parámetros validados»). Incluye el analizador Web de `p`, `c` y `q` con pruebas unitarias; no integra aún rutas consumidoras ni verifica la propiedad de `c` mediante un contrato de Application. El parámetro `e` sigue fuera de alcance. El cierre funcional de S7.6 permanece pendiente de esos puntos de integración.
- [S7.7 — Dashboard y respuestas privadas](../specs/s7.7-dashboard-y-respuestas-privadas/spec.md), con [plan técnico aprobado](../specs/s7.7-dashboard-y-respuestas-privadas/plan.md) y [tareas aprobadas](../specs/s7.7-dashboard-y-respuestas-privadas/tasks.md): completado y aceptado por DLG el 24-X-2026 («Prueba superada»). Cubre `/app`, el dashboard del alcance disponible, el `404` genérico por método no permitido en rutas privadas y el aviso común de recurso no accesible sin filtrar información. El alcance excluye la gestión de etiquetas, reservada a MVP1.
- [S7.8 — Registro de peticiones Web](../specs/s7.8-registro-peticiones-web/spec.md), con [plan técnico aprobado](../specs/s7.8-registro-peticiones-web/plan.md) y [tareas aprobadas](../specs/s7.8-registro-peticiones-web/tasks.md): implementación en curso desde el 24-X-2026. La prueba focalizada del helper pasó (1/1), pero la captura de eventos reales del middleware aún no está demostrada; aceptación humana registrada como «Completado», cierre técnico pendiente. S7 continúa activa.
- [S7.9 — Pruebas de integración HTTP](../specs/s7.9-pruebas-integracion-http/spec.md), con [plan técnico aprobado](../specs/s7.9-pruebas-integracion-http/plan.md) y [tareas, evidencia y cierre](../specs/s7.9-pruebas-integracion-http/tasks.md#aceptación-y-cierre); completada y aceptada por DLG el 24-X-2026 («Completado»). Build Tests 0 warnings/0 errors, suite HTTP combinada 17/17, comprobación negativa de redirección HTTPS y `git diff --check` satisfactorios. Este cierre no cierra la etapa S7, que sigue activa.
- [S7.10 — Revisión manual del dashboard](../specs/s7.10-revision-manual-dashboard/spec.md), con [plan técnico aprobado](../specs/s7.10-revision-manual-dashboard/plan.md) y [tareas, evidencia y aceptación](../specs/s7.10-revision-manual-dashboard/tasks.md#aceptación-y-cierre); especificación aprobada por DLG el 24-X-2026 («Siguiente»), plan aprobado («Vamos») y revisión completada y aceptada por DLG el 24-X-2026 («Dashboard correcto»). Se revisaron teclado y foco, contraste y encabezados en el dashboard; no se registraron detalles instrumentales del navegador ni mediciones. El alcance no incluyó cambios productivos.
- [S7.11 — Validación del estilo base](../specs/s7.11-validacion-estilo-base/spec.md), con [plan aprobado](../specs/s7.11-validacion-estilo-base/plan.md) y [tareas, evidencia y cierre](../specs/s7.11-validacion-estilo-base/tasks.md#aceptación-y-cierre); especificación, plan y tareas aprobados por DLG («Siguiente», «Siguiente» y «Vamos»). Revisión aceptada y completada por DLG el 24-X-2026 («Pues por mi lo daría por finalizado a falta de pulir flecos de diseño»). En la revalidación, los formularios se apilan dentro del ancho estrecho emulado y los contrastes representativos superan 4.5:1; el solapamiento de 13 px entre el estado vacío del dashboard y el footer fijo se considera aceptable («Lo considero aceptable»). El viewport estrecho real no se comprobó por limitación de emulación. Los flecos de diseño quedan como seguimiento separado; esta aceptación no los declara implementados ni cierra la etapa S7.

Criterio de finalización:

- Las rutas privadas resuelven la identidad de desarrollo inyectada y no permiten elegir o alterar el `UserId` desde el cliente.
- El dashboard y la base de las páginas privadas respetan [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).
- La seguridad web y la accesibilidad de la base privada cumplen sus fuentes funcionales y técnicas enlazadas arriba.

## Next

Siguientes etapas de implementación, en el orden aprobado en [mvp0-plan.md → «Resumen de sprints»](../plans/mvp0-plan.md#2-resumen-de-sprints):

1. [S8: Gestión de colecciones (L)](../plans/mvp0-plan.md#s8-gestión-de-colecciones-l); [S8.1 — Listado de colecciones](../specs/s8.1-listado-colecciones/spec.md), su [plan técnico](../specs/s8.1-listado-colecciones/plan.md) y sus [tareas y cierre](../specs/s8.1-listado-colecciones/tasks.md#aceptación-y-cierre) están aprobados y completados; DLG aceptó el resultado el 25-X-2026 («Completado»). La implementación y validación técnica finalizaron el 25-X-2026: suite focalizada 112/112, build Web 0 warnings/0 errors y `git diff --check` sin errores. DLG revisó manualmente Tab/Shift+Tab y el foco visible en la búsqueda y el estado vacío con Chrome de escritorio; no se registraron versión/viewport ni comprobaciones manuales de paginación. La decisión aprobada sobre el resultado paginado de Application preservó `ListAllCollections`; los cambios preexistentes del sidebar se mantienen separados de la evidencia S8.1. La regla de `p` inválido/fuera de rango fue sincronizada en `specifications.md` con autorización de DLG. [S8.2 — Operaciones de colecciones](../specs/s8.2-operaciones-colecciones/spec.md), su [plan técnico](../specs/s8.2-operaciones-colecciones/plan.md) y sus [tareas](../specs/s8.2-operaciones-colecciones/tasks.md#aceptación-y-cierre) están aprobados; DLG aceptó la implementación el 25-X-2026 («Completado»). Los cambios están implementados y pasan build de solución (0 warnings/0 errors) y `CollectionServiceTests` (62/62); el cierre técnico sigue pendiente porque la última ejecución completa informó 1,193/1,195 tests, permanece sin resolver una regresión del test de paginación y no se ha completado la validación integrada. [S8.3 — Errores de formularios de colecciones](../specs/s8.3-errores-formularios-colecciones/spec.md), su [plan técnico](../specs/s8.3-errores-formularios-colecciones/plan.md) y sus [tareas](../specs/s8.3-errores-formularios-colecciones/tasks.md) están completados y aceptados por DLG el 25-X-2026 («Completado»). La implementación y validación técnica finalizó ese mismo día con build Web 0 warnings/0 errors y suite focalizada 12/12 en `CollectionsListingIntegrationTests`; la evidencia del cierre quedó sincronizada con la documentación del trabajo. [S8.4 — Identificadores y respuestas de colecciones](../specs/s8.4-identificadores-y-respuestas-colecciones/spec.md), su [plan técnico](../specs/s8.4-identificadores-y-respuestas-colecciones/plan.md) están aprobados por DLG el 25-X-2026 («Siguiente»); sus [tareas](../specs/s8.4-identificadores-y-respuestas-colecciones/tasks.md#aceptación-y-cierre) están aprobadas por DLG el 25-X-2026 («Vamos»). La implementación y validación se completaron el 25-X-2026: `CollectionsListingIntegrationTests` 12/12 y suite completa `Linkubator.Tests` 1,203/1,203, 0 fallidos, 0 omitidos; `git diff --check` satisfactorio. Se corrigieron además dos fallos globales: expectativa de logging obsoleta para el flujo unificado de actualización y respuesta no genérica ante método no permitido en ruta privada. La aceptación humana de S8.4 está pendiente; esto no cierra S8 ni sus tareas vecinas. [S8.5 — Pruebas de integración HTTP de colecciones](../specs/s8.5-pruebas-integracion-http-colecciones/spec.md) está aprobada por DLG el 25-X-2026 («Siguiente»); su [plan técnico](../specs/s8.5-pruebas-integracion-http-colecciones/plan.md) está aprobado por DLG el 25-X-2026 («Siguiente»); sus [tareas y evidencia](../specs/s8.5-pruebas-integracion-http-colecciones/tasks.md#aceptación-y-cierre) están aprobadas por DLG el 25-X-2026 («Vamos»); la implementación y validación técnica se completaron el 25-X-2026: `CollectionsListingIntegrationTests` 19/19, prueba negativa de sensibilidad con fallo esperado/restauración 1/1, `git diff --check` y enlaces satisfactorios. Resultado aceptado por DLG el 25-X-2026 («Completado»). La suite completa del proyecto no se ejecutó; la evidencia cubre el filtro focalizado. La [spec S8.6 — Revisión manual de colecciones](../specs/s8.6-revision-manual-colecciones/spec.md) está en borrador, pendiente de aprobación. S8.2 no cierra S8: las tareas restantes de S8.3–S8.7 conservan sus propios alcances y aceptación.2. [S9: Gestión de enlaces (L)](../plans/mvp0-plan.md#s9-gestión-de-enlaces-l).
3. [S10: Listado y búsqueda de enlaces (L)](../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l).
4. [S10a: Web: refactor (M)](../plans/mvp0-plan.md#s10a-web-refactor-m).

Estas etapas completan la gestión privada y su búsqueda según [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas), [«Búsqueda y filtros»](specifications.md#búsqueda-y-filtros) y [«Rutas privadas»](specifications.md#rutas-privadas). Deben conservar la propiedad por usuario, el aislamiento de cada listado y la accesibilidad definida en [requirements.md → «Accesibilidad»](requirements.md#accesibilidad).

## Later

Después de completar las etapas de Next:

1. [S11: Área pública y SEO (L)](../plans/mvp0-plan.md#s11-área-pública-y-seo-l): landing, página de usuario, página de colecciones y página de colección, según [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas). Incluye solo los fundamentos SEO comprometidos para MVP0; el SEO avanzado queda fuera de alcance ([decisions.md → «Fuera de alcance del MVP0»](decisions.md#fuera-de-alcance-del-mvp0)).
2. [S12: Cierre y hardening (M)](../plans/mvp0-plan.md#s12-cierre-y-hardening-m): completar la checklist de calidad del MVP0 y aceptar el cierre de la etapa.

Criterios para las páginas públicas:

- No quedan textos pendientes.
- Las colecciones y los enlaces privados nunca aparecen en las páginas públicas.
- El HTML público se renderiza en servidor y es legible por los buscadores.
- En la landing del MVP0, el enlace de registro tiene `href="#"` y no navega a un formulario ni envía una solicitud.

Las etapas confirmadas de MVP1 y el backlog no comprometido se detallan más adelante en este documento.

## Checklist de calidad del MVP0

Cada punto remite a la regla que se comprueba.

Dominio y datos:

- Tests de las invariantes de [domain-model.md → «Collection»](domain-model.md#collection), [domain-model.md → «Link»](domain-model.md#link), [domain-model.md → «Propiedad de los datos»](domain-model.md#propiedad-de-los-datos) y [domain-model.md → «Público y privado»](domain-model.md#público-y-privado) aplicables al MVP0. Las de cuentas, tokens, etiquetas y scraping se prueban en sus etapas de MVP1; el estado válido del usuario provisionado se comprueba en las pruebas de identidad de desarrollo.
- Tests de propiedad entre usuarios, incluida la igualdad de `UserId` entre enlace y colección ([domain-model.md → «Propiedad de los datos»](domain-model.md#propiedad-de-los-datos)).
- Tests de las reglas de público y privado y de sus transiciones ([domain-model.md → «Público y privado»](domain-model.md#público-y-privado)), incluida la opción pública deshabilitada al crear o editar un enlace en una colección privada y el rechazo, mediante integración HTTP, de peticiones manipuladas de creación, edición y cambio de privacidad.
- Tests de que siempre queda al menos una colección.
- Tests de la identidad de desarrollo ([decisions.md → «Identidad de desarrollo»](decisions.md#identidad-de-desarrollo)): cada arranque aplica migraciones y aprovisiona o completa el usuario configurado antes de resolverlo; el aprovisionamiento es idempotente, la identidad no puede alterarse mediante datos de la petición y todas las operaciones privadas se limitan al `UserId` inyectado. Hasta LoginV0, esta identidad se usa en todos los entornos y se acepta el riesgo de datos compartidos.
- Tests de transacciones y de condiciones de carrera con `BEGIN IMMEDIATE` ([architecture.md → «Persistencia»](architecture.md#persistencia)): hacer privada una colección mientras se hace público uno de sus enlaces y mover un enlace a una colección ajena con un `CollectionId` manipulado (rechazado por la aplicación y por la clave foránea compuesta).
- Tests de atomicidad de las escrituras de enlaces y colecciones: si falla una operación compuesta, no quedan cambios parciales ni proyecciones FTS5 desincronizadas.

Especificaciones:

- Tests de recorte de textos y de longitudes máximas de los campos del MVP0 según [specifications.md → «Textos introducidos por el usuario»](specifications.md#textos-introducidos-por-el-usuario) y [specifications.md → «Longitudes máximas»](specifications.md#longitudes-máximas): rechazo de lo introducido por el usuario; un carácter fuera del BMP cuenta como un punto de código y una letra seguida de una marca combinante cuenta según los puntos de código que tenga tras las transformaciones aplicables.
- Tests de la transformación común a ASCII, de la generación del alias (acentos, espacios pegados, guiones duplicados, caracteres eliminados, rechazo de controles y de letras y números no ASCII, rangos, palabras reservadas y alias ocupado) y del slug de colección (todos los ejemplos de la tabla de conversión, rangos, colisiones y regeneración al renombrar).
- Tests de ajuste y validación por separado para `UrlOriginal` e `Image`, con todos los ejemplos de [specifications.md → «Ajuste de URL»](specifications.md#ajuste-de-url), [«Ajuste de `Image`»](specifications.md#ajuste-de-image) y [«Validación»](specifications.md#validación): `UrlOriginal` admite los esquemas y hosts indicados en esas reglas, incluidos `http`, `localhost` e IP; `Image` se ajusta a `https` y rechaza `localhost` e IP. En ambos casos, probar la conversión de `//` a `https:`.
- Tests de `UrlNormalized` con los ejemplos de [specifications.md → «Normalización para duplicados»](specifications.md#normalización-para-duplicados), con caracteres estructurales codificados (`%23`, `%26`), texto codificado dos veces (`%2520`), `+` en la ruta y en la consulta.
- Tests de búsqueda MVP0 ([specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros)): usuario identificado, sin distinguir acentos, equivalencia entre formas Unicode NFC precompuestas y descompuestas tanto en `q` como en los campos indexados, entrada separada en secuencias de letras o números con el resto de caracteres como separadores (comillas, `*`, guiones, `OR`, `NEAR`) sin errores de sintaxis ni operadores, todos los términos obligatorios, palabras completas sin prefijo, entrada sin términos equivalente a no buscar, filtro por colección combinado con `q` y resultado más relevante primero.
- Tests de búsqueda de colecciones MVP0 ([specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros) y [specifications.md → «Listados»](specifications.md#listados)): las mismas reglas de `q` que en enlaces, incluida la equivalencia Unicode NFC entre formas precompuestas y descompuestas en `q` y `Collection.Name`, coincidencia solo por `Name`, resultado solo de colecciones del usuario identificado, sin enlaces en el resultado, orden alfabético con empates por `Id` (sin relevancia), paginación sobre el resultado filtrado y sincronización de la proyección de colecciones al crear, renombrar y eliminar colecciones, incluidas las filas de sus enlaces en la proyección de enlaces al renombrar.
- Tests de aislamiento por entidad y por usuario: un texto que coincide con un enlace y con una colección aparece solo en el listado de cada uno, entidades de usuarios distintos con el mismo `Id` no se mezclan ni se confunden, y los datos de otro usuario nunca aparecen en ninguno de los listados.
- Tests de los parámetros de los listados ([specifications.md → «Rutas privadas»](specifications.md#rutas-privadas)): parámetros aceptados por cada listado; desconocidos, no aplicables (por ejemplo, `c` en el listado de colecciones) o de una capacidad no disponible (`e` en el MVP0) ignorados sin validarlos ni consultarlos; `p`, `c` y `q` vacíos tratados según sus reglas, conservando los demás filtros válidos; filtro de colección inexistente o ajena ignorado; valores inválidos o fuera de rango con el mensaje previsto; `q` por encima de su máximo con mensaje de error; `q` vacío o solo con espacios equivalente a no buscar; formularios de búsqueda que conservan los filtros del listado y descartan `p`; y enlaces de paginación que conservan `q` y los filtros aplicables.
- Tests de las rutas privadas ([specifications.md → «Páginas privadas»](specifications.md#páginas-privadas)): redirección de `/app` al dashboard y dashboard sin resultados, edición ni eliminación; los identificadores inválidos producen validación, mientras que un identificador válido inexistente o ajeno produce el mismo resultado de recurso no accesible, sin exponer datos ni modificarlos, con enlace al listado en GET y redirección con aviso en POST, incluidos los mensajes específicos de eliminación de colección y enlace; errores de validación para colecciones inválidas, inexistentes o ajenas al crear, editar o mover enlaces; `404` para métodos no permitidos; `?p=1` con el mismo contenido que la primera página sin parámetro, listados vacíos en la primera página y mensaje de error fuera de rango; selector y filtro con más colecciones que el tamaño de página; estado destino de visibilidad válido, ausente, repetido o inválido y redirección con aviso si se rechaza; redirección tras cada POST correcto; confirmación de borrado; y ausencia del texto de `q` en los logs. Estas pruebas se realizan mediante integración HTTP; la presentación visual y accesible de los mensajes se revisa manualmente.
- Tests de listados ([specifications.md → «Listados»](specifications.md#listados)): orden alfabético de las colecciones privadas y públicas y del texto mostrado de los enlaces en la página pública de colección, con mayúsculas, acentos, `ñ` y empates resueltos por `Id`; tamaño de página y, en la página de colección, el parámetro `p`, la canonical de `?p=1` hacia la URL sin parámetro y la canonical propia de las páginas interiores.
- Tests de páginas públicas según [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas): contenido y orden de los últimos enlaces públicos de todas las colecciones que se muestran en público, seguidos por las últimas colecciones que se muestran en público y el enlace a su listado completo; página de usuario existente con las secciones vacías y sus mensajes cuando no tiene enlaces públicos o colecciones que se muestren en público; `404` de alias y slugs antiguos, y de colección sin enlaces públicos; ausencia de colecciones públicas vacías, enlaces privados y colecciones privadas; `rel` y `target` de los enlaces externos; canonical, barra final y redirección de las variantes de URL previstas; paginación según [specifications.md → «Paginación de las páginas públicas»](specifications.md#paginación-de-las-páginas-públicas), incluida la primera página sin error para valores vacíos, inválidos o fuera de rango, el uso del primer parámetro repetido y la canonical normalizada sin ceros iniciales ni parámetros desconocidos.
- Revisión humana del HTML público renderizado: títulos, descripciones y canonical de cada página; texto alternativo de imágenes y codificación segura de sus valores; encabezados semánticos, contenido y enlaces visibles. No se automatiza la interacción de la UI ni se usan pruebas de navegador.
- Tests de los encabezados y del idioma de las páginas públicas: un único `h1` por página ([specifications.md → «Páginas públicas»](specifications.md#páginas-públicas), [specifications.md → «Página de usuario»](specifications.md#página-de-usuario), [specifications.md → «Página de colecciones»](specifications.md#página-de-colecciones) y [specifications.md → «Página de colección»](specifications.md#página-de-colección)), e idioma declarado en castellano ([requirements.md → «Accesibilidad»](requirements.md#accesibilidad)).
- Test de la landing del MVP0: su botón de login lleva a la entrada de la zona privada ([specifications.md → «Landing»](specifications.md#landing) y [specifications.md → «Rutas privadas»](specifications.md#rutas-privadas)).
- Tests de errores: formularios con errores de validación y mensajes previstos para recursos inexistentes o de otro usuario en `/app`, comprobados mediante integración HTTP. La presentación visual y accesible de esos mensajes se revisa manualmente.
- Test del comando de reconstrucción de las proyecciones FTS5 de enlaces y de colecciones.

Seguridad web:

- Tests de que la zona privada usa la identidad de desarrollo y muestra los mensajes previstos para recursos inexistentes y ajenos, comprobados mediante integración HTTP. La presentación visual y accesible de esos mensajes se revisa manualmente.
- Tests de redirección a HTTPS, cabeceras de seguridad, antiforgery y rechazo de un `Host` ajeno.

Accesibilidad y operación:

- Revisión humana de accesibilidad según [requirements.md → «Accesibilidad»](requirements.md#accesibilidad): navegación completa con teclado, orden y visibilidad del foco, foco en formularios y errores, labels, anuncios para tecnologías de asistencia, landmarks y encabezados, salto al contenido, contraste WCAG AA, estados vacíos, nombres accesibles y respeto de `prefers-reduced-motion`.
- Pruebas manuales en navegadores actuales, incluidas las páginas públicas y privadas, formularios, listados, mensajes de error y estados vacíos. No se requieren pruebas automatizadas de UI ni herramientas de automatización de navegador.
- Logs disponibles por consola y en archivo rotativo ([architecture.md → «Logs»](architecture.md#logs)).
- Tests de registro de eventos del MVP0 y de que los logs no contienen los datos que prohíbe [specifications.md → «Registro de eventos»](specifications.md#registro-de-eventos).

## MVP1 confirmado

### Gestión privada mediante diálogos

Objetivos:

- Presentar mediante diálogos los formularios canónicos de creación, edición y eliminación de enlaces y colecciones, y el de mover enlaces, sin cambiar sus rutas, métodos ni comportamiento ([specifications.md → «Páginas privadas»](specifications.md#páginas-privadas)).

Criterio de finalización:

- Los formularios de creación, edición, eliminación y movimiento de enlaces y colecciones se presentan en diálogos.
- Los errores de validación y de reglas del dominio se muestran en el diálogo correspondiente, con los errores de validación asociados a sus campos.
- Una revisión humana en navegadores actuales confirma que los diálogos permiten completar y cancelar estas operaciones con teclado, conservan el comportamiento de los formularios canónicos y presentan los errores de forma accesible. No se automatizan pruebas de UI ni se usan herramientas de automatización de navegador.

### Implementar el LoginV0

Esta etapa comprueba el inicio de sesión y la autorización de una página protegida usando el usuario de desarrollo. No implementa el registro ni el envío de correo.

Objetivos:

- Provisionar mediante el SQL local especial un usuario de desarrollo completado, con `EmailConfirmedAt` informado y su colección privada «Bandeja de entrada» ([decisions.md → «Identidad de desarrollo»](decisions.md#identidad-de-desarrollo)). Este estado permite probar el login, pero no demuestra que una persona haya confirmado la titularidad del email.
- Antes de habilitar el login, ejecutar explícitamente la preparación local de la base de datos: obtener la contraseña de desarrollo desde configuración local no versionada, calcular su hash Argon2id mediante `IPasswordHasher` y actualizar `User.Password` mediante el script SQL local, identificado por el email de desarrollo. El script recibe el hash como parámetro y no contiene la contraseña en claro ni un hash fijo; repetir la preparación actualiza el hash según la configuración vigente. No ejecutar esta preparación al arrancar la aplicación ([decisions.md → «Identidad de desarrollo»](decisions.md#identidad-de-desarrollo) y [architecture.md → «Autenticación»](architecture.md#autenticación)).
- Implementar el caso de uso de login con las reglas de [specifications.md → «Inicio de sesión»](specifications.md#inicio-de-sesión), [«Bloqueo por intentos fallidos»](specifications.md#bloqueo-por-intentos-fallidos) y [«Respuestas que no revelan si una cuenta existe»](specifications.md#respuestas-que-no-revelan-si-una-cuenta-existe).
- Emitir la cookie de autenticación y proteger el dashboard según [architecture.md → «Autenticación»](architecture.md#autenticación). El login debe ser anónimo bajo `/app`; `/app` redirige al dashboard, que exige autenticación. Tras el login se valida `ru` según su especificación.
- Sustituir la identidad de desarrollo inyectada por la identidad resuelta desde la cookie, sin permitir que el cliente elija el `UserId`.

Criterio de finalización:

- El usuario de desarrollo inicia sesión con la verificación Argon2id y recibe la cookie conforme a las reglas de sesión.
- Una petición anónima no accede al dashboard; tras iniciar sesión, el usuario puede acceder a esa página protegida y las operaciones reciben su identidad desde la cookie.
- Credenciales incorrectas producen la respuesta genérica especificada; también se verifica el bloqueo por intentos fallidos.
- Las pruebas automatizadas de backend e integración HTTP cubren credenciales, cookie, autorización, `SecurityStamp`, antiforgery y `ru`. La presentación y el recorrido del formulario se revisan manualmente; no se automatiza la UI ni se usan herramientas de navegador.

Para que el email esté confirmado por su titular y no solo marcado como confirmado en el SQL local, hay que implementar el flujo de registro: crear la cuenta sin completar, emitir y enviar un token, y fijar `EmailConfirmedAt` al completar el registro mediante POST. Eso requiere `UserToken`, `IEmailSender`, MailKit y smtp4dev; forma parte de [Autenticación y gestión de cuenta](#autenticación-y-gestión-de-cuenta), no de LoginV0. El SQL puede preparar un usuario confirmado para la prueba local, pero no sustituye esa verificación real.

### Gestión de etiquetas

Objetivos:

- Implementar `Tag` y `LinkTag` y las operaciones para crear, editar y eliminar etiquetas, asociarlas a enlaces y consultarlas ([domain-model.md → «Tag»](domain-model.md#tag) y [domain-model.md → «LinkTag»](domain-model.md#linktag)).
- Incorporar nombres de etiquetas a la búsqueda de enlaces, permitir filtrar enlaces por etiqueta y buscar etiquetas por nombre en su propio listado ([specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros) y [specifications.md → «Rutas privadas»](specifications.md#rutas-privadas)).
- Añadir la proyección FTS5 de etiquetas, incluir sus nombres en la de enlaces y mantener las tres proyecciones sincronizadas ([architecture.md → «FTS5»](architecture.md#fts5)).
- La gestión de etiquetas estará debajo de la ruta común de la zona privada [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).

Criterio de finalización:

- La gestión de etiquetas respeta la propiedad por usuario y sus operaciones actualizan de forma atómica las relaciones y las proyecciones de búsqueda afectadas.
- La búsqueda de etiquetas devuelve solo etiquetas del usuario identificado y consulta solo su propia proyección.
- El filtro por etiqueta se aplica desde el MVP1 y no antes.

#### Checklist de calidad de etiquetas

- Tests de las invariantes de [domain-model.md → «Tag»](domain-model.md#tag) y [domain-model.md → «LinkTag»](domain-model.md#linktag).
- Tests de generación y colisión de slugs de etiquetas, incluida la asociación de una existente al crearla desde el formulario del enlace.
- Tests de propiedad al asociar etiquetas y de rechazo de etiquetas de otro usuario.
- Tests del orden alfabético del listado de etiquetas ([specifications.md → «Listados»](specifications.md#listados)).
- Tests de búsqueda por nombres de etiquetas y de sincronización de las proyecciones FTS5 al crear, modificar, asociar, desasociar y eliminar etiquetas, incluidas las filas de sus enlaces en la proyección de enlaces.
- Tests del filtro por etiqueta ([specifications.md → «Rutas privadas»](specifications.md#rutas-privadas)): mismas reglas de formato, propiedad, inexistencia y repetición que el filtro de colección, combinación con la búsqueda y con el filtro de colección, y conservación al buscar y al paginar; el filtro por etiqueta se ignora en los listados de colecciones y de etiquetas.
- Tests de búsqueda de etiquetas ([specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros) y [specifications.md → «Listados»](specifications.md#listados)): mismas reglas de `q`, coincidencia solo por `Tag.Name`, resultado solo de etiquetas del usuario identificado, orden alfabético con empates por `Id`, paginación sobre el resultado filtrado, aislamiento frente a enlaces y colecciones que contienen el mismo texto, y `Id` iguales entre entidades y usuarios distintos sin colisiones.
- Test del comando de reconstrucción de las tres proyecciones FTS5 (enlaces, colecciones y etiquetas).

### Autenticación y gestión de cuenta

La implementación queda bloqueada hasta resolver [decisions.md → «Pendiente de rutas de cuenta»](decisions.md#pendiente-de-rutas-de-cuenta) y definir las rutas y métodos en [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).

Objetivos:

- Sustituir la identidad de desarrollo inyectada por autenticación real mediante cookies y autorización de la zona privada ([architecture.md → «Autenticación»](architecture.md#autenticación)), y el botón de entrada de la landing por los de registro y login ([specifications.md → «Landing»](specifications.md#landing)).
- Implementar registro, login y cierre de sesión, recuperación de contraseña, configuración y eliminación de cuenta ([specifications.md → «Cuenta»](specifications.md#cuenta) y [specifications.md → «Sesión»](specifications.md#sesión)).
- Incorporar envío de correo local con MailKit y smtp4dev ([architecture.md → «Correo»](architecture.md#correo)).
- Aplicar las reglas de [specifications.md → «Contraseñas»](specifications.md#contraseñas), [specifications.md → «Tokens»](specifications.md#tokens), [specifications.md → «Respuestas que no revelan si una cuenta existe»](specifications.md#respuestas-que-no-revelan-si-una-cuenta-existe) y [specifications.md → «Sesión»](specifications.md#sesión).
- La gestión de la cuenta estará debajo de la ruta común de la zona privada [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).

Criterio de finalización:

- Los flujos de cuenta y autenticación cumplen sus especificaciones y las operaciones privadas reciben la identidad de la sesión, nunca un identificador elegido por el cliente.
- La cookie, el correo, la autorización y los cambios de sesión superan las pruebas de seguridad correspondientes.

#### Checklist de calidad de autenticación

- Tests de las invariantes de [domain-model.md → «User»](domain-model.md#user) y [domain-model.md → «UserToken»](domain-model.md#usertoken) en los flujos de cuenta.
- Tests de normalización y validación mínima del email y las contraseñas, incluida la equivalencia NFC entre contraseña establecida y contraseña presentada al verificarla, además de bloqueo, respuestas que no revelan cuentas y hash de contraseñas.
- Tests de tokens: caducidad, un solo uso, almacenamiento solo como hash, correspondencia entre `UserId`, `Purpose`, operación y estado de cuenta, invalidación registrada en `InvalidatedAt`, distinción frente a `UsedAt` y exclusión mutua entre ambos estados.
- Tests de concurrencia en verificaciones de contraseña ([specifications.md → «Verificación concurrente de contraseña»](specifications.md#verificación-concurrente-de-contraseña)): un cambio o restablecimiento concurrente del sello impide continuar con una verificación antigua; una verificación fallida contra un hash obsoleto no incrementa el contador nuevo; el login no emite una cookie con un sello desactualizado; y un bloqueo que aparece durante la verificación impide la operación.
- Tests de los límites de correo y de las respuestas de registro, recuperación y cambio de email.
- Tests de registro y compleción, recuperación, cambio de email y cambio de contraseña; en eliminación, el GET no modifica ni consume el token y solo el POST explícito elimina la cuenta y sus datos.
- Tests de sesión: caducidad, «Recordarme», `SecurityStamp`, cierre y renovación de sesiones; el GET del enlace de cambio de email no modifica datos.
- Tests de `ru`, atributos de la cookie y ausencia de datos personales en claro en el ticket ([specifications.md → «Inicio de sesión»](specifications.md#inicio-de-sesión) y [architecture.md → «Autenticación»](architecture.md#autenticación)).
- Tests del origen de enlaces de correo y del registro de eventos de autenticación sin email en claro.

### Scraping de metadatos

Objetivos:

- Resolver los pendientes de [decisions.md → «Pendientes de scraping»](decisions.md#pendientes-de-scraping) antes de implementar.
- Implementar el flujo de [specifications.md → «Scraping»](specifications.md#scraping).
- Implementar las medidas de [architecture.md → «Scraping»](architecture.md#scraping), incluida la protección SSRF.

Criterio de finalización:

- El enlace se conserva aunque no haya metadatos.
- Los reintentos no bloquean las operaciones de base de datos.
- El flujo y sus decisiones pendientes quedan resueltos y documentados.

#### Checklist de calidad del scraping

- Tests de recorte y truncado de metadatos obtenidos por el scraper según [specifications.md → «Textos introducidos por el usuario»](specifications.md#textos-introducidos-por-el-usuario) y [specifications.md → «Longitudes máximas»](specifications.md#longitudes-máximas), sin partir parejas sustitutas UTF-16.
- Tests de `og:image` con rutas relativas en páginas `http` y `https`, incluyendo `cdn.example.com/img.jpg` como referencia relativa, `//cdn.example.com/img.jpg` como autoridad sin esquema y `localhost:5000/img.jpg` como autoridad rechazada.
- Tests de que el resultado de un intento actualiza la proyección FTS5 en la misma transacción y de que nunca sobrescribe campos ya informados.
- Tests del flujo de scraping: respuesta `200` con y sin metadatos, error transitorio, error definitivo, protección SSRF y registro de fallos y bloqueos por SSRF.

## Backlog de MVP1 (borrador, no comprometido)

Candidatos sin fecha ni compromiso de entrega. La justificación de cada uno está en [decisions.md → «Fuera de alcance del MVP0»](decisions.md#fuera-de-alcance-del-mvp0).

- Redirecciones `301` de alias y de slug de colección.
- Reserva temporal de los alias liberados.
- Consulta online de contraseñas comprometidas (Pwned Passwords).
- Proveedor real de correo.
- «Recordarme» persistente sin fecha de fin, con un token de larga duración separado de la sesión.
- Limpieza de los registros sin completar.
- SEO avanzado.
- Content-Security-Policy completa.
- Proxy o caché de imágenes en el servidor.
- Revisión de la conversión a ASCII de alias y slugs, y de una clave propia para las etiquetas.
