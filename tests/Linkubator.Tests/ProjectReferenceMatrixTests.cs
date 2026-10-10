using System.Xml.Linq;

namespace Linkubator.Tests;

public class ProjectReferenceMatrixTests
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedProjectReferences =
        new Dictionary<string, string[]>
        {
            ["src/Linkubator.Domain/Linkubator.Domain.csproj"] = [],
            ["src/Linkubator.Application/Linkubator.Application.csproj"] =
            [
                "src/Linkubator.Domain/Linkubator.Domain.csproj"
            ],
            ["src/Linkubator.Infrastructure/Linkubator.Infrastructure.csproj"] =
            [
                "src/Linkubator.Application/Linkubator.Application.csproj",
                "src/Linkubator.Domain/Linkubator.Domain.csproj"
            ],
            ["src/Linkubator.Web/Linkubator.Web.csproj"] =
            [
                "src/Linkubator.Application/Linkubator.Application.csproj",
                "src/Linkubator.Infrastructure/Linkubator.Infrastructure.csproj"
            ],
            ["tests/Linkubator.Tests/Linkubator.Tests.csproj"] =
            [
                "src/Linkubator.Application/Linkubator.Application.csproj",
                "src/Linkubator.Domain/Linkubator.Domain.csproj",
                "src/Linkubator.Infrastructure/Linkubator.Infrastructure.csproj",
                "src/Linkubator.Web/Linkubator.Web.csproj"
            ]
        };

    [Fact]
    public void ProjectReferencesMatchTheAllowedMatrix()
    {
        string repositoryRoot = FindRepositoryRoot();

        foreach (KeyValuePair<string, string[]> matrixEntry in AllowedProjectReferences)
        {
            string? projectPath = Path.Combine(repositoryRoot, ToPlatformPath(matrixEntry.Key));
            string? projectDirectory = Path.GetDirectoryName(projectPath)!;
            string[]? actualReferences = XDocument.Load(projectPath)
                .Descendants("ProjectReference")
                .Select(projectReference => Path.GetFullPath(Path.Combine(
                    projectDirectory,
                    projectReference.Attribute("Include")!.Value)))
                .OrderBy(referencePath => referencePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            string[]? expectedReferences = matrixEntry.Value
                .Select(referencePath => Path.GetFullPath(Path.Combine(
                    repositoryRoot,
                    ToPlatformPath(referencePath))))
                .OrderBy(referencePath => referencePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Assert.Equal(expectedReferences, actualReferences);
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Linkubator.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("No se encontró Linkubator.sln desde el directorio de tests.");
    }

    private static string ToPlatformPath(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }
}