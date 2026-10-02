# Requisitos de Linkubator

Este documento describe qué hace Linkubator y para quién, en lenguaje de producto. Las reglas exactas (valores, algoritmos, mensajes y códigos de respuesta) están en [specifications.md](specifications.md); las entidades y sus invariantes, en [domain-model.md](domain-model.md); las razones de cada decisión, en [decisions.md](decisions.md).

## Propósito

Linkubator es una aplicación web para que cada usuario guarde sus enlaces favoritos, los organice en colecciones y etiquetas, y haga pública, de forma selectiva, parte de ese contenido.

## Contexto del MVP0

- El MVP0 se ejecuta únicamente en local y no incluye autenticación real ni envío de correo. La zona privada recibe desde código una identidad de desarrollo que representa a un usuario válido; el detalle técnico está en [architecture.md → «Autenticación»](architecture.md#autenticación).
- El producto está preparado para varios usuarios, y cada uno solo ve y modifica sus propios datos.
- La interfaz está en castellano y funciona en los navegadores actuales.
- La zona privada tiene un dashboard; sus rutas se definen en [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas). En el MVP1, las páginas de cuenta también estarán bajo esa ruta; la raíz queda para las páginas públicas.
- Las páginas públicas y sus fundamentos SEO se pueden verificar en local, pero no serán rastreables por buscadores mientras no haya un despliegue público.

## Identidad para la gestión privada

En el MVP0, el usuario podrá gestionar sus colecciones y enlaces desde el área privada sin iniciar sesión. El dashboard le permite iniciar la creación, buscar enlaces y acceder a los listados de enlaces y colecciones. Desde los listados se accede a la edición y a la eliminación. Linkubator le asigna una identidad de desarrollo desde código, y todas las operaciones privadas se realizan en nombre de ese usuario, sin aceptar su identidad desde datos enviados por el navegador. Así se pueden preparar y probar los casos de uso y la propiedad de los datos antes de incorporar la autenticación real en el MVP1. La gestión de etiquetas se incorpora en el MVP1.

## Cuenta de usuario en el MVP1

Con autenticación real, el usuario podrá:

- Registrarse con solo su email. Recibe un correo con un enlace para completar el registro con su nombre, su alias y su contraseña. Al completarlo dispone de una colección privada llamada «Bandeja de entrada» para guardar sus primeros enlaces sin tener que crear una colección.
- No podrá iniciar sesión hasta completar el registro. Si no recibe el correo, basta con registrarse de nuevo con el mismo email para que se le reenvíe.
- Iniciar sesión, con la opción «Recordarme».
- Cerrar sesión desde cualquier página de la zona privada.
- Editar su configuración: nombre, alias, email y contraseña. Cambiar el email exige su contraseña y verificar la nueva dirección.
- Recuperar su contraseña mediante un enlace enviado a su email.
- Eliminar su cuenta y todos sus datos, reintroduciendo su contraseña.

Garantías para el usuario:

- El alias es público y forma parte de las direcciones de sus páginas públicas. Se le muestra cómo quedará antes de guardarlo. El campo solo admite letras, números y guiones.
- Las respuestas de login, registro, recuperación y cambio de email nunca revelan si un email tiene cuenta.
- La contraseña sigue la guía NIST SP 800-63B-4: frases largas, sin reglas de composición y con protección frente a intentos repetidos. El usuario es el único responsable de la fortaleza de la contraseña que elija.
- Puede mostrar u ocultar lo que escribe en cualquier campo de contraseña (ver [specifications.md → «Contraseñas»](specifications.md#contraseñas)).
- Recibirá un aviso por correo cuando cambie su contraseña o su email, y cuando elimine su cuenta.

El comportamiento exacto de cada operación está en [specifications.md → «Cuenta»](specifications.md#cuenta), [«Contraseñas»](specifications.md#contraseñas), [«Correo»](specifications.md#correo), [«Tokens»](specifications.md#tokens) y [«Sesión»](specifications.md#sesión).

## Colecciones

El usuario podrá:

- Crear y editar colecciones, con nombre y descripción.
- Hacer pública o privada una colección.
- Eliminar una colección solo si no tiene enlaces y no es la última que le queda.
- Renombrar la «Bandeja de entrada» como cualquier otra colección.

No puede haber dos colecciones del usuario con nombres equivalentes: se consideran el mismo nombre los que solo se diferencian en mayúsculas, acentos o símbolos (por ejemplo, «Papá» y «papa»). Si el nombre no es válido o equivale al de otra colección suya, se rechaza indicándole con cuál coincide, y debe elegir otro.

Una colección pública con al menos un enlace público tiene su propia página pública y aparece en la página de colecciones del usuario. Una colección pública sin enlaces públicos no se muestra en ninguna página pública hasta que tenga uno; la zona privada se lo indica al usuario. Si se renombra la colección, su dirección pública anterior deja de existir.

## Etiquetas (MVP1)

El usuario podrá:

- Crear, editar y eliminar etiquetas. Solo se pueden eliminar las etiquetas sin enlaces.
- Asociar varias etiquetas a un enlace, eligiendo las existentes o creando una nueva desde el propio formulario del enlace.

No puede haber dos etiquetas del usuario con nombres equivalentes, con el mismo criterio que en las colecciones.

## Enlaces

El usuario podrá:

- Crear un enlace a partir de una URL, asignándolo a una única colección. La URL se ajusta y valida antes de guardarse, y solo se aceptan direcciones web (`http` y `https`).
- Editar su título, su descripción, su imagen y si es público o privado. Desde el MVP1, también podrá editar sus etiquetas.
- Mover un enlace a otra colección.
- Eliminar un enlace.

Reglas visibles para el usuario:

- La URL de un enlace no se puede cambiar. Para cambiarla, hay que eliminar el enlace y crearlo de nuevo.
- La imagen de un enlace aparece en las páginas públicas. Si el usuario introduce una dirección de imagen que no cumple sus reglas, la operación se rechaza y se le indica que la corrija (ver [specifications.md → «Reglas adicionales para `Image`»](specifications.md#reglas-adicionales-para-image)).
- No se puede guardar dos veces el mismo enlace. Si ya existe, se le indica en qué colección está.
- En el MVP0, el usuario rellena el título, la descripción y la imagen, o los deja vacíos. En el MVP1, la aplicación intentará obtenerlos automáticamente de la página, sin sobrescribir nunca lo que ya esté informado.

Las reglas exactas están en [specifications.md → «URLs de los enlaces»](specifications.md#urls-de-los-enlaces) y [«Metadatos y scraping»](specifications.md#metadatos-y-scraping).

## Público y privado

Cada colección y cada enlace es público o privado. Un enlace solo se ve en público si él y su colección son públicos, y una colección solo se ve en público si es pública y tiene al menos un enlace público (ver [domain-model.md → «Público y privado»](domain-model.md#público-y-privado)).

Hacer algo público es siempre una acción explícita del usuario: todo nace privado y la casilla «Pública» o «Público» aparece desmarcada al crear. Al crear un enlace, la opción de hacerlo público solo está habilitada si su colección es pública; si es privada, queda deshabilitada y se informa de que no se puede crear ahí un enlace público. Una petición manipulada que intente hacerlo se rechaza. Ninguna otra operación (hacer pública una colección, mover un enlace) hace público un enlace por su cuenta. Hacer privada una colección hace privados todos sus enlaces. Las reglas completas están en [domain-model.md → «Público y privado»](domain-model.md#público-y-privado).

## Búsqueda y filtros

En el MVP0, el usuario podrá:

- Buscar texto en sus propios enlaces: URL, título, descripción y nombre de la colección. La búsqueda no distingue acentos.
- Filtrar sus enlaces por colección y combinar ese filtro con la búsqueda.
- Buscar sus colecciones por nombre desde el listado de colecciones.

En el MVP1, la búsqueda de enlaces también incluirá los nombres de las etiquetas y se podrá filtrar por etiqueta, combinando ese filtro con el resto de la búsqueda. Además, el usuario podrá buscar sus etiquetas por nombre desde el listado de etiquetas.

Cada listado busca únicamente su propia entidad: buscar en el listado de enlaces solo devuelve enlaces, en el de colecciones solo colecciones y en el de etiquetas solo etiquetas, sin mezclarlos. Una búsqueda nunca devuelve elementos de otro usuario. Las reglas exactas, el orden y la paginación están en [specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros) y [«Listados»](specifications.md#listados).

## Páginas públicas

En el MVP0, cualquier visitante podrá consultar sin iniciar sesión:

- La landing, que explica qué es Linkubator y, en el MVP0, ofrece un botón para entrar en la zona privada con la identidad de desarrollo. El registro y el login reales se incorporan en el MVP1.
- La página pública de un usuario, con sus enlaces públicos recientes, después sus colecciones públicas recientes y un acceso a todas sus colecciones públicas (ver [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas)).
- La página pública de colecciones, con todas las colecciones del usuario que se muestran en público.
- La página pública de una colección, con sus enlaces públicos.

Solo estas páginas son indexables. Un enlace nunca tiene página propia. Las páginas públicas no muestran el nombre ni el email del usuario, y una colección o un enlace privados nunca aparecen en ellas.

El contenido de cada página, sus rutas y sus respuestas están en [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas).

## SEO

El SEO se aborda por fases. Lo que afecta al dominio, a las rutas, a la distinción entre público y privado y al HTML se define desde el MVP0: páginas públicas legibles por los buscadores, con títulos, descripciones y URL canonical, y que responden como inexistentes ante lo que no existe o no es público (ver [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas)). El SEO avanzado queda fuera del MVP0 (ver [decisions.md → «Fuera de alcance del MVP0»](decisions.md#fuera-de-alcance-del-mvp0)).

## Accesibilidad

La aplicación será usable con teclado y con tecnologías de asistencia:

- Orden de tabulación lógico y coincidente con el orden visual.
- Foco visible.
- Primer campo lógico enfocado al entrar en un formulario.
- Labels asociados a los campos.
- Mensajes de error junto al campo (o debajo, si falta espacio) y anunciables por tecnologías de asistencia. El campo con error también queda marcado visualmente.
- Idioma de la página declarado (castellano).
- Navegación para saltar al contenido principal.
- Landmarks y encabezados semánticos.
- Contraste suficiente.
- No depender únicamente del color.
- Nombres accesibles para enlaces y botones.
- Respeto de `prefers-reduced-motion` para evitar molestias a personas con trastornos vestibulares.
- Soporte para lectores de pantalla.
- Estados vacíos con mensajes accesibles.

## Operación

- La aplicación registra desde el inicio los eventos relevantes, sin datos sensibles (ver [specifications.md → «Registro de eventos»](specifications.md#registro-de-eventos)).
- La aplicación responde sin quedarse bloqueada esperando al usuario ni a servicios externos (ver [architecture.md → «Persistencia»](architecture.md#persistencia)).

## Fuera de alcance

Ver [decisions.md → «Fuera de alcance del MVP0»](decisions.md#fuera-de-alcance-del-mvp0).
