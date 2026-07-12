using NUnit.Framework;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.Rules;

namespace Sasd.MailWorkbench.Domain.Tests;

[TestFixture]
public sealed class ImportStateRulesTests
{
    [Test]
    public void CanTransition_AllowsExpectedHappyPath()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ImportStateRules.CanTransition(ImportAttemptState.Discovered, ImportAttemptState.Downloading), Is.True);
            Assert.That(ImportStateRules.CanTransition(ImportAttemptState.Downloading, ImportAttemptState.Staged), Is.True);
            Assert.That(ImportStateRules.CanTransition(ImportAttemptState.Staged, ImportAttemptState.Committing), Is.True);
            Assert.That(ImportStateRules.CanTransition(ImportAttemptState.Committing, ImportAttemptState.Completed), Is.True);
        });
    }

    [Test]
    public void CanTransition_RejectsCompletedToDownloading()
    {
        Assert.That(ImportStateRules.CanTransition(ImportAttemptState.Completed, ImportAttemptState.Downloading), Is.False);
    }
}
