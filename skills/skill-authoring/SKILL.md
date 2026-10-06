---
name: skill-authoring
description: "Use when creating, updating or reviewing a Linkubator project skill under skills/. Applies the skill-development pattern from Anthropic as guidance while following this repository's conventions for location, frontmatter, language, scope and source-of-truth rules."
---

# Skill Authoring

## Propósito

Crear skills del proyecto que aporten un flujo de trabajo especializado, sean fáciles de descubrir y mantener, y respeten las convenciones de Linkubator. Esta guía adapta el patrón de desarrollo de skills de [Anthropic](https://www.skills.sh/anthropics/claude-code/skill-development); no copia su estructura específica de Claude Code ni reemplaza las reglas de este repositorio.

## Cuándo usar

Usa esta skill cuando se solicite:

- crear una skill nueva para un flujo de trabajo de Linkubator;
- modificar o revisar una skill existente;
- decidir qué instrucciones, recursos o plantillas debe incluir una skill.

No la uses para responder una tarea normal de dominio de otra skill ni para crear agentes, instrucciones globales, prompts o hooks que no sean skills.

## Convenciones obligatorias del repositorio

Antes de escribir, lee [AGENTS.md](../../AGENTS.md) y las skills relacionadas con el flujo que se va a documentar. Si hay conflicto entre una recomendación externa y el repositorio, prevalece el repositorio.

- Ubicación: `skills/<nombre>/SKILL.md`.
- Nombre de directorio y valor YAML `name`: iguales, en minúsculas y kebab-case.
- Frontmatter YAML delimitado por `---`, con `name` y `description` descriptiva y entre comillas cuando corresponda.
- Redacción en castellano, salvo términos técnicos establecidos.
- Añade la skill a la lista «Skills de Linkubator» en [AGENTS.md](../../AGENTS.md), con enlace relativo y propósito.
- Respeta la fuente única: las skills describen procedimientos de trabajo y enlazan reglas del producto; no copian requisitos ni decisiones.
- No accedas a directorios cuyo nombre empiece por `.` en ningún nivel del workspace.

La guía [Skill Development for Claude Code Plugins](https://www.skills.sh/anthropics/claude-code/skill-development) es material de referencia para diseño de skills, no una especificación del runtime de GitHub Copilot. No traslades automáticamente ubicaciones, claves de frontmatter, herramientas ni comportamientos propios de Claude Code.

## Procedimiento

### 1. Definir el propósito y el disparador

- Formula una única capacidad o flujo especializado que la skill habilita.
- Define cuándo debe usarse y cuándo no.
- Escribe `description` como superficie de descubrimiento: usa verbos y términos que una persona realmente escribiría al pedir esa tarea.
- Si el propósito se solapa con otra skill, amplía la existente o delimita claramente responsabilidades antes de crear otra.

### 2. Diseñar el flujo

- Ordena los pasos desde la recopilación de contexto hasta la validación y entrega.
- Especifica qué fuentes leer, qué hacer con contradicciones y qué condiciones bloquean el avance.
- Distingue decisiones confirmadas, propuestas y preguntas pendientes.
- Indica el resultado que debe producir la skill y los criterios para comprobarlo.
- Mantén el procedimiento accionable; evita prosa general que no cambie el comportamiento del agente.

### 3. Mantener la skill autocontenida y ligera

- Incluye en `SKILL.md` lo necesario para detectar y ejecutar el flujo principal.
- Aplica divulgación progresiva: deja en `SKILL.md` el flujo, las reglas de uso y los enlaces; mueve el material extenso o especializado a archivos del paquete y cárgalo solo cuando el paso lo necesite.
- Usa enlaces y rutas relativos a la carpeta de la skill, de modo que sus recursos sigan funcionando al moverla o revisarla.
- Crea solo las carpetas de recursos que la skill necesite; no añadas directorios vacíos ni una estructura ceremonial.
- Organiza los recursos opcionales así:

| Carpeta | Úsala para |
|---|---|
| `references/` | Guías de consulta, documentación de dominio o material de apoyo que el agente debe leer cuando corresponda. |
| `templates/` | Plantillas reutilizables que la skill copia o completa para producir documentos o artefactos. |
| `scripts/` | Automatizaciones deterministas que la skill ejecuta como parte de su flujo. Deben tener un propósito claro, instrucciones de uso y no sustituir decisiones humanas. |
| `assets/` | Archivos auxiliares consumidos por la skill o por el usuario, como ejemplos de datos, imágenes o recursos de salida. |

- Mantén cada recurso fuera de `SKILL.md` solo si es reutilizable, voluminoso o específico de un paso; para instrucciones cortas, el propio `SKILL.md` basta.
- Evita duplicar instrucciones de `AGENTS.md`, reglas de producto o contenido que ya pertenece a otra skill; enlázalos.
- No añadas scripts, dependencias ni assets que no sean necesarios para el flujo.

### 4. Crear o actualizar

- Revisa primero la estructura y los patrones de las skills vecinas.
- Crea la carpeta de la skill y su `SKILL.md` en `skills/<nombre>/`.
- Mantén `name` coherente con el directorio y comprueba que `description` distingue la skill de las demás.
- Si se añade una skill, actualiza la lista de `AGENTS.md` en el mismo cambio.
- Si una skill incluye `references/`, `templates/`, `scripts/` o `assets/`, enlaza cada recurso desde `SKILL.md`, explica cuándo se usa y comprueba la ruta relativa.
- Para scripts, comprueba también que el comando indicado corresponde al entorno disponible y que los efectos del script están claros antes de ejecutarlo.

### 5. Validar

Comprueba antes de entregar:

- el nombre de carpeta y `name` coinciden;
- el frontmatter está delimitado y contiene `name` y `description` válidos;
- la descripción comunica propósito y disparadores concretos;
- el alcance, los límites y el flujo se entienden sin contexto externo innecesario;
- los enlaces a recursos y documentos del repositorio resuelven;
- no se duplican reglas del producto ni instrucciones globales;
- el enlace y resumen en `AGENTS.md` están presentes y correctos;
- no se introdujeron convenciones exclusivas de otra plataforma como si fueran de Copilot.

## Resultado esperado

Informa de la skill creada o modificada, su disparador, los recursos incluidos y las comprobaciones realizadas. Señala cualquier convención o comportamiento dependiente de plataforma que no se haya podido validar.
