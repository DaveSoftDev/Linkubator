---
name: sdd-writer
description: "Use when creating, updating or preparing a technical SDD in specs/ for a Linkubator subsystem or sprint. Derives implementation design from approved context/ rules and acceptance criteria, maintains traceability, detects open decisions, and never invents product rules or duplicates their source."
---

# SDD Writer

## Propósito

Crear y mantener documentación de diseño de implementación en `specs/`. Un SDD explica cómo se implementa un subsistema a partir de la fuente funcional, pero no se convierte en una fuente de reglas del producto.

## Cuándo usar

Usa esta skill cuando:

- se prepara un sprint que requiere diseño técnico;
- se crea un SDD nuevo en `specs/`;
- se modifica el diseño de implementación de un subsistema;
- se necesita comprobar la trazabilidad entre una decisión funcional, el diseño y sus pruebas;
- un cambio de implementación obliga a descubrir dependencias, transacciones, contratos o riesgos técnicos.

No la uses para:

- definir requisitos, límites, mensajes, respuestas o algoritmos del producto;
- resolver una ambigüedad funcional sin decisión humana;
- sustituir la revisión de coherencia de `requirements-review`;
- generar código, proyectos, SQL ejecutable, migraciones, dependencias o configuración de despliegue.

## Fuentes de verdad

Antes de redactar, lee los documentos completos que correspondan al área:

1. [AGENTS.md](../../AGENTS.md): reglas de trabajo e invariantes críticas.
2. [requirements.md](../../context/requirements.md): qué hace el producto y para quién.
3. [specifications.md](../../context/specifications.md): reglas exactas del producto.
4. [domain-model.md](../../context/domain-model.md): entidades, propiedades, relaciones e invariantes.
5. [architecture.md](../../context/architecture.md): plataforma, capas y medidas técnicas generales.
6. [decisions.md](../../context/decisions.md): decisiones, riesgos, pendientes y fuera de alcance.
7. [roadmap.md](../../context/roadmap.md): orden de trabajo y criterios de finalización.
8. [mvp0-plan.md](../../plans/mvp0-plan.md), si el SDD prepara ese plan.
9. [acceptance-criteria.md](../../plans/acceptance-criteria.md), cuando exista y afecte al caso de uso.

El SDD usa estas fuentes mediante enlaces. Si una fuente falta, está incompleta o se contradice, no inventes su contenido: informa del hallazgo y pide la decisión necesaria.

## Flujo de trabajo

### 1. Delimitar el diseño

- Identifica el subsistema, el trabajo que se pretende preparar y el SDD propietario.
- Localiza las reglas funcionales, invariantes y decisiones que aplican.
- Formula una hipótesis concreta de diseño técnico y una comprobación que pueda refutarla.
- Determina qué debe quedar fuera del SDD para no invadir otro subsistema ni duplicar una fuente funcional.

### 2. Comprobar la puerta de entrada

Antes de redactar un diseño para implementación, confirma que:

- las reglas funcionales necesarias tienen fuente propietaria;
- los criterios de aceptación aplicables existen o están claramente identificados;
- no queda una decisión funcional bloqueante;
- la propuesta no contradice las capas ni las medidas técnicas de `architecture.md`.

Si falta una condición, entrega un informe con el bloqueo, la fuente afectada y la pregunta que debe resolver la persona responsable. No presentes una propuesta como decisión confirmada.

### 3. Redactar o actualizar el SDD

- Parte de [templates/sdd-template.md](templates/sdd-template.md).
- Cada SDD nuevo va en su propia subcarpeta: `specs/<slug>/spec.md`, con un `slug` descriptivo en kebab-case. Añade un enlace al nuevo SDD en `specs/README.md`.
- Ajusta las rutas relativas de la plantilla a la ubicación final del SDD; están escritas para el documento resultante, no para su ubicación dentro de los assets de la skill.
- Al actualizar un SDD existente, conserva su ubicación; esta convención no requiere migrar documentos anteriores.
- Mantén el contenido en castellano.
- En `Fuente funcional`, enlaza solo las secciones que sustentan el diseño.
- En `Decisiones técnicas`, describe componentes, contratos, dependencias, flujos internos y límites entre capas.
- En `Persistencia y dependencias`, describe mecanismos técnicos; los valores y reglas funcionales se enlazan desde su fuente.
- En `Invariantes de implementación`, expresa garantías técnicas, especialmente las invariantes críticas de `AGENTS.md` que el diseño debe preservar.
- En `Trazabilidad`, enlaza criterios de aceptación, checklist y verificaciones que demuestran el diseño.
- En `Riesgos y decisiones pendientes`, separa lo aprobado de las propuestas y preguntas abiertas.

### 4. Aplicar cambios relacionados

Solo actualiza fuera de `specs/` cuando el cambio sea de su propiedad:

- El orden del trabajo, la puerta del sprint o el criterio de finalización pertenecen a [roadmap.md](../../context/roadmap.md) o al plan correspondiente.
- Una nueva regla, límite, mensaje, respuesta o algoritmo pertenece a [specifications.md](../../context/specifications.md).
- Una entidad, propiedad, relación o invariante de datos pertenece a [domain-model.md](../../context/domain-model.md).
- El motivo de una decisión, riesgo aceptado o pendiente pertenece a [decisions.md](../../context/decisions.md).
- Una medida técnica general de plataforma o capa pertenece a [architecture.md](../../context/architecture.md).

Un SDD no adelanta decisiones de producto. Si el diseño revela que hace falta una, se detiene y la propone en su documento propietario para aprobación humana.

### 5. Verificar antes de entregar

Comprueba:

- cada enlace a `context/` existe y apunta a la sección correcta;
- no hay límites, listas cerradas, mensajes, códigos de respuesta o reglas de producto duplicados;
- las referencias a MVP concretos solo aparecen en [decisions.md](../../context/decisions.md) y [roadmap.md](../../context/roadmap.md);
- la terminología coincide con `AGENTS.md`;
- el diseño preserva propiedad de datos, público/privado, privacidad de cuenta y límites de transacción aplicables;
- el SDD contiene una estrategia de verificación trazable.

Cuando el cambio abarque varias fuentes o pueda introducir duplicados, ejecuta también `requirements-review` antes de dar el SDD por listo.

## Resultado esperado

Al terminar, informa de:

- SDD creado o actualizado y su alcance técnico;
- fuentes funcionales y criterios trazados;
- decisiones técnicas propuestas y confirmadas;
- bloqueos o preguntas que requieren decisión humana;
- verificación realizada y riesgos restantes.
