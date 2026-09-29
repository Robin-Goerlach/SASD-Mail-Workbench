using System.Xml.Linq;
using NUnit.Framework;

namespace Sasd.MailWorkbench.Notification.Architecture.Tests;

[TestFixture]
public sealed class NotificationArchitectureTests
{
    [Test]
    public void Domain_HasNoProjectOrPackageReferences()
    {
        var project = LoadProject("src/Sasd.MailWorkbench.Notification.Domain/Sasd.MailWorkbench.Notification.Domain.csproj");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetReferences(project, "ProjectReference"), Is.Empty);
            Assert.That(GetReferences(project, "PackageReference"), Is.Empty);
        }
    }

    [Test]
    public void Contracts_ReferencesOnlyNotificationDomain()
    {
        var project = LoadProject("src/Sasd.MailWorkbench.Notification.Contracts/Sasd.MailWorkbench.Notification.Contracts.csproj");
        var references = GetReferences(project, "ProjectReference");

        Assert.That(references, Is.EqualTo(new[]
        {
            "../Sasd.MailWorkbench.Notification.Domain/Sasd.MailWorkbench.Notification.Domain.csproj"
        }));
    }

    [Test]
    public void Application_DoesNotReferenceInfrastructureOrUi()
    {
        var project = LoadProject("src/Sasd.MailWorkbench.Notification.Application/Sasd.MailWorkbench.Notification.Application.csproj");
        var references = GetReferences(project, "ProjectReference");

        var hasForbiddenReference = references.Any(reference =>
            reference.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase) ||
            reference.Contains("Host", StringComparison.OrdinalIgnoreCase) ||
            reference.Contains("WinForms", StringComparison.OrdinalIgnoreCase));

        Assert.That(hasForbiddenReference, Is.False);
    }

    [Test]
    public void TrayHost_DoesNotDirectlyReferenceMailKitOrSqlitePackages()
    {
        var project = LoadProject("src/Sasd.MailWorkbench.Host.Tray/Sasd.MailWorkbench.Host.Tray.csproj");
        var packages = GetReferences(project, "PackageReference");

        var hasForbiddenPackage = packages.Any(package =>
            package.Contains("MailKit", StringComparison.OrdinalIgnoreCase) ||
            package.Contains("MimeKit", StringComparison.OrdinalIgnoreCase) ||
            package.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));

        Assert.That(hasForbiddenPackage, Is.False);
    }

    [Test]
    public void CoreSource_DoesNotReferenceWindowsFormsMailKitOrSqlite()
    {
        var root = FindRepositoryRoot();
        var coreDirectories = new[]
        {
            "src/Sasd.MailWorkbench.Notification.Domain",
            "src/Sasd.MailWorkbench.Notification.Contracts",
            "src/Sasd.MailWorkbench.Notification.Application"
        };

        var forbiddenUsingPrefixes = new[]
        {
            "using System.Windows.Forms",
            "using MailKit",
            "using MimeKit",
            "using Microsoft.Data.Sqlite"
        };

        var violations = new List<string>();
        foreach (var relativeDirectory in coreDirectories)
        {
            var directory = Path.Combine(root, relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                foreach (var token in forbiddenUsingPrefixes.Where(token => content.Contains(token, StringComparison.Ordinal)))
                {
                    violations.Add($"{Path.GetRelativePath(root, file)} enthält die verbotene Abhängigkeit '{token}'.");
                }
            }
        }

        Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
    }

    private static XDocument LoadProject(string relativePath)
    {
        var root = FindRepositoryRoot();
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return XDocument.Load(path);
    }

    private static string[] GetReferences(XDocument project, string elementName) =>
        project.Descendants(elementName)
            .Select(element => element.Attribute("Include")?.Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Sasd.MailWorkbench.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Das SASD-Mail-Workbench-Repository wurde nicht gefunden.");
    }
}
