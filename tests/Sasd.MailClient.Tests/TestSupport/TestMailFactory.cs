using System.IO;
using System.Threading.Tasks;

namespace Sasd.MailClient.Tests.TestSupport;

/// <summary>
/// Erzeugt kleine Testmails fuer Parser- und Integrationspruefungen.
/// Die Inhalte werden direkt im Test erzeugt, damit das Testprojekt keine externen
/// Dateien voraussetzt.
/// </summary>
public static class TestMailFactory
{
    public static string CreatePlainTextProjectMail()
    {
        return @"From: Example Sales <sales@example.test>
To: User <user@example.test>
Subject: Project Offer for SASD
Date: Thu, 21 Mar 2026 09:15:00 +0000
Message-ID: <project-offer-001@example.test>
Content-Type: text/plain; charset=utf-8

Hello,

Project: SASD MailClient
Customer: Internal Demo
Ticket: PRE-SALES-17
";
    }

    public static string CreateMultipartInvoiceMail()
    {
        return @"From: Accounting <accounting@example.test>
To: User <user@example.test>
Subject: Rechnung und Invoice Warning
Date: Thu, 21 Mar 2026 10:30:00 +0000
Message-ID: <invoice-002@example.test>
Content-Type: multipart/alternative; boundary=""MAIL-BOUNDARY-001""

--MAIL-BOUNDARY-001
Content-Type: text/plain; charset=utf-8

Invoice: INV-2026-0007
Rechnung: RE-2026-19
Order: ORDER-48
--MAIL-BOUNDARY-001
Content-Type: text/html; charset=utf-8

<html><body><p>Invoice: <strong>INV-2026-0007</strong></p></body></html>
--MAIL-BOUNDARY-001--
";
    }

    public static async Task SeedInboxAsync(string inboxDirectory)
    {
        Directory.CreateDirectory(inboxDirectory);

        await File.WriteAllTextAsync(Path.Combine(inboxDirectory, "001-project-offer.eml"), CreatePlainTextProjectMail());
        await File.WriteAllTextAsync(Path.Combine(inboxDirectory, "002-invoice-alert.eml"), CreateMultipartInvoiceMail());
    }
}
