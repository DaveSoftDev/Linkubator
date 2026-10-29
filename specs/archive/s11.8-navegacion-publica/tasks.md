# Tareas: S11.8 — Prueba manual de navegación pública

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada y aceptada el 28-X-2026; S11.8 cerrada.
  - Inicio: DLG autorizó el inicio de S11.8 el 27-X-2026 con la referencia «Inicia la implementación».
  - Validación: V01–V04 ejecutadas satisfactoriamente el 28-X-2026; V02 se repitió guardando explícitamente ambos estados desde edición.
  - Evidencia consolidada: resultados y observaciones registrados en «Evidencia por criterio»; fixtures temporales eliminados y ausencia verificada.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 confirma los prerrequisitos y prepara los datos seguros. T02–T05 ejecutan V01–V04; se mantienen en secuencia porque la revisión reutiliza datos de prueba y T04 cambia el estado de una colección que antes se observa como pública. T06 consolida la evidencia y sincroniza los documentos de navegación; T07 solicita la aceptación humana.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar que las páginas, rutas y formularios requeridos están disponibles; identificar un entorno local o de pruebas y datos seguros para los casos CA01–CA04. Registrar navegador, versión y límites de preparación. | CA01–CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | Ninguna | Comprobar el acceso y que los datos pueden usarse sin afectar datos ajenos a la revisión. Resultado: prerrequisitos y escenarios observables y seguros, o bloqueo concreto registrado. | completada | 28-X-2026: entorno local `https://localhost:7263`, perfil Development, perfil de prueba Demo User (`my-demo-user`), páginas y formularios accesibles. Navegador integrado de VS Code 1.143.1, Chromium 150.0.7871.250. Preparados datos identificables S11.8: colección pública con un enlace público y otro privado (ID 5), colección privada vacía (ID 6) y colección pública vacía (ID 7). No se modificaron los registros preexistentes ni se usaron datos de producción. |
