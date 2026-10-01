# Normalización de URL para detectar duplicados

Diagrama del proceso de `Link.UrlNormalized`, definido en [specifications.md → «Normalización para duplicados»](../specifications.md#normalización-para-duplicados).

```mermaid
flowchart TD
    start([URL ajustada y validada])
    parse["Analizar con System.Uri y separar los componentes sobre la URL codificada"]
    fragment["Descartar fragmento"]
    portChoice{"¿Puerto predeterminado del esquema escrito explícitamente?"}
    discardPort["Eliminar puerto"]
    keepPort["Conservar el puerto no predeterminado, si existe"]
    structure["Descartar esquema y tomar el host en formato IdnHost"]
    path["Ruta: decodificar una vez con decodificación URI<br/>El signo + es literal"]
    query["Consulta: separar parámetros y decodificar una vez<br/>cada nombre y valor con decodificación de formularios<br/>El signo + equivale a espacio"]
    removeMarketing["Eliminar parámetros de marketing<br/>lista definida en specifications.md"]
    lowercaseParams["Pasar a minúsculas nombres y valores restantes"]
    sortParams["Ordenar por nombre y valor con comparación ordinal<br/>Conservar parámetros repetidos"]
    combine["Unir host, puerto conservado, ruta y parámetros ordenados"]
    lowercaseAll["Pasar toda la cadena a minúsculas"]
    accents["Eliminar acentos y marcas diacríticas conservando la letra base<br/>Sustituir ç por c y ñ por n"]
    spaces["Convertir espacios en guiones medios"]
    allowed["Conservar solo letras y números de cualquier alfabeto y guiones medios<br/>Eliminar los demás caracteres"]
    collapse["Eliminar guiones medios duplicados"]
    trim["Eliminar guiones medios iniciales y finales"]
    result([UrlNormalized])

    start --> parse --> fragment --> portChoice
    portChoice -->|Sí| discardPort --> structure
    portChoice -->|No| keepPort --> structure
    structure --> path
    structure --> query
    path --> combine
    query --> removeMarketing --> lowercaseParams --> sortParams --> combine
    combine --> lowercaseAll --> accents --> spaces --> allowed --> collapse --> trim --> result
```