$ErrorActionPreference = "Stop"

$repoRoot = (Get-Item (Join-Path $PSScriptRoot "..\..\..")).FullName
$domainProject = Join-Path $repoRoot "src\Linkubator.Domain\Linkubator.Domain.csproj"

if (-not (Test-Path $domainProject)) {
    throw "No se encuentra el proyecto de Domain: $domainProject"
}

# Rellenar con los 20 nombres reales de colecciones que apunte DLG.
# Las entradas deben estar en una lista simple de cadenas. Si no hay nombres, el script termina sin error.
$names = @(
    "C#",
    "C++",
    "F+",
    "Prompts de Viajes ;)",
    "Método GTD",
    "Método Second Brain",
    "Método PARA",
    "Método Second Brain & PARA",
    "Método Zettelkasten",
    "Método GTD & PARA & Zettelkasten",
    "S-O-L-I-D",
    "SEO: ASO",
    "SEO: GEO",
    "Javier Figuerola-Ferreti",
    "Drácula - El empalador",
    "Napoleón Bonaparte",
    "Leonardo da Vinci & Commonplace Book",
    "Castilla y León (España)",
    "Caçadors de bolets",
    "13 straße del percebe",
    "日本語 pa mi bro",
    "ｃｏｎｔａｍｉｎａｃｉóｎ",
    "😀 Emoji Test",
    "ＴＥＳＴ ２０℃"
)

if ($names.Count -eq 0) {
    Write-Host "No hay nombres reales configurados para esta comprobación. Añade los 20 nombres de DLG antes de ejecutar el script." -ForegroundColor Yellow
    exit 0
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "linkubator-names-probe"
if (Test-Path $tempRoot) {
    Remove-Item $tempRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $tempRoot | Out-Null

$projectFile = Join-Path $tempRoot "Linkubator.NamesProbe.csproj"
$programFile = Join-Path $tempRoot "Program.cs"

& dotnet new console --framework net10.0 --output $tempRoot --name "Linkubator.NamesProbe" --force | Out-Null
& dotnet add "$projectFile" reference "$domainProject" | Out-Null

$escapedNames = foreach ($name in $names) {
    '"' + ($name.Replace('"', '\"')) + '"'
}
$namesLiteral = ($escapedNames -join ",`n    ")

$programText = @"
using Linkubator.Domain.Policies;

var names = new[]
{
    $namesLiteral
};

Console.WriteLine($"SDK: {Environment.Version}");
Console.WriteLine($"Nombres a probar: {names.Length}");

for (int i = 0; i < names.Length; i++)
{
    string name = names[i];
    try
    {
        string slug = SlugPolicy.Generate(name);
        Console.WriteLine($"{i + 1}. {name} => {slug}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{i + 1}. {name} => EXCEPTION: {ex.GetType().Name}: {ex.Message}");
    }
}
"@

Set-Content -Path $programFile -Value $programText -Encoding UTF8

& dotnet run --project $projectFile --no-restore --verbosity minimal
$exitCode = $LASTEXITCODE

Remove-Item $tempRoot -Recurse -Force
exit $exitCode
