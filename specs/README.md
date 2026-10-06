# Especificaciones de diseño (SDD)

Este directorio reúne la documentación técnica del diseño de implementación. Su finalidad es describir cómo se estructura y se comporta cada subsistema con base en la documentación funcional de `context/`, sin duplicar las reglas del producto.

## Propósito

- Definir la arquitectura interna de cada subsistema.
- Establecer los contratos, dependencias y límites entre capas.
- Documentar decisiones de implementación con trazabilidad a sus fuentes funcionales.
- Dejar claro qué se debe validar antes de cerrar cada sprint.

## Regla de origen

La fuente de verdad sigue siendo `context/`:

- [requirements.md](../context/requirements.md): qué hace el producto y para quién.
- [specifications.md](../context/specifications.md): reglas exactas del producto.
- [domain-model.md](../context/domain-model.md): entidades, propiedades y invariantes.
- [architecture.md](../context/architecture.md): decisiones de plataforma y capas.
- [decisions.md](../context/decisions.md): porqué de decisiones relevantes.
- [roadmap.md](../context/roadmap.md): orden y criterio de finalización.

Los documentos de `specs/` enlazan a estas fuentes y no repiten sus reglas.

## Estructura

- [sdd-writer](../skills/sdd-writer/SKILL.md): skill que crea y mantiene los SDD mediante su plantilla interna.
- [fundaciones.md](fundaciones.md): diseño de la base técnica y del entorno de trabajo.
- [dominio.md](dominio.md): diseño del núcleo funcional y sus invariantes técnicas.
- [aplicacion.md](aplicacion.md): contratos de casos de uso, identidad y coordinación entre capas.

## Ciclo recomendado

1. Revisar la fuente funcional antes de redactar el SDD.
2. Preparar la propuesta técnica y dejarla abierta a comentarios.
3. Aprobación previa al sprint antes de implementar el trabajo de esa área.
4. Ajustes de implementación durante el sprint solo si la decisión ya queda respaldada por su documento padre.
5. Cierre con comprobación de trazabilidad y coherencia entre `context/` y `specs/`.

## Criterio de entrada

Un SDD se considera válido cuando:

- conecta claramente con la regla funcional que lo motiva;
- no repite un valor, límite o mensaje del producto como regla independiente;
- describe mecanismos, dependencias y responsabilidades de implementación;
- deja explícita la verificación que confirma que el diseño funciona.

## Criterio de salida

Un SDD queda obsoleto si:

- introduce una regla de negocio sin reflejar su origen en `context/`;
- deja de corresponderse con la solución real o con la decisión aprobada;
- no se puede seguir con una trazabilidad clara a la fuente funcional.

En esos casos, se actualiza primero la fuente funcional y después el SDD que la utiliza.
