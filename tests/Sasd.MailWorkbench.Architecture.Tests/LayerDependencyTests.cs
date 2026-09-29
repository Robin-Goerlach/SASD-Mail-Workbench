using System.Reflection;
using NUnit.Framework;
using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Domain.ValueObjects;
using Sasd.MailWorkbench.ExtensionModel;
using Sasd.MailWorkbench.Infrastructure.Services;
using Sasd.MailWorkbench.Persistence.Repositories;

namespace Sasd.MailWorkbench.Architecture.Tests;

[TestFixture]
public sealed class LayerDependencyTests
{
    [Test]
    public void Domain_DoesNotReferenceApplicationInfrastructureOrPersistence()
    {
        string[] forbidden = [
            "Sasd.MailWorkbench.Application",
            "Sasd.MailWorkbench.Infrastructure",
            "Sasd.MailWorkbench.Persistence",
            "Microsoft.Data.Sqlite",
            "System.Windows.Forms"
        ];
        AssertNoReferences(typeof(MessageFingerprint).Assembly, forbidden);
    }

    [Test]
    public void Application_DoesNotReferenceInfrastructurePersistenceOrUi()
    {
        AssertNoReferences(typeof(IClock).Assembly, [
            "Sasd.MailWorkbench.Infrastructure",
            "Sasd.MailWorkbench.Persistence",
            "Microsoft.Data.Sqlite",
            "System.Windows.Forms"
        ]);
    }

    [Test]
    public void ExtensionModel_IsTechnologyIndependent()
    {
        AssertNoReferences(typeof(ExtensionDescriptor).Assembly, [
            "Sasd.MailWorkbench.Application",
            "Sasd.MailWorkbench.Infrastructure",
            "Sasd.MailWorkbench.Persistence",
            "Microsoft.Data.Sqlite",
            "System.Windows.Forms"
        ]);
    }

    [Test]
    public void InfrastructureAndPersistence_AreSeparateAssemblies()
    {
        Assert.That(typeof(SystemClock).Assembly, Is.Not.EqualTo(typeof(SqliteMessageImportRepository).Assembly));
    }

    private static void AssertNoReferences(Assembly assembly, IReadOnlyCollection<string> forbidden)
    {
        string[] references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();
        foreach (string forbiddenReference in forbidden)
        {
            Assert.That(references, Does.Not.Contain(forbiddenReference),
                $"{assembly.GetName().Name} darf {forbiddenReference} nicht referenzieren.");
        }
    }
}
