---
name: sdd-writer
description: "Usa esta skill para crear, actualizar o archivar specs bajo Spec-Driven Development en Linkubator. Prepara criterios, diseño, tareas, validaciones y cierre trazables sin implementar código ni inventar aprobaciones o evidencias."
---

# SDD Writer: Spec-Driven Development

## Propósito

Preparar y mantener los artefactos que guían un trabajo bajo Spec-Driven Development (SDD): especificar, aprobar, diseñar y planificar, aprobar, implementar por tareas, validar, aceptar y archivar. SDD nombra el proceso, no un documento de diseño ni una carpeta.

Esta skill escribe documentación y prepara la entrega a implementación. No ejecuta las tareas productivas ni concede aprobaciones humanas. Puede comprobar sus documentos y registrar evidencia de implementación aportada o comprobada mediante validaciones autorizadas, sin confundirla con una aceptación.

## Cuándo usar

- Preparar una especificación de trabajo para un subsistema o bloque del plan.
- Derivar criterios de aceptación verificables de las fuentes aprobadas.
- Diseñar la implementación y descomponerla en tareas con dependencias y validaciones.
- Revisar trazabilidad entre fuente, criterio, tarea y evidencia.
- Mantener estos artefactos cuando cambie una fuente o la implementación revele un bloqueo.
- Archivar una spec terminada cuando se pida explícitamente (por ejemplo, «Archiva la spec S0.2»), tras comprobar sus condiciones de cierre.

No la uses para definir reglas de producto, decidir ambigüedades funcionales, sustituir [requirements-review](../requirements-review/SKILL.md), generar código, proyectos, SQL ejecutable, migraciones, paquetes o despliegue. Una petición de redactar specs no autoriza implementar sus tareas.

## Fuentes y responsabilidad

Lee [AGENTS.md](../../AGENTS.md) y los documentos completos que correspondan al área:

- [requirements.md](../../context/requirements.md): capacidades del producto.
- [specifications.md](../../context/specifications.md): reglas exactas.
- [domain-model.md](../../context/domain-model.md): entidades e invariantes.
- [architecture.md](../../context/architecture.md): tecnologías, arquitectura y medidas técnicas.
- [decisions.md](../../context/decisions.md): decisiones, motivos y pendientes.
- [roadmap.md](../../context/roadmap.md): orden y criterios de finalización.
- [Plan de implementación](../../plans/mvp0-plan.md): tarea, dependencias y revisión del SDD antes del desarrollo cuando aplique.
- Los criterios de aceptación aprobados del trabajo, cuando existan. Comprueba su ubicación antes de enlazarlos; no presupongas un archivo inexistente.

La especificación de trabajo es derivada: no reemplaza `context/`. Enlaza las secciones propietarias, sin copiar valores, algoritmos, mensajes, listas cerradas ni reglas como fuentes independientes. Los criterios expresan comprobaciones de esas reglas, no las redefinen. Si ya existe un criterio aprobado, enlázalo con su identificador en lugar de duplicarlo.

## Artefactos

Para trabajos nuevos usa `specs/<slug>/`, con un nombre descriptivo en kebab-case:

| Archivo | Responsabilidad | Plantilla |
| --- | --- | --- |
| `spec.md` | Resultado técnico esperado, alcance, fuentes y criterios de aceptación derivados. | [spec-template.md](templates/spec-template.md) |
| `plan.md` | Diseño técnico, dependencias, riesgos y estrategia de validación. | [plan-template.md](templates/plan-template.md) |
| `tasks.md` | Tareas ordenadas, estados, evidencia por criterio y aceptación final. | [tasks-template.md](templates/tasks-template.md) |

Es una convención local para los trabajos de esta skill, no un estándar universal de SDD. No crees otros artefactos sin necesidad. Carga cada plantilla cuando vayas a redactar su documento. Sus enlaces están preparados para el destino `specs/<slug>/`; ajusta las rutas si la ubicación difiere. Enlaza el trabajo desde [specs-index.md](../../specs/specs-index.md) sin duplicar su contenido. Mantén [roadmap.md](../../context/roadmap.md) como fuente del estado de las etapas: enlaza el trabajo cuando sea necesario para mostrar su avance, sin convertir los artefactos SDD en una segunda fuente de estado.

