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

### Dominio IV

La Definition of Done de S3a está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S3a: Dominio IV (M)»](../plans/mvp0-plan.md#s3a-dominio-iv-refactor-m).

### Aplicación

La Definition of Done de S4 está completada. Las tareas y su evidencia se mantienen en [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l).

## Now

Esta es la etapa activa del roadmap. La persistencia en curso se detalla bajo esta sección; la fase de Aplicación está completada y documentada en [Hecho → «Aplicación»](#aplicación).

### Persistencia — trabajos S5 cerrados y archivados

Objetivos:

- Preparar SQLite, incorporar Dapper y crear los scripts SQL versionados.
- Preparar el archivo SQL local especial que provisiona el usuario de desarrollo y su colección privada base, según [decisions.md → «Identidad de desarrollo»](decisions.md#identidad-de-desarrollo).
- Añadir las restricciones únicas y los índices.
- Registrar la colación propia del orden alfabético en todas las conexiones ([architecture.md → «Persistencia»](architecture.md#persistencia)).
- Implementar el Unit of Work y las transacciones según [architecture.md → «Persistencia»](architecture.md#persistencia). Las proyecciones FTS5 pertenecen a S6 y no bloquean el cierre de Persistencia.

La [S5.1 — configuración de SQLite y Dapper](../specs/archive/s5.1-configuracion-sqlite-dapper/spec.md), su [plan técnico](../specs/archive/s5.1-configuracion-sqlite-dapper/plan.md) y sus [tareas](../specs/archive/s5.1-configuracion-sqlite-dapper/tasks.md) fueron aprobados por DLG el 19-X-2026 («Siguiente» en cada artefacto). La implementación quedó completada y aceptada por DLG el 19-X-2026 («Tenemos BBDD»); los tests focalizados pasaron 2/2 y el build de solución fue correcto. El archivo físico se creará al abrir una conexión de la aplicación; el directorio `data/database` está preparado. S5.1 queda cerrada para su alcance.

La [S5.2 — ejecutor de scripts SQL versionados](../specs/archive/s5.2-ejecutor-scripts-sql-versionados/spec.md) y su [plan técnico](../specs/archive/s5.2-ejecutor-scripts-sql-versionados/plan.md) fueron aprobados por DLG el 19-X-2026 («Siguiente»). DLG aprobó las [tareas](../specs/archive/s5.2-ejecutor-scripts-sql-versionados/tasks.md) con «Completado» y aceptó el resultado con «Migraciones ready», ambas referencias el 19-X-2026. La implementación y validación quedaron completadas; los tests focalizados pasaron 3/3 y el build de solución fue correcto. La evidencia consta en sus tareas. Los criterios cubren aplicación ordenada, idempotencia y registro de fallos mediante scripts de prueba, sin depender del esquema de S5.3.

La [S5.3 — esquema de `User`, `Collection` y `Link`](../specs/archive/s5.3-esquema-user-collection-link/spec.md) y su [plan técnico](../specs/archive/s5.3-esquema-user-collection-link/plan.md) fueron aprobados por DLG el 19-X-2026 («Siguiente» en cada artefacto); DLG aprobó las [tareas](../specs/archive/s5.3-esquema-user-collection-link/tasks.md) el mismo día («Ready»). La implementación y la validación estructural quedaron completadas el 19-X-2026. La revisión final amplió la inspección de columnas, tipos y nulabilidad, índices únicos y secundarios (incluida su dirección), acciones FK, FK compuesta, valores por defecto, alias nulos, formato UTC ISO-8601 y ausencia de cascadas/triggers. Se quitó de `V004` el índice redundante sobre `(UserId, Id)`, ya cubierto por la clave única; como no hay bases creadas, no se necesita una migración correctiva adicional. Los tests focalizados pasaron 4/4 y el build de solución fue correcto. DLG aceptó y cerró S5.3 el 19-X-2026 («Tenemos schema»); el registro está en las tareas.

La [S5.4 — registro de la colación `es-ES` en las conexiones SQLite](../specs/archive/s5.4-colacion-es-es/spec.md), su [plan técnico](../specs/archive/s5.4-colacion-es-es/plan.md) y sus [tareas](../specs/archive/s5.4-colacion-es-es/tasks.md) fueron aprobados por DLG el 19-X-2026 («Siguiente» en cada artefacto); el nombre SQL `LINKUBATOR_ES_ES` quedó confirmado al aprobar el plan. La implementación y validación quedaron completadas el 19-X-2026: tests focalizados 5/5, build correcto y `git diff --check` limpio. DLG aceptó y cerró S5.4 para su alcance el 19-X-2026 («Colación añadida»); el detalle y la evidencia constan en sus [tareas → «Aceptación y cierre»](../specs/archive/s5.4-colacion-es-es/tasks.md#aceptación-y-cierre). S5.8 contiene las pruebas integradas del orden completo de los listados.

La [S5.5 — Unit of Work y repositorios Dapper](../specs/archive/s5.5-unit-of-work-y-repositorios-dapper/spec.md) fue aprobada por DLG el 19-X-2026 («Siguiente»), el [plan técnico](../specs/archive/s5.5-unit-of-work-y-repositorios-dapper/plan.md) con «Siguiente» y las [tareas](../specs/archive/s5.5-unit-of-work-y-repositorios-dapper/tasks.md) con «Ready». DLG confirmó que la búsqueda textual queda para S6. La implementación y validación técnica se completaron: tests focalizados SQLite 60/60, suite completa 1005/1005, build correcto y `git diff --check` limpio el 19-X-2026. DLG aceptó y cerró S5.5 para su alcance el 19-X-2026 («Repositorios listos»); el riesgo de `LinkService.CreateLink` sigue como seguimiento independiente y no reabre este trabajo.

La [S5.6 — aprovisionamiento SQL de identidad de desarrollo](../specs/archive/s5.6-sql-local-identidad-desarrollo/spec.md) fue aprobada por DLG el 19-X-2026 («Validado y confirmado»), su [plan técnico](../specs/archive/s5.6-sql-local-identidad-desarrollo/plan.md) por «Siguiente» y sus [tareas](../specs/archive/s5.6-sql-local-identidad-desarrollo/tasks.md) por «Vamos», el mismo día. DLG confirma que la implementación comenzó después de aprobar las tareas; el día exacto de inicio no consta. La implementación y validación técnica incluyen escrituras condicionales de cuenta/colección y un verificador criptográfico Argon2id PHC en `Infrastructure.Security.PasswordHashing`; el 19-X-2026 pasaron los tests focalizados 26/26, la suite 1026/1026, el build y la publicación Web. DLG comunicó la aceptación «Tenemos demo user» y confirmó que la comprobación PHC con vectores de prueba basta en este momento; S5.6 queda cerrada para su alcance. El detalle y las evidencias constan en sus [tareas → «Evidencia por criterio» y «Aceptación y cierre»](../specs/archive/s5.6-sql-local-identidad-desarrollo/tasks.md#evidencia-por-criterio). Las reglas y el riesgo aceptado constan en [specifications.md → «Identidad de desarrollo»](specifications.md#identidad-de-desarrollo) y [decisions.md → «Identidad de desarrollo»](decisions.md#identidad-de-desarrollo).

La [S5.7 — proveedor de identidad de desarrollo](../specs/archive/s5.7-proveedor-identidad-desarrollo/spec.md), su [plan técnico](../specs/archive/s5.7-proveedor-identidad-desarrollo/plan.md) y sus [tareas](../specs/archive/s5.7-proveedor-identidad-desarrollo/tasks.md) fueron aprobados por DLG el 19-X-2026 («Siguiente»). La implementación y validación técnica se completaron el 19-X-2026: proveedor SQLite registrado en todos los entornos, resolución comprobada al arranque después del aprovisionamiento, y middleware asíncrono de `/app` que establece el usuario scoped sin aceptar un identificador de la petición. Pasaron los tests focalizados 11/11, la suite 1032/1032 y el build de solución; el detalle consta en las tareas. DLG aceptó y cerró S5.7 para su alcance el 19-X-2026 («Completado»).

La [S5.8 — tests de integración de persistencia](../specs/archive/s5.8-tests-integracion-persistencia/spec.md), su [plan técnico](../specs/archive/s5.8-tests-integracion-persistencia/plan.md) y sus [tareas](../specs/archive/s5.8-tests-integracion-persistencia/tasks.md) están aprobados por DLG. La implementación terminó el 19-X-2026: tests focalizados 13/13, suite completa 1045/1045, build y `git diff --check` correctos. DLG aceptó y cerró S5.8 para su alcance el 19-X-2026 («Completada»).

La [S5.9 — revisión manual de migraciones y esquema SQLite](../specs/archive/s5.9-revision-manual-migraciones-esquema/spec.md), su [plan técnico](../specs/archive/s5.9-revision-manual-migraciones-esquema/plan.md) y sus [tareas](../specs/archive/s5.9-revision-manual-migraciones-esquema/tasks.md) quedaron archivados el 19-X-2026. CA04 se completó con una verificación suplementaria identificada como tal: base temporal `s59-review-20261019-003.db`, .NET SDK 10.0.401, Microsoft.Data.Sqlite 10.0.0.0 y SQLite 3.53.3; V004 y el esquema se inspeccionaron sin discrepancias. Se conserva sin cambios la aceptación anterior de DLG («Completada»).

La especificación S5.10, [revisión manual de la aplicación repetida del script local](../specs/archive/s5.10-revision-manual-aplicacion-script-local/spec.md), y su [plan técnico](../specs/archive/s5.10-revision-manual-aplicacion-script-local/plan.md) fueron aprobados por DLG con «Siguiente» el 19-X-2026; sus [tareas](../specs/archive/s5.10-revision-manual-aplicacion-script-local/tasks.md) fueron aprobadas por DLG con «Vamos» el 19-X-2026. La comprobación manual se completó el 19-X-2026 y CA01–CA03 tienen evidencia satisfactoria. DLG aceptó y cerró S5.10 para su alcance el 19-X-2026 con «Completado»; el detalle y la evidencia constan en las tareas archivadas.

La spec, el [plan técnico](../specs/archive/s5.2.1-separar-scripts-prueba-catalogo/plan.md) y las [tareas](../specs/archive/s5.2.1-separar-scripts-prueba-catalogo/tasks.md) de [S5.2.1 — separación de scripts de prueba del catálogo de migraciones](../specs/archive/s5.2.1-separar-scripts-prueba-catalogo/spec.md) fueron aprobados por DLG («Siguiente», «Siguiente» y «Aprobado», respectivamente, 19-X-2026). La implementación y validación terminaron el 19-X-2026: suite completa 1046/1046, build correcto, catálogo real solo con V004 y compatibilidad comprobada con un historial previo V001–V004. DLG aceptó y cerró S5.2.1 para su alcance con «Completada» el 19-X-2026; el trabajo se archivó el 19-X-2026. Todos los trabajos S5 del roadmap están archivados.

Criterio de finalización:

- Las operaciones de enlaces y colecciones son atómicas.
- La búsqueda respeta siempre el usuario identificado y consulta solo la proyección de la entidad de su listado.
- Las transacciones son breves y no contienen operaciones externas.
- WAL, `busy_timeout` y claves foráneas están configurados.

Las proyecciones FTS5 se implementan en S6 según [architecture.md → «FTS5»](architecture.md#fts5); no son prerrequisito para cerrar Persistencia. La [S6.1 — proyecciones FTS5 de enlaces y colecciones](../specs/s6.1-proyecciones-fts5-enlaces-colecciones/spec.md), su [plan técnico](../specs/s6.1-proyecciones-fts5-enlaces-colecciones/plan.md) y sus [tareas y evidencia de cierre](../specs/s6.1-proyecciones-fts5-enlaces-colecciones/tasks.md#aceptación-y-cierre) están aprobados y cerrados por DLG el 19-X-2026 («Tenemos proyeciones»); se implementó V005 y la suite completa pasó 1050/1050. La [S6.2 — normalización NFC de búsqueda](../specs/s6.2-normalizacion-nfc-busqueda/spec.md), su [plan técnico](../specs/s6.2-normalizacion-nfc-busqueda/plan.md) y sus [tareas](../specs/s6.2-normalizacion-nfc-busqueda/tasks.md) están aprobados por DLG («Siguiente», «Aprobar», «Aprobar», 19-X-2026). Tras la reapertura solicitada por DLG, se revisaron las tareas y su alcance: S6.2 entrega el normalizador y sus tests unitarios directos; la integración de `q`, proyecciones y sus pruebas conjuntas corresponde a S6.3/S6.4/S6.7. El 20-X-2026, `SearchTextNormalizerTests` pasó 6/6 con .NET SDK 10.0.401 y `git diff --check` quedó limpio. La [S6.2 — normalización NFC de búsqueda](../specs/s6.2-normalizacion-nfc-busqueda/spec.md) se completó y DLG la aceptó por «Completado» el 20-X-2026. Sus tests focalizados pasaron 6/6 y `git diff --check` quedó limpio; evidencia en las [tareas S6.2](../specs/s6.2-normalizacion-nfc-busqueda/tasks.md#aceptación-y-cierre). La [S6.3 — constructor de consultas FTS5](../specs/s6.3-constructor-consulta-fts5/spec.md), su [plan técnico](../specs/s6.3-constructor-consulta-fts5/plan.md) y sus [tareas](../specs/s6.3-constructor-consulta-fts5/tasks.md) fueron aprobados por DLG el 20-X-2026 («Siguiente» en cada artefacto y «Vamos» para las tareas). DLG inició la implementación el 20-X-2026 («Inicia la implementación»); T01 confirmó la ubicación del módulo `Search` en Infrastructure y el patrón de tests en `Linkubator.Tests`. El constructor y sus tests unitarios quedaron implementados; la validación focalizada pasó 12/12 con .NET SDK 10.0.401 y `git diff --check` quedó limpio el 20-X-2026. DLG aceptó y cerró S6.3 para su alcance con «Completado» el 20-X-2026. La ejecución integrada de consultas sigue asignada a trabajos posteriores y no reabre S6.3. Evidencia en [tasks.md → «Evidencia por criterio»](../specs/s6.3-constructor-consulta-fts5/tasks.md#evidencia-por-criterio). La [S6.4 — sincronización de proyecciones FTS5](../specs/s6.4-sincronizacion-proyecciones-fts5/spec.md), su [plan técnico](../specs/s6.4-sincronizacion-proyecciones-fts5/plan.md) y sus [tareas](../specs/s6.4-sincronizacion-proyecciones-fts5/tasks.md#aceptación-y-cierre) fueron aprobados por DLG («Siguiente», «Siguiente» y «Vamos») el 20-X-2026. La implementación sincroniza las escrituras de enlaces y colecciones con sus proyecciones, normaliza el texto proyectado a NFC y mantiene los valores originales; los tests focalizados pasaron 35/35, la suite completa 1073/1073 y `git diff --check` quedó limpio. DLG aceptó y cerró S6.4 para su alcance con «Tenemos proyecciones» el 20-X-2026. La búsqueda integrada y el aislamiento permanecen asignados a S6.5/S6.7; S6.4 no se archiva automáticamente.

La [S6.5 — consultas de búsqueda FTS5](../specs/s6.5-consultas-busqueda-fts5/spec.md), su [plan técnico](../specs/s6.5-consultas-busqueda-fts5/plan.md) y sus [tareas](../specs/s6.5-consultas-busqueda-fts5/tasks.md#aceptación-y-cierre) fueron aprobados por DLG el 20-X-2026 («Siguiente» en la spec y el plan; «Vamos» en las tareas). DLG solicitó iniciar la implementación el 20-X-2026 («Inicia la implementación»). La implementación y validación técnica se completaron el 20-X-2026: tests focalizados 61/61, suite completa 1081/1081 con .NET SDK 10.0.401 y `git diff --check` limpio. DLG aceptó y cerró S6.5 el 20-X-2026 («Consultas y búsquedas»); las evidencias por criterio constan en las tareas.

La [S6.6 — reconstrucción manual de proyecciones FTS5](../specs/s6.6-reconstruccion-proyecciones/spec.md), su [plan técnico](../specs/s6.6-reconstruccion-proyecciones/plan.md) y sus [tareas](../specs/s6.6-reconstruccion-proyecciones/tasks.md#aceptación-y-cierre) fueron aprobados por DLG («Siguiente», «Siguiente» y «Vamos») el 20-X-2026. La implementación se inició y completó el 20-X-2026: servicio transaccional y herramienta manual local; pruebas focalizadas 5/5, build de solución correcto sin warnings ni errores, CLI verificada con SQLite 3.53.3 y `git diff --check` limpio. DLG aceptó y cerró S6.6 para su alcance el 20-X-2026 («Tenemos reconstrucción de proyecciones»). La reconstrucción se limita a enlaces y colecciones; las evidencias por criterio constan en sus tareas. La comprobación manual S6.9 sigue en su etapa propietaria.

## Next

### Interfaz privada - Gestión básica

Objetivos:

- Seguridad web para la aplicación local: HTTPS, antiforgery y cabeceras ([architecture.md → «Seguridad web»](architecture.md#seguridad-web)).
- Gestión de colecciones y enlaces.
- Dashboard y rutas privadas según [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).
- Búsqueda de enlaces con filtro por colección y búsqueda de colecciones por nombre, cada una en su propio listado ([specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros)).
- Estados vacíos y errores accesibles ([requirements.md → «Accesibilidad»](requirements.md#accesibilidad)).

Criterio de finalización:

- La gestión privada usa la identidad de desarrollo inyectada y solo opera sobre los datos de ese usuario.
- Cada listado aplica solo sus propios parámetros, ignora el resto y conserva sus filtros al buscar y al paginar ([specifications.md → «Rutas privadas»](specifications.md#rutas-privadas)).
- El formulario de creación de enlaces muestra la casilla de público desmarcada; al crear un enlace en una colección privada, la opción pública está deshabilitada y la aplicación rechaza una petición manipulada de creación, edición o cambio de privacidad según [specifications.md → «Errores controlados»](specifications.md#errores-controlados), mediante integración HTTP.
- El dashboard, los listados y los formularios privados respetan [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas); los casos de uso no permiten elegir o alterar el `UserId` desde el cliente.

## Later

### Área pública y fundamentos SEO

Objetivos:

- Crear la landing, la página de usuario (con los últimos enlaces públicos, después las últimas colecciones que se muestran en público y acceso a todas), la página de colecciones y la página de colección según [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas).

Criterio de finalización:

- No quedan textos pendientes en las páginas públicas.
- Las colecciones y los enlaces privados nunca aparecen en las páginas públicas.
- El HTML público se renderiza en servidor y es legible por los buscadores.
- En la landing del MVP0, el enlace de registro tiene `href="#"` y no navega a un formulario ni envía una solicitud.

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
