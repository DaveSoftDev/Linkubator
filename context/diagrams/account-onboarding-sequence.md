# Secuencia de registro y primer uso

Flujo derivado de las reglas de cuenta y persistencia descritas en [specifications.md](../specifications.md), [domain-model.md](../domain-model.md) y [architecture.md](../architecture.md).

```mermaid
sequenceDiagram
    actor User as Usuario
    participant Browser as Navegador
    participant App as Aplicacion web
    participant DB as SQLite
    participant Hasher as Servicio de hashing
    participant MailQueue as Cola en memoria y BackgroundService
    participant SMTP as smtp4dev local

    User->>Browser: Introduce email y solicita registro
    Browser->>App: POST de registro con email
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Buscar cuenta por email y comprobar el límite aplicable
    alt No existe una cuenta
        App->>DB: Crear registro incompleto con solo el email
        opt Se permite enviar correo con token
            App->>DB: Emitir token y actualizar LastEmailSentAt
            App->>DB: Guardar el hash del token
        end
    else Existe un registro sin completar
        opt Se permite enviar correo con token
            App->>DB: Emitir token nuevo y actualizar LastEmailSentAt
            App->>DB: Guardar el hash del token
        end
    else Existe una cuenta completada
        opt Se permite enviar aviso de cuenta existente
            App->>DB: Preparar aviso y actualizar LastExistingAccountNoticeAt
        end
    end
    App->>DB: COMMIT
    opt Se preparó un correo
        App->>MailQueue: Encolar correo o aviso tras el commit
    end
    App-->>Browser: Mostrar respuesta genérica
    opt Correo encolado
        MailQueue->>SMTP: Enviar el mensaje en segundo plano, fuera de la petición y la transacción
    end

    opt Se emitió un token de completar registro
        Browser->>SMTP: Consultar el correo local y obtener el enlace
        User->>Browser: Abre el enlace de completar registro
        Browser->>App: GET con token en claro
        App->>DB: Buscar hash del token y comprobar que sea válido
        App-->>Browser: Mostrar formulario de nombre, alias y contraseña sin consumir el token
        User->>Browser: Introduce datos y envía el formulario
        Browser->>App: POST de compleción con token y antiforgery
        App->>App: Validar datos y normalizar alias
        App->>Hasher: Calcular hash de la contraseña
        Hasher-->>App: Devolver hash
        App->>DB: BEGIN IMMEDIATE
        App->>DB: Volver a validar token y alias
        App->>DB: Fijar nombre, alias, hash y EmailConfirmedAt
        App->>DB: Crear Bandeja de entrada privada y consumir token
        App->>DB: COMMIT
        App-->>Browser: Registro completado. Ir al inicio de sesión
    end

    User->>Browser: Entra con email y contraseña
    Browser->>App: POST de inicio de sesión
    App->>DB: Cargar cuenta completada, hash y SecurityStamp
    App->>Hasher: Verificar contraseña
    Hasher-->>App: Contraseña válida
    App->>DB: Volver a leer SecurityStamp y LockoutEnd
    App-->>Browser: Si coinciden y no hay bloqueo, emitir cookie con el SecurityStamp comprobado y abrir /app/dashboard
```