Los trabajos terminados y aceptados se trasladan, con su carpeta completa, a `specs/archive/<slug>/`. El estado y la evidencia de cierre pertenecen a `tasks.md`; el índice es solo navegación. Un documento redundante, una propuesta descartada o un diseño antiguo sin cierre no se archivan como trabajo terminado.

## Flujo de trabajo

### 1. Delimitar y contrastar

- Identifica el trabajo, su fuente propietaria y los artefactos existentes.
- Contrasta el alcance con el plan y las tareas vecinas: distingue quién crea un componente, quién escribe sus tests y quién los ejecuta para aceptarlo. No fusiones etapas implícitamente ni presupongas que todos los artefactos SDD ya existen.
- Formula una hipótesis local sobre el resultado esperado y una comprobación que pueda refutarla.
- Si ya hay implementación, úsala solo para detectar detalles técnicos útiles y contrastar que la propuesta no contradice las fuentes. Separa esa lectura del texto que redactas: la existencia del código no demuestra que el criterio esté cumplido ni que se aprobó antes de construir.
- Si falta una fuente necesaria, hay contradicción o una decisión bloqueante, informa del bloqueo, su fuente y la pregunta pendiente. No inventes la regla ni presentes el trabajo como listo para implementar.

### 2. Especificar y preparar la revisión

Redacta `spec.md` con objetivo, fuentes, alcance incluido y excluido, dependencias y criterios identificados como `CA01`, `CA02`, etc. En las celdas de columna `ID` escribe los identificadores sin guion (por ejemplo, `CA01`, `V01`, `T01`); aplica esta forma también a sus referencias en texto, enlaces y evidencias para mantener una nomenclatura uniforme. Usa dado/cuando/entonces o una formulación igualmente observable. Cada criterio enlaza a su fuente o al criterio aprobado propietario y tiene una comprobación posible. No impongas un resultado nuevo sin respaldo.

Identifica casos negativos y límites aplicables mediante enlaces. Mantén el cómo en `plan.md`. Marca la especificación como borrador hasta tener aprobación humana explícita; registra fecha, responsable y referencia real. Una autorización general de implementación no implica aprobación de una spec recién redactada.

Tras crear `spec.md`, revisa [roadmap.md](../../context/roadmap.md). Si el trabajo descompone o hace trazable una etapa activa, añade o actualiza desde el roadmap su enlace a la especificación creada. No declares implementación, aceptación ni cierre por crear o aprobar la especificación.

### 3. Diseñar y planificar

Redacta `plan.md` solo después de que `spec.md` tenga aprobación humana registrada. El plan cubre componentes, responsabilidades, contratos, flujos internos, persistencia y transacciones cuando apliquen, dependencias, garantías técnicas y riesgos. Reutiliza el diseño y los tests existentes adecuados. Separa decisiones confirmadas de propuestas.

Para cada criterio, define validación, alcance, prerrequisitos, resultado esperado y evidencia que se registrará. Distingue prueba automatizada, revisión técnica y validación manual; no atribuyas a un test garantías que no comprueba. Usa comandos concretos y filtros compatibles con el runner cuando se conozcan; si no, registra la detección necesaria como tarea. Evita pruebas de UI o herramientas ajenas a las fuentes aprobadas.

Cuando una garantía dependa de un test arquitectónico o de seguridad, planifica una comprobación negativa proporcionada al riesgo que demuestre que detecta la infracción. No alteres cambios del usuario ni conviertas ese requisito en una nueva infraestructura de testing.

### 4. Descomponer en tareas

Redacta `tasks.md` solo después de que `plan.md` tenga aprobación humana registrada. No crees el archivo de tareas como borrador previo a la aprobación del plan. Usa identificadores estables. Cada tarea debe tener:

- criterio cubierto y enlace al diseño;
- dependencias explícitas y paralelismo solo si las tareas son independientes;
- acción acotada, entregable y comprobación inmediata con resultado esperado;
- estado: pendiente, en curso, completada o bloqueada;
- evidencia real o indicación de que está pendiente.

