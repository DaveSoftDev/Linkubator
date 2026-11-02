# Tareas: S12.4 — Revisión y sincronización de la documentación

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada y aceptada por DLG.
  - Inicio: 31-X-2026, por solicitud de DLG «Inicia la implementación».
  - Finalización técnica: 31-X-2026; cambios documentales y validación de referencias registrados en T03–T05.
  - Evidencia consolidada: informe previo, cambios por fuente y comprobación de 624 enlaces y anclas sin errores.
- La preparación de estas tareas no inicia la revisión ni inventa aprobaciones o evidencias.

## Descomposición

Las tareas son secuenciales: la lectura completa alimenta el informe, que debe entregarse antes de editar; la sincronización precede a la verificación y consolidación finales. Cualquier decisión necesaria se registra y solo bloquea el cambio documental afectado.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras aprobar las tareas e iniciar el trabajo, registrar la fecha y referencia reales en este resumen y actualizar el avance de S12.4 en el roadmap. Comprobar la disponibilidad e inventariar las fuentes y diagramas del alcance; leerlos completos y registrar las áreas revisadas. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V01 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | Ninguna | Verificar el inventario contra las fuentes definidas en la spec y registrar cada área de consistencia como revisada o explícitamente ausente, no aplicable o no verificable. | completada | Leídos AGENTS.md, los seis documentos de contexto, la skill y los 18 diagramas; matriz de consistencia revisada. La comprobación previa cubrió 528 enlaces y anclas locales, sin errores. |
| T02 | Contrastar la documentación completa, clasificar y priorizar los hallazgos con archivo y línea, y entregar primero el informe con su estado general, recomendaciones, decisiones necesarias y riesgos residuales. | CA02; [plan.md → «Flujos internos»](plan.md#flujos-internos), V02 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Revisar cada hallazgo contra sus fuentes propietarias. Resultado: clasificación y prioridad trazables; propuestas y decisiones confirmadas claramente diferenciadas; el informe se entrega antes de aplicar cambios. | completada | Informe previo comunicado antes de las ediciones; hallazgos y ubicaciones registrados a continuación. |
| T03 | Tras entregar el informe, aplicar las sincronizaciones respaldadas por reglas propietarias o decisiones confirmadas; registrar por cambio el documento afectado y su fundamento. Si no hay cambios aplicables, dejarlo indicado. | CA03; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V03 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T02 | Comprobar cada cambio contra su fuente propietaria o decisión confirmada. Resultado: no se inventan ni se cambian reglas en disputa; preguntas pendientes quedan registradas y bloquean solo el cambio afectado. | completada | Sincronizados requirements.md, decisions.md, domain-model.md, architecture.md, roadmap.md y el diagrama de registro; cambios y fuentes propietarias descritos en el informe. |
| T04 | Verificar los enlaces y anclas de la documentación y diagramas revisados, y contrastar el contenido modificado con las responsabilidades, terminología y restricciones de alcance. | CA04; V04 de [«Estrategia de validación»](plan.md#estrategia-de-validación) | T03 | Inspeccionar destinos y anclas y revisar los cambios. Resultado: no quedan referencias rotas introducidas por la sincronización ni cambios fuera de las fuentes confirmadas. | completada | Validados 624 enlaces locales y anclas de AGENTS.md, context/ y los artefactos S12.4; 0 errores. Comprobados los valores exactos afectados y la terminología. La fila S12.4 del índice y su ancla también se validaron. |
| T05 | Consolidar en la evidencia de este archivo el informe final, los resultados por criterio, los cambios aplicados o la ausencia de cambios, los límites y riesgos residuales. Marcar la implementación técnica completada y actualizar el roadmap con evidencia real, sin anticipar aceptación. | CA01–CA05; V01–V05 de [«Estrategia de validación»](plan.md#estrategia-de-validación), [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Contrastar todos los criterios con su evidencia real y las secciones de «Expected output» de la skill. Resultado: no hay resultados esperados presentados como reales ni aprobaciones fabricadas. | completada | Evidencia y resultados consolidados; roadmap e índice actualizados. La aceptación humana permanece pendiente en T06. |
| T06 | Revisar la evidencia consolidada y registrar la aceptación o el rechazo humano del resultado; si se rechaza, anotar las correcciones necesarias sin declarar cerrado el trabajo. | CA01–CA05; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | T05 | Confirmar que CA01–CA05 tienen evidencia satisfactoria y que las decisiones pendientes y límites están claros; registrar la decisión humana explícita. | completada | DLG aceptó S12.4 el 31-X-2026 con la referencia «Acepto S12.4». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01 | V01 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Fuentes y diagramas leídos; matriz de consistencia revisada; 528 enlaces y anclas comprobados sin errores | 31-X-2026; repositorio de trabajo | Inventario y matriz descritos en el informe |
| CA02 | T02 | V02 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Informe de hallazgos priorizados comunicado antes de editar; no requiere decisiones humanas | 31-X-2026; conversación y repositorio de trabajo | [Informe de revisión](#informe-de-revisión) |
| CA03 | T03 | V03 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Sincronizaciones aplicadas conforme a fuentes propietarias; no se modificaron reglas en disputa | 31-X-2026; repositorio de trabajo | Relación de cambios en T03 |
| CA04 | T04 | V04 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | 624 enlaces y anclas locales comprobados; 0 errores | 31-X-2026; repositorio de trabajo | Validación en T04 |
| CA05 | T05–T06 | V05 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Informe, cambios y riesgos consolidados; aceptados por DLG | 31-X-2026; repositorio de trabajo | Evidencia y aceptación en este archivo |

Un resultado no disponible o fallido seguirá pendiente o bloqueado; no se registrarán resultados esperados como reales.

## Informe de revisión

### Informe previo a la edición

Estado: coherente con observaciones.

Fuentes revisadas: [AGENTS.md](../../AGENTS.md), los seis documentos de [context/](../../context/), la skill [requirements-review](../../skills/requirements-review/SKILL.md) y los 18 diagramas de [context/diagrams/](../../context/diagrams/). Se contrastaron responsabilidades documentales, terminología, alcance, identidad y autenticación, propiedad, privacidad, cuenta y seguridad, URLs, búsqueda y listados, páginas públicas, persistencia, scraping, decisiones y roadmap. No se detectaron contradicciones ni deriva de alcance. La comprobación de enlaces locales y anclas encontró 0 errores en 528 referencias.

Hallazgos, por prioridad:

1. **Prioridad 6 — Duplicado:** el nombre exacto de la colección predeterminada aparece fuera de su fuente propietaria en [requirements.md:13](../../context/requirements.md), [requirements.md:41](../../context/requirements.md), [decisions.md:22](../../context/decisions.md), [domain-model.md:41](../../context/domain-model.md), [architecture.md:185](../../context/architecture.md), [roadmap.md:97](../../context/roadmap.md) y [account-onboarding-sequence.md:58](../../context/diagrams/account-onboarding-sequence.md). Destino: [specifications.md → «Completar el registro»](../../context/specifications.md#completar-el-registro) y [«Generación de slugs»](../../context/specifications.md#generación-de-slugs). Recomendación: referirse a la colección predeterminada y enlazar su especificación.
2. **Prioridad 6 — Duplicado:** la decisión de idioma reproduce el segmento exacto de las rutas públicas en [decisions.md:22](../../context/decisions.md). Destino: [specifications.md → «Rutas públicas»](../../context/specifications.md#rutas-públicas). Recomendación: conservar la justificación y enlazar la regla, sin copiar el valor.
3. **Prioridad 6 — Duplicado:** la checklist del roadmap enumera detalles, ejemplos y valores de reglas propietarias, en particular en [roadmap.md:198](../../context/roadmap.md), [roadmap.md:200](../../context/roadmap.md) y [roadmap.md:225](../../context/roadmap.md)–[roadmap.md:250](../../context/roadmap.md). Destinos: las secciones enlazadas de [specifications.md](../../context/specifications.md), [domain-model.md](../../context/domain-model.md) y [architecture.md](../../context/architecture.md). Recomendación: mantener los ámbitos de prueba y remitir a las fuentes para los casos concretos, límites y valores.
4. **Prioridad 6 — Omisión:** [requirements.md:93](../../context/requirements.md) resume el tratamiento de parámetros de URL no aplicables sin enlazar las reglas exactas. Destino: [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [«Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas). Recomendación: añadir ambos enlaces.

Decisiones necesarias: ninguna. Las sincronizaciones propuestas se respaldan en las fuentes propietarias; no se identificaron preguntas abiertas que bloqueen la implementación documental.

Riesgos residuales: esta revisión evalúa la documentación y sus referencias, no demuestra que la implementación de código cumpla las reglas. Una comprobación ampliada del `specs-index.md` completo detectó 51 referencias históricas a artefactos archivados ausentes; la fila S12.4 y su ancla sí existen. Esas referencias preexistentes quedan fuera del alcance de esta revisión y no se modificaron.

### Cambios documentales aplicados

- [requirements.md](../../context/requirements.md): se sustituyeron menciones literales del nombre de la colección por referencias a la especificación y se enlazó la regla de parámetros de URL.
- [decisions.md](../../context/decisions.md): se mantuvo la razón de idioma sin repetir el nombre de la colección ni el segmento de ruta.
- [domain-model.md](../../context/domain-model.md) y [architecture.md](../../context/architecture.md): se conservaron la invariante y el flujo de persistencia, enlazando el nombre exacto a su especificación.
- [account-onboarding-sequence.md](../../context/diagrams/account-onboarding-sequence.md): se sustituyó el nombre literal por una descripción genérica.
- [roadmap.md](../../context/roadmap.md): se enlazaron los detalles de rutas, parámetros y cabeceras a sus fuentes y se resumió la checklist para que conserve los ámbitos de prueba sin duplicar casos y valores exactos.
- No se modificaron reglas propietarias, código ni pruebas. Los cambios se comprobaron contra sus fuentes y no introducen decisiones nuevas.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG el 31-X-2026.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: no quedan bloqueos de S12.4; la revisión no audita la implementación de código y el índice conserva referencias históricas ausentes documentadas arriba.
- Aceptación humana: DLG, 31-X-2026, referencia «Acepto S12.4».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: ninguno para S12.4; S12 permanece abierta hasta completar su Definition of Done.
