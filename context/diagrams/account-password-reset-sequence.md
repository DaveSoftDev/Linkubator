# Secuencia de recuperación y restablecimiento de contraseña

Flujo del camino válido de una cuenta completada, definido en [specifications.md → «Recuperación de contraseña»](../specifications.md#recuperación-de-contraseña).

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
    User->>Browser: Abre enlace de restablecimiento
    Browser->>App: GET con token en claro
    App->>DB: Buscar hash del token y comprobar validez
    alt Token no válido
        App-->>Browser: Mostrar respuesta común de token no válido
    else Token válido
        App-->>Browser: Mostrar formulario sin consumir el token
        User->>Browser: Introduce y envía la nueva contraseña
        Browser->>App: POST con token, contraseña nueva y antiforgery
        App->>App: Validar nueva contraseña
        App->>Hasher: Calcular hash de la nueva contraseña
        Hasher-->>App: Devolver hash
        App->>DB: BEGIN IMMEDIATE
        App->>DB: Volver a validar el token
        alt Token todavía válido
            App->>DB: Guardar nuevo hash y consumir token
            App->>DB: Invalidar tokens pendientes de restablecimiento, cambio de email y eliminación de cuenta
            App->>DB: Reiniciar FailedLoginAttempts y anular LockoutEnd
            App->>DB: Regenerar SecurityStamp y cerrar todas las sesiones
            App->>DB: COMMIT
            App->>MailQueue: Encolar aviso de contraseña cambiada tras el commit
            App-->>Browser: Confirmar restablecimiento
            MailQueue->>SMTP: Enviar aviso de seguridad
        else Token no válido
            App->>DB: ROLLBACK
            App-->>Browser: Mostrar respuesta común de token no válido
        end
    end
```
