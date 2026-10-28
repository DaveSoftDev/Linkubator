# Plan técnico: S11.4 — Atributos transversales de páginas públicas

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 27-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- Validar las cuatro respuestas públicas servidas por las vistas Razor existentes: [Index.cshtml](../../src/Linkubator.Web/Pages/Index.cshtml), [User.cshtml](../../src/Linkubator.Web/Pages/Public/User.cshtml), [Collections.cshtml](../../src/Linkubator.Web/Pages/Public/Collections.cshtml) y [Collection.cshtml](../../src/Linkubator.Web/Pages/Public/Collection.cshtml).
- Verificar el idioma del documento en el layout público compartido [_Layout.cshtml](../../src/Linkubator.Web/Pages/Shared/_Layout.cshtml); mantener el layout privado fuera del alcance.
- Conservar la salida de enlaces a `UrlOriginal` en las páginas de usuario y colección conforme a [spec.md](spec.md), y validar el texto alternativo de imágenes en esas mismas páginas. Mantener la codificación contextual de Razor para valores dinámicos.
- Extender las pruebas de integración HTTP existentes en [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), reutilizando su fábrica Web y sus fixtures SQLite. La inspección del código existente sugiere que parte de estos comportamientos ya está presente; su cumplimiento solo se dará por demostrado con las comprobaciones de esta spec.
- No se prevén nuevos contratos, servicios, componentes compartidos ni cambios de persistencia. Aplicar cambios en las vistas o el layout únicamente si las pruebas demuestran una discrepancia con los criterios aprobados.

### Flujos internos

1. Las pruebas solicitan por HTTP la landing, la página de usuario, la página de colecciones y la página de colección mediante la fábrica Web existente.
2. Se inspecciona el HTML recibido para comprobar los criterios de atributos, estructura de encabezados e idioma.
3. Si una comprobación falla, se corrige únicamente la vista o el layout responsable y se repiten las comprobaciones focalizadas; no se alteran rutas, reglas de contenido ni canonical.

### Persistencia y dependencias

- No se requieren cambios de esquema, migraciones, transacciones, paquetes ni configuración de despliegue.
- La validación integrada requiere disponer de las páginas de S11.1, S11.2 y S11.3. Aunque los archivos correspondientes existen, S11.2 sigue en curso según el [roadmap](../../context/roadmap.md#área-pública-y-seo); esa dependencia debe estar terminada antes de registrar la validación completa de las cuatro rutas.
- La fábrica, el almacenamiento SQLite temporal y los datos de ejemplo de `WebStartupTests` son los prerrequisitos de las comprobaciones HTTP.

### Garantías técnicas

- Los criterios se comprueban sobre las respuestas HTTP renderizadas, no solo mediante inspección estática de las vistas.
- Los datos dinámicos de los atributos conservan la codificación contextual de Razor y no se convierten en marcado interpretado.
- Los cambios se limitan a las páginas públicas contempladas por [spec.md](spec.md); se preservan el contenido público/privado, las rutas, los metadatos y las canonical ya establecidos por S11.1–S11.3.
- Estas pruebas no demuestran el comportamiento de tecnologías de asistencia ni la indexación efectiva por motores de búsqueda; la revisión manual general del HTML se mantiene en S11.7.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar las vistas y el layout público existentes, sin añadir abstracciones de presentación. | Propuesta | [spec.md](spec.md), estructura actual de Razor Pages | Reduce el alcance del cambio; modificar solo los archivos que fallen las comprobaciones. |
| Reutilizar y ampliar `WebStartupTests` para validar las respuestas HTML reales. | Propuesta | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) | Evita crear infraestructura de pruebas paralela y permite comprobar las cuatro rutas mediante HTTP. |
| La validación completa requiere que S11.2 haya terminado. | Confirmado | [roadmap.md → «Área pública y SEO»](../../context/roadmap.md#área-pública-y-seo), [spec.md](spec.md) | No registrar evidencia completa de CA01–CA04 antes de probar la ruta pública de usuario integrada. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración HTTP de las páginas públicas de usuario y colección | Fábrica Web y datos de prueba con enlaces públicos | Extender los tests HTTP existentes de usuario y colección y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage_RendersPublicContentMetadataAndConfiguredCanonical|FullyQualifiedName~WebStartupTests.PublicCollectionPage_RendersPublicLinksMetadataAndCanonical"` | Las respuestas incluyen los atributos establecidos por sus fuentes en los enlaces a `UrlOriginal`. | Resultado de los tests focalizados y aserciones sobre el HTML recibido. |
| V02 | CA02 | Automatizada, integración HTTP de imágenes en las páginas de usuario y colección | Fábrica Web y fixture con imágenes asociadas a enlaces con título informado y no informado | Ampliar los casos HTTP existentes; inspeccionar la respuesta HTML sin decodificar previamente sus entidades y ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.PublicPage_WithImages_RendersAltAsText"` | El `alt` coincide con el texto fuente aplicable en ambos casos y un título con caracteres de marcado se conserva como valor de atributo, no como elementos HTML. | Datos del fixture, resultado del test focalizado y aserciones sobre el HTML original recibido. |
| V03 | CA03 | Automatizada, integración HTTP de las cuatro páginas públicas | Landing y rutas de S11.1–S11.3 disponibles en la fábrica Web | Ampliar los tests de landing, usuario, colecciones y colección; contar los elementos `h1` del HTML de cada respuesta. Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Cada una de las cuatro respuestas de página contiene exactamente un `h1`. | Resultado de la suite focalizada de `WebStartupTests` y conteo de encabezados por ruta. |
| V04 | CA04 | Automatizada, integración HTTP de las cuatro páginas públicas | Landing y rutas de S11.1–S11.3 disponibles en la fábrica Web | Ampliar los tests de landing, usuario, colecciones y colección para inspeccionar el elemento raíz de cada respuesta; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | El elemento `<html>` de cada respuesta declara el idioma definido por la fuente propietaria. | Resultado de la suite focalizada y aserciones del elemento raíz por ruta. |

Las pruebas HTTP verifican el HTML emitido por la aplicación; no sustituyen la revisión manual de S11.7 ni prueban el comportamiento de un navegador o de tecnologías de asistencia.

## Orden de ejecución y cierre

- Las [tareas](tasks.md) se crearon como propuesta tras la aprobación de este plan; su aprobación explícita sigue pendiente.
- Las tareas propias cubrirán la comprobación de las cuatro rutas y las correcciones que resulten necesarias. No dependerán de S11.5–S11.7 para demostrar CA01–CA04.
- El cierre requerirá evidencia satisfactoria de CA01–CA04 sobre las cuatro respuestas públicas, tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Si una comprobación revela una contradicción en una regla funcional, detener el cambio afectado y solicitar su resolución en el documento propietario antes de continuar.
