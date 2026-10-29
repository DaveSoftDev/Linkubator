# Tareas: S11.6 — Pruebas de integración HTTP del área pública

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada y verificada técnicamente el 27-X-2026.
  - Inicio: DLG autorizó la implementación de S11.6 el 27-X-2026 («Inicia la implementación»).
  - Finalización: completada el 27-X-2026 tras la comprobación de la suite focalizada de integración Web.
  - Evidencia consolidada: la validación técnica se ejecutó con `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; la salida registró 94 pruebas correctas, 0 errores.
- Aceptación humana:
  - Estado: completada por DLG el 27-X-2026 con la referencia «Completado».
- La preparación de estas tareas no inventa aprobaciones ni evidencias de implementación; la evidencia técnica real queda registrada en esta misma hoja y en el historial del entorno de ejecución.

## Descomposición

T02 y T03 se ejecutan en secuencia porque ambas pueden modificar los casos y la preparación de datos de `WebStartupTests`. La validación integrada V06 debe esperar a que S11.2 y S11.5 estén disponibles y cerradas; esta dependencia no incorpora sus entregables ni evidencias a S11.6.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras aprobar estas tareas e iniciar el trabajo, actualizar aquí el estado de implementación y sincronizar el roadmap y el índice con el inicio real de S11.6. | Seguimiento SDD; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar la aprobación e inicio reales, registrar fecha y referencia, y reflejar el inicio sin cambiar el estado de otros trabajos. | completado | Inicio de S11.6 registrado y sincronizado con roadmap e índice el 27-X-2026. |
| T02 | Revisar los casos existentes y completar las pruebas HTTP del contenido y orden públicos, los marcadores privados, los estados vacíos y los `404` requeridos. Reutilizar la fábrica, la base temporal y las semillas existentes; no cambiar producción. | CA01–CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Garantías técnicas»](plan.md#garantías-técnicas), V01–V02 | T01 | Ejecutar V01 y V02; los casos pertinentes demuestran el contenido y orden esperados, la ausencia de marcadores privados y las respuestas `404` directas sin `Location` ni filtración. | completado | Evidencia técnica en `WebStartupTests`: validación de páginas públicas, privacidad, `404` y ausencia de contenido privado. |
| T03 | Completar las pruebas HTTP de variantes de ruta, conservación de paginación, canonical normalizada y origen configurado frente a un `Host` distinto. Reutilizar los casos existentes y no duplicar coberturas equivalentes. | CA03–CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos), [«Garantías técnicas»](plan.md#garantías-técnicas), V03–V05 | T02 | Ejecutar V03–V05; las redirecciones conducen a la ruta canónica, la respuesta paginada y su canonical corresponden a la página resultante, y la canonical usa el origen configurado. | completado | Evidencia técnica en `WebStartupTests` para redirecciones, canonical y paginación. |
| T04 | Ejecutar la suite integrada focalizada cuando se hayan cerrado los prerrequisitos de S11.2 y S11.5, y registrar resultados y límites. | CA01–CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V06 | T03; cierre de S11.2 y S11.5 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; todas las pruebas aplicables de la suite pasan. | completado | `dotnet test ... --filter "FullyQualifiedName~WebStartupTests"` → 94/94 pruebas correctas, 0 errores. |
| T05 | Consolidar la evidencia por criterio y actualizar el estado de implementación, el roadmap y el índice con el resultado técnico, sin registrar aceptación humana. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que todos los criterios enlazan validaciones y resultados reales, y que roadmap e índice reflejan el estado sin declarar aceptación. | completado | Roadmap e índice sincronizados con la evidencia técnica real. |
| T06 | Revisar la evidencia y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar V01–V06 y la evidencia por criterio; registrar aceptación humana explícita o el bloqueo y su siguiente acción. | completado | DLG acepta la evidencia y registra la referencia «Completado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01 | Correcto | 27-X-2026, entorno local de ejecución de tests `dotnet test --filter "FullyQualifiedName~WebStartupTests"` | `WebStartupTests` con validación de contenido, orden, privacidad y `404` |
| CA02 | T02, T04 | V02 | Correcto | 27-X-2026, entorno local de ejecución de tests `dotnet test --filter "FullyQualifiedName~WebStartupTests"` | `WebStartupTests` con validación de ausencia de marcas privadas y de contenido no público |
| CA03 | T03, T04 | V03 | Correcto | 27-X-2026, entorno local de ejecución de tests `dotnet test --filter "FullyQualifiedName~WebStartupTests"` | `WebStartupTests` con validación de variantes de ruta y redirecciones |
| CA04 | T03, T04 | V04 | Correcto | 27-X-2026, entorno local de ejecución de tests `dotnet test --filter "FullyQualifiedName~WebStartupTests"` | `WebStartupTests` con validación de canonical y paginación |
| CA05 | T03, T04 | V05 | Correcto | 27-X-2026, entorno local de ejecución de tests `dotnet test --filter "FullyQualifiedName~WebStartupTests"` | `WebStartupTests` con validación de host configurado y canonical final |

## Aceptación y cierre

- Estado del trabajo: cierre técnico y aceptación humana completados.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno.
- Aceptación humana: completada por DLG el 27-X-2026 con la referencia «Completado».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: cerrar el trabajo como completado y continuar con el flujo siguiente del roadmap.
