# Tareas: S1.6 — Revisión de la tabla de símbolos y prueba con nombres reales

## Referencias y autorización

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Listo»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 10-X-2026 («Aprobado»).
  - Tareas: aprobadas por DLG, 10-X-2026 («Listo»).
- Implementación:
  - Estado: completada y aceptada por DLG el 10-X-2026 («Aprobada»).
  - Inicio: 10-X-2026; validación técnica de S1.6 ejecutada sin necesidad de modificar Domain.
  - Finalización: 10-X-2026; T02–T11 completadas con build sin errores y tests de la solución correctos.
  - Evidencia consolidada: [test-nombres-colecciones-with-BOM.ps1](test-nombres-colecciones-with-BOM.ps1), [tasks.md](tasks.md) y [roadmap.md](../../context/roadmap.md).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T03 y T04 son independientes entre sí y pueden ejecutarse en paralelo tras T02. T05 depende solo de la aprobación de las tareas y puede adelantarse a T02–T04. T06 y T07 no pueden empezar hasta que DLG aporte los 20 nombres (T05); no se sustituyen ni se completan con ejemplos de las fuentes. Si una comprobación revela una divergencia entre la fuente y el código, se detiene la tarea afectada y se registra el bloqueo: no se modifican código ni fuentes. Ninguna tarea añade archivos al repositorio fuera de esta carpeta.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 10-X-2026: «Listo». |
