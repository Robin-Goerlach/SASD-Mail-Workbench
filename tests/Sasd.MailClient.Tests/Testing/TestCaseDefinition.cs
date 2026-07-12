using System;
using System.Threading.Tasks;

namespace Sasd.MailClient.Tests.Testing;

/// <summary>
/// Beschreibt einen einzelnen Testfall fuer den einfachen Test-Runner.
/// </summary>
public sealed class TestCaseDefinition
{
    public required string Name { get; init; }

    public required Func<Task> ExecuteAsync { get; init; }
}
