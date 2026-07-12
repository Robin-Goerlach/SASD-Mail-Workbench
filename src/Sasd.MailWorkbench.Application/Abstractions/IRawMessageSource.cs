using Sasd.MailWorkbench.Domain.Models;

namespace Sasd.MailWorkbench.Application.Abstractions;

/// <summary>
/// Providerneutraler Zugriff auf sichtbare Rohnachrichten.
/// </summary>
/// <remarks>
/// In Milestone 0.3.1 wird diese Schnittstelle durch eine lokale EML-Quelle
/// implementiert. In Milestone 0.4.0 folgt ein POP3-Adapter, ohne dass der
/// Import-Use-Case geändert werden muss.
/// </remarks>
public interface IRawMessageSource
{
    /// <summary>
    /// Listet alle aktuell sichtbaren Nachrichtenreferenzen eines Kontos.
    /// </summary>
    Task<IReadOnlyList<RawMessageSourceItem>> ListAsync(
        string accountId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Öffnet die unveränderten Bytes einer referenzierten Nachricht zum Lesen.
    /// </summary>
    Task<Stream> OpenReadAsync(
        string accountId,
        RawMessageSourceItem item,
        CancellationToken cancellationToken);
}