Ordena preparación, cambios pequeños, validación integrada y revisión humana. Incluye tareas documentales cuando correspondan a este trabajo, sin adelantar trabajo de otra etapa. Todo criterio debe tener tareas y validación; toda tarea debe justificar su contribución. Una tarea no está completada solo por haber creado archivos.

Bajo «Descomposición» usa texto introductorio en esa sección solo para aportar dependencias, paralelismo o restricciones concretas del trabajo que no se entiendan en la tabla.

### 5. Aprobaciones y entrega a implementación

- Registra tres aprobaciones diferenciadas y secuenciales. Al registrar la aprobación de `spec.md`, genera de inmediato `plan.md` a partir de su plantilla, con estado de propuesta y su aprobación pendiente. Al registrar la aprobación de `plan.md`, genera de inmediato `tasks.md` a partir de su plantilla, con estado pendiente de aprobación. Cada documento conserva su estado, responsable, fecha y referencia real.
- La aprobación de un documento no implica la de los otros. Solicita y registra una decisión explícita para cada artefacto; no combines las tres aprobaciones en una sola.
- No crees `plan.md` antes de aprobar `spec.md`, ni `tasks.md` antes de aprobar `plan.md`. Al crear automáticamente el artefacto siguiente, rellénalo hasta donde permitan las fuentes y el documento recién aprobado; registra bloqueos y preguntas reales, pero no inventes decisiones. Hasta su creación, no incluyas el artefacto siguiente en el índice ni dejes enlaces que aparenten que existe.
- No empieces cambios productivos hasta que `tasks.md` esté aprobado.
- No marques aprobado un artefacto no cubierto por una referencia explícita. Una decisión pendiente que afecte a requisitos, contrato o alcance se registra como pregunta concreta.
- Una vez aprobadas las tareas, registra en la cabecera de `tasks.md` el estado de implementación como `no iniciada`. Al iniciarla, actualiza ese resumen con la fecha y referencia reales y crea o completa la tarea documental que sincroniza [roadmap.md](../../context/roadmap.md), cuando el trabajo haga trazable una etapa.
- La implementación avanza por tareas con su check inmediato y evidencia. Tras completar las tareas de implementación y validación, actualiza su resumen en la cabecera de `tasks.md` a `completada`, con su fecha, comprobación y enlaces a la evidencia consolidada. Este estado no constituye aceptación humana ni cierre.
- Revisa [roadmap.md](../../context/roadmap.md) tras cada aprobación y tras el inicio o finalización de la implementación: actualiza una referencia de trazabilidad o el estado de la etapa si es necesario para entender su avance, pero conserva el trabajo en curso hasta que exista evidencia satisfactoria y aceptación humana del resultado.

### 6. Validación, aceptación y mantenimiento

- Antes de entregar documentos, verifica frontmatter cuando aplique, rutas, anclas, identificadores, dependencias sin ciclos y cobertura fuente -> criterio -> tarea -> validación.
- Comprueba que no se duplican reglas del producto y que se respetan terminología e invariantes de [AGENTS.md](../../AGENTS.md). Si el cambio requiere sincronizar fuentes o introduce una posible duplicación, usa [requirements-review](../requirements-review/SKILL.md) antes de darlo por listo.
- Al actualizar evidencia, registra criterio, comprobación, entorno o versión pertinente, resultado, fecha y referencia. No inventes comandos ejecutados, resultados, aprobaciones ni fechas. Una validación pendiente, fallida o no disponible sigue explícita.
- Si la implementación se bloquea o se reabre, actualiza de inmediato su resumen en la cabecera de `tasks.md`, la tarea afectada y [roadmap.md](../../context/roadmap.md) cuando este muestre el avance de la etapa. Distingue este estado de la aprobación de tareas y de la aceptación del resultado.
- El cierre requiere evidencia satisfactoria para todos los criterios, tareas completadas y aceptación humana registrada. Pruebas verdes no conceden aceptación humana ni cierran otras etapas.
- Al registrar la aceptación humana del resultado, actualiza [roadmap.md](../../context/roadmap.md) para reflejar el trabajo terminado y enlazar la evidencia de cierre en `tasks.md`. Si el trabajo aún no se archiva, usa su ruta activa; recalcula el enlace si se archiva después.
- Si un cambio revela una regla nueva o una contradicción, bloquea la tarea afectada, propone el cambio en su documento propietario y espera aprobación antes de actualizar e implementar los artefactos dependientes. No edites reglas funcionales por iniciativa propia.

