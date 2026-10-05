# Secuencia de cambio de email

Flujo válido del cambio de email, definido en [specifications.md → «Cambio de email»](../specifications.md#cambio-de-email). Las ramas de rechazo se enumeran después del diagrama.

```mermaid
sequenceDiagram
    participant User as Usuario
    participant Browser as Navegador
    participant App as Aplicación web
    participant DB as SQLite
    participant Hasher as Servicio de hashing
    participant MailQueue as Cola de correo
    participant SMTP as smtp4dev local
    participant OtherSession as Otra sesión

    User->>Browser: Solicita cambiar email e introduce contraseña actual
    Browser->>App: Enviar contraseña actual y nuevo email
    App->>DB: Cargar cuenta autenticada, hash y SecurityStamp
    App->>Hasher: Verificar contraseña actual
    Hasher-->>App: Contraseña válida
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Comprobar que SecurityStamp no cambió y que la cuenta no está bloqueada
    App->>DB: Comprobar disponibilidad del nuevo email y limite de correo
    App->>DB: Guardar token de cambio como hash, NewEmail y LastEmailSentAt
    App->>DB: COMMIT
    App->>MailQueue: Encolar verificacion del nuevo email tras el commit
    App-->>Browser: Respuesta generica sobre la solicitud
    MailQueue->>SMTP: Enviar enlace de verificacion
    User->>Browser: Abre el correo en el entorno local
    Browser->>SMTP: Consultar mensaje capturado
    User->>Browser: Abre enlace de verificacion
    Browser->>App: GET con token en claro
    App->>DB: Buscar hash de token y comprobar validez
    App-->>Browser: Mostrar página de confirmación con el nuevo email
    User->>Browser: Pulsa confirmar el cambio
    Browser->>App: POST con token y antiforgery
    App->>DB: Buscar hash de token y comprobar validez
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Volver a validar el token y confirmar que NewEmail sigue disponible
    App->>DB: Aplicar nuevo email, regenerar SecurityStamp y consumir token
    App->>DB: Invalidar tokens pendientes según specifications.md → «Tokens»
    App->>DB: COMMIT
    App-->>Browser: Si la sesión actual es del dueño del token, reemitir cookie con el nuevo SecurityStamp
    OtherSession->>App: Realizar siguiente solicitud con cookie anterior
    App-->>OtherSession: Rechazar sesión cuyo SecurityStamp ya no coincide
    App->>MailQueue: Encolar aviso a la direccion anterior tras el commit
    MailQueue->>SMTP: Enviar aviso de email cambiado
```

Ramas de rechazo omitidas en el diagrama:

- Si el nuevo email es el mismo que el actual, no se emite token y se informa al usuario.
- Si el nuevo email ya pertenece a otra cuenta, no se emite token y se envía a esa dirección el aviso de cuenta existente; la respuesta al usuario sigue siendo genérica.
- Si la contraseña actual es incorrecta o la cuenta está bloqueada, se rechaza la solicitud según [specifications.md → «Bloqueo por intentos fallidos»](../specifications.md#bloqueo-por-intentos-fallidos); no se continúa con el cambio.
- Si `SecurityStamp` cambió desde que se verificó la contraseña, o la cuenta quedó bloqueada mientras se calculaba el hash, se revierte la transacción y no se emite token ni se cuentan intentos contra el estado nuevo de la cuenta (ver [specifications.md → «Verificación concurrente de contraseña»](../specifications.md#verificación-concurrente-de-contraseña)).
- Si se alcanza el límite de correo, no se emite un token nuevo ni se envía el correo; se aplica [specifications.md → «Correo»](../specifications.md#correo).
- Si el token no es válido al abrir el enlace o al confirmar, se muestra la respuesta común de [specifications.md → «Tokens»](../specifications.md#tokens).
- Si otra cuenta ocupa el nuevo email antes de la confirmación, se muestra esa misma respuesta común.
