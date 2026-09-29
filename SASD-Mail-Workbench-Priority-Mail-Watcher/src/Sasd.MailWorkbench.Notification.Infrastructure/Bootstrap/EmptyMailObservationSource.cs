using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Infrastructure.Bootstrap;

/// <summary>
/// Liefert im Architektur-Foundation-Stand keine Nachrichten.
/// </summary>
/// <remarks>
/// Dieser Adapter verhindert bewusst eine zweite, provisorische Mailprotokollimplementierung.
/// Er wird ersetzt, sobald die gemeinsamen Mail-Workbench-Interfaces einen Watcher-Adapter tragen.
/// </remarks>
public sealed class EmptyMailObservationSource : IMailObservationSource
{
    /// <inheritdoc />
    public Task<IReadOnlyList<MailObservation>> GetNewObservationsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<MailObservation>>(Array.Empty<MailObservation>());
    }
}
