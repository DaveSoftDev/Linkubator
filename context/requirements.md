# Requisitos de Linkubator

Este documento describe qué hace Linkubator y para quién, en lenguaje de producto. Las reglas exactas (valores, algoritmos, mensajes y códigos de respuesta) están en specifications.md; las entidades y sus invariantes, en domain-model.md; las razones de cada decisión, en decisions.md.

## 1. Propósito

Linkubator es una aplicación web para que cada usuario guarde sus enlaces favoritos, los organice en colecciones y etiquetas, y haga pública, de forma selectiva, parte de ese contenido.

## 2. Contexto del MVP0

- El MVP0 se ejecuta únicamente en local. No depende de servicios externos: la autenticación es local y los correos los captura un servidor SMTP local de desarrollo.
- El producto está preparado para varios usuarios, y cada uno solo ve y modifica sus propios datos.
- La interfaz está en castellano y funciona en los navegadores actuales.
- Toda la zona privada y de cuenta (registro, login, panel de gestión y configuración) cuelga de `/app`. La raíz queda para las páginas públicas.
- Las páginas públicas y sus fundamentos SEO se pueden verificar en local, pero no serán rastreables por buscadores mientras no haya un despliegue público.

## 3. Cuenta de usuario

El usuario podrá:

- Registrarse con nombre, email, alias y contraseña. Tras el registro dispone de una colección privada llamada «Bandeja de entrada» para guardar sus primeros enlaces sin tener que crear una colección.
- Verificar su email mediante un enlace. No podrá iniciar sesión sin haberlo verificado.
- Iniciar sesión, con la opción «Recordarme».
- Cerrar sesión desde cualquier página de la zona privada.
- Editar su configuración: nombre, alias, email y contraseña. Cambiar el email exige su contraseña y verificar la nueva dirección.
- Recuperar su contraseña mediante un enlace enviado a su email.
- Eliminar su cuenta y todos sus datos, reintroduciendo su contraseña.

Garantías para el usuario:

- El alias es público y forma parte de las direcciones de sus páginas públicas. Se le muestra cómo quedará antes de guardarlo.
- Las respuestas de login, registro y recuperación nunca revelan si un email tiene cuenta.
- La contraseña sigue la guía NIST SP 800-63B-4: frases largas, sin reglas de composición y con protección frente a intentos repetidos. El usuario es el único responsable de la fortaleza de la contraseña que elija.
- Recibirá un aviso por correo cuando cambie su contraseña o su email, y cuando elimine su cuenta.

El comportamiento exacto de cada operación está en specifications.md → Cuenta, Contraseñas, Correo y tokens y Sesión.

## 4. Colecciones

El usuario podrá:

- Crear y editar colecciones, con nombre y descripción.
- Hacer pública o privada una colección.
- Eliminar una colección solo si no tiene enlaces y no es la última que le queda.
- Renombrar la «Bandeja de entrada» como cualquier otra colección.

El nombre de una colección es único dentro del usuario. Si el nombre no es válido o coincide con el de otra colección suya, se rechaza y debe elegir otro.

Una colección pública aparece en la página pública del usuario y tiene su propia página pública. Si se renombra, su dirección pública anterior deja de existir.

## 5. Etiquetas

El usuario podrá:

- Crear, editar y eliminar etiquetas. Solo se pueden eliminar las etiquetas sin enlaces.
- Asociar varias etiquetas a un enlace, eligiendo las existentes o creando una nueva desde el propio formulario del enlace.

El nombre de una etiqueta es único dentro del usuario.

## 6. Enlaces

El usuario podrá:

- Crear un enlace a partir de una URL, asignándolo a una única colección. La URL se ajusta y valida antes de guardarse, y solo se aceptan direcciones web (`http` y `https`).
- Editar su título, su descripción, su imagen, si es público o privado y sus etiquetas.
- Mover un enlace a otra colección.
- Eliminar un enlace.

Reglas visibles para el usuario:

