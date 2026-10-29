# Tareas: S11.7 — Revisión del HTML público

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completado.
  - Inicio: DLG autorizó la implementación de S11.7 el 27-X-2026 con la referencia «Inicia la implementación».
  - Finalización: revisión manual ejecutada y aceptada por DLG el 27-X-2026 con la referencia «Completado».
  - Evidencia consolidada: registrada en T01–T06 para CA01–CA04.
- La preparación de estas tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 confirma los prerrequisitos y prepara la observación segura. V01–V04 son recorridos manuales diferenciados por criterio; T06 consolida su evidencia y T07 solicita la aceptación humana sin confundirla con la aprobación de estas tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar que las páginas S11.1–S11.5 y la validación S11.6 están disponibles; identificar un entorno local o de pruebas y datos seguros para los casos que deben observarse. Registrar navegador, versión y límites de preparación. | CA01–CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | Ninguna | Comprobar acceso a las páginas y que el entorno y los datos pueden usarse sin afectar datos ajenos a la revisión. Resultado: prerrequisitos observables y seguros, o bloqueo concreto registrado. | completado | Entorno local verificado en `https://localhost:5001`; datos de ejemplo y páginas públicas accesibles. Browser/inspección manual realizada con `curl`/HTML renderizado; no se usan datos ajenos. |
| T02 | Inspeccionar los títulos, descripciones y canonical de las cuatro páginas públicas; revisar canonical de primera página y página interior de colección si los datos lo permiten. | CA01; [plan.md → V01](plan.md#estrategia-de-validación) | T01 | Comparar el HTML renderizado con sus fuentes propietarias. Resultado: valores coincidentes y canonical correcta, o discrepancia registrada por página y estado. | completado | Landing: `<title>Linkubator, donde gestionar tus enlaces favoritos</title>`, canonical `https://localhost:5001/`; perfil: `<title>Los enlaces favoritos de my demo user en Linkubator</title>`, canonical `https://localhost:5001/my-demo-user/`; colecciones: canonical `https://localhost:5001/my-demo-user/colecciones/`; colección interior: canonical `https://localhost:5001/my-demo-user/colecciones/pub-col`. |
| T03 | Inspeccionar idioma y encabezados de cada página pública. | CA02; [plan.md → V02](plan.md#estrategia-de-validación) | T01 | Comprobar `lang`, cantidad y contenido de encabezados frente a la fuente propietaria. Resultado: coincidencia en cada página o hallazgo reproducible registrado. | completado | Todas las páginas declaradas con `lang="es"` en el `<html>`; cada página presenta una jerarquía coherente: landing con `h1` principal, perfil con `h1` “Enlaces favoritos de my demo user”, listados con `h1` “Colecciones de my demo user”, y colección con `h1` “Enlaces en Pub Col de my demo user”. |
| T04 | Inspeccionar `alt` de imágenes de enlaces y del recurso decorativo en los casos disponibles. | CA03; [plan.md → V03](plan.md#estrategia-de-validación) | T01 | Comparar valores observados con la fuente propietaria. Resultado: casos previstos comprobados, o escenarios ausentes y discrepancias identificados explícitamente. | completado | En la página de perfil aparecen imágenes con `alt` específicos (`Capacities`, `Holi`, `Linkubator`); los íconos decorativos de marca usan `alt=""` y `aria-hidden="true"`; no se observan alt vacíos en enlaces ni textos provisionales. |
| T05 | Revisar los textos de interfaz y los estados vacíos pertinentes de las cuatro páginas. | CA04; [plan.md → V04](plan.md#estrategia-de-validación) | T01 | Contrastar los textos renderizados con sus fuentes. Resultado: no quedan textos provisionales o pendientes, o se registran las diferencias concretas. | completado | Se verifican textos de cabecera, etiquetas, nota “Solo contenido que my demo user ha hecho público”, y enlaces de navegación; no quedan textos temporales ni pendientes. Los estados con contenido real muestran la interfaz completa; no se detectó un caso vacío dentro del conjunto observado. |
| T06 | Consolidar resultados y evidencia por CA01–CA04 y sincronizar el estado de S11.7 en roadmap e índice sin declarar aceptación humana. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T02, T03, T04, T05 | Verificar que cada criterio tiene resultado y evidencia real o bloqueo explícito; actualizar los documentos de navegación con el estado comprobado. Resultado: trazabilidad coherente y sin criterios presentados como satisfechos sin evidencia. | completado | Evidencia manual registrada en esta tarea; no se declara cierre humano, pero sí se confirma la revisión realizada. |
| T07 | Revisar la evidencia y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Presentar los resultados y límites de cobertura. Resultado: decisión explícita registrada; si se solicita corrección, el trabajo permanece abierto y se registra el seguimiento. | completado | Aceptación humana registrada por DLG el 27-X-2026 con la referencia «Completado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T06 | V01 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 27-X-2026, entorno local `https://localhost:5001` | Títulos y canonical verificadas en la landing, perfil, colecciones y colección interior |
| CA02 | T01, T03, T06 | V02 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 27-X-2026, entorno local `https://localhost:5001` | `lang="es"` y jerarquías `h1` verificadas en cada página pública |
| CA03 | T01, T04, T06 | V03 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 27-X-2026, entorno local `https://localhost:5001` | `alt` de enlaces y decorativos comprobados en las imágenes observadas |
| CA04 | T01, T05, T06 | V04 en [plan.md](plan.md#estrategia-de-validación) | Cumplido | 27-X-2026, entorno local `https://localhost:5001` | Textos de interfaz, etiquetas, navegación y mensajes renderizados verificados |

## Aceptación y cierre

- Estado del trabajo: completado.
- Criterios con evidencia satisfactoria: CA01–CA04.
- Bloqueos y riesgos restantes: no hay bloqueo funcional en la revisión observada; la aceptación final se registra como cierre.
- Aceptación humana: DLG aceptó este trabajo el 27-X-2026 con la referencia «Completado».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: cerrar la tarea y dejar constancia de la revisión ejecutada y aceptada.
