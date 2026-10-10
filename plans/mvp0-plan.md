# Plan de implementación del MVP0

Este plan organiza la construcción del MVP0 en sprints de 1 semana, con la IA generando código y el humano guiando y realizando las pruebas manuales. Sigue el orden de [roadmap.md](../context/roadmap.md) y no define reglas de producto: las reglas viven en `context/` y aquí solo se enlazan. Quedan fuera `Tag`, `UserToken`, login, scraping, correo y diálogos, que son de MVP1 ([decisions.md → «Producto y alcance»](../context/decisions.md#producto-y-alcance)).

## 1. Reparto de responsabilidades

### Tareas principales de la IA

- Crear la solución .NET 10 y mantener las capas Domain, Application, Infrastructure, Web y Tests, respetando [architecture.md → «Arquitectura»](../context/architecture.md#arquitectura).
- Implementar reglas de dominio, casos de uso, SQL explícito con Dapper, scripts versionados, FTS5, Razor Pages y Tailwind.
- Escribir los tests automáticos de la [checklist de calidad](../context/roadmap.md#checklist-de-calidad-del-mvp0): unitarios, integración SQLite e integración HTTP con `WebApplicationFactory`.
- Entregar con cada sprint una guía de pruebas manuales con pasos y resultado esperado, y una matriz de trazabilidad regla → test.
- Autorrevisar cada cambio contra [specifications.md](../context/specifications.md) y las invariantes de [AGENTS.md](../AGENTS.md).
- Detener el trabajo y proponer un cambio documental cuando detecte una ambigüedad o contradicción. No improvisa reglas ni edita `context/` por su cuenta.

### Tareas principales del humano

- Preparar el entorno: SDK .NET 10, certificado HTTPS de desarrollo, binario de Tailwind CLI y repositorio.
- Priorizar y aceptar el alcance de cada sprint, y decidir las ambigüedades.
- Revisar el código y los diffs, con foco en seguridad, propiedad por usuario, transacciones y SQL.
- Aprobar los criterios de aceptación de los casos de uso (puerta de S4).
- Ejecutar las pruebas manuales: navegadores actuales, teclado, foco, contraste, lector de pantalla, HTML público renderizado y textos.
- Aportar datos reales (URLs, nombres de colecciones) para contrastar los algoritmos.
- Cerrar cada sprint con demo, retro y actualización del [roadmap.md](../context/roadmap.md).

### Ritual de cada sprint

1. Planning: la IA propone el desglose y el humano confirma el alcance.
2. SDD de la subárea: la IA prepara la vista técnica del bloque a implementar y el humano la aprueba antes de empezar el desarrollo.
3. Desarrollo en rebanadas pequeñas, con tests verdes antes de cada revisión.
4. Revisión humana de los diffs.
5. Pruebas manuales con la guía.
6. Demo y retro: lo rechazado vuelve al backlog del siguiente sprint.

### Puerta de entrada de la implementación (criterios de aceptación de S4)

- La IA redacta, para cada caso de uso del MVP0, criterios en formato dado/cuando/entonces. Cada criterio enlaza a su regla en [specifications.md](../context/specifications.md) o [domain-model.md](../context/domain-model.md).
- La IA prepara también el SDD técnico del bloque a implementar y lo deja enlazado a las fuentes funcionales y a los criterios de aceptación. Sin la aprobación explícita del humano, no empieza la implementación del sprint.
- Sin la aprobación explícita del humano (S4.5), la IA no implementa ningún caso de uso, repositorio concreto ni página Razor.
- Los criterios aprobados son la fuente de los tests de aplicación (S4), de integración HTTP (S8–S11) y de las guías manuales.
- Los sprints S5–S11 no redefinen criterios. Si descubren un hueco, la IA propone el cambio y el humano lo aprueba antes de implementarlo.
- Los criterios se guardan en [`plans/acceptance-criteria.md`](acceptance-criteria.md), fuera de `context/`, y enlazan a las reglas sin copiarlas.

## 2. Resumen de sprints

| Sprint | Nombre | Talla | Área | Entregables |
|---|---|---|---|---|
| S0 | Fundaciones | M | Transversal | Solución con 5 proyectos, Serilog, Tailwind CLI, tests base, README de desarrollo |
| S1 | Dominio I: textos, alias y slugs | L | Domain | Recorte y longitudes, generación de alias y de slugs, y sus tests |
| S2 | Dominio II: URLs | L | Domain | Ajuste de URL y de `Image`, validación, `UrlNormalized`, duplicados, y sus tests |
| S3 | Dominio III: entidades e invariantes | M | Domain | `User`, `Collection` y `Link`, propiedad, público/privado, tests de invariantes |
| S4 | Aplicación: casos de uso | L | Application | `Result`, contratos, criterios de aceptación aprobados (puerta), abstracciones de repos y UoW, casos de uso con fakes, contrato de identidad de desarrollo |
| S5 | Persistencia I: SQLite y transacciones | L | Infrastructure | Esquema versionado, PRAGMAs, colación `es-ES`, UoW con `BEGIN IMMEDIATE`, repositorios Dapper, SQL local de desarrollo, proveedor de identidad |
| S6 | Persistencia II: FTS5 | L | Infrastructure | Proyecciones de enlaces y de colecciones, sincronización atómica, consulta de búsqueda, comando de reconstrucción |
| S7 | Web base, seguridad y accesibilidad | L | Web | HTTPS, cabeceras, `Host`, antiforgery, layout accesible, dashboard, parámetros de listado, errores comunes, logs sin `q` |
| S8 | Gestión de colecciones | L | Web | Listado con búsqueda y paginación, crear, editar, eliminar y cambiar visibilidad |
| S9 | Gestión de enlaces | L | Web | Crear, editar, mover, cambiar visibilidad y eliminar enlaces, con duplicados y rechazo de peticiones manipuladas |
| S10 | Listado y búsqueda de enlaces | L | Web | `/app/enlaces` con `q`, filtro `c` y paginación, relevancia, búsqueda desde el dashboard |
| S11 | Área pública y SEO | L | Web | Landing, página de usuario, de colecciones y de colección, canonical, redirecciones, `404` |
| S12 | Cierre y hardening | M | Transversal | Checklist completa, revisión de accesibilidad y navegadores, auditoría de seguridad, documentación sincronizada |

Ninguna tarea se talla como XL. El mayor bloque de funcionalidad, los enlaces, ya está partido en S9 y S10.

## 3. Definition of Done común (aplica a todos los sprints)

- [ ] Compila sin warnings y todos los tests pasan.
- [ ] Cada regla implementada tiene test enlazado a su sección de [specifications.md](../context/specifications.md) o [domain-model.md](../context/domain-model.md).
- [ ] No hay dependencias que rompan las capas, ni reglas de negocio en Web.
- [ ] SQL explícito y transacciones cortas, sin HTTP ni correo dentro de ellas.
- [ ] Ningún `UserId`, alias o slug del cliente se usa para autorizar.
- [ ] Sin secretos ni contraseñas en el repositorio ni en los logs.
- [ ] La IA ha entregado la guía de pruebas manuales y el humano la ha ejecutado y aceptado.
- [ ] Si el sprint cambia o descubre una regla, la documentación se actualiza en su documento propietario.
- [ ] El comportamiento implementado coincide con los criterios de aceptación aprobados en S4 (desde S5).

## 4. Sprints

### S0: Fundaciones (M)

**Objetivo:** dejar un esqueleto que compila y arranca por HTTPS, con convenciones y base de tests listas.

| ID | Tarea | Resp. |
|---|---|---|
| S0.1 | Instalar SDK .NET 10, `dotnet dev-certs https --trust` y Tailwind standalone CLI | Humano |
| S0.2 | Crear solución con Domain, Application, Infrastructure, Web y Tests, con las referencias permitidas por las capas | IA |
| S0.3 | Configurar analizadores, nullable, `.editorconfig` y `.gitignore` (excluye BD, logs y configuración local) | IA |
| S0.4 | Integrar Serilog con consola y archivo rotativo, configurable sin tocar casos de uso | IA |
| S0.5 | Configurar Razor Pages, compilación de Tailwind en el build y `AllowedHosts` | IA |
| S0.6 | Añadir configuración local: origen público, ruta de la BD y email de desarrollo | IA |
| S0.7 | Configurar xUnit y `WebApplicationFactory` en el proyecto Tests creado en S0.2, comprobar las referencias entre proyectos y añadir con NetArchTest la regla de que solo `Web/Program.cs` depende de Infrastructure | IA |
| S0.8 | Escribir el README de desarrollo (arranque, tests, comandos) | IA |
| S0.9 | Acordar convenciones de ramas, PRs y commits, y crear el backlog con los sprints | Humano |

**Definition of Done**

- [x] La aplicación arranca por HTTPS en local y muestra una página mínima.
- [x] `dotnet build` y `dotnet test` pasan en limpio.
- [x] Hay logs por consola y por archivo.
- [x] Los tests de referencias entre proyectos y del límite del composition root pasan.
- [x] El README permite a otra persona arrancar el proyecto.
- [x] Las convenciones de ramas, PRs y commits están acordadas.

### S1: Dominio I: textos, alias y slugs (L)

**Objetivo:** implementar las transformaciones de texto de dominio, con tests exhaustivos de sus ejemplos.

| ID | Tarea | Resp. |
|---|---|---|
| S1.1 | Implementar el recorte y la validación de longitudes máximas por puntos de código (BMP, marcas combinantes) según [«Textos introducidos por el usuario»](../context/specifications.md#textos-introducidos-por-el-usuario) y [«Longitudes máximas»](../context/specifications.md#longitudes-máximas) | IA |
| S1.2 | Implementar los errores de dominio base y los enumeradores necesarios | IA |
| S1.3 | Implementar la generación del alias, con acentos, guiones, rangos, palabras reservadas y alias ocupado | IA |
| S1.4 | Implementar la transformación común a ASCII (NFKD, tabla de conversión y rechazo de caracteres no admitidos), la generación de slugs con sus rangos y la adaptación del alias a ella; la detección de colisiones se integra en S3–S5 | IA |
| S1.5 | Escribir tests con todos los ejemplos de [«Generación del alias»](../context/specifications.md#generación-del-alias) y [«Generación de slugs»](../context/specifications.md#generación-de-slugs) | IA |
| S1.6 | Revisar la tabla de símbolos contra el código y probar 20 nombres reales de colecciones | Humano |
| S1.7 | Resolver las ambigüedades que surjan y aprobar el comportamiento | Humano |

**Definition of Done**

- [x] Todos los ejemplos de las especificaciones de alias y slug están cubiertos por tests.
- [x] Recorte y longitudes cubren puntos de código fuera del BMP y marcas combinantes.
- [x] Las reglas viven en Domain, sin dependencias de infraestructura.
- [x] El humano ha validado los 20 nombres reales.
- [x] No quedan ambigüedades abiertas sin registrar.

### S2: Dominio II: URLs (L)

**Objetivo:** implementar ajuste, validación y normalización de URLs de enlace e imagen, como reglas separadas.

| ID | Tarea | Resp. |
|---|---|---|
| S2.1 | Implementar la detección de esquema URI y el ajuste de `UrlOriginal` ([«Ajuste de URL»](../context/specifications.md#ajuste-de-url)) | IA |
| S2.2 | Implementar el ajuste de `Image` (`https`, rechazo de `localhost` e IP, `//` → `https:`) ([«Ajuste de `Image`»](../context/specifications.md#ajuste-de-image)) | IA |
| S2.3 | Implementar la validación de ambas ([«Validación»](../context/specifications.md#validación)) | IA |
| S2.4 | Implementar `UrlNormalized` con `%23`, `%26`, `%2520`, `+` y la regla de duplicados ([«Normalización para duplicados»](../context/specifications.md#normalización-para-duplicados) y [«Duplicados»](../context/specifications.md#duplicados)) | IA |
| S2.5 | Escribir tests con todos los ejemplos de las especificaciones, separando `UrlOriginal` e `Image` | IA |
| S2.6 | Proporcionar 30 URLs reales (favoritos propios) y revisar su normalización | Humano |
| S2.7 | Decidir los casos dudosos y registrarlos | Humano |

**Definition of Done**

- [ ] Ajuste y validación de `UrlOriginal` pasan todos los ejemplos.
- [ ] Ajuste y validación de `Image` pasan todos los ejemplos, por separado.
- [ ] `UrlNormalized` y la detección de duplicados cubren todos los ejemplos, incluidos los caracteres codificados.
- [ ] El humano ha comparado las 30 URLs reales con el resultado.
- [ ] Los casos dudosos están decididos y registrados.

### S3: Dominio III: entidades e invariantes (M)

**Objetivo:** modelar `User`, `Collection` y `Link` con sus invariantes, incluida la propiedad y las reglas público/privado.

| ID | Tarea | Resp. |
|---|---|---|
| S3.1 | Implementar `User` (estado completado), `Collection` y `Link` según [domain-model.md](../context/domain-model.md) | IA |
| S3.2 | Implementar las invariantes de [«Propiedad de los datos»](../context/domain-model.md#propiedad-de-los-datos), incluida la igualdad de `UserId` entre enlace y colección | IA |
| S3.3 | Implementar las invariantes y transiciones de [«Público y privado»](../context/domain-model.md#público-y-privado) | IA |
| S3.4 | Escribir tests de invariantes: propiedad entre usuarios, público/privado y transiciones | IA |
| S3.5 | Generar la ficha de cada entidad con la skill requirements-review | IA |
| S3.6 | Revisar las fichas contra el código y aprobar las invariantes | Humano |

**Definition of Done**

- [ ] Las invariantes aplicables al MVP0 tienen test.
- [ ] No existe ningún camino de dominio para hacer público un enlace de una colección privada.
- [ ] Las fichas de entidad coinciden con el código.
- [ ] El humano ha aprobado el modelo.

### S4: Aplicación: casos de uso (L)

**Objetivo:** definir los contratos y los criterios de aceptación, obtener su aprobación y solo entonces implementar los casos de uso con repositorios en memoria.

| ID | Tarea | Resp. | Depende de |
|---|---|---|---|
| S4.1 | Definir `Result`, errores de aplicación, DTOs y validadores | IA | — |
| S4.2 | Definir las interfaces de repositorios y de Unit of Work, y la abstracción del usuario identificado | IA | S4.1 |
| S4.3 | Definir el contrato del proveedor de identidad de desarrollo ([«Identidad de desarrollo»](../context/decisions.md#identidad-de-desarrollo)) | IA | S4.2 |
| S4.4 | Redactar criterios de aceptación (dado/cuando/entonces) de cada caso de uso en [`plans/acceptance-criteria.md`](acceptance-criteria.md), enlazados a su regla | IA | S4.1 |
| S4.5 | **Puerta:** revisar y aprobar los criterios de aceptación, dejando constancia por escrito | Humano | S4.4 |
| S4.6 | Implementar los casos de uso de colecciones: crear, renombrar con regeneración de slug, cambiar visibilidad, eliminar (no la última ni con enlaces) y listar | IA | S4.5 |
| S4.7 | Implementar los casos de uso de enlaces: crear con duplicados, editar, mover, cambiar visibilidad, eliminar y listar | IA | S4.5 |
| S4.8 | Implementar el caso de uso del dashboard y los de lectura pública | IA | S4.5 |
| S4.9 | Escribir fakes en memoria y tests de casos de uso, uno o más por criterio aprobado, incluida la propiedad entre usuarios | IA | S4.6–S4.8 |
| S4.10 | Registrar eventos con `ILogger`, sin datos prohibidos ([«Registro de eventos»](../context/specifications.md#registro-de-eventos)) | IA | S4.6–S4.8 |

S4.1–S4.4 pueden avanzar en paralelo con la revisión; S4.6–S4.10 no empiezan hasta que S4.5 está aprobado.

**Definition of Done**

- [ ] Todos los casos de uso del MVP0 tienen contrato y criterios de aceptación.
- [ ] El humano ha aprobado los criterios por escrito (S4.5) antes de empezar S4.6.
- [ ] Cada criterio aprobado tiene al menos un test que lo verifica.
- [ ] Los casos de uso no aceptan `UserId` del cliente.
- [ ] Los tests con fakes cubren los caminos felices y los de error.
- [ ] Los logs no incluyen contenido prohibido.

### S5: Persistencia I: SQLite y transacciones (L)

**Objetivo:** tener una base de datos real con esquema versionado, restricciones y operaciones atómicas.

| ID | Tarea | Resp. |
|---|---|---|
| S5.1 | Configurar `Microsoft.Data.Sqlite` y Dapper, con WAL, `busy_timeout` y claves foráneas en cada conexión | IA |
| S5.2 | Crear el ejecutor de scripts SQL versionados | IA |
| S5.3 | Escribir el esquema de `User`, `Collection` y `Link` con únicos, la clave foránea compuesta `Link (UserId, CollectionId)` e índices | IA |
| S5.4 | Registrar la colación `es-ES` con `CreateCollation` en todas las conexiones | IA |
| S5.5 | Implementar el Unit of Work con `BEGIN IMMEDIATE` y los repositorios Dapper | IA |
| S5.6 | Escribir el SQL local de desarrollo, idempotente: usuario completado y «Bandeja de entrada» | IA |
| S5.7 | Implementar el proveedor de identidad, que solo se registra en desarrollo y falla al arrancar si no hay usuario completado | IA |
| S5.8 | Escribir tests de integración sobre SQLite temporal: únicos, clave compuesta, carreras (última colección, privada vs público, mover a colección ajena) y orden alfabético | IA |
| S5.9 | Ejecutar las migraciones, revisar el SQL y comprobar el esquema con una herramienta SQLite | Humano |
| S5.10 | Revisar manualmente la aplicación del script local dos veces seguidas | Humano |

**Definition of Done**

- [ ] Los scripts se aplican sobre una BD vacía y la dejan en el estado esperado.
- [ ] WAL, `busy_timeout` y claves foráneas están activos en cada conexión.
- [ ] La colación `es-ES` ordena según [«Listados»](../context/specifications.md#listados).
- [ ] El script local se puede repetir sin duplicar datos.
- [ ] La aplicación no arranca sin usuario de desarrollo completado, y el proveedor solo se registra en desarrollo.
- [ ] Los tests de transacciones y de carreras pasan.
- [ ] Los repositorios cumplen los contratos y criterios de S4.
- [ ] El humano ha revisado el esquema y el SQL.

### S6: Persistencia II: FTS5 (L)

**Objetivo:** añadir la búsqueda de texto sin romper la atomicidad ni el aislamiento entre usuarios y entre entidades.

| ID | Tarea | Resp. |
|---|---|---|
| S6.1 | Crear las proyecciones FTS5 de enlaces y de colecciones, con `unicode61` y `remove_diacritics 2` | IA |
| S6.2 | Implementar la normalización NFC al indexar y al buscar ([«Búsqueda y filtros»](../context/specifications.md#búsqueda-y-filtros)) | IA |
| S6.3 | Implementar el constructor de la consulta: términos entre comillas, AND implícito, sin prefijos y sin términos = no buscar | IA |
| S6.4 | Sincronizar las proyecciones en la misma transacción que altas, cambios y borrados, incluido el renombrado de colección sobre sus enlaces | IA |
| S6.5 | Implementar la consulta de enlaces con `ORDER BY rank` y la de colecciones con orden alfabético | IA |
| S6.6 | Implementar el comando manual de reconstrucción de proyecciones en una transacción | IA |
| S6.7 | Escribir tests: sincronización, atomicidad con fallo inyectado, aislamiento por usuario y por entidad, acentos y NFC | IA |
| S6.8 | Preparar un script local, no versionado, con datos de ejemplo para pruebas manuales | IA |
| S6.9 | Probar búsquedas con los datos de ejemplo y ejecutar el comando de reconstrucción | Humano |

**Definition of Done**

- [ ] Las proyecciones se mantienen sincronizadas en altas, cambios y borrados.
- [ ] Renombrar una colección actualiza su proyección y las filas de sus enlaces.
- [ ] Un fallo en una operación compuesta no deja proyecciones desincronizadas.
- [ ] Las búsquedas respetan siempre el usuario identificado y usan solo la proyección de su entidad.
- [ ] Acentos, NFC, operadores como términos literales y entrada sin términos se comportan según la especificación.
- [ ] El comando de reconstrucción restaura una proyección alterada a mano.
- [ ] El humano ha probado búsquedas con los datos de ejemplo.

### S7: Web base, seguridad y accesibilidad (L)

**Objetivo:** tener la zona privada segura y accesible, con identidad de desarrollo, para construir las pantallas sobre ella.

| ID | Tarea | Resp. |
|---|---|---|
| S7.1 | Configurar redirección a HTTPS, rechazo de `Host` ajeno y antiforgery ([«Seguridad web»](../context/architecture.md#seguridad-web)) | IA |
| S7.2 | Añadir las cabeceras `nosniff`, `frame-ancestors 'none'` y `Referrer-Policy` | IA |
| S7.3 | Resolver la identidad de desarrollo en `/app` e inyectarla en los casos de uso | IA |
| S7.4 | Crear el layout privado: idioma `es`, landmarks, enlace de salto, foco visible, contraste AA, `prefers-reduced-motion` y `noindex, nofollow` | IA |
| S7.5 | Crear componentes: resumen de errores accesible, avisos, estado vacío, paginación y formulario de búsqueda | IA |
| S7.6 | Implementar el análisis de `p`, `c`, `q` y `e` con sus reglas de ignorar y de error ([«Rutas privadas»](../context/specifications.md#rutas-privadas)) | IA |
| S7.7 | Implementar `/app` → dashboard, `404` genérico por método no permitido y el mensaje «No se ha podido acceder al elemento solicitado.» | IA |
| S7.8 | Configurar el registro de peticiones sin cadena de consulta de los listados privados | IA |
| S7.9 | Escribir tests de integración HTTP: HTTPS, cabeceras, `Host`, antiforgery, redirección, `404` y parámetros | IA |
| S7.10 | Revisar manualmente el dashboard con teclado, foco, contraste y estructura de encabezados | Humano |
| S7.11 | Validar el estilo base y pedir ajustes | Humano |

**Definition of Done**

- [ ] Los tests de HTTPS, cabeceras, `Host` y antiforgery pasan.
- [ ] El `UserId` solo procede de la identidad inyectada.
- [ ] El análisis de parámetros cubre todas las reglas de «Rutas privadas».
- [ ] El dashboard cumple «Páginas privadas», sin resultados, edición ni eliminación.
- [ ] Los logs no incluyen `q`.
- [ ] La revisión de teclado y de foco del layout está aceptada por el humano.
- [ ] El estilo base está aceptado por el humano.

### S8: Gestión de colecciones (L)

**Objetivo:** gestionar colecciones de principio a fin desde la interfaz privada.

| ID | Tarea | Resp. |
|---|---|---|
| S8.1 | Implementar el listado `/app/colecciones` con `q`, `p`, orden alfabético y estado vacío | IA |
| S8.2 | Implementar crear, editar y eliminar (con confirmación) y cambiar visibilidad por POST con estado destino explícito | IA |
| S8.3 | Mostrar errores de validación junto al campo, foco en el primer campo con error, y errores de reglas de dominio | IA |
| S8.4 | Aplicar la respuesta común para identificadores inválidos, inexistentes o ajenos (GET con enlace al listado, POST con redirección y aviso) | IA |
| S8.5 | Escribir tests HTTP derivados de los criterios aprobados en S4, de [«Rutas privadas»](../context/specifications.md#rutas-privadas) y de búsqueda de colecciones, con aislamiento entre usuarios | IA |
| S8.6 | Ejecutar la guía manual: crear, renombrar, buscar, paginar, eliminar y reglas de borrado | Humano |
| S8.7 | Revisar textos, mensajes y accesibilidad de los formularios | Humano |

**Definition of Done**

- [ ] Todas las operaciones de colecciones funcionan según [«Páginas privadas»](../context/specifications.md#páginas-privadas).
- [ ] Siempre queda al menos una colección y no se elimina una colección con enlaces.
- [ ] Hacer privada una colección vuelve privados sus enlaces en la misma transacción.
- [ ] La búsqueda de colecciones solo devuelve colecciones del usuario, por `Name`, en orden alfabético.
- [ ] Los tests de integración HTTP pasan.
- [ ] Los mensajes, formularios y estados vacíos están aceptados por el humano.

### S9: Gestión de enlaces (L)

**Objetivo:** crear, editar, mover, cambiar la visibilidad y eliminar enlaces, con rechazo de peticiones manipuladas.

| ID | Tarea | Resp. |
|---|---|---|
| S9.1 | Implementar crear enlace con casilla pública desmarcada, selector de colecciones sin paginar y duplicados con la colección donde existe | IA |
| S9.2 | Deshabilitar la opción pública en colecciones privadas con su explicación, y rechazar con el mensaje previsto cualquier petición manipulada | IA |
| S9.3 | Implementar editar (la URL no cambia), mover y eliminar con confirmación | IA |
| S9.4 | Implementar el POST de visibilidad con `IsPublic` único y válido, o redirección con aviso | IA |
| S9.5 | Escribir tests HTTP derivados de los criterios aprobados en S4: colección ajena, inexistente o inválida; peticiones manipuladas; propiedad; atomicidad | IA |
| S9.6 | Ejecutar la guía manual con URLs reales, duplicados y cambios de visibilidad | Humano |
| S9.7 | Revisar la presentación de imagen, título y descripción | Humano |

**Definition of Done**

- [ ] Las operaciones de enlaces cumplen [«Páginas privadas»](../context/specifications.md#páginas-privadas) y [«Errores controlados»](../context/specifications.md#errores-controlados).
- [ ] Un `CollectionId` manipulado se rechaza en la aplicación y por la clave foránea.
- [ ] Una petición manipulada no puede hacer público un enlace de una colección privada.
- [ ] Un enlace duplicado se rechaza indicando la colección donde está.
- [ ] La proyección FTS5 se mantiene sincronizada en cada operación.
- [ ] El humano ha aceptado las pantallas y los mensajes.

### S10: Listado y búsqueda de enlaces (L)

**Objetivo:** ofrecer el listado de enlaces con búsqueda por relevancia, filtro por colección y paginación.

| ID | Tarea | Resp. |
|---|---|---|
| S10.1 | Implementar `/app/enlaces` con `q`, `c` y `p`, orden por relevancia o por fecha, y estado vacío | IA |
| S10.2 | Aplicar la regla de `e` ignorado, filtro inexistente o ajeno ignorado y `q` por encima del máximo con error | IA |
| S10.3 | Conservar filtros al buscar y al paginar, y descartar `p` en el formulario | IA |
| S10.4 | Conectar el formulario de búsqueda del dashboard a `/app/enlaces` | IA |
| S10.5 | Escribir tests HTTP y de datos derivados de los criterios aprobados en S4: combinación de `q` y `c`, ranking, 15 por página, fuera de rango, aislamiento por entidad y por usuario | IA |
| S10.6 | Probar la búsqueda con los datos de ejemplo: acentos, términos, operadores literales, filtro de colección y paginación | Humano |

**Definition of Done**

- [ ] Todos los parámetros de [«Rutas privadas»](../context/specifications.md#rutas-privadas) tienen test.
- [ ] El texto de `q` no aparece en los logs.
- [ ] El orden con y sin `q` cumple [«Listados»](../context/specifications.md#listados).
- [ ] Un término que coincide con un enlace y una colección aparece solo en el listado de cada uno.
- [ ] Los enlaces de paginación y los formularios de búsqueda conservan los filtros aplicables.
- [ ] El humano ha probado la búsqueda con los datos de ejemplo.

### S11: Área pública y SEO (L)

**Objetivo:** publicar las cuatro páginas públicas, renderizadas en servidor y legibles para buscadores.

| ID | Tarea | Resp. |
|---|---|---|
| S11.1 | Implementar la landing con textos, `meta`, `h1`, canonical y botones de login (a `/app`) y de registro (`href="#"`) | IA |
| S11.2 | Implementar la página de usuario: últimos 5 enlaces, últimas 5 colecciones, textos de vacío, enlace a colecciones y `404` | IA |
| S11.3 | Implementar la página de colecciones y la de colección, con paginación `p` y canonical normalizada | IA |
| S11.4 | Aplicar `rel`, `target`, `alt` seguro, un solo `h1` e idioma `es` | IA |
| S11.5 | Redirigir variantes de URL (mayúsculas y barra final) y servir `404` para alias, slugs antiguos y colecciones sin enlaces públicos | IA |
| S11.6 | Escribir tests derivados de los criterios aprobados en S4: ausencia de contenido privado, orden, `404`, redirecciones, paginación y canonical | IA |
| S11.7 | Revisar el HTML renderizado: títulos, descripciones, canonical, encabezados, `alt` y textos finales | Humano |
| S11.8 | Probar la navegación pública con colecciones públicas y privadas, y con enlaces cambiados de visibilidad | Humano |

**Definition of Done**

- [ ] Los textos y metadatos de [«Páginas públicas»](../context/specifications.md#páginas-públicas) coinciden exactamente.
- [ ] Nada privado aparece en ninguna página pública.
- [ ] Las colecciones públicas sin enlaces públicos no aparecen.
- [ ] Cada página tiene un único `h1` y declara el idioma castellano.
- [ ] No quedan textos pendientes.
- [ ] El HTML se sirve renderizado en servidor.
- [ ] La revisión humana del HTML público está aceptada.

### S12: Cierre y hardening (M)

**Objetivo:** demostrar que el MVP0 cumple la [checklist de calidad](../context/roadmap.md#checklist-de-calidad-del-mvp0) y dejarlo cerrado.

| ID | Tarea | Resp. |
|---|---|---|
| S12.1 | Elaborar la matriz de trazabilidad checklist → test y cubrir los huecos | IA |
| S12.2 | Tests de registro de eventos y de ausencia de datos prohibidos en los logs | IA |
| S12.3 | Auditar las invariantes críticas de [AGENTS.md](../AGENTS.md) en código y SQL, y reportar | IA |
| S12.4 | Ejecutar la skill requirements-review y sincronizar la documentación | IA |
| S12.5 | Revisión de accesibilidad: teclado, foco, labels, anuncios, landmarks, contraste AA y `prefers-reduced-motion` | Humano |
| S12.6 | Pruebas manuales en navegadores actuales, públicas y privadas | Humano |
| S12.7 | Revisar los hallazgos de la auditoría y aceptar los riesgos que queden | Humano |
| S12.8 | Actualizar el [roadmap.md](../context/roadmap.md) moviendo el MVP0 a «Hecho», etiquetar la versión y hacer la retro final | Humano |

**Definition of Done**

- [ ] Cada punto de la checklist tiene un test o una revisión manual registrada.
- [ ] Los logs están disponibles por consola y por archivo, sin datos prohibidos.
- [ ] La reconstrucción de las proyecciones está probada.
- [ ] La auditoría de las invariantes críticas no deja hallazgos abiertos.
- [ ] La revisión de accesibilidad y las pruebas en navegadores están completadas y registradas.
- [ ] La documentación es coherente con el código.
- [ ] El roadmap refleja el cierre del MVP0.

## 5. Decisiones y alcance del plan

- Cadencia de 1 semana y humano a tiempo parcial.
- Pruebas con xUnit y `WebApplicationFactory`, sin automatización de navegador (según [roadmap.md](../context/roadmap.md)).
- Tailwind CLI standalone, sin Node.
- Diseño sobrio sin maquetas previas, con ajuste por sprint.
- Solo se crean las tablas del MVP0: `User`, `Collection` y `Link`.
- Los datos de ejemplo de S6.8 son una ayuda local no versionada y no forman parte del producto.
- Los criterios de aceptación de S4 son la puerta de entrada de la implementación y no se redefinen en sprints posteriores sin aprobación.
- Si un sprint se desborda, el recorte va al sprint siguiente y no se rebajan las invariantes críticas.
- S1 y S2 se mantienen secuenciales.
