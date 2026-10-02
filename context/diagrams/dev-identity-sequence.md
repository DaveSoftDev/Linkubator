# Secuencia de la identidad de desarrollo

Flujo del MVP0, derivado de [architecture.md → «Identidad de desarrollo»](../architecture.md#identidad-de-desarrollo). En el MVP1 este proveedor se sustituye por la sesión autenticada.

```mermaid
sequenceDiagram
    actor Dev as Desarrollador
    participant Script as SQL local especial
    participant DB as SQLite
    participant App as Aplicacion web
    participant Config as Configuracion local
    participant Provider as Proveedor de identidad de desarrollo
    actor User as Usuario
    participant Browser as Navegador

    Dev->>DB: Ejecutar los scripts SQL versionados
    Dev->>Script: Ejecutar el SQL local especial
    Script->>DB: Crear el usuario completado si no existe
    Note over Script,DB: Email, nombre, alias, hash de una contraseña<br/>aleatoria descartada, EmailConfirmedAt y SecurityStamp
    Script->>DB: Crear su Bandeja de entrada privada si no existe

    Dev->>App: Arrancar en el entorno de desarrollo
    App->>Provider: Registrar el proveedor solo en desarrollo
    Provider->>Config: Leer el email del usuario de desarrollo
    Provider->>DB: Buscar el usuario por email
    alt No existe o su registro no está completado
        Provider-->>App: Error de arranque
        App-->>Dev: La aplicación no arranca
    else Usuario completado
        Provider-->>App: Conservar su UserId
    end

    User->>Browser: Entra en la zona privada desde la landing
    Browser->>App: Petición a /app/dashboard sin identidad del cliente
    App->>Provider: Obtener el usuario identificado
    Provider-->>App: UserId del usuario de desarrollo
    App->>DB: Operación limitada a ese UserId
    App-->>Browser: Mostrar la zona privada
```