### 7. Archivar un trabajo terminado

Aplica este paso solo ante una petición inequívoca de archivado; preguntar si se puede archivar, preparar un plan o aprobar la skill no es una orden de archivar una spec concreta.

1. Identifica el trabajo por su carpeta y por [specs-index.md](../../specs/specs-index.md). Si ya está en `specs/archive/<slug>/`, comprueba el cierre y los enlaces; informa de que ya está archivado sin duplicarlo ni alterar la fecha. Si existen carpeta activa y archivada con el mismo slug, detente y comunica la colisión; nunca sobrescribas ni mezcles contenidos.
2. Confirma que existen `spec.md`, `plan.md` y `tasks.md` coherentes, que todos los criterios tienen evidencia satisfactoria y vigente para el estado que se pretende cerrar, que todas las tareas necesarias están completadas y que no hay bloqueos. Comprueba las referencias a validaciones y a cualquier evidencia local. Una spec antigua que aún no sigue este flujo debe migrarse y auditarse primero, sin inventar tareas ejecutadas ni aprobaciones previas.
3. Comprueba la aceptación humana explícita registrada en `tasks.md`. Una orden directa e inequívoca de archivar el trabajo identificado puede servir de aceptación del resultado **solo después** de verificar todos los puntos anteriores: consigna quién la dio, fecha y referencia real a esa instrucción. Si no se conoce su autor o falta una referencia verificable, solicita la aceptación sin fabricarla. La orden nunca reemplaza evidencia ni convierte pruebas pendientes en satisfactorias.
4. Antes de mover, inventaría los archivos de la carpeta, los enlaces internos y entrantes desde documentos permitidos del repositorio —incluido [roadmap.md](../../context/roadmap.md) cuando haga referencia al trabajo— y el destino de imágenes y evidencias. No entres ni busques en directorios cuyo nombre empiece por `.`; si resulta imprescindible acceder a uno, detente y explícaselo al usuario. Prepara cambios de rutas resolviendo el destino anterior de cada enlace y recalculando su ruta desde la nueva ubicación; no sustituyas `../` globalmente. Los enlaces dentro de la misma carpeta suelen conservarse.
5. Registra la aceptación y el cierre en `tasks.md`, pero deja el **archivado** como pendiente hasta comprobar el traslado. Mueve todos los artefactos y evidencias de la carpeta a `specs/archive/<slug>/`; crea `archive/` solo en el primer archivado efectivo. Actualiza referencias entrantes, enlaces salientes, [specs-index.md](../../specs/specs-index.md) y el enlace de [roadmap.md](../../context/roadmap.md) si existe, para distinguir trabajos activos y archivados. No dejes duplicados ni un alias en la ruta antigua por defecto.
6. Verifica que existen los archivos esperados en el destino, que la carpeta activa ya no existe, que los enlaces y anclas entrantes/salientes —incluido el del roadmap— y evidencias siguen resolviendo y que no hay referencias a la ruta anterior. **Solo entonces** registra en el `tasks.md` trasladado el estado `archivado`, la fecha efectiva y la ubicación, y confirma que el índice y el roadmap apuntan a ese cierre cuando corresponda. Ante un fallo, informa de la ubicación y del estado reales, corrige únicamente lo que acabas de cambiar si es seguro y no declares el archivado completado.

Archivar no significa obsoleto ni cierra trabajos vecinos. Si un cambio posterior requiere modificar lo ya aceptado, prepara una nueva spec que enlace la archivada; no reescribas en silencio su aceptación histórica.

## Resultado esperado

Informa de los artefactos creados o actualizados, alcance, trazabilidad y tareas preparadas; separa propuesta de aprobación y comprobación documental de evidencia de implementación. Si se solicitó archivado, indica la ubicación comprobada o el bloqueo concreto, sin declarar terminado un movimiento parcial. Señala validaciones pendientes y el siguiente paso autorizado. No declares el bloque implementado o aceptado por haber redactado sus specs.
