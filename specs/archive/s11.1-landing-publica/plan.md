# Plan técnico: S11.1 — Landing pública

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 27-X-2026 («Siguiente»).

## Artefactos relacionados

- [Tareas](tasks.md) — implementación y validación completadas; evidencia registrada en [tasks.md](tasks.md).

## Diseño de implementación

### Componentes y contratos

- Sustituir el contenido provisional de `Pages/Index.cshtml` por la landing dentro de la ruta Razor existente. Mantener el comportamiento público de `GET /`; no añadir caso de uso ni introducir dependencias de Application o Infrastructure.
- Usar el `IndexModel` existente únicamente para proporcionar los metadatos de esta página que necesite la vista.
- Adaptar `Pages/Shared/_Layout.cshtml` para renderizar metadatos específicos cuando una página los proporcione y conservar la salida actual de las páginas que no los establezcan. La landing proporciona su `meta title`, `meta description` y canonical conforme a [spec.md](spec.md).
- Construir la canonical con el origen público configurado, sin usar `Request.Host`; seguir [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento).
- Reutilizar el layout, las clases y los recursos existentes. El cambio no altera las páginas de gestión de colecciones ni introduce estilos compartidos nuevos.
- La revisión visual solicitada por DLG el 27-X-2026 incorpora una composición corporativa en `Index.cshtml`. Los estilos específicos se cargan desde `wwwroot/css/landing.css` mediante la sección opcional `Styles` del layout y reutilizan las variables del tema privado. `ViewData["MainClass"]` permite ampliar solo esta portada; las demás páginas conservan el contenedor predeterminado. Las marcas, los testimonios y las cifras se identifican como contenido de ejemplo; no se consultan datos privados ni se incorporan recursos externos.
- La vista previa usa [dashboard-preview.webp](../../../src/Linkubator.Web/wwwroot/brand/dashboard-preview.webp), captura estática del dashboard real con una base aislada de colecciones y enlaces ficticios. El perfil se presenta como usuario de ejemplo, sin email. Se captura en Edge sin cabeceras del navegador, con viewport de 1440 × 1000, y se codifica como WebP con Sharp. La imagen conserva su proporción, tiene texto alternativo y permite abrir el recurso a tamaño completo; no incrusta una sesión privada. Al cambiar el diseño del dashboard, renovar la captura con datos ficticios y comprobar que no aparecen datos personales.
- La cabecera reutiliza `_PublicAuthLinks.cshtml` y `public-actions.css` con privacidad, las páginas públicas de usuario y colecciones y la página de error. `.landing-nav__auth` mantiene las acciones juntas cuando la navegación se distribuye en varias líneas. El registro conserva su destino provisional y un nombre accesible que indica su disponibilidad; las etiquetas y el tooltip se definen en [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas). La ampliación de la rejilla reutiliza `.landing-feature` y presenta búsqueda, edición del contenido del enlace y acceso a los enlaces recientes, sin introducir operaciones nuevas.

### Identidad visual compartida

Las cabeceras públicas comparten la geometría de `.public-header` en `public-actions.css`, además de los botones: altura, separación y colocación responsive. La navegación propia de la landing ocupa una fila independiente en pantallas estrechas sin desplazar las acciones de acceso respecto a las otras páginas.

La hoja compartida pública reserva el espacio de la barra de desplazamiento con `scrollbar-gutter: stable` en el elemento raíz. Así los contenedores conservan su ancho al navegar entre páginas con distinta altura de contenido; la hoja no se carga en el layout privado. `footer.css` aplica los mismos colores de reposo y hover a los enlaces de texto del pie, incluido el alias, sin afectar a la marca.

La identidad aprobada se registra en [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance). La integración reutiliza las dimensiones de los contenedores existentes y no cambia la navegación ni el comportamiento de contracción del menú lateral.

