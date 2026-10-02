# Secuencia de cambio de email

Flujo previsto para el MVP1, definido en [specifications.md → «Cambio de email»](../specifications.md#cambio-de-email). Las ramas de rechazo se resumen después del diagrama para mantener la sintaxis de secuencia compatible con el renderizador.

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
    App->>DB: Cargar cuenta autenticada
    App->>Hasher: Verificar contraseña actual
    Hasher-->>App: Contraseña válida
    App->>DB: BEGIN IMMEDIATE
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
    App->>DB: Invalidar tokens de restablecimiento pendientes
    App->>DB: COMMIT
    App-->>Browser: Si la sesión actual es del dueño del token, reemitir cookie con el nuevo SecurityStamp
    OtherSession->>App: Realizar siguiente solicitud con cookie anterior
    App-->>OtherSession: Rechazar sesión cuyo SecurityStamp ya no coincide
    App->>MailQueue: Encolar aviso a la direccion anterior tras el commit
    MailQueue->>SMTP: Enviar aviso de email cambiado
```

La contraseña actual debe verificarse antes de solicitar el cambio. Si el nuevo email está ocupado, no se emite token y se envía un aviso a esa dirección; la respuesta al usuario sigue siendo genérica. Si se supera el límite de correo, no se emite token ni se envía el correo. Si otra cuenta ocupa el nuevo email antes de la confirmación, esta falla con la misma respuesta que un token inválido o caducado. El token en claro solo viaja en el enlace; se almacena su hash. Abrir el enlace solo muestra la página de confirmación; el cambio se aplica con el POST. La sesión desde la que se confirma sigue abierta con la cookie renovada solo si pertenece al dueño del token; las demás dejan de ser válidas por el cambio de `SecurityStamp`.