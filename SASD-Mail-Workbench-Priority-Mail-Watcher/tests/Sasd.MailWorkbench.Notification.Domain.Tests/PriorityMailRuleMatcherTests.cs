using NUnit.Framework;
using Sasd.MailWorkbench.Notification.Domain.Matching;
using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Domain.Tests;

[TestFixture]
public sealed class PriorityMailRuleMatcherTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 24, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void Match_SenderDomainAndSubjectMatch_ReturnsPositiveResult()
    {
        var rule = CreateRule(new PriorityMailRuleCriteria(
            senderDomains: ["wbs-codingschool.de"],
            subjectTerms: ["Einladung"]));
        var observation = CreateObservation(
            fromAddress: "bewerbung@wbs-codingschool.de",
            subject: "Einladung zum Gespräch");

        var result = new PriorityMailRuleMatcher().Match(rule, observation, Now);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsMatch, Is.True);
            Assert.That(result.ReasonCodes, Does.Contain(MatchReasonCodes.SenderDomain));
            Assert.That(result.ReasonCodes, Does.Contain(MatchReasonCodes.Subject));
        }
    }

    [Test]
    public void Match_OneRequiredCategoryDoesNotMatch_ReturnsNegativeResult()
    {
        var rule = CreateRule(new PriorityMailRuleCriteria(
            senderDomains: ["wbs-codingschool.de"],
            subjectTerms: ["Einladung"]));
        var observation = CreateObservation(
            fromAddress: "bewerbung@wbs-codingschool.de",
            subject: "Newsletter");

        var result = new PriorityMailRuleMatcher().Match(rule, observation, Now);

        Assert.That(result.IsMatch, Is.False);
    }

    [Test]
    public void Match_RuleExpired_ReturnsNegativeResult()
    {
        var rule = new PriorityMailRule(
            Guid.NewGuid(),
            "Befristete Beobachtung",
            new PriorityMailRuleCriteria(senderDomains: ["example.com"]),
            expiresAtUtc: Now.AddMinutes(-1));

        var result = new PriorityMailRuleMatcher().Match(
            rule,
            CreateObservation(fromAddress: "mail@example.com"),
            Now);

        Assert.That(result.IsMatch, Is.False);
    }

    [Test]
    public void Match_ExactSenderMatchesCaseInsensitively_ReturnsPositiveResult()
    {
        var rule = CreateRule(new PriorityMailRuleCriteria(
            senderAddresses: ["Support@Example.com"]));

        var result = new PriorityMailRuleMatcher().Match(
            rule,
            CreateObservation(fromAddress: "support@example.com"),
            Now);

        Assert.That(result.IsMatch, Is.True);
    }

    private static PriorityMailRule CreateRule(PriorityMailRuleCriteria criteria) =>
        new(Guid.NewGuid(), "Testregel", criteria);

    private static MailObservation CreateObservation(
        string fromAddress,
        string subject = "Antwort") =>
        new(
            "account-1",
            "inbox:42:1001",
            fromAddress,
            null,
            subject,
            "Lokaler Nachrichtentext",
            false,
            Now);
}