El acabado del área privada se concentra en [private.css](../../../src/Linkubator.Web/wwwroot/css/private.css), cargado solo por [_PrivateLayout.cshtml](../../../src/Linkubator.Web/Pages/Shared/_PrivateLayout.cshtml) después de los estilos base. Sus selectores se limitan a `.private-shell`: ajustan superficies, tipografía y estados de interacción sin cambiar la estructura responsive ni el CSS generado. Las tarjetas reutilizan el título enlazado del cuerpo tanto en rejilla como en lista; el título superpuesto queda oculto y el símbolo sin imagen se centra. Los formularios conservan sus estados de validación y las acciones destructivas mantienen su tratamiento diferenciado.

La prueba de ampliación de objetivos de interacción se limita a esa misma hoja: acciones de tarjetas y filas, selector de vista, controles de la barra de herramientas y controles de contracción y creación de colección del menú lateral. Crece el contenedor pulsable, no el icono. La metainformación de las tarjetas permite cambiar de línea y el pie reserva espacio para la acción de apertura. Este ajuste aislado no acredita conformidad completa con WCAG AAA.

El listado de colecciones usa [collection-list.css](../../../src/Linkubator.Web/wwwroot/css/collection-list.css) para asignar áreas de Grid independientes al contador y al estado de privacidad; este último se alinea al extremo derecho bajo las herramientas. El botón de creación reutiliza el SVG decorativo de suma de las acciones existentes.

