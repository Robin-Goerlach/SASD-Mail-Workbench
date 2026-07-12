using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;
using Sasd.MailClient.Infrastructure.Parsing;
using Sasd.MailClient.Tests.TestSupport;
using Sasd.MailClient.Tests.Testing;

namespace Sasd.MailClient.Tests.Suites;

/// <summary>
/// Testet die aktuell wichtigste Parserlogik.
/// Ziel ist nicht perfekte RFC-Abdeckung, sondern robuste Grundfunktion fuer V1.
/// </summary>
public static class ParserTests
{
    public static IEnumerable<TestCaseDefinition> GetCases()
    {
        yield return new TestCaseDefinition
        {
            Name = "Parser extrahiert Plain-Text-Mail sauber",
            ExecuteAsync = ParsePlainTextMailAsync
        };

        yield return new TestCaseDefinition
        {
            Name = "Parser extrahiert Multipart-Mail mit Plain- und Html-Body",
            ExecuteAsync = ParseMultipartMailAsync
        };
    }

    private static async Task ParsePlainTextMailAsync()
    {
        SimpleMimeMessageParser parser = new SimpleMimeMessageParser();

        MailAccount account = new MailAccount
        {
            Id = "account-1",
            DisplayName = "Testkonto"
        };

        RawMessage rawMessage = new RawMessage
        {
            Id = "msg-plain-1",
            AccountId = account.Id,
            RetrievedAtUtc = new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero)
        };

        ParsedMessage result = await parser.ParseAsync(
            account,
            rawMessage,
            TestMailFactory.CreatePlainTextProjectMail(),
            CancellationToken.None);

        AssertEx.Equal("Project Offer for SASD", result.Subject, "Der Betreff wurde nicht korrekt gelesen.");
        AssertEx.NotNull(result.From, "Der Absender wurde nicht geparst.");
        AssertEx.Equal("sales@example.test", result.From!.Address, "Die Absenderadresse stimmt nicht.");
        AssertEx.Contains("Project: SASD MailClient", result.TextBody, "Der Textbody enthaelt die erwarteten Nutzdaten nicht.");
        AssertEx.Contains("Ticket: PRE-SALES-17", result.NormalizedText, "Der normalisierte Text wurde nicht wie erwartet aufgebaut.");
    }

    private static async Task ParseMultipartMailAsync()
    {
        SimpleMimeMessageParser parser = new SimpleMimeMessageParser();

        MailAccount account = new MailAccount
        {
            Id = "account-1",
            DisplayName = "Testkonto"
        };

        RawMessage rawMessage = new RawMessage
        {
            Id = "msg-multipart-1",
            AccountId = account.Id,
            RetrievedAtUtc = new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero)
        };

        ParsedMessage result = await parser.ParseAsync(
            account,
            rawMessage,
            TestMailFactory.CreateMultipartInvoiceMail(),
            CancellationToken.None);

        AssertEx.Contains("Invoice: INV-2026-0007", result.TextBody, "Der Plain-Text-Teil wurde nicht gefunden.");
        AssertEx.Contains("<strong>INV-2026-0007</strong>", result.HtmlBody, "Der HTML-Teil wurde nicht gefunden.");
        AssertEx.Contains("Rechnung: RE-2026-19", result.NormalizedText, "Der normalisierte Text enthaelt die erwartete Rechnungsnummer nicht.");
    }
}
