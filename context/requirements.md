# Requisitos de Linkubator

Este documento describe qué hace Linkubator y para quién, en lenguaje de producto. Las reglas exactas (valores, algoritmos, mensajes y códigos de respuesta) están en [specifications.md](specifications.md); las entidades y sus invariantes, en [domain-model.md](domain-model.md); las razones de cada decisión, en [decisions.md](decisions.md).

## Propósito

Linkubator es una aplicación web para que cada usuario guarde sus enlaces favoritos, los organice en colecciones y etiquetas, y haga pública, de forma selectiva, parte de ese contenido.

## Cuenta de usuario

Con autenticación real, el usuario podrá:

- Registrarse con solo su email. Recibe un correo con un enlace para completar el registro con su nombre, su alias y su contraseña. Al completarlo dispone de una colección privada llamada «Bandeja de entrada» para guardar sus primeros enlaces sin tener que crear una colección.
- No podrá iniciar sesión hasta completar el registro. Si no recibe el correo, basta con registrarse de nuevo con el mismo email para que se le reenvíe.
- Iniciar sesión, con la opción «Recordarme».
- Cerrar sesión desde cualquier página de la zona privada.
- Editar su configuración: nombre, alias, email y contraseña. Cambiar el email exige su contraseña y verificar la nueva dirección.
- Recuperar su contraseña mediante un enlace enviado a su email.
- Eliminar su cuenta y todos sus datos: debe confirmarlo introduciendo su contraseña y confirmar la eliminación desde un enlace enviado a su email.

Garantías para el usuario:

