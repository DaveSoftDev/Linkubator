# Especificación: S1.3 — Generación de alias

## Estado y aprobación

- Estado: aprobada por DLG el 09-X-2026.
- Referencia de aprobación: «Requisitos entendidos».

## Objetivo

Proporcionar en Domain la generación determinista del alias de usuario a partir del texto introducido, junto con la identificación de los resultados no admisibles para que los flujos de cuenta puedan rechazarlo sin dependencias de infraestructura.

## Fuentes

- [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l): tarea S1.3 y reparto con los trabajos vecinos.
- [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias): transformación, resultados no admisibles, palabras reservadas y tratamiento de la colisión.
- [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas): rango aplicable al resultado transformado y unidad de conteo.
- [domain-model.md → «User»](../../context/domain-model.md#user): propiedad, carácter público, mutabilidad y unicidad global de `User.Alias`.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): responsabilidad y aislamiento de Domain.

## Alcance

- Incluye:
  - Generar en Domain el alias resultante a partir del texto de entrada conforme a [«Generación del alias»](../../context/specifications.md#generación-del-alias).
  - Identificar los resultados vacíos, fuera del rango de longitud y reservados para que la entidad o el caso de uso que corresponda pueda rechazar la operación.
  - Definir los errores de dominio específicos necesarios para esos rechazos sobre la jerarquía entregada por S1.2, sin incluir contenido introducido por el usuario.
  - Pruebas automatizadas del comportamiento de generación y de los rechazos que pertenecen a este trabajo; S1.5 completa la cobertura exhaustiva de los ejemplos de alias y slugs.

- Excluye:
  - Consultar, persistir o resolver la ocupación del alias; la unicidad global y el rechazo de una colisión se integran con `User` y los casos de uso en S3 y S4, y su garantía persistente corresponde a S5.
  - Mostrar la vista previa o los mensajes de validación en el formulario; corresponde a las etapas de cuenta y Web.
  - Crear o completar registros, cambiar el alias de una cuenta y decidir la traducción de errores de dominio a resultados de aplicación.
  - Generar slugs, aplicar su tabla de símbolos o rechazar caracteres no ASCII en nombres; corresponde a S1.4.
  - Definir o cambiar reglas, límites, palabras reservadas o mensajes de producto.

- Trabajo vecino y responsable de sus entregables:
  - S1.1 (aceptada) proporciona el recorte y la comprobación por puntos de código que este trabajo reutiliza cuando proceda.
  - S1.2 (aceptada) proporciona la excepción base de dominio de la que derivan los errores específicos de este trabajo.
  - S1.4 implementa la generación de slugs.
  - S1.5 completa los tests de todos los ejemplos de alias y slug de la fuente.
  - S3 integra el alias en `User` y sus invariantes; S4 y S5 realizan, respectivamente, el rechazo de colisiones en los casos de uso y la garantía de unicidad persistente.

## Dependencias y prerrequisitos

- Existen el proyecto `Linkubator.Domain` y el proyecto de tests con referencia a Domain.
- S1.1 y S1.2 están aceptadas e implementadas; sus componentes se pueden consumir sin introducir dependencias de otras capas.
- La especificación no presupone que exista una entidad `User` ni almacenamiento para consultar la disponibilidad del alias.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) | Un texto introducido para formar un alias. | Domain genera su resultado. | Aplica en el orden establecido las transformaciones de la fuente y produce el alias ASCII resultante. |
| CA02 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) | Entradas representativas con diacríticos, espacios pegados, guiones duplicados o caracteres eliminables. | Domain genera el alias. | El resultado coincide con los ejemplos de transformación de la fuente. |
| CA03 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) y [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | Un resultado transformado vacío o fuera del rango aplicable. | Una operación de Domain solicita validarlo. | Se identifica como no admisible, sin truncarlo ni conservar el texto de entrada en el error de dominio. |
| CA04 | [«Generación del alias»](../../context/specifications.md#generación-del-alias) | Un resultado transformado que coincide exactamente con una palabra reservada. | Una operación de Domain solicita validarlo. | Se identifica como no admisible mediante el error de dominio correspondiente. |
| CA05 | [domain-model.md → «User»](../../context/domain-model.md#user) y [«Generación del alias»](../../context/specifications.md#generación-del-alias) | Un alias generado para una cuenta completada. | Se prepara para asignarlo a `User.Alias`. | Conserva el resultado transformado; la consulta de si está ocupado no se realiza en este trabajo. |
| CA06 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | La implementación de S1.3. | Se comprueban las dependencias del proyecto Domain. | Reside en Domain y no introduce dependencias de Application, Infrastructure, Web ni paquetes externos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para la generación y validación local del alias. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md).