El parcial privado `_LinkCard.cshtml` representa ambas ramas de `LinkCardViewModel.IsPublic`, independientemente del estado de la colección. `private.css` coloca ambos distintivos en la misma celda de la vista Lista y conserva la composición de Tarjetas, conforme a [specifications.md → «Páginas privadas»](../../../context/specifications.md#páginas-privadas). Las vistas públicas no usan este parcial.

| Recurso | Implementación |
| --- | --- |
| Símbolo maestro | [symbol.svg](../../../src/Linkubator.Web/wwwroot/brand/symbol.svg), SVG autónomo que conserva la geometría elegida. |
| Logotipos reutilizables | [brand/](../../../src/Linkubator.Web/wwwroot/brand): versiones SVG y PNG transparentes para fondos claros y oscuros y alternativas monocromas. Las letras son trazados derivados de Segoe UI; no requieren fuentes instaladas ni externas. El trazado maestro está en [wordmark.svg](../../../tools/brand-assets/wordmark.svg). |
| Presentación Web | [_BrandMark.cshtml](../../../src/Linkubator.Web/Pages/Shared/_BrandMark.cshtml) y [brand.css](../../../src/Linkubator.Web/wwwroot/css/brand.css) comparten el símbolo. El nombre sigue siendo texto HTML y el icono es decorativo. |
| Iconos del navegador | [_Favicons.cshtml](../../../src/Linkubator.Web/Pages/Shared/_Favicons.cshtml) comparte SVG, PNG, ICO multirresolución e icono Apple entre ambos layouts. Los PNG adicionales de aplicación son recursos gráficos, sin manifiesto ni comportamiento PWA. |

La exportación está aislada en [tools/brand-assets/](../../../tools/brand-assets) y usa Sharp como herramienta de desarrollo, no como dependencia de ejecución de la Web. Desde esa carpeta:

```powershell
npm ci --ignore-scripts
npm run build
npm run check
```

El generador deriva los recursos del símbolo y del trazado maestros. La comprobación verifica las dimensiones de los PNG, el fondo opaco del icono Apple, cada imagen del ICO y la ausencia de fuentes e imágenes externas en los logotipos.

### Flujos internos

1. Una petición anónima `GET /` se resuelve en la Razor Page raíz.
2. `IndexModel` prepara los metadatos de la página a partir de la configuración aplicable.
3. Razor renderiza la landing y el layout compartido compone el documento HTML con esos metadatos.
4. Los botones se presentan como enlaces con los destinos definidos por [spec.md](spec.md); el enlace de registro permanece inerte y no requiere endpoint ni handler.

### Persistencia y dependencias

- No hay cambios de persistencia, transacciones ni contratos de Application.
- La configuración existente incluye `PublicOrigin` en `appsettings.Development.json`, pero no en `appsettings.json`. Las pruebas deben fijar un origen controlado en su configuración; el despliegue y la configuración de producción siguen fuera del alcance.
- No se requieren paquetes externos de ejecución ni cambios en la matriz de dependencias de proyectos. La herramienta de exportación gráfica se describe en [«Identidad visual compartida»](#identidad-visual-compartida).

### Garantías técnicas

- El contenido y los metadatos se sirven en el HTML inicial mediante Razor, sin depender de JavaScript para que la página sea legible.
- La canonical permanece ligada al origen configurado incluso si la petición llega por otro host permitido; no se confía en la cabecera `Host`.
- El ajuste del layout conserva los valores de metadatos actuales para las páginas que no configuren datos específicos, evitando cambiar sus respuestas como efecto lateral.
- La vista mantiene la codificación contextual de Razor para cualquier valor que se represente en HTML, conforme a [architecture.md → «Web»](../../../context/architecture.md#web).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar la Razor Page raíz y el layout compartido para los metadatos específicos de la landing. | Propuesta | [spec.md](spec.md), [architecture.md → «Web»](../../../context/architecture.md#web) | Mantiene el flujo Razor existente; el plan propone preservar los metadatos actuales como valor predeterminado para las demás páginas. |
| El origen canonical depende de `PublicOrigin`, que actualmente solo aparece en la configuración de desarrollo. | Riesgo conocido | [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento) | Las pruebas fijarán el origen en su configuración. La configuración de producción no forma parte de este trabajo; el entorno deberá aportar su origen antes de publicar la aplicación. |
| No añadir una ruta ni lógica de registro. | Confirmado | [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance) | La acción de registro es una referencia inerte en esta etapa; no se creará un flujo ficticio de registro. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada, integración HTTP de la Razor Page raíz | `PublicOrigin` definido en la fábrica de pruebas | Añadir `LandingIntegrationTests` siguiendo el patrón existente de `WebApplicationFactory`; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~LandingIntegrationTests` | La respuesta anónima de `GET /` devuelve la página y el HTML recibido contiene el contenido, `h1` y metadatos requeridos, sin ejecución de JavaScript. | Resultado del test focalizado y comprobación de compilación del proyecto de tests. |
| V02 | CA03 | Automatizada, integración HTTP de la canonical | Fábrica con origen público configurado y un host alternativo permitido | En `LandingIntegrationTests`, solicitar `/` con un `Host` permitido distinto del origen configurado y examinar la canonical | La canonical apunta al origen configurado y no cambia al variar el host de la petición. | Resultado del caso de integración con el host alternativo. |
| V03 | CA04 | Automatizada, comprobación HTTP de los destinos renderizados | Aplicación de pruebas disponible | En `LandingIntegrationTests`, comprobar el destino del enlace de login y el `href` del enlace de registro en el HTML | Login apunta a `/app`; registro mantiene `href="#"` y no se presenta un formulario de registro ni una ruta de alta. | Resultado del test focalizado. |
| V04 | CA04 | Manual, interacción del enlace de registro | Aplicación local disponible | Abrir la landing en un navegador y activar el enlace de registro; observar URL y solicitudes de red | La navegación no abandona la landing ni inicia una solicitud de registro. | Navegador y versión, resultado observado y cualquier incidencia. |
| V05 | CA01–CA03 | Revisión técnica de regresión | Cambio del layout compartido implementado | Revisar el HTML de páginas existentes que usan los metadatos predeterminados y ejecutar la suite focalizada anterior | Las páginas sin metadatos específicos conservan el título y la descripción previos; no aparece canonical derivada de la petición en ellas. | Referencia a la revisión y resultado de la suite ejecutada. |

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan.
- Las tareas propias cubrirán la landing, los metadatos del layout, las comprobaciones automatizadas y la revisión manual de CA04. Ningún criterio dependerá de páginas públicas de etapas vecinas ni de despliegue.
- El cierre requerirá evidencia satisfactoria para CA01–CA04, tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Si la implementación revela que falta una regla o que la canonical no puede generarse con la configuración prevista, detener la tarea afectada y resolverlo en el documento propietario antes de continuar.
