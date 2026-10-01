# Secuencia de recuperación y restablecimiento de contraseña

Diagrama del camino válido de una cuenta completada, definido en [specifications.md → «Recuperación de contraseña»](../specifications.md#recuperación-de-contraseña).

```mermaid
sequenceDiagram
    participant User as Usuario
    participant Browser as Navegador
    participant App as Aplicación web
    participant DB as SQLite
    participant Hasher as Servicio de hashing
    participant MailQueue as Cola de correo
    participant SMTP as smtp4dev local

    User->>Browser: Solicita recuperar contraseña con su email
    Browser->>App: Enviar email de la cuenta
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Buscar cuenta y comprobar limite de correo
    App->>DB: Guardar hash del token y actualizar LastEmailSentAt
    App->>DB: COMMIT
    App->>MailQueue: Encolar correo de restablecimiento tras el commit
    App-->>Browser: Mostrar respuesta genérica
    MailQueue->>SMTP: Enviar enlace de restablecimiento
    User->>Browser: Abre el correo en el entorno local
    Browser->>SMTP: Consultar mensaje capturado
    User->>Browser: Abre enlace e introduce nueva contraseña
    Browser->>App: Enviar token y nueva contraseña
    App->>DB: Buscar hash del token y comprobar validez
    App->>App: Validar nueva contraseña
    App->>Hasher: Calcular hash de la nueva contraseña
    Hasher-->>App: Devolver hash
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Volver a validar el token
    App->>DB: Guardar nuevo hash y consumir token
    App->>DB: Invalidar tokens pendientes de recuperacion y cambio de email
    App->>DB: Reiniciar FailedLoginAttempts y anular LockoutEnd
    App->>DB: Regenerar SecurityStamp y cerrar todas las sesiones
    App->>DB: COMMIT
    App->>MailQueue: Encolar aviso de contraseña cambiada tras el commit
    App-->>Browser: Confirmar restablecimiento
    MailQueue->>SMTP: Enviar aviso de seguridad
```

La solicitud siempre recibe la misma respuesta, exista o no una cuenta. Para una cuenta completada se envía el enlace de restablecimiento; para un registro sin completar se envía el correo de completar registro; si no existe cuenta, no se envía correo. Si se supera el límite de correo, no se emite token ni se envía mensaje. Un token inválido, usado o caducado no permite completar el cambio. El token en claro solo viaja en el enlace; se almacena su hash. El restablecimiento cierra todas las sesiones.