# Ficha derivada: User

> Estado: vista de revisión derivada dentro de S3.5. No es una fuente de reglas ni sustituye la aprobación final de S3.6.

## Fuentes consultadas

- [domain-model.md → «User»](../../context/domain-model.md#user)
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos)
- [domain-model.md → «Restricciones de unicidad»](../../context/domain-model.md#restricciones-de-unicidad)
- [requirements.md → «Cuenta de usuario»](../../context/requirements.md#cuenta-de-usuario)
- [specifications.md → «Cuenta»](../../context/specifications.md#cuenta)
- [specifications.md → «Email»](../../context/specifications.md#email)
- [specifications.md → «Contraseñas»](../../context/specifications.md#contraseñas)
- [specifications.md → «Correo»](../../context/specifications.md#correo)
- [specifications.md → «Tokens»](../../context/specifications.md#tokens)
- [specifications.md → «Sesión»](../../context/specifications.md#sesión)
- [decisions.md](../../context/decisions.md)

## Propiedades y semántica

El dominio define a `User` como el propietario de los datos y la entidad de identidad del sistema. Su clase reúne los datos de cuenta, de perfil y de auditoría que siguen en [domain-model.md → «User»](../../context/domain-model.md#user): `Id`, `Email`, `Name`, `Alias`, `Password`, `EmailConfirmedAt`, `SecurityStamp`, `FailedLoginAttempts`, `LockoutEnd`, `LastEmailSentAt`, `LastExistingAccountNoticeAt` y `CreatedAt`.

- `Email` es obligatorio y único en todo el sistema; se normaliza y valida según [specifications.md → «Email»](../../context/specifications.md#email).
- `Alias` es público, mutable y único también en todo el sistema. Su generación queda en [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias).
- `Password` se guarda como hash, nunca como texto plano, y solo cambia por completar registro, cambio de contraseña o restablecimiento validado; ver [specifications.md → «Contraseñas»](../../context/specifications.md#contraseñas).
- `SecurityStamp` se genera al crear la cuenta, y se regenera al cambiar o restablecer la contraseña y al confirmar un cambio de email; esto invalida sesiones anteriores, según [domain-model.md → «User»](../../context/domain-model.md#user).
- `LastEmailSentAt` y `LastExistingAccountNoticeAt` guardan las marcas del último correo con token y del último aviso de cuenta existente, y cada uno alimenta su propio límite; ver [specifications.md → «Correo»](../../context/specifications.md#correo).
- `FailedLoginAttempts` y `LockoutEnd` forman la protección contra intentos fallidos; su comportamiento se explica en [specifications.md → «Bloqueo por intentos fallidos»](../../context/specifications.md#bloqueo-por-intentos-fallidos).

Las propiedades se enumeran en [domain-model.md#L7-L42](../../context/domain-model.md#L7). Las reglas exactas de email y contraseña están en [specifications.md#L232-L257](../../context/specifications.md#L232) y [specifications.md#L244-L293](../../context/specifications.md#L244).

## Reglas de ciclo de vida y registro

- Un registro incompleto solo contiene el email y el `SecurityStamp`; `Name`, `Alias` y `Password` son `null` y no puede iniciar sesión. Al completar el registro, los cuatro valores se fijan a la vez: [domain-model.md → «User»](../../context/domain-model.md#user).
- Reintentarse el registro con el mismo email de un registro incompleto no crea una cuenta nueva: se reenvía el correo de completar registro, según [specifications.md → «Registro»](../../context/specifications.md#registro).
- La cuenta no permite iniciar sesión mientras el registro esté sin completar, de acuerdo con [requirements.md → «Cuenta de usuario»](../../context/requirements.md#cuenta-de-usuario).
- La eliminación del usuario elimina todos sus datos asociados: colecciones, enlaces, etiquetas, relaciones `LinkTag`, índice de búsqueda y tokens, según la misma sección de [domain-model.md → «User»](../../context/domain-model.md#user).

Los flujos completos de registro, compleción, login, recuperación, cambio de email, cambio de contraseña, configuración y eliminación están en [specifications.md#L40-L114](../../context/specifications.md#L40); los requisitos de cuenta se resumen en [requirements.md#L9-L30](../../context/requirements.md#L9). La distinción de alcance y decisiones confirmadas consta en [decisions.md#L14-L52](../../context/decisions.md#L14) y [decisions.md#L126-L142](../../context/decisions.md#L126).

## Propiedad, privacidad y seguridad

- Toda lectura o escritura privada se limita al usuario identificado; esa regla se formaliza en [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- Un usuario no puede operar sobre recursos ajenos ni tomar decisiones de propiedad por `UserId` no verificado; la regla se incorpora a la sustancia del mismo documento.
- Las respuestas de login, registro, recuperación y cambio de email no revelan si una cuenta existe. La política de respuestas genéricas está en [specifications.md → «Respuestas que no revelan si una cuenta existe»](../../context/specifications.md#respuestas-que-no-revelan-si-una-cuenta-existe).
- Los tokens que gestionan cuenta, recuperación y cambio de email se tratan con almacenamiento protegido y caducidad; ver [specifications.md → «Tokens»](../../context/specifications.md#tokens).

La identidad pública por alias y las páginas públicas relacionadas se describen en [requirements.md#L24-L28](../../context/requirements.md#L24) y [requirements.md#L98-L109](../../context/requirements.md#L98). Las invariantes críticas de privacidad y secretos también constan en [AGENTS.md#L64-L71](../../AGENTS.md#L64) (referencia a la guía del repositorio).

## Colecciones y vida del usuario

El dominio exige que un usuario con registro completo tenga siempre al menos una colección; la colección privada «Bandeja de entrada» se crea al completar el registro, según [domain-model.md → «User»](../../context/domain-model.md#user).

Además, la entidad refleja la separación de identidad y contenido personal:

- `Name` no necesita ser único, porque es un dato privado y no forma parte de la identidad pública.
- `Alias` sí es público y forma parte de las direcciones públicas del usuario.
- La propiedad del contenido privado se debe verificar por identidad y coincidencia de `UserId`, no por un `Alias` o `Slug` recibido desde la interfaz.

## Observaciones finales

- No se detectan contradicciones específicas de `User` en el alcance revisado. Las reglas de cuenta y seguridad incluyen trabajo posterior y decisiones pendientes; sus referencias propietarias se conservan arriba y esta ficha no las presenta como implementadas.
- El riesgo residual es documental: esta ficha debe usarse como vista derivada para revisión y no como una segunda fuente de reglas.
- La aprobación final de correspondencia con el código y con las invariantes pertenece a S3.6, según [spec.md → «Alcance»](spec.md#alcance).
