using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Provider-neutrale Quelle fuer Mailnachrichten.
/// Im ersten Startwurf wird dies durch eine lokale Verzeichnisquelle implementiert.
/// Spaeter kann dieselbe Schnittstelle von einem POP3-Adapter via MailKit umgesetzt werden.
/// </summary>
public interface IMailSource
{
    Task<MailSourceConnectionTestResult> TestConnectionAsync(
        MailAccount account,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RemoteMessageInfo>> ListAvailableMessagesAsync(
        MailAccount account,
        CancellationToken cancellationToken);

    Task<RemoteMessageContent> FetchMessageAsync(
        MailAccount account,
        RemoteMessageInfo remoteMessage,
        CancellationToken cancellationToken);
}