- La URL de un enlace no se puede cambiar. Para cambiarla, hay que eliminar el enlace y crearlo de nuevo.
- No se puede guardar dos veces el mismo enlace. Si ya existe, se le indica en qué colección está.
- Hasta la etapa de scraping, el usuario rellena el título, la descripción y la imagen, o los deja vacíos. Después, la aplicación intentará obtenerlos automáticamente de la página, sin sobrescribir nunca lo que ya esté informado.

Las reglas exactas están en specifications.md → URLs de los enlaces y Metadatos y scraping.

## 7. Público y privado

Cada colección y cada enlace es público o privado. Una colección pública aparece en la página pública del usuario; un enlace público aparece en la página pública de su colección. Un enlace solo se ve en público si él y su colección son públicos.

Hacer algo público es siempre una acción explícita del usuario:

- Al crear una colección o un enlace, la casilla «Pública» o «Público» aparece desmarcada, y el usuario puede marcarla antes de guardar.
- En un enlace de una colección privada, la casilla no se puede marcar.
- Al hacer privada una colección, todos sus enlaces pasan a privados. Al volver a hacerla pública, sus enlaces siguen privados.
- Al mover un enlace a otra colección, pasa a privado.

Las invariantes completas están en domain-model.md → Público y privado.

## 8. Búsqueda y filtros

El usuario podrá:

- Buscar texto en sus propios enlaces: URL, título, descripción, nombre de la colección y nombres de las etiquetas. La búsqueda no distingue acentos.
- Filtrar sus enlaces por una colección y/o una etiqueta, y combinar esos filtros con la búsqueda.

Una búsqueda nunca devuelve enlaces de otro usuario. El orden y la paginación están en specifications.md → Búsqueda y filtros y Listados.

## 9. Páginas públicas

Cualquier visitante, sin iniciar sesión, podrá consultar:

- La landing, que explica qué es Linkubator y da acceso al registro y al login.
- La página pública de un usuario, con sus 5 últimas colecciones públicas y sus 5 últimos enlaces públicos (de colecciones públicas).
- La página pública de colecciones, con todas las colecciones públicas del usuario.
- La página pública de una colección, con sus enlaces públicos.

Solo estas páginas son indexables. Un enlace nunca tiene página propia. Las páginas públicas no muestran el nombre ni el email del usuario, y una colección o un enlace privados nunca aparecen en ellas.

El contenido de cada página, sus rutas y sus respuestas están en specifications.md → Páginas públicas.

## 10. SEO

El SEO se aborda por fases. Lo que afecta al dominio, a las rutas, a la distinción entre público y privado y al HTML se define desde el MVP0: páginas públicas legibles por los buscadores, con títulos, descripciones, URL canonical y respuestas `404` para lo que no existe o no es público (ver specifications.md → Páginas públicas). El SEO avanzado queda fuera del MVP0 (ver decisions.md → Fuera de alcance del MVP0).

## 11. Accesibilidad

La aplicación será usable con teclado y con tecnologías de asistencia:

- Orden de tabulación lógico y coincidente con el orden visual.
- Foco visible.
- Primer campo lógico enfocado al entrar en un formulario.
- Labels asociados a los campos.
- Mensajes de error junto al campo (o debajo, si falta espacio) y anunciables por tecnologías de asistencia. El campo con error también queda marcado visualmente.
- Idioma de la página declarado con `lang="es"`.
- Navegación para saltar al contenido principal.
- Landmarks y encabezados semánticos.
- Contraste suficiente.
- No depender únicamente del color.
- Nombres accesibles para enlaces y botones.
- Respeto de `prefers-reduced-motion` para evitar molestias a personas con trastornos vestibulares.
- Soporte para lectores de pantalla.

## 12. Operación

- La aplicación registra desde el inicio los eventos relevantes, sin datos sensibles (ver specifications.md → Registro de eventos).
- La aplicación responde sin quedarse bloqueada esperando al usuario ni a servicios externos (ver architecture.md → Persistencia).

## 13. Fuera de alcance

Ver decisions.md → Fuera de alcance del MVP0.
