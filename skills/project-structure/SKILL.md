---
name: project-structure
description: "Use when organizing a new or existing project layer so types are placed by responsibility instead of being left in the root or mixed by convenience. Applies the generic repository convention for Contracts, Results, Enums, Requests, Models, Constants and UseCases."
---

# Project Structure

## Propósito

Esta skill ayuda a reorganizar un proyecto cuando la estructura se está desordenando, pero no duplica la convención del repositorio.

La fuente canónica de la regla está en [AGENTS.md](../../AGENTS.md), en la sección “Convención de estructura del código”. Esta skill es el procedimiento práctico para aplicarla.

## Cuándo usar

Usa esta skill cuando:

- se crea una nueva capa o proyecto;
- se reorganiza una estructura existente;
- aparecen tipos mezclados en la raíz o en carpetas sin responsabilidad clara;
- se necesita decidir dónde va una interfaz, un error, un enum, un request o un modelo.

## Procedimiento

1. Revisa la ubicación real del tipo y su responsabilidad, no su nombre superficial.
2. Comprueba si ya existe la regla canónica en [AGENTS.md](../../AGENTS.md).
3. Reubica el tipo según la clasificación de ese documento, manteniendo el namespace alineado con la ruta.
4. No dejes “por ahora” tipos sueltos en la raíz; si no encaja, se mueve antes de continuar.
5. Verifica que imports, namespaces y referencias siguen correctos.

## Referencias

- [AGENTS.md](../../AGENTS.md): fuente única de la convención.
- [.github/instructions/csharp.instructions.md](../../.github/instructions/csharp.instructions.md): recordatorio técnico de C# para la misma convención.

## Criterio de cierre

La reorganización está completa cuando la estructura del proyecto refleja responsabilidades, no conveniencia temporal, y el documento fuente de referencia sigue siendo [AGENTS.md](../../AGENTS.md).
