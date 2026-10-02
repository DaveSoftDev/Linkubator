# Secuencia de registro y primer uso

Flujo previsto para el MVP1, derivado de las reglas de cuenta y persistencia descritas en [specifications.md](../specifications.md), [domain-model.md](../domain-model.md) y [architecture.md](../architecture.md). En el MVP0, la gestión privada usa la identidad de desarrollo inyectada y no ejecuta este registro ni este login.

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
    App->>DB: Buscar cuenta y comprobar límite de correo
    App->>DB: Crear usuario incompleto con solo el email
    App->>DB: Guardar hash del token y actualizar LastEmailSentAt
    App->>DB: COMMIT
    App->>MailQueue: Encolar correo de completar registro tras el commit
    App-->>Browser: Mostrar respuesta genérica

    MailQueue->>SMTP: Enviar correo por SMTP fuera de la transacción
    Browser->>SMTP: Consultar el correo local y obtener el enlace
    User->>Browser: Abre el enlace de completar registro
    Browser->>App: GET con token en claro
    App->>DB: Buscar hash del token y comprobar que sea válido
    App-->>Browser: Mostrar formulario de nombre, alias y contraseña
    User->>Browser: Introduce datos y envía el formulario
    Browser->>App: POST de compleción con token
    App->>App: Validar datos y normalizar alias
    App->>Hasher: Calcular hash de la contraseña
    Hasher-->>App: Devolver hash
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Volver a validar token y alias
    App->>DB: Fijar nombre, alias, hash y EmailConfirmedAt
    App->>DB: Crear Bandeja de entrada privada y consumir token
    App->>DB: COMMIT
    App-->>Browser: Registro completado. Ir al inicio de sesión

    User->>Browser: Entra con email y contraseña
    Browser->>App: POST de inicio de sesión
    App->>DB: Cargar cuenta completada y su hash guardado
    App->>Hasher: Verificar contraseña
    Hasher-->>App: Contraseña válida
    App-->>Browser: Emitir cookie de autenticación y abrir /app/dashboard

    User->>Browser: Crea un enlace en Bandeja de entrada
    Browser->>App: Enviar URL y metadatos opcionales
    App->>App: Ajustar, validar y normalizar URL
    App->>DB: BEGIN IMMEDIATE
    App->>DB: Comprobar duplicado y propiedad de colección
    App->>DB: Guardar enlace privado y actualizar proyección FTS5
    App->>DB: COMMIT
    App-->>Browser: Redirigir al listado de enlaces

    User->>Browser: Busca el enlace por texto o filtro
    Browser->>App: Solicitar resultados de búsqueda
    App->>DB: Consultar FTS5 y filtrar por usuario autenticado
    DB-->>App: Devolver solo resultados del usuario
    App-->>Browser: Mostrar el enlace
```

La secuencia muestra el recorrido de una cuenta nueva con email disponible y datos válidos. Si el email ya tiene un registro incompleto, se renueva el token; si la cuenta está completada, se envía un aviso sin token. La respuesta al registro es genérica en todos los casos. Si se supera el límite de correo no se emite otro token ni se envía correo, y cualquier token previo sin usar sigue vigente. Si el token no es válido o los datos no superan la validación, no se completa la cuenta; los errores del formulario no consumen el token.

El token en claro solo viaja en el enlace del correo; SQLite conserva su hash. El usuario informa manualmente los metadatos del enlace. La obtención automática de metadatos se incorpora en MVP1. La cuenta solo queda confirmada al completar correctamente el registro; el inicio de sesión se realiza después con normalidad.