- El alias es público y forma parte de las direcciones de sus páginas públicas. Se le muestra cómo quedará antes de guardarlo.
- Las respuestas de login, registro, recuperación y cambio de email nunca revelan si un email tiene cuenta.
- El usuario elige una contraseña, que se valida y protege según las reglas de [specifications.md → «Contraseñas»](specifications.md#contraseñas).
- Puede mostrar u ocultar lo que escribe en cualquier campo de contraseña (ver [specifications.md → «Contraseñas»](specifications.md#contraseñas)).
- Recibirá un aviso por correo cuando cambie su contraseña o su email, y cuando elimine su cuenta.
- La sesión tiene una duración limitada; «Recordarme» determina si se mantiene al cerrar el navegador, sin ampliar esa duración (ver [specifications.md → «Sesión»](specifications.md#sesión)).

El comportamiento exacto de cada operación está en [specifications.md → «Cuenta»](specifications.md#cuenta), [«Contraseñas»](specifications.md#contraseñas), [«Correo»](specifications.md#correo), [«Tokens»](specifications.md#tokens) y [«Sesión»](specifications.md#sesión).

## Colecciones

El usuario podrá:

- Crear y editar colecciones, con nombre y descripción.
- Buscar colecciones.
- Hacer pública o privada una colección.
- Eliminar una colección solo si no tiene enlaces y no es la última que le queda.

La colección «Bandeja de entrada» es una colección normal como cualquier otra, aunque sea creada por la aplicación. El usuario podrá actuar sobre esta colección igual que sobre cualquier otra. La podrá:
- Editarla.
- Cambiarle la privacidad.
- Eliminarla.
- Asignarle enlaces

No puede haber dos colecciones del usuario con nombres equivalentes: se consideran el mismo nombre los que solo se diferencian en mayúsculas, acentos (por ejemplo, «Papá» y «papa»). Si el nombre no es válido o equivale al de otra colección suya, se rechaza indicándole con cuál coincide, y debe elegir otro.

Una colección pública con al menos un enlace público tiene su propia página pública y aparece en la página de colecciones del usuario. Una colección pública sin enlaces públicos no se muestra en ninguna página pública hasta que tenga uno. Si se renombra la colección, su dirección pública anterior deja de existir.

## Etiquetas

El usuario podrá:

- Crear, editar y eliminar etiquetas. Solo se pueden eliminar las etiquetas sin enlaces.
- Buscar etiquetas.
- Asociar varias etiquetas a un enlace, eligiendo las existentes o creando una nueva desde el propio formulario del enlace.

No puede haber dos etiquetas del usuario con nombres equivalentes, se resuelve con el mismo criterio que en las colecciones.

## Enlaces

El usuario podrá:

- Crear un enlace a partir de una URL, asignándolo a una única colección y asignando etiquetas. La URL se ajusta y valida antes de guardarse (ver [specifications.md → «Ajuste de URL»](specifications.md#ajuste-de-url) y [specifications.md → «Validación»](specifications.md#validación)).
- Editar su título, su descripción, su imagen, su colección, sus etiquetas y si es público o privado.
- Mover un enlace a otra colección.
- Ejecutar el scrapeo de los metadatos.
- Eliminar un enlace.
- Al lado del campo de URL, hay un botón para poder lanzar el scraping automático de forma manual.

Reglas visibles para el usuario:

- La URL de un enlace no se puede cambiar. Para cambiarla, hay que eliminar el enlace y crearlo de nuevo.
- La imagen de un enlace aparece en las páginas públicas. Si el usuario introduce una dirección de imagen que no cumple sus reglas, la operación se rechaza y se le indica que la corrija (ver [specifications.md → «Ajuste de `Image`»](specifications.md#ajuste-de-image)).
- No se puede guardar un enlace cuya URL se considere equivalente a la de otro tras normalizarla, aunque las URL originales sean distintas. Si ya existe, se indica en qué colección está (ver [specifications.md → «Normalización para duplicados»](specifications.md#normalización-para-duplicados)).
- El usuario rellena el título, la descripción y la imagen, o los deja vacíos.

Las reglas exactas están en [specifications.md → «URLs de los enlaces»](specifications.md#urls-de-los-enlaces) y [«Scraping»](specifications.md#scraping).

## Público y privado

El usuario puede hacer públicas o privadas sus colecciones y sus enlaces. En las páginas públicas aparecen los enlaces que ha hecho públicos y que pertenecen a colecciones públicas. Una colección aparece en las páginas públicas cuando es pública y contiene al menos un enlace público. Las reglas completas están en [domain-model.md → «Público y privado»](domain-model.md#público-y-privado).

## Búsqueda y filtros

El usuario podrá:

- Buscar texto en sus propios enlaces: URL, título, descripción, nombre de la colección y etiquetas.
- Filtrar sus enlaces por colección, por etiquetas o combinar esos filtros con la búsqueda.
- Buscar sus colecciones por nombre desde el listado de colecciones.
- Buscar sus etiquetas por nombre desde el listado de etiquetas.

Cada listado busca únicamente su propia entidad: buscar en el listado de enlaces solo devuelve enlaces, en el de colecciones solo colecciones y en el de etiquetas solo etiquetas, sin mezclarlos. Una búsqueda nunca devuelve elementos de otro usuario. Las reglas exactas, el orden y la paginación están en [specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros) y [«Listados»](specifications.md#listados).

Cualquier página (Pública o privada) que reciba un parámetro de URL que no le corresponde, debe hacer caso omiso a su valor.

## Páginas públicas

Cualquier visitante podrá consultar sin iniciar sesión:

- La landing, que explica qué es Linkubator y ofrece dos botones, uno para entrar en la zona privada (`Login`) y otro para registrarse como usuario en Linkubator (`Registro`).
- La página pública de un usuario, con sus enlaces públicos recientes, después sus colecciones públicas recientes y un acceso a todas sus colecciones públicas (ver [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas)).
- La página pública de colecciones, con todas las colecciones del usuario que se muestran en público.
- La página pública de una colección, con sus enlaces públicos.

Solo estas páginas son indexables. Las páginas públicas no muestran el nombre ni el email del usuario, y una colección o un enlace privados nunca aparecen en ellas.

El contenido de cada página, sus rutas y sus respuestas están en [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas).

## Páginas privadas

Cualquier usuario registrado e identificado tendrá acceso a la zona privada y podrá:

- Acceder a su dashboard; sus rutas se definen en [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).
- Gestionar sus enlaces, colecciones, etiquetas y su usuario.
- Las páginas privadas siguen una ruta común; sus detalles se definen en [specifications.md → «Rutas privadas»](specifications.md#rutas-privadas).

## SEO

El SEO se aborda por fases. Lo que afecta al dominio, a las rutas, a la distinción entre público y privado y al HTML se define desde el inicio: páginas públicas legibles por los buscadores, con títulos, descripciones y URL canonical, y que responden como inexistentes ante lo que no existe o no es público (ver [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas)). 

## Accesibilidad

La aplicación será usable con teclado y con tecnologías de asistencia:

- Todas las páginas, públicas y privadas, declaran su idioma en castellano (regla técnica en [specifications.md → «Idioma de las páginas»](specifications.md#idioma-de-las-páginas)).
- Orden de tabulación lógico y coincidente con el orden visual.
- Foco visible.
- Primer campo lógico enfocado al entrar en un formulario.
- Labels asociados a los campos.
- Mensajes de error junto al campo (o debajo, si falta espacio) y anunciables por tecnologías de asistencia. El campo con error también queda marcado visualmente. Después, el foco lo recibirá el primer campo con error.
- Navegación para saltar al contenido principal.
- Landmarks y encabezados semánticos.
- Contraste de color suficiente según **WCAG AA**.
- No depender únicamente del color.
- Nombres accesibles para enlaces y botones.
- Respeto de `prefers-reduced-motion` para evitar molestias a personas con trastornos vestibulares.
- Soporte para lectores de pantalla.
- Estados vacíos con mensajes accesibles.

## Operación

- La aplicación registra desde el inicio los eventos relevantes, sin datos sensibles (ver [specifications.md → «Registro de eventos»](specifications.md#registro-de-eventos)).
- La aplicación responde sin quedarse bloqueada esperando al usuario ni a servicios externos (ver [architecture.md → «Persistencia»](architecture.md#persistencia)).
