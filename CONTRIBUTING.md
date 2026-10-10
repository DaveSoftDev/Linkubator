# Guía de contribución

## Ramas

Usa nombres breves y descriptivos:

- `feature/<id>-<descripcion>`
- `fix/<id>-<descripcion>`
- `docs/<id>-<descripcion>`
- `chore/<id>-<descripcion>`

Ejemplos:

- `feature/s1-alias-generation`
- `fix/s0-tailwind-build`
- `docs/s0-readme`

## Commits

Usa prefijos claros:

- `fea:` para funcionalidad
- `fix:` para correcciones
- `doc:` para documentación
- `chore:` para tareas de mantenimiento y configuración
- `test:` para pruebas

Ejemplos:

- `fea: add alias generation rules`
- `doc: update development readme`
- `test: add architecture dependency checks`

## Pull requests

Cada PR debe incluir:

1. resumen breve de la tarea;
2. enlace a la spec o documento que lo acompaña;
3. comprobación ejecutada (build, tests o validación concreta);
4. nota de riesgos, si aplica.

## Backlog

El orden de ejecución del MVP0 está en [plans/mvp0-plan.md](plans/mvp0-plan.md) y debe respetarse antes de pasar a un bloque funcional posterior.