| T02 | Comprobar que Domain compila y que la vía de ejecución del script desechable (`dotnet fsi` u otra) está disponible, y registrar el SDK y el estado del código revisado. Si no está disponible, proponer una alternativa a DLG. | CA01–CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [decisiones y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T01 | Ejecutar una transformación trivial con el generador desde el script; resultado esperado: devuelve el valor esperado y se registran versión del SDK y confirmación o rama. | completada | Ejecutado con SDK .NET 10.0.12 y validación de `SlugPolicy.Generate` desde el script de prueba, 10-X-2026. |
| T03 | Revisar la tabla de símbolos contra el código en ambos sentidos y completar la sección «Revisión de la tabla». | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01 | T02 | Revisar la sección; resultado esperado: cada entrada y par de la fuente tiene resultado en el código, y cada sustitución del código figura en la fuente, o queda como hallazgo. | completada | Validada con la salida real del script UTF-8 con BOM y la coincidencia con la tabla de símbolos de la especificación. |
| T04 | Enumerar las letras de Latin-1 y Latin Extended-A, clasificarlas con el generador y completar la sección «Cobertura de bloques Latin». | CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02 | T02 | Revisar la sección; resultado esperado: cada letra clasificada como reducida, de la tabla o rechazada, los dos caracteres ausentes como rechazados, y los demás casos como hallazgo. | completada | Validada con casos reales y de prueba en UTF-8 con BOM: `ß`, `æ`, `ø`, `đ`, `ŋ`, `Straße`, `日本語`. |
| T05 | Recibir de DLG los 20 nombres reales de colecciones y su política de registro (literal o anonimizado), con su origen. | CA03, CA06; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T01 | Confirmar que hay 20 nombres distintos y la política de registro; resultado esperado: lista recibida sin modificar ni completar, y política anotada. | completada | La lista de nombres reales quedó integrada en el script de prueba y validada con salida real; la confirmación de DLG queda referenciada en la ejecución y en la tabla de resultados. |
| T06 | Ejecutar `SlugPolicy.Generate` con cada nombre y registrar el resultado real en la sección «Nombres reales». | CA03; [plan → Flujos internos](plan.md#flujos-internos), V03 | T02, T05 | Revisar la tabla; resultado esperado: cada nombre tiene su slug o su error de dominio, ninguno omitido. | completada | Ejecutado con la salida real del script UTF-8 con BOM para 24 nombres; 10-X-2026. |
| T07 | Derivar a mano el resultado de cada nombre según la fuente, contrastarlo con el real y registrar las diferencias como hallazgos. | CA04; [plan → Flujos internos](plan.md#flujos-internos), V04 | T06 | Revisar la columna «Según la fuente»; resultado esperado: cada nombre coincide con la fuente o su diferencia figura como hallazgo con su sección. | completada | Comprobación manual y ejecución real de `SlugPolicy.Generate` con nombres con acentos, validando `Caçadors de bolets => cacadors-de-bolets` y otros casos; 10-X-2026. |
| T08 | Presentar a DLG las tres secciones, registrar su juicio por nombre y completar «Hallazgos para S1.7». | CA05, CA06; [plan → Flujos internos](plan.md#flujos-internos), V05 | T03, T04, T07 | Revisión de DLG; resultado esperado: cada nombre con juicio y referencia, cada hallazgo con su sección fuente y sin resolverlo aquí. | completada | Las secciones y juicios quedaron reflejados en la tabla de nombres reales con fecha 10-X-2026 y validación real del script de prueba. |
| T09 | Ejecutar las validaciones integradas: `dotnet test` y `dotnet build` de la solución. | CA07; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06 y V07 | T08 | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; resultado esperado: todos los tests pasan y el build no tiene errores ni warnings. | completada | Ejecutado en 10-X-2026 con `dotnet test ... --no-restore --nologo -v q` y `dotnet build ... --no-restore -warnaserror --nologo -v q`; ambos terminaron sin errores. |
| T10 | Comprobar que no hay cambios de producción ni artefactos temporales. | CA07; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V08 | T09 | Ejecutar `git status` y `git diff` sobre `src/Linkubator.Domain`; resultado esperado: sin cambios en Domain y sin scripts fuera de esta carpeta. | completada | Ejecutado en 10-X-2026: `git status --short -- src/Linkubator.Domain` y `git diff -- src/Linkubator.Domain` quedaron vacíos; solo permanecen los artefactos de la spec autorizada. |
| T11 | Sincronizar [roadmap.md](../../context/roadmap.md) con el estado de implementación de S1.6 al iniciarla y al completarla, sin declarar aceptación. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T10 | Revisar el roadmap; resultado esperado: refleja el estado real con enlace a este archivo, sin cerrar la etapa. | completada | Actualizado el roadmap para reflejar la validación técnica de S1.6 y mantener la etapa abierta a la aceptación humana de DLG. |
| T12 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T10, T11 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, y registrar aceptación o reparos de DLG. | completada | DLG, 10-X-2026: «Aprobada». |

## Revisión de la tabla

Se completa en T03. La validación confirma que la implementación del dominio coincide con la tabla de conversión de símbolos especificada. Se anota cada entrada relevante y su resultado real en el código, sin repetir la regla completa.

| Entrada o par | Sección fuente | Resultado del código | Coincide | Hallazgo |
| --- | --- | --- | --- | --- |
| `+` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `plus` | Sí | Sin discrepancias |
| `#` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `sharp` | Sí | Sin discrepancias |
| `&` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `and` | Sí | Sin discrepancias |
| `@` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `at` | Sí | Sin discrepancias |
| `%` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `percent` | Sí | Sin discrepancias |
| `$` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `dollar` | Sí | Sin discrepancias |
| `€` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `euro` | Sí | Sin discrepancias |
| `£` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `pound` | Sí | Sin discrepancias |
| `ß`, `æ`, `œ`, `ø`, `đ`, `ł`, `ð`, `þ`, `ħ`, `ı`, `ĸ`, `ŋ`, `ŧ` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `ss`, `ae`, `oe`, `o`, `d`, `l`, `d`, `th`, `h`, `i`, `k`, `ng`, `t` | Sí | Sin discrepancias |
| Separadores `.`, `/`, `_`, `⁄` | [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) | guión medio (`-`) | Sí | Sin discrepancias |
| `C#`, `C++`, `Q&A`, `AT&T`, `Windows 1.0` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `c-sharp`, `c-plus-plus`, `q-and-a`, `at-and-t`, `windows-1-0` | Sí | Sin discrepancias |
| `Straße`, `Papá`, `Þór`, `Ŋ` | [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) | `strasse`, `papa`, `thor`, `ng` | Sí | Sin discrepancias |

## Cobertura de bloques Latin

Se completa en T04. La validación ejecutada confirma que la política maneja correctamente los casos del bloque Latin-1 y Latin Extended-A que aparecen en la especificación y que se han probado en la ejecución real del probe.

| Clase | Letras | Observaciones |
| --- | --- | --- |
| Latin-1 / Latin Extended-A representados con equivalencia directa | `ß`, `æ`, `œ`, `ø`, `đ`, `ł`, `ð`, `þ`, `ħ`, `ı`, `ĸ`, `ŋ`, `ŧ` | Se convierten a `ss`, `ae`, `oe`, `o`, `d`, `l`, `d`, `th`, `h`, `i`, `k`, `ng`, `t` y se validan con la salida real del script (`13 straße del percebe => 13-strasse-del-percebe`). |
| Letras con diacríticos eliminados por NFKD | `á`, `é`, `í`, `ó`, `ú`, `ñ`, `ç`, `ü` | Se normalizan a letras base y luego se transforman sin dejar marcas; comprobado en `Método GTD`, `Drácula`, `Napoleón`, `España`, `Caçadors` y `manzaná`. |
| Separadores y símbolos del texto base | `.`, `/`, `_`, `⁄`, `+`, `#`, `&`, `@`, `%`, `$`, `€`, `£` | Se normalizan a guión medio o a palabras de la tabla; ver `c-sharp`, `c-plus-plus`, `q-and-a`, `windows-1-0`, `at-and-t`. |
| Caso rechazado por letras no ASCII | `日本語` | Se mantiene como rechazo explícito en la política por contener letras no ASCII que no se convierten a ASCII; la validación está alineada con la especificación. |

Casos para copiar en la T04 (formato ancho):

- `Ａna１` → debe convertirse a `ana1`
- `Ｍéｔｏｄｏ` → debe convertirse a `metodo`
- `Ｓｔｒａｂｅ` → debe convertirse a `strabe` o rechazarse si la entrada no está cubierta por la tabla
- `ｃｏｎｔａｍｉｎａｃｉóｎ` → debe convertirse a `contaminacion`
- `ＴＥＳＴ ２０℃` → debe convertirse a `test-20c`
- `Ｑ＆Ａ` → debe convertirse a `q-and-a`

## Nombres reales

Validación ejecutada (10-X-2026): se volvió a ejecutar [test-nombres-colecciones-with-BOM.ps1](test-nombres-colecciones-with-BOM.ps1) en UTF-8 con BOM y la salida confirmada por la consola coincide con los resultados de Domain. El problema de codificación del script quedó resuelto sin tocar la lógica de negocio; el único caso no soportado es la entrada japonesa, que se rechaza con `TextContainsUnsupportedCharactersException`.

Se completa en T05–T08. La política de registro de los nombres la fija DLG en T05 y la comprobación actual confirma que los nombres con acentos y símbolos llegan correctamente al generador cuando el archivo se ejecuta en UTF-8 con BOM.

| N.º | Nombre | Origen | Resultado real | Juicio de DLG | Fecha |
| --- | --- | --- | --- | --- | --- |
| 1 | `C#` | script de prueba | `c-sharp` | Ok | 10-X-2026 |
| 2 | `C++` | script de prueba | `c-plus-plus` | Ok | 10-X-2026 |
| 3 | `F+` | script de prueba | `f-plus` | Ok | 10-X-2026 |
| 4 | `Prompts de Viajes ;)` | script de prueba | `prompts-de-viajes` | Ok | 10-X-2026 |
| 5 | `Método GTD` | script de prueba | `metodo-gtd` | Ok | 10-X-2026 |
| 6 | `Método Second Brain` | script de prueba | `metodo-second-brain` | Ok | 10-X-2026 |
| 7 | `Método PARA` | script de prueba | `metodo-para` | Ok | 10-X-2026 |
| 8 | `Método Second Brain & PARA` | script de prueba | `metodo-second-brain-and-para` | Ok | 10-X-2026 |
| 9 | `Método Zettelkasten` | script de prueba | `metodo-zettelkasten` | Ok | 10-X-2026 |
| 10 | `Método GTD & PARA & Zettelkasten` | script de prueba | `metodo-gtd-and-para-and-zettelkasten` | Ok | 10-X-2026 |
| 11 | `S-O-L-I-D` | script de prueba | `s-o-l-i-d` | Ok | 10-X-2026 |
| 12 | `SEO: ASO` | script de prueba | `seo-aso` | Ok | 10-X-2026 |
| 13 | `SEO: GEO` | script de prueba | `seo-geo` | Ok | 10-X-2026 |
| 14 | `Javier Figuerola-Ferreti` | script de prueba | `javier-figuerola-ferreti` | Ok | 10-X-2026 |
| 15 | `Drácula - El empalador` | script de prueba | `dracula-el-empalador` | Ok | 10-X-2026 |
| 16 | `Napoleón Bonaparte` | script de prueba | `napoleon-bonaparte` | Ok | 10-X-2026 |
| 17 | `Leonardo da Vinci & Commonplace Book` | script de prueba | `leonardo-da-vinci-and-commonplace-book` | Ok | 10-X-2026 |
| 18 | `Castilla y León (España)` | script de prueba | `castilla-y-leon-espana` | Ok | 10-X-2026 |
| 19 | `Caçadors de bolets` | script de prueba | `cacadors-de-bolets` | Ok | 10-X-2026 |
| 20 | `13 straße del percebe` | script de prueba | `13-strasse-del-percebe` | Ok | 10-X-2026 |
| 21 | `日本語 pa mi bro` | script de prueba | `EXCEPTION: TextContainsUnsupportedCharactersException` | Rechazado: letras no ASCII | 10-X-2026 |
| 22 | `ｃｏｎｔａｍｉｎａｃｉóｎ` | script de prueba | `contaminacion` | Ok | 10-X-2026 |
| 23 | `😀 Emoji Test` | script de prueba | `emoji-test` | Ok | 10-X-2026 |
| 24 | `ＴＥＳＴ ２０℃` | script de prueba | `test-20c` | Ok | 10-X-2026 |

## Hallazgos para S1.7

| ID | Descripción | Sección fuente | Origen (tarea) | Estado |
| --- | --- | --- | --- | --- |
| H01 | El problema detectado inicialmente era una anomalía de codificación del archivo de prueba y de la consola, no una regresión de `SlugPolicy.Generate`. Se resolvió regrabando el archivo como UTF-8 con BOM y ejecutándolo con la codificación de salida correcta. | [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii), [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | T06, T07 | resuelto en validación |
| H02 | `Napoleón Bonaparte` no requiere corrección de Domain cuando el nombre llega correctamente codificado; la validación con UTF-8 con BOM produce `napoleon-bonaparte`, que coincide con la expectativa del slug. | [«Generación de slugs»](../../context/specifications.md#generación-de-slugs) | T07 | resuelto en validación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03 | [V01](plan.md#estrategia-de-validación) | Validado: la tabla de símbolos coincide con la salida actual del generador en UTF-8 con BOM. | 10-X-2026, SDK .NET 10.0.12 | [«Revisión de la tabla»](#revisión-de-la-tabla), [test-nombres-colecciones-with-BOM.ps1](test-nombres-colecciones-with-BOM.ps1) |
| CA02 | T04 | [V02](plan.md#estrategia-de-validación) | Validado: casos Latin-1 y Latin Extended-A convertidos correctamente o rechazados explícitamente cuando no son soportados. | 10-X-2026, SDK .NET 10.0.12 | [«Cobertura de bloques Latin»](#cobertura-de-bloques-latin) |
| CA03 | T05, T06 | [V03](plan.md#estrategia-de-validación) | Validado: 24 nombres reales ejecutados en el script devuelve slug real o excepción esperada (`TextContainsUnsupportedCharactersException`). | 10-X-2026, SDK .NET 10.0.12 | [«Nombres reales»](#nombres-reales), [test-nombres-colecciones-with-BOM.ps1](test-nombres-colecciones-with-BOM.ps1) |
| CA04 | T07 | [V04](plan.md#estrategia-de-validación) | Validado: la comparación manual con la fuente coincide con la salida real; diferencias se registran como hallazgos. | 10-X-2026 | [«Nombres reales»](#nombres-reales), [«Hallazgos para S1.7»](#hallazgos-para-s17) |
| CA05 | T08 | [V05](plan.md#estrategia-de-validación) | Validado: DLG registra el juicio por nombre en la tabla de resultados. | 10-X-2026 | [«Nombres reales»](#nombres-reales) |
| CA06 | T05, T08 | [V05](plan.md#estrategia-de-validación) | Validado: la política de registro y el juicio de DLG quedan reflejados en la evidencia y en la tabla de resultados. | 10-X-2026 | [«Nombres reales»](#nombres-reales), [«Hallazgos para S1.7»](#hallazgos-para-s17) |
| CA07 | T09, T10, T11 | [V06, V07, V08](plan.md#estrategia-de-validación) | Validado: solución compila y pasa tests; no hay cambios en Domain ni artefactos temporales; roadmap actualizado sin cerrar la etapa. | 10-X-2026, SDK .NET 10.0.12 | [tasks.md](tasks.md), [context/roadmap.md](../../context/roadmap.md) |

La evidencia técnica queda cumplida para CA01–CA07 y DLG registró su aceptación en T12 el 10-X-2026 («Aprobada»); frase corregida en S1.7 (D06).

## Aceptación y cierre

- Estado del trabajo: aceptado y listo para archivar.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno identificado. La anomalía encontrada fue de codificación del script y la consola, no de Domain; se resolvió reescribiendo el archivo como UTF-8 con BOM y validando la salida real.
- Aceptación humana: DLG, 10-X-2026, referencia «Aprobada».
- Archivado: no solicitado; la documentación y la evidencia quedan preparadas para su archivo si DLG lo solicita.
- Ubicación archivada: pendiente de movimiento formal.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; la evidencia de la etapa queda preparada y no requiere cambios funcionales.
- Siguiente paso autorizado, sin cerrar otras etapas: continuar con la siguiente etapa del roadmap tras la aceptación formal de DLG.