| T02 | Recorrer la página pública del usuario y la página de colecciones; verificar qué colecciones aparecen según sus estados. | CA01; [plan.md → V01](plan.md#estrategia-de-validación) | T01 | Comparar las colecciones públicas visibles con los datos preparados. Resultado: aparecen las que cumplen las condiciones fuente y no se identifican las privadas ni las que carecen de enlaces públicos. | completada | 28-X-2026, `https://localhost:7263`, Chromium 150.0.7871.250: `S11.8 Publica 20261027` aparece en el perfil y en la página de colecciones; `S11.8 Vacía 20261027` y `S11.8 Privada 20261027` no aparecen. |
| T03 | Comprobar los estados de enlaces guardados en edición y recorrer la página de una colección pública. | CA02; [plan.md → V02](plan.md#estrategia-de-validación) | T02 | Confirmar que aparece el enlace público y no el privado, de acuerdo con los estados guardados. Resultado: coincidencia o discrepancia concreta registrada. | completada | 28-X-2026: repetición V02 con colección pública temporal ID 5 y enlaces ID 70 (privado) e ID 71 (público). El enlace ID 70 se guardó desmarcado y el listado lo mostró «Privado». En edición del ID 71, el checkbox empezó desmarcado y habilitado; se marcó y guardó, y el listado lo mostró «Público». La página pública `/my-demo-user/colecciones/s11-8-v02-retest-20261027` respondió `200`, mostró el título del enlace público y no mostró el privado. Se eliminaron ambos enlaces y la colección temporal; se verificó su ausencia. |
| T04 | Editar y guardar como privada la colección pública preparada; comprobar los estados de sus enlaces y volver a visitar su ruta pública. | CA03; [plan.md → V03](plan.md#estrategia-de-validación) | T03 | Revisar los estados de los enlaces en edición y la navegación pública. Resultado: todos los enlaces quedan privados, la colección deja de aparecer y no se expone contenido privado, o se registra el hallazgo. | completada | 28-X-2026: la colección ID 5 quedó guardada como privada; el listado la muestra como «Privada» y los enlaces ID 70 y 71 aparecen como privados. Sus formularios muestran el checkbox desmarcado y deshabilitado. La colección no aparece en el perfil ni en la página pública de colecciones; su ruta responde `404` sin los títulos de prueba. |
| T05 | Solicitar directamente las rutas de la colección privada y de la colección pública sin enlaces públicos; inspeccionar estado HTTP y respuesta. | CA04; [plan.md → V04](plan.md#estrategia-de-validación) | T04 | Comprobar ambas rutas y revisar su contenido. Resultado: respuesta `404` sin contenido privado ni distinción frente a una colección inexistente o no accesible públicamente, o discrepancia registrada. | completada | 28-X-2026: las rutas de la colección privada ID 6 (`/my-demo-user/colecciones/s11-8-privada-20261027`) y la colección pública vacía ID 7 (`/my-demo-user/colecciones/s11-8-vacia-20261027`) responden `404`, sin títulos ni URL de prueba, y el texto visible de ambas coincide con el de una ruta inexistente. La colección ID 5, ya privada tras T04, también respondió `404` sin los títulos de prueba. |
| T06 | Consolidar los resultados y la evidencia por criterio; actualizar el estado de S11.8 en el roadmap y el índice sin declarar aceptación humana. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T02, T03, T04, T05 | Verificar que cada criterio tiene evidencia real satisfactoria o bloqueo explícito. Resultado: trazabilidad coherente y sin criterios presentados como satisfechos sin evidencia. | completada | 28-X-2026: CA01–CA04 tienen evidencia satisfactoria en esta tabla; índice y roadmap sincronizados. La aceptación humana quedó registrada por separado en T07. |
| T07 | Revisar la evidencia consolidada y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Presentar los resultados y límites de cobertura. Resultado: aceptación explícita registrada o correcciones/bloqueos documentados; la revisión no se cierra sin aceptación. | completada | 28-X-2026: DLG aceptó el resultado y el cierre funcional mediante la opción «Aceptar — registrar “Completado” y cerrar S11.8 (sin archivar)». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T06 | V01 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 28-X-2026, `https://localhost:7263`, Chromium 150.0.7871.250 | Perfil público y página de colecciones: colección pública con enlace visible; colección pública vacía y colección privada no aparecen |
| CA02 | T01, T03, T06 | V02 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 28-X-2026, `https://localhost:7263`, Development; navegador integrado de VS Code 1.143.1, Chromium 150.0.7871.250 | Repetición con colección pública temporal ID 5: enlace ID 70 guardado privado y enlace ID 71 guardado público desde edición. Para el ID 71, el checkbox empezó desmarcado y habilitado; al marcarlo y guardar, el listado indicó «Público». El listado indicó «Privado» para el ID 70. La página `/my-demo-user/colecciones/s11-8-v02-retest-20261027` respondió `200`, mostró el enlace público y omitió el privado. Los enlaces y la colección temporal se eliminaron y su ausencia se verificó. |
| CA03 | T01, T04, T06 | V03 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 28-X-2026, `https://localhost:7263`, Chromium 150.0.7871.250 | Colección ID 5 guardada privada; ambos enlaces aparecen privados y sus checkboxes desmarcados/deshabilitados; perfil y página de colecciones no exponen el nombre; ruta de colección devuelve `404` sin títulos de prueba |
| CA04 | T01, T05, T06 | V04 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 28-X-2026, `https://localhost:7263`, Chromium 150.0.7871.250 | Rutas de colección privada ID 6 y colección pública vacía ID 7 respondieron `404`, sin datos de prueba y con texto visible igual al de una ruta inexistente; la ruta de colección ID 5, privada tras T04, también respondió `404` sin títulos de prueba |

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado el 28-X-2026.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno. Los fixtures iniciales y los de la repetición de V02 se retiraron; no se modificó código dentro de S11.8.
- Aceptación humana: DLG aceptó el 28-X-2026 mediante la opción «Aceptar — registrar “Completado” y cerrar S11.8 (sin archivar)».
- Archivado: no solicitado.
- Ubicación archivada: no aplica; el trabajo permanece en su carpeta activa.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso: ninguno para S11.8; el archivado no fue solicitado.
