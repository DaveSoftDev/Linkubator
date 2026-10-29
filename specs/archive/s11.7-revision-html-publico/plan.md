# Plan técnico: S11.7 — Revisión del HTML público

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 27-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- Una persona revisora inspeccionará en navegador el HTML renderizado de la landing, la página pública de usuario, la página de colecciones y la página de colección. Contrastará cada observación con los criterios CA01–CA04 de [spec.md](spec.md) y sus fuentes propietarias.
- La inspección abarcará el `head` del documento y el DOM renderizado para comprobar metadatos, canonical, idioma, encabezados, `alt` y textos de interfaz. No se modificarán código, reglas funcionales ni datos fuera del entorno y los datos de prueba expresamente preparados para la revisión.
- La evidencia se registrará en las tareas una vez aprobadas. Por cada criterio incluirá entorno, navegador y versión, página y estado revisados, resultado esperado y observado, fecha y hallazgos reproducibles. No se registrarán credenciales, tokens ni otros secretos.
- Un hallazgo no autoriza a cambiar una regla ni a corregir producción dentro de esta revisión. Se remitirá al trabajo propietario; si afecta a un criterio, este no se dará por satisfactorio hasta que el cambio autorizado esté disponible y se repita la comprobación.

### Flujos internos

1. Confirmar que el entorno es local o de pruebas y que los datos disponibles son seguros para la revisión; registrar el navegador y su versión.
2. Identificar datos públicos que permitan renderizar las cuatro páginas, imágenes de enlaces y el recurso decorativo, así como los estados vacíos incluidos en las fuentes. No usar datos de producción ni inferir cobertura de un escenario que no pueda observarse.
3. En cada página, inspeccionar título, descripción, canonical, idioma y jerarquía de encabezados; comparar los valores observados con la fuente propietaria. En la página de colección, incluir la canonical de la primera página y de una página interior cuando los datos preparados permitan observarla.
4. Inspeccionar las imágenes de enlaces y el recurso decorativo en las páginas donde aparezcan; contrastar sus atributos `alt` con CA03.
5. Leer los textos de interfaz y los estados vacíos pertinentes en las cuatro páginas; contrastarlos con las fuentes y comprobar que no haya texto provisional o pendiente.
6. Registrar resultados y hallazgos por CA01–CA04. Escalar cualquier discrepancia al trabajo propietario sin modificar comportamiento ni reglas durante la revisión.

### Persistencia y dependencias

- No hay cambios de persistencia, consultas, transacciones, infraestructura ni dependencias de software previstos.
- La revisión depende de que estén disponibles y estables las páginas y los atributos transversales de S11.1–S11.5, además de la validación HTTP de S11.6, según [roadmap.md → «Área pública y SEO»](../../../context/roadmap.md#área-pública-y-seo). La [spec](spec.md#dependencias-y-prerrequisitos) recoge el estado comprobado; su preparación no supone que esos trabajos ya estén cerrados.
- Para comprobar todos los elementos de CA01–CA04, el entorno debe permitir observar las canonical de las páginas, imágenes con sus casos de texto alternativo y los estados vacíos aplicables. Si falta un escenario, se prepararán datos de prueba no sensibles en el entorno autorizado; no se sustituirá la observación por una suposición.
- S11.8 es una revisión separada de navegación pública: no aporta evidencia a estos criterios ni es prerrequisito para cerrar S11.7.

### Garantías técnicas

- Cada conclusión se limita a los documentos y estados inspeccionados; la revisión no demuestra indexación efectiva por buscadores, calidad general del diseño ni comportamiento de navegación o respuestas HTTP cubierto por otros trabajos.
- La canonical se contrasta con su fuente propietaria y con el origen público configurado; no se considera suficiente que el HTML contenga cualquier URL canonical.
- Los datos preparados permiten inspeccionar los atributos y textos observados sin convertir ejemplos de prueba en nuevas reglas de producto.
- Una discrepancia o un estado no comprobado se registra explícitamente. No se declara satisfecho un criterio sin su evidencia manual correspondiente.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar inspección manual del HTML renderizado en un entorno local o de pruebas, sin automatizar una prueba de navegador. | Propuesta | [spec.md → «Alcance»](spec.md#alcance) y [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | Mantiene la revisión dentro del trabajo humano previsto; confirmar el entorno seguro y el navegador al iniciar las tareas. |
| La revisión de estados vacíos y atributos `alt` necesita datos que los hagan observables. | Confirmado | [spec.md → CA03–CA04](spec.md#criterios-de-aceptación) | Si no se dispone de esos escenarios, el criterio afectado queda sin evidencia; preparar datos locales adecuados antes de concluir. |
| Una diferencia respecto de una fuente no se corrige como parte de esta revisión. | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | Se comunica al trabajo propietario; si afecta a un criterio, se repite su validación tras la corrección autorizada. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual: metadatos y canonical de las cuatro páginas públicas; primera página y página interior de colección cuando estén disponibles | Páginas listas, origen público configurado y datos suficientes para observar una página interior si aplica | Inspeccionar el `title`, el `meta description` y la canonical del HTML renderizado; contrastar cada valor con las fuentes de CA01 | Títulos y descripciones coinciden; cada canonical corresponde a la ruta y origen de la fuente, incluida la normalización de paginación aplicable | Fecha, entorno, navegador/versión, página y estado revisados, valores esperados y observados, resultado y hallazgos |
| V02 | CA02 | Manual: idioma y jerarquía de encabezados en cada página | Las cuatro páginas renderizadas | Inspeccionar el atributo `lang`, contar los `h1` y contrastar el texto y la función de los encabezados con las fuentes de CA02 | Cada página declara `lang="es"`, contiene un solo `h1` y sus encabezados coinciden con la fuente propietaria | Fecha, entorno, navegador/versión, página, observaciones de idioma y encabezados, resultado y hallazgos |
| V03 | CA03 | Manual: imágenes de enlaces y recurso decorativo | Datos que rendericen enlaces con imagen, casos de título alternativo previsto y tarjeta sin imagen | Inspeccionar los atributos `alt` en el DOM renderizado y compararlos con la fuente de CA03 | El texto alternativo y la ausencia de texto en el recurso decorativo cumplen la fuente propietaria | Fecha, entorno, navegador/versión, página, imagen o recurso observado, valor esperado/observado y resultado |
| V04 | CA04 | Manual: textos de interfaz y estados vacíos pertinentes en las cuatro páginas | Datos que permitan observar los estados aplicables y las páginas renderizadas | Leer los textos de interfaz renderizados y contrastarlos con requirements.md y las secciones de página de specifications.md | Los textos coinciden con las fuentes y no queda texto provisional o pendiente | Fecha, entorno, navegador/versión, página/estado, referencias fuente, resultado observado y hallazgos |

La validación es manual: no se atribuirá a una compilación ni a las pruebas automatizadas la comprobación de textos, metadatos, encabezados o `alt` realizada por la persona revisora.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan. Las tareas desglosarán la preparación del entorno y datos, las revisiones V01–V04, la consolidación de evidencia y la aceptación humana, sin adelantarlas ahora.
- La evidencia de cada criterio debe ser satisfactoria y no puede depender de que S11.8 aporte código, pruebas o resultados. Si se detecta una discrepancia o falta un escenario, registrar el bloqueo concreto y su responsable antes de declarar un resultado.
- El cierre requiere todos los criterios con evidencia manual satisfactoria y aceptación humana registrada en las tareas. Si una corrección cambia una regla funcional, detener el criterio y obtener aprobación en su documento propietario antes de continuar.
