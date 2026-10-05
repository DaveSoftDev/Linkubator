# Secuencia de eliminación de cuenta

Flujo de eliminación de cuenta, definido en [specifications.md → «Eliminación de cuenta»](../specifications.md#eliminación-de-cuenta).

```mermaid
sequenceDiagram
    actor User as Usuario
    participant Browser as Navegador
    participant App as Aplicación web
    participant DB as SQLite
    participant Hasher as Servicio de hashing
    participant MailQueue as Cola de correo
    participant SMTP as smtp4dev local

    User->>Browser: Solicita eliminar la cuenta e introduce la contraseña actual
    Browser->>App: POST de solicitud con contraseña y antiforgery
    App->>DB: Cargar cuenta autenticada, hash y SecurityStamp
    App->>Hasher: Verificar contraseña actual
    Hasher-->>App: Contraseña válida
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Comprobar que SecurityStamp no cambió y que la cuenta no está bloqueada
    App->>DB: Comprobar límite de correo con token
    App->>DB: Guardar hash del token de eliminación y actualizar LastEmailSentAt
    App->>DB: COMMIT
    App->>MailQueue: Encolar enlace de confirmación tras el commit
    App-->>Browser: Indicar que debe confirmar la eliminación
    MailQueue->>SMTP: Enviar enlace al email actual
    User->>Browser: Abre el enlace del correo
    Browser->>App: GET con token en claro
    App->>DB: Buscar hash del token y comprobar validez
    App-->>Browser: Mostrar página de confirmación sin consumir el token
    User->>Browser: Confirma explícitamente la eliminación
    Browser->>App: POST con token y antiforgery
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Volver a validar el token
    App->>DB: Borrar usuario, datos asociados y tokens
    App->>DB: COMMIT
    App-->>Browser: Cerrar la sesión
    App->>MailQueue: Encolar aviso de cuenta eliminada tras el commit
    MailQueue->>SMTP: Enviar aviso al email que tenía la cuenta
```

Si la contraseña no es válida, no se emite el token. Si `SecurityStamp` cambió desde que se verificó la contraseña o la cuenta quedó bloqueada mientras se calculaba el hash, se revierte la transacción y no se emite token ni se cuentan intentos contra el estado nuevo de la cuenta (ver [specifications.md → «Verificación concurrente de contraseña»](../specifications.md#verificación-concurrente-de-contraseña)). Si se alcanza el límite de correo, no se emite uno nuevo ni se envía el mensaje; los tokens anteriores conservan su vigencia según [specifications.md → «Correo»](../specifications.md#correo) y [«Tokens»](../specifications.md#tokens). El GET no elimina la cuenta ni consume el token; la operación destructiva solo ocurre en el POST explícito.
