using System.IO;
using System.Threading.Tasks;

namespace Sasd.MailClient.Bootstrap.ConsoleApp;

/// <summary>
/// Stellt sicher, dass der Demo-Start ohne Vorarbeit moeglich ist.
/// Wenn noch keine Beispielmails vorhanden sind, werden zwei kleine Testmails angelegt.
/// </summary>
public static class SampleMailSeeder
{
    public static async Task EnsureSampleMailsExistAsync(string inboxDirectory)
    {
        Directory.CreateDirectory(inboxDirectory);

        string firstPath = Path.Combine(inboxDirectory, "001-project-offer.eml");
        string secondPath = Path.Combine(inboxDirectory, "002-invoice-alert.eml");

        if (!File.Exists(firstPath))
        {
            string firstMail = @"From: Example Sales <sales@example.test>
To: User <user@example.test>
Subject: Project Offer for SASD
Date: Thu, 21 Mar 2026 09:15:00 +0000
Message-ID: <project-offer-001@example.test>
Content-Type: text/plain; charset=utf-8

Hello,

this is a small project offer.

Project: SASD MailClient
Customer: Internal Demo
Ticket: PRE-SALES-17

Best regards
Example Sales
";
            await File.WriteAllTextAsync(firstPath, firstMail);
        }

        if (!File.Exists(secondPath))
        {
            string secondMail = @"From: Accounting <accounting@example.test>
To: User <user@example.test>
Subject: Rechnung und Invoice Warning
Date: Thu, 21 Mar 2026 10:30:00 +0000
Message-ID: <invoice-002@example.test>
Content-Type: multipart/alternative; boundary=""MAIL-BOUNDARY-001""

--MAIL-BOUNDARY-001
Content-Type: text/plain; charset=utf-8

Guten Tag,

Invoice: INV-2026-0007
Rechnung: RE-2026-19
Order: ORDER-48

Vielen Dank.
--MAIL-BOUNDARY-001
Content-Type: text/html; charset=utf-8

<html><body><p>Invoice: <strong>INV-2026-0007</strong></p></body></html>
--MAIL-BOUNDARY-001--
";
            await File.WriteAllTextAsync(secondPath, secondMail);
        }
    }
